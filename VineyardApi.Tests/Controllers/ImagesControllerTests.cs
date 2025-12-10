using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
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
        private ImagesController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _service = new Mock<IImageService>();
            _controller = new ImagesController(_service.Object, NullLogger<ImagesController>.Instance);
        }

        [Test]
        public async Task SaveImage_ReturnsOk()
        {
            var input = new Image { Url = "a" };
            _service.Setup(s => s.SaveImageAsync(input)).ReturnsAsync(input);

            var result = await _controller.SaveImage(input);

            result.Should().BeOfType<OkObjectResult>();
            _service.Verify(s => s.SaveImageAsync(input), Times.Once);
        }
    }
}
