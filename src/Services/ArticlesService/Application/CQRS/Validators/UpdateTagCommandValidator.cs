using ArticlesService.Application.CQRS.Commands.TagCommands.UpdateTag;
using ArticlesService.Domain.Constants.EntityConstraints;
using FluentValidation;

namespace ArticlesService.Application.CQRS.Validators
{
    public sealed class UpdateTagCommandValidator : AbstractValidator<UpdateTagCommand>
    {
        public UpdateTagCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                    .WithMessage("Tag ID is required.");

            RuleFor(x => x.Label)
               .Length(TagConstraints.MinLabelLength, TagConstraints.MaxLabelLength)
                   .WithMessage("Label must be between {MinLength} and {MaxLength} characters long")
                   .When(x => x.Label != null); 

            RuleFor(v => v.Color)
                .Matches(TagConstraints.HexColorRegex)
                    .WithMessage("Color must be a valid HEX string (e.g., #FFFFFF)")
                    .When(v => v.Color != null);
        }
    }
}
