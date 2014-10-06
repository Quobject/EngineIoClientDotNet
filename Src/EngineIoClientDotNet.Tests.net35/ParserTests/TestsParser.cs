using Quobject.EngineIoClientDotNet.Modules;
using Quobject.EngineIoClientDotNet.Parser;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Xunit;

namespace Quobject.EngineIoClientDotNet_Tests.ParserTests
{
    public class TestsParser
    {

        public interface IPacketTest
        {
            Packet GetPacket();

        }

        [Fact]
        public void EncodeTests()
        {
            LogManager.SetupLogManager();
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");

            var testList = new List<IPacketTest>()
            {
                new EncodeAsStringCallback(),
                new DecodeAsPacketCallback(),
                new NoDataCallback(),
                new EncodeOpenPacket(),
                new EncodeClosePacket(),
                new EncodePingPacket(),
                new EncodePongPacket(),
                new EncodeMessagePacket(),
                new EncodeUTF8SpecialCharsPacket(),
                new EncodeUpgradePacket(),
                new EncodeFormat1(),
                new EncodeFormat2(),


            };

            foreach (var test in testList)
            {
                Parser.EncodePacket(test.GetPacket(), (IEncodeCallback) test);
            }
        }


        public class EncodeAsStringCallback : IEncodeCallback, IPacketTest
        {
            public void Call(object data)
            {
                Assert.IsType<string>(data);
            }

            public Packet GetPacket()
            {
                return new Packet(Packet.MESSAGE, "test");
            }
        }


        public class DecodeAsPacketCallback : IEncodeCallback, IPacketTest
        {
            public void Call(object data)
            {
                Assert.IsType<Packet>(Parser.DecodePacket((string) data));
            }

            public Packet GetPacket()
            {
                return new Packet(Packet.MESSAGE, "test");
            }
        }


        public class NoDataCallback : IEncodeCallback, IPacketTest
        {
            public void Call(object data)
            {
                Packet p = Parser.DecodePacket((string) data);
                Assert.Equal(Packet.MESSAGE, p.Type);
                Assert.Null(p.Data);
            }

            public Packet GetPacket()
            {
                return new Packet(Packet.MESSAGE);
            }
        }

        public class EncodeOpenPacket : IEncodeCallback, IPacketTest
        {
            private static string Json = "{\"some\":\"json\"}";

            public void Call(object data)
            {
                Packet p = Parser.DecodePacket((string) data);
                Assert.Equal(Packet.OPEN, p.Type);
                Assert.Equal(Json, p.Data);
            }

            public Packet GetPacket()
            {
                return new Packet(Packet.OPEN, Json);
            }
        }

        public class EncodeClosePacket : IEncodeCallback, IPacketTest
        {
            public void Call(object data)
            {
                Packet p = Parser.DecodePacket((string) data);
                Assert.Equal(Packet.CLOSE, p.Type);
            }

            public Packet GetPacket()
            {
                return new Packet(Packet.CLOSE);
            }
        }

        public class EncodePingPacket : IEncodeCallback, IPacketTest
        {
            public void Call(object data)
            {
                Packet p = Parser.DecodePacket((string) data);
                Assert.Equal(Packet.PING, p.Type);
                Assert.Equal("1", p.Data);
            }

            public Packet GetPacket()
            {
                return new Packet(Packet.PING, "1");
            }
        }

        public class EncodePongPacket : IEncodeCallback, IPacketTest
        {
            public void Call(object data)
            {
                Packet p = Parser.DecodePacket((string) data);
                Assert.Equal(Packet.PONG, p.Type);
                Assert.Equal("1", p.Data);
            }

            public Packet GetPacket()
            {
                return new Packet(Packet.PONG, "1");
            }
        }

        public class EncodeMessagePacket : IEncodeCallback, IPacketTest
        {
            public void Call(object data)
            {
                Packet p = Parser.DecodePacket((string) data);
                Assert.Equal(Packet.MESSAGE, p.Type);
                Assert.Equal("aaa", p.Data);
            }

            public Packet GetPacket()
            {
                return new Packet(Packet.MESSAGE, "aaa");
            }
        }

        public class EncodeUTF8SpecialCharsPacket : IEncodeCallback, IPacketTest
        {
            public void Call(object data)
            {
                Packet p = Parser.DecodePacket((string) data);
                Assert.Equal(Packet.MESSAGE, p.Type);
                Assert.Equal("utf8 â€” string", p.Data);
            }

            public Packet GetPacket()
            {
                return new Packet(Packet.MESSAGE, "utf8 â€” string");
            }
        }

        public class EncodeUpgradePacket : IEncodeCallback, IPacketTest
        {
            public void Call(object data)
            {
                Packet p = Parser.DecodePacket((string) data);
                Assert.Equal(Packet.UPGRADE, p.Type);
            }

            public Packet GetPacket()
            {
                return new Packet(Packet.UPGRADE);
            }
        }

        public class EncodeFormat1 : IEncodeCallback, IPacketTest
        {
            public void Call(object data)
            {
                var dataString = data as string;
                var r = new Regex(@"[0-9]", RegexOptions.IgnoreCase);
                Assert.True(r.Match(dataString).Success);
            }

            public Packet GetPacket()
            {
                return new Packet(Packet.MESSAGE);
            }
        }

        public class EncodeFormat2 : IEncodeCallback, IPacketTest
        {
            public void Call(object data)
            {
                var dataString = data as string;
                Assert.Equal("4test", dataString);
            }

            public Packet GetPacket()
            {
                return new Packet(Packet.MESSAGE, "test");
            }
        }



        public class EncodePayloadsCallback : IEncodeCallback
        {
            public void Call(object data)
            {
                Assert.IsType<byte[]>(data);
            }



        }

        [Fact]
        public void EncodePayloads()
        {
            LogManager.SetupLogManager();
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");

            var packets = new Packet[] {new Packet(Packet.PING), new Packet(Packet.PONG),};
            Parser.EncodePayload(packets, new EncodePayloadsCallback());

        }


        public class EncodeAndDecodePayloads_EncodeCallback : IEncodeCallback
        {
            public void Call(object data)
            {
                Parser.DecodePayload((byte[]) data, new EncodeAndDecodePayloads_DecodeCallback());
            }

            public class EncodeAndDecodePayloads_DecodeCallback : IDecodePayloadCallback
            {

                public bool Call(Packet packet, int index, int total)
                {
                    bool isLast = index + 1 == total;
                    Assert.True(isLast);
                    return true;
                }
            }

        }

        [Fact]
        public void EncodeAndDecodePayloads()
        {
            LogManager.SetupLogManager();
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");

            var packets = new Packet[] {new Packet(Packet.MESSAGE, "a"),};
            Parser.EncodePayload(packets, new EncodeAndDecodePayloads_EncodeCallback());

        }

        public class EncodeAndDecodePayloads_EncodeCallback2 : IEncodeCallback
        {
            public void Call(object data)
            {
                Parser.DecodePayload((byte[]) data, new EncodeAndDecodePayloads_DecodeCallback2());
            }

            public class EncodeAndDecodePayloads_DecodeCallback2 : IDecodePayloadCallback
            {

                public bool Call(Packet packet, int index, int total)
                {
                    var isLast = index + 1 == total;
                    Assert.Equal(isLast ? Packet.PING : Packet.MESSAGE, packet.Type);
                    return true;
                }
            }

        }

        [Fact]
        public void EncodeAndDecodePayloads2()
        {
            LogManager.SetupLogManager();
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");

            var packets = new Packet[] {new Packet(Packet.MESSAGE, "a"), new Packet(Packet.PING),};
            Parser.EncodePayload(packets, new EncodeAndDecodePayloads_EncodeCallback2());

        }

        public class EncodeAndDecodeEmptyPayloads_EncodeCallback : IEncodeCallback
        {
            public void Call(object data)
            {
                Parser.DecodePayload((byte[]) data, new EncodeAndDecodeEmptyPayloads_DecodeCallback());
            }

            public class EncodeAndDecodeEmptyPayloads_DecodeCallback : IDecodePayloadCallback
            {

                public bool Call(Packet packet, int index, int total)
                {
                    Assert.Equal(Packet.OPEN, packet.Type);
                    var isLast = index + 2 == total;
                    Assert.True(isLast);
                    return true;
                }
            }

        }

        [Fact]
        public void EncodeAndDecodeEmptyPayloads()
        {
            LogManager.SetupLogManager();
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");

            var packets = new Packet[] {};
            Parser.EncodePayload(packets, new EncodeAndDecodeEmptyPayloads_EncodeCallback());

        }


        public class EncodeAndDecodeBinaryContents_EncodeCallback : IEncodeCallback
        {
            public void Call(object data)
            {
                Parser.DecodePayload((byte[]) data, new EncodeAndDecodeBinaryContents_DecodeCallback());
            }

            public class EncodeAndDecodeBinaryContents_DecodeCallback : IDecodePayloadCallback
            {

                public bool Call(Packet packet, int index, int total)
                {
                    Assert.Equal(Packet.MESSAGE, packet.Type);
                    var isLast = index + 1 == total;
                    if (!isLast)
                    {
                        Assert.Equal(FirstBuffer(), packet.Data);
                    }
                    else
                    {
                        Assert.Equal(SecondBuffer(), packet.Data);
                    }
                    return true;
                }
            }

        }

        [Fact]
        public void EncodeAndDecodeBinaryContents()
        {
            LogManager.SetupLogManager();
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");

            var firstBuffer = FirstBuffer();
            var secondBuffer = SecondBuffer();

            var packets = new Packet[]
            {new Packet(Packet.MESSAGE, firstBuffer), new Packet(Packet.MESSAGE, secondBuffer)};
            Parser.EncodePayload(packets, new EncodeAndDecodeBinaryContents_EncodeCallback());

        }

        private static byte[] SecondBuffer()
        {
            var secondBuffer = new byte[4];
            for (int i = 0; i < secondBuffer.Length; i++)
            {
                secondBuffer[i] = (byte) (5 + i);
            }
            return secondBuffer;
        }

        private static byte[] FirstBuffer()
        {
            var firstBuffer = new byte[5];
            for (int i = 0; i < firstBuffer.Length; i++)
            {
                firstBuffer[i] = (byte) i;
            }
            return firstBuffer;
        }

        private static byte[] ThirdBuffer()
        {
            var result = new byte[123];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = (byte) i;
            }
            return result;
        }


        public class EncodeMixedBinaryAndStringContents_EncodeCallback : IEncodeCallback
        {

            public void Call(object data)
            {
                Parser.DecodePayload((byte[]) data, new EncodeMixedBinaryAndStringContents_DecodeCallback());
            }

            public class EncodeMixedBinaryAndStringContents_DecodeCallback : IDecodePayloadCallback
            {

                public bool Call(Packet packet, int index, int total)
                {
                    if (index == 0)
                    {
                        Assert.Equal(Packet.MESSAGE, packet.Type);
                        Assert.Equal(ThirdBuffer(), packet.Data);
                    }
                    else if (index == 1)
                    {
                        Assert.Equal(Packet.MESSAGE, packet.Type);
                        Assert.Equal("hello", packet.Data);
                    }
                    else
                    {
                        Assert.Equal(Packet.CLOSE, packet.Type);
                    }
                    return true;
                }
            }

        }

        [Fact]
        public void EncodeMixedBinaryAndStringContents()
        {
            LogManager.SetupLogManager();
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");

            var packets = new Packet[]
            {
                new Packet(Packet.MESSAGE, ThirdBuffer()),
                new Packet(Packet.MESSAGE, "hello"),
                new Packet(Packet.CLOSE),
            };
            Parser.EncodePayload(packets, new EncodeMixedBinaryAndStringContents_EncodeCallback());

        }

    }
}
