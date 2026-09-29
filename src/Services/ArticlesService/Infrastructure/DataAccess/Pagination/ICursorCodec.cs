namespace ArticlesService.Infrastructure.DataAccess.Pagination;

/// <summary>
/// Интерфейс для кодирования и декодирования курсоров
/// </summary>
public interface ICursorCodec
{
    /// <summary>
    /// Кодирует значение в строку курсора
    /// </summary>
    string Encode<T>(T value) where T : struct;

    /// <summary>
    /// Декодирует строку курсора в значение
    /// </summary>
    T Decode<T>(string cursor) where T : struct;
}
