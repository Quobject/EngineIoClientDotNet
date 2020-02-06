
namespace Quobject.EngineIoClientDotNet.Parser
{
    /// <remarks>
    /// This is the JavaScript parser for the engine.io protocol encoding, 
    /// shared by both engine.io-client and engine.io.
    /// <see href="https://github.com/Automattic/engine.io-parser">https://github.com/Automattic/engine.io-parser</see>
    /// </remarks>
    public class Parser
    {        

        public static readonly int Protocol = 3;


        private Parser()
        {
        }

        public static void EncodePacket(Packet packet, IEncodeCallback callback)
        {
            packet.Encode(callback);
        }

        public static Packet DecodePacket(string data, bool utf8decode = false)
        {
            return Packet.DecodePacket(data, utf8decode);
        }

        public static Packet DecodePacket(byte[] data)
        {
            return Packet.DecodePacket(data);
        }

        public static void EncodePayload(Packet[] packets, IEncodeCallback callback)
        {
            Packet.EncodePayload(packets, callback);
        }


        public static void DecodePayload(string data, IDecodePayloadCallback callback)
        {
            Packet.DecodePayload(data, callback);
        }

        public static void DecodePayload(byte[] data, IDecodePayloadCallback callback)
        {
            Packet.DecodePayload(data, callback);
        }

    }
}
