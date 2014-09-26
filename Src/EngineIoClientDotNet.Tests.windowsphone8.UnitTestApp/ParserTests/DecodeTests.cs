
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Quobject.EngineIoClientDotNet.Modules;
using Quobject.EngineIoClientDotNet.Parser;


namespace Quobject.EngineIoClientDotNet_Tests.ParserTests
{
    [TestClass]
    public class DecodeTests
    {
        private const string PARSER_ERROR = "parser error";

        [TestMethod]
        public void DecodeBadFormat()
        {
            LogManager.SetupLogManager();
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");

            Packet p = Parser.DecodePacket(":::");
            Assert.AreEqual(Packet.ERROR, p.Type);
            Assert.AreEqual(PARSER_ERROR, p.Data);
        }

        [TestMethod]
        public void DecodeInexistingTypes()
        {
            LogManager.SetupLogManager();
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");

            Packet p = Parser.DecodePacket("94103");
            Assert.AreEqual(Packet.ERROR, p.Type);
            Assert.AreEqual(PARSER_ERROR, p.Data);
        }

        [TestMethod]
        public void DecodeInvalidUTF8()
        {
            LogManager.SetupLogManager();
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");

            Packet p = Parser.DecodePacket("4\uffff");
            Assert.AreEqual(Packet.ERROR, p.Type);
            Assert.AreEqual(PARSER_ERROR, p.Data);
        }


        public class DecodePayloadBadFormat_DecodeCallback : IDecodePayloadCallback
        {

            public bool Call(Packet packet, int index, int total)
            {
                var isLast = index + 1 == total;
                Assert.IsTrue(isLast);
                Assert.AreEqual(Packet.ERROR, packet.Type);
                Assert.AreEqual(PARSER_ERROR, packet.Data);
                return true;
            }
        }

        [TestMethod]
        public void EncodeAndDecodeEmptyPayloads()
        {
            LogManager.SetupLogManager();
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");

            Packet.DecodePayload("1!", new DecodePayloadBadFormat_DecodeCallback());
            Packet.DecodePayload("", new DecodePayloadBadFormat_DecodeCallback());
            Packet.DecodePayload("))", new DecodePayloadBadFormat_DecodeCallback());
        }

        [TestMethod]
        public void DecodePayloadBadPacketFormat()
        {
            LogManager.SetupLogManager();
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");

            Packet.DecodePayload("3:99", new DecodePayloadBadFormat_DecodeCallback());
            Packet.DecodePayload("1:aa", new DecodePayloadBadFormat_DecodeCallback());
            Packet.DecodePayload("1:a2:b", new DecodePayloadBadFormat_DecodeCallback());
        }

        [TestMethod]
        public void DecodePayloadInvalidUTF8()
        {
            LogManager.SetupLogManager();
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");

            Packet.DecodePayload("2:4\uffff", new DecodePayloadBadFormat_DecodeCallback());
        }



    }
}
