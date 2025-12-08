using FluentValidation;
using VineyardApi.Controllers;
using VineyardApi.Models;

namespace VineyardApi.Validation
{
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Username).NotEmpty();
            RuleFor(x => x.Password).NotEmpty();
        }
    }

    public class PageOverrideValidator : AbstractValidator<PageOverride>
    {
        public PageOverrideValidator()
        {
            RuleFor(x => x.PageId).NotEmpty();
            RuleFor(x => x.OverrideContent).NotNull();
            RuleFor(x => x.UpdatedById).NotEmpty();
        }
    }

    public class ImageValidator : AbstractValidator<Image>
    {
        public ImageValidator()
        {
            RuleFor(x => x.Url).NotEmpty();
            RuleFor(x => x.CreatedAt).NotEqual(default(DateTime));
        }
    }

    public class ContentOverrideValidator : AbstractValidator<ContentOverride>
    {
        public ContentOverrideValidator()
        {
            RuleFor(x => x.PageId).NotEmpty();
            RuleFor(x => x.BlockKey).NotEmpty().MaximumLength(255);
            RuleFor(x => x.HtmlValue).NotEmpty();
            RuleFor(x => x.Status).NotEmpty();
            RuleFor(x => x.ChangedById).NotEmpty();
        }
    }

    public class ThemeOverrideValidator : AbstractValidator<ThemeOverride>
    {
        public ThemeOverrideValidator()
        {
            RuleFor(x => x.ThemeDefaultId).GreaterThan(0);
            RuleFor(x => x.Value).NotEmpty();
            RuleFor(x => x.UpdatedById).NotEmpty();
        }
    }

    public class IdRequestValidator : AbstractValidator<IdRequest>
    {
        public IdRequestValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }

    public class RevertRequestValidator : AbstractValidator<RevertRequest>
    {
        public RevertRequestValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.ChangedById).NotEmpty();
        }
    }
}
