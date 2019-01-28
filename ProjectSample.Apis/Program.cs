using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using RedisUsage.CqrsCore.RegisterEngine;

namespace ProjectSample.Apis
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args)
                   .UseKestrel()
            .UseContentRoot(Directory.GetCurrentDirectory())
            .UseIISIntegration()
                .Build().Run();


            CommandsAndEventsRegisterEngine.Init();

            CommandsAndEventsRegisterEngine.AutoRegisterForHandlers();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
