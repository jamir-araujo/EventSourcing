using Microsoft.Extensions.DependencyInjection;

namespace EventSourcing.EntityFrameworkCore
{
    public static class EventSourcingEFCoreServiceCollectionExtensions
    {
        public static IServiceCollection AddTnfEventSourcingEFCore(IServiceCollection services)
        {
            return services;
        }
    }
}
