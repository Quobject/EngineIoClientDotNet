//using log4net;

using EngineIoClientDotNet.Modules;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Quobject.EngineIoClientDotNet.Client;
using Quobject.EngineIoClientDotNet.Client.Transports;
using Quobject.EngineIoClientDotNet.ComponentEmitter;
using System;

using System.Threading.Tasks;


namespace Quobject.EngineIoClientDotNet_Tests.ClientTests
{
    [TestClass]
    public class SSLServerConnectionTest : Connection
    {
        [TestMethod]
        public async Task OpenAndClose()
        {
            LogManager.SetupLogManager();
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");

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
            Assert.AreEqual(Socket.EVENT_OPEN, result);
            events.TryDequeue(out result);
            Assert.AreEqual(Socket.EVENT_CLOSE, result);
            await Task.Delay(1);
            socket.Close();
        }


        [TestMethod]
        public void Messages()
        {
            LogManager.SetupLogManager();
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");

            var events = new ConcurrentQueue<string>();

            var socket = new Socket(CreateOptionsSecure());
            socket.On(Socket.EVENT_OPEN, () =>
            {
                log.Info("EVENT_OPEN");
                socket.Send("hello");
            });
            socket.On(Socket.EVENT_MESSAGE, (d) =>
            {
                var data = (string) d;
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
            Assert.AreEqual("hi", result);
            events.TryDequeue(out result);
            Assert.AreEqual("hello", result);
        }

        [TestMethod]
        public void Handshake()
        {
            LogManager.SetupLogManager();
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");

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
                LogManager.SetupLogManager();
                var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
                log.Info(string.Format("open args[0]={0} args.Length={1}", args[0], args.Length));
                HandshakeData = args[0] as HandshakeData;
            }
        }

        [TestMethod]
        public void Handshake2()
        {
            LogManager.SetupLogManager();
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");


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


        [TestMethod]
        public async Task Upgrade()
        {
            LogManager.SetupLogManager();
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");

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



        [TestMethod]
        public async Task RememberWebsocket()
        {
            LogManager.SetupLogManager();
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");

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
                var transport = (Transport) data;
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
            Assert.AreEqual(Polling.NAME, socket1TransportName);
            Assert.AreEqual(WebSocket.NAME, socket2TransportName);
        }



        [TestMethod]
        public void NotRememberWebsocket()
        {
            LogManager.SetupLogManager();
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");

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
                var transport = (Transport) data;
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
            Assert.AreEqual(Polling.NAME, socket1TransportName);
            Assert.AreEqual(Polling.NAME, socket2TransportName);
        }




    }


}

