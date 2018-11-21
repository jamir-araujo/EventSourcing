using System;
using System.Collections.Generic;

namespace EventSourcing
{
    public interface IEventSourcedAggregateRoot
    {
        Guid Id { get; }
        long Version { get; }

        IReadOnlyList<IVersionedEvent> GetPendingChanges();
        void ClearPendingChanges();
        void Load(IEnumerable<IVersionedEvent> events);
    }
}
