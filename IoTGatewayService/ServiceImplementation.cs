using IoTGatewayService.Framework;
using System.ServiceProcess;
using System.Diagnostics;
using System;

namespace IoTGatewayService
{
    /// <summary>
    /// The actual implementation of the windows service goes here...
    /// </summary>
    [WindowsService("IoTGatewayService",
        DisplayName = "IoTGatewayService",
        Description = "Windows service that implements the Azure IoT Gateway.",
        EventLogSource = "IoTGatewayService",
        StartMode = ServiceStartMode.Automatic)]
    public class ServiceImplementation : IWindowsService
    {
        Process exeProccess;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
        }

        /// <summary>
        /// This method is called when the service gets a request to start.
        /// </summary>
        /// <param name="args">Any command line arguments</param>
        public void OnStart(string[] args)
        {
            //Run Azure IoT Gateway
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;
            startInfo.FileName = "dotnet_binding_sample.exe";
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.Arguments = "Configuration.json";

            try
            {
                exeProccess = Process.Start(startInfo);
                
            }
            catch (Exception ex)
            {
                EventLog log = new EventLog();
                log.WriteEntry("Error executing IoT Gateway: " +  ex.Message + " - " + ex.StackTrace, EventLogEntryType.Error);
            }
        }

        /// <summary>
        /// This method is called when the service gets a request to stop.
        /// </summary>
        public void OnStop()
        {
            exeProccess.Close();
        }

        /// <summary>
        /// This method is called when a service gets a request to pause,
        /// but not stop completely.
        /// </summary>
        public void OnPause()
        {
        }

        /// <summary>
        /// This method is called when a service gets a request to resume 
        /// after a pause is issued.
        /// </summary>
        public void OnContinue()
        {
        }

        /// <summary>
        /// This method is called when the machine the service is running on
        /// is being shutdown.
        /// </summary>
        public void OnShutdown()
        {
            exeProccess.Kill();
        }

        /// <summary>
        /// This method is called when a custom command is issued to the service.
        /// </summary>
        /// <param name="command">The command identifier to execute.</param >
        public void OnCustomCommand(int command)
        {
        }
    }
}
