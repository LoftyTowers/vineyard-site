using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using LayeredMicroservice.Domain;
using LayeredMicroservice.Shared;
using Microsoft.Extensions.Logging;

namespace LayeredMicroservice.Application;

public interface ICreateOrderHandler
{
    Task<Result<Order>> HandleAsync(CreateOrderCommand command, CancellationToken cancellationToken);
}

public sealed class CreateOrderHandler : ICreateOrderHandler
{
    private readonly IValidator<CreateOrderCommand> _validator;
    private readonly IOrderRepository _repository;
    private readonly ILogger<CreateOrderHandler> _logger;
    private readonly IClock _clock;

    public CreateOrderHandler(
        IValidator<CreateOrderCommand> validator,
        IOrderRepository repository,
        ILogger<CreateOrderHandler> logger,
        IClock clock)
    {
        _validator = validator;
        _repository = repository;
        _logger = logger;
        _clock = clock;
    }

    public async Task<Result<Order>> HandleAsync(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object?>
        {
            ["CorrelationId"] = command.CorrelationId,
            ["CustomerId"] = command.CustomerId
        });

        var validation = await _validator.ValidateAsync(command, cancellationToken);
        if (!validation.IsValid)
        {
            return Result<Order>.Failure(ErrorCode.Validation, validation.Errors.Select(e => e.ErrorMessage));
        }

        try
        {
            var order = Order.Create(
                command.CustomerId,
                command.Lines.Select(line => new OrderLineDraft(line.Sku, line.Quantity)),
                _clock.UtcNow);

            using var orderScope = _logger.BeginScope(new { command.CorrelationId, OrderId = order.Id });

            var persistence = await _repository.SaveAsync(order, cancellationToken);
            if (!persistence.IsSuccess)
            {
                _logger.LogError("Failed to persist order {OrderId}: {Errors}", order.Id, string.Join("; ", persistence.Errors));
                return Result<Order>.Failure(persistence.Code ?? ErrorCode.Unexpected, persistence.Errors);
            }

            return Result<Order>.Success(order);
        }
        catch (DomainRuleException ex)
        {
            _logger.LogInformation(ex, "Domain rule violation when creating order for {CustomerId}", command.CustomerId);
            return Result<Order>.Failure(ErrorCode.Domain, new[] { ex.Message });
        }
        catch (OperationCanceledException)
        {
            return Result<Order>.Failure(ErrorCode.Cancelled, Array.Empty<string>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected failure when creating order for {CustomerId}", command.CustomerId);
            return Result<Order>.Failure(ErrorCode.Unexpected, new[] { "Unexpected failure" });
        }
    }
}

public sealed record CreateOrderCommand(Guid CustomerId, IReadOnlyCollection<CreateOrderLine> Lines, string CorrelationId);

public sealed record CreateOrderLine(string Sku, int Quantity);

public interface IOrderRepository
{
    Task<Result> SaveAsync(Order order, CancellationToken cancellationToken);
}
