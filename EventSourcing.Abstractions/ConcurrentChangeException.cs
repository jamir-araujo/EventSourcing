using System;

namespace EventSourcing
{
    [Serializable]
    public class ConcurrentChangeException : Exception
    {
        private Guid _id;
        private string _aggregateType;

        public ConcurrentChangeException(Guid id, string aggregateType)
        {
            _id = id;
            _aggregateType = aggregateType;
        }

        public ConcurrentChangeException(Guid id, string aggregateType, Exception exception)
            : base(exception.Message, exception)
        {
            _id = id;
            _aggregateType = aggregateType;
        }
    }
}
