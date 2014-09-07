using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Quobject.EngineIoClientDotNet.Client;


namespace Quobject.EngineIoClientDotNet_Tests.ClientTests
{
    [TestClass]    
    public class HandshakeDataTests
    {
        [TestMethod]
        public void Test()
        {
            var json = @"{
                sid: 'nne323',
                upgrades: ['u1','u2'],
                pingInterval: 12,
                pingTimeout: 23
            }";

            var handshakeData = new HandshakeData(json);
            Assert.AreEqual("u1", handshakeData.Upgrades[0]);
            Assert.AreEqual("u2", handshakeData.Upgrades[1]);

            Assert.AreEqual(12, handshakeData.PingInterval);
            Assert.AreEqual(23, handshakeData.PingTimeout);

        }
    }
}
