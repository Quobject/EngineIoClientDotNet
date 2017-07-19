using Quobject.EngineIoClientDotNet.Client;
using Quobject.EngineIoClientDotNet.Modules;
using System;
using Xunit;

namespace Quobject.EngineIoClientDotNet_Tests.ClientTests
{
    public class UsageTest : Connection
    {
        [Fact]
        public void Usage1()
        {
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");

            var options = CreateOptions();
            var socket = new Socket(options);

            //You can use `Socket` to connect:
            //var socket = new Socket("ws://localhost");
            socket.On(Socket.EVENT_OPEN, () =>
            {
                socket.Send("hi");
                socket.Close();
            });
            socket.Open();

            //System.Threading.Thread.Sleep(TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void Usage2()
        {
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");

            var options = CreateOptions();
            var socket = new Socket(options);

            //Receiving data
            //var socket = new Socket("ws://localhost:3000");
            socket.On(Socket.EVENT_OPEN, () =>
            {
                socket.On(Socket.EVENT_MESSAGE, (data) => Console.WriteLine((string)data));
            });
            socket.Open();

            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(2));
            socket.Close();
        }
    }
}