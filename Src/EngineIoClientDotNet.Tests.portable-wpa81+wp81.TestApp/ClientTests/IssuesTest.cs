using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Quobject.EngineIoClientDotNet.Client;
using Quobject.EngineIoClientDotNet.Modules;
using Quobject.EngineIoClientDotNet_Tests.ClientTests;

namespace EngineIoClientDotNet.Tests.portable_wpa81_wp81.TestApp.ClientTests
{
    [TestClass]
    public class IssuesTest : Connection
    {

        /// <summary>
        /// https://github.com/Quobject/EngineIoClientDotNet/issues/2
        /// After connecting to the websocket with Engine.IO, I recieve messages from the webserver, but as 
        /// soon as the second ping is sent, the socket closes.
        /// Edit: This is a Windows Phone 8.1 project (Universal app, C#)
        /// </summary>
        [TestMethod]
        public void WebsocketClosesAfterWritingPingPacket()
        {

            var log = LogManager.GetLogger(Global.CallerName());
            log.Info("Start");
            bool open = false;
            bool close = false;


            var options = CreateOptions();
            //options.ForceBase64 = true;
            //options.Secure = false;
            //options.RememberUpgrade = false;
            //options.TimestampRequests = true;
            var socket = new Socket(options);

            //You can use `Socket` to connect:
            //var socket = new Socket("ws://localhost");          

            socket.On(Socket.EVENT_OPEN, () =>
            {
                log.Info("EVENT_OPEN");
                open = true;
            });

            socket.On(Socket.EVENT_CLOSE, () =>
            {
                log.Info("EVENT_CLOSE");
                close = true;
            });
            socket.Open();

            


            Task.Delay(60000).Wait();
            Assert.IsTrue(open);
            Assert.IsFalse(close);

            socket.Close();
        }
    }
}
