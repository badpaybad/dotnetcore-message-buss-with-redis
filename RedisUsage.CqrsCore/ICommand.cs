using System;

namespace RedisUsage.CqrsCore
{
    public interface ICommand
    {
        Guid? PublishedCommandId { get; set; }

        string TokenSession { get; set; }
    }
}
