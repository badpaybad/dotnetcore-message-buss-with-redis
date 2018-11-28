using RedisUsage.Commands;
using RedisUsage.RedisServices;
using System;
using System.Collections.Generic;
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
            RedisServices.RedisServices.Init("192.168.15.188", null, "Du@211284");
            Console.WriteLine(RedisServices.RedisServices.Ping());

            while (!appStop)
            {
                var cmd = Console.ReadLine() ?? string.Empty;

                switch (cmd.ToLower())
                {
                    case "quit":
                        lock (locker)
                        {
                            appStop = true;
                        }
                        return;
                        break;
                    case "push-queue":
                        Console.WriteLine("Enter message:");
                        var msg1 = Console.ReadLine();
                        for (var i = 0; i < 5; i++)
                        {
                            MessageBussServices.Publish<SampleTest>(new SampleTest
                            {
                                Message = i + ". " + msg1,
                                CreatedDate = DateTime.Now
                            }, ProcessType.Queue);
                        }
                        break;
                    case "push-stack":
                        Console.WriteLine("Enter message:");
                        var msg2 = Console.ReadLine();
                        for (var i = 0; i < 5; i++)
                        {
                            MessageBussServices.Publish<SampleTest>(new SampleTest
                            {
                                Message = i + ". " + msg2,
                                CreatedDate = DateTime.Now
                            }, ProcessType.Stack);
                        }

                        break;
                    case "push":
                        Console.WriteLine("Enter message:");
                        var msg = Console.ReadLine();
                        MessageBussServices.Publish<SampleTest>(new SampleTest
                        {
                            Message = msg,
                            CreatedDate = DateTime.Now
                        });
                        break;
                    case "push-mad":
                        lock (locker)
                        {
                            madStop = false;
                        }
                        Console.WriteLine("Enter message:");
                        var msg3 = Console.ReadLine();
                        new Thread(() =>
                        {
                            while (!madStop)
                            {
                                MessageBussServices.Publish<SampleTest>(new SampleTest
                                {
                                    Message = msg3,
                                    CreatedDate = DateTime.Now
                                });
                                Thread.Sleep(1000);
                            }
                        }).Start();

                        break;
                    case "stop-mad":
                        lock (locker)
                        {
                            madStop = true;
                        }
                        break;
                    default:
                        HelpGuider();
                        break;
                }

            }

            Console.ReadLine();
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
            Console.WriteLine(help.ToString());
        }
    }


}
