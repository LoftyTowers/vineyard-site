using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using VineyardApi.Repositories;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using VineyardApi.Infrastructure;

namespace VineyardApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _users;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IUserRepository users, IConfiguration config, ILogger<AuthService> logger)
        {
            _users = users;
            _config = config;
            _logger = logger;
        }

        public async Task<Result<string>> LoginAsync(string username, string password, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _users.GetByUsernameAsync(username, cancellationToken);
                if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                {
                    _logger.LogWarning("User not found during login");
                    return Result<string>.Failure(ErrorCode.Unauthorized, "Invalid credentials.");
                }

                user.LastLogin = DateTime.UtcNow;
                await _users.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Recorded login for {Username}", username);

                var tokenHandler = new JwtSecurityTokenHandler();
                var keyString = _config["Jwt:Key"] ?? string.Empty;
                if (keyString.Length < 32)
                {
                    keyString = keyString.PadRight(32, '0');
                }

                var key = Encoding.UTF8.GetBytes(keyString);
                var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) };
                foreach (var role in user.Roles.Select(r => r.Role!.Name))
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddHours(1),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                return Result<string>.Success(tokenHandler.WriteToken(token));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging in user {Username}", username);
                return Result<string>.Failure(ErrorCode.Unexpected);
            }
        }
    }
}
