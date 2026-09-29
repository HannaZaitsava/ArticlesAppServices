namespace ArticlesService.Infrastructure.Cache

{
    // Внутреннее маркерное исключение, скрытое внутри инфраструктуры кэша
    internal sealed class FactoryException(Exception innerException) : Exception(string.Empty, innerException);
}
