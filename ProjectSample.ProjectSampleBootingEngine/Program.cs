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
                        
            CommandPublisher.Instance.Send(new CreateSample(sampleId, "Version.1.0", "{}"));

            CommandPublisher.Instance.Send(new ChangeVersionOfSample(sampleId, "Version.2.0"));

            while (true)
            {
                var cmd = Console.ReadLine();

                if (cmd == "quit") return;
            }
        }
    }
}
