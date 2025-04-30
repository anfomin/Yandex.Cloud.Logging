using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Http;

namespace Yandex.Cloud.Logging;

/// <summary>
/// Provides <see cref="NavigationManagerFeature"/> for <see cref="HttpContext"/> when circuit is opened.
/// </summary>
internal class LoggingCircuitHandler(IHttpContextAccessor httpContextAccessor, NavigationManager navigationManager) : CircuitHandler
{
	public override async Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
	{
		await base.OnCircuitOpenedAsync(circuit, cancellationToken);
		httpContextAccessor.HttpContext?.Features.Set(new NavigationManagerFeature(navigationManager));
	}

	public override async Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
	{
		await base.OnCircuitClosedAsync(circuit, cancellationToken);
		if (httpContextAccessor.HttpContext?.Features.Get<NavigationManagerFeature>() is { } feature)
			feature.NavigationManager = null;
	}
}