using FluentValidation;
using VineyardApi.Models.Requests;

namespace VineyardApi.Validators;

public class RevertRequestValidator : AbstractValidator<RevertRequest>
{
    public RevertRequestValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.ChangedById).NotEmpty();
    }
}
