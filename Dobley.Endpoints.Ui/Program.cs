using Dobley.Data.Core;

var builder = WebApplication.CreateBuilder(args);
builder.AddDobleyLogging("Dobley.Endpoints.Ui");

var app = builder.Build();

app.UseDobleyRequestLogging();
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/config.js", (IConfiguration configuration) =>
{
    var botUserName = configuration.GetValue<string>("TELEGRAM_BOT_USERNAME") ?? string.Empty;
    return Results.Text(
        $"window.DobleyUiConfig = {{ telegramBotUserName: {System.Text.Json.JsonSerializer.Serialize(botUserName)} }};",
        "application/javascript");
});

app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));
app.MapFallbackToFile("index.html");

app.Run();
