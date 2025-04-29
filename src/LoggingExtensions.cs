using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging.Configuration;
using Yandex.Cloud.Logging;

namespace Microsoft.Extensions.Logging;

/// <summary>
/// <see cref="ILoggingBuilder"/> extension methods for the Yandex.Cloud logging registration.
/// </summary>
public static class YandexCloudLoggingExtensions
{
	/// <summary>
	/// Adds a Yandex.Cloud logger to the factory and registers required services.
	/// </summary>
	public static ILoggingBuilder AddYandexCloud(this ILoggingBuilder builder)
	{
		builder.AddConfiguration();
		builder.Services.TryAddSingleton<YandexCloudLoggerService>();
		builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, YandexCloudLoggerProvider>());
		builder.Services.AddHostedService(s => s.GetRequiredService<YandexCloudLoggerService>());
		LoggerProviderOptions.RegisterProviderOptions<YandexCloudLoggerOptions, YandexCloudLoggerProvider>(builder.Services);
		return builder;
	}

	/// <summary>
	/// Adds a Yandex.Cloud logger to the factory and registers required services.
	/// </summary>
	/// <param name="configure">A delegate to configure the <see cref="YandexCloudLoggerOptions"/>.</param>
	public static ILoggingBuilder AddYandexCloud(this ILoggingBuilder builder, Action<YandexCloudLoggerOptions> configure)
	{
		builder.AddYandexCloud();
		builder.Services.Configure(configure);
		return builder;
	}
}