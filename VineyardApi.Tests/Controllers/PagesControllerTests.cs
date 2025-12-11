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
using VineyardApi.Infrastructure;
using VineyardApi.Controllers;
using VineyardApi.Domain.Content;
using VineyardApi.Models;
using VineyardApi.Services;
using VineyardApi.Tests;

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
            _service.Setup(s => s.GetPageContentAsync("home", It.IsAny<CancellationToken>())).ReturnsAsync(Result<PageContent>.Success(content));

            var result = await _controller.GetPage("home", CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Test]
        public async Task GetPage_ReturnsNotFound_WhenMissing()
        {
            _service.Setup(s => s.GetPageContentAsync("missing", It.IsAny<CancellationToken>())).ReturnsAsync(Result<PageContent>.Failure(ErrorCode.NotFound));

            var result = await _controller.GetPage("missing", CancellationToken.None);

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
                .ReturnsAsync(Result.Success());

            var result = await _controller.SaveOverride(model, CancellationToken.None);

            result.Should().BeOfType<OkResult>();
            _service.Verify(s => s.SaveOverrideAsync(model, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
