using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace EventSourcing.EventStore
{
    public class EventStoreRepository<TAggregateRoot> : IEventSourcedRepository<TAggregateRoot>
        where TAggregateRoot : IEventSourcedAggregateRoot
    {
        private static readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple
        };

        private readonly IEventStoreConnectionAccessor _connectionAccessor;
        private readonly IOptions<EventStoreOptions> _options;
        private readonly ILogger<EventStoreRepository<TAggregateRoot>> _logger;
        private readonly string _aggregateName;

        public EventStoreRepository(
            IEventStoreConnectionAccessor connectionAccessor,
            IOptions<EventStoreOptions> options,
            ILogger<EventStoreRepository<TAggregateRoot>> logger)
        {
            _connectionAccessor = connectionAccessor;
            _options = options;
            _logger = logger;
            _aggregateName = typeof(TAggregateRoot).Name;
        }

        public async Task<TAggregateRoot> GetAsync(Guid id)
        {
            TAggregateRoot aggregate = default;

            var connection = await _connectionAccessor.GetConnectionAsync();

            long start = 0;
            int count = _options.Value.ReadingBlockSize;
            StreamEventsSlice eventsSlice = null;
            do
            {
                if (eventsSlice != null)
                {
                    start = eventsSlice.NextEventNumber;
                }

                var streamName = GetStreamName(id);
                eventsSlice = await connection.ReadStreamEventsForwardAsync(streamName, start, count, false);

                if (eventsSlice.Status == SliceReadStatus.Success)
                {
                    if (aggregate == default)
                    {
                        aggregate = (TAggregateRoot)Activator.CreateInstance(typeof(TAggregateRoot), id);
                    }
                }
                else
                {
                    break;
                }

                aggregate.Load(Deserialize(eventsSlice.Events));

            } while (!eventsSlice.IsEndOfStream);

            return aggregate;
        }

        public async Task SaveAsync(TAggregateRoot aggregate)
        {
            var connection = await _connectionAccessor.GetConnectionAsync();

            var streamName = GetStreamName(aggregate.Id);
            var changeEvents = aggregate.GetPendingChanges();
            var spectedVersion = aggregate.Version - changeEvents.Count - 1;

            if (spectedVersion <= 0)
            {
                spectedVersion = ExpectedVersion.NoStream;
            }

            using (var transaction = await connection.StartTransactionAsync(streamName, spectedVersion))
            {
                try
                {
                    await transaction.WriteAsync(changeEvents.Select(ToEventData));

                    //publish events to event bus here

                    await transaction.CommitAsync();

                    aggregate.ClearPendingChanges();
                }
                catch (WrongExpectedVersionException exception)
                {
                    _logger?.LogError(exception, exception.Message);
                    throw new ConcurrentChangeException(aggregate.Id, _aggregateName, exception);
                }
                catch (Exception exception)
                {
                    _logger?.LogError(exception, exception.Message);
                    transaction.Rollback();
                    throw;
                }
            }
        }

        private IEnumerable<IVersionedEvent> Deserialize(ResolvedEvent[] events)
        {
            return events.Select(e =>
            {
                var jsonString = Encoding.UTF8.GetString(e.OriginalEvent.Data);
                return JsonConvert.DeserializeObject(jsonString, _serializerSettings) as IVersionedEvent;
            });
        }

        private EventData ToEventData(IVersionedEvent versionedEvent)
        {
            var eventType = versionedEvent.GetType().Name;

            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(versionedEvent, _serializerSettings));

            return new EventData(versionedEvent.EventId, eventType, true, body, new byte[0]);
        }

        private string GetStreamName(Guid id)
        {
            return $"{_aggregateName}_{id}";
        }
    }
}
