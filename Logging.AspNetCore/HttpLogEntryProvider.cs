using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Options;

namespace Yandex.Cloud.Logging;

/// <summary>
/// Provides Yandex.Cloud logger payload with <see cref="HttpContext"/> request and user information.
/// Supports Blazor URI navigation.
/// </summary>
public class HttpLogEntryProvider(IHttpContextAccessor httpContextAccessor, IOptions<HttpLogEntryOptions> options)
	: IYandexCloudLogEntryProvider
{
	readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
	readonly HttpLogEntryOptions _options = options.Value;

	public void ApplyPayload(Struct payload)
	{
		var context = _httpContextAccessor.HttpContext;
		if (context == null)
			return;

		var request = context.Request;
		Struct requestStruct = new()
		{
			Fields =
			{
				["url"] = Value.ForString(request.GetDisplayUrl()),
				["method"] = Value.ForString(request.Method),
				["ip"] = Value.ForString(GetIPAddress(context.Connection))
			}
		};
		if (_options.Headers)
			requestStruct.Fields["headers"] = Value.ForStruct(GetValuesStruct(request.Headers));
		if (_options.Cookies)
			requestStruct.Fields["cookies"] = Value.ForStruct(GetValuesStruct(request.Cookies));
		if (_options.Form && GetFormStruct(request) is {} formStruct)
			requestStruct.Fields["form"] = Value.ForStruct(formStruct);
		if (context.Features.Get<NavigationManagerFeature>()?.NavigationManager is {} navigation)
		{
			requestStruct.Fields["url"] = Value.ForString(navigation.Uri);
			requestStruct.Fields["circuit"] = Value.ForBool(true);
		}
		payload.Fields["request"] = Value.ForStruct(requestStruct);

		if (context.User.Identity?.IsAuthenticated == true)
			payload.Fields["user"] = Value.ForString(context.User.Identity.Name);
	}

	static string? GetIPAddress(ConnectionInfo connection)
	{
		var ip = connection.RemoteIpAddress ?? connection.LocalIpAddress;
		if (ip == null)
			return null;

		int? port = connection.RemotePort == 0 ? connection.LocalPort : connection.RemotePort;
		if (port != 0)
			return ip + ":" + port.Value;
		return ip.ToString();
	}

	static Struct GetValuesStruct<T>(IEnumerable<KeyValuePair<string, T>> items)
		where T : notnull
	{
		Struct res = new();
		foreach (var item in items)
			res.Fields[item.Key] = Value.ForString(item.Value.ToString());
		return res;
	}

	static Struct? GetFormStruct(HttpRequest request)
	{
		if (!request.HasFormContentType)
			return null;
		try
		{
			return GetValuesStruct(request.Form);
		}
		catch (InvalidOperationException)
		{
			return null;
		}
	}
}