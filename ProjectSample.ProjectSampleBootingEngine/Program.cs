using Microsoft.Extensions.Configuration;
using ProjectSample.CommandsAndEvents;
using RedisUsage.CqrsCore.Ddd;
using RedisUsage.CqrsCore.RegisterEngine;
using RedisUsage.RedisServices;
using System;

namespace ProjectSample.ProjectSampleBootingEngine
{
    class Program
    {
        static void Main(string[] args)
        {
            CommandsAndEventsRegisterEngine.Init();            
         
            CommandsAndEventsRegisterEngine.AutoRegisterForHandlers();

            Console.WriteLine("Try to create sample data");
            Guid sampleId = Guid.NewGuid();
            Random rnd = new Random();

            while (true)
            {
                Console.WriteLine("--- Menu:Begin ---");
                Console.WriteLine("Type 'create' to create new");
                Console.WriteLine("Type 'update' to update with latest create Id with random version");
                Console.WriteLine("Type 'quit' to close console");
                Console.WriteLine("--- Menu:End ---");

                var cmd = Console.ReadLine();

                if (cmd == "quit") {
                    Environment.Exit(0);
                    return;
                }

                if (cmd == "create")
                {
                    sampleId = Guid.NewGuid();
                    CommandPublisher.Instance.Send(new CreateSample(sampleId, "Version.1.0", "{}"));
                }
                
                if (cmd == "update")
                {
                    var v = rnd.Next(1,100);
                    CommandPublisher.Instance.Send(new ChangeVersionOfSample(sampleId, $"Version.{v}.0"));
                }
            }
        }
    }
}
