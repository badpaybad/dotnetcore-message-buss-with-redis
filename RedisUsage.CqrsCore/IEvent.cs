using System;

namespace RedisUsage.CqrsCore
{
    public interface IEvent
    {
        Guid? PublishedEventId { get; set; }
        long Version { get; set; }
    }
}
