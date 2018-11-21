using System;

namespace EventSourcing
{
    public class VersionedEvent : IVersionedEvent
    {
        public Guid EventId { get; }
        public Guid SourceId { get; set; }
        public long Version { get; set; }

        public VersionedEvent()
        {
            EventId = Guid.NewGuid();
        }
    }
}
