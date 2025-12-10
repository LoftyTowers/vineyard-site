using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using VineyardApi.Repositories;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

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

        public async Task<string?> LoginAsync(string username, string password)
        {
            using var scope = _logger.BeginScope(new Dictionary<string, object>{{"Username", username}});
            var user = await _users.GetByUsernameAsync(username);
            if (user == null)
            {
                _logger.LogWarning("User not found during login");
                return null;
            }
            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                _logger.LogWarning("Invalid password for {Username}", username);
                return null;
            }

            user.LastLogin = DateTime.UtcNow;
            await _users.SaveChangesAsync();
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
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
