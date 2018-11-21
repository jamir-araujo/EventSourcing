using Microsoft.EntityFrameworkCore;

namespace EventSourcing.EntityFrameworkCore.SqlServer
{
    public class SqlServerEventsDbContext : EventsDbContext
    {
        public SqlServerEventsDbContext(DbContextOptions options) : base(options)
        {
        }
    }
}
