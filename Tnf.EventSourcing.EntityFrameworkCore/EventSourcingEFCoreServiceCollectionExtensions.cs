using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace Tnf.EventSourcing.EntityFrameworkCore
{
    public static class EventSourcingEFCoreServiceCollectionExtensions
    {
        public static IServiceCollection AddTnfEventSourcingEFCore(IServiceCollection services)
        {
            return services;
        }
    }
}
