using Quobject.EngineIoClientDotNet.Client;
using Xunit;

namespace Quobject.EngineIoClientDotNet_Tests.ClientTests
{
    public class HandshakeDataTests
    {
        [Fact]
        public void Test()
        {
            var json = @"{
                sid: 'nne323',
                upgrades: ['u1','u2'],
                pingInterval: 12,
                pingTimeout: 23
            }";

            var handshakeData = new HandshakeData(json);
            Assert.Equal("u1", handshakeData.Upgrades[0]);
            Assert.Equal("u2", handshakeData.Upgrades[1]);

            Assert.Equal(12, handshakeData.PingInterval);
            Assert.Equal(23, handshakeData.PingTimeout);
        }
    }
}