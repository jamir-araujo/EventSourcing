using System;
using System.Collections.Generic;

namespace Tnf.EventSourcing
{
    public abstract class EventSourcedAggregateRoot : IEventSourcedAggregateRoot
    {
        private Dictionary<Type, Action<IVersionedEvent>> _handlers = new Dictionary<Type, Action<IVersionedEvent>>();
        private readonly List<IVersionedEvent> _pendingChanges = new List<IVersionedEvent>();

        public Guid Id { get; private set; }
        public long Version { get; private set; }

        protected EventSourcedAggregateRoot(Guid id)
        {
            Id = id;
        }

        IReadOnlyList<IVersionedEvent> IEventSourcedAggregateRoot.GetPendingChanges() => _pendingChanges.AsReadOnly();

        void IEventSourcedAggregateRoot.ClearPendingChanges() => _pendingChanges.Clear();

        void IEventSourcedAggregateRoot.Load(IEnumerable<IVersionedEvent> events)
        {
            foreach (var @event in events)
            {
                Invoke(@event);
                Version = @event.Version;
            }
        }

        protected void Handle<TEvent>(Action<TEvent> handler)
            where TEvent : IVersionedEvent
        {
            _handlers.Add(typeof(TEvent), e => handler((TEvent)e));
        }

        protected void Update(VersionedEvent @event)
        {
            @event.SourceId = Id;
            @event.Version = Version + 1;

            Invoke(@event);

            Version = @event.Version;
            _pendingChanges.Add(@event);
        }

        private void Invoke(IVersionedEvent @event)
        {
            _handlers[@event.GetType()](@event);
        }
    }
}
