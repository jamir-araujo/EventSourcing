using System;
using System.Threading.Tasks;
using EventSourcing;
using EventSourcing.EntityFrameworkCore;
using EventSourcing.EntityFrameworkCore.SqlServer;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Tnf.EventSourcing.EntityFrameworkCore.Tests
{
    public class EventSourcedEFCoreRepositoryTests
    {
        private EventSourcedEFCoreRepository<Cash> _repository;

        public EventSourcedEFCoreRepositoryTests()
        {
            var builder = new DbContextOptionsBuilder<SqlServerEventsDbContext>();
            builder.UseSqlServer("Data Source=(localdb)\\mssqllocaldb;DataBase=EventsDatabase");

            _repository = new EventSourcedEFCoreRepository<Cash>(new SqlServerEventsDbContext(builder.Options));
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

        [Fact]
        public async Task LoadInBlocks()
        {
            var id = Guid.NewGuid();
            var cash = new Cash(id, 10000);

            cash.Deposit(100);
            cash.Deposit(100);
            cash.Deposit(100);
            cash.Deposit(100);
            cash.Deposit(100);
            cash.Deposit(100);

            await _repository.SaveAsync(cash);

            var savedCash = await _repository.GetAsync(id);

            Assert.Equal(600, cash.Balance);
            Assert.Equal(600, savedCash.Balance);
        }

        [Fact]
        public async Task Should_Throw_ConcurrencyException()
        {
            var id = Guid.NewGuid();
            var cash = new Cash(id, 10000);

            cash.Deposit(100);
            cash.Deposit(100);

            await _repository.SaveAsync(cash);

            var cash1 = await _repository.GetAsync(id);
            var cash2 = await _repository.GetAsync(id);

            cash1.Withdraw(100);

            await _repository.SaveAsync(cash1);

            cash2.Deposit(100);

            await Assert.ThrowsAsync<ConcurrentChangeException>(() => _repository.SaveAsync(cash2));
        }
    }
}
