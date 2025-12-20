using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VineyardApi.Models;
using Moq;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using VineyardApi.Controllers;
using VineyardApi.Models.Requests;
using VineyardApi.Services;
using VineyardApi.Tests;

namespace VineyardApi.Tests.Controllers
{
    public class AuthControllerTests
    {
        private Mock<IAuthService> _service = null!;
        private Mock<IValidator<LoginRequest>> _validator = null!;
        private AuthController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _service = new Mock<IAuthService>();

            _validator = new Mock<IValidator<LoginRequest>>();
            _validator
                .Setup(v => v.ValidateAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _controller = new AuthController(
                _service.Object,
                NullLogger<AuthController>.Instance,
                _validator.Object
            );
        }

        [Test]
        public async Task Login_ReturnsOk_WhenTokenReturned()
        {
            _service.Setup(s => s.LoginAsync("user", "pass", It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<string>.Ok("tok"));

            var request = new LoginRequest("user", "pass");

            var result = await _controller.LoginAsync(request, CancellationToken.None);

            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.Value.Should().BeEquivalentTo(new { token = "tok" });
            ResultHttpMapper.MapToStatusCode(result).Should().Be(StatusCodes.Status200OK);
        }

        [Test]
        public async Task Login_ReturnsBadRequest_WhenTokenNull()
        {
            _service.Setup(s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<string>.Failure(ErrorCode.BadRequest, "Invalid username or password"));

            var request = new LoginRequest("u", "p");

            var result = await _controller.LoginAsync(request, CancellationToken.None);

            var problem = result.Should().BeOfType<ObjectResult>().Subject.Value as ProblemDetails;
            problem.Should().NotBeNull();
            problem!.Status.Should().Be(StatusCodes.Status400BadRequest);
            problem.Extensions["errorCode"].Should().Be(ErrorCode.BadRequest.ToString());
        }

        [Test]
        public async Task Login_ReturnsCancelled_WhenTokenCancelled()
        {
            var request = new LoginRequest("user", "pass");
            _validator.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            _service.Setup(s => s.LoginAsync(request.Username, request.Password, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationCanceledException());

            var result = await _controller.LoginAsync(request, new CancellationToken(true));

            var problem = result.Should().BeOfType<ObjectResult>().Subject.Value as ProblemDetails;
            problem.Should().NotBeNull();
            problem!.Status.Should().Be(499);
            problem.Extensions["errorCode"].Should().Be(ErrorCode.Cancelled.ToString());
        }

        [Test]
        public async Task Login_ReturnsServerError_OnUnexpectedException()
        {
            var request = new LoginRequest("user", "pass");
            _validator.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());
            _service.Setup(s => s.LoginAsync(request.Username, request.Password, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("boom"));

            var result = await _controller.LoginAsync(request, CancellationToken.None);

            var problem = result.Should().BeOfType<ObjectResult>().Subject.Value as ProblemDetails;
            problem.Should().NotBeNull();
            problem!.Status.Should().Be(StatusCodes.Status500InternalServerError);
            problem.Extensions["errorCode"].Should().Be(ErrorCode.Unexpected.ToString());
        }
    }
}
