using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using VineyardApi.Controllers;
using VineyardApi.Models;
using VineyardApi.Services;

namespace VineyardApi.Tests.Controllers
{
    public class AuditControllerTests
    {
        private Mock<IAuditService> _service = null!;
        private AuditController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _service = new Mock<IAuditService>();
            _controller = new AuditController(_service.Object, NullLogger<AuditController>.Instance);
        }

        [Test]
        public async Task GetRecent_ReturnsOk()
        {
            _service.Setup(s => s.GetRecentAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<List<AuditLog>>.Ok(new List<AuditLog>()));

            var result = await _controller.GetRecentAsync(CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Test]
        public async Task GetRecent_ReturnsBadRequest_OnValidationFailure()
        {
            var validationErrors = new[] { new ValidationError("Count", "Invalid") };
            _service.Setup(s => s.GetRecentAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<List<AuditLog>>.Failure(ErrorCode.Validation, "Validation failed", validationErrors));

            var result = await _controller.GetRecentAsync(CancellationToken.None);

            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequest.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }

        [Test]
        public async Task GetRecent_ReturnsServerError_OnUnexpectedException()
        {
            _service.Setup(s => s.GetRecentAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("boom"));

            var result = await _controller.GetRecentAsync(CancellationToken.None);

            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }

        [Test]
        public async Task GetRecent_ReturnsServerError_OnCancellation()
        {
            _service.Setup(s => s.GetRecentAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationCanceledException());

            var result = await _controller.GetRecentAsync(new CancellationToken(true));

            var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
            objectResult.StatusCode.Should().Be(499);
            var problem = objectResult.Value.Should().BeAssignableTo<ProblemDetails>().Subject;
            problem.Status.Should().Be(499);
            problem.Extensions["errorCode"].Should().Be(ErrorCode.Cancelled.ToString());
        }

        [Test]
        public void Controller_HasAuthorizeAttribute()
        {
            var attr = typeof(AuditController)
                .GetCustomAttributes(typeof(AuthorizeAttribute), false)
                .Cast<AuthorizeAttribute>()
                .SingleOrDefault();

            attr.Should().NotBeNull();
            attr!.Roles.Should().Be("Admin");
        }
    }
}
