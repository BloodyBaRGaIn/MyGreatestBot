using DicordNET.Config;
using System.Runtime.Versioning;
using System.ServiceProcess;

namespace DicordNET.Sql
{
    [SupportedOSPlatform("windows")]
    internal static class SqlServiceWrapper
    {
        private static readonly SqlServiceConfigJSON config;

        static SqlServiceWrapper()
        {
            config = ConfigManager.GetSqlServiceConfigJSON();
        }

        internal static void Run()
        {
            RunService(config.ServerServiceName, config.ServerServiceArgument);
            RunService(config.BrowserServiceName);
        }

        private static void RunService(string name, params string[] arguments)
        {
            using ServiceController service = new(name);

            switch (service.Status)
            {
                case ServiceControllerStatus.Stopped:
                case ServiceControllerStatus.StopPending:
                    if (service.Status == ServiceControllerStatus.StopPending)
                        service.WaitForStatus(ServiceControllerStatus.Stopped);

                    if (arguments == null || !arguments.Any())
                    {
                        service.Start();
                    }
                    else
                    {
                        service.Start(arguments);
                    }

                    break;

                case ServiceControllerStatus.Paused:
                case ServiceControllerStatus.PausePending:
                    if (service.Status == ServiceControllerStatus.PausePending)
                        service.WaitForStatus(ServiceControllerStatus.Paused);

                    service.Continue();
                    break;
            }

            service.WaitForStatus(ServiceControllerStatus.Running);
        }
    }
}
