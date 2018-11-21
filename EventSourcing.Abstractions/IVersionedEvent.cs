using System;

namespace EventSourcing
{
    public interface IVersionedEvent
    {
        Guid EventId { get; }
        Guid SourceId { get; }
        long Version { get; }
    }
}
