using System;

namespace RedisUsage.CqrsCore
{
    public interface IEvent
    {
        /// <summary>
        /// be long to eventsource
        /// </summary>
        Guid? PublishedEventId { get; set; }
        /// <summary>
        /// be long to eventsource
        /// </summary>
        long Version { get; set; }
    }
}
