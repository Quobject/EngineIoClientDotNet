//using log4net;

using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using EngineIoClientDotNet.Modules;
using Quobject.EngineIoClientDotNet.Client;
using Quobject.EngineIoClientDotNet.Client.Transports;
using Quobject.EngineIoClientDotNet.ComponentEmitter;
using Xunit;

namespace Quobject.EngineIoClientDotNet_Tests.ClientTests
{
    public class ConnectionTest : Connection
    {
        private Socket socket;
        public string Message;

        [Fact]
        public void ConnectToLocalhost()
        {

            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");

            socket = new Socket(CreateOptions());
            socket.On(Socket.EVENT_OPEN, new TestListener());
            socket.On(Socket.EVENT_MESSAGE, new MessageListener(socket, this));
            socket.Open();

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
                connectionTest.Message = (string) args[0];
                socket.Close();
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

            this.Message = "";

            var options = CreateOptions();
            options.Transports = ImmutableList.Create<string>(Polling.NAME);
            socket = new Socket(options);

            //socket = new Socket(CreateOptions());
            socket.On(Socket.EVENT_OPEN, () =>
            {
                log.Info("open");
                //socket.Send("test send");

            });
            socket.On(Socket.EVENT_MESSAGE, (d) =>
            {
                var data = (string) d;

                log.Info("message2 = " + data);
                this.Message = data;
                socket.Close();
            });
            //socket.Open();
            socket.Open();
            //socket.Send("test send");
            //Task.Delay(TimeSpan.FromSeconds(3)).Wait();
            //socket.Close();
            Assert.Equal("hi", this.Message);
        }

        [Fact]
        public void TestmultibyteUtf8StringsWithPolling()
        {

            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");


            const string SendMessage = "cash money €€€";


            socket = new Socket(CreateOptions());
            socket.On(Socket.EVENT_OPEN, () =>
            {
                log.Info("open");

                socket.On(Socket.EVENT_MESSAGE, (d) =>
                {
                    var data = (string) d;

                    log.Info("TestMessage data = " + data);

                    if (data == "hi")
                    {
                        return;
                    }

                    this.Message = data;
                    socket.Close();
                });
                socket.Send(SendMessage);
            });

            socket.Open();
            log.Info("TestmultibyteUtf8StringsWithPolling this.Message = " + this.Message);
            Assert.Equal(SendMessage, this.Message);
        }



        [Fact]
        public void Testemoji()
        {

            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");

            const string SendMessage = "\uD800-\uDB7F\uDB80-\uDBFF\uDC00-\uDFFF\uE000-\uF8FF";


            socket = new Socket(CreateOptions());
            socket.On(Socket.EVENT_OPEN, () =>
            {
                log.Info("open");

                socket.On(Socket.EVENT_MESSAGE, (d) =>
                {
                    var data = (string) d;

                    log.Info(Socket.EVENT_MESSAGE);

                    if (data == "hi")
                    {
                        return;
                    }

                    this.Message = data;
                    socket.Close();
                });
                socket.Send(SendMessage);
            });

            socket.Open();
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
            log.Info("NotSendPacketsIfSocketCloses end noPacket = " + noPacket);
            Assert.True(noPacket);
        }


    }
}
