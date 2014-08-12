using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quobject.EngineIoClientDotNet.Parser;
using Xunit;

namespace Quobject.EngineIoClientDotNet_Tests.ParserTests
{
    public class TestsParser
    {

        //
        // EncodeAsString
        //

        public class EncodeAsStringCallback : IEncodeCallback
        {
            public void Call(object data)
            {
                Assert.IsType<string>( data);
            }
        }

        [Fact]
        public void EncodeAsString()
        {
            Parser.EncodePacket(new Packet(Packet.MESSAGE,"test"), new EncodeAsStringCallback() );
        }

        //
        // DecodeAsPacket
        //

        public class DecodeAsPacketCallback : IEncodeCallback
        {
            public void Call(object data)
            {
                Assert.IsType<Packet>( Parser.DecodePacket((string)data) );
            }
        }

        [Fact]
        public void DecodeAsPacket()
        {
            Parser.EncodePacket(new Packet(Packet.MESSAGE, "test"), new DecodeAsPacketCallback());
        }

        //
        // no data
        //

        public class NoDataCallback : IEncodeCallback
        {
            public void Call(object data)
            {
                Packet p = Parser.DecodePacket((string)data);
                Assert.Equal(Packet.MESSAGE, p.Type);
                Assert.Null(p.Data);
            }
        }

        [Fact]
        public void NoData()
        {
            Parser.EncodePacket(new Packet(Packet.MESSAGE, null), new NoDataCallback());
        }






    }
}
