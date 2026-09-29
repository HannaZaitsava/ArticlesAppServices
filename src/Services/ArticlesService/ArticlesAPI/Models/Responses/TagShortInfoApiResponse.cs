namespace ArticlesService.ArticlesAPI.Models.Responses
{
    public sealed record TagShortInfoApiResponse(Guid Id, string? Color, string Label = default!);
}
