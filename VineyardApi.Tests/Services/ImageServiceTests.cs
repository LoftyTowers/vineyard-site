using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
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
            _service = new ImageService(_repo.Object);
        }

        [Test]
        public async Task SaveImageAsync_SetsIdAndCreatedAt_AndPersists()
        {
            var image = new Image { Url = "test" };
            _repo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

            var saved = await _service.SaveImageAsync(image);

            saved.Id.Should().NotBeEmpty();
            saved.CreatedAt.Should().NotBe(default);
            _repo.Verify(r => r.AddImage(image), Times.Once);
            _repo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
    }
}
