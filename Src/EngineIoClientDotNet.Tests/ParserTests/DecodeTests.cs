using Quobject.EngineIoClientDotNet.Modules;
using Quobject.EngineIoClientDotNet.Parser;
using Xunit;

namespace Quobject.EngineIoClientDotNet_Tests.ParserTests
{
    public class DecodeTests
    {
        private const string PARSER_ERROR = "parser error";

        [Fact]
        public void DecodeBadFormat()
        {
            LogManager.SetupLogManager();
            var log = LogManager.GetLogger("DecodeTests DecodeBadFormat");


            Packet p = Parser.DecodePacket(":::");
            Assert.Equal(Packet.ERROR, p.Type);
            Assert.Equal(PARSER_ERROR, p.Data);
        }

        [Fact]
        public void DecodeInexistingTypes()
        {

            Packet p = Parser.DecodePacket("94103");
            Assert.Equal(Packet.ERROR, p.Type);
            Assert.Equal(PARSER_ERROR, p.Data);
        }

        [Fact]
        public void DecodeInvalidUTF8()
        {

            Packet p = Parser.DecodePacket("4\uffff", true);
            Assert.Equal(Packet.ERROR, p.Type);
            Assert.Equal(PARSER_ERROR, p.Data);
        }


        public class DecodePayloadBadFormat_DecodeCallback : IDecodePayloadCallback
        {

            public bool Call(Packet packet, int index, int total)
            {
                var isLast = index + 1 == total;
                Assert.True(isLast);
                Assert.Equal(Packet.ERROR, packet.Type);
                Assert.Equal(PARSER_ERROR, packet.Data);
                return true;
            }
        }

        [Fact]
        public void EncodeAndDecodeEmptyPayloads()
        {

            Packet.DecodePayload("1!", new DecodePayloadBadFormat_DecodeCallback());
            Packet.DecodePayload("", new DecodePayloadBadFormat_DecodeCallback());
            Packet.DecodePayload("))", new DecodePayloadBadFormat_DecodeCallback());
        }

        [Fact]
        public void DecodePayloadBadPacketFormat()
        {

            Packet.DecodePayload("3:99", new DecodePayloadBadFormat_DecodeCallback());
            Packet.DecodePayload("1:aa", new DecodePayloadBadFormat_DecodeCallback());
            Packet.DecodePayload("1:a2:b", new DecodePayloadBadFormat_DecodeCallback());
        }

        [Fact]
        public void DecodePayloadInvalidUTF8()
        {
            Packet.DecodePayload("2:4\uffff", new DecodePayloadBadFormat_DecodeCallback());
        }



    }
}
