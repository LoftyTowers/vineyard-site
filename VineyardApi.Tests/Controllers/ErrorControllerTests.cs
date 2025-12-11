using System.Threading;
using FluentAssertions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using VineyardApi.Controllers;
using VineyardApi.Tests;

namespace VineyardApi.Tests.Controllers
{
    public class ErrorControllerTests
    {
        private Mock<ILogger<ErrorController>> _logger = null!;
        private ErrorController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _logger = new Mock<ILogger<ErrorController>>();
            _controller = new ErrorController(_logger.Object);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        [Test]
        public void HandleStatusCode_404_ReturnsNotFoundMessage()
        {
            var result = _controller.HandleStatusCode(404, CancellationToken.None);

            var problem = (result as ObjectResult)?.Value as ProblemDetails;
            problem.Should().NotBeNull();
            problem!.Status.Should().Be(404);
            problem!.Title.Should().Be("Resource not found");
        }

        [Test]
        public void HandleStatusCode_Non404_ReturnsGenericMessage()
        {
            var result = _controller.HandleStatusCode(500, CancellationToken.None);

            var problem = (result as ObjectResult)?.Value as ProblemDetails;
            problem.Should().NotBeNull();
            problem!.Status.Should().Be(500);
            problem!.Title.Should().Be("An unexpected error occurred");
        }

        [Test]
        public void HandleStatusCode_400_MapsToBadRequest()
        {
            var result = _controller.HandleStatusCode(StatusCodes.Status400BadRequest, CancellationToken.None);

            ResultHttpMapper.MapToStatusCode(result).Should().Be(StatusCodes.Status400BadRequest);
        }

        [Test]
        public void HandleStatusCode_499_MapsToClientClosedRequest()
        {
            var result = _controller.HandleStatusCode(499, CancellationToken.None);

            ResultHttpMapper.MapToStatusCode(result).Should().Be(499);
        }

        [Test]
        public void HandleException_LogsErrorAndReturnsProblem()
        {
            var httpContext = new DefaultHttpContext();
            var exception = new System.Exception("oops");
            var feature = new Mock<IExceptionHandlerFeature>();
            feature.Setup(f => f.Error).Returns(exception);
            httpContext.Features.Set(feature.Object);
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var result = _controller.HandleException(CancellationToken.None);

            _logger.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                exception,
                It.IsAny<Func<It.IsAnyType, System.Exception?, string>>()),
                Times.Once);

            var problem = (result as ObjectResult)?.Value as ProblemDetails;
            problem.Should().NotBeNull();
            problem!.Status.Should().Be(500);
            ResultHttpMapper.MapToStatusCode(result).Should().Be(StatusCodes.Status500InternalServerError);
        }
    }
}
