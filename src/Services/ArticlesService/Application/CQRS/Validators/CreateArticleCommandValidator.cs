using ArticlesService.Application.CQRS.Commands.ArticleCommands.CreateArticle;
using ArticlesService.Domain.Constants.EntityConstraints;
using FluentValidation;

namespace ArticlesService.Application.CQRS.Validators
{
    public sealed class CreateArticleCommandValidator : AbstractValidator<CreateArticleCommand>
    {
        public CreateArticleCommandValidator()
        {
            RuleFor(x => x.Title)
               .NotEmpty().WithMessage("Title is required")
               .Length(ArticleConstraints.TitleMinLength, ArticleConstraints.TitleMaxLength)
               .WithMessage("Title must be between {MinLength} and {MaxLength} characters");

            RuleFor(x => x.Content)
               .NotEmpty().WithMessage("Content is required")
               .Length(ArticleConstraints.ContentMinLength, ArticleConstraints.ContentMaxLength)
               .WithMessage("Content must be between {MinLength} and {MaxLength} characters");
        }
    }
}
