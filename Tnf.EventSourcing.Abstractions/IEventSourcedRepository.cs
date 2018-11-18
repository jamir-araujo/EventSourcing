using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Tnf.EventSourcing.Abstractions
{
    public interface IEventSourcedRepository<TAggregateRoot>
        where TAggregateRoot : IEventSourcedAggregateRoot
    {
        Task<TAggregateRoot> GetAsync(Guid id);
        Task SaveAsync(TAggregateRoot aggregate);
    }
}
