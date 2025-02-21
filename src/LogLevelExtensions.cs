using Microsoft.Extensions.Logging;
using static Yandex.Cloud.Logging.V1.LogLevel.Types;

namespace Yandex.Cloud.Logging;

public static class LogLevelExtensions
{
	/// <summary>
	/// Converts <see cref="LogLevel"/> to <see cref="Yandex.Cloud.Logging.V1.LogLevel"/>.
	/// </summary>
	public static Level ToYandex(this LogLevel logLevel) => logLevel switch
	{
		LogLevel.Trace => Level.Trace,
		LogLevel.Debug => Level.Debug,
		LogLevel.Information => Level.Info,
		LogLevel.Warning => Level.Warn,
		LogLevel.Error => Level.Error,
		LogLevel.Critical => Level.Fatal,
		_ => Level.Unspecified
	};
}