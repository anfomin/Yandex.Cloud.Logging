using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Yandex.Cloud.Logging;

/// <summary>
/// Provides loggers for Yandex.Cloud.
/// </summary>
[ProviderAlias("YandexCloud")]
public sealed class YandexCloudLoggerProvider(YandexCloudLoggerService service) : ILoggerProvider
{
	readonly YandexCloudLoggerService _service = service;
	readonly ConcurrentDictionary<string, YandexCloudLogger> _loggers = new(StringComparer.OrdinalIgnoreCase);

	/// <inheritdoc />
	public ILogger CreateLogger(string categoryName)
		=> _loggers.GetOrAdd(categoryName, key => new YandexCloudLogger(key, _service));

	/// <inheritdoc />
	public void Dispose()
		=> _loggers.Clear();
}