namespace Yandex.Cloud.Logging;

/// <summary>
/// State object for logging with details.
/// </summary>
/// <param name="Message">Log message.</param>
internal record LogState(object Message)
{
	/// <summary>
	/// Additional details object to be logged.
	/// </summary>
	public required object Details { get; init; }

	public override string ToString()
		=> Message.ToString() ?? "";
}