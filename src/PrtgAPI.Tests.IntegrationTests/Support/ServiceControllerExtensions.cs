using System;
using System.ServiceProcess;

namespace PrtgAPI.Tests.IntegrationTests.Support
{
    internal static class ServiceControllerExtensions
    {
        internal static void StartAndWaitForStatus(this ServiceController service, ServiceControllerStatus status = ServiceControllerStatus.Running)
        {
            var timeout = TimeSpan.FromSeconds(10);

            service.Start();

            for (var i = 0; i < 10; i++)
            {
                try
                {
                    service.WaitForStatus(status, timeout);

                    return;
                }
                catch
                {
                    if (service.Status == ServiceControllerStatus.Stopped)
                    {
                        service = new ServiceController(service.ServiceName, service.MachineName);
                        service.Start();
                    }
                }
            }

            throw new InvalidOperationException($"Failed to start service '{service.ServiceName}': service doesn't want to start");
        }

        internal static void StopAndWaitForStatus(this ServiceController service, ServiceControllerStatus status = ServiceControllerStatus.Stopped)
        {
            var timeout = TimeSpan.FromSeconds(10);

            service.Stop();

            for (var i = 0; i < 10; i++)
            {
                try
                {
                    service.WaitForStatus(status, timeout);

                    return;
                }
                catch
                {
                    if (service.Status == ServiceControllerStatus.Running)
                    {
                        service = new ServiceController(service.ServiceName, service.MachineName);
                        service.Stop();
                    }
                }
            }

            throw new InvalidOperationException($"Failed to start service '{service.ServiceName}': service doesn't want to start");
        }
    }
}
