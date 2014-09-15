//using log4net;

using System.Threading;
using System.Threading.Tasks;
using EngineIoClientDotNet.Modules;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Quobject.Collections.Immutable;
using Quobject.EngineIoClientDotNet.Client;
using System;



namespace Quobject.EngineIoClientDotNet_Tests.ClientTests
{
    [TestClass]    
    public class SocketTest : Connection
    {
        private Socket socket;
        public string Message;

        [TestMethod]
        public void FilterUpgrades()
        {
            LogManager.SetupLogManager();
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");

            var options = CreateOptions();
            options.Transports = ImmutableList<string>.Empty.Add("polling");

            socket = new Socket(options);

            var immutablelist = socket.FilterUpgrades(ImmutableList<string>.Empty.Add("polling").Add("websocket"));

            Assert.AreEqual("polling", immutablelist[0]);
            Assert.AreEqual(1, immutablelist.Count);
        }


        [TestMethod]
        public async Task SocketClosing()
        {
            LogManager.SetupLogManager();
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");          

            var closed = false;
            var error = false;

            var options = CreateOptions();

            socket = new Socket("ws://0.0.0.0:8080", options);
            socket.On(Socket.EVENT_OPEN, () =>
            {
                log.Info("EVENT_OPEN");
                //socket.Send("test send");
                
            });
            socket.On(Socket.EVENT_CLOSE, () =>
            {
                log.Info("EVENT_CLOSE = ");
                closed = true;                
            });

            socket.Once(Socket.EVENT_ERROR, () =>
            {
                log.Info("EVENT_ERROR = ");
                error = true;                
            });

            socket.Open();
            await Task.Delay(1000);
            log.Info("After WaitOne");
            Assert.IsTrue(closed);
            Assert.IsTrue(error);
        }
    }
}
