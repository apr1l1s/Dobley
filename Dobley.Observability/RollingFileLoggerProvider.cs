using Microsoft.Extensions.Logging;

namespace Dobley.Observability;

public sealed class RollingFileLoggerProvider(string logFilePath)
    : ILoggerProvider
{
    private readonly string preparedLogFilePath = PrepareLogFilePath(logFilePath);
    private readonly object writeLock = new();

    public ILogger CreateLogger(string categoryName) => new RollingFileLogger(categoryName, preparedLogFilePath,
        writeLock);

    public void Dispose()
    {
    }

    private static string PrepareLogFilePath(string logFilePath)
    {
        var directoryPath = Path.GetDirectoryName(logFilePath);
        if (!string.IsNullOrEmpty(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        return logFilePath;
    }
}
