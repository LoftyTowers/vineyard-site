using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using VineyardApi.Controllers;
using VineyardApi.Infrastructure;
using VineyardApi.Models;
using VineyardApi.Services;

namespace VineyardApi.Tests.Controllers
{
    public class AuditControllerTests
    {
        private Mock<IAuditService> _service = null!;
        private AuditController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _service = new Mock<IAuditService>();
            _controller = new AuditController(_service.Object);
        }

        [Test]
        public async Task GetRecent_ReturnsOk()
        {
            _service.Setup(s => s.GetRecentAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<List<AuditLog>>.Success(new List<AuditLog>()));

            var result = await _controller.GetRecent(CancellationToken.None);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Test]
        public void Controller_HasAuthorizeAttribute()
        {
            var attr = typeof(AuditController)
                .GetCustomAttributes(typeof(AuthorizeAttribute), false)
                .Cast<AuthorizeAttribute>()
                .SingleOrDefault();

            attr.Should().NotBeNull();
            attr!.Roles.Should().Be("Admin");
        }
    }
}
