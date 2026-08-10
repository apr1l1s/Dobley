using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Dobley.Domain.Core.Entities.Users;
using Dobley.Domain.Core.Repositories;
using Dobley.Domain.Core.Repositories.Users;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Dobley.Data.Core.Repositories.Users;

public class AuthService(IUserRepository userRepository, ICommonRepository commonRepository, IDistributedCache cache,
    ILogger<AuthService> logger)
    : IAuthService
{
    private const int SALT_SIZE = 16;
    private const int HASH_SIZE = 32;
    private const int ITERATIONS = 100_000;
    private const int TOKEN_LIFETIME_MINUTES = 360;
    private const int REFRESH_TOKEN_LIFETIME_DAYS = 30;

    public async Task<bool> Register(string login, string password, CancellationToken cancellationToken = default)
    {
        if (await userRepository.GetByLogin(login, cancellationToken) is not null)
        {
            logger.LogInformation("Регистрация отклонена: пользователь уже существует. Логин: {Login}", login);
            return false;
        }

        try
        {
            var hash = Hash(password);
            var user = User.Create(login, hash);
            await userRepository.AddAsync(user, cancellationToken);
            await commonRepository.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Пользователь зарегистрирован. Логин: {Login}", login);
        }
        catch (DbUpdateException exception)
        {
            logger.LogWarning(exception, "Регистрация не выполнена из-за ошибки обновления базы данных. Логин: {Login}",
                login);
            return false;
        }

        return true;
    }

    public async Task<AuthTokenPair?> Login(string login, string password, CancellationToken cancellationToken = default)
    {
        if (await userRepository.GetByLogin(login, cancellationToken) is not { } user ||
            !Verify(password, user.Password))
        {
            logger.LogWarning("Вход не выполнен. Логин: {Login}", login);
            return null;
        }

        logger.LogInformation("Вход выполнен. Логин: {Login}", login);
        return await GenerateTokenPair(user, cancellationToken);
    }

    public async Task<AuthTokenPair?> Refresh(string refreshToken, CancellationToken cancellationToken = default)
    {
        var refreshTokenHash = HashRefreshToken(refreshToken);
        var cacheKey = GetRefreshTokenCacheKey(refreshTokenHash);
        var userLogin = await cache.GetStringAsync(cacheKey, cancellationToken);

        if (string.IsNullOrEmpty(userLogin))
        {
            logger.LogWarning("Refresh-токен отклонён.");
            return null;
        }

        await cache.RemoveAsync(cacheKey, cancellationToken);

        var user = await userRepository.GetByLogin(userLogin, cancellationToken);
        if (user == null)
        {
            logger.LogWarning("Refresh-токен отклонён: пользователь не найден. Логин: {Login}", userLogin);
            return null;
        }

        logger.LogInformation("Refresh-токен обновлён. Логин: {Login}", userLogin);
        return await GenerateTokenPair(user, cancellationToken);
    }

    public Task Logout(string refreshToken, CancellationToken cancellationToken = default)
    {
        var refreshTokenHash = HashRefreshToken(refreshToken);
        logger.LogInformation("Refresh-токен отозван при выходе.");
        return cache.RemoveAsync(GetRefreshTokenCacheKey(refreshTokenHash), cancellationToken);
    }

    private async Task<AuthTokenPair> GenerateTokenPair(User user, CancellationToken cancellationToken)
    {
        var refreshToken = GenerateRefreshToken();
        var refreshTokenHash = HashRefreshToken(refreshToken);

        await cache.SetStringAsync(GetRefreshTokenCacheKey(refreshTokenHash), user.Login,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(REFRESH_TOKEN_LIFETIME_DAYS)
            }, cancellationToken);

        return new AuthTokenPair(GenerateAccessToken(user), refreshToken);
    }

    private string GenerateAccessToken(User user)
    {
        var bytes = DependencyInjection.GetRequiredSecretKey();
        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(bytes));
        var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Login),
            new Claim(ClaimTypes.Name, user.Login)
        };

        var token = new JwtSecurityToken(
            issuer: DependencyInjection.GetJwtIssuer(),
            audience: DependencyInjection.GetJwtAudience(),
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(TOKEN_LIFETIME_MINUTES),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static string HashPassword(string password) => Hash(password);

    private static string Hash(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SALT_SIZE);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, ITERATIONS, HashAlgorithmName.SHA256, HASH_SIZE);

        return Convert.ToBase64String(salt) + "." + Convert.ToBase64String(hash);
    }

    private static bool Verify(string password, string hash)
    {
        var parts = hash.Split('.');
        if (parts.Length != 2)
        {
            return false;
        }

        byte[] salt = Convert.FromBase64String(parts[0]);
        byte[] expectedHash = Convert.FromBase64String(parts[1]);
        byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, ITERATIONS, HashAlgorithmName.SHA256,
            expectedHash.Length);

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }

    private static string GenerateRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    private static string HashRefreshToken(string refreshToken)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
        return Convert.ToBase64String(hash);
    }

    private static string GetRefreshTokenCacheKey(string refreshTokenHash) => $"refresh:{refreshTokenHash}";
}
