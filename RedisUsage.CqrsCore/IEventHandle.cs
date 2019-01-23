namespace RedisUsage.CqrsCore
{
    public interface IEventHandle<T> : ICqrsHandle where T : IEvent
    {
        void Handle(T e);
    }
}
