using System.Text.Json.Serialization;

namespace BuildingBlocks.Contracts.IntegrationEvents
{
    public record IntegrationEvent
    {
        public IntegrationEvent()
        {
            Id = Guid.NewGuid();
            CreationDate = DateTimeOffset.UtcNow;
        }

        [JsonInclude]
        public Guid Id { get; set; }

        [JsonInclude]
        public DateTimeOffset CreationDate { get; set; }

        public long SequenceNumber { get; init; }
    }
}
