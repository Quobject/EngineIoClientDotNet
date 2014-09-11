using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EngineIoClientDotNet.Modules
{
    public class ServerCertificate
    {
        public static bool Ignore { get; set; }

        static ServerCertificate()
        {
            Ignore = false;
        }

        public static void IgnoreServerCertificateValidation()
        {
            //ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            Ignore = true;
        }
    }
}
