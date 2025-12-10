using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using VineyardApi.Controllers;
using VineyardApi.Infrastructure;
using VineyardApi.Services;
using VineyardApi.Tests;

namespace VineyardApi.Tests.Controllers
{
    public class AuthControllerTests
    {
        private Mock<IAuthService> _service = null!;
        private AuthController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _service = new Mock<IAuthService>();
            _controller = new AuthController(_service.Object);
        }

        [Test]
        public async Task Login_ReturnsOk_WhenTokenReturned()
        {
            _service.Setup(s => s.LoginAsync("user", "pass")).ReturnsAsync("tok");

            var result = await _controller.Login(new LoginRequest("user", "pass"));

            result.Should().BeOfType<OkObjectResult>();
            ResultHttpMapper.MapToStatusCode(result).Should().Be(StatusCodes.Status200OK);
        }

        [Test]
        public async Task Login_ReturnsUnauthorized_WhenTokenNull()
        {
            _service.Setup(s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((string?)null);

            var result = await _controller.Login(new LoginRequest("u", "p"));

            var problem = result.Should().BeOfType<ObjectResult>().Subject.Value as ProblemDetails;
            problem.Should().NotBeNull();
            problem!.Status.Should().Be(StatusCodes.Status400BadRequest);
            problem.Extensions["errorCode"].Should().Be(ErrorCode.BadRequest.ToString());
        }
    }
}
