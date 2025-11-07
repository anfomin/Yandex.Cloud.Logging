using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Yandex.Cloud.Logging;

/// <summary>
/// Helper to create FormattedLogValues instances via reflection.
/// </summary>
internal static class FormattedLogValuesHelper
{
	static ConstructorInfo? _ctor;

	/// <summary>
	/// Creates an instance of FormattedLogValues.
	/// </summary>
	public static object Create(string? message, params object?[] args)
	{
		if (_ctor == null)
		{
			var type = typeof(ILogger).Assembly.GetType("Microsoft.Extensions.Logging.FormattedLogValues")
				?? throw new InvalidOperationException("FormattedLogValues type not found.");
			_ctor = type.GetConstructors().FirstOrDefault(c => c.GetParameters().Length == 2)
				?? throw new InvalidOperationException("FormattedLogValues constructor not found.");
		}
		return _ctor.Invoke([message, args]);
	}
}