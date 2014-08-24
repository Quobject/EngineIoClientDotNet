using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Quobject.EngineIoClientDotNet.Client;
using Quobject.EngineIoClientDotNet.Client.Transports;
using Quobject.EngineIoClientDotNet.ComponentEmitter;
using Xunit;

namespace Quobject.EngineIoClientDotNet_Tests.ClientTests
{
    public class ServerConnectionTest : Connection
    {
        [Fact]
        public void OpenAndClose()
        {
            Socket.SetupLog4Net();

            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            var events = new ConcurrentQueue<string>();

            var socket = new Socket(CreateOptions());
            socket.On(Socket.EVENT_OPEN, () =>
            {
                log.Info("EVENT_OPEN");
                events.Enqueue(Socket.EVENT_OPEN);

            });
            socket.On(Socket.EVENT_CLOSE, () =>
            {
                log.Info("EVENT_CLOSE");
                events.Enqueue(Socket.EVENT_CLOSE);
            });
            socket.Open();
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(3));

            socket.Close();
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(0.1));

            string result;
            events.TryDequeue(out result);
            Assert.Equal(Socket.EVENT_OPEN, result);
            events.TryDequeue(out result);
            Assert.Equal(Socket.EVENT_CLOSE, result);
        }


        [Fact]
        public void Messages()
        {
            Socket.SetupLog4Net();

            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
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
            });
            socket.Open();
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(3));

            socket.Close();

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

            var socket = new Socket(CreateOptions());

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
            private Socket socket;
            public HandshakeData HandshakeData;

            public TestHandshakeListener(Socket socket)
            {
                this.socket = socket;
            }

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

            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            var socket = new Socket(CreateOptions());
            var testListener = new TestHandshakeListener(socket);
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
        public void Upgrade()
        {
            Socket.SetupLog4Net();

            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
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
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(5));
            socket.Close();

            object test = null;
            events.TryDequeue(out test);
            Assert.NotNull(test);
            Assert.IsAssignableFrom<Transport>(test);

            events.TryDequeue(out test);
            Assert.NotNull(test);
            Assert.IsAssignableFrom<Transport>(test);
        }



        [Fact]
        public void RememberWebsocket()
        {
            Socket.SetupLog4Net();

            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            var socket = new Socket(CreateOptions());
            string socket2TransportName = null;

            socket.On(Socket.EVENT_UPGRADE, (data) =>
            {
                log.Info(Socket.EVENT_UPGRADE + string.Format(" data = {0}", data));
                var transport = (Transport) data;
                socket.Close();
                if (WebSocket.NAME == transport.Name)
                {
                    var options = CreateOptions();
                    options.RememberUpgrade = true;
                    var socket2 = new Socket(options);
                    socket2.Open();
                    System.Threading.Thread.Sleep(TimeSpan.FromSeconds(2));
                    socket2TransportName = socket2.Transport.Name;
                    socket2.Close();
                }
            });

            socket.Open();
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1));
            Assert.Equal(Polling.NAME,socket.Transport.Name);
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(5));

            Assert.Equal(WebSocket.NAME, socket2TransportName);
        }




        [Fact]
        public void NotRememberWebsocket()
        {
            Socket.SetupLog4Net();

            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            var socket = new Socket(CreateOptions());
            string socket2TransportName = null;

            socket.On(Socket.EVENT_UPGRADE, (data) =>
            {
                log.Info(Socket.EVENT_UPGRADE + string.Format(" data = {0}", data));
                var transport = (Transport)data;
                socket.Close();
                if (WebSocket.NAME == transport.Name)
                {
                    var options = CreateOptions();
                    options.RememberUpgrade = false;
                    var socket2 = new Socket(options);
                    socket2.Open();
                    System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1));
                    socket2TransportName = socket2.Transport.Name;
                    socket2.Close();
                }
            });

            socket.Open();
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1));
            Assert.Equal(Polling.NAME, socket.Transport.Name);
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1));

            Assert.NotEqual(WebSocket.NAME, socket2TransportName);
        }









        //[Fact]
        //public void PollingHeaders()
        //{
        //    Socket.SetupLog4Net();

        //    var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        //    var message = new ConcurrentQueue<object>();

        //    var options = CreateOptions();
        //    options.Transports = ImmutableList.Create(Polling.NAME);
        //    var socket = new Socket(CreateOptions());

        //    socket.On(Socket.EVENT_TRANSPORT, (data) =>
        //    {
        //        var transport = (Transport) data;
        //        transport.On(Transport.EVENT_REQUEST_HEADERS, o =>
        //        {
        //            log.Info(Transport.EVENT_REQUEST_HEADERS + string.Format(" data = {0}", o));
        //            var headers = (Dictionary<string, string>)o;
        //            log.Info(Transport.EVENT_REQUEST_HEADERS + string.Format(" headers = {0}", headers.ToString()));                    
        //            headers.Add("X-EngineIO", "foo");
        //        });
        //        transport.On(Transport.EVENT_RESPONSE_HEADERS, o =>
        //        {
        //            log.Info(Transport.EVENT_RESPONSE_HEADERS + string.Format(" data = {0}", o));
        //            var headers = (Dictionary<string, string>)o;
        //            log.Info(Transport.EVENT_RESPONSE_HEADERS + string.Format(" headers = {0}", headers.ToString()));                   
        //            string result;
        //            headers.TryGetValue("X-EngineIO", out result);
        //            message.Enqueue(result);
        //        });
        //    });
           
        //    socket.Open();
        //    System.Threading.Thread.Sleep(TimeSpan.FromSeconds(6));
        //    socket.Close();

        //    object test = null;
        //    message.TryDequeue(out test);
        //    log.Info(string.Format("PollingHeaders test1 = {0}", test));
        //    //Assert.NotNull(test);
        //    Assert.True(message.Count == 1);
        //}




    }


}

