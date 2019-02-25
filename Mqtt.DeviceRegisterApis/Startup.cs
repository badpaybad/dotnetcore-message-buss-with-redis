using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RedisUsage.CqrsCore.Ef;

namespace Mqtt.DeviceRegisterApis
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            var redisHost = ConfigurationManagerExtensions.GetValueByKey("Redis:Host") ?? "127.0.0.1";
            var redisPort = ConfigurationManagerExtensions.GetValueByKey("Redis:Port") ?? "6379";
            var redisPwd = ConfigurationManagerExtensions.GetValueByKey("Redis:Password") ?? string.Empty;
            int? redisPortInt = null;
            if (!string.IsNullOrEmpty(redisPort))
            {
                redisPortInt = int.Parse(redisPort);
            }

            RedisUsage.RedisServices.RedisServices.Init(redisHost, redisPortInt, redisPwd);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
