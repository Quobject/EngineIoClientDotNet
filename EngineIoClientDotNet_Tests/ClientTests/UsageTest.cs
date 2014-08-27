using log4net;
using Quobject.EngineIoClientDotNet.Client;
using System;
using System.Collections.Immutable;
using Xunit;

namespace Quobject.EngineIoClientDotNet_Tests.ClientTests
{
    public class UsageTest : Connection
    {


        [Fact]
        public void Usage1()
        {

            var socket = new Socket(new Socket.Options { Port = 3000, Hostname = "localhost" });
            socket.On(Socket.EVENT_OPEN, () =>
            {
                socket.Send("hi");
                //socket.Send("hi", () =>
                //{
                //    System.Threading.Thread.Sleep(TimeSpan.FromSeconds(4));
                //    socket.Close();
                //});
            });
            socket.Open();

            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(4));
            socket.Close();



        }

   
    }
}
