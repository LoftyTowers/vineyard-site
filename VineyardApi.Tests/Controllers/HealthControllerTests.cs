using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using VineyardApi.Controllers;

namespace VineyardApi.Tests.Controllers
{
    public class HealthControllerTests
    {
        private HealthController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _controller = new HealthController();
        }

        [Test]
        public void Get_ReturnsOk()
        {
            var result = _controller.Get();

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
