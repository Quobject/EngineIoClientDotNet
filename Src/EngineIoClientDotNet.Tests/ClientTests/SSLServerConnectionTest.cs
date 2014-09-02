using log4net;
using Quobject.EngineIoClientDotNet.Client;
using Quobject.EngineIoClientDotNet.Client.Transports;
using Quobject.EngineIoClientDotNet.ComponentEmitter;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Xunit;

namespace Quobject.EngineIoClientDotNet_Tests.ClientTests
{
    public class SSLServerConnectionTest : Connection
    {
        [Fact]
        public async Task OpenAndClose()
        {
            Socket.SetupLog4Net();

            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            var events = new ConcurrentQueue<string>();

            var socket = new Socket(CreateOptionsSecure());
            socket.On(Socket.EVENT_OPEN, () =>
            {
                log.Info("EVENT_OPEN");
                events.Enqueue(Socket.EVENT_OPEN);
                socket.Close();

            });
            socket.On(Socket.EVENT_CLOSE, () =>
            {
                log.Info("EVENT_CLOSE");
                events.Enqueue(Socket.EVENT_CLOSE);
            });
            socket.Open();

            string result;
            events.TryDequeue(out result);
            Assert.Equal(Socket.EVENT_OPEN, result);
            events.TryDequeue(out result);
            Assert.Equal(Socket.EVENT_CLOSE, result);
            await Task.Delay(1);
            socket.Close();
        }


        [Fact]
        public void Messages()
        {
            Socket.SetupLog4Net();

            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            var events = new ConcurrentQueue<string>();

            var socket = new Socket(CreateOptionsSecure());
            socket.On(Socket.EVENT_OPEN, () =>
            {
                log.Info("EVENT_OPEN");
                socket.Send("hello");
            });
            socket.On(Socket.EVENT_MESSAGE, (d) =>
            {
                var data = (string)d;
                log.Info("EVENT_MESSAGE data = " + data);
                events.Enqueue(data);
                if (events.Count > 1)
                {
                    socket.Close();
                }
            });
            socket.Open();


            string result;
            events.TryDequeue(out result);
            Assert.Equal("hi", result);
            events.TryDequeue(out result);
            Assert.Equal("hello", result);
        }

        [Fact]
        public void Handshake()
        {
            Socket.SetupLog4Net();

            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            HandshakeData handshake_data = null;

            var socket = new Socket(CreateOptionsSecure());

            socket.On(Socket.EVENT_HANDSHAKE, (data) =>
            {
                log.Info(Socket.EVENT_HANDSHAKE + string.Format(" data = {0}", data));
                handshake_data = data as HandshakeData;
            });

            socket.Open();
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(5));
            socket.Close();

            Assert.NotNull(handshake_data);
            Assert.NotNull(handshake_data.Upgrades);
            Assert.True(handshake_data.Upgrades.Count > 0);
            Assert.True(handshake_data.PingInterval > 0);
            Assert.True(handshake_data.PingTimeout > 0);
        }


        public class TestHandshakeListener : IListener
        {
            public HandshakeData HandshakeData;

            public void Call(params object[] args)
            {
                var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
                log.Info(string.Format("open args[0]={0} args.Length={1}", args[0], args.Length));
                HandshakeData = args[0] as HandshakeData;
            }
        }

        [Fact]
        public void Handshake2()
        {

            Socket.SetupLog4Net();

            var socket = new Socket(CreateOptionsSecure());
            var testListener = new SSLServerConnectionTest.TestHandshakeListener();
            socket.On(Socket.EVENT_HANDSHAKE, testListener);
            socket.Open();
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(4));
            socket.Close();

            Assert.NotNull(testListener.HandshakeData);
            Assert.NotNull(testListener.HandshakeData.Upgrades);
            Assert.True(testListener.HandshakeData.Upgrades.Count > 0);
            Assert.True(testListener.HandshakeData.PingInterval > 0);
            Assert.True(testListener.HandshakeData.PingTimeout > 0);
        }


        [Fact]
        public async Task Upgrade()
        {
            Socket.SetupLog4Net();

            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            var events = new ConcurrentQueue<object>();

            var socket = new Socket(CreateOptionsSecure());

            socket.On(Socket.EVENT_UPGRADING, (data) =>
            {
                log.Info(Socket.EVENT_UPGRADING + string.Format(" data = {0}", data));
                events.Enqueue(data);
            });
            socket.On(Socket.EVENT_UPGRADE, (data) =>
            {
                log.Info(Socket.EVENT_UPGRADE + string.Format(" data = {0}", data));
                events.Enqueue(data);

            });

            socket.Open();


            object test = null;
            events.TryDequeue(out test);
            Assert.NotNull(test);
            Assert.IsAssignableFrom<Transport>(test);

            events.TryDequeue(out test);
            Assert.NotNull(test);
            Assert.IsAssignableFrom<Transport>(test);
            await Task.Delay(3000);
            socket.Close();
        }



        [Fact]
        public async Task RememberWebsocket()
        {
            Socket.SetupLog4Net();

            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            var socket1 = new Socket(CreateOptionsSecure());
            string socket1TransportName = null;
            string socket2TransportName = null;

            socket1.On(Socket.EVENT_OPEN, () =>
            {
                log.Info("EVENT_OPEN");
                socket1TransportName = socket1.Transport.Name;
            });

            socket1.On(Socket.EVENT_UPGRADE, (data) =>
            {
                log.Info(Socket.EVENT_UPGRADE + string.Format(" data = {0}", data));
                var transport = (Transport)data;
                socket1.Close();
                if (WebSocket.NAME == transport.Name)
                {
                    var options = CreateOptionsSecure();
                    options.RememberUpgrade = true;
                    var socket2 = new Socket(options);
                    socket2.Open();
                    socket2TransportName = socket2.Transport.Name;
                    socket2.Close();
                }
            });

            socket1.Open();
            await Task.Delay(1000);
            Assert.Equal(Polling.NAME, socket1TransportName);
            Assert.Equal(WebSocket.NAME, socket2TransportName);
        }



        [Fact]
        public void NotRememberWebsocket()
        {
            Socket.SetupLog4Net();

            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            var socket1 = new Socket(CreateOptionsSecure());
            string socket1TransportName = null;
            string socket2TransportName = null;

            socket1.On(Socket.EVENT_OPEN, () =>
            {
                log.Info("EVENT_OPEN");
                socket1TransportName = socket1.Transport.Name;
            });

            socket1.On(Socket.EVENT_UPGRADE, (data) =>
            {
                log.Info(Socket.EVENT_UPGRADE + string.Format(" data = {0}", data));
                var transport = (Transport)data;
                if (WebSocket.NAME == transport.Name)
                {
                    socket1.Close();
                    var options = CreateOptionsSecure();
                    options.RememberUpgrade = false;
                    var socket2 = new Socket(options);
                    socket2.On(Socket.EVENT_OPEN, () =>
                    {
                        log.Info("EVENT_OPEN socket 2");
                        socket2TransportName = socket2.Transport.Name;
                        socket2.Close();
                    });
                    socket2.Open();
                }
            });

            socket1.Open();
            Assert.Equal(Polling.NAME, socket1TransportName);
            Assert.Equal(Polling.NAME, socket2TransportName);
        }




    }


}

