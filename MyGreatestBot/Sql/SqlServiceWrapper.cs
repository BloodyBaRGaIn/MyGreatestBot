using MyGreatestBot.ConfigStructs;
using MyGreatestBot.Utils;
using System;
using System.Linq;
using System.Runtime.Versioning;
using System.ServiceProcess;

namespace MyGreatestBot.Sql
{
    [SupportedOSPlatform("windows")]
    internal static class SqlServiceWrapper
    {
        private static readonly SqlServiceConfigJSON config;
        private static readonly TimeSpan timeout = TimeSpan.FromSeconds(10);

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
            try
            {
                using ServiceController service = new(name);

                switch (service.Status)
                {
                    case ServiceControllerStatus.Running:
                        return;

                    case ServiceControllerStatus.StartPending:
                    case ServiceControllerStatus.ContinuePending:
                        break;

                    case ServiceControllerStatus.Stopped:
                    case ServiceControllerStatus.StopPending:
                        if (service.Status == ServiceControllerStatus.StopPending)
                        {
                            service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
                        }
                        if (service.Status != ServiceControllerStatus.Stopped)
                        {
                            throw new ApplicationException($"Service {name} timeout");
                        }

                        if (arguments == null || !arguments.Any(a => !string.IsNullOrWhiteSpace(a)))
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
                        {
                            service.WaitForStatus(ServiceControllerStatus.Paused, timeout);
                        }
                        if (service.Status != ServiceControllerStatus.Paused)
                        {
                            throw new ApplicationException($"Service {name} timeout");
                        }

                        service.Continue();
                        break;
                }

                service.WaitForStatus(ServiceControllerStatus.Running, timeout);
                if (service.Status != ServiceControllerStatus.Running)
                {
                    throw new ApplicationException($"Service {name} timeout");
                }
            }
            catch (Exception ex)
            {
                throw new ApiClasses.SqlApiException("Cannot run service", ex);
            }
        }
    }
}
