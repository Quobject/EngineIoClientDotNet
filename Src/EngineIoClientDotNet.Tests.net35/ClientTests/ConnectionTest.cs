using Quobject.EngineIoClientDotNet.Client;
using Quobject.EngineIoClientDotNet.ComponentEmitter;
using Quobject.EngineIoClientDotNet.Modules;
using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;

namespace Quobject.EngineIoClientDotNet_Tests.ClientTests
{
    public class ConnectionTest : Connection
    {
        private ManualResetEvent _manualResetEvent = null;
        private Socket socket;
        public string Message;

        [Fact]
        public void ConnectToLocalhost()
        {
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");
            _manualResetEvent = new ManualResetEvent(false);

            var options = CreateOptions();

            socket = new Socket(options);
            socket.On(Socket.EVENT_OPEN, new TestListener());
            socket.On(Socket.EVENT_MESSAGE, new MessageListener(socket, this));
            socket.Open();
            _manualResetEvent.WaitOne();
            socket.Close();
            Assert.Equal("hi", this.Message);
        }

        public class TestListener : IListener
        {
            public void Call(params object[] args)
            {
                var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
                log.Info("open");
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

        public class MessageListener : IListener
        {
            private Socket socket;
            private ConnectionTest connectionTest;

            public MessageListener(Socket socket)
            {
                this.socket = socket;
            }

            public MessageListener(Socket socket, ConnectionTest connectionTest)
            {
                this.socket = socket;
                this.connectionTest = connectionTest;
            }

            public void Call(params object[] args)
            {
                var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
                log.Info("message = " + args[0]);
                connectionTest.Message = (string)args[0];
                connectionTest._manualResetEvent.Set();
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
        public void ConnectToLocalhost2()
        {
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");
            _manualResetEvent = new ManualResetEvent(false);
            this.Message = "";

            var options = CreateOptions();
            options.Transports = new List<string>() { "polling" };
            socket = new Socket(options);

            //socket = new Socket(CreateOptions());
            socket.On(Socket.EVENT_OPEN, () =>
            {
                log.Info("open");
                //socket.Send("test send");
            });
            socket.On(Socket.EVENT_MESSAGE, (d) =>
            {
                var data = (string)d;

                log.Info("message2 = " + data);
                this.Message = data;
                _manualResetEvent.Set();
            });

            socket.Open();
            _manualResetEvent.WaitOne();
            socket.Close();
            Assert.Equal("hi", this.Message);
        }

        [Fact]
        public void TestmultibyteUtf8StringsWithPolling()
        {
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");
            _manualResetEvent = new ManualResetEvent(false);

            const string SendMessage = "cash money €€€";

            socket = new Socket(CreateOptions());
            socket.On(Socket.EVENT_OPEN, () =>
            {
                log.Info("open");

                socket.Send(SendMessage);
            });
            socket.On(Socket.EVENT_MESSAGE, (d) =>
            {
                var data = (string)d;

                log.Info("TestMessage data = " + data);

                if (data == "hi")
                {
                    return;
                }

                this.Message = data;
                _manualResetEvent.Set();
            });

            socket.Open();
            _manualResetEvent.WaitOne();
            socket.Close();
            log.Info("TestmultibyteUtf8StringsWithPolling this.Message = " + this.Message);
            Assert.Equal(SendMessage, this.Message);
        }

        [Fact]
        public void Testemoji()
        {
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");
            _manualResetEvent = new ManualResetEvent(false);
            const string SendMessage = "\uD800-\uDB7F\uDB80-\uDBFF\uDC00-\uDFFF\uE000-\uF8FF";

            var options = CreateOptions();
            socket = new Socket(options);

            socket.On(Socket.EVENT_OPEN, () =>
            {
                log.Info("open");

                socket.Send(SendMessage);
            });

            socket.On(Socket.EVENT_MESSAGE, (d) =>
            {
                var data = (string)d;

                log.Info(Socket.EVENT_MESSAGE);

                if (data == "hi")
                {
                    return;
                }

                this.Message = data;
                _manualResetEvent.Set();
            });

            socket.Open();
            _manualResetEvent.WaitOne();
            socket.Close();

            Assert.True(SendMessage == this.Message);
        }

        [Fact]
        public void NotSendPacketsIfSocketCloses()
        {
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");

            var noPacket = true;

            socket = new Socket(CreateOptions());
            socket.On(Socket.EVENT_OPEN, () =>
            {
                noPacket = true;
            });

            socket.Open();
            socket.On(Socket.EVENT_PACKET_CREATE, () =>
            {
                noPacket = false;
                log.Info("NotSendPacketsIfSocketCloses EVENT_PACKET_CREATE noPacket = " + noPacket);
            });
            socket.Close();
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1));
            //await Task.Delay(1000);
            log.Info("NotSendPacketsIfSocketCloses end noPacket = " + noPacket);
            Assert.True(noPacket);
        }
    }
}