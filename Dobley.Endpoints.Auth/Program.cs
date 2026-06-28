using Dobley.Data.Core;
using Dobley.Domain.Core.Repositories.Users;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateSlimBuilder(args);

var isLocal = builder.Configuration.GetValue<bool>("ASPNET_LOCAL");

builder.Host.ConfigureAppServices(isLocal);

var services = builder.Services;
services.AddAuthServices();

var app = builder.Build();

app.MapPost("/login", async ([FromBody] UserCredentials credentials, [FromServices] IAuthService auth)
    => await auth.Login(credentials.Login, credentials.Password) is { } token
        ? Results.Ok(new JwtToken(token))
        : Results.Unauthorized());

app.MapPost("/reg", async ([FromBody] UserCredentials credentials, [FromServices] IAuthService auth)
        => await auth.Register(credentials.Login, credentials.Password)
            ? Results.Ok()
            : Results.Forbid());

app.Run();

public record UserCredentials(string Login, string Password);

public record JwtToken(string Token);