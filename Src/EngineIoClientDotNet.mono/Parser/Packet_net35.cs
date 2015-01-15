using Quobject.EngineIoClientDotNet.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        public static readonly string OPEN = "open";
        public static readonly string CLOSE = "close";
        public static readonly string PING = "ping";
        public static readonly string PONG = "pong";
        public static readonly string UPGRADE = "upgrade";
        public static readonly string MESSAGE = "message";
        public static readonly string NOOP = "noop";
        public static readonly string ERROR = "error";

        private static readonly int MAX_INT_CHAR_LENGTH = int.MaxValue.ToString().Length;

        //TODO: suport binary?
        private bool SupportsBinary = false;

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

        public Packet(string type)
        {
            this.Type = type;
            this.Data = null;
        }

        internal void Encode(IEncodeCallback callback,bool utf8encode = false)
        {
            if ( Data is byte[])
            {
                if (!SupportsBinary)
                {
                    EncodeBase64Packet(callback);
                    return;
                }
                EncodeByteArray(callback);
                return;
            }
            var encodedStringBuilder = new StringBuilder();
            encodedStringBuilder.Append(_packets[Type]);

            if (Data != null)
            {
                encodedStringBuilder.Append( utf8encode ? UTF8.Encode((string) Data): (string)Data);
            }

            callback.Call(encodedStringBuilder.ToString());
        }

        private void EncodeBase64Packet(IEncodeCallback callback)
        {
            var byteData = Data as byte[];
            if (byteData != null)
            {
                var result = new StringBuilder();
                result.Append("b");
                result.Append(_packets[Type]);
                result.Append(Convert.ToBase64String(byteData));
                callback.Call(result.ToString());
                return;
            }
            throw new Exception("byteData == null");
        }

        private void EncodeByteArray(IEncodeCallback callback)
        {
            var byteData = Data as byte[];
            if (byteData != null)
            {
                var resultArray = new byte[1 + byteData.Length];
                resultArray[0] = _packets[Type];
                Array.Copy(byteData, 0, resultArray, 1, byteData.Length);
                callback.Call(resultArray);
                return;
            }
            throw new Exception("byteData == null");
        }

        internal static Packet DecodePacket(string data, bool utf8decode = false)
        {
            if (data.StartsWith("b"))
            {
                return DecodeBase64Packet(data.Substring(1));
            }

            int type;
            var s = data.Substring(0, 1);
            if (!int.TryParse(s, out type))
            {
                type = -1;
            }

            if (utf8decode)
            {
                try
                {
                    data = UTF8.Decode(data);
                }
                catch (Exception)
                {

                    return _err;
                }                
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

        private static Packet DecodeBase64Packet(string msg)
        {
            int type;
            var s = msg.Substring(0, 1);
            if (!int.TryParse(s, out type))
            {
                type = -1;
            }
            if (type < 0 || type >= _packetsList.Count)
            {
                return _err;
            }
            msg = msg.Substring(1);
            byte[] decodedFromBase64 = Convert.FromBase64String(msg);
            return new Packet(_packetsList[(byte)type], decodedFromBase64);
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
                packet.Encode(encodePayloadCallback, true);
            }

            callback.Call(Buffer.Concat(results.ToArray()));//new byte[results.Count][]
        }

        /// <summary>
        /// Decodes data when a payload is maybe expected.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="callback"></param>
        public static void DecodePayload(string data, IDecodePayloadCallback callback)
        {
            if (String.IsNullOrEmpty(data))
            {
                callback.Call(_err, 0, 1);
                return;
            }

            var length = new StringBuilder();
            for (int i = 0, l = data.Length; i < l; i++)
            {
                var chr = Convert.ToChar(data.Substring(i, 1));

                if (chr != ':')
                {
                    length.Append(chr);
                }
                else
                {
                    int n;
                    if (!int.TryParse(length.ToString(), out n))
                    {
                        callback.Call(_err, 0, 1);
                        return;
                    }

                    string msg;
                    try
                    {
                        msg = data.Substring(i + 1, n);
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        callback.Call(_err, 0, 1);
                        return;
                    }

                    if (msg.Length != 0)
                    {
                        Packet packet = DecodePacket(msg, true);
                        if (_err.Type == packet.Type && _err.Data == packet.Data)
                        {
                            callback.Call(_err, 0, 1);
                            return;
                        }

                        bool ret = callback.Call(packet, i + n, l);
                        if (!ret)
                        {
                            return;
                        }

                    }

                    i += n;
                    length = new StringBuilder();
                }
            }
            if (length.Length > 0)
            {
                callback.Call(_err, 0, 1);
            }
        }

        /// <summary>
        /// Decodes data when a payload is maybe expected.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="callback"></param>
        public static void DecodePayload(byte[] data, IDecodePayloadCallback callback)
        {
            var bufferTail = ByteBuffer.Wrap(data);

            var buffers = new List<object>();
            int bufferTail_offset = 0;
            while (bufferTail.Capacity - bufferTail_offset > 0)
            {
                var strLen = new StringBuilder();
                var isString = (bufferTail.Get(0 + bufferTail_offset) & 0xFF) == 0;
                var numberTooLong = false;
                for (int i = 1;; i++)
                {
                    int b = bufferTail.Get(i + bufferTail_offset) & 0xFF;
                    if (b == 255)
                    {
                        break;
                    }
                    // support only integer
                    if (strLen.Length > MAX_INT_CHAR_LENGTH)
                    {
                        numberTooLong = true;
                        break;
                    }
                    strLen.Append(b);
                }
                if (numberTooLong)
                {
                    callback.Call(_err, 0, 1);
                    return;
                }
                bufferTail_offset += strLen.Length + 1;

                int msgLength = int.Parse(strLen.ToString());

                bufferTail.Position(1 + bufferTail_offset);
                bufferTail.Limit(msgLength + 1 + bufferTail_offset);
                var msg = new byte[bufferTail.Remaining()];
                bufferTail.Get(msg, 0, msg.Length);
                
                if (isString)
                {
                    buffers.Add(ByteArrayToString(msg));
                }
                else
                {
                    buffers.Add(msg);
                }
                bufferTail.Clear();
                bufferTail.Position(msgLength + 1 + bufferTail_offset);
                bufferTail_offset += msgLength + 1;
            }

            var total = buffers.Count;
            for (int i = 0; i < total; i++)
            {
                var buffer = buffers[i];
                if (buffer is string)
                {
                    callback.Call(DecodePacket((string) buffer, true), i, total);
                }
                else if (buffer is byte[])
                {
                    callback.Call(DecodePacket((byte[])buffer), i, total);                    
                }
            }

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
            //return Encoding.ASCII.GetString(bytes);
            //http://stackoverflow.com/questions/7750850/encoding-ascii-getstring-in-windows-phone-platform
            // return string.Concat(bytes.Select(b => b <= 0x7f ? (char)b : '?')); //.net 4.0 only
            return string.Concat(
                (bytes.Select(b => (b <= 0x7f ? (char) b : '?').ToString()))
                    .ToArray()
                );
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
