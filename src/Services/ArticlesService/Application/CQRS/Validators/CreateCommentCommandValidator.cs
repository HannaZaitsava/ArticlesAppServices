using ArticlesService.Application.CQRS.Commands.CommentCommands.CreateComment;
using ArticlesService.Domain.Constants.EntityConstraints;
using FluentValidation;

namespace ArticlesService.Application.CQRS.Validators
{    
    public sealed class CreateCommentCommandValidator : AbstractValidator<CreateCommentCommand>
    {
        public CreateCommentCommandValidator()
        {
            RuleFor(x => x.ArticleId)
              .NotEmpty().WithMessage("Article Id is required");

            RuleFor(x => x.Text)
               .NotEmpty().WithMessage("Text is required")
               .Length(CommentConstraints.MinTextLength, CommentConstraints.MaxTextLength)
               .WithMessage("Text must be between {MinLength} and {MaxLength} characters");           
        }
    }
}
