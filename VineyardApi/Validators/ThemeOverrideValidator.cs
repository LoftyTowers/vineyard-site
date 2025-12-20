using FluentValidation;
using VineyardApi.Models;

namespace VineyardApi.Validators;

public class ThemeOverrideValidator : AbstractValidator<ThemeOverride>
{
    public ThemeOverrideValidator()
    {
        RuleFor(x => x.ThemeDefaultId).GreaterThan(0);
        RuleFor(x => x.Value).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.UpdatedById).NotEmpty();
    }
}
