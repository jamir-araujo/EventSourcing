using System;
using System.Collections.Generic;

namespace Tnf.EventSourcing
{
    public interface IEventSourcedAggregateRoot
    {
        Guid Id { get; }
        long Version { get; }
        IReadOnlyList<IVersionedEvent> GetPendingEvents();
        void LoadFrom(IEnumerable<IVersionedEvent> events);
    }
}
