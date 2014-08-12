using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quobject.EngineIoClientDotNet.Modules;

namespace Quobject.EngineIoClientDotNet.Parser
{
    /// <remarks>
    /// Packet type which is ported from the JavaScript module.
    /// This is the JavaScript parser for the engine.io protocol encoding, 
    /// shared by both engine.io-client and engine.io.
    /// <see href="https://github.com/Automattic/engine.io-parser">https://github.com/Automattic/engine.io-parser</see>
    /// </remarks>
    public class Packet
    {
        public static String OPEN = "open";
        public static String CLOSE = "close";
        public static String PING = "ping";
        public static String PONG = "pong";
        public static String UPGRADE = "upgrade";
        public static String MESSAGE = "message";
        public static String NOOP = "noop";
        public static String ERROR = "error";


        private static readonly Dictionary<string, byte> _packets = new Dictionary<string, byte>()
        {
            {Packet.OPEN, 0},
            {Packet.CLOSE, 1},
            {Packet.PING, 2},
            {Packet.PONG, 3},
            {Packet.MESSAGE, 4},
            {Packet.UPGRADE, 5},
            {Packet.NOOP, 6}
        };

        private static readonly Dictionary<byte, string> _packetsList = new Dictionary<byte, string>();

        static Packet()
        {
            foreach (var entry in _packets)
            {
                _packetsList.Add(entry.Value,entry.Key);
            }
        }

        private static readonly Packet _err = new Packet(Packet.ERROR,"parser error");




        public string Type { get; set; }
        public object Data { get; set; }
       

        public Packet(string type, object data)
        {
            this.Type = type;
            this.Data = data;
        }

        internal void Encode(IEncodeCallback callback)
        {
            if ( Data is byte[])
            {
                EncodeByteArray(callback);
                return;
            }
            var encodedStringBuilder = new StringBuilder();
            encodedStringBuilder.Append(_packets[Type]);

            if (Data != null)
            {
                encodedStringBuilder.Append(UTF8.Encode((string) Data));
            }

            callback.Call(encodedStringBuilder.ToString());

        }

        private void EncodeByteArray(IEncodeCallback callback)
        {
            var byteData = Data as byte[];
            if (byteData != null)
            {
                var resultArray = new byte[1 + byteData.Length];
                resultArray[0] =  _packets[Type];
                Array.Copy(byteData, 0, resultArray, 1, byteData.Length);
                callback.Call(resultArray);
            }
            throw new Exception("byteData == null");
        }

        internal static Packet DecodePacket(string data)
        {
            int type;

            if (int.TryParse(data.Substring(0, 1), out type))
            {
                type = -1;
            }

            try
            {
                data = UTF8.Decode(data);
            }
            catch (Exception)
            {

                return _err;
            }

            if (type < 0 || type >= _packetsList.Count)
            {
                return _err;
            }

            if (data.Length > 1)
            {
                return new Packet(_packetsList[(byte) type], data.Substring(1));
            }
            return new Packet(_packetsList[(byte) type], null);
        }

        internal static Packet DecodePacket(byte[] data)
        {
            int type = data[0];
            var byteArray = new byte[data.Length - 1];
            Array.Copy(data,1,byteArray,0, byteArray.Length);
            return new Packet(_packetsList[(byte)type], byteArray);
        }



        internal static void EncodePayload(Packet[] packets, IEncodeCallback callback)
        {
            if (packets.Length == 0)
            {
                callback.Call(new byte[0]);
                return;
            }

            var results = new List<byte[]>(packets.Length);
            var encodePayloadCallback = new EncodePayloadCallback(results);
            foreach (var packet in packets)
            {
                packet.Encode(encodePayloadCallback);
            }

            callback.Call(Buffer.Concat(results.ToArray()));//new byte[results.Count][]


        }

        internal static byte[] StringToByteArray(string str)
        {
            int len = str.Length;
            var bytes = new byte[len];
            for (int i = 0; i < len; i++)
            {
                bytes[i] = (byte)str[i];
            }
            return bytes;
        }

        internal static string ByteArrayToString(byte[] bytes)
        {
            return Encoding.ASCII.GetString(bytes);
        }

        private class EncodePayloadCallback : IEncodeCallback
        {
            private readonly List<byte[]> _results;

            public EncodePayloadCallback(List<byte[]> results)
            {
                this._results = results;
            }

            public void Call(object data)
            {
                if (data is string)
                {
                    var packet = (string) data;
                    var encodingLength = packet.Length.ToString();
                    var sizeBuffer = new byte[encodingLength.Length + 2];
                    sizeBuffer[0] = (byte) 0; // is a string
                    for (var i = 0; i < encodingLength.Length; i++)
                    {
                        sizeBuffer[i + 1] = byte.Parse(encodingLength.Substring(i,1));
                    }
                    sizeBuffer[sizeBuffer.Length - 1] = (byte) 255;
                    _results.Add(Buffer.Concat(new byte[][] { sizeBuffer, StringToByteArray(packet) }));
                    return;
                }

                var packet1 = (byte[]) data;
                var encodingLength1 = packet1.Length.ToString();
                var sizeBuffer1 = new byte[encodingLength1.Length + 2];
                sizeBuffer1[0] = (byte)1; // is binary
                for (var i = 0; i < encodingLength1.Length; i++)
                {
                    sizeBuffer1[i + 1] = byte.Parse(encodingLength1.Substring(i, 1));
                }
                sizeBuffer1[sizeBuffer1.Length - 1] = (byte)255;
                _results.Add(Buffer.Concat(new byte[][] { sizeBuffer1, packet1 }));
            }
        }

    }
}
