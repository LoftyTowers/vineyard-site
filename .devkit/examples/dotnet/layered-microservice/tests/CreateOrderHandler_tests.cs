using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using LayeredMicroservice.Application;
using LayeredMicroservice.Domain;
using LayeredMicroservice.Shared;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace LayeredMicroservice.Tests;

public sealed class CreateOrderHandlerTests
{
    private readonly Mock<IValidator<CreateOrderCommand>> _validator = new();
    private readonly Mock<IOrderRepository> _repository = new();
    private readonly Mock<ILogger<CreateOrderHandler>> _logger = new();
    private readonly Mock<IClock> _clock = new();

    [SetUp]
    public void SetUp()
    {
        _clock.Setup(c => c.UtcNow).Returns(DateTimeOffset.UtcNow);
    }

    [Test]
    public async Task HandleAsync_WhenValid_SavesOrder()
    {
        var command = new CreateOrderCommand(Guid.NewGuid(), new[] { new CreateOrderLine("SKU", 1) }, "corr-1");
        _validator.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _repository.Setup(r => r.SaveAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var handler = new CreateOrderHandler(_validator.Object, _repository.Object, _logger.Object, _clock.Object);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        _repository.Verify(r => r.SaveAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task HandleAsync_WhenDomainRuleFails_ReturnsDomainError()
    {
        var command = new CreateOrderCommand(Guid.NewGuid(), Array.Empty<CreateOrderLine>(), "corr-2");
        _validator.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var handler = new CreateOrderHandler(_validator.Object, _repository.Object, _logger.Object, _clock.Object);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Code, Is.EqualTo(ErrorCode.Domain));
    }
}
