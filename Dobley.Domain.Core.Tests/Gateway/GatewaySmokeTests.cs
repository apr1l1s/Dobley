using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Dobley.Domain.Core.Tests.Gateway;

public class GatewaySmokeTests
{
    private const string RUN_GATEWAY_TESTS_VARIABLE = "DOBLEY_RUN_GATEWAY_TESTS";
    private static readonly Uri GatewayUri = new("http://127.0.0.1:5000");

    [Fact]
    public async Task GatewayHealth_ShouldReturnOk()
    {
        if (!ShouldRun())
        {
            return;
        }

        using var client = CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Products_ShouldRequireAuthorization()
    {
        if (!ShouldRun())
        {
            return;
        }

        using var client = CreateClient();

        var response = await client.GetAsync("/api/products/");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreatingProduct_WithInvalidBody_ShouldReturnRussianValidationError()
    {
        if (!ShouldRun())
        {
            return;
        }

        using var client = CreateClient();
        var accessToken = await RegisterAndLogin(client);
        var body = new
        {
            name = "Bread",
            description = "Good bread",
            price = 78,
            category = "Unknown",
            unit = 1,
            unitType = "Pieces",
            barcode = "75623"
        };
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/products/create")
        {
            Content = JsonContent.Create(body)
        };
        request.Headers.Authorization = new("Bearer", accessToken);

        var response = await client.SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("Некорректное тело запроса", responseBody);
    }

    private static HttpClient CreateClient() => new()
    {
        BaseAddress = GatewayUri,
        Timeout = TimeSpan.FromSeconds(10)
    };

    private static async Task<string> RegisterAndLogin(HttpClient client)
    {
        var login = $"smoke-{Guid.NewGuid():N}";
        var credentials = new
        {
            login,
            password = "password"
        };

        await client.PostAsJsonAsync("/auth/reg", credentials);

        var response = await client.PostAsJsonAsync("/auth/login", credentials);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(responseBody);

        return document.RootElement.GetProperty("accessToken").GetString()
               ?? throw new InvalidOperationException("Auth response does not contain accessToken.");
    }

    private static bool ShouldRun()
        => bool.TryParse(Environment.GetEnvironmentVariable(RUN_GATEWAY_TESTS_VARIABLE), out var shouldRun) &&
           shouldRun;
}
