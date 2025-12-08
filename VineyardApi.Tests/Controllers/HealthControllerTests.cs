using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using VineyardApi.Controllers;
using VineyardApi.Tests;

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

        [Test]
        public void Get_MapsToStatusCode200()
        {
            var result = _controller.Get();

            ResultHttpMapper.MapToStatusCode(result).Should().Be(StatusCodes.Status200OK);
        }
    }
}
