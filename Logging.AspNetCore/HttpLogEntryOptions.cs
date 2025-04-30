namespace Yandex.Cloud.Logging;

/// <summary>
/// Provides options for <see cref="HttpLogEntryProvider"/>.
/// </summary>
public record HttpLogEntryOptions
{
	/// <summary>
	/// Gets or sets if request headers are logged.
	/// </summary>
	public bool Headers { get; set; }

	/// <summary>
	/// Gets or sets if request cookies are logged.
	/// </summary>
	public bool Cookies { get; set; }

	/// <summary>
	/// Gets or sets if request form is logged.
	/// </summary>
	public bool Form { get; set; }
}