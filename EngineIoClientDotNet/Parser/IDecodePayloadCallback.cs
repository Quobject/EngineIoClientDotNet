using System.Security.Cryptography.X509Certificates;

namespace Quobject.EngineIoClientDotNet.Parser
{

    public interface IDecodePayloadCallback
    {
         bool Call(Packet packet, int index, int total);
    }


}