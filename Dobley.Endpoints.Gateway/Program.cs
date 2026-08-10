using Dobley.Endpoints.Gateway.Dto;
using Dobley.Observability;

var builder = WebApplication.CreateSlimBuilder(args);
builder.AddDobleyLogging("Dobley.Endpoints.Gateway");

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.UseDobleyRequestLogging();

app.MapGet("/health", () => Results.Ok(new GatewayHealthResponse("Healthy")));
app.MapReverseProxy();

app.Run();
