using HSDT.AutoSync;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudSync
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Start process loader.
            ProcessLoader.Load("CloudSync.dll");
            // Start web host.
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            })
            .UseNLog(new NLogAspNetCoreOptions()
            {
                RemoveLoggerFactoryFilter = false
            });
    }
}
