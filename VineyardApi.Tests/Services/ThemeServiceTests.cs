using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using VineyardApi.Models;
using VineyardApi.Repositories;
using VineyardApi.Services;

namespace VineyardApi.Tests.Services
{
    public class ThemeServiceTests
    {
        private Mock<IThemeRepository> _repo = null!;
        private ThemeService _service = null!;

        [SetUp]
        public void Setup()
        {
            _repo = new Mock<IThemeRepository>();
            _service = new ThemeService(_repo.Object, NullLogger<ThemeService>.Instance);
        }

        [Test]
        public async Task GetThemeAsync_ReturnsDefaults_WhenNoOverrides()
        {
            var defaults = new List<ThemeDefault>
            {
                new ThemeDefault { Id = 1, Key = "primary", Value = "#fff" },
                new ThemeDefault { Id = 2, Key = "secondary", Value = "#000" }
            };
            _repo.Setup(r => r.GetDefaultsAsync()).ReturnsAsync(defaults);
            _repo.Setup(r => r.GetOverridesAsync()).ReturnsAsync(new List<ThemeOverride>());

            var result = await _service.GetThemeAsync();

            result.Should().ContainKey("primary");
            result["primary"].Should().Be("#fff");

            result.Should().ContainKey("secondary");
            result["secondary"].Should().Be("#000");
        }


        [Test]
        public async Task GetThemeAsync_MergesOverrides_WhenPresent()
        {
            var defaults = new List<ThemeDefault>
            {
                new ThemeDefault { Id = 1, Key = "primary", Value = "#fff" }
            };
            var overrides = new List<ThemeOverride>
            {
                new ThemeOverride { ThemeDefaultId = 1, Value = "#111", UpdatedAt = DateTime.UtcNow }
            };
            _repo.Setup(r => r.GetDefaultsAsync()).ReturnsAsync(defaults);
            _repo.Setup(r => r.GetOverridesAsync()).ReturnsAsync(overrides);

            var result = await _service.GetThemeAsync();

            result["primary"].Should().Be("#111");
        }

        [Test]
        public async Task SaveOverrideAsync_Adds_WhenNoExisting()
        {
            var model = new ThemeOverride { ThemeDefaultId = 1 };
            _repo.Setup(r => r.GetOverrideAsync(1)).ReturnsAsync((ThemeOverride?)null);

            await _service.SaveOverrideAsync(model);

            _repo.Verify(r => r.AddThemeOverride(model), Times.Once);
            _repo.Verify(r => r.SaveChangesAsync(), Times.Once);
            model.UpdatedAt.Should().NotBe(default);
        }

        [Test]
        public async Task SaveOverrideAsync_Updates_WhenExisting()
        {
            var existing = new ThemeOverride { ThemeDefaultId = 1, Value = "old" };
            var model = new ThemeOverride { ThemeDefaultId = 1, Value = "new", UpdatedById = Guid.NewGuid() };
            _repo.Setup(r => r.GetOverrideAsync(1)).ReturnsAsync(existing);

            await _service.SaveOverrideAsync(model);

            existing.Value.Should().Be("new");
            existing.UpdatedById.Should().Be(model.UpdatedById);
            _repo.Verify(r => r.AddThemeOverride(It.IsAny<ThemeOverride>()), Times.Never);
            _repo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
    }
}
