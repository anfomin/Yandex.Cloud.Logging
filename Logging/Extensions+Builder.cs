using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging.Configuration;
using Yandex.Cloud.Logging;

namespace Microsoft.Extensions.Logging;

/// <summary>
/// Extension methods for the Yandex.Cloud logging registration.
/// </summary>
public static partial class YandexCloudLoggingExtensions
{
	extension(ILoggingBuilder builder)
	{
		/// <summary>
		/// Adds a Yandex.Cloud logger to the factory and registers required services.
		/// </summary>
		public ILoggingBuilder AddYandexCloud()
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
		public ILoggingBuilder AddYandexCloud(Action<YandexCloudLoggerOptions> configure)
		{
			builder.AddYandexCloud();
			builder.Services.Configure(configure);
			return builder;
		}

		/// <summary>
		/// Registers custom <see cref="IYandexCloudLogEntryProvider"/> to provide additional payload to the Yandex.Cloud logger.
		/// </summary>
		/// <typeparam name="TProvider">Custom provider type.</typeparam>
		public ILoggingBuilder AddYandexCloudLogEntryProvider<TProvider>()
			where TProvider : class, IYandexCloudLogEntryProvider
		{
			builder.Services.TryAddEnumerable(ServiceDescriptor.Transient<IYandexCloudLogEntryProvider, TProvider>());
			return builder;
		}
	}
}