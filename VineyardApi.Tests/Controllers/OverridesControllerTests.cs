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
    public class OverridesControllerTests
    {
        private Mock<IContentOverrideService> _service = null!;
        private OverridesController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _service = new Mock<IContentOverrideService>();
            _controller = new OverridesController(_service.Object);
        }

        [Test]
        public async Task GetOverrides_ReturnsOk()
        {
            _service.Setup(s => s.GetPublishedOverridesAsync("home"))
                .ReturnsAsync(new Dictionary<string, string>());

            var result = await _controller.GetOverrides("home");

            result.Should().BeOfType<OkObjectResult>();
        }

        [Test]
        public async Task SaveDraft_ReturnsOk()
        {
            var model = new ContentOverride();

            var result = await _controller.SaveDraft(model);

            result.Should().BeOfType<OkResult>();
            _service.Verify(s => s.SaveDraftAsync(model), Times.Once);
        }
    }
}
