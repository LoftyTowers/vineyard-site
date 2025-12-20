using System;
using FluentValidation;
using VineyardApi.Models;

namespace VineyardApi.Validators;

public class ImageValidator : AbstractValidator<Image>
{
    public ImageValidator()
    {
        RuleFor(x => x.StorageKey)
            .NotEmpty()
            .MaximumLength(500);
        RuleFor(x => x.PublicUrl)
            .NotEmpty()
            .MaximumLength(2048)
            .Must(BeValidUri)
            .WithMessage("PublicUrl must be a valid absolute URI.");
        RuleFor(x => x.AltText).MaximumLength(255).When(x => x.AltText != null);
        RuleFor(x => x.Caption).MaximumLength(500).When(x => x.Caption != null);
    }

    private static bool BeValidUri(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}
