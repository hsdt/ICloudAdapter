using HSDT.AutoSync;
using HSDT.Common;
using HSDT.Common.Helper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NLog.Web;
using System;
using Topshelf;
using Topshelf.Extensions.Hosting;
using Host = Microsoft.Extensions.Hosting.Host;

namespace CloudSync
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // 01. create web host.
            CreateHostBuilder(args)
            // 02. set base path for application
            .UseContentRoot(EnvHelper.RootDirectory)
            // 03. register windows service.
            .RunAsTopshelfService(hc =>
            {
                hc.SetServiceName("ICloudSync");
                hc.SetDisplayName("ICloudSync");
                hc.SetDescription("ICloudSync Adapter WebHost");
                hc.EnableServiceRecovery(rs => rs.RestartService(TimeSpan.FromSeconds(10)));
                hc.RunAsLocalSystem();
                hc.StartAutomatically();
                hc.UseNLog();
            }, hc =>
            {
                // Start embedded-process loader.
                ProcessLoader.Load("CloudSync.dll");
            }, hc =>
            {
                // Stop all AutoSync Background services.
                ProcessLoader.Stop();
            });
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            })
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                var logger = NLogger.GetLogger("info");
                var hostingEnv = hostingContext.HostingEnvironment.EnvironmentName;
                var contentRootPath = hostingContext.HostingEnvironment.ContentRootPath;
                var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                logger.Info($"Read config file from: {contentRootPath}, ENV={hostingEnv}_{environmentName}");
                config
                    .SetBasePath(contentRootPath)
                    .AddJsonFile("appsettings.json", true, true)
                    .AddJsonFile($"appsettings.{hostingEnv}.json", true, true)
                    .AddEnvironmentVariables();
            })
            .UseNLog(new NLogAspNetCoreOptions()
            {
                RemoveLoggerFactoryFilter = false
            });
    }
}
