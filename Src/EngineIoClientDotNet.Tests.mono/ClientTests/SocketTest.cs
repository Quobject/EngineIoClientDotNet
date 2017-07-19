using Quobject.EngineIoClientDotNet.Client;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Xunit;

namespace Quobject.EngineIoClientDotNet_Tests.ClientTests
{
    public class SocketTest : Connection
    {
        private Socket socket;
        public string Message;

        [Fact]
        public void FilterUpgrades()
        {
            var options = CreateOptions();
            options.Transports = ImmutableList<string>.Empty.Add("polling");

            socket = new Socket(options);

            var immutablelist = socket.FilterUpgrades(ImmutableList<string>.Empty.Add("polling").Add("websocket"));

            Assert.Equal("polling", immutablelist[0]);
            Assert.Equal(1, immutablelist.Count);
        }

        [Fact]
        public async Task SocketClosing()
        {
            var closed = false;
            var error = false;

            var options = CreateOptions();

            socket = new Socket("ws://0.0.0.0:8080", options);
            socket.On(Socket.EVENT_OPEN, () =>
            {
                //socket.Send("test send");
            });
            socket.On(Socket.EVENT_CLOSE, () =>
            {
                //log.Info("EVENT_CLOSE = ");
                closed = true;
            });

            socket.Once(Socket.EVENT_ERROR, () =>
            {
                //log.Info("EVENT_ERROR = ");
                error = true;
            });

            socket.Open();
            //System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1));
            await Task.Delay(1000);
            Assert.True(closed);
            Assert.True(error);
        }

        [Fact]
        public void SocketOptionCookies()
        {
            var options = new Socket.Options();
            options.Cookies.Add("foo", "bar");
            Assert.Equal("foo=bar", options.GetCookiesAsString());
            options.Cookies.Add("name2", "value2");
            Assert.Equal("foo=bar; name2=value2", options.GetCookiesAsString());
        }

        [Fact]
        public void DefaultProtocol()
        {
            var socket = new Socket("testme.quobject.com");
            Assert.NotNull(socket);
        }
    }
}