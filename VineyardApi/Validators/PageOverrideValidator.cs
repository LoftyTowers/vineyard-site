using System;
using System.Text.Json;
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

public class PageContentValidator : AbstractValidator<PageContent?>
{
    public PageContentValidator()
    {
        RuleFor(x => x)
            .NotNull();

        RuleFor(x => x!.Blocks)
            .NotNull();

        RuleForEach(x => x!.Blocks)
            .SetValidator(new PageBlockValidator());
    }
}

public class PageBlockValidator : AbstractValidator<PageBlock>
{
    public PageBlockValidator()
    {
        RuleFor(x => x.Type).NotEmpty();
        RuleFor(x => x.Content.ValueKind)
            .Must(kind => kind != JsonValueKind.Undefined)
            .WithMessage("Content must be provided for each block.");
    }
}
