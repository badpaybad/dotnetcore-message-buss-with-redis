namespace RedisUsage.CqrsCore
{
    public interface ICommandHandle<T> : ICqrsHandle where T : ICommand
    {
        ICqrsEventSourcingRepository Repository { get; }

        void Handle(T c);
    }
}
