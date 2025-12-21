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
        RuleFor(x => x).Custom((block, context) =>
        {
            if (string.Equals(block.Type, "richText", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(block.ContentHtml))
                {
                    context.AddFailure("contentHtml", "ContentHtml must be provided for richText blocks.");
                    return;
                }

                if (block.ContentHtml.Length > 20000)
                {
                    context.AddFailure("contentHtml", "ContentHtml exceeds the maximum length.");
                }

                return;
            }

            if (block.Content.ValueKind == JsonValueKind.Undefined)
            {
                context.AddFailure("content", "Content must be provided for each block.");
            }
        });
    }
}
