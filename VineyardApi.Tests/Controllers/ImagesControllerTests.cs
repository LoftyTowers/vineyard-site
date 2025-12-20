using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using VineyardApi.Controllers;
using VineyardApi.Models;
using VineyardApi.Services;

namespace VineyardApi.Tests.Controllers
{
    public class ImagesControllerTests
    {
        private Mock<IImageService> _service = null!;
        private Mock<IValidator<Image>> _validator = null!;
        private ImagesController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _service = new Mock<IImageService>();

            _validator = new Mock<IValidator<Image>>();
            _validator
                .Setup(v => v.ValidateAsync(It.IsAny<Image>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _controller = new ImagesController(
                _service.Object,
                NullLogger<ImagesController>.Instance,
                _validator.Object
            );
        }

        [Test]
        public async Task SaveImage_ReturnsOk()
        {
            var input = new Image { StorageKey = "images/test.jpg", PublicUrl = "https://cdn.example.com/test.jpg" };

            _service
                .Setup(s => s.SaveImageAsync(input, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<Image>.Ok(input));

            var result = await _controller.SaveImageAsync(input, CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
            _service.Verify(s => s.SaveImageAsync(input, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task GetImages_ReturnsCancelled_WhenTokenCancelled()
        {
            _service.Setup(s => s.GetActiveImagesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationCanceledException());

            var result = await _controller.GetImagesAsync(new CancellationToken(true));

            var problem = result.Should().BeOfType<ObjectResult>().Subject.Value as ProblemDetails;
            problem.Should().NotBeNull();
            problem!.Status.Should().Be(499);
            problem.Extensions["errorCode"].Should().Be(ErrorCode.Cancelled.ToString());
        }

        [Test]
        public async Task SaveImage_ReturnsServerError_OnUnexpectedException()
        {
            var input = new Image { StorageKey = "images/test.jpg" };
            _service.Setup(s => s.SaveImageAsync(input, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("boom"));

            var result = await _controller.SaveImageAsync(input, CancellationToken.None);

            var problem = result.Should().BeOfType<ObjectResult>().Subject.Value as ProblemDetails;
            problem.Should().NotBeNull();
            problem!.Status.Should().Be(StatusCodes.Status500InternalServerError);
            problem.Extensions["errorCode"].Should().Be(ErrorCode.Unexpected.ToString());
        }
    }
}
