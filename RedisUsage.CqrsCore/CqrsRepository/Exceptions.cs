using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace RedisUsage.CqrsCore.CqrsRepository
{
    public class AggregateConflickVersionException : Exception
    {
        public AggregateConflickVersionException()
        {
        }

        public AggregateConflickVersionException(string message) : base(message)
        {
        }

        public AggregateConflickVersionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected AggregateConflickVersionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    public class AggregateHistoryBuilderException : Exception
    {
        public AggregateHistoryBuilderException()
        {
        }

        public AggregateHistoryBuilderException(string message) : base(message)
        {
        }

        public AggregateHistoryBuilderException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected AggregateHistoryBuilderException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    public class AggregateNotFoundException : Exception
    {
        public AggregateNotFoundException()
        {
        }

        public AggregateNotFoundException(string message) : base(message)
        {
        }

        public AggregateNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected AggregateNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
