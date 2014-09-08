//using log4net;

using System.Diagnostics;
using EngineIoClientDotNet.Modules;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Quobject.EngineIoClientDotNet.Client;
using System;
using Quobject.EngineIoClientDotNet.Modules;


namespace Quobject.EngineIoClientDotNet_Tests.ClientTests
{
    [TestClass]
    public class UsageTest : Connection
    {


        [TestMethod]
        public void Usage1()
        {

            var log = LogManager.GetLogger(Global.CallerName());
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

        [TestMethod]
        public void Usage2()
        {

            var log = LogManager.GetLogger(Global.CallerName());
            log.Info("Start");


            var options = CreateOptions();
            var socket = new Socket(options);

            //Receiving data
            //var socket = new Socket("ws://localhost:3000");
            socket.On(Socket.EVENT_OPEN, () =>
            {
                socket.On(Socket.EVENT_MESSAGE, (data) => Debug.WriteLine((string)data));
            });
            socket.Open();


            //System.Threading.Thread.Sleep(TimeSpan.FromSeconds(2));
            socket.Close();

            
        }


    }
}
