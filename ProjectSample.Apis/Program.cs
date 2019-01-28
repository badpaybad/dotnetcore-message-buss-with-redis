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
            .Build()
            .Run();
            //https://stackify.com/how-to-deploy-asp-net-core-to-iis/
            //https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/iis/?view=aspnetcore-2.2

            //https://www.microsoft.com/net/permalink/dotnetcore-current-windows-runtime-bundle-installer

        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
