using ArticlesService.Domain.Enums;

namespace ArticlesService.Domain.Errors
{
    public sealed record Error(string Name, string Message, ErrorType Type = ErrorType.Failure)
    {
        public static readonly Error None = new(string.Empty, string.Empty);
    }
}
