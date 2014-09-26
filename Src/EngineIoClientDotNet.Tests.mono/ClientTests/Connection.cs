using Newtonsoft.Json;
using Quobject.EngineIoClientDotNet.Client;
using Quobject.EngineIoClientDotNet.Modules;
using System.IO;


namespace Quobject.EngineIoClientDotNet_Tests.ClientTests
{
    public class Connection
    {
        public static readonly int TIMEOUT = 300000;

        static Connection()
        {
            LogManager.SetupLogManager();
        }

        protected Socket.Options CreateOptions()
        {            
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());


            var config = ConfigBase.Load();
            var options = new Socket.Options();
            options.Port = config.server.port;
            options.Hostname = config.server.hostname;
            log.Info("Please add to your hosts file: 127.0.0.1 " + options.Hostname);

            return options;
        }


        protected Socket.Options CreateOptionsSecure()
        {            
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());

            var config = ConfigBase.Load();
            var options = new Socket.Options();
            options.Port = config.server.ssl_port;
            options.Hostname = config.server.hostname;
            log.Info("Please add to your hosts file: 127.0.0.1 " + options.Hostname);
            options.Secure = true;
            options.IgnoreServerCertificateValidation = true;
            return options;
        }
    }

    public class ConfigBase
    {
        public string version { get; set; }
        public ConfigServer server { get; set; }

        public static ConfigBase Load()
        {
            var configString = File.ReadAllText("./../../../../grunt/config.json");

            var config = JsonConvert.DeserializeObject<ConfigBase>(configString);
            return config;
        }
    }

    public class ConfigServer
    {
        public string hostname { get; set; }
        public int port { get; set; }
        public int ssl_port { get; set; }        
    }
}
