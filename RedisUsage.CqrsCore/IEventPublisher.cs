namespace RedisUsage.CqrsCore
{
    public interface IEventPublisher
    {
        void Publish(IEvent e);
    }
}
