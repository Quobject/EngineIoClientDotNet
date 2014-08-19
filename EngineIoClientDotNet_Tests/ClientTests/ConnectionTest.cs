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

        [Fact]
        public void ConnectToLocalhost()
        {
            socket = new Socket(CreateOptions());
            socket.On(Socket.EVENT_OPEN, new TestListener(socket));
            socket.Open();

            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(10));
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
                Assert.Equal("hi1",(string)args[0]);
                socket.Close();
            }
        }


    }
}
