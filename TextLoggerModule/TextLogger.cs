using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.IoT.Gateway;
using Newtonsoft.Json;
using System.IO;

namespace TextLoggerModule
{
    public class TextLogger : IGatewayModule
    {
        private string filePath;
        public void Create(Broker broker, byte[] configuration)
        {
            string conf = Encoding.UTF8.GetString(configuration);
            this.ReadConfiguration(conf);
        }

        private void ReadConfiguration(string configuration)
        {
            filePath = configuration;
        }

        public void Destroy()
        {
            //Do nothing, this is the last module to destroy
        }

        public void Receive(Message received_message)
        {
            string propString = " - ";
            foreach (var prop in received_message.Properties)
            {
                propString += prop.Key + ":" + prop.Value + " - ";
            }
            this.LogToText(propString, Encoding.UTF8.GetString(received_message.Content));
        }

        private void LogToText(string properties, string text)
        {
            string str = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + properties + text;
            File.AppendAllLines(this.filePath, new List<string>() { str });            
        }
    }
}
