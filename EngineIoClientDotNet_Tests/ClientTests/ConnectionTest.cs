using log4net;
using Quobject.EngineIoClientDotNet.Client;
using Quobject.EngineIoClientDotNet.ComponentEmitter;
using System;
using System.Collections.Generic;
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
            Socket.SetupLog4Net();
            socket = new Socket(CreateOptions());
            socket.On(Socket.EVENT_OPEN, new TestListener(socket));
            socket.On(Socket.EVENT_MESSAGE, new MessageListener(socket, this));
            socket.Open();

            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(4));
            Assert.Equal("hi", this.Message);
        }


        public class TestListener : IListener
        {
            private Socket socket;

            public TestListener(Socket socket)
            {                
                this.socket = socket;
            }


            public void Call(params object[] args)
            {
                var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
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
                var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
                log.Info("message = " + args[0]);
                connectionTest.Message = (string) args[0];
                socket.Close();
            }
        }


        [Fact]
        public void ConnectToLocalhost2()
        {

            Socket.SetupLog4Net();

            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            this.Message = "";

            socket = new Socket(CreateOptions());
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
                
            });
            socket.Open();

            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(3));
            socket.Close();
            Assert.Equal("hi", this.Message);
        }   

        [Fact]
        public void TestSend()
        {
            var testList = new List<Data>()
            {
                 new Data { Info = "multibyte utf-8 strings with polling", Test = "cash money €€€" },
                 new Data { Info = "emoji", Test = "\uD800-\uDB7F\uDB80-\uDBFF\uDC00-\uDFFF\uE000-\uF8FF" }
            };

            foreach (var test in testList)
            {
                TestMessage(test);
                
            }
        }

        private void TestMessage(Data test)
        {
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            this.Message = "";

            Socket.SetupLog4Net();
            socket = new Socket(CreateOptions());
            socket.On(Socket.EVENT_OPEN, () =>
            {
                log.Info("open");

                socket.On(Socket.EVENT_MESSAGE, (d) =>
                {
                    var data = (string)d;

                    log.Info("TestMessage data = " + data);

                    if (data == "hi")
                    {
                        return;
                    }

                    this.Message = data;
                });
                socket.Send(test.Test);
            });

            socket.Open();

            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(3));
            socket.Close();
            log.Info("Test Info = " + test.Info);
            log.Info("TestMessage this.Message = " + this.Message);
            Assert.True(test.Test == this.Message);
        }

        private class Data
        {
            public string Info;
            public string Test;
        }

        [Fact]
        public void NotSendPacketsIfSocketCloses()
        {

            Socket.SetupLog4Net();

            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            var noPacket = true;

            socket = new Socket(CreateOptions());
            socket.On(Socket.EVENT_OPEN, () =>
            {
                noPacket = true;
//                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(6));

            });

            socket.Open();
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(3));
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
