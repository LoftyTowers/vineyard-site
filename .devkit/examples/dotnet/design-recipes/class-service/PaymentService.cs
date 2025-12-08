// NOTE: Canonical ErrorCode/Result/ResultExtensions live in examples/dotnet/layered-microservice/shared/.
// For real code, import those instead of re-defining types.
// See examples/dotnet/layered-microservice for the canonical layered structure.
// using layered shared primitives from: examples/dotnet/layered-microservice/shared
using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using LayeredMicroservice.Shared;
using Microsoft.Extensions.Logging;

namespace DevKit.Examples.ClassService;

public interface IPaymentGateway
{
    Task<Result<PaymentReceipt>> PayAsync(Guid orderId, Guid paymentId, decimal amount, CancellationToken ct);
}

public interface IClock { DateTime UtcNow { get; } }

public sealed record ProcessPaymentCommand(Guid OrderId, Guid PaymentId, decimal Amount);
public sealed record PaymentReceipt(Guid OrderId, Guid PaymentId, decimal Amount, DateTime PaidAtUtc);

public sealed class ProcessPaymentValidator : AbstractValidator<ProcessPaymentCommand>
{
    public ProcessPaymentValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.PaymentId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}

public sealed class PaymentService
{
    private readonly ILogger<PaymentService> _log;
    private readonly IPaymentGateway _gateway;
    private readonly IValidator<ProcessPaymentCommand> _validator;
    private readonly IClock _clock;

    public PaymentService(
        ILogger<PaymentService> log,
        IPaymentGateway gateway,
        IValidator<ProcessPaymentCommand> validator,
        IClock clock)
    {
        _log = log;
        _gateway = gateway;
        _validator = validator;
        _clock = clock;
    }

    public async Task<Result<PaymentReceipt>> ProcessAsync(ProcessPaymentCommand cmd, CancellationToken ct)
    {
        var validation = await _validator.ValidateAsync(cmd, ct);
        if (!validation.IsValid)
            return Result<PaymentReceipt>.Failure(ErrorCode.Validation, validation.ErrorsToMessages());

        using var _ = _log.BeginScope(new { cmd.OrderId, cmd.PaymentId, CorrelationId = Guid.NewGuid() });

        try
        {
            var paid = await _gateway.PayAsync(cmd.OrderId, cmd.PaymentId, cmd.Amount, ct);
            if (!paid.IsSuccess) return Result<PaymentReceipt>.Failure(paid.Code ?? ErrorCode.Unexpected, paid.Errors);

            var receipt = new PaymentReceipt(cmd.OrderId, cmd.PaymentId, cmd.Amount, _clock.UtcNow);
            return Result<PaymentReceipt>.Success(receipt);
        }
        catch (OperationCanceledException)
        {
            _log.LogInformation("Payment cancelled for {OrderId}", cmd.OrderId);
            return Result<PaymentReceipt>.Failure(ErrorCode.Cancelled, new[] { "Operation cancelled." });
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Unexpected error processing payment {OrderId}", cmd.OrderId);
            return Result<PaymentReceipt>.Failure(ErrorCode.Unexpected, new[] { "Unexpected error." });
        }
    }
}

internal static class ValidationExtensions
{
    public static string[] ErrorsToMessages(this FluentValidation.Results.ValidationResult v)
        => v.Errors is null ? Array.Empty<string>() : Array.ConvertAll(v.Errors.ToArray(), e => e.ErrorMessage);
}
