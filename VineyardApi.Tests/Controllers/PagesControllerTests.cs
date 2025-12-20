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
using VineyardApi.Domain.Content;
using VineyardApi.Models;
using VineyardApi.Services;
using VineyardApi.Tests;
using System.Collections.Generic;
using System;

namespace VineyardApi.Tests.Controllers
{
    public class PagesControllerTests
    {
        private Mock<IPageService> _service = null!;
        private Mock<IValidator<PageOverride>> _validator = null!;
        private PagesController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _service = new Mock<IPageService>();

            _validator = new Mock<IValidator<PageOverride>>();
            _validator
                .Setup(v => v.ValidateAsync(It.IsAny<PageOverride>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _controller = new PagesController(
                _service.Object,
                NullLogger<PagesController>.Instance,
                _validator.Object
            );
        }

        [Test]
        public async Task GetPage_ReturnsOk_WhenFound()
        {
            var content = new PageContent();
            _service
                .Setup(s => s.GetPageContentAsync("home", It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<PageContent>.Ok(content));

            var result = await _controller.GetPageAsync("home", CancellationToken.None);

            var ok = result.Should().BeAssignableTo<ObjectResult>().Subject;
            ok.StatusCode.Should().Be(StatusCodes.Status200OK);
            ok.Value.Should().BeSameAs(content);
            _service.Verify(s => s.GetPageContentAsync("home", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task GetPage_ReturnsNotFound_WhenMissing()
        {
            _service.Setup(s => s.GetPageContentAsync("missing", It.IsAny<CancellationToken>())).ReturnsAsync(Result<PageContent>.Failure(ErrorCode.NotFound));

            var result = await _controller.GetPageAsync("missing", CancellationToken.None);

            var problem = result.Should().BeOfType<ObjectResult>().Subject.Value as ProblemDetails;
            problem.Should().NotBeNull();
            problem!.Status.Should().Be(StatusCodes.Status404NotFound);
            problem.Extensions["errorCode"].Should().Be(ErrorCode.NotFound.ToString());
        }

        [Test]
        public async Task SaveOverride_ReturnsOk()
        {
            var model = new PageOverride();
            _service.Setup(s => s.SaveOverrideAsync(model, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Ok());

            var result = await _controller.SaveOverrideAsync(model, CancellationToken.None);

            result.Should().BeOfType<OkResult>();
            _service.Verify(s => s.SaveOverrideAsync(model, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Autosave_ReturnsOk_WhenServiceSucceeds()
        {
            var content = new PageContent();
            var model = new AutosaveDraftRequest { Content = content };
            _service.Setup(s => s.AutosaveDraftAsync("home", content, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Ok());

            var result = await _controller.AutosaveAsync("home", model, CancellationToken.None);

            result.Should().BeOfType<OkResult>();
            _service.Verify(s => s.AutosaveDraftAsync("home", content, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Autosave_ReturnsBadRequest_WhenContentMissing()
        {
            var model = new AutosaveDraftRequest { Content = null };

            var result = await _controller.AutosaveAsync("home", model, CancellationToken.None);

            var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequest.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }

        [Test]
        public async Task GetPage_ReturnsCancelled_WhenTokenCancelled()
        {
            var cts = new CancellationTokenSource();
            _service
                .Setup(s => s.GetPageContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationCanceledException());

            var result = await _controller.GetPageAsync("home", cts.Token);

            var problem = result.Should().BeOfType<ObjectResult>().Subject.Value as ProblemDetails;
            problem.Should().NotBeNull();
            problem!.Status.Should().Be(499);
            problem.Extensions["errorCode"].Should().Be(ErrorCode.Cancelled.ToString());
        }

        [Test]
        public async Task SaveOverride_ReturnsServerError_OnUnexpectedException()
        {
            var model = new PageOverride();
            _service.Setup(s => s.SaveOverrideAsync(model, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("boom"));

            var result = await _controller.SaveOverrideAsync(model, CancellationToken.None);

            var problem = result.Should().BeOfType<ObjectResult>().Subject.Value as ProblemDetails;
            problem.Should().NotBeNull();
            problem!.Status.Should().Be(StatusCodes.Status500InternalServerError);
            problem.Extensions["errorCode"].Should().Be(ErrorCode.Unexpected.ToString());
        }

        [Test]
        public async Task GetPublishedVersions_ReturnsOk()
        {
            var versions = new List<PageVersionSummary>
            {
                new(Guid.NewGuid(), 2, DateTime.UtcNow, "note")
            };
            _service.Setup(s => s.GetPublishedVersionsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<List<PageVersionSummary>>.Ok(versions));

            var result = await _controller.GetPublishedVersionsAsync("home", CancellationToken.None);

            var ok = result.Should().BeAssignableTo<ObjectResult>().Subject;
            ok.StatusCode.Should().Be(StatusCodes.Status200OK);
            ok.Value.Should().BeSameAs(versions);
        }

        [Test]
        public async Task GetPublishedVersionContent_ReturnsNotFound()
        {
            _service.Setup(s => s.GetPublishedVersionContentAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<PageVersionContentResponse>.Failure(ErrorCode.NotFound));

            var result = await _controller.GetPublishedVersionContentAsync("home", Guid.NewGuid(), CancellationToken.None);

            var problem = result.Should().BeOfType<ObjectResult>().Subject.Value as ProblemDetails;
            problem.Should().NotBeNull();
            problem!.Status.Should().Be(StatusCodes.Status404NotFound);
            problem.Extensions["errorCode"].Should().Be(ErrorCode.NotFound.ToString());
        }

        [Test]
        public async Task Rollback_ReturnsOk()
        {
            _service.Setup(s => s.RollbackToVersionAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<PageContent>.Ok(new PageContent()));

            var result = await _controller.RollbackToVersionAsync("home", Guid.NewGuid(), CancellationToken.None);

            result.Should().BeAssignableTo<ObjectResult>().Subject.StatusCode.Should().Be(StatusCodes.Status200OK);
        }
    }
}
