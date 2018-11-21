using System;
using System.Threading.Tasks;

namespace EventSourcing
{
    public interface IEventSourcedRepository<TAggregateRoot>
        where TAggregateRoot : IEventSourcedAggregateRoot
    {
        Task<TAggregateRoot> GetAsync(Guid id);
        Task SaveAsync(TAggregateRoot aggregate);
    }
}
