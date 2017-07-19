using Quobject.EngineIoClientDotNet.Client;
using Quobject.EngineIoClientDotNet.Modules;
using System;
using System.Collections.Generic;
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
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");

            var options = CreateOptions();
            options.Transports = new List<string>() { "polling" };

            socket = new Socket(options);

            var List = socket.FilterUpgrades(new List<string>() { "polling", "websocket" });

            Assert.Equal("polling", List[0]);
            Assert.Equal(1, List.Count);
        }

        [Fact]
        public void SocketClosing()
        {
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");

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
                log.Info("EVENT_CLOSE = ");
                closed = true;
            });

            socket.Once(Socket.EVENT_ERROR, () =>
            {
                log.Info("EVENT_ERROR = ");
                error = true;
            });

            socket.Open();
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(5));
            //Task.Delay(1000);
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
    }
}