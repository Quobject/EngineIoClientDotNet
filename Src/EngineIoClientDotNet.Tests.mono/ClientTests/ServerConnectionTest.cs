//using log4net;

using System.Threading;
using EngineIoClientDotNet.Modules;
using Quobject.EngineIoClientDotNet.Client;
using Quobject.EngineIoClientDotNet.Client.Transports;
using Quobject.EngineIoClientDotNet.ComponentEmitter;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Xunit;

namespace Quobject.EngineIoClientDotNet_Tests.ClientTests
{
    public class ServerConnectionTest : Connection
    {

        AutoResetEvent _autoResetEvent;


        [Fact]
        public async Task OpenAndClose()
        {

            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");

            this._autoResetEvent = new AutoResetEvent(false);

            var events = new ConcurrentQueue<string>();

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
            log.Info("AFTER socket.Open()");
            this._autoResetEvent.WaitOne();
            log.Info("AFTER WaitOne()");
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

            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");

            var events = new ConcurrentQueue<string>();

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

            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");
            //this._autoResetEvent = new AutoResetEvent(false);

            HandshakeData handshake_data = null;

            var socket = new Socket(CreateOptions());

            socket.On(Socket.EVENT_HANDSHAKE, (data) =>
            {
                log.Info(Socket.EVENT_HANDSHAKE + string.Format(" data = {0}", data));
                handshake_data = data as HandshakeData;
                //this._autoResetEvent.Set();
            });

            socket.Open();
            //log.Info("AFTER socket.Open()");
            //this._autoResetEvent.WaitOne();
            //log.Info("AFTER WaitOne()");
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
    
                var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
                log.Info(string.Format("open args[0]={0} args.Length={1}", args[0], args.Length));
                HandshakeData = args[0] as HandshakeData;
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


            var socket = new Socket(CreateOptions());
            var testListener = new TestHandshakeListener();
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

            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");


            var events = new ConcurrentQueue<object>();

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

            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");

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
                    socket2.Close();
                }
            });

            socket1.Open();
            await Task.Delay(5);
            Assert.Equal(Polling.NAME, socket1TransportName);
            Assert.Equal(WebSocket.NAME, socket2TransportName);
        }



        [Fact]
        public void NotRememberWebsocket()
        {

            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");

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

