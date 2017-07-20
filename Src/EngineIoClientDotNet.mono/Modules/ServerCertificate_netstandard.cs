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
            Ignore = true;
        }
    }
}