using FluentValidation;
using VineyardApi.Models.Requests;

namespace VineyardApi.Validators;

public class IdRequestValidator : AbstractValidator<IdRequest>
{
    public IdRequestValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
