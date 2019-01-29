using System;
using System.Collections.Generic;
using System.Text;

namespace RedisUsage.CqrsCore.CqrsRepository
{
    public class EventPublisher : IEventPublisher
    {
        static EventPublisher _instance = new EventPublisher();

        static EventPublisher()
        {
            _instance = _instance ?? new EventPublisher();
        }

        //private EventPublisher() can be private constructor for singleton instance
        public EventPublisher()
        {

        }

        public IEventPublisher Instance { get { return _instance; } }

        public void Publish(IEvent e)
        {
            if (e.PublishedEventId == null || e.PublishedEventId == Guid.Empty)
            {
                e.PublishedEventId = Guid.NewGuid();
            }

            RedisServices.MessageBussServices.Publish(e, RedisServices.MessageBussServices.ProcessType.Topic);
        }
    }
}
