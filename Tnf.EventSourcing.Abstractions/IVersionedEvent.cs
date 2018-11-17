using System;

namespace Tnf.EventSourcing
{
    public interface IVersionedEvent
    {
        Guid EventId { get; }
        Guid SourceId { get; }
        long Version { get; }
    }
}
