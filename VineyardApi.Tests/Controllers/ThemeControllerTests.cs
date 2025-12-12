using System.Collections.Generic;
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
    public class ThemeControllerTests
    {
        private Mock<IThemeService> _service = null!;
        private Mock<IValidator<ThemeOverride>> _validator = null!;
        private ThemeController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _service = new Mock<IThemeService>();

            _validator = new Mock<IValidator<ThemeOverride>>();
            _validator
                .Setup(v => v.ValidateAsync(It.IsAny<ThemeOverride>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _controller = new ThemeController(
                _service.Object,
                NullLogger<ThemeController>.Instance,
                _validator.Object
            );
        }

        [Test]
        public async Task GetTheme_ReturnsOk()
        {
            _service.Setup(s => s.GetThemeAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<Dictionary<string, string>>.Ok(new Dictionary<string, string>()));

            var result = await _controller.GetThemeAsync(CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Test]
        public async Task SaveOverride_ReturnsOk()
        {
            var model = new ThemeOverride();
            _service.Setup(s => s.SaveOverrideAsync(model, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Ok());

            var result = await _controller.SaveOverrideAsync(model, CancellationToken.None);

            result.Should().BeOfType<OkResult>();
            _service.Verify(s => s.SaveOverrideAsync(model, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
