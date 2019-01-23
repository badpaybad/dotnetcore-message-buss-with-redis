namespace RedisUsage.CqrsCore
{
    public interface ICommandPublisher
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="isDataInStack">false: queue, true: stack, default:queue</param>
        void Send(ICommand cmd, bool isDataInStack = false);
    }
}
