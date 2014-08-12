using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quobject.EngineIoClientDotNet.Parser
{
    /// <remarks>
    /// This is the JavaScript parser for the engine.io protocol encoding, 
    /// shared by both engine.io-client and engine.io.
    /// <see href="https://github.com/Automattic/engine.io-parser">https://github.com/Automattic/engine.io-parser</see>
    /// </remarks>
    public class Parser
    {
        private static readonly int MAX_INT_CHAR_LENGTH = int.MaxValue.ToString().Length;

        public static readonly int Protocol = 3;


        private Parser()
        {
        }

        public static void EncodePacket(Packet packet, IEncodeCallback callback)
        {
            packet.Encode(callback);
        }

        public static Packet DecodePacket(string data)
        {
            return Packet.DecodePacket(data);
        }

        public static Packet DecodePacket(byte[] data)
        {
            return Packet.DecodePacket(data);
        }

        public static void EncodePayload(Packet[] packets, IEncodeCallback callback)
        {
            Packet.EncodePayload(packets, callback);
        }


    }
}
