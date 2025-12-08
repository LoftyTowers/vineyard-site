using System;
using FluentValidation;
using VineyardApi.Domain.Content;
using VineyardApi.Models;

namespace VineyardApi.Validators;

public class PageOverrideValidator : AbstractValidator<PageOverride>
{
    public PageOverrideValidator()
    {
        RuleFor(x => x.PageId).NotEmpty();
        RuleFor(x => x.OverrideContent).NotNull().SetValidator(new PageContentValidator());
        RuleFor(x => x.UpdatedById).NotEmpty();
    }
}

public class PageContentValidator : AbstractValidator<PageContent>
{
    public PageContentValidator()
    {
        RuleFor(x => x.Blocks).NotNull();
        RuleForEach(x => x.Blocks).SetValidator(new ContentBlockValidator());
    }
}

public class ContentBlockValidator : AbstractValidator<ContentBlock>
{
    public ContentBlockValidator()
    {
        RuleFor(x => x.Type).NotEmpty();

        When(x => x is RichTextBlock, () =>
        {
            RuleFor(x => ((RichTextBlock)x).Html)
                .NotEmpty()
                .MaximumLength(20000);
        });

        When(x => x is ImageBlock, () =>
        {
            RuleFor(x => ((ImageBlock)x).Url)
                .NotEmpty()
                .Must(BeValidUri)
                .WithMessage("Url must be a valid absolute URI.");
            RuleFor(x => ((ImageBlock)x).Alt)
                .NotEmpty()
                .MaximumLength(255);
        });
    }

    private static bool BeValidUri(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}
