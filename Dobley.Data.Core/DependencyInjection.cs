using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Text.Unicode;
using Dobley.Data.Core.Context;
using Dobley.Data.Core.Integrations.RabbitMq;
using Dobley.Data.Core.Integrations.Telegram;
using Dobley.Data.Core.Repositories;
using Dobley.Data.Core.Repositories.Notifications;
using Dobley.Data.Core.Repositories.Products;
using Dobley.Data.Core.Repositories.Storages;
using Dobley.Data.Core.Repositories.Users;
using Dobley.Domain.Core.Repositories;
using Dobley.Domain.Core.Repositories.Notifications;
using Dobley.Domain.Core.Repositories.Products;
using Dobley.Domain.Core.Repositories.Storages;
using Dobley.Domain.Core.Repositories.Users;
using Dobley.Domain.Core.UseCases;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NLog.Config;
using NLog.Extensions.Logging;
using NLog.Targets;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using MsLogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Dobley.Data.Core;

public static class DependencyInjection
{
    private const string DATA_PROTECTION_KEYS_PATH_VARIABLE = "DATA_PROTECTION_KEYS_PATH";
    private const string LOG_FILE_PATH_VARIABLE = "LOG_FILE_PATH";
    private const string OTLP_ENDPOINT_VARIABLE = "OTEL_EXPORTER_OTLP_ENDPOINT";

    public static IServiceCollection AddAuthServices(this IServiceCollection services)
    {
        services
            .AddCoreServices()
            .AddScoped<IAuthService, AuthService>();

        return services;
    }

    public static IServiceCollection AddApiAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = GetJwtIssuer(),
                    ValidAudience = GetJwtAudience(),
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GetRequiredSecretKey()))
                };
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                            .CreateLogger("JwtBearer");
                        logger.LogWarning(context.Exception, "Ошибка JWT-аутентификации.");

                        return Task.CompletedTask;
                    }
                };
            });

        return services;
    }

    public static IServiceCollection AddApiSwagger(this IServiceCollection services)
    {
        services
            .AddEndpointsApiExplorer()
            .AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT токен из Auth API. Вставлять без префикса Bearer."
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    [new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    }] = []
                });
            });

        return services;
    }

    public static IServiceCollection AddCache(this IServiceCollection services)
    {
        var redisConnection = Environment.GetEnvironmentVariable("REDIS_CONNECTION");
        if (string.IsNullOrEmpty(redisConnection))
        {
            services.AddDistributedMemoryCache();
            return services;
        }

        services.AddStackExchangeRedisCache(options => options.Configuration = redisConnection);

        return services;
    }

    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services
            .AddDataBase()
            .AddCache()
            .AddRepositories()
            .AddMediatr();

        return services;
    }

    public static IServiceCollection AddDataBase(this IServiceCollection services)
    {
        services.AddDbContext<DobleyContext>(options =>
        {
            options.UseNpgsql(GetConnectionString(), npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorCodesToAdd: null
                );
            });
        });

        return services;
    }

    public static void AddDefaultJsonConverters(this JsonSerializerOptions jsonOptions)
    {
        jsonOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public static WebApplicationBuilder AddDobleyLogging(this WebApplicationBuilder builder, string serviceName)
    {
        ConfigureDobleyLogging(builder.Services, builder.Logging, serviceName);

        return builder;
    }

    public static HostApplicationBuilder AddDobleyLogging(this HostApplicationBuilder builder, string serviceName)
    {
        ConfigureDobleyLogging(builder.Services, builder.Logging, serviceName);

        return builder;
    }

    public static IServiceCollection AddMediatr(this IServiceCollection services)
    {
        services
            .AddMediatR(x => x.RegisterServicesFromAssemblies(typeof(IUseCaseDispatcher).Assembly))
            .AddScoped<IUseCaseDispatcher, UseCaseDispatcher>();

        return services;
    }

    public static IServiceCollection AddNotificationIntegrations(this IServiceCollection services)
    {
        services
            .AddHttpClient()
            .AddSingleton<RabbitMqOptions>()
            .AddSingleton<ITelegramBotClient, TelegramBotClient>()
            .AddSingleton<INotificationMessageConsumer, RabbitMqNotificationMessageConsumer>()
            .AddSingleton<INotificationMessagePublisher, RabbitMqNotificationMessagePublisher>();

        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services
            .AddScoped<ICommonRepository, CommonRepository>()
            .AddScoped<IHealthCheckRepository, HealthCheckRepository>()
            .AddScoped<INotificationDeliveryRepository, NotificationDeliveryRepository>()
            .AddScoped<INotificationInviteRepository, NotificationInviteRepository>()
            .AddScoped<INotificationRecipientRepository, NotificationRecipientRepository>()
            .AddScoped<IProductRepository, ProductRepository>()
            .AddScoped<IStorageRepository, StorageRepository>()
            .AddScoped<IStorageNotificationSubscriptionRepository, StorageNotificationSubscriptionRepository>()
            .AddScoped<IUserRepository, UserRepository>();

        return services;
    }

    public static IHostBuilder ConfigureAppServices(this IHostBuilder builder, bool isLocal)
    {
        return builder.ConfigureAppConfiguration((hostingContext, config) =>
        {
            var appPath = string.Empty;
            var basePath = string.Empty;

            var environment = hostingContext.HostingEnvironment;
            if (environment.IsDevelopment() || isLocal)
            {
                appPath = environment.ContentRootPath;
                basePath = Path.Combine(environment.ContentRootPath, "..",
                    typeof(DependencyInjection).Assembly.GetName().Name ?? string.Empty);
            }

            config
                .AddJsonFile(Path.Combine(basePath, "settings.json"), optional: true, reloadOnChange: true)
                .AddJsonFile(Path.Combine(basePath, $"settings.{environment.EnvironmentName}.json"), optional: true,
                    reloadOnChange: true)
                .AddJsonFile(Path.Combine(appPath, $"appsettings.{environment.EnvironmentName}.json"),
                    optional: true, reloadOnChange: true);
        });
    }

    public static JsonSerializerOptions GetDefaultJsonOptions()
    {
        var jsonOptions = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
            PropertyNameCaseInsensitive = true
        };

        AddDefaultJsonConverters(jsonOptions);

        return jsonOptions;
    }

    public static string GetJwtAudience()
        => Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "apr1l1s_services";

    public static string GetJwtIssuer()
        => Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "apr1l1s_auth";

    public static string GetRequiredEnvironmentVariable(string name)
        => Environment.GetEnvironmentVariable(name)
           ?? throw new InvalidOperationException($"Environment variable '{name}' is required.");

    public static string GetRequiredSecretKey()
    {
        var secretKey = GetRequiredEnvironmentVariable("SECRET_KEY");
        if (Encoding.UTF8.GetByteCount(secretKey) < 32)
        {
            throw new InvalidOperationException("Environment variable 'SECRET_KEY' must contain at least 32 bytes.");
        }

        return secretKey;
    }

    public static bool IsApiSwaggerEnabled(this IConfiguration configuration)
        => configuration.GetValue<bool>("ASPNET_LOCAL") || configuration.GetValue<bool>("ENABLE_SWAGGER");

    public static TSection GetTypedSection<TSection>(this IConfiguration configuration)
        where TSection : class, new()
    {
        return configuration.GetTypedSectionNullable<TSection>() ?? new TSection();
    }

    public static TSection? GetTypedSectionNullable<TSection>(this IConfiguration configuration)
        where TSection : class, new()
    {
        return configuration.GetSection(typeof(TSection).Name)
            .Get<TSection>(x => x.BindNonPublicProperties = true);
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

    private static void ConfigureDobleyLogging(IServiceCollection services, ILoggingBuilder logging,
        string serviceName)
    {
        var otlpEndpoint = Environment.GetEnvironmentVariable(OTLP_ENDPOINT_VARIABLE) ??
                           "http://dobley.otel-collector:4317";
        var logFilePath = Environment.GetEnvironmentVariable(LOG_FILE_PATH_VARIABLE) ??
                          Path.Combine("logs", $"{serviceName}.log");
        var dataProtectionKeysPath = Environment.GetEnvironmentVariable(DATA_PROTECTION_KEYS_PATH_VARIABLE);

        if (!string.IsNullOrWhiteSpace(dataProtectionKeysPath))
        {
            services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath))
                .SetApplicationName(serviceName);
        }

        logging.ClearProviders();
        logging.AddNLog(CreateNLogConfiguration(logFilePath), new NLogProviderOptions
        {
            CaptureMessageTemplates = true,
            CaptureMessageProperties = true,
            IncludeScopes = true,
            RemoveLoggerFactoryFilter = false,
            ShutdownOnDispose = true
        });
        logging.AddOpenTelemetry(options =>
        {
            options.IncludeFormattedMessage = true;
            options.IncludeScopes = true;
            options.ParseStateValues = true;
            options.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName));
            options.AddOtlpExporter(exporterOptions => exporterOptions.Endpoint = new Uri(otlpEndpoint));
        });

        logging.AddFilter("Microsoft", MsLogLevel.Warning);
        logging.AddFilter("Microsoft.AspNetCore", MsLogLevel.Warning);
        logging.AddFilter("Microsoft.AspNetCore.DataProtection.KeyManagement.XmlKeyManager", MsLogLevel.Error);
        logging.AddFilter("Microsoft.Extensions.Http", MsLogLevel.Warning);
        logging.AddFilter("System.Net.Http", MsLogLevel.Warning);
        logging.AddFilter("Yarp", MsLogLevel.Information);
    }

    private static string GetConnectionString()
    {
        var dbHost = GetRequiredEnvironmentVariable("DB_HOST");
        var dbPort = GetRequiredEnvironmentVariable("DB_PORT");
        var dbUser = GetRequiredEnvironmentVariable("DB_USER");
        var dbPassword = GetRequiredEnvironmentVariable("DB_PASSWORD");
        var dbName = GetRequiredEnvironmentVariable("DB_NAME");

        return $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPassword}";
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
}
