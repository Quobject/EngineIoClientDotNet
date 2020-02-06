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

        protected static Socket.Options CreateOptions()
        {
            var options = new Socket.Options
            {
                Port = ConnectionConstants.PORT,
                Hostname = ConnectionConstants.HOSTNAME
            };
            //log.Info("Please add to your hosts file: 127.0.0.1 " + options.Hostname);

            return options;
        }

        protected static Socket.Options CreateOptionsSecure()
        {
            var options = new Socket.Options
            {
                Port = ConnectionConstants.SSL_PORT,
                Hostname = ConnectionConstants.HOSTNAME,
                //log.Info("Please add to your hosts file: 127.0.0.1 " + options.Hostname);
                Secure = true,
                IgnoreServerCertificateValidation = true
            };
            return options;
        }
    }
}