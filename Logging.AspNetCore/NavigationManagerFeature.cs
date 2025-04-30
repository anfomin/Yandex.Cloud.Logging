using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;

namespace Yandex.Cloud.Logging;

/// <summary>
/// Represents <see cref="HttpContext"/> feature that provides <see cref="Microsoft.AspNetCore.Components.NavigationManager"/>.
/// </summary>
internal class NavigationManagerFeature(NavigationManager navigationManager)
{
	/// <summary>
	/// Gets or sets current navigation manager.
	/// </summary>
	public NavigationManager? NavigationManager { get; set; } = navigationManager;
}