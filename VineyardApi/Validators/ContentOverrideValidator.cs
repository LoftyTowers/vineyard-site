using System.Collections.Generic;
using FluentValidation;
using VineyardApi.Models;

namespace VineyardApi.Validators;

public class ContentOverrideValidator : AbstractValidator<ContentOverride>
{
    private static readonly HashSet<string> AllowedStatuses = ["draft", "published"];

    public ContentOverrideValidator()
    {
        RuleFor(x => x.PageId).NotEmpty();
        RuleFor(x => x.BlockKey).NotEmpty().MaximumLength(255);
        RuleFor(x => x.HtmlValue).NotEmpty();
        RuleFor(x => x.Status)
            .Must(status => string.IsNullOrEmpty(status) || AllowedStatuses.Contains(status.ToLowerInvariant()))
            .WithMessage("Status must be 'draft' or 'published' when provided.");
        RuleFor(x => x.Note).MaximumLength(1000).When(x => x.Note != null);
        RuleFor(x => x.ChangedById).NotEmpty();
    }
}
