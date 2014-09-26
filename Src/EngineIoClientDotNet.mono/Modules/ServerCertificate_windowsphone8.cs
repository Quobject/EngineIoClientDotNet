
namespace Quobject.EngineIoClientDotNet.Modules
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
