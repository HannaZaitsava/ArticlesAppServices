using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Contracts.IntegrationEvents;
using EntityFrameworkCore.Locking;

namespace BuildingBlocks.IntegrationEventLogEF.Services;

public class IntegrationEventLogService<TContext> : IIntegrationEventLogService, IDisposable
    where TContext : DbContext
{
    private readonly TimeProvider _timeProvider;
    private volatile bool _disposedValue;
    private readonly TContext _context;
    private readonly Type[] _eventTypes;

    public IntegrationEventLogService(TContext context, TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
        _context = context;      
        _eventTypes = typeof(IntegrationEvent).Assembly //Assembly.Load(Assembly.GetEntryAssembly().FullName)
        .GetTypes()
        .Where(t => t.Name.EndsWith(nameof(IntegrationEvent)) && !t.IsAbstract && t.IsClass)
        .ToArray();
    }

    /// <summary>
    /// Атомарно извлекает пачку необработанных событий и блокирует их статусом InProgress.
    /// Безопасно для масштабирования (использует SKIP LOCKED).
    /// </summary>
    //public async Task<IEnumerable<IntegrationEventLogEntry>> RetrieveAndLockEventLogsAsync(int batchSize = 500, int maxTimesSent = 3, CancellationToken ct = default)
    //{       
    //    //var rawSql = @"
    //    //    UPDATE ""IntegrationEventLog""
    //    //    SET ""State"" = {0}, ""TimesSent"" = ""TimesSent"" + 1, ""LastAttemptOnUtc"" = {1}
    //    //    WHERE ""EventId"" IN (
    //    //        SELECT ""EventId"" 
    //    //        FROM ""IntegrationEventLog""
    //    //        WHERE ""State"" = {2} AND ""TimesSent"" < {3} 
    //    //        ORDER BY ""CreatedOnUtc"" ASC
    //    //        LIMIT {4}
    //    //        FOR UPDATE SKIP LOCKED
    //    //    )
    //    //    RETURNING *";

    //    //    var result = await _context.Set<IntegrationEventLogEntry>()
    //    //    .FromSqlRaw(rawSql, EventStateEnum.InProgress, _timeProvider.GetUtcNow(), EventStateEnum.NotPublished, maxTimesSent, batchSize)
    //    //    .ToListAsync(ct);


    //    // Альтернатива на EF Core
    //    // Подзапрос на выборку ID с блокировкой строк в PostgreSQL
    //    // https://github.com/mnbuhl/efcore-locking#queue-processing

    /*
     
    Метод ExecuteUpdateAsync в EF Core спроектирован так, что он всегда создаёт и выполняет свой собственный внутренний запрос к базе данных, 
    игнорируя текущее состояние транзакции или контекста, если не управлять этим явно.
    
    Из-за этого внутри одного метода у нас происходит следующее:
    1. Мы открываете transaction и блокируете строки через .ForUpdate(LockBehavior.SkipLocked).
    2. Вызывается ExecuteUpdateAsync. Он пытается обновить эти же строки, но делает это в рамках другого внутреннего соединения/команды, 
       которая «не знает», что текущий поток уже заблокировал эти строки.
    3. ExecuteUpdateAsync упирается в вашу же блокировку FOR UPDATE и начинает бесконечно ждать, пока она отпустит строки.
    4. Происходит Self-Deadlock (самоблокировка). Поток ждет сам себя, через 30 секунд срабатывает таймаут, и всё падает, параллельно забивая пул соединений.
     
     */
    //    await using var transaction = await _context.Database.BeginTransactionAsync(ct);

    //    // Выкачиваем только ID заблокированных строк в память 
    //    var pendingEventIds = await _context.Set<IntegrationEventLogEntry>()
    //        .Where(e => e.State == EventStateEnum.NotPublished) //&& e.TimesSent < 3)
    //        .OrderBy(e => e.CreatedOnUtc)
    //        .Take(batchSize)
    //        .ForUpdate(LockBehavior.SkipLocked) // Блокируем строки до конца транзакции
    //        .Select(e => e.EventId)
    //        .ToListAsync(ct); // Выполняем в память - т.к. далее нужно будет использовать его результат в двух местах, ин

    //    if (!pendingEventIds.Any())
    //    {
    //        return []; 
    //    }

    //    // Обновление статуса в базе по конкретным ID
    //    await _context.Set<IntegrationEventLogEntry>()
    //        .Where(e => pendingEventIds.Contains(e.EventId))
    //        .ExecuteUpdateAsync(s => s
    //            .SetProperty(e => e.State, EventStateEnum.InProgress)
    //            .SetProperty(e => e.TimesSent, e => e.TimesSent + 1)
    //            .SetProperty(e => e.LastAttemptOnUtc, e => _timeProvider.GetUtcNow()),
    //            ct);

    //    // Загружаем эти же записи для работы воркера
    //    var result = await _context.Set<IntegrationEventLogEntry>()
    //        .Where(e => pendingEventIds.Contains(e.EventId))
    //        .ToListAsync(ct);

    //    await transaction.CommitAsync(ct);


    //    if (result.Count == 0) return [];

    //    // Десериализация контента в памяти перед отправкой в воркер
    //    return result.Select(e => e.DeserializeJsonContent(_eventTypes.FirstOrDefault(t => t.Name == e.EventTypeShortName)));
    //}


    /// <summary>
    /// Атомарно извлекает пачку необработанных событий и блокирует их статусом InProgress.
    /// Безопасно для масштабирования (использует SKIP LOCKED). https://github.com/mnbuhl/efcore-locking#queue-processing
    /// </summary>
    public async Task<IEnumerable<IntegrationEventLogEntry>> RetrieveAndLockEventLogsAsync(int batchSize = 500, int maxTimesSent = 3, CancellationToken ct = default)
    {       
        var strategy = _context.Database.CreateExecutionStrategy();

        var dbEntries = await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(ct);

            var result = await _context.Set<IntegrationEventLogEntry>()
                .Where(e => e.State == EventStateEnum.NotPublished)
                .OrderBy(e => e.CreatedOnUtc)
                .Take(batchSize)
                .ForUpdate(LockBehavior.SkipLocked) // Гарантирует, что другие инстансы воркера пропустят эти строки
                .AsTracking()
                .ToListAsync(ct);

            if (!result.Any())
            {
                await transaction.RollbackAsync(ct);
                return [];
            }

            foreach (var e in result)
            {
                e.State = EventStateEnum.InProgress;
                e.TimesSent++;
                e.LastAttemptOnUtc = _timeProvider.GetUtcNow();
            }

            await _context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            return result; 
        });

        if (dbEntries.Count == 0) return [];

        return DeserializeEventEntries(dbEntries);
    }


    public async Task<IEnumerable<IntegrationEventLogEntry>> RetrieveEventLogsPendingToPublishAsync(Guid transactionId)
    {
        var result = await _context.Set<IntegrationEventLogEntry>()
            .Where(e => e.TransactionId == transactionId && e.State == EventStateEnum.NotPublished)
            .ToListAsync();

        if (result.Count != 0)
        {
            var sortedResults = result.OrderBy(o => o.CreatedOnUtc);
            return DeserializeEventEntries(sortedResults);
        }

        return [];
    }

    /// <summary>
    /// Метод сохранения события в outbox-таблицу
    /// </summary>
    /// <param name="event">Событие</param>
    /// <param name="transaction">Тразакция, которая связана с DB-контекстом микросервиса, в котором будет лежать outbox-таблица</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public Task SaveEventAsync(IntegrationEvent @event, IDbContextTransaction? transaction)
    {
        if (transaction == null) 
            throw new ArgumentNullException(nameof(transaction));

        var eventLogEntry = new IntegrationEventLogEntry(@event, transaction.TransactionId, Activity.Current?.Id);

        _context.Database.UseTransaction(transaction.GetDbTransaction());
        _context.Set<IntegrationEventLogEntry>().Add(eventLogEntry);

        return _context.SaveChangesAsync();
    }

    public Task MarkEventAsPublishedAsync(Guid eventId)
    {
        return UpdateEventStatus(eventId, EventStateEnum.Published);
    }

    public Task MarkEventAsInProgressAsync(Guid eventId)
    {
        return UpdateEventStatus(eventId, EventStateEnum.InProgress);
    }

    public Task MarkEventAsFailedAsync(Guid eventId, int maxTimesSent)
    {
        return UpdateEventStatus(eventId, EventStateEnum.PublishedFailed, maxTimesSent);
    }

    private Task UpdateEventStatus(Guid eventId, EventStateEnum status, int maxTimesSent = 3)
    {
        var eventLogEntry = _context.Set<IntegrationEventLogEntry>().Single(ie => ie.EventId == eventId);
        
        if (status == EventStateEnum.PublishedFailed && eventLogEntry.TimesSent >= maxTimesSent)
        {
            eventLogEntry.State = status;
        }
        else if (eventLogEntry.State == EventStateEnum.PublishedFailed)
        {
            // Попытка неудачная, но лимит попыток не исчерпан — возвращаем в очередь
            eventLogEntry.State = EventStateEnum.NotPublished;
        }
        else if (status == EventStateEnum.InProgress)
        {
            eventLogEntry.TimesSent++;
        }
        else
        {
            eventLogEntry.State = status;
        }

        return _context.SaveChangesAsync();
    }

    /// <summary>
    /// Находит сообщения, зависшие в статусе InProgress дольше определенного времени,
    /// и возвращает их в статус NotPublished для повторной обработки.
    /// </summary>    
    public async Task ResetStaleInProgressEventsAsync(TimeSpan timeout)
    {
        // Теперь мы точно знаем, КОГДА воркер взял эту запись в работу
        var cutoffTime = DateTimeOffset.UtcNow.Subtract(timeout);

        await _context.Set<IntegrationEventLogEntry>()
            .Where(e => e.State == EventStateEnum.InProgress && e.LastAttemptOnUtc < cutoffTime) 
            .ExecuteUpdateAsync(s => s.SetProperty(e => e.State, EventStateEnum.NotPublished));
    }

    public async Task BatchMarkEventAsInProgressedAsync(IEnumerable<Guid> eventIds)
    {
        await BatchUpdateEventStatusAsync(eventIds, EventStateEnum.InProgress);
    }

    public async Task BatchMarkEventAsPublishedAsync(IEnumerable<Guid> eventIds)
    {
        await BatchUpdateEventStatusAsync(eventIds, EventStateEnum.Published);
    }

    public async Task BatchMarkEventAsFailedAsync(IEnumerable<Guid> eventIds, int maxTimesSent)
    {
        await BatchUpdateEventStatusAsync(eventIds, EventStateEnum.PublishedFailed, maxTimesSent);
    }

    private async Task BatchUpdateEventStatusAsync(IEnumerable<Guid> eventIds, EventStateEnum status, int maxTimesSent = 3)
    {
        // .ToList(), чтобы не вычислять коллекцию далее внутри EF Core
        var ids = eventIds.ToList();
        if (ids.Count == 0) return;

        /*
        e => status вместо status: 
        Использование лямбды внутри SetProperty указывает EF Core, 
        что нужно взять значение из внешней переменной и передать его как стандартный SQL-параметр @status. 
        Это защищает от потенциальных ошибок трансляции LINQ в SQL.
         */
        if (status == EventStateEnum.Published)
        {
            await _context.Set<IntegrationEventLogEntry>()
           .Where(e => ids.Contains(e.EventId))
           .ExecuteUpdateAsync(s => s
                .SetProperty(e => e.State, e => status)
                .SetProperty(e => e.PublishedOnUtc, e => _timeProvider.GetUtcNow()));
        }
        else if (status == EventStateEnum.PublishedFailed)
        {
            // Те, кто превысил лимит, окончательно помечаются как PublishedFailed
            var finalFailedCount = await _context.Set<IntegrationEventLogEntry>()
                .Where(e => ids.Contains(e.EventId) && e.TimesSent >= maxTimesSent)
                .ExecuteUpdateAsync(s => s.SetProperty(e => e.State, e => status));

            //if (finalFailedCount > 0)
            //{                
            //    _logger.LogCritical("{Count} messages from the current batch exceeded max retries and were isolated as FAILED. IDs: {Ids}",
            //        finalFailedCount, string.Join(", ", ids));
            //}

            // Те, у кого попытки еще остались, возвращаются в статус NotPublished для следующего круга воркера
            await _context.Set<IntegrationEventLogEntry>()
                .Where(e => ids.Contains(e.EventId) && e.TimesSent < maxTimesSent)
                .ExecuteUpdateAsync(s => s.SetProperty(e => e.State, e => EventStateEnum.NotPublished));
        }       
        else
        {
            await _context.Set<IntegrationEventLogEntry>()
                .Where(e => ids.Contains(e.EventId))
                .ExecuteUpdateAsync(s => s.SetProperty(e => e.State, e => status));
        }
    }

    /*
     * Реализация IDisposable здесь — это соблюдение контракта сквозного управления ресурсами, гарантирующее, что микросервис не «потечет» по памяти под нагрузкой.
     * 
     * Класс IntegrationEventLogService реализует интерфейс IDisposable (или IAsyncDisposable) исключительно для того, 
     * чтобы правильно управлять временем жизни контекста базы данных (DbContext) и вовремя освобождать его ресурсы.
     * 
     * Важный архитектурный нюанс (Внедрение зависимостей)В .NET Core встроенный DI-контейнер (IServiceProvider) умеет автоматически вызывать Dispose(), 
     * если сервис зарегистрирован с правильным временем жизни:Обычно IntegrationEventLogService регистрируется как Scoped 
     * (создается на один HTTP-запрос или на одну итерацию обработки сообщения).
     * Когда Scoped-контейнер завершает свою работу, он сам автоматически вызывает Dispose() у IntegrationEventLogService, 
     * а тот, в свою очередь, «паровозиком» вызывает Dispose() у DbContext.
     * */
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _context.Dispose();
            }


            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private IEnumerable<IntegrationEventLogEntry> DeserializeEventEntries(IEnumerable<IntegrationEventLogEntry> entries)
    {
        var deserializedEntries = new List<IntegrationEventLogEntry>();

        foreach (var entry in entries)
        {
            Type? eventType = _eventTypes.FirstOrDefault(t => t.Name == entry.EventTypeShortName);

            if (eventType == null)
            {
                throw new InvalidOperationException(
                    $"Event type '{entry.EventTypeShortName}' was found in Outbox database (EventId: {entry.EventId}), " +
                    $"but its corresponding C# class is not registered in the application standard event types list.");
            }

            deserializedEntries.Add(entry.DeserializeJsonContent(eventType));
        }

        return deserializedEntries;
    }
}
