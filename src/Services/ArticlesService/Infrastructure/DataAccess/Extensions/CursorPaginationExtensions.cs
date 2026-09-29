using ArticlesService.Application.Common.Constants;
using ArticlesService.Application.Enums;
using ArticlesService.Application.RequestFeatures.CursorPagination;
using ArticlesService.Domain.Entities.Base;
using ArticlesService.Infrastructure.DataAccess.Pagination;
using Microsoft.EntityFrameworkCore;

namespace ArticlesService.Infrastructure.DataAccess.Extensions;


    /*
     Рекомендация: 
     Главное преимущество пагинации по курсору — её высокая скорость на огромных объемах данных (миллионы строк), так как она использует индексы напрямую.
     Если для каждой страницы делать _context.Comments.Count(), это убьет преимущество.
     Если элементов для пагинации планируется очень много (сотни тысяч), лучше вообще убрать ItemsTotalCount из ответа.         
     Курсорной пагинации достаточно знать HasNextPage, чтобы показать или скрыть кнопку «Загрузить еще». 
     Выдавать ItemsTotalCount только там, где это критически важно для интерфейса.
    */

/// <summary>
/// Методы расширения для cursor-based пагинации
/// </summary>
public static class CursorPaginationExtensions
{
    private static readonly ICursorCodec CursorCodec = new Base64CursorCodec();

    public static IQueryable<T> ApplyCursorPagination<T>(
    this IQueryable<T> query,
    CursorPaginationParameters paginationParameters)
    where T : BaseEntity, IAuditableEntity  
    {
        var pageSize = paginationParameters.PageSize == 0
            ? PaginationConstants.MaxPageSize
            : paginationParameters.PageSize;

        if (!string.IsNullOrWhiteSpace(paginationParameters.Cursor))
        {
            // Декодируем составной курсор 
            var cursor = CursorCodec.Decode<CreatedOnCursor>(paginationParameters.Cursor);

            query = paginationParameters.Direction switch
            {
                PaginationDirection.Forward =>
                    // Вперед -> к более старым -> МЕНЬШЕ курсора
                    query.Where(x => EF.Functions.LessThan(
                        ValueTuple.Create(x.CreatedOn, x.Id),
                        ValueTuple.Create(cursor.CreatedOn, cursor.Id))),

                PaginationDirection.Backward =>
                    // Назад -> к более новым -> БОЛЬШЕ курсора
                    query.Where(x => EF.Functions.GreaterThan(
                        ValueTuple.Create(x.CreatedOn, x.Id),
                        ValueTuple.Create(cursor.CreatedOn, cursor.Id))),

                _ => query
            };
        }

        // Применяем правильный OrderBy для PostgreSQL в зависимости от направления движения
        query = paginationParameters.Direction switch
        {
            PaginationDirection.Forward => query
                .OrderByDescending(c => c.CreatedOn)
                .ThenByDescending(c => c.Id),

            PaginationDirection.Backward => query
                .OrderBy(c => c.CreatedOn)
                .ThenBy(c => c.Id),

            _ => query
                .OrderByDescending(c => c.CreatedOn)
                .ThenByDescending(c => c.Id)
        };

        return query.Take(pageSize + 1);
    }

    public static async Task<CursorPagedResult<T>> ToCursorPagedResultAsync<T>(
    this IQueryable<T> query,
    CursorPaginationParameters paginationParameters,
    Func<T, CreatedOnCursor> cursorSelector) 
    {
        var pageSize = paginationParameters.PageSize == 0 ? PaginationConstants.MaxPageSize : paginationParameters.PageSize;
        var items = await query.ToListAsync();
        bool hasExtraItem = items.Count > pageSize;

        if (hasExtraItem)
        {
            items.RemoveAt(items.Count - 1);
        }

        bool hasNextPage = hasExtraItem;//paginationParameters.Direction == PaginationDirection.Forward ? hasExtraItem : true;
        bool hasPreviousPage = !string.IsNullOrWhiteSpace(paginationParameters.Cursor); //paginationParameters.Direction == PaginationDirection.Backward ? hasExtraItem : !string.IsNullOrWhiteSpace(paginationParameters.Cursor);

        string? nextCursor = null;
        string? previousCursor = null;

        if (items.Count > 0)
        {
            previousCursor = CursorCodec.Encode(cursorSelector(items.First()));
            nextCursor = CursorCodec.Encode(cursorSelector(items.Last()));
        }

        if (paginationParameters.Direction == PaginationDirection.Backward)
        {
            items.Reverse(); 
        }

        /* Оптимизация памяти
            // Вариант 1 - БЕЗ проверки (неэффективно)
            var ro1 = items.AsReadOnly();  // Создаёт новый ReadOnlyCollection<T> в памяти
            // Теперь в памяти: List<T> + ReadOnlyCollection<T> (две копии данных!)

            // Вариант 2 - С проверкой (эффективно)
            var ro2 = items as IReadOnlyList<T> ?? items.AsReadOnly();
            // Просто приводим существующий List<T> к интерфейсу, новый объект НЕ создаётся!
         */
        var itemsList = items as IReadOnlyList<T> ?? items.AsReadOnly();
              
        return new CursorPagedResult<T>(
           Items: itemsList,
           NextCursor: hasNextPage ? nextCursor : null,  
           PreviousCursor: hasPreviousPage ? previousCursor : null,
           HasNextPage: hasNextPage,
           HasPreviousPage: hasPreviousPage,
           //ItemsTotalCount: totalCount,
           PageSize: pageSize);
    }
}