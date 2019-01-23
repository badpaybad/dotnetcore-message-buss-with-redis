using Newtonsoft.Json;
using RedisUsage.RedisServices;
using System;
using System.Linq;
using System.Text;
using System.Threading;

namespace RedisUsage.Test
{
    public static class Programs
    {
        static bool madStop = false;
        static bool appStop = false;
        static object locker = new object();

        public static void Main(params string[] args)
        {
            ////RedisServices.RedisServices.Init("192.168.15.188", null, "");
            //RedisServices.RedisServices.Init("127.0.0.1", null, "");
            //Console.WriteLine(RedisServices.RedisServices.Ping());
            //HelpGuider();
            ////MessageBussServices.Subscribe<SampleTest>("RedisUsage.Test", (data) => {
            ////    Console.WriteLine("subscribe inline console");
            ////    Console.WriteLine("Recieved to process:");
            ////    Console.WriteLine(JsonConvert.SerializeObject(data));
            ////});
            //while (!appStop)
            //{
            //    var cmd = (Console.ReadLine() ?? string.Empty).ToLower();
            //    if (cmd.Equals("quit"))
            //    {
            //        lock (locker)
            //        {
            //            appStop = true;
            //        }
            //        return;
            //    }
            //    if (cmd.IndexOf("channel-", StringComparison.OrdinalIgnoreCase) == 0)
            //    {
            //        var cmd1 = cmd.Split('-')[1];
            //        switch (cmd1)
            //        {
            //            case "all":
            //                Console.WriteLine(JsonConvert.SerializeObject(MessageBussServices.GetAllChannelInfo()));
            //                break;
            //            case "allname":
            //                Console.WriteLine(JsonConvert.SerializeObject(MessageBussServices.GetAllChannelInfo().Select(i=>i.Name)));
            //                break;
            //            default:
            //                Console.WriteLine("Enter channel name:");
            //                var cname = Console.ReadLine();
            //                switch (cmd1)
            //                {
            //                    case "info":
            //                        Console.WriteLine(JsonConvert.SerializeObject(MessageBussServices.GetChannelInfo(cname)));
            //                        break;
            //                    case "subscriber":
            //                        Console.WriteLine(JsonConvert.SerializeObject(MessageBussServices.GetSubscribers(cname)));
            //                        break;
            //                    case "statistic":
            //                        var info = MessageBussServices.GetChannelInfo(cname);
            //                        Console.WriteLine(JsonConvert.SerializeObject(new
            //                        {
            //                            info.PendingDataQueueLength,
            //                            info.SuccessDataQueueLength,
            //                            info.ErrorDataQueueLength
            //                        }));
            //                        break;
            //                    default:
            //                        HelpGuider();
            //                        break;
            //                }

            //                break;
            //        }
            //    }
            //    else
            //    {
            //        CommandUsageTest(cmd);
            //    }
            //}
           
            //Console.ReadLine();
        }

        private static void CommandUsageTest(string cmd)
        {
            //switch (cmd)
            //{
            //    case "push-queue":
            //        Console.WriteLine("Enter message:");
            //        var msg1 = Console.ReadLine();
            //        for (var i = 0; i < 5; i++)
            //        {
            //            MessageBussServices.Publish<SampleTestCommand>(new SampleTestCommand
            //            {
            //                Message = i + ". " + msg1,
            //                CreatedDate = DateTime.Now
            //            }, MessageBussServices.ProcessType.Queue);
            //        }
            //        break;
            //    case "push-stack":
            //        Console.WriteLine("Enter message:");
            //        var msg2 = Console.ReadLine();
            //        for (var i = 0; i < 5; i++)
            //        {
            //            MessageBussServices.Publish<SampleTestCommand>(new SampleTestCommand
            //            {
            //                Message = i + ". " + msg2,
            //                CreatedDate = DateTime.Now
            //            }, MessageBussServices.ProcessType.Stack);
            //        }

            //        break;
            //    case "push":
            //        Console.WriteLine("Enter message:");
            //        var msg = Console.ReadLine();
            //        MessageBussServices.Publish<SampleTestCommand>(new SampleTestCommand
            //        {
            //            Message = msg,
            //            CreatedDate = DateTime.Now
            //        });
            //        break;
            //    case "push-mad":
            //        lock (locker)
            //        {
            //            madStop = false;
            //        }
            //        Console.WriteLine("Enter message:");
            //        var msg3 = Console.ReadLine();
            //        new Thread(() =>
            //        {
            //            while (!madStop)
            //            {
            //                MessageBussServices.Publish<SampleTestCommand>(new SampleTestCommand
            //                {
            //                    Message = msg3,
            //                    CreatedDate = DateTime.Now
            //                });
            //                Thread.Sleep(1000);
            //            }
            //        }).Start();

            //        break;
            //    case "stop-mad":
            //        lock (locker)
            //        {
            //            madStop = true;
            //        }
            //        break;
            //    default:
            //        HelpGuider();
            //        break;
            //}
        }

        private static void HelpGuider()
        {
            StringBuilder help = new StringBuilder();
            help.AppendLine($"quit: to quit");
            help.AppendLine($"push: to push once");
            help.AppendLine($"push-queue: to push queue");
            help.AppendLine($"push-stack: to push stack");
            help.AppendLine($"push-mad: to push mad");
            help.AppendLine($"stop-mad: to stop mad");

            help.AppendLine($"channel-allname: to list all name of channel");
            help.AppendLine($"channel-all: to list all channel");
            help.AppendLine($"channel-info: to get info channel");
            help.AppendLine($"channel-subscriber: to get all subscriber of channel");
            help.AppendLine($"channel-statistic: to get queue data length of channel");

            Console.WriteLine(help.ToString());
        }
    }


}
