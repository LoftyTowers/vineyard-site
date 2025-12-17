using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using System.Text.Json;
using VineyardApi.Domain.Content;
using VineyardApi.Models;
using VineyardApi.Repositories;
using VineyardApi.Services;

namespace VineyardApi.Tests.Services
{
    public class PageServiceTests
    {
        private Mock<IPageRepository> _repo = null!;
        private PageService _service = null!;

        [SetUp]
        public void Setup()
        {
            _repo = new Mock<IPageRepository>();
            _service = new PageService(_repo.Object, NullLogger<PageService>.Instance);
        }

        [Test]
        public async Task GetPageContentAsync_ReturnsMergedContent_WhenOverrideExists()
        {
            var page = new Page
            {
                Id = Guid.NewGuid(),
                Route = "home",
                DefaultContent = new PageContent
                {
                    Blocks = { CreateTextBlock("hi") }
                },
                Overrides = new[]
                {
                    new PageOverride
                    {
                        OverrideContent = new PageContent
                        {
                            Blocks = { CreateTextBlock("bye") }
                        },
                        UpdatedAt = DateTime.UtcNow
                    }
                }
            };
            _repo.Setup(r => r.GetPageWithOverridesAsync("home", It.IsAny<CancellationToken>())).ReturnsAsync(page);

            var result = await _service.GetPageContentAsync("home");

            result.IsSuccess.Should().BeTrue();
            result.Value!.Blocks.First().Content.GetString().Should().Be("bye");
        }

        [Test]
        public async Task GetPageContentAsync_ReturnsError_WhenPageMissing()
        {
            _repo.Setup(r => r.GetPageWithOverridesAsync("missing", It.IsAny<CancellationToken>())).ReturnsAsync((Page?)null);

            var result = await _service.GetPageContentAsync("missing");

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorCode.NotFound);
        }

        [Test]
        public async Task SaveOverrideAsync_Inserts_WhenNoExisting()
        {
            var model = new PageOverride { PageId = Guid.NewGuid() };
            _repo.Setup(r => r.GetPageOverrideByPageIdAsync(model.PageId, It.IsAny<CancellationToken>())).ReturnsAsync((PageOverride?)null);
            _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var result = await _service.SaveOverrideAsync(model);

            result.IsSuccess.Should().BeTrue();
            _repo.Verify(r => r.AddPageOverride(model), Times.Once);
            _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            model.UpdatedAt.Should().NotBe(default);
        }

        [Test]
        public async Task SaveOverrideAsync_Updates_WhenExisting()
        {
            var existing = new PageOverride
            {
                PageId = Guid.NewGuid(),
                OverrideContent = new PageContent
                {
                    Blocks = { CreateTextBlock("bar") }
                }
            };
            var model = new PageOverride
            {
                PageId = existing.PageId,
                OverrideContent = new PageContent
                {
                    Blocks = { CreateTextBlock("baz") }
                },
                UpdatedById = Guid.NewGuid()
            };
            _repo.Setup(r => r.GetPageOverrideByPageIdAsync(existing.PageId, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
            _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var result = await _service.SaveOverrideAsync(model);

            result.IsSuccess.Should().BeTrue();
            existing.OverrideContent!.Blocks.First().Content.GetString().Should().Be("baz");
            existing.UpdatedById.Should().Be(model.UpdatedById);
            _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _repo.Verify(r => r.AddPageOverride(It.IsAny<PageOverride>()), Times.Never);
        }

        private static PageBlock CreateTextBlock(string text)
        {
            return new PageBlock
            {
                Type = "p",
                Content = JsonDocument.Parse($"\"{text}\"").RootElement
            };
        }
    }
}
