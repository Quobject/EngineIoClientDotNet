using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Quobject.EngineIoClientDotNet.Client;
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
                Assert.Equal("hi1", connectionTest.Message);
                //throw new Exception("test");
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
            socket.On(Socket.EVENT_MESSAGE, (data) =>
            {
                log.Info("message2 = " + data);
                this.Message = data;            
                
            });
            socket.Open();

            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(10));
            socket.Close();
            Assert.Equal("hi", this.Message);
        }


    }
}
