using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Xunit;

namespace Tnf.EventSourcing.EventStore.Tests
{
    public class EventStoreRepositoryTests
    {
        private EventStoreRepository<Cash> _repository;

        public EventStoreRepositoryTests()
        {
            var options = Options.Create(new EventStoreOptions { ConnectionString = "ConnectTo=tcp://test:1234@localhost:1113" });
            _repository = new EventStoreRepository<Cash>(options, null);
        }

        [Fact]
        public async Task GetAsync_Should_Throw_When_AggregateDoesNotHaveAndEmptyConstructor()
        {
            await Assert.ThrowsAsync<MissingMethodException>(() => _repository.GetAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task SaveAndGet()
        {
            var id = Guid.NewGuid();
            var cash = new Cash(id, 1000);

            await _repository.SaveAsync(cash);

            var savedCash = await _repository.GetAsync(id);
            Assert.Equal(id, savedCash.Id);
            Assert.Equal(cash.Version, savedCash.Version);
            Assert.Equal(cash.Limit, savedCash.Limit);
        }

        public class Cash : EventSourcedAggregateRoot
        {
            public double Limit { get; private set; }

            public Cash(Guid id) : base(id)
            {
                Handle<CashCreated>(Apply);
            }

            public Cash(Guid id, double limit) : this(id)
            {
                Update(new CashCreated(limit));
            }

            private void Apply(CashCreated cashCreated)
            {
                Limit = cashCreated.Limit;
            }
        }

        public class CashCreated : VersionedEvent
        {
            public double Limit { get; set; }

            public CashCreated(double limit)
            {
                Limit = limit;
            }
        }
    }
}
