using System;
using System.Collections.Generic;

namespace RedisUsage.CqrsCore
{
    public interface IAggregateRoot
    {
        Guid Id { get; set; }

        void LoadFromHistory(IList<IEvent> eventsHistory);

        IList<IEvent> Changes { get; }
    }
}
