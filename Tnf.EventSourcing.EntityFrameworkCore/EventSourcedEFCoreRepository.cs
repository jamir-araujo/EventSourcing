using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Tnf.EventSourcing.Abstractions;

namespace Tnf.EventSourcing.EntityFrameworkCore
{
    public class EventSourcedEFCoreRepository<TAggregateRoot> : IEventSourcedRepository<TAggregateRoot>
        where TAggregateRoot : IEventSourcedAggregateRoot
    {
        private static readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple
        };

        private readonly EventsDbContext _context;
        private readonly string _aggregateType;

        public EventSourcedEFCoreRepository(EventsDbContext context)
        {
            _context = context;
            _aggregateType = typeof(TAggregateRoot).Name;
        }

        public async Task<TAggregateRoot> GetAsync(Guid id)
        {
            var storedEvents = await _context.Events
                .Where(e => e.AggregateId == id)
                .Where(e => e.AggregateType == _aggregateType)
                .OrderBy(p => p.Version)
                .ToListAsync();

            TAggregateRoot aggregate = default;

            if (storedEvents.Count > 0)
            {
                var events = storedEvents.Select(ToVersionedEvent);

                aggregate = (TAggregateRoot)Activator.CreateInstance(typeof(TAggregateRoot), id);
                aggregate.Load(events);
            }

            return aggregate;
        }

        public async Task SaveAsync(TAggregateRoot aggregate)
        {
            var changeEvents = aggregate.GetPendingChanges();
            var originalVersion = aggregate.Version - changeEvents.Count;

            await CheckConcurrency(aggregate, originalVersion);

            var storeEvents = changeEvents.Select(ToStoredEvent);

            _context.Events.AddRange(storeEvents);

            await _context.SaveChangesAsync();

            aggregate.ClearPendingChanges();
        }

        private async Task CheckConcurrency(TAggregateRoot aggregate, long originalVersion)
        {
            if (originalVersion == 0)
            {
                return;
            }

            var lastVersion = await _context.Events
                .Where(e => e.AggregateId == aggregate.Id)
                .Where(e => e.AggregateType == _aggregateType)
                .MaxAsync(e => e.Version);

            if (originalVersion != lastVersion)
            {
                throw new ConcurrentChangeException(aggregate.Id, _aggregateType);
            }
        }

        private StoredEvent ToStoredEvent(IVersionedEvent versionedEvent)
        {
            return new StoredEvent(
                versionedEvent.SourceId,
                _aggregateType,
                JsonConvert.SerializeObject(versionedEvent, _serializerSettings),
                versionedEvent.Version,
                versionedEvent.GetType().Name,
                null);
        }

        private IVersionedEvent ToVersionedEvent(StoredEvent storedEvent)
        {
            return JsonConvert.DeserializeObject(storedEvent.Body, _serializerSettings) as IVersionedEvent;
        }
    }
}
