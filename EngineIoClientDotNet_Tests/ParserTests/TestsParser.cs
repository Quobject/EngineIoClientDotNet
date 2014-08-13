using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Quobject.EngineIoClientDotNet.Parser;
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


            };

            foreach (var test in testList)
            {
                Parser.EncodePacket(test.GetPacket(), (IEncodeCallback)test);
            }
        }


        public class EncodeAsStringCallback : IEncodeCallback, IPacketTest
        {
            public void Call(object data)
            {
                Assert.IsType<string>( data);
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
                Assert.IsType<Packet>( Parser.DecodePacket((string)data) );
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
                Packet p = Parser.DecodePacket((string)data);
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
                Packet p = Parser.DecodePacket((string)data);
                Assert.Equal(Packet.OPEN, p.Type);
                Assert.Equal(Json,p.Data);
            }

            public Packet GetPacket()
            {
                return new Packet(Packet.OPEN,Json);
            }
        }

        public class EncodeClosePacket : IEncodeCallback, IPacketTest
        {
            public void Call(object data)
            {
                Packet p = Parser.DecodePacket((string)data);
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
                Packet p = Parser.DecodePacket((string)data);
                Assert.Equal(Packet.PING, p.Type);
                Assert.Equal("1",p.Data);
            }

            public Packet GetPacket()
            {
                return new Packet(Packet.PING,"1");
            }
        }

        public class EncodePongPacket : IEncodeCallback, IPacketTest
        {
            public void Call(object data)
            {
                Packet p = Parser.DecodePacket((string)data);
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
                Packet p = Parser.DecodePacket((string)data);
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
                Packet p = Parser.DecodePacket((string)data);
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
                Packet p = Parser.DecodePacket((string)data);
                Assert.Equal(Packet.UPGRADE, p.Type);
            }

            public Packet GetPacket()
            {
                return new Packet(Packet.UPGRADE);
            }
        }
    }
}
