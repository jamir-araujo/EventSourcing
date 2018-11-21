using System;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Microsoft.Extensions.Options;

namespace EventSourcing.EventStore
{
    public interface IEventStoreConnectionAccessor
    {
        Task<IEventStoreConnection> GetConnectionAsync();
    }

    public class EventStoreConnectionAccessor : IEventStoreConnectionAccessor, IDisposable
    {
        private readonly IOptions<EventStoreOptions> _options;
        private IEventStoreConnection _connection;

        public EventStoreConnectionAccessor(IOptions<EventStoreOptions> options)
        {
            _options = options;
        }

        public async Task<IEventStoreConnection> GetConnectionAsync()
        {
            if (_connection == null)
            {
                _connection = EventStoreConnection.Create(_options.Value.ConnectionString);
                await _connection.ConnectAsync();
            }

            return _connection;
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }
}
