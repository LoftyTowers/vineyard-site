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
    public class ContentOverrideServiceTests
    {
        private Mock<IContentOverrideRepository> _repo = null!;
        private ContentOverrideService _service = null!;

        [SetUp]
        public void Setup()
        {
            _repo = new Mock<IContentOverrideRepository>();
            _service = new ContentOverrideService(_repo.Object);
        }

        [Test]
        public async Task PublishAsync_SetsStatusAndPersists()
        {
            var model = new ContentOverride
            {
                PageId = Guid.NewGuid(),
                BlockKey = "key",
                HtmlValue = "<p></p>",
                Note = "test note",
                ChangedById = Guid.NewGuid()
            };
            _repo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

            await _service.PublishAsync(model);

            model.Status.Should().Be("published");
            _repo.Verify(r => r.Add(model), Times.Once);
            _repo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
    }
}
