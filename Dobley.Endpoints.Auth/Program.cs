using System.Text.Json.Serialization;
using Dobley.Data.Core.Repositories;
using Dobley.Data.Core.Repositories.Users;
using Dobley.Data.Core.Services;
using Dobley.Domain.Core.Repositories;
using Dobley.Domain.Core.Repositories.Users;

var builder = WebApplication.CreateSlimBuilder(args);
var services = builder.Services;
services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

services.AddDataBase()
    .AddScoped<IUserRepository, UserRepository>()
    .AddScoped<IAuthService, AuthService>()
    .AddScoped<ICommonRepository, CommonRepository>();

var app = builder.Build();

app.MapPost("/login", async (UserCredentials credentials, IAuthService auth)
    => await auth.Login(credentials.Login, credentials.Password) is { } token
        ? Results.Ok(new JwtToken(token))
        : Results.Unauthorized());

app.MapPost("/reg", async (UserCredentials credentials, IAuthService auth)
        => await auth.Register(credentials.Login, credentials.Password)
            ? Results.Ok()
            : Results.Forbid());

app.Run();

public record UserCredentials(string Login, string Password);

public record JwtToken(string Token);

[JsonSerializable(typeof(UserCredentials[]))]
internal partial class AppJsonSerializerContext : JsonSerializerContext;