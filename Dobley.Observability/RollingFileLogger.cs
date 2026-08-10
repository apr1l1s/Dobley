using Microsoft.Extensions.Logging;

namespace Dobley.Observability;

public sealed class RollingFileLogger(string categoryName, string logFilePath, object writeLock)
    : ILogger
{
    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull
        => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        var filePath = GetCurrentFilePath();
        var message = formatter(state, exception);
        var logLine = $"{DateTimeOffset.UtcNow:O} [{logLevel}] {categoryName}: {message}";

        if (exception != null)
        {
            logLine += Environment.NewLine + exception;
        }

        lock (writeLock)
        {
            File.AppendAllText(filePath, logLine + Environment.NewLine);
        }
    }

    private string GetCurrentFilePath()
    {
        var directoryPath = Path.GetDirectoryName(logFilePath);
        var fileName = Path.GetFileNameWithoutExtension(logFilePath);
        var extension = Path.GetExtension(logFilePath);

        if (string.IsNullOrEmpty(directoryPath))
        {
            directoryPath = ".";
        }

        if (string.IsNullOrEmpty(extension))
        {
            extension = ".log";
        }

        return Path.Combine(directoryPath, $"{fileName}-{DateTime.UtcNow:yyyyMMdd}{extension}");
    }
}
