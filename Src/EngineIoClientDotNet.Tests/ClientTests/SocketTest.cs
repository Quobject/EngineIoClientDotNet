//using log4net;

using EngineIoClientDotNet.Modules;
using Quobject.EngineIoClientDotNet.Client;
using System;
using System.Collections.Immutable;
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
            Assert.Equal(1,immutablelist.Count);
        }

        [Fact]
        public void SocketClosing()
        {
            

            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            var closed = false;
            var error = false;

            var options = CreateOptions();

            socket = new Socket("ws://0.0.0.0:8080", options);
            socket.On(Socket.EVENT_OPEN, () =>
            {
                log.Info("EVENT_OPEN");
                //socket.Send("test send");

            });
            socket.On(Socket.EVENT_CLOSE, () =>
            {
                log.Info("EVENT_CLOSE = " );
                closed = true;

            });

            socket.Once(Socket.EVENT_ERROR, () =>
            {
                log.Info("EVENT_ERROR = ");
                error = true;

            });

            socket.Open();
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1));
            Assert.True(closed);
            Assert.True(error);
        }
    }
}
