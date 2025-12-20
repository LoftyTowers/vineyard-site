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
        private Mock<IImageRepository> _imageRepo = null!;
        private Mock<IImageUsageRepository> _imageUsageRepo = null!;
        private Mock<IPeopleRepository> _peopleRepo = null!;
        private PageService _service = null!;

        [SetUp]
        public void Setup()
        {
            _repo = new Mock<IPageRepository>();
            _imageRepo = new Mock<IImageRepository>();
            _imageUsageRepo = new Mock<IImageUsageRepository>();
            _peopleRepo = new Mock<IPeopleRepository>();
            _service = new PageService(_repo.Object, _imageRepo.Object, _imageUsageRepo.Object, _peopleRepo.Object, NullLogger<PageService>.Instance);
        }

        [Test]
        public async Task GetPageContentAsync_ReturnsMergedContent_WhenOverrideExists()
        {
            var page = new Page
            {
                Id = Guid.NewGuid(),
                Route = "home",
                CurrentVersion = new PageVersion
                {
                    ContentJson = new PageContent
                    {
                        Blocks = { CreateTextBlock("hi") }
                    }
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
        public async Task GetDraftContentAsync_FallsBackToPublished_WhenNoDraftExists()
        {
            var page = new Page
            {
                Id = Guid.NewGuid(),
                Route = "home",
                CurrentVersion = new PageVersion
                {
                    Id = Guid.NewGuid(),
                    Status = PageVersionStatus.Published,
                    ContentJson = new PageContent { Blocks = { CreateTextBlock("live") } }
                },
                Versions = new List<PageVersion>()
            };
            _repo.Setup(r => r.GetPageWithVersionsAsync("home", It.IsAny<CancellationToken>()))
                .ReturnsAsync(page);

            var result = await _service.GetDraftContentAsync("home");

            result.IsSuccess.Should().BeTrue();
            result.Value!.Blocks.First().Content.GetString().Should().Be("live");
        }

        [Test]
        public async Task PublishDraftAsync_PersistsPublishedThenRemovesDraft()
        {
            var route = "home";
            var draftId = Guid.NewGuid();
            var pageId = Guid.NewGuid();
            var draft = new PageVersion
            {
                Id = draftId,
                PageId = pageId,
                ContentJson = new PageContent { Blocks = { CreateTextBlock("draft") } },
                Status = PageVersionStatus.Draft,
                VersionNo = 1
            };
            var page = new Page
            {
                Id = pageId,
                Route = route,
                DraftVersionId = draftId,
                Versions = new List<PageVersion> { draft }
            };
            _repo.Setup(r => r.GetPageWithVersionsAsync(route, It.IsAny<CancellationToken>()))
                .ReturnsAsync(page);
            _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await _service.PublishDraftAsync(route);

            result.IsSuccess.Should().BeTrue();
            _repo.Verify(r => r.AddPageVersion(It.IsAny<PageVersion>()), Times.Never);
            _repo.Verify(r => r.RemovePageVersion(It.IsAny<PageVersion>()), Times.Never);
            page.DraftVersionId.Should().BeNull();
            page.CurrentVersionId.Should().Be(draftId);
            draft.Status.Should().Be(PageVersionStatus.Published);
        }

        [Test]
        public async Task SaveOverrideAsync_Inserts_WhenNoExisting()
        {
            var model = new PageOverride { PageId = Guid.NewGuid() };
            _repo.Setup(r => r.GetPageOverrideByPageIdAsync(model.PageId, It.IsAny<CancellationToken>())).ReturnsAsync((PageOverride?)null);
            _repo.Setup(r => r.GetPageByIdAsync(model.PageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Page { Id = model.PageId, Route = "home", DefaultContent = new PageContent() });
            _imageUsageRepo.Setup(r => r.DeletePageUsagesAsync("home", "Override", It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _imageUsageRepo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var result = await _service.SaveOverrideAsync(model);

            result.IsSuccess.Should().BeTrue();
            _repo.Verify(r => r.AddPageOverride(model), Times.Once);
            _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            model.UpdatedAt.Should().NotBe(default);
        }

        [Test]
        public async Task GetPublishedVersionsAsync_ReturnsSummaries()
        {
            var pageId = Guid.NewGuid();
            var page = new Page { Id = pageId, Route = "home" };
            _repo.Setup(r => r.GetPageWithVersionsAsync("home", It.IsAny<CancellationToken>()))
                .ReturnsAsync(page);
            _repo.Setup(r => r.GetPublishedVersionsAsync(pageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PageVersion>
                {
                    new PageVersion { Id = Guid.NewGuid(), PageId = pageId, VersionNo = 2, Status = PageVersionStatus.Published, PublishedUtc = DateTime.UtcNow }
                });

            var result = await _service.GetPublishedVersionsAsync("home");

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().HaveCount(1);
            result.Value!.First().VersionNo.Should().Be(2);
        }

        [Test]
        public async Task GetPublishedVersionContentAsync_ReturnsValidation_WhenNotPublished()
        {
            var pageId = Guid.NewGuid();
            var versionId = Guid.NewGuid();
            var page = new Page { Id = pageId, Route = "home" };
            var version = new PageVersion
            {
                Id = versionId,
                PageId = pageId,
                Status = PageVersionStatus.Draft,
                ContentJson = new PageContent()
            };
            _repo.Setup(r => r.GetPageWithVersionsAsync("home", It.IsAny<CancellationToken>()))
                .ReturnsAsync(page);
            _repo.Setup(r => r.GetVersionByIdAsync(versionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(version);

            var result = await _service.GetPublishedVersionContentAsync("home", versionId);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorCode.Validation);
        }

        [Test]
        public async Task RollbackToVersionAsync_Succeeds_WhenPublishedVersion()
        {
            var pageId = Guid.NewGuid();
            var versionId = Guid.NewGuid();
            var page = new Page { Id = pageId, Route = "home" };
            var version = new PageVersion
            {
                Id = versionId,
                PageId = pageId,
                Status = PageVersionStatus.Published,
                ContentJson = new PageContent()
            };
            _repo.Setup(r => r.GetPageWithVersionsAsync("home", It.IsAny<CancellationToken>()))
                .ReturnsAsync(page);
            _repo.Setup(r => r.GetVersionByIdAsync(versionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(version);
            _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await _service.RollbackToVersionAsync("home", versionId);

            result.IsSuccess.Should().BeTrue();
            page.CurrentVersionId.Should().Be(versionId);
            _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
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
            _repo.Setup(r => r.GetPageByIdAsync(existing.PageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Page { Id = existing.PageId, Route = "home", DefaultContent = new PageContent() });
            _imageUsageRepo.Setup(r => r.DeletePageUsagesAsync("home", "Override", It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _imageUsageRepo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
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

        private static PageBlock CreateImageBlock(Guid imageId)
        {
            return new PageBlock
            {
                Type = "image",
                Content = JsonDocument.Parse($"{{\"imageId\":\"{imageId}\"}}").RootElement
            };
        }

        private static PageBlock CreatePeopleBlock(string json)
        {
            return new PageBlock
            {
                Type = "people",
                Content = JsonDocument.Parse(json).RootElement
            };
        }

        [Test]
        public async Task GetPageContentAsync_HydratesImageBlocks_WhenImageIdPresent()
        {
            var imageId = Guid.NewGuid();
            var page = new Page
            {
                Id = Guid.NewGuid(),
                Route = "home",
                CurrentVersion = new PageVersion
                {
                    ContentJson = new PageContent
                    {
                        Blocks = { CreateImageBlock(imageId) }
                    }
                }
            };
            var image = new Image
            {
                Id = imageId,
                StorageKey = "images/hero.jpg",
                PublicUrl = "https://cdn.example.com/hero.jpg"
            };
            _repo.Setup(r => r.GetPageWithOverridesAsync("home", It.IsAny<CancellationToken>())).ReturnsAsync(page);
            _imageRepo.Setup(r => r.GetActiveByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<Guid, Image> { [imageId] = image });

            var result = await _service.GetPageContentAsync("home");

            result.IsSuccess.Should().BeTrue();
            var block = result.Value!.Blocks.Single();
            block.Content.GetProperty("url").GetString().Should().Be(image.PublicUrl);
        }

        [Test]
        public async Task GetPageContentAsync_OverrideWins_AndHydrates()
        {
            var imageId = Guid.NewGuid();
            var page = new Page
            {
                Id = Guid.NewGuid(),
                Route = "home",
                CurrentVersion = new PageVersion
                {
                    ContentJson = new PageContent
                    {
                        Blocks = { CreateTextBlock("default") }
                    }
                },
                Overrides = new[]
                {
                    new PageOverride
                    {
                        OverrideContent = new PageContent
                        {
                            Blocks = { CreateImageBlock(imageId) }
                        },
                        UpdatedAt = DateTime.UtcNow
                    }
                }
            };
            _repo.Setup(r => r.GetPageWithOverridesAsync("home", It.IsAny<CancellationToken>())).ReturnsAsync(page);
            _imageRepo.Setup(r => r.GetActiveByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<Guid, Image>
                {
                    [imageId] = new Image { Id = imageId, StorageKey = "images/override.jpg", PublicUrl = "https://cdn.example.com/override.jpg" }
                });

            var result = await _service.GetPageContentAsync("home");

            result.IsSuccess.Should().BeTrue();
            result.Value!.Blocks.Single().Content.GetProperty("url").GetString().Should().Be("https://cdn.example.com/override.jpg");
        }

        [Test]
        public async Task AutosaveDraftAsync_CreatesDraft_WhenMissing()
        {
            var pageId = Guid.NewGuid();
            var page = new Page
            {
                Id = pageId,
                Route = "home",
                Versions = new List<PageVersion>
                {
                    new PageVersion
                    {
                        Id = Guid.NewGuid(),
                        PageId = pageId,
                        VersionNo = 1,
                        Status = PageVersionStatus.Published,
                        ContentJson = new PageContent
                        {
                            Blocks = { CreateTextBlock("published") }
                        }
                    }
                }
            };
            _repo.Setup(r => r.GetPageWithVersionsAsync("home", It.IsAny<CancellationToken>()))
                .ReturnsAsync(page);
            _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var content = new PageContent
            {
                Blocks = { CreateTextBlock("draft") }
            };

            var result = await _service.AutosaveDraftAsync("home", content);

            result.IsSuccess.Should().BeTrue();
            page.DraftVersionId.Should().NotBeNull();
            var draft = page.Versions.Single(v => v.Id == page.DraftVersionId);
            draft.Status.Should().Be(PageVersionStatus.Draft);
            draft.VersionNo.Should().Be(2);
            draft.ContentJson.Blocks.Single().Content.GetString().Should().Be("draft");
            draft.UpdatedUtc.Should().NotBeNull();
            _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task AutosaveDraftAsync_UpdatesDraft_WhenExists()
        {
            var pageId = Guid.NewGuid();
            var draftId = Guid.NewGuid();
            var draft = new PageVersion
            {
                Id = draftId,
                PageId = pageId,
                VersionNo = 2,
                Status = PageVersionStatus.Draft,
                UpdatedUtc = DateTime.UtcNow.AddMinutes(-10),
                ContentJson = new PageContent
                {
                    Blocks = { CreateTextBlock("old") }
                }
            };
            var page = new Page
            {
                Id = pageId,
                Route = "home",
                DraftVersionId = draftId,
                Versions = new List<PageVersion>
                {
                    new PageVersion
                    {
                        Id = Guid.NewGuid(),
                        PageId = pageId,
                        VersionNo = 1,
                        Status = PageVersionStatus.Published,
                        ContentJson = new PageContent
                        {
                            Blocks = { CreateTextBlock("published") }
                        }
                    },
                    draft
                }
            };
            _repo.Setup(r => r.GetPageWithVersionsAsync("home", It.IsAny<CancellationToken>()))
                .ReturnsAsync(page);
            _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var content = new PageContent
            {
                Blocks = { CreateTextBlock("new") }
            };
            var previousUpdated = draft.UpdatedUtc;

            var result = await _service.AutosaveDraftAsync("home", content);

            result.IsSuccess.Should().BeTrue();
            draft.ContentJson.Blocks.Single().Content.GetString().Should().Be("new");
            draft.UpdatedUtc.Should().NotBeNull();
            draft.UpdatedUtc.Should().NotBe(previousUpdated);
            _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task AutosaveDraftAsync_ReturnsConflict_WhenDraftIsNotDraft()
        {
            var pageId = Guid.NewGuid();
            var versionId = Guid.NewGuid();
            var page = new Page
            {
                Id = pageId,
                Route = "home",
                DraftVersionId = versionId,
                Versions = new List<PageVersion>
                {
                    new PageVersion
                    {
                        Id = versionId,
                        PageId = pageId,
                        VersionNo = 1,
                        Status = PageVersionStatus.Published,
                        ContentJson = new PageContent()
                    }
                }
            };
            _repo.Setup(r => r.GetPageWithVersionsAsync("home", It.IsAny<CancellationToken>()))
                .ReturnsAsync(page);

            var content = new PageContent
            {
                Blocks = { CreateTextBlock("draft") }
            };

            var result = await _service.AutosaveDraftAsync("home", content);

            result.IsFailure.Should().BeTrue();
            result.ErrorCode.Should().Be(ErrorCode.Conflict);
            _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task AutosaveDraftAsync_ReturnsValidation_WhenAboutPeopleInvalid()
        {
            var pageId = Guid.NewGuid();
            var page = new Page
            {
                Id = pageId,
                Route = "about",
                Versions = new List<PageVersion>
                {
                    new PageVersion
                    {
                        Id = Guid.NewGuid(),
                        PageId = pageId,
                        VersionNo = 1,
                        Status = PageVersionStatus.Published,
                        ContentJson = new PageContent
                        {
                            Blocks = { CreateTextBlock("published") }
                        }
                    }
                }
            };
            _repo.Setup(r => r.GetPageWithVersionsAsync("about", It.IsAny<CancellationToken>()))
                .ReturnsAsync(page);

            var content = new PageContent
            {
                Blocks =
                {
                    CreatePeopleBlock("[{\"name\":\"\",\"text\":\"Some blurb\",\"sortOrder\":1}]")
                }
            };

            var result = await _service.AutosaveDraftAsync("about", content);

            result.IsFailure.Should().BeTrue();
            result.ErrorCode.Should().Be(ErrorCode.Validation);
            result.ValidationErrors.Should().NotBeEmpty();
            _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
