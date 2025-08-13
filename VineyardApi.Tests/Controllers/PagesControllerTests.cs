using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using VineyardApi.Controllers;
using VineyardApi.Domain.Content;
using VineyardApi.Models;
using VineyardApi.Services;

namespace VineyardApi.Tests.Controllers
{
    public class PagesControllerTests
    {
        private Mock<IPageService> _service = null!;
        private PagesController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _service = new Mock<IPageService>();
            _controller = new PagesController(_service.Object);
        }

        [Test]
        public async Task GetPage_ReturnsOk_WhenFound()
        {
            var content = new PageContent();
            _service.Setup(s => s.GetPageContentAsync("home")).ReturnsAsync(content);

            var result = await _controller.GetPage("home");

            result.Should().BeOfType<OkObjectResult>();
        }

        [Test]
        public async Task GetPage_ReturnsNotFound_WhenMissing()
        {
            _service.Setup(s => s.GetPageContentAsync("missing")).ReturnsAsync((PageContent?)null);

            var result = await _controller.GetPage("missing");

            result.Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public async Task SaveOverride_ReturnsOk()
        {
            var model = new PageOverride();

            var result = await _controller.SaveOverride(model);

            result.Should().BeOfType<OkResult>();
            _service.Verify(s => s.SaveOverrideAsync(model), Times.Once);
        }
    }
}
