using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.IoT.Gateway;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Threading;
using System.Net;

namespace WeatherModule
{
    public class Weather : IGatewayModule, IGatewayModuleStart
    {
        private Broker broker;
        private int milliseconds;
        private string deviceName;
        private string deviceKey;
        private string url;
        private int millisecondsCache;
        private DateTime lastRun;
        private string weatherRestCache;
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
            this.millisecondsCache = int.Parse((string)jsonConfig["MillisecondsCache"]);
            this.deviceKey = (string)jsonConfig["deviceKey"];
            this.deviceName = (string)jsonConfig["deviceName"];
            this.url = (string)jsonConfig["url"];
        }

        public void Destroy()
        {
            //Send to Logger Module a message saying tha module was destroyed
            Dictionary<string, string> thisIsMyProperty = new Dictionary<string, string>();
            thisIsMyProperty.Add("source", "Weather");
            thisIsMyProperty.Add("type", "Log");
            Message messageToPublish = new Message("Weather Module Destroyed.", thisIsMyProperty);
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
                Message messageToPublish = this.BuildMessage();
                if(messageToPublish != null)
                    this.broker.Publish(messageToPublish);
                
                //Publish a message every X milliseconds based on config. 
                Thread.Sleep(this.milliseconds);
            }
        }

        private Message BuildMessage()
        {
            try
            {
                Dictionary<string, string> thisIsMyProperty = new Dictionary<string, string>();
                thisIsMyProperty.Add("source", "mapping");
                thisIsMyProperty.Add("deviceName", this.deviceName);
                thisIsMyProperty.Add("deviceKey", this.deviceKey);
                thisIsMyProperty.Add("signal", "Weather");
                thisIsMyProperty.Add("type", "signal");
                if (this.weatherRestCache == null || this.lastRun == null || (DateTime.Now - this.lastRun).Seconds >= this.millisecondsCache)
                {                
                    this.weatherRestCache = this.CallRestApi(this.url);
                    this.lastRun = DateTime.Now;                
                }
                JObject restApi = JObject.Parse(this.weatherRestCache);
                return new Message(this.BuildJSonToSend(restApi), thisIsMyProperty);
            }
            catch (Exception ex)
            {
                this.weatherRestCache = null;
                this.LogError(ex);
                return null;
            }
            
        }

        private string BuildJSonToSend(JObject json)
        {
            float tempOri = (float)json["main"]["temp"];
            //must to divide in 10
            tempOri = tempOri / 10;
            JObject job = new JObject(new JProperty("Temperature", tempOri));
            job.Add(new JProperty("GatewayTime", DateTime.Now));
            //job.Add(new JProperty("SourceId", 1));
            return job.ToString();
        }

        private string CallRestApi(string url)
        {
            //Call the REST Api from Url
            WebClient client = new WebClient();
            return client.DownloadString(url);
        }

        private void LogError(Exception ex)
        {
            //Get filename
            JObject jsonConfig = JObject.Parse(File.ReadAllText("Configuration.json"));
            string filePath = (string)jsonConfig["modules"].First["args"]["dotnet_module_args"];
            //Write to textfile
            string str = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + " Error: "  + ex.Message + " - " + ex.StackTrace;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(str);
            Console.ForegroundColor = ConsoleColor.Gray;
            File.AppendAllLines(filePath, new List<string>() { str });            
        }
    }
}
