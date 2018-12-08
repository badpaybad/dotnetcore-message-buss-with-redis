using System;

namespace RedisUsage.Commands
{
    public class SampleTestCommand
    {
        public DateTime CreatedDate { get; set; }
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Message { get; set; }
    }
}
