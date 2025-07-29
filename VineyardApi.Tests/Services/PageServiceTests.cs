using System;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
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
            _service = new PageService(_repo.Object);
        }

        [Test]
        public async Task GetPageContentAsync_ReturnsMergedContent_WhenOverrideExists()
        {
            var page = new Page
            {
                Id = Guid.NewGuid(),
                Route = "home",
                DefaultContent = JsonNode.Parse("{\"greeting\":\"hi\"}")!.AsObject(),
                Overrides = new[]
                {
                    new PageOverride
                    {
                        OverrideContent = JsonNode.Parse("{\"greeting\":\"bye\"}")!.AsObject(),
                        UpdatedAt = DateTime.UtcNow
                    }
                }
            };
            _repo.Setup(r => r.GetPageWithOverridesAsync("home")).ReturnsAsync(page);

            var result = await _service.GetPageContentAsync("home");

            result.Should().NotBeNull();
            result!["greeting"]!.GetValue<string>().Should().Be("bye");
        }

        [Test]
        public async Task GetPageContentAsync_ReturnsNull_WhenPageMissing()
        {
            _repo.Setup(r => r.GetPageWithOverridesAsync("missing")).ReturnsAsync((Page?)null);

            var result = await _service.GetPageContentAsync("missing");

            result.Should().BeNull();
        }

        [Test]
        public async Task SaveOverrideAsync_Inserts_WhenNoExisting()
        {
            var model = new PageOverride { PageId = Guid.NewGuid() };
            _repo.Setup(r => r.GetPageOverrideByPageIdAsync(model.PageId)).ReturnsAsync((PageOverride?)null);

            await _service.SaveOverrideAsync(model);

            _repo.Verify(r => r.AddPageOverride(model), Times.Once);
            _repo.Verify(r => r.SaveChangesAsync(), Times.Once);
            model.UpdatedAt.Should().NotBe(default);
        }

        [Test]
        public async Task SaveOverrideAsync_Updates_WhenExisting()
        {
            var existing = new PageOverride { PageId = Guid.NewGuid(), OverrideContent = new JsonObject { ["foo"] = "bar" } };
            var model = new PageOverride { PageId = existing.PageId, OverrideContent = new JsonObject { ["foo"] = "baz" }, UpdatedById = Guid.NewGuid() };
            _repo.Setup(r => r.GetPageOverrideByPageIdAsync(existing.PageId)).ReturnsAsync(existing);

            await _service.SaveOverrideAsync(model);

            existing.OverrideContent!["foo"]!.GetValue<string>().Should().Be("baz");
            existing.UpdatedById.Should().Be(model.UpdatedById);
            _repo.Verify(r => r.SaveChangesAsync(), Times.Once);
            _repo.Verify(r => r.AddPageOverride(It.IsAny<PageOverride>()), Times.Never);
        }
    }
}
