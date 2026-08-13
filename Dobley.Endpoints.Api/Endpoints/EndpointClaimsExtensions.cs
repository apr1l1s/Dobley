using System.Security.Claims;

namespace Dobley.Endpoints.Api.Endpoints;

public static class EndpointClaimsExtensions
{
    public static string GetCurrentUserName(this ClaimsPrincipal user)
        => user.FindFirstValue(ClaimTypes.Name)
           ?? user.FindFirstValue(ClaimTypes.NameIdentifier)
           ?? throw new UnauthorizedAccessException("User name claim is required.");
}
