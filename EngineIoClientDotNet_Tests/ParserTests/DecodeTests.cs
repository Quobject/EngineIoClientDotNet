using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Quobject.EngineIoClientDotNet.Parser;
using Xunit;

namespace Quobject.EngineIoClientDotNet_Tests.ParserTests
{
    public class DecodeTests
    {

        [Fact]
        public void DecodeBadFormat()
        {
            Packet p = Parser.DecodePacket(":::");
            Assert.Equal(Packet.ERROR,p.Type);
            Assert.Equal("parser error", p.Data);
        }

        [Fact]
        public void DecodeInexistingTypes()
        {
            Packet p = Parser.DecodePacket("94103");
            Assert.Equal(Packet.ERROR, p.Type);
            Assert.Equal("parser error", p.Data);
        }

        [Fact]
        public void DecodeInvalidUTF8()
        {
            Packet p = Parser.DecodePacket("4\uffff");
            Assert.Equal(Packet.ERROR, p.Type);
            Assert.Equal("parser error", p.Data);
        }

    }
}
