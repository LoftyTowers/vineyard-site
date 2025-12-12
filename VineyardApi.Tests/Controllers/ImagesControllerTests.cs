using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
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
            var input = new Image { Url = "a" };

            _service
                .Setup(s => s.SaveImageAsync(input, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<Image>.Ok(input));

            var result = await _controller.SaveImageAsync(input, CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
            _service.Verify(s => s.SaveImageAsync(input, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
