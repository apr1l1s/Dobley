using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Text.Unicode;
using Dobley.Data.Core.Repositories;
using Dobley.Data.Core.Repositories.Users;
using Dobley.Domain.Core.Repositories;
using Dobley.Domain.Core.Repositories.Products;
using Dobley.Domain.Core.Repositories.Users;
using Dobley.Domain.Core.UseCases;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Dobley.Data.Core;

public static class DependencyInjection
{
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
                    optional: true, reloadOnChange: true)
                ;
        });
    }

    public static TSection? GetTypedSectionNullable<TSection>(this IConfiguration configuration)
        where TSection : class, new()
    {
        return configuration.GetSection(typeof(TSection).Name)
            .Get<TSection>(x => x.BindNonPublicProperties = true);
    }

    public static TSection GetTypedSection<TSection>(this IConfiguration configuration)
        where TSection : class, new()
    {
        return configuration.GetTypedSectionNullable<TSection>() ?? new TSection();
    }
    
    public static JsonSerializerOptions GetDefaultJsonOptions()
    {
        var jsonOptions = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
            // DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            PropertyNameCaseInsensitive = true
        };

        AddDefaultJsonConverters(jsonOptions);

        return jsonOptions;
    }

    public static void AddDefaultJsonConverters(this JsonSerializerOptions jsonOptions)
    {
        jsonOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public static IServiceCollection AddAuthServices(this IServiceCollection services)
    {
        services
            .AddCoreServices()
            .AddScoped<IAuthService, AuthService>()
            ;

        return services;
    }

    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services
            .AddDataBase()
            .AddRepositories()
            .AddMediatr()
            ;

        return services;
    }

    public static IServiceCollection AddMediatr(this IServiceCollection services)
    {
        services
            .AddMediatR(x => x.RegisterServicesFromAssemblies(
                // typeof(IDomainEventNotificationHandler<>).Assembly,
                typeof(IUseCaseDispatcher).Assembly))
            // .AddScoped<IDomainEventDispatcher, DomainEventDispatcher>()
            .AddScoped<IUseCaseDispatcher, UseCaseDispatcher>()
            ;

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

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services
            .AddScoped<ICommonRepository, CommonRepository>()
            .AddScoped<IProductRepository, ProductRepository>()
            .AddScoped<IUserRepository, UserRepository>()
            ;

        return services;
    }

    private static string GetConnectionString()
    {
        var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
        var dbPort = Environment.GetEnvironmentVariable("DB_PORT");
        var dbUser = Environment.GetEnvironmentVariable("DB_USER");
        var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
        var dbName = Environment.GetEnvironmentVariable("DB_NAME");

        return $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPassword}";
    }
}