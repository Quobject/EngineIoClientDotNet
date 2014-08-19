using System;
using System.Collections.Generic;
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
        private List<string> Transports;
        private List<string> Upgrades;
        private Dictionary<string, string> Query;
        private LinkedList<Packet> WriteBuffer = new LinkedList<Packet>();
        private LinkedList<Action> CallbackBuffer = new LinkedList<Action>();
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
                var pieces = options.Host.Split(":");
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
            Query = options.Query != null ? ParseQS.Decode(options.Query) : new Dictionary<string, string>();
            Upgrade = options.Upgrade;
            Path = (options.Path ?? "/engine.io").Replace("/$", "") + "/";
            TimestampParam = (options.TimestampParam ?? "t");
            TimestampRequests = options.TimestampRequests;
            Transports = options.Transports ?? new List<string>() {Polling.NAME, WebSocket.NAME};
            PolicyPort = options.PolicyPort != 0 ? options.PolicyPort : 843;
            RememberUpgrade = options.RememberUpgrade;
        }





        public class Options : Transport.Options
        {

            public List<string> Transports;

            public bool Upgrade = true;

            public bool RememberUpgrade;
            public string Host;
            public string Query;

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
                    opts.Query = uri.Query;
                }

                return opts;
            }
        }

    }
}
