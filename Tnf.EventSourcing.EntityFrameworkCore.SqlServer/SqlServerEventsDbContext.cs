using System;
using Microsoft.EntityFrameworkCore;

namespace Tnf.EventSourcing.EntityFrameworkCore.SqlServer
{
    public class SqlServerEventsDbContext : EventsDbContext
    {
        public SqlServerEventsDbContext(DbContextOptions options) : base(options)
        {
        }
    }
}
