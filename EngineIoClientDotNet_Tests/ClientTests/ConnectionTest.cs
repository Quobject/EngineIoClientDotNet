using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            socket = new Socket(CreateOptions());
            socket.On(Socket.EVENT_OPEN, new TestListener(socket));
            socket.On(Socket.EVENT_MESSAGE, new MessageListener(socket, this));
            socket.Open();

            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(2));
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
                Console.WriteLine("open");
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
                Console.WriteLine("message = " + args[0]);
                connectionTest.Message = (string) args[0];
                socket.Close();
            }
        }


    }
}
