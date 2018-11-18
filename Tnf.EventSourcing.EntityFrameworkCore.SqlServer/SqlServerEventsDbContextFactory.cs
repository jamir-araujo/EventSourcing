using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Tnf.EventSourcing.EntityFrameworkCore.SqlServer
{
    public class SqlServerEventsDbContextFactory : IDesignTimeDbContextFactory<SqlServerEventsDbContext>
    {
        public SqlServerEventsDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<EventsDbContext>();
            builder.UseSqlServer("Data Source=(localdb)\\mssqllocaldb;DataBase=EventsDatabase;MultipleActiveResultSets=true");
            return new SqlServerEventsDbContext(builder.Options);
        }

    }
}
