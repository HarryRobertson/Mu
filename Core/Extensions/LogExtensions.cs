using System.Text.Json;

namespace Mu.Core.Extensions;

public static class LogExtensions 
{
    public static void LogJson<T>(this ILogger logger, T state, LogLevel logLevel = LogLevel.Information)
    {
        if (logger.IsEnabled(logLevel))
        {
            logger.Log<T>(logLevel, 0, state, null, (s, _) => JsonSerializer.Serialize(s));
        }
    }
}
