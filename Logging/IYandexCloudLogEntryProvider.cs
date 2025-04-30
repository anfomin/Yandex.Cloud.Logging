using Google.Protobuf.WellKnownTypes;

namespace Yandex.Cloud.Logging;

/// <summary>
/// Provides additional payload for Yandex.Cloud log entries.
/// </summary>
public interface IYandexCloudLogEntryProvider
{
	/// <summary>
	/// Applies additional payload to the Yandex.Cloud log entry payload.
	/// </summary>
	void ApplyPayload(Struct payload);
}