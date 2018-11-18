using System;
using System.Collections.Generic;
using System.Text;

namespace Tnf.EventSourcing.EventStore
{
    public class EventStoreOptions
    {
        public string ConnectionString { get; set; }
        public int ReadingBlockSize { get; set; } = 100;
    }
}
