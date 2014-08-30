using System.Collections.Generic;
using log4net;
using Quobject.EngineIoClientDotNet.Client;
using System;
using Quobject.EngineIoClientDotNet.Client.Transports;
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
            Socket.SetupLog4Net();
            var options = CreateOptions();
            options.Transports = new List<string> {"polling"};

            socket = new Socket(options);

            var builder = new List<string> {"polling", "websocket"};


            var list = socket.FilterUpgrades(new List<string>{ Polling.NAME, WebSocket.NAME });

            Assert.Equal("polling", list[0]);
            Assert.Equal(1,list.Count);
        }

        [Fact]
        public void SocketClosing()
        {
            Socket.SetupLog4Net();

            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
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
