using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Yandex.Cloud.Logging;

namespace Microsoft.Extensions.Logging;

/// <summary>
/// <see cref="ILoggingBuilder"/> extension methods for the Yandex.Cloud logging registration.
/// </summary>
public static class YandexCloudLoggingExtensions
{
	/// <summary>
	/// Adds a Yandex.Cloud logger to the factory and registers required services.
	/// Provides Yandex.Cloud logger payload with <see cref="HttpContext"/> request and user information.
	/// Supports Blazor URI navigation.
	/// </summary>
	public static ILoggingBuilder AddYandexCloudWithHttp(this ILoggingBuilder builder, Action<HttpLogEntryOptions>? options = null)
	{
		builder.AddYandexCloud();
		builder.Services.AddHttpContextAccessor();
		builder.Services.TryAddScoped<CircuitHandler, LoggingCircuitHandler>();
		builder.AddYandexCloudLogEntryProvider<HttpLogEntryProvider>();
		if (options != null)
			builder.Services.Configure(options);
		return builder;
	}
}