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

            //https://weblog.west-wind.com/posts/2016/Jun/06/Publishing-and-Running-ASPNET-Core-Applications-with-IIS
            //Microsoft.AspNetCore.Hosting
            //DotNetCore.1.0.0.RC2-WindowsHosting
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
