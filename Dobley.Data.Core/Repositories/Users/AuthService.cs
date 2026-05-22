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
    private const int SALT_SIZE = 16; // Размер соли (в байтах)
    private const int HASH_SIZE = 32; // Размер хэша (в байтах)
    private const int ITERATIONS = 100_000; // Количество итераций
    private const int TOKEN_LIFETIME_MINUTES = 360;

    public async Task<bool> Register(string login, string password)
    {
        var hash = Hash(password);
        var user = User.Create(login, hash);
        await userRepository.AddAsync(user);
        await commonRepository.SaveChangesAsync();

        return true;
    }

    public async Task<string> Login(string login, string password)
    {
        var user = await userRepository.GetByLogin(login);
        if (user == null || !Verify(password, user.Password))
        {
            throw new UnauthorizedAccessException();
        }

        return GenerateToken(user);
    }

    private string GenerateToken(User user)
    {
        var secretKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("SECRET_KEY")!));
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

    public string Hash(string password)
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

    public bool Verify(string password, string hash)
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