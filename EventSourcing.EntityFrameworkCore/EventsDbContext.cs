using System;
using Microsoft.EntityFrameworkCore;

namespace EventSourcing.EntityFrameworkCore
{
    public class EventsDbContext : DbContext
    {
        public DbSet<StoredEvent> Events { get; set; }

        public EventsDbContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StoredEvent>(entity =>
            {
                entity.HasKey(nameof(StoredEvent.AggregateId), nameof(StoredEvent.Version));
                entity.HasIndex(nameof(StoredEvent.AggregateId), nameof(StoredEvent.AggregateType));

                entity.Property(e => e.AggregateId).IsRequired(true);
                entity.Property(e => e.AggregateType).IsRequired(true);
                entity.Property(e => e.Body).IsRequired(true);
                entity.Property(e => e.EventType).IsRequired(true);
                entity.Property(e => e.Version).IsRequired(true);
                entity.Property(e => e.Metadata).IsRequired(false);
            });

            base.OnModelCreating(modelBuilder);
        }
    }

    public class StoredEvent
    {
        public Guid AggregateId { get; private set; }
        public string AggregateType { get; private set; }
        public string Body { get; private set; }
        public long Version { get; private set; }
        public string EventType { get; private set; }
        public string Metadata { get; private set; }

        public StoredEvent(
            Guid aggregateId,
            string aggregateType,
            string body,
            long version,
            string eventType,
            string metadata)
        {
            AggregateId = aggregateId;
            AggregateType = aggregateType;
            Body = body;
            Version = version;
            EventType = eventType;
            Metadata = metadata;
        }
    }
}
