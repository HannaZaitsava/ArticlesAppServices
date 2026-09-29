using ArticlesService.Application.CQRS.Commands.ArticleCategoryCommands.CreateArticleCategory;
using ArticlesService.Domain.Constants.EntityConstraints;
using FluentValidation;

namespace ArticlesService.Application.CQRS.Validators
{
    public sealed class CreateArticleCategoryCommandValidator : AbstractValidator<CreateArticleCategoryCommand>
    {
        public CreateArticleCategoryCommandValidator()
        {
            RuleFor(x => x.Name)
               .NotEmpty().WithMessage("Name is required")               
               .Length(ArticleCategoryConstraints.NameMinLength, ArticleCategoryConstraints.NameMaxLength)
               // Вариант с плейсхолдерами {MinLength} и {MaxLength} — работает чуть быстрее,
               // так как строка форматируется библиотекой FluentValidation только в момент возникновения ошибки.
                   .WithMessage("Name must be between {MinLength} and {MaxLength} characters");
        }
    }
}
