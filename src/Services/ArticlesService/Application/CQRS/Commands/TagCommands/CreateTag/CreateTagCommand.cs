using ArticlesService.Application.DTOs.Tags;
using ArticlesService.Domain.Result;
using MediatR;

namespace ArticlesService.Application.CQRS.Commands.TagCommands.CreateTag
{
    public record CreateTagCommand(string Label, string? Color) : IRequest<Result<TagResponseDTO>>;
}
