using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quobject.EngineIoClientDotNet.Client;

namespace Quobject.EngineIoClientDotNet_Tests.ClientTests
{
    public class Connection
    {
        public static readonly int TIMEOUT = 300000;
        public static readonly int PORT = 80;
        public static readonly int SECURE_PORT = 443;

        protected Socket.Options CreateOptions()
        {
            var options = new Socket.Options();
            options.Port = PORT;
            options.Hostname = "localhost";
            return options;
        }

        protected Socket.Options CreateOptionsSecure()
        {
            var options = new Socket.Options();
            options.Port = SECURE_PORT;
            options.Hostname = "localhost";
            options.Secure = true;
            options.IgnoreServerCertificateValidation = true;
            return options;
        }
    }
}
