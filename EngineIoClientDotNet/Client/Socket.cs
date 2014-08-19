using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quobject.EngineIoClientDotNet.Client.Transports;
using Quobject.EngineIoClientDotNet.ComponentEmitter;
using Quobject.EngineIoClientDotNet.Modules;
using Quobject.EngineIoClientDotNet.Parser;
using Quobject.EngineIoClientDotNet.Thread;

namespace Quobject.EngineIoClientDotNet.Client
{
    public class Socket : Emitter
    {
        private enum ReadyStateEnum
        {
            OPENING,
            OPEN,
            CLOSED,
            PAUSED
        }

        public static readonly string EVENT_OPEN = "open";
        public static readonly string EVENT_CLOSE = "close";
        public static readonly string EVENT_PACKET = "packet";
        public static readonly string EVENT_DRAIN = "drain";
        public static readonly string EVENT_ERROR = "error";
        public static readonly string EVENT_DATA = "data";
        public static readonly string EVENT_MESSAGE = "message";
        public static readonly string EVENT_UPGRADE_ERROR = "upgradeError";
        public static readonly string EVENT_FLUSH = "flush";
        public static readonly string EVENT_HANDSHAKE = "handshake";
        public static readonly string EVENT_UPGRADING = "upgrading";
        public static readonly string EVENT_UPGRADE = "upgrade";
        public static readonly string EVENT_PACKET_CREATE = "packetCreate";
        public static readonly string EVENT_HEARTBEAT = "heartbeat";
        public static readonly string EVENT_TRANSPORT = "transport";

        public static readonly int Protocol = Parser.Parser.Protocol;

        public static bool PriorWebsocketSuccess = false;

        private static SSLContext DefaultSSLContext;

        private bool Secure;
        private bool Upgrade;
        private bool TimestampRequests;
        private bool Upgrading;
        private bool RememberUpgrade;
        private int Port;
        private int PolicyPort;
        private int PrevBufferLen;
        private long PingInterval;
        private long PingTimeout;
        private string Id;
        private string Hostname;
        private string Path;
        private string TimestampParam;
        private ImmutableList<string> Transports;
        private ImmutableList<string> Upgrades;
        private ImmutableDictionary<string, string> Query;
        private ConcurrentQueue<Packet> WriteBuffer = new ConcurrentQueue<Packet>();
        private ConcurrentQueue<Action> CallbackBuffer = new ConcurrentQueue<Action>();
        /*package*/
        private Transport Transport;
        private Task PingTimeoutTimer;
        private Task PingIntervalTimer;
        private SSLContext SslContext;

        private ReadyStateEnum ReadyState;
        //private ScheduledExecutorService heartbeatScheduler = Executors.newSingleThreadScheduledExecutor();
        private HeartBeatTasks HeartBeatTasks = new HeartBeatTasks();
        private Uri uri;
        private Options options;

        public Socket() : this(new Options())
        {
        }

        public Socket(string uri, Options options) : this(uri == null ? null : new Uri(uri), options)
        {            
        }

        public Socket(Uri uri, Options options) : this(uri == null ? options : Options.FromURI(uri, options))
        {
            this.uri = uri;
            this.options = options;
        }


        public Socket(Options options)
        {
            if (options.Host != null)
            {
                var pieces = options.Host.Split(':');
                options.Hostname = pieces[0];
                if (pieces.Length > 1)
                {
                    options.Port = int.Parse(pieces[pieces.Length - 1]);
                }
            }

            Secure = options.Secure;
            SslContext = options.SSLContext;
            Hostname = options.Hostname;
            Port = options.Port;
            Query = options.QueryString != null ? ParseQS.Decode(options.QueryString) : ImmutableDictionary<string, string>.Empty;
            Upgrade = options.Upgrade;
            Path = (options.Path ?? "/engine.io").Replace("/$", "") + "/";
            TimestampParam = (options.TimestampParam ?? "t");
            TimestampRequests = options.TimestampRequests;
            Transports = options.Transports ?? ImmutableList<string>.Empty.Add(Polling.NAME).Add(WebSocket.NAME);
            PolicyPort = options.PolicyPort != 0 ? options.PolicyPort : 843;
            RememberUpgrade = options.RememberUpgrade;
        }

        public Socket Open()
        {
            EventTasks.Exec(n =>
            {
                string transportName;
                if (RememberUpgrade && PriorWebsocketSuccess && Transports.Contains(WebSocket.NAME))
                {
                    transportName = WebSocket.NAME;
                }
                else
                {
                    transportName = Transports[0];
                }
                ReadyState = ReadyStateEnum.OPENING;
                var transport = CreateTransport(transportName);
                SetTransport(transport);
                transport.Open();
            });
            return this;
        }

        private Transport CreateTransport(string name)
        {
            Debug.WriteLine(string.Format("creating transport '{0}'",name), "Socket fine");
            var query = Query.Add("EIO", Parser.Parser.Protocol.ToString());
            query = query.Add("transport", name);
            if (Id != null)
            {
                query = query.Add("sid", Id);
            }
            var options = new Transport.Options();
            options.SSLContext = SslContext;
            options.Hostname = Hostname;
            options.Port = Port;
            options.Secure = Secure;
            options.Path = Path;
            options.Query = query;
            options.TimestampRequests = TimestampRequests;
            options.TimestampParam = TimestampParam;
            options.PolicyPort = PolicyPort;
            options.Socket = this;

            if (name == WebSocket.NAME)
            {
                return new WebSocket(options);
            }
            else if (name == Polling.NAME)
            {
                return new PollingXHR(options);
            }

            throw new EngineIOException("CreateTransport failed");
        }

        private void SetTransport(Transport transport)
        {
           // continue here
           











        }


        public class Options : Transport.Options
        {

            public ImmutableList<string> Transports;

            public bool Upgrade = true;

            public bool RememberUpgrade;
            public string Host;
            public string QueryString;

            public static Options FromURI(Uri uri, Options opts)
            {
                if (opts == null)
                {
                    opts = new Options();
                }

                opts.Host = uri.Host;
                opts.Secure = uri.Scheme == "https" || uri.Scheme == "wss";

                if (!string.IsNullOrEmpty(uri.Query))
                {
                    opts.QueryString = uri.Query;
                }

                return opts;
            }
        }

    }
}
