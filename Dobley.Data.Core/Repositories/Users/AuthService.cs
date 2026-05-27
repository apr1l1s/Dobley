using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Dobley.Domain.Core.Entities.Users;
using Dobley.Domain.Core.Repositories;
using Dobley.Domain.Core.Repositories.Users;
using Microsoft.IdentityModel.Tokens;

namespace Dobley.Data.Core.Repositories.Users;

public class AuthService(IUserRepository userRepository, ICommonRepository commonRepository)
    : IAuthService
{
    private const int SALT_SIZE = 16;
    private const int HASH_SIZE = 32;
    private const int ITERATIONS = 100_000;
    private const int TOKEN_LIFETIME_MINUTES = 360;

    public async Task<bool> Register(string login, string password)
    {
        try
        {
            var hash = Hash(password);
            var user = User.Create(login, hash);
            await userRepository.AddAsync(user);
            await commonRepository.SaveChangesAsync();
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }

    public async Task<string?> Login(string login, string password)
    {
        return await userRepository.GetByLogin(login) is { } user && Verify(password, user.Password)
            ? GenerateToken(user)
            : null;
    }

    private string GenerateToken(User user)
    {
        var bytes = Environment.GetEnvironmentVariable("SECRET_KEY")!;
        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(bytes));
        var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Login),
            new Claim(ClaimTypes.Name, user.Login)
        };

        var token = new JwtSecurityToken(
            issuer: "your-auth-service",
            audience: "your-audience",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(TOKEN_LIFETIME_MINUTES),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string Hash(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SALT_SIZE);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            ITERATIONS,
            HashAlgorithmName.SHA256,
            HASH_SIZE
        );

        return Convert.ToBase64String(salt) + "." + Convert.ToBase64String(hash);
    }

    private bool Verify(string password, string hash)
    {
        var parts = hash.Split('.');
        if (parts.Length != 2)
        {
            return false;
        }

        byte[] salt = Convert.FromBase64String(parts[0]);
        byte[] expectedHash = Convert.FromBase64String(parts[1]);
        byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            ITERATIONS,
            HashAlgorithmName.SHA256,
            expectedHash.Length
        );

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }
}