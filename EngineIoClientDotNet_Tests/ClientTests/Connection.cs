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
        public static readonly int PORT = 3000;

        protected Socket.Options CreateOptions()
        {
            var options = new Socket.Options();
            options.Port = PORT;
            options.Hostname = "localhost";
            return options;
        }
    }
}
