using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GW = Microsoft.Azure.IoT.Gateway;
using Newtonsoft.Json;
using IoT = Microsoft.Azure.Devices.Client;

namespace IoTHubConnectorModule
{
    public class IoTHubConnector : GW.IGatewayModule
    {
        private GW.Broker broker;
        private IoT.DeviceClient client;
        public void Create(GW.Broker broker, byte[] configuration)
        {
            this.broker = broker;
            //ReadConfig
            //Init DeviceClient
            client = IoT.DeviceClient.CreateFromConnectionString(Encoding.UTF8.GetString(configuration));
        }

        public void Destroy()
        {
            //Send to Logger Module a message saying tha module was destroyed
            Dictionary<string, string> thisIsMyProperty = new Dictionary<string, string>();
            thisIsMyProperty.Add("source", "IoTHubConnector");
            thisIsMyProperty.Add("type", "Log");
            GW.Message messageToPublish = new GW.Message("IoTHubConnector Module Destroyed.", thisIsMyProperty);
            this.broker.Publish(messageToPublish);
        }

        public void Receive(GW.Message received_message)
        {
            //ToDo: Identify signal as device
            this.SendDeviceToCloudMessagesAsync(received_message.Content);
        }

        private async void SendDeviceToCloudMessagesAsync(byte[] messageBytes)
        {
            var message = new IoT.Message(messageBytes);
            await this.client.SendEventAsync(message);
        }


    }
}
