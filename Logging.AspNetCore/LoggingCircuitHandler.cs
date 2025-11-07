using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Yandex.Cloud.Logging;

/// <summary>
/// Provides <see cref="NavigationManagerFeature"/> for <see cref="HttpContext"/> when circuit is opened.
/// </summary>
internal class LoggingCircuitHandler(IServiceProvider services, IHttpContextAccessor httpContextAccessor) : CircuitHandler
{
	public override async Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
	{
		await base.OnCircuitOpenedAsync(circuit, cancellationToken);
		var navigation = services.GetRequiredService<NavigationManager>();
		httpContextAccessor.HttpContext?.Features.Set(new NavigationManagerFeature(navigation));
	}

	public override async Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
	{
		await base.OnCircuitClosedAsync(circuit, cancellationToken);
		if (httpContextAccessor.HttpContext?.Features.Get<NavigationManagerFeature>() is { } feature)
			feature.NavigationManager = null;
	}
}