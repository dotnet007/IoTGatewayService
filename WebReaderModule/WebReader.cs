using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.IoT.Gateway;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net;

namespace WebReaderModule
{
    public class WebReader : IGatewayModule, IGatewayModuleStart
    {
        private Broker broker;
        private int milliseconds;
        private JToken signals;
        public void Create(Broker broker, byte[] configuration)
        {
            this.broker = broker;
            string config = Encoding.UTF8.GetString(configuration);
            this.ReadConfiguration(config);
        }

        private void ReadConfiguration(string configuration)
        {
            JObject jsonConfig = JObject.Parse(File.ReadAllText(configuration));
            this.milliseconds = int.Parse((string)jsonConfig["MillisecondsInterval"]);
            this.signals = jsonConfig["signals"];
        }

        public void Destroy()
        {
            //Send to Logger Module a message saying tha module was destroyed
            Dictionary<string, string> thisIsMyProperty = new Dictionary<string, string>();
            thisIsMyProperty.Add("source", "WebReader");
            thisIsMyProperty.Add("type", "Log");
            Message messageToPublish = new Message("WebReader Module Destroyed.", thisIsMyProperty);
            this.broker.Publish(messageToPublish);
        }

        public void Receive(Message received_message)
        {
            //Just Ignore the Message. WebReader do not need to receive messages
        }

        public void Start()
        {
            Thread oThread = new Thread(new ThreadStart(this.threadBody));
            // Start the thread
            oThread.Start();
        }

        public void threadBody()
        {
            while (true)
            {
                foreach (var signal in signals)
                {
                    Message messageToPublish = this.BuildMessage(signal);
                    if (messageToPublish != null)
                        this.broker.Publish(messageToPublish);
                }

                //Publish a message every X milliseconds based on config. 
                Thread.Sleep(this.milliseconds);
            }
        }

        private Message BuildMessage(JToken signal)
        {
            try
            {
                Dictionary<string, string> thisIsMyProperty = new Dictionary<string, string>();
                thisIsMyProperty.Add("source", "mapping");
                thisIsMyProperty.Add("deviceName", (string)signal["deviceName"]);
                thisIsMyProperty.Add("deviceKey", (string)signal["deviceKey"]);
                thisIsMyProperty.Add("signal", (string)signal["name"]);
                thisIsMyProperty.Add("type", "signal");
                JObject restApi = JObject.Parse(this.CallRestApi((string)signal["url"]));
                return new Message(this.BuildJSonToSend((string)signal["tags"], restApi), thisIsMyProperty);
            }
            catch (Exception ex)
            {
                this.LogError(ex);
                return null;
            }
}

        private string BuildJSonToSend(string tags, JObject json)
        {
            JObject job = new JObject();            
            foreach (string tag in tags.Split(','))
            {
                job.Add(new JProperty(tag, json[tag]));
            }
            job.Add(new JProperty("GatewayTime", DateTime.Now));
            //job.Add(new JProperty("SourceId", 1));
            return job.ToString();
        }

        private string CallRestApi(string url)
        {
            //Call the REST Api from Url
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => { return true; };
            WebClient client = new WebClient();            
            return client.DownloadString(url);
        }

        private void LogError(Exception ex)
        {
            //Get filename
            JObject jsonConfig = JObject.Parse(File.ReadAllText("Configuration.json"));
            string filePath = (string)jsonConfig["modules"].First["args"]["dotnet_module_args"];
            //Write to textfile
            string str = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + " Error: " + ex.Message + " - " + ex.StackTrace;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(str);
            Console.ForegroundColor = ConsoleColor.Gray;
            File.AppendAllLines(filePath, new List<string>() { str });
        }
    }
}
