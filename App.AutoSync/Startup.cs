using HSDT.AutoSync.Process;
using HSDT.AutoSync;
using HSDT.Common;
using Topshelf;

namespace App.AutoSync
{
    public class Startup : ServiceControl
    {
        public bool Start(HostControl hostControl)
        {
            NLogger.GetLogger().Info("System is starting ...");

            // Load all processes
            ProcessLoader.Load();
            CronJobProcess.Start();

            NLogger.GetLogger().Info("System is started!");
            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            try
            {
                ProcessLoader.Stop();
                CronJobProcess.Stop();
                return true;
            }
            catch (Exception ex)
            {
                NLogger.GetLogger().Error(ex, "System is stopped!");
                return true;
            }
        }
    }
}
