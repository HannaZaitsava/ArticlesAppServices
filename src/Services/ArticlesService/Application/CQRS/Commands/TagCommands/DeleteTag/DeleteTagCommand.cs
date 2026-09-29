using ArticlesService.Domain.Result;
using MediatR;

namespace ArticlesService.Application.CQRS.Commands.TagCommands.DeleteTag
{
    public sealed record DeleteTagCommand(Guid Id) : IRequest<Result<bool>>;
}
