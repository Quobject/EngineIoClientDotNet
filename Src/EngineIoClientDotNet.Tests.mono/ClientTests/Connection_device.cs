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



            var config = ConfigBase.Load();
            var options = new Socket.Options();
            options.Port = config.server.port;
            options.Hostname = config.server.hostname;
            //log.Info("Please add to your hosts file: 127.0.0.1 " + options.Hostname);

            return options;
        }


        protected Socket.Options CreateOptionsSecure()
        {            


            var config = ConfigBase.Load();
            var options = new Socket.Options();
            options.Port = config.server.ssl_port;
            options.Hostname = config.server.hostname;
            //log.Info("Please add to your hosts file: 127.0.0.1 " + options.Hostname);
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
            //var configString = File.ReadAllText("./../../../../grunt/config.json");
			var configString = @"{""version"":""0.1.0.0"",""server"":{""port"":80,""ssl_port"":443,""hostname"":""192.168.178.59""},""win"":{""powershell"":""C:/WINDOWS/System32/WindowsPowerShell/v1.0/powershell.exe"",""msbuild"":""C:/Windows/Microsoft.NET/Framework/v4.0.30319/msbuild.exe"",""xunit_path"":""C:/vendors/xunit"",""nuget"":""C:/vendors/nuget/nuget.exe""},""linux"":{""msbuild"":""xbuild"",""xunit_path"":""/home/apollo/vendors/xunit""}}";

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
