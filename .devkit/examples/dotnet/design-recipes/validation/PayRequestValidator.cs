// See examples/dotnet/layered-microservice for the canonical layered structure.
namespace DevKit.Examples.ApiEndpoint;

public sealed class PayRequestValidator : AbstractValidator<PayRequest>
{
    public PayRequestValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than zero.");

        RuleFor(x => x.PaymentId)
            .NotEmpty()
            .MaximumLength(64)
            .WithMessage("PaymentId is required and must be <= 64 characters.");
    }
}
