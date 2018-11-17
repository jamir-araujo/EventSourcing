using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Tnf.EventSourcing.EventStore
{
    public interface IEventSourcedRepository<TAggregateRoot>
    {
        Task<TAggregateRoot> GetAsync(Guid id);
        Task SaveAsync(TAggregateRoot entity);
    }

    public class EventStoreRepository<TAggregateRoot> : IEventSourcedRepository<TAggregateRoot>
        where TAggregateRoot : IEventSourcedAggregateRoot
    {
        private static readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple
        };

        private readonly IOptions<EventStoreOptions> _options;
        private readonly ILogger<EventStoreRepository<TAggregateRoot>> _logger;
        private readonly string _aggregateName;

        public EventStoreRepository(
            IOptions<EventStoreOptions> options,
            ILogger<EventStoreRepository<TAggregateRoot>> logger)
        {
            _options = options;
            _logger = logger;
            _aggregateName = typeof(TAggregateRoot).Name;
        }

        public async Task<TAggregateRoot> GetAsync(Guid id)
        {
            TAggregateRoot aggregate = default;

            using (var connection = EventStoreConnection.Create(_options.Value.ConnectionString))
            {
                await connection.ConnectAsync();

                long start = 0;
                StreamEventsSlice eventsSlice = null;
                do
                {
                    if (eventsSlice != null)
                    {
                        start = eventsSlice.NextEventNumber;
                    }

                    var streamName = GetStreamName(id);
                    eventsSlice = await connection.ReadStreamEventsForwardAsync(streamName, start, 100, false);

                    if (eventsSlice.Status == SliceReadStatus.Success && aggregate == default)
                    {
                        aggregate = (TAggregateRoot)Activator.CreateInstance(typeof(TAggregateRoot), id);
                    }
                    else
                    {
                        break;
                    }

                    aggregate.LoadFrom(Deserialize(eventsSlice.Events));

                } while (!eventsSlice.IsEndOfStream);
            }

            return aggregate;
        }

        public async Task SaveAsync(TAggregateRoot entity)
        {
            using (var connection = EventStoreConnection.Create(_options.Value.ConnectionString))
            {
                await connection.ConnectAsync();

                var streamName = GetStreamName(entity.Id);
                var events = entity.GetPendingEvents();
                var spectedVersion = entity.Version - events.Count;

                if (spectedVersion <= 0)
                {
                    spectedVersion = ExpectedVersion.NoStream;
                }

                using (var transaction = await connection.StartTransactionAsync(streamName, spectedVersion))
                {
                    try
                    {
                        await transaction.WriteAsync(events.Select(ToEventData));

                        //publish events to event bus here

                        await transaction.CommitAsync();
                    }
                    catch (Exception e)
                    {
                        _logger?.LogError(e, e.Message);
                        transaction.Rollback();
                        throw;
                    }
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
