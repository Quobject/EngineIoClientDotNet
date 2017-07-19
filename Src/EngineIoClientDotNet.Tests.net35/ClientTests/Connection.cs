using Quobject.EngineIoClientDotNet.Client;
using Quobject.EngineIoClientDotNet.Modules;

namespace Quobject.EngineIoClientDotNet_Tests.ClientTests
{
    public class Connection
    {
        static Connection()
        {
            LogManager.SetupLogManager();
        }

        protected Socket.Options CreateOptions()
        {
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());

            //var config = ConfigBase.Load();
            var options = new Socket.Options
            {
                Port = ConnectionConstants.PORT,
                Hostname = ConnectionConstants.HOSTNAME
            };
            log.Info("Please add to your hosts file: 127.0.0.1 " + options.Hostname);

            return options;
        }

        protected Socket.Options CreateOptionsSecure()
        {
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());

            //var config = ConfigBase.Load();
            var options = new Socket.Options
            {
                Port = ConnectionConstants.SSL_PORT,
                Hostname = ConnectionConstants.HOSTNAME,
                Secure = true,
                IgnoreServerCertificateValidation = true
            };
            log.Info("Please add to your hosts file: 127.0.0.1 " + options.Hostname);
            return options;
        }
    }
}