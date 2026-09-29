namespace ArticlesService.Domain.Result
{
    /// <summary>
    /// Интерфейс для реализации паттерна Адаптер
    /// </summary>
    /// <remarks>
    /// Нужен для корректной работы кэширования.
    /// Т.к. в бизнес-логике реализован Result Pattern, то кэшировать нужно не все значение Result<T>, а только его Value.
    /// </remarks>
    public interface IResult
    {
        bool IsSuccess { get; }
        object? RawValue { get; }

        /// <summary>
        /// Метод для пересоздания объекта ошибки
        /// </summary>
        /// <returns></returns>
        object ToFailureResult(); 
    }

    //public interface IResult<out TData> : IResult
    //{
    //    TData? Value { get; }
    //}
}
