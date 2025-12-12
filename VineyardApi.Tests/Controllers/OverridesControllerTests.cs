using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using VineyardApi.Controllers;
using VineyardApi.Models;
using VineyardApi.Models.Requests;
using VineyardApi.Services;

namespace VineyardApi.Tests.Controllers
{
    public class OverridesControllerTests
    {
        private Mock<IContentOverrideService> _service = null!;
        private Mock<IValidator<ContentOverride>> _overrideValidator = null!;
        private Mock<IValidator<IdRequest>> _idValidator = null!;
        private Mock<IValidator<RevertRequest>> _revertValidator = null!;
        private OverridesController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _service = new Mock<IContentOverrideService>();

            _overrideValidator = new Mock<IValidator<ContentOverride>>();
            _overrideValidator
                .Setup(v => v.ValidateAsync(It.IsAny<ContentOverride>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _idValidator = new Mock<IValidator<IdRequest>>();
            _idValidator
                .Setup(v => v.ValidateAsync(It.IsAny<IdRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _revertValidator = new Mock<IValidator<RevertRequest>>();
            _revertValidator
                .Setup(v => v.ValidateAsync(It.IsAny<RevertRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _controller = new OverridesController(
                _service.Object,
                NullLogger<OverridesController>.Instance,
                _overrideValidator.Object,
                _idValidator.Object,
                _revertValidator.Object
            );
        }

        [Test]
        public async Task GetOverrides_ReturnsOk()
        {
            _service.Setup(s => s.GetPublishedOverridesAsync("home", It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<Dictionary<string, string>>.Ok(new Dictionary<string, string>()));

            var result = await _controller.GetOverridesAsync("home", CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Test]
        public async Task SaveDraft_ReturnsOk()
        {
            var model = new ContentOverride();
            _service.Setup(s => s.SaveDraftAsync(model, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Ok());

            var result = await _controller.SaveDraftAsync(model, CancellationToken.None);

            result.Should().BeOfType<OkResult>();
            _service.Verify(s => s.SaveDraftAsync(model, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void SaveDraft_HasAuthorizeAttribute()
        {
            var attr = typeof(OverridesController)
                .GetMethod(nameof(OverridesController.SaveDraftAsync))!
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
            _service.Setup(s => s.PublishDraftAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Ok());

            var result = await _controller.PublishDraftAsync(request, CancellationToken.None);

            result.Should().BeOfType<OkResult>();
            _service.Verify(s => s.PublishDraftAsync(id, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void PublishDraft_HasAuthorizeAttribute()
        {
            var attr = typeof(OverridesController)
                .GetMethod(nameof(OverridesController.PublishDraftAsync))!
                .GetCustomAttributes(typeof(AuthorizeAttribute), false)
                .Cast<AuthorizeAttribute>()
                .SingleOrDefault();

            attr.Should().NotBeNull();
            attr!.Roles.Should().Be("Admin,Editor");
        }

        [Test]
        public async Task GetHistory_ReturnsOk()
        {
            _service.Setup(s => s.GetHistoryAsync("home", "key", It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<List<ContentOverride>>.Ok(new List<ContentOverride>()));

            var result = await _controller.GetHistoryAsync("home", "key", CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
            _service.Verify(s => s.GetHistoryAsync("home", "key", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Revert_ReturnsOk()
        {
            var request = new RevertRequest(Guid.NewGuid(), Guid.NewGuid());
            _service.Setup(s => s.RevertAsync(request.Id, request.ChangedById, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Ok());

            var result = await _controller.RevertAsync(request, CancellationToken.None);

            result.Should().BeOfType<OkResult>();
            _service.Verify(s => s.RevertAsync(request.Id, request.ChangedById, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void Revert_HasAuthorizeAttribute()
        {
            var attr = typeof(OverridesController)
                .GetMethod(nameof(OverridesController.RevertAsync))!
                .GetCustomAttributes(typeof(AuthorizeAttribute), false)
                .Cast<AuthorizeAttribute>()
                .SingleOrDefault();

            attr.Should().NotBeNull();
            attr!.Roles.Should().Be("Admin,Editor");
        }
    }
}
