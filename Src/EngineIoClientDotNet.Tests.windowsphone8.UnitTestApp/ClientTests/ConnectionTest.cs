//using log4net;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EngineIoClientDotNet.Modules;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Quobject.Collections.Immutable;
using Quobject.EngineIoClientDotNet.Client;
using Quobject.EngineIoClientDotNet.Client.Transports;
using Quobject.EngineIoClientDotNet.ComponentEmitter;


namespace Quobject.EngineIoClientDotNet_Tests.ClientTests
{
    [TestClass]
    public class ConnectionTest : Connection
    {
        private Socket socket;
        public string Message;

        //[TestMethod]
        //public async Task PingTest()
        //{

        //    var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
        //    log.Info("Start");

        //    var binaryData = new byte[5];
        //    for (int i = 0; i < binaryData.Length; i++)
        //    {
        //        binaryData[i] = (byte)i;
        //    }

        //    var events = new Queue<object>();


        //    var options = CreateOptions();
        //    options.Transports = ImmutableList.Create<string>(Polling.NAME);

        //    var socket = new Socket(options);

        //    socket.On(Socket.EVENT_OPEN, () =>
        //    {

        //        log.Info("EVENT_OPEN");

        //        socket.Send(binaryData);
        //        socket.Send("cash money €€€");
        //    });

        //    socket.On(Socket.EVENT_MESSAGE, (d) =>
        //    {

        //        var data = d as string;
        //        log.Info(string.Format("EVENT_MESSAGE data ={0} d = {1} ", data, d));

        //        if (data == "hi")
        //        {
        //            return;
        //        }
        //        events.Enqueue(d);
        //        //socket.Close();
        //    });

        //    socket.Open();
        //    await Task.Delay(20000);
        //    socket.Close();
        //    log.Info("ReceiveBinaryData end");

        //    var binaryData2 = new byte[5];
        //    for (int i = 0; i < binaryData2.Length; i++)
        //    {
        //        binaryData2[i] = (byte)(i + 1);
        //    }
           
        //    Assert.AreEqual("1", "1");
        //}

        private ManualResetEvent _manualResetEvent = null;

        [TestMethod]
        public void ConnectToLocalhost()
        {
            LogManager.SetupLogManager();
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");
            _manualResetEvent = new ManualResetEvent(false);

            socket = new Socket(CreateOptions());
            socket.On(Socket.EVENT_OPEN, new TestListener());
            socket.On(Socket.EVENT_MESSAGE, new MessageListener(socket, this));
            socket.Open();
            _manualResetEvent.WaitOne();
            socket.Close();
            Assert.AreEqual("hi", this.Message);
        }


        public class TestListener : IListener
        {


            public void Call(params object[] args)
            {
                LogManager.SetupLogManager();
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
                LogManager.SetupLogManager();
                var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
                log.Info("message = " + args[0]);
                connectionTest.Message = (string) args[0];
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


        AutoResetEvent _autoResetEvent;

        [TestMethod]
        public void ConnectToLocalhost2()
        {
            LogManager.SetupLogManager();
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
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
            LogManager.SetupLogManager();
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
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
            LogManager.SetupLogManager();
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
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
        public async Task NotSendPacketsIfSocketCloses()
        {
            LogManager.SetupLogManager();
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
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
            await Task.Delay(2000);
            log.Info("NotSendPacketsIfSocketCloses end noPacket = " + noPacket);
            Assert.IsTrue(noPacket);
        }


    }
}
