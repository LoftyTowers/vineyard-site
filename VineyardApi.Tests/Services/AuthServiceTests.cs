using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using VineyardApi.Models;
using VineyardApi.Repositories;
using VineyardApi.Services;

namespace VineyardApi.Tests.Services
{
    public class AuthServiceTests
    {
        private Mock<IUserRepository> _users = null!;
        private IConfiguration _config = null!;
        private AuthService _service = null!;

        [SetUp]
        public void Setup()
        {
            _users = new Mock<IUserRepository>();
            var inMemorySettings = new Dictionary<string, string?>
            {
                {"Jwt:Key", "supersecret_key_123"}
            };
            _config = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
            _service = new AuthService(_users.Object, _config, NullLogger<AuthService>.Instance);
        }

        [Test]
        public async Task LoginAsync_ReturnsToken_WhenCredentialsValid()
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "john",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass")
            };
            _users.Setup(u => u.GetByUsernameAsync("john", It.IsAny<CancellationToken>()))
                  .ReturnsAsync(user);
            _users.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                  .ReturnsAsync(1);

            var token = await _service.LoginAsync("john", "pass");

            token.IsSuccess.Should().BeTrue();
            _users.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            user.LastLogin.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Test]
        public async Task LoginAsync_ReturnsUnauthorized_WhenUserNotFound()
        {
            _users.Setup(u => u.GetByUsernameAsync("missing", It.IsAny<CancellationToken>()))
                  .ReturnsAsync((User?)null);

            var token = await _service.LoginAsync("missing", "pass");

            token.IsFailure.Should().BeTrue();
            token.Error.Should().Be(ErrorCode.Unauthorized);
        }

        [Test]
        public async Task LoginAsync_ReturnsUnauthorized_WhenPasswordInvalid()
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "john",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("pass")
            };
            _users.Setup(u => u.GetByUsernameAsync("john", It.IsAny<CancellationToken>()))
                  .ReturnsAsync(user);

            var token = await _service.LoginAsync("john", "wrong");

            token.IsFailure.Should().BeTrue();
            token.Error.Should().Be(ErrorCode.Unauthorized);
        }
    }
}
