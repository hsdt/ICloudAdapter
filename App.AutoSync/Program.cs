using Topshelf;

namespace App.AutoSync
{
    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                x.Service<Startup>();
                x.EnableServiceRecovery(r => r.RestartService(TimeSpan.FromSeconds(10)));
                x.SetServiceName("AutoSync");
                x.StartAutomatically();
                x.RunAsLocalSystem();
                x.UseNLog();
            });
        }
    }
}
