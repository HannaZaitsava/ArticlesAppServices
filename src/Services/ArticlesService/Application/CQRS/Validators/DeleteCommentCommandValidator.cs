using ArticlesService.Application.CQRS.Commands.CommentCommands.DeleteComment;
using FluentValidation;

namespace ArticlesService.Application.CQRS.Validators
{    
    public sealed class DeleteCommentCommandValidator : AbstractValidator<DeleteCommentCommand>
    {
        public DeleteCommentCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Comment ID is required.");
        }
    }
}
