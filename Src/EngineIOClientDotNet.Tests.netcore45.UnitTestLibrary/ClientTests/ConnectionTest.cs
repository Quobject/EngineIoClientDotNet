//using log4net;

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using EngineIoClientDotNet.Modules;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Quobject.EngineIoClientDotNet.Client;
using Quobject.EngineIoClientDotNet.ComponentEmitter;
using Quobject.EngineIoClientDotNet.Modules;


namespace Quobject.EngineIoClientDotNet_Tests.ClientTests
{
    [TestClass]
    public class ConnectionTest : Connection
    {
        private Socket socket;
        public string Message;

        [TestMethod]
        public void ConnectToLocalhost()
        {

            var log = LogManager.GetLogger(Global.CallerName());
            log.Info("Start");

            socket = new Socket(CreateOptions());
            socket.On(Socket.EVENT_OPEN, new TestListener());
            socket.On(Socket.EVENT_MESSAGE, new MessageListener(socket, this));
            socket.Open();
            Task.Delay(TimeSpan.FromSeconds(3)).Wait();
            Assert.AreEqual("hi", this.Message);
        }

 


        public class TestListener : IListener
        {


            public void Call(params object[] args)
            {
    
                var log = LogManager.GetLogger(Global.CallerName());
                log.Info("open");
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
    
                var log = LogManager.GetLogger(Global.CallerName());
                log.Info("message = " + args[0]);
                connectionTest.Message = (string) args[0];
                socket.Close();
            }
        }


        AutoResetEvent _autoResetEvent;

        [TestMethod]
        public void ConnectToLocalhost2()
        {

            var log = LogManager.GetLogger(Global.CallerName());
            log.Info("Start");
            this._autoResetEvent = new AutoResetEvent(false);
            this.Message = "";

            socket = new Socket(CreateOptions());
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
                this._autoResetEvent.Set(); 
            });
            socket.Open();
            this._autoResetEvent.WaitOne();
            socket.Close();

            Assert.AreEqual("hi", this.Message);
        }

        [TestMethod]
        public void TestmultibyteUtf8StringsWithPolling()
        {

            var log = LogManager.GetLogger(Global.CallerName());
            log.Info("Start");
            this._autoResetEvent = new AutoResetEvent(false);

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
                    this._autoResetEvent.Set(); 
                });
                socket.Send(SendMessage);
            });

            socket.Open();
            this._autoResetEvent.WaitOne();
            socket.Close();

            log.Info("TestmultibyteUtf8StringsWithPolling this.Message = " + this.Message);
            Assert.AreEqual(SendMessage, this.Message);
        }



        [TestMethod]
        public void Testemoji()
        {

            var log = LogManager.GetLogger(Global.CallerName());
            log.Info("Start");
            this._autoResetEvent = new AutoResetEvent(false);

            const string SendMessage = "\uD800-\uDB7F\uDB80-\uDBFF\uDC00-\uDFFF\uE000-\uF8FF";


            socket = new Socket(CreateOptions());
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
                this._autoResetEvent.Set();
            });

            socket.Open();
            this._autoResetEvent.WaitOne();
            socket.Close();
            Assert.AreEqual(SendMessage , this.Message);

        }


        [TestMethod]
        public void NotSendPacketsIfSocketCloses()
        {

            var log = LogManager.GetLogger(Global.CallerName());
            log.Info("Start");


            var noPacket = true;

            socket = new Socket(CreateOptions());
            socket.On(Socket.EVENT_OPEN, () =>
            {
                noPacket = true;

            });


            socket.On(Socket.EVENT_PACKET_CREATE, () =>
            {
                noPacket = false;
                log.Info("NotSendPacketsIfSocketCloses EVENT_PACKET_CREATE noPacket = " + noPacket);
            });
            socket.Open();
            socket.Close();
            //System.Threading.Thread.Sleep(TimeSpan.FromSeconds(2));
            log.Info("NotSendPacketsIfSocketCloses end noPacket = " + noPacket);
            Assert.IsTrue(noPacket);
        }


    }
}
