using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
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

        [Test]
        public void SaveDraft_HasAuthorizeAttribute()
        {
            var attr = typeof(OverridesController)
                .GetMethod(nameof(OverridesController.SaveDraft))!
                .GetCustomAttributes(typeof(AuthorizeAttribute), false)
                .Cast<AuthorizeAttribute>()
                .SingleOrDefault();

            attr.Should().NotBeNull();
            attr!.Roles.Should().Be("Admin,Editor");
        }

        [Test]
        public async Task PublishDraft_ReturnsOk()
        {
            var id = Guid.NewGuid();
            var request = new IdRequest(id);

            var result = await _controller.PublishDraft(request);

            result.Should().BeOfType<OkResult>();
            _service.Verify(s => s.PublishDraftAsync(id), Times.Once);
        }

        [Test]
        public void PublishDraft_HasAuthorizeAttribute()
        {
            var attr = typeof(OverridesController)
                .GetMethod(nameof(OverridesController.PublishDraft))!
                .GetCustomAttributes(typeof(AuthorizeAttribute), false)
                .Cast<AuthorizeAttribute>()
                .SingleOrDefault();

            attr.Should().NotBeNull();
            attr!.Roles.Should().Be("Admin,Editor");
        }

        [Test]
        public async Task GetHistory_ReturnsOk()
        {
            _service.Setup(s => s.GetHistoryAsync("home", "key"))
                .ReturnsAsync(new List<ContentOverride>());

            var result = await _controller.GetHistory("home", "key");

            result.Should().BeOfType<OkObjectResult>();
            _service.Verify(s => s.GetHistoryAsync("home", "key"), Times.Once);
        }

        [Test]
        public async Task Revert_ReturnsOk()
        {
            var request = new RevertRequest(Guid.NewGuid(), Guid.NewGuid());

            var result = await _controller.Revert(request);

            result.Should().BeOfType<OkResult>();
            _service.Verify(s => s.RevertAsync(request.Id, request.ChangedById), Times.Once);
        }

        [Test]
        public void Revert_HasAuthorizeAttribute()
        {
            var attr = typeof(OverridesController)
                .GetMethod(nameof(OverridesController.Revert))!
                .GetCustomAttributes(typeof(AuthorizeAttribute), false)
                .Cast<AuthorizeAttribute>()
                .SingleOrDefault();

            attr.Should().NotBeNull();
            attr!.Roles.Should().Be("Admin,Editor");
        }
    }
}
