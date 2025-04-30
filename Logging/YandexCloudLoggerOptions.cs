using Yandex.Cloud.Credentials;

namespace Yandex.Cloud.Logging;

/// <summary>
/// Provides options for the <see cref="YandexCloudLoggerService"/>.
/// </summary>
public record YandexCloudLoggerOptions
{
	/// <summary>
	/// Credentials used to authorize by <see cref="Sdk"/>.
	/// If null then <see cref="Sdk"/> service will be used.
	/// </summary>
	public ICredentialsProvider? Credentials { get; set; }

	/// <summary>
	/// Required logging folder identifier.
	/// Value must match the regular expression ([a-zA-Z][-a-zA-Z0-9_.]{0,63})?.
	/// </summary>
	public string? FolderId { get; set; }

	/// <summary>
	/// Required logging group identifier.
	/// Value must match the regular expression [a-zA-Z][-a-zA-Z0-9_.]{0,63}.
	/// </summary>
	public string? GroupId { get; set; }

	/// <summary>
	/// Optional resource type, i.e., serverless.function.
	/// Value must match the regular expression [a-zA-Z][-a-zA-Z0-9_.]{0,63}.
	/// </summary>
	public string? ResourceType { get; set; }

	/// <summary>
	/// Optional resource identifier, i.e., ID of the function producing logs.
	/// Value must match the regular expression [a-zA-Z0-9][-a-zA-Z0-9_.]{0,63}.
	/// </summary>
	public string? ResourceId { get; set; }

	/// <summary>
	/// Log entries will be sent in batches of this size.
	/// </summary>
	public int BatchSize { get; set; } = 50;

	/// <summary>
	/// If the number of log entries is less than <see cref="BatchSize"/>
	/// then the service will wait for this time before sending the log entries.
	/// </summary>
	public TimeSpan CacheTime { get; set; } = TimeSpan.FromSeconds(10);

	/// <summary>
	/// Validates required properties.
	/// </summary>
	public void Validate()
	{
		if (string.IsNullOrEmpty(FolderId))
			throw new InvalidOperationException("Log FolderId is not set");
		if (string.IsNullOrEmpty(GroupId))
			throw new InvalidOperationException("Log GroupId is not set");
	}
}