using ArticlesService.Application.CQRS.Commands.ArticleCommands.UpdateArticle;
using ArticlesService.Domain.Constants.EntityConstraints;
using FluentValidation;

namespace ArticlesService.Application.CQRS.Validators
{
    public sealed class UpdateArticleCommandValidator : AbstractValidator<UpdateArticleCommand>
    {
        public UpdateArticleCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Article Id is required");            

            RuleFor(x => x.Title)                   
                .Length(ArticleConstraints.TitleMinLength, ArticleConstraints.TitleMaxLength)
                   .WithMessage("Title must be between {MinLength} and {MaxLength} characters")
                   .When(x => x.Title != null); 

            RuleFor(x => x.Content)               
               .Length(ArticleConstraints.ContentMinLength, ArticleConstraints.ContentMaxLength)
                   .WithMessage("Content must be between {MinLength} and {MaxLength} characters")
                   .When(x => x.Content != null); 
        }
    }
}
