using Quobject.EngineIoClientDotNet.Client;
using Quobject.EngineIoClientDotNet.Client.Transports;
using Quobject.EngineIoClientDotNet.ComponentEmitter;
using Quobject.EngineIoClientDotNet.Modules;
using System.Collections.Generic;
using System.Threading;
using Xunit;

namespace Quobject.EngineIoClientDotNet_Tests.ClientTests
{
    public class ServerConnectionTest : Connection
    {
        private ManualResetEvent _manualResetEvent = null;

        [Fact]
        public void OpenAndClose()
        {
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");
            _manualResetEvent = new ManualResetEvent(false);

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
                _manualResetEvent.Set();
            });

            socket.Open();
            _manualResetEvent.WaitOne();
            socket.Close();
            string result;
            result = events.Dequeue();
            Assert.Equal(Socket.EVENT_OPEN, result);
            result = events.Dequeue();
            Assert.Equal(Socket.EVENT_CLOSE, result);
        }

        [Fact]
        public void Messages()
        {
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");
            _manualResetEvent = new ManualResetEvent(false);

            var events = new Queue<string>();

            var socket = new Socket(CreateOptions());
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
                    _manualResetEvent.Set();
                }
            });
            socket.Open();
            _manualResetEvent.WaitOne();
            socket.Close();

            string result;
            result = events.Dequeue();
            Assert.Equal("hi", result);
            result = events.Dequeue();
            Assert.Equal("hello", result);
        }

        [Fact]
        public void Handshake()
        {
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

            Assert.NotNull(handshake_data);
            Assert.NotNull(handshake_data.Upgrades);
            Assert.True(handshake_data.Upgrades.Count > 0);
            Assert.True(handshake_data.PingInterval > 0);
            Assert.True(handshake_data.PingTimeout > 0);
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

        [Fact]
        public void Handshake2()
        {
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");
            _manualResetEvent = new ManualResetEvent(false);

            var socket = new Socket(CreateOptions());
            var testListener = new TestHandshakeListener(this);
            socket.On(Socket.EVENT_HANDSHAKE, testListener);
            socket.Open();
            _manualResetEvent.WaitOne();
            socket.Close();

            Assert.NotNull(testListener.HandshakeData);
            Assert.NotNull(testListener.HandshakeData.Upgrades);
            Assert.True(testListener.HandshakeData.Upgrades.Count > 0);
            Assert.True(testListener.HandshakeData.PingInterval > 0);
            Assert.True(testListener.HandshakeData.PingTimeout > 0);
        }

        [Fact]
        public void Upgrade()
        {
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");
            _manualResetEvent = new ManualResetEvent(false);

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
                _manualResetEvent.Set();
            });

            socket.Open();
            _manualResetEvent.WaitOne();

            object test = null;
            test = events.Dequeue();
            Assert.NotNull(test);
            Assert.IsAssignableFrom<Transport>(test);

            test = events.Dequeue();
            Assert.NotNull(test);
            Assert.IsAssignableFrom<Transport>(test);
        }

        [Fact]
        public void RememberWebsocket()
        {
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");
            _manualResetEvent = new ManualResetEvent(false);

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
                var transport = (Transport)data;
                socket1.Close();
                if (WebSocket.NAME == transport.Name)
                {
                    var options = CreateOptions();
                    options.RememberUpgrade = true;
                    var socket2 = new Socket(options);
                    socket2.Open();
                    socket2TransportName = socket2.Transport.Name;
                    socket2.Close();
                    _manualResetEvent.Set();
                }
            });

            socket1.Open();
            _manualResetEvent.WaitOne();
            Assert.Equal(Polling.NAME, socket1TransportName);
            Assert.Equal(WebSocket.NAME, socket2TransportName);
        }

        [Fact]
        public void NotRememberWebsocket()
        {
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");
            _manualResetEvent = new ManualResetEvent(false);

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
                var transport = (Transport)data;
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
                        _manualResetEvent.Set();
                    });
                    socket2.Open();
                }
            });

            socket1.Open();
            _manualResetEvent.WaitOne();
            Assert.Equal(Polling.NAME, socket1TransportName);
            Assert.Equal(Polling.NAME, socket2TransportName);
        }

        [Fact]
        public void Cookie()
        {
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
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
            Assert.Equal("hi", result);
            result = events.Dequeue();
            Assert.Equal("got cookie", result);
        }

        [Fact]
        public void UpgradeCookie()
        {
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
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
            Assert.NotNull(test);
            Assert.IsAssignableFrom<Transport>(test);

            test = events.Dequeue();
            Assert.NotNull(test);
            Assert.IsAssignableFrom<Transport>(test);
            test = events.Dequeue();
            Assert.Equal("got cookie", test);
        }
    }
}