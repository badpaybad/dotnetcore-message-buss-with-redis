using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RedisUsage.CqrsCore.Ef
{
    public static class ConfigurationManagerExtensions
    {
        public static IConfiguration Configuration { get; private set; }

        public static void SetConfiguration(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static string GetConnectionString(string name)
        {
            TryReadConfigFile();

            var connectionString = Configuration[$"ConnectionStrings:{name}"];
            return connectionString;
        }

        private static void TryReadConfigFile()
        {
            if (Configuration == null)
            {
                var consoleFileApp = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appSettings.json");

                var builder = new ConfigurationBuilder().AddJsonFile("appSettings.json");

                if (File.Exists(consoleFileApp))
                {
                    builder.AddJsonFile(consoleFileApp);
                }

                Configuration = builder.Build();
            }
        }

        public static string GetValueByKey(string key)
        {
            TryReadConfigFile();

            var val = Configuration[key];
            return val;
        }
    }
}
