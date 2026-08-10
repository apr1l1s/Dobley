using Dobley.Domain.Core.Repositories.Users;

namespace Dobley.Endpoints.Auth.Dto;

public record AuthTokens(string AccessToken, string RefreshToken)
{
    public static AuthTokens Create(AuthTokenPair tokens) => new(tokens.AccessToken, tokens.RefreshToken);
}
