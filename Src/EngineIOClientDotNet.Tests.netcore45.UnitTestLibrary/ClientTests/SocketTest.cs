

using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Quobject.Collections.Immutable;
using Quobject.EngineIoClientDotNet.Client;
using Quobject.EngineIoClientDotNet.Modules;
using System.Threading;


namespace Quobject.EngineIoClientDotNet_Tests.ClientTests
{
    [TestClass]    
    public class SocketTest : Connection
    {
        private Socket socket;
        public string Message;
        private ManualResetEvent _manualResetEvent = null;

        [TestMethod]
        public void FilterUpgrades()
        {

            var log = LogManager.GetLogger(Global.CallerName());
            log.Info("Start");

            var options = CreateOptions();
            options.Transports = ImmutableList<string>.Empty.Add("polling");

            socket = new Socket(options);

            var immutablelist = socket.FilterUpgrades(ImmutableList<string>.Empty.Add("polling").Add("websocket"));

            Assert.AreEqual("polling", immutablelist[0]);
            Assert.AreEqual(1, immutablelist.Count);
        }


        [TestMethod]
        public void SocketClosing()
        {

            var log = LogManager.GetLogger(Global.CallerName());
            log.Info("Start");
            _manualResetEvent = new ManualResetEvent(false);
            var manualResetEventError = new ManualResetEvent(false);
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
                _manualResetEvent.Set();
            });

            socket.Once(Socket.EVENT_ERROR, () =>
            {
                log.Info("EVENT_ERROR = ");
                error = true;
                manualResetEventError.Set();
            });

            socket.Open();
            _manualResetEvent.WaitOne();
            manualResetEventError.WaitOne();
            Assert.IsTrue(closed);
            Assert.IsTrue(error);
        }
    }
}
