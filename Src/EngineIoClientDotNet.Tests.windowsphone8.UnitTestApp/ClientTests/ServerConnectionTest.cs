//using log4net;

using System.Collections.Generic;
using System.Threading;

using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Quobject.EngineIoClientDotNet.Client;
using Quobject.EngineIoClientDotNet.Client.Transports;
using Quobject.EngineIoClientDotNet.ComponentEmitter;
using System;

using System.Threading.Tasks;
using Quobject.EngineIoClientDotNet.Modules;


namespace Quobject.EngineIoClientDotNet_Tests.ClientTests
{
    [TestClass]
    public class ServerConnectionTest : Connection
    {
        private ManualResetEvent _manualResetEvent = null;


        [TestMethod]
        public void OpenAndClose()
        {
            LogManager.SetupLogManager();
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");
            this._autoResetEvent = new AutoResetEvent(false);

            var events = new Queue<string>();

            var socket = new Socket(CreateOptions());
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
                this._autoResetEvent.Set(); 
            });
            socket.Open();
            log.Info("After open");
            this._autoResetEvent.WaitOne();
            string result;
            log.Info("Before dequeue events.count="+events.Count);
            result = events.Dequeue();
            Assert.AreEqual(Socket.EVENT_OPEN, result);
            result = events.Dequeue();
            Assert.AreEqual(Socket.EVENT_CLOSE, result);
        }

        AutoResetEvent _autoResetEvent;

        [TestMethod]
        public void Messages()
        {
            LogManager.SetupLogManager();
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");
            //Setting the event to false.
            this._autoResetEvent = new AutoResetEvent(false);

            var events = new Queue<string>();

            var socket = new Socket(CreateOptions());
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
                    log.Info("EVENT_MESSAGE 2"); 
                    this._autoResetEvent.Set(); 
                }
            });
            socket.Open();
            this._autoResetEvent.WaitOne();
            socket.Close();

            string result;
            result = events.Dequeue();
            Assert.AreEqual("hi", result);
            result = events.Dequeue();
            Assert.AreEqual("hello", result);

            

        }

        [TestMethod]
        public void Handshake()
        {
            LogManager.SetupLogManager();
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");
            _manualResetEvent = new ManualResetEvent(false);

            HandshakeData handshake_data = null;

            var socket = new Socket(CreateOptions());

            socket.On(Socket.EVENT_HANDSHAKE, (data) =>
            {
                log.Info(Socket.EVENT_HANDSHAKE + string.Format(" data = {0}", data));
                handshake_data = data as HandshakeData;
                _manualResetEvent.Set();
            });

            socket.Open();
            _manualResetEvent.WaitOne();	
            socket.Close();

            Assert.IsNotNull(handshake_data);
            Assert.IsNotNull(handshake_data.Upgrades);
            Assert.IsTrue(handshake_data.Upgrades.Count > 0);
            Assert.IsTrue(handshake_data.PingInterval > 0);
            Assert.IsTrue(handshake_data.PingTimeout > 0);
        }


        public class TestHandshakeListener : IListener
        {
            public HandshakeData HandshakeData;
            private ServerConnectionTest serverConnectionTest;

            public TestHandshakeListener(ServerConnectionTest serverConnectionTest)
            {
                this.serverConnectionTest = serverConnectionTest;
            }



            public void Call(params object[] args)
            {
                LogManager.SetupLogManager();
                var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
                log.Info(string.Format("open args[0]={0} args.Length={1}", args[0], args.Length));
                HandshakeData = args[0] as HandshakeData;
                serverConnectionTest._manualResetEvent.Set();
            }

            public int CompareTo(IListener other)
            {
                return this.GetId().CompareTo(other.GetId());
            }

            public int GetId()
            {
                return 0;
            }
        }

        [TestMethod]
        public void Handshake2()
        {

            LogManager.SetupLogManager();
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");
            _manualResetEvent = new ManualResetEvent(false);


            var socket = new Socket(CreateOptions());
            var testListener = new TestHandshakeListener(this);
            socket.On(Socket.EVENT_HANDSHAKE, testListener);
            socket.Open();
            _manualResetEvent.WaitOne();
            socket.Close();

            Assert.IsNotNull(testListener.HandshakeData);
            Assert.IsNotNull(testListener.HandshakeData.Upgrades);
            Assert.IsTrue(testListener.HandshakeData.Upgrades.Count > 0);
            Assert.IsTrue(testListener.HandshakeData.PingInterval > 0);
            Assert.IsTrue(testListener.HandshakeData.PingTimeout > 0);
        }


        [TestMethod]
        public void Upgrade()
        {
            LogManager.SetupLogManager();
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");
            this._autoResetEvent = new AutoResetEvent(false);

            var events = new Queue<object>();

            var socket = new Socket(CreateOptions());

            socket.On(Socket.EVENT_UPGRADING, (data) =>
            {
                log.Info(Socket.EVENT_UPGRADING + string.Format(" data = {0}", data));
                events.Enqueue(data);
            });
            socket.On(Socket.EVENT_UPGRADE, (data) =>
            {
                log.Info(Socket.EVENT_UPGRADE + string.Format(" data = {0}", data));
                events.Enqueue(data);
                this._autoResetEvent.Set(); 
            });

            socket.Open();
            this._autoResetEvent.WaitOne();

            object test = null;
            test = events.Dequeue();
            Assert.IsNotNull(test);
            //Assert.IsAssignableFrom<Transport>(test);

            test = events.Dequeue();
            Assert.IsNotNull(test);
            //Assert.IsAssignableFrom<Transport>(test);
            //await Task.Delay(3000);
            socket.Close();
        }



        [TestMethod]
        public void RememberWebsocket()
        {
            LogManager.SetupLogManager();
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");
            this._autoResetEvent = new AutoResetEvent(false);

            var socket1 = new Socket(CreateOptions());
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
                    var options = CreateOptions();
                    options.RememberUpgrade = true;
                    var socket2 = new Socket(options);
                    socket2.Open();
                    socket2TransportName = socket2.Transport.Name;
                    this._autoResetEvent.Set(); 
                    socket2.Close();
                }
            });

            socket1.Open();
            this._autoResetEvent.WaitOne();
            Assert.AreEqual(Polling.NAME, socket1TransportName);
            Assert.AreEqual(WebSocket.NAME, socket2TransportName);
        }



        [TestMethod]
        public void NotRememberWebsocket()
        {
            LogManager.SetupLogManager();
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");
            this._autoResetEvent = new AutoResetEvent(false);

            var socket1 = new Socket(CreateOptions());
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
                    var options = CreateOptions();
                    options.RememberUpgrade = false;
                    var socket2 = new Socket(options);
                    socket2.On(Socket.EVENT_OPEN, () =>
                    {
                        log.Info("EVENT_OPEN socket 2");
                        socket2TransportName = socket2.Transport.Name;
                        socket2.Close();
                        this._autoResetEvent.Set(); 
                    });
                    socket2.Open();
                }
            });

            socket1.Open();
            this._autoResetEvent.WaitOne();
            Assert.AreEqual(Polling.NAME, socket1TransportName);
            Assert.AreEqual(Polling.NAME, socket2TransportName);
        }

        [TestMethod]
        public void Cookie()
        {
            var log = LogManager.GetLogger(Global.CallerName());
            log.Info("Start");
            _manualResetEvent = new ManualResetEvent(false);

            var events = new Queue<string>();

            var options = CreateOptions();
            options.Cookies.Add("foo", "bar");
            var socket = new Socket(options);
            socket.On(Socket.EVENT_OPEN, () =>
            {
                log.Info("EVENT_OPEN");
                socket.Send("cookie");
            });
            socket.On(Socket.EVENT_MESSAGE, (d) =>
            {
                var data = (string)d;
                log.Info("EVENT_MESSAGE data = " + data);
                events.Enqueue(data);
                if (events.Count > 1)
                {
                    _manualResetEvent.Set();
                }
            });
            socket.Open();
            _manualResetEvent.WaitOne();
            socket.Close();

            string result;
            result = events.Dequeue();
            Assert.AreEqual("hi", result);
            result = events.Dequeue();
            Assert.AreEqual("got cookie", result);
        }


        [TestMethod]
        public void UpgradeCookie()
        {

            var log = LogManager.GetLogger(Global.CallerName());
            log.Info("Start");
            _manualResetEvent = new ManualResetEvent(false);

            var events = new Queue<object>();

            var options = CreateOptions();
            options.Cookies.Add("foo", "bar");
            var socket = new Socket(options);

            socket.On(Socket.EVENT_UPGRADING, (data) =>
            {
                log.Info(Socket.EVENT_UPGRADING + string.Format(" data = {0}", data));
                events.Enqueue(data);
            });

            socket.On(Socket.EVENT_UPGRADE, (data) =>
            {
                log.Info(Socket.EVENT_UPGRADE + string.Format(" data = {0}", data));
                events.Enqueue(data);
                socket.Send("cookie");
            });

            socket.On(Socket.EVENT_MESSAGE, (d) =>
            {

                if (events.Count > 1)
                {
                    var data = (string)d;
                    log.Info("EVENT_MESSAGE data = " + data);
                    events.Enqueue(data);
                    _manualResetEvent.Set();
                }
            });

            socket.Open();
            _manualResetEvent.WaitOne();

            object test = null;
            test = events.Dequeue();
            Assert.IsNotNull(test);

            test = events.Dequeue();
            Assert.IsNotNull(test);
            test = events.Dequeue();
            Assert.AreEqual("got cookie", test);
        }

    }

}

