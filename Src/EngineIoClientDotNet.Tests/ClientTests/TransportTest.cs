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
            var socket = new Socket(CreateOptions());

            socket.Open();
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(3));
            Assert.NotNull(socket.Transport);

            socket.Close();
        }

        [Fact]
        public void Uri()
        {
            var options = new Transport.Options
            {
                Path = "/engine.io",
                Hostname = Connection.CreateOptions().Hostname,
                Secure = false,
                Query = new Dictionary<string, string> { { "sid", "test" } },
                TimestampRequests = false
            };
            var polling = new Polling(options);
            var expected = string.Format("http://{0}/engine.io?sid=test&b64=1", options.Hostname);
            Assert.Contains(expected, polling.Uri());
        }

        [Fact]
        public void UriWithDefaultPort()
        {
            var options = new Transport.Options
            {
                Path = "/engine.io",
                Hostname = Connection.CreateOptions().Hostname,
                Secure = false,
                Query = new Dictionary<string, string> { { "sid", "test" } },
                TimestampRequests = false,
                Port = 80
            };
            var polling = new Polling(options);
            //Assert.Contains("http://localhost/engine.io?sid=test&b64=1", polling.Uri());
            var expected = string.Format("http://{0}/engine.io?sid=test&b64=1", options.Hostname);
            Assert.Contains(expected, polling.Uri());
        }

        [Fact]
        public void UriWithPort()
        {
            var options = new Transport.Options
            {
                Path = "/engine.io",
                Hostname = Connection.CreateOptions().Hostname,
                Secure = false,
                Query = new Dictionary<string, string> { { "sid", "test" } },
                TimestampRequests = false,
                Port = 3000
            };
            var polling = new Polling(options);
            //Assert.Contains("http://localhost:3000/engine.io?sid=test&b64=1", polling.Uri());
            var expected = string.Format("http://{0}:{1}/engine.io?sid=test&b64=1", options.Hostname, options.Port);
            Assert.Contains(expected, polling.Uri());
        }

        [Fact]
        public void HttpsUriWithDefaultPort()
        {
            var options = new Transport.Options
            {
                Path = "/engine.io",
                Hostname = Connection.CreateOptions().Hostname,
                Secure = true,
                Query = new Dictionary<string, string> { { "sid", "test" } },
                TimestampRequests = false,
                Port = 443
            };
            var polling = new Polling(options);
            //Assert.Contains("https://localhost/engine.io?sid=test&b64=1", polling.Uri());
            var expected = string.Format("https://{0}/engine.io?sid=test&b64=1", options.Hostname);
            Assert.Contains(expected, polling.Uri());
        }

        [Fact]
        public void TimestampedUri()
        {
            var options = new Transport.Options
            {
                Path = "/engine.io",
                Hostname = "test",
                Secure = false,
                Query = new Dictionary<string, string> { { "sid", "test" } },
                TimestampRequests = true,
                TimestampParam = "t"
            };
            var polling = new Polling(options);

            var pat = @"http://test/engine.io\?sid=test&(t=[0-9]+-[0-9]+)";
            var r = new Regex(pat, RegexOptions.IgnoreCase);
            var test = polling.Uri();
            //log.Info(test);
            var m = r.Match(test);
            Assert.True(m.Success);
        }

        [Fact]
        public void WsUri()
        {
            var options = new Transport.Options
            {
                Path = "/engine.io",
                Hostname = "test",
                Secure = false,
                Query = new Dictionary<string, string> { { "transport", "websocket" } },
                TimestampRequests = false
            };
            var ws = new WebSocket(options);
            Assert.Contains("ws://test/engine.io?transport=websocket", ws.Uri());
        }

        [Fact]
        public void WssUri()
        {
            var options = new Transport.Options
            {
                Path = "/engine.io",
                Hostname = "test",
                Secure = true,
                Query = new Dictionary<string, string> { { "transport", "websocket" } },
                TimestampRequests = false
            };
            var ws = new WebSocket(options);
            Assert.Contains("wss://test/engine.io?transport=websocket", ws.Uri());
        }

        [Fact]
        public void WsTimestampedUri()
        {
            var options = new Transport.Options
            {
                Path = "/engine.io",
                Hostname = "test",
                Secure = false,
                Query = new Dictionary<string, string> { { "sid", "test" } },
                TimestampRequests = true,
                TimestampParam = "woot"
            };
            var ws = new WebSocket(options);

            var pat = @"ws://test/engine.io\?sid=test&(woot=[0-9]+-[0-9]+)";
            var r = new Regex(pat, RegexOptions.IgnoreCase);
            var test = ws.Uri();
            //log.Info(test);
            var m = r.Match(test);
            Assert.True(m.Success);
        }
    }
}