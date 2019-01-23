//using Newtonsoft.Json;
//using RedisUsage.Commands;
//using RedisUsage.Events;
//using RedisUsage.RedisServices;
//using System;

//namespace RedisUsage.ConsoleHandle
//{
//    class Program
//    {
//        static void Main(string[] args)
//        {
//            RedisServices.RedisServices.Init("192.168.15.188", null, "Du@211284");
//            Console.WriteLine(RedisServices.RedisServices.Ping());

//            RedisServices.MessageBussServices.Subscribe<SampleTestCommand>("console handle", (obj) =>
//            {
//                Console.WriteLine("console handle");
//                Console.WriteLine(JsonConvert.SerializeObject(obj));
//                //after process done fire event
//                MessageBussServices.Publish<SampleTestEvent>(new SampleTestEvent()
//                {
//                    Id = Guid.NewGuid()
//                ,
//                    Data = JsonConvert.SerializeObject(new
//                    {
//                        Msg = "Data processed from console handle",
//                        Cmd = obj
//                    })
//                }
//                , MessageBussServices.ProcessType.Topic);
//            });

//            RedisServices.MessageBussServices.Subscribe<SampleTestCommand>("console handle1", (cmd) =>
//            {
//                Console.WriteLine("console handle1");
//                Console.WriteLine(JsonConvert.SerializeObject(cmd));
//            });
//            RedisServices.MessageBussServices.Subscribe<SampleTestCommand>("console handle2", (cmd) =>
//            {
//                Console.WriteLine("console handle2");
//                Console.WriteLine(JsonConvert.SerializeObject(cmd));
//            });

//            // can move to other console app for consumer event
//            MessageBussServices.Subscribe<SampleTestEvent>("consumer event handler 1", (evt) =>
//            {
//                Console.WriteLine("Event published from consold handle");
//                Console.WriteLine(JsonConvert.SerializeObject(evt));
//            });
//        }
//    }
//}
