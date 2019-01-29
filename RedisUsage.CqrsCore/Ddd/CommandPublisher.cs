using System;
using System.Collections.Generic;
using System.Text;

namespace RedisUsage.CqrsCore.Ddd
{
    public class CommandPublisher : ICommandPublisher
    {
        static CommandPublisher _instance = new CommandPublisher();

        static CommandPublisher()
        {
            _instance = _instance ?? new CommandPublisher();
        }

        //private CommandPublisher() can be private constructor for singleton instance
        public CommandPublisher()
        {

        }

        public static ICommandPublisher Instance { get { return _instance; } }


        public void Send(ICommand cmd, bool isDataInStack = false)
        {
            RedisServices.MessageBussServices.ProcessType type = RedisServices.MessageBussServices.ProcessType.Queue;

            if (isDataInStack)
            {
                type = RedisServices.MessageBussServices.ProcessType.Stack;
            }

            if (cmd.PublishedCommandId == null || cmd.PublishedCommandId == Guid.Empty)
            {
                cmd.PublishedCommandId = Guid.NewGuid();
            }
            
            //data to process command should not in topic, (topic for events)
            RedisServices.MessageBussServices.Publish(cmd, type);
        }
    }
}
