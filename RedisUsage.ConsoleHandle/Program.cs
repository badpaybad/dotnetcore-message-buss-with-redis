using Newtonsoft.Json;
using RedisUsage.Commands;
using System;

namespace RedisUsage.ConsoleHandle
{
    class Program
    {
        static void Main(string[] args)
        {
            RedisServices.RedisServices.Init("192.168.15.188", null, "Du@211284");
            Console.WriteLine(RedisServices.RedisServices.Ping());

            RedisServices.MessageBussServices.Subscribe<SampleTest>("console handle", (obj) =>
            {
                Console.WriteLine("console handle");
                Console.WriteLine(JsonConvert.SerializeObject(obj));
            });

            RedisServices.MessageBussServices.Subscribe<SampleTest>("console handle1", (obj) =>
            {
                Console.WriteLine("console handle1");
                Console.WriteLine(JsonConvert.SerializeObject(obj));
            });
            RedisServices.MessageBussServices.Subscribe<SampleTest>("console handle2", (obj) =>
            {
                Console.WriteLine("console handle2");
                Console.WriteLine(JsonConvert.SerializeObject(obj));
            });
        }
    }
}
