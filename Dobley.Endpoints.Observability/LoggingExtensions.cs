using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;

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
        builder.Logging.AddConsole();
        builder.Logging.AddProvider(new RollingFileLoggerProvider(logFilePath));
        builder.Logging.AddOpenTelemetry(options =>
        {
            options.IncludeFormattedMessage = true;
            options.IncludeScopes = true;
            options.ParseStateValues = true;
            options.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName));
            options.AddOtlpExporter(exporterOptions => exporterOptions.Endpoint = new Uri(otlpEndpoint));
        });

        builder.Logging.AddFilter("Microsoft", LogLevel.Warning);
        builder.Logging.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);
        builder.Logging.AddFilter("Microsoft.AspNetCore.DataProtection.KeyManagement.XmlKeyManager", LogLevel.Error);
        builder.Logging.AddFilter("Yarp", LogLevel.Information);

        return builder;
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
                    ? LogLevel.Warning
                    : LogLevel.Information;

                logger.Log(logLevel,
                    "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMilliseconds} ms. TraceId: {TraceId}",
                    context.Request.Method, context.Request.Path, statusCode, elapsed.TotalMilliseconds,
                    context.TraceIdentifier);
            }
        });
    }
}
