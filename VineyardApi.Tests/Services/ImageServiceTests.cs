using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using VineyardApi.Models;
using VineyardApi.Repositories;
using VineyardApi.Services;

namespace VineyardApi.Tests.Services
{
    public class ImageServiceTests
    {
        private Mock<IImageRepository> _repo = null!;
        private ImageService _service = null!;

        [SetUp]
        public void Setup()
        {
            _repo = new Mock<IImageRepository>();
            _service = new ImageService(_repo.Object, NullLogger<ImageService>.Instance);
        }

        [Test]
        public async Task SaveImageAsync_SetsIdAndCreatedUtc_AndPersists()
        {
            var image = new Image { StorageKey = "images/test.jpg", PublicUrl = "https://cdn.example.com/test.jpg" };
            _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var saved = await _service.SaveImageAsync(image);

            saved.IsSuccess.Should().BeTrue();
            saved.Value!.Id.Should().NotBeEmpty();
            saved.Value.CreatedUtc.Should().NotBe(default);
            _repo.Verify(r => r.AddImage(image), Times.Once);
            _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
