using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using VineyardApi.Controllers;
using VineyardApi.Models;
using VineyardApi.Services;

namespace VineyardApi.Tests.Controllers
{
    public class ThemeControllerTests
    {
        private Mock<IThemeService> _service = null!;
        private ThemeController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _service = new Mock<IThemeService>();
            _controller = new ThemeController(_service.Object);
        }

        [Test]
        public async Task GetTheme_ReturnsOk()
        {
            _service.Setup(s => s.GetThemeAsync()).ReturnsAsync(new Dictionary<string, string>());

            var result = await _controller.GetTheme();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Test]
        public async Task SaveOverride_ReturnsOk()
        {
            var model = new ThemeOverride();

            var result = await _controller.SaveOverride(model);

            result.Should().BeOfType<OkResult>();
            _service.Verify(s => s.SaveOverrideAsync(model), Times.Once);
        }
    }
}
