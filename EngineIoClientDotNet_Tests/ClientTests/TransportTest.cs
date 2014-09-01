using log4net;
using Quobject.EngineIoClientDotNet.Client;
using Quobject.EngineIoClientDotNet.Client.Transports;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Xunit;

namespace Quobject.EngineIoClientDotNet_Tests.ClientTests
{
    // NOTE: tests for the rememberUpgrade option are on ServerConnectionTest.

    public class TransportTest : Connection
    {
        [Fact]
        public void Constructors()
        {
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            var socket = new Socket(CreateOptions());

            socket.Open();
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(3));
            Assert.NotNull(socket.Transport);

            socket.Close();             
        }

        [Fact]
        public void Uri()
        {
            var options = new Transport.Options();
            options.Path = "/engine.io";
            options.Hostname = "localhost";
            options.Secure = false;
            options.Query = new Dictionary<string, string> {{"sid", "test"}};
            options.TimestampRequests = false;
            var polling = new Polling(options);
            Assert.Contains("http://localhost/engine.io?sid=test&b64=1", polling.Uri());
        }

        [Fact]
        public void UriWithDefaultPort()
        {
            var options = new Transport.Options();
            options.Path = "/engine.io";
            options.Hostname = "localhost";
            options.Secure = false;
            options.Query = new Dictionary<string, string> { { "sid", "test" } };
            options.TimestampRequests = false;
            options.Port = 80;
            var polling = new Polling(options);
            Assert.Contains("http://localhost/engine.io?sid=test&b64=1", polling.Uri());
        }

        [Fact]
        public void UriWithPort()
        {
            var options = new Transport.Options();
            options.Path = "/engine.io";
            options.Hostname = "localhost";
            options.Secure = false;
            options.Query = new Dictionary<string, string> { { "sid", "test" } };
            options.TimestampRequests = false;
            options.Port = 3000;
            var polling = new Polling(options);
            Assert.Contains("http://localhost:3000/engine.io?sid=test&b64=1", polling.Uri());
        }


        [Fact]
        public void HttpsUriWithDefaultPort()
        {
            var options = new Transport.Options();
            options.Path = "/engine.io";
            options.Hostname = "localhost";
            options.Secure = true;
            options.Query = new Dictionary<string, string> { { "sid", "test" } };
            options.TimestampRequests = false;
            options.Port = 443;
            var polling = new Polling(options);
            Assert.Contains("https://localhost/engine.io?sid=test&b64=1", polling.Uri());
        }


        [Fact]
        public void TimestampedUri()
        {
            Socket.SetupLog4Net();
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            var options = new Transport.Options();
            options.Path = "/engine.io";
            options.Hostname = "localhost";
            options.Secure = false;
            options.Query = new Dictionary<string, string> { { "sid", "test" } };
            options.TimestampRequests = true;
            options.TimestampParam = "t";
            var polling = new Polling(options);

            string pat = @"http://localhost/engine.io\?sid=test&(t=[0-9]+-[0-9]+)";
            var r = new Regex(pat, RegexOptions.IgnoreCase);
            var test = polling.Uri();
            log.Info(test);
            Match m = r.Match(test);
            Assert.True(m.Success);
        }


        [Fact]
        public void WsUri()
        {
            var options = new Transport.Options();
            options.Path = "/engine.io";
            options.Hostname = "test";
            options.Secure = false;
            options.Query = new Dictionary<string, string> { { "transport", "websocket" } };
            options.TimestampRequests = false;
            var ws = new WebSocket(options);
            Assert.Contains("ws://test/engine.io?transport=websocket", ws.Uri());
        }

        [Fact]
        public void WssUri()
        {
            var options = new Transport.Options();
            options.Path = "/engine.io";
            options.Hostname = "test";
            options.Secure = true;
            options.Query = new Dictionary<string, string> { { "transport", "websocket" } };
            options.TimestampRequests = false;
            var ws = new WebSocket(options);
            Assert.Contains("wss://test/engine.io?transport=websocket", ws.Uri());
        }


        [Fact]
        public void WsTimestampedUri()
        {
            Socket.SetupLog4Net();
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

            var options = new Transport.Options();
            options.Path = "/engine.io";
            options.Hostname = "localhost";
            options.Secure = false;
            options.Query = new Dictionary<string, string> { { "sid", "test" } };
            options.TimestampRequests = true;
            options.TimestampParam = "woot";
            var ws = new WebSocket(options);

            string pat = @"ws://localhost/engine.io\?sid=test&(woot=[0-9]+-[0-9]+)";
            var r = new Regex(pat, RegexOptions.IgnoreCase);
            var test = ws.Uri();
            log.Info(test);
            Match m = r.Match(test);
            Assert.True(m.Success);
        }













    }
}
