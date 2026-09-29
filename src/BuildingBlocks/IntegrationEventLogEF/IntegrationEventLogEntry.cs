using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using BuildingBlocks.Contracts.IntegrationEvents;

namespace BuildingBlocks.IntegrationEventLogEF;

/*
 Проект IntegrationEventLogEF реализует паттерн "Outbox" (исходящий ящик) 
 для надежной доставки интеграционных событий между сервисами в распределенной 
 архитектуре BuildingBlocks.
    Основная назначение:
    Гарантирует доставку событий "ровно один раз" (at-least-once delivery с идемпотентностью):
    •	Когда служба совершает важное действие (например, заказ создан), событие сохраняется в БД одной транзакцией вместе с самыми данными
    •	Это исключает сценарий: "данные сохранены, но событие не отправлено" (сбой сети)

    IntegrationEventLogEntry (класс сущности):
    ├── EventId         - уникальный идентификатор события
    ├── EventTypeName   - полное имя типа события (для десериализации)
    ├── Content         - JSON содержимое события
    ├── State           - статус (NotPublished, InProgress, Published)
    ├── TimesSent       - количество попыток отправки
    ├── TransactionId   - привязка к транзакции БД
    └── CreationTime    - время создания

    Как это работает в BuildingBlocks:
    1.	Сохранение: Микросервис (Catalog, Ordering и т.д.) сохраняет событие в свою таблицу IntegrationEventLog одной транзакцией с основными данными
    2.	Фоновый процесс: EventBus периодически сканирует события со статусом NotPublished
    3.	Доставка: Публикует события в шину сообщений (RabbitMQ/Kafka)
    4.	Отметка: Устанавливает статус Published только после успешной 
 */
public class IntegrationEventLogEntry
{
    private static readonly JsonSerializerOptions s_indentedOptions = new() { WriteIndented = true };
    private static readonly JsonSerializerOptions s_caseInsensitiveOptions = new() { PropertyNameCaseInsensitive = true };

    private IntegrationEventLogEntry() { }
    public IntegrationEventLogEntry(IntegrationEvent @event, Guid transactionId, string? traceParent)
    {
        EventId = @event.Id;
        CreatedOnUtc = @event.CreationDate;
        EventTypeName = @event.GetType().FullName ?? @event.GetType().Name;
        Content = JsonSerializer.Serialize(@event, @event.GetType(), s_indentedOptions);
        State = EventStateEnum.NotPublished;
        TimesSent = 0;
        TraceParent = traceParent;
    }

    /// <summary>
    /// Id интеграционного события
    /// </summary>
    public Guid EventId { get; private set; }

    /// <summary>
    /// Полное наименование типа интеграционного события
    /// </summary>
    [Required]
    public string EventTypeName { get; private set; } = null!;

    /// <summary>
    /// Краткое наименование типа интеграционного события
    /// </summary>
    [NotMapped]
    public string EventTypeShortName => EventTypeName.Contains('.')
        ? EventTypeName.Split('.').Last()
        : EventTypeName;

    /// <summary>
    /// Исходное интеграционное событие, на основе которого создается данное событие
    /// </summary>
    [NotMapped]
    public IntegrationEvent IntegrationEvent { get; private set; } = null!;

    /// <summary>
    /// Статус
    /// </summary>
    public EventStateEnum State { get; set; }

    /// <summary>
    /// Количество попыток отправить событие в очередь сообщений
    /// </summary>
    public int TimesSent { get; set; }

    /// <summary>
    /// Данные события
    /// </summary>
    [Required]
    public string Content { get; private set; } = null!;
    
    /// <summary>
    /// Транзакция, в рамках которой было создано событие
    /// </summary>
    public Guid TransactionId { get; private set; }

    /// <summary>
    /// Дата создания события
    /// </summary>
    public DateTimeOffset CreatedOnUtc { get; private set; }

    /// <summary>
    /// Дата последней попытки опубликовать событие в очередь сообщений
    /// </summary>
    public DateTimeOffset?  LastAttemptOnUtc { get; set; }

    /// <summary>
    /// Дата, когда событие было опубликовано в очередь сообщений
    /// </summary>
    public DateTimeOffset? PublishedOnUtc { get; set; }

    /// <summary>
    /// Идентификатор контекста распределенной трассировки в формате W3C (traceparent).
    /// Используется для сквозного логирования между микросервисами.
    /// </summary>
    public string? TraceParent { get; set; }

    /// <summary>
    /// Метод десериализации контента интеграционного события
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public IntegrationEventLogEntry DeserializeJsonContent(Type type)
    {
        var deserialized = JsonSerializer.Deserialize(Content, type, s_caseInsensitiveOptions);

        IntegrationEvent = deserialized as IntegrationEvent
            ?? throw new InvalidOperationException($"Failed to deserialize content to {type.Name} or event type is invalid.");

        return this;
    }
}
