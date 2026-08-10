using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Config;
using NLog.Extensions.Logging;
using NLog.Targets;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using MsLogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Dobley.Endpoints.Observability;

public static class LoggingExtensions
{
    private const string OTLP_ENDPOINT_VARIABLE = "OTEL_EXPORTER_OTLP_ENDPOINT";
    private const string LOG_FILE_PATH_VARIABLE = "LOG_FILE_PATH";
    private const string DATA_PROTECTION_KEYS_PATH_VARIABLE = "DATA_PROTECTION_KEYS_PATH";

    public static WebApplicationBuilder AddDobleyLogging(this WebApplicationBuilder builder, string serviceName)
    {
        var otlpEndpoint = Environment.GetEnvironmentVariable(OTLP_ENDPOINT_VARIABLE) ??
                           "http://dobley.otel-collector:4317";
        var logFilePath = Environment.GetEnvironmentVariable(LOG_FILE_PATH_VARIABLE) ??
                          Path.Combine("logs", $"{serviceName}.log");
        var dataProtectionKeysPath = Environment.GetEnvironmentVariable(DATA_PROTECTION_KEYS_PATH_VARIABLE);

        if (!string.IsNullOrWhiteSpace(dataProtectionKeysPath))
        {
            builder.Services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath))
                .SetApplicationName(serviceName);
        }

        builder.Logging.ClearProviders();
        builder.Logging.AddNLog(CreateNLogConfiguration(logFilePath), new NLogProviderOptions
        {
            CaptureMessageTemplates = true,
            CaptureMessageProperties = true,
            IncludeScopes = true,
            RemoveLoggerFactoryFilter = false,
            ShutdownOnDispose = true
        });
        builder.Logging.AddOpenTelemetry(options =>
        {
            options.IncludeFormattedMessage = true;
            options.IncludeScopes = true;
            options.ParseStateValues = true;
            options.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName));
            options.AddOtlpExporter(exporterOptions => exporterOptions.Endpoint = new Uri(otlpEndpoint));
        });

        builder.Logging.AddFilter("Microsoft", MsLogLevel.Warning);
        builder.Logging.AddFilter("Microsoft.AspNetCore", MsLogLevel.Warning);
        builder.Logging.AddFilter("Microsoft.AspNetCore.DataProtection.KeyManagement.XmlKeyManager", MsLogLevel.Error);
        builder.Logging.AddFilter("Yarp", MsLogLevel.Information);

        return builder;
    }

    private static LoggingConfiguration CreateNLogConfiguration(string logFilePath)
    {
        var layout = "${longdate} [${uppercase:${level}}] ${logger}: ${message} ${exception:format=tostring}";
        var configuration = new LoggingConfiguration();
        var consoleTarget = new ConsoleTarget("console")
        {
            Layout = layout
        };
        var fileTarget = new FileTarget("file")
        {
            FileName = GetDailyLogFileName(logFilePath),
            KeepFileOpen = false,
            CreateDirs = true,
            Layout = layout
        };

        configuration.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, consoleTarget);
        configuration.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, fileTarget);

        return configuration;
    }

    private static string GetDailyLogFileName(string logFilePath)
    {
        var directoryPath = Path.GetDirectoryName(logFilePath);
        var fileName = Path.GetFileNameWithoutExtension(logFilePath);
        var extension = Path.GetExtension(logFilePath);

        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            directoryPath = ".";
        }

        if (string.IsNullOrWhiteSpace(extension))
        {
            extension = ".log";
        }

        return Path.Combine(directoryPath, $"{fileName}-${{shortdate}}{extension}");
    }

    public static IApplicationBuilder UseDobleyRequestLogging(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            var logger = context.RequestServices.GetRequiredService<ILoggerFactory>()
                .CreateLogger("Dobley.HttpRequest");
            var startedAt = TimeProvider.System.GetTimestamp();

            try
            {
                await next();
            }
            finally
            {
                var elapsed = TimeProvider.System.GetElapsedTime(startedAt);
                var statusCode = context.Response.StatusCode;
                var logLevel = statusCode >= StatusCodes.Status500InternalServerError || elapsed.TotalSeconds > 1
                    ? MsLogLevel.Warning
                    : MsLogLevel.Information;

                logger.Log(logLevel,
                    "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMilliseconds} ms. TraceId: {TraceId}",
                    context.Request.Method, context.Request.Path, statusCode, elapsed.TotalMilliseconds,
                    context.TraceIdentifier);
            }
        });
    }
}
