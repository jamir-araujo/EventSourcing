using System;
using EventSourcing;
using EventSourcing.EventStore;
using Microsoft.Extensions.Configuration;


namespace Microsoft.Extensions.DependencyInjection
{
    public static class EventStoreServiceCollectionExtensions
    {
        public static IServiceCollection AddTnfEventStore(this IServiceCollection services, IConfiguration configuration)
        {
            return services.AddTnfEventStore(svs => svs.Configure<EventStoreOptions>(configuration));
        }

        public static IServiceCollection AddTnfEventStore(this IServiceCollection services, Action<EventStoreOptions> configure)
        {
            return services.AddTnfEventStore(svs => svs.Configure(configure));
        }

        public static IServiceCollection AddTnfEventStore(this IServiceCollection services, Func<IServiceCollection, IServiceCollection> configure)
        {
            configure(services);

            services.AddSingleton<IEventStoreConnectionAccessor, EventStoreConnectionAccessor>();
            services.AddTransient(typeof(IEventSourcedRepository<>), typeof(EventStoreRepository<>));

            return services;
        }
    }
}
