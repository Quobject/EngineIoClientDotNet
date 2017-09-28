using System.Text;
using System.Collections.Immutable;
using Quobject.EngineIoClientDotNet.Client.Transports;
using Quobject.EngineIoClientDotNet.ComponentEmitter;
using Quobject.EngineIoClientDotNet.Modules;
using Quobject.EngineIoClientDotNet.Parser;
using Quobject.EngineIoClientDotNet.Thread;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;


namespace Quobject.EngineIoClientDotNet.Client
{
    public class Socket : Emitter
    {
        private enum ReadyStateEnum
        {
            OPENING,
            OPEN,
            CLOSING,
            CLOSED
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


        private bool Secure;
        private bool Upgrade;
        private bool TimestampRequests = true;
        private bool Upgrading;
        private bool RememberUpgrade;
        private int Port;
        private int PolicyPort;
        private int PrevBufferLen;
        private long PingInterval;
        private long PingTimeout;
        public string Id;
        private string Hostname;
        private string Path;
        private string TimestampParam;
        private ImmutableList<string> Transports;
        private ImmutableList<string> Upgrades;
        private Dictionary<string, string> Query;
        private ImmutableList<Packet> WriteBuffer = ImmutableList<Packet>.Empty;
        private ImmutableList<Action> CallbackBuffer = ImmutableList<Action>.Empty;
        private Dictionary<string, string> Cookies = new Dictionary<string, string>();
        /*package*/
        public Transport Transport;
        private EasyTimer PingTimeoutTimer;
        private EasyTimer PingIntervalTimer;

        private ReadyStateEnum ReadyState;
        private bool Agent = false;
        private bool ForceBase64 = false;
        private bool ForceJsonp = false;

        public Dictionary<string, string> ExtraHeaders;


        //public static void SetupLog4Net()
        //{
        //    var hierarchy = (Hierarchy)LogManager.GetRepository();
        //    hierarchy.Root.RemoveAllAppenders(); /*Remove any other appenders*/

        //    var fileAppender = new FileAppender();
        //    fileAppender.AppendToFile = true;
        //    fileAppender.LockingModel = new FileAppender.MinimalLock();
        //    fileAppender.File = "EngineIoClientDotNet.log";
        //    var pl = new PatternLayout();
        //    pl.ConversionPattern = "%d [%2%t] %-5p [%-10c]   %m%n";
        //    pl.ActivateOptions();
        //    fileAppender.Layout = pl;
        //    fileAppender.ActivateOptions();
        //    BasicConfigurator.Configure(fileAppender);
        //}

        public Socket()
            : this(new Options())
        {
        }

        public Socket(string uri)
            : this(uri, null)
        {
        }

        public Socket(string uri, Options options)
            : this(uri == null ? null : String2Uri(uri), options)
        {
            
        }

        private static Uri String2Uri(string uri)
        {
            if (uri.StartsWith("http") || uri.StartsWith("ws"))
            {
                return new Uri(uri);
            }
            else
            {
                return new Uri("http://" + uri);
            }
        }

        public Socket(Uri uri, Options options)
            : this(uri == null ? options : Options.FromURI(uri, options))
        {
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
            Hostname = options.Hostname;
            Port = options.Port;
            Query = options.QueryString != null ? ParseQS.Decode(options.QueryString) : new Dictionary<string, string>();

            if (options.Query != null)
            {
                foreach (var item in options.Query)
                {
                    Query.Add(item.Key,item.Value);
                }
            }


            Upgrade = options.Upgrade;
            Path = (options.Path ?? "/engine.io").Replace("/$", "") + "/";
            TimestampParam = (options.TimestampParam ?? "t");
            TimestampRequests = options.TimestampRequests;
            Transports = options.Transports ?? ImmutableList<string>.Empty.Add(Polling.NAME).Add(WebSocket.NAME);
            PolicyPort = options.PolicyPort != 0 ? options.PolicyPort : 843;
            RememberUpgrade = options.RememberUpgrade;
            Cookies = options.Cookies;
            if (options.IgnoreServerCertificateValidation)
            {
                ServerCertificate.IgnoreServerCertificateValidation();
            }
            ExtraHeaders = options.ExtraHeaders;

        }

        public Socket Open()
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
//            EventTasks.Exec((n) =>
            Task.Run(() =>
            {
                var log2 = LogManager.GetLogger(Global.CallerName());
                log2.Info("Task.Run Open start");
                transport.Open();
                log2.Info("Task.Run Open finish");
            });
            return this;
        }

        private Transport CreateTransport(string name)
        {
            var query = new Dictionary<string, string>(Query);
            query.Add("EIO", Parser.Parser.Protocol.ToString());
            query.Add("transport", name);
            if (Id != null)
            {
                query.Add("sid", Id);
            }
            var options = new Transport.Options
            {
                Hostname = Hostname,
                Port = Port,
                Secure = Secure,
                Path = Path,
                Query = query,
                TimestampRequests = TimestampRequests,
                TimestampParam = TimestampParam,
                PolicyPort = PolicyPort,
                Socket = this,
                Agent = this.Agent,
                ForceBase64 = this.ForceBase64,
                ForceJsonp = this.ForceJsonp,
                Cookies = this.Cookies,
                ExtraHeaders = this.ExtraHeaders
            };

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
            var log = LogManager.GetLogger(Global.CallerName());
            log.Info(string.Format("SetTransport setting transport '{0}'", transport.Name));

            if (this.Transport != null)
            {
                log.Info(string.Format("SetTransport clearing existing transport '{0}'", transport.Name));
                this.Transport.Off();
            }

            Transport = transport;

            Emit(EVENT_TRANSPORT, transport);

            transport.On(EVENT_DRAIN, new EventDrainListener(this));
            transport.On(EVENT_PACKET, new EventPacketListener(this));
            transport.On(EVENT_ERROR, new EventErrorListener(this));
            transport.On(EVENT_CLOSE, new EventCloseListener(this));
        }

        private class EventDrainListener : IListener
        {
            private Socket socket;

            public EventDrainListener(Socket socket)
            {
                this.socket = socket;
            }

            void IListener.Call(params object[] args)
            {
                socket.OnDrain();
            }

            public int CompareTo(IListener other)
            {
                return this.GetId().CompareTo(other.GetId());
            }

            public int GetId()
            {
                return 0;
            }
        }

        private class EventPacketListener : IListener
        {
            private Socket socket;

            public EventPacketListener(Socket socket)
            {
                this.socket = socket;
            }

            void IListener.Call(params object[] args)
            {
                socket.OnPacket(args.Length > 0 ? (Packet)args[0] : null);
            }

            public int CompareTo(IListener other)
            {
                return this.GetId().CompareTo(other.GetId());
            }

            public int GetId()
            {
                return 0;
            }
        }

        private class EventErrorListener : IListener
        {
            private Socket socket;

            public EventErrorListener(Socket socket)
            {
                this.socket = socket;
            }

            public void Call(params object[] args)
            {
                socket.OnError(args.Length > 0 ? (Exception)args[0] : null);
            }

            public int CompareTo(IListener other)
            {
                return this.GetId().CompareTo(other.GetId());
            }

            public int GetId()
            {
                return 0;
            }
        }

        private class EventCloseListener : IListener
        {
            private Socket socket;

            public EventCloseListener(Socket socket)
            {
                this.socket = socket;
            }

            public void Call(params object[] args)
            {
                socket.OnClose("transport close");
            }

            public int CompareTo(IListener other)
            {
                return this.GetId().CompareTo(other.GetId());
            }

            public int GetId()
            {
                return 0;
            }
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
                opts.Port = uri.Port;

                if (!string.IsNullOrEmpty(uri.Query))
                {
                    opts.QueryString = uri.Query;
                }

                return opts;
            }

    
        }


        internal void OnDrain()
        {
            //var log = LogManager.GetLogger(Global.CallerName());
            //log.Info(string.Format("OnDrain1 PrevBufferLen={0} WriteBuffer.Count={1}", PrevBufferLen, WriteBuffer.Count));

            for (int i = 0; i < this.PrevBufferLen; i++)
            {
                try
                {
                    var callback = this.CallbackBuffer[i];
                    if (callback != null)
                    {
                        callback();
                    }
                }
                catch (ArgumentOutOfRangeException)
                {
                    WriteBuffer = WriteBuffer.Clear();
                    CallbackBuffer = CallbackBuffer.Clear();
                    PrevBufferLen = 0;
                }
            }
            //log.Info(string.Format("OnDrain2 PrevBufferLen={0} WriteBuffer.Count={1}", PrevBufferLen, WriteBuffer.Count));


            try
            {
                WriteBuffer = WriteBuffer.RemoveRange(0, PrevBufferLen);
                CallbackBuffer = CallbackBuffer.RemoveRange(0, PrevBufferLen);
            }
            catch (Exception)
            {
                WriteBuffer = WriteBuffer.Clear();
                CallbackBuffer = CallbackBuffer.Clear();
            }
           

            this.PrevBufferLen = 0;
            //log.Info(string.Format("OnDrain3 PrevBufferLen={0} WriteBuffer.Count={1}", PrevBufferLen, WriteBuffer.Count));

            if (this.WriteBuffer.Count == 0)
            {
                this.Emit(EVENT_DRAIN);
            }
            else
            {
                this.Flush();
            }
        }

        private bool Flush()
        {
            var log = LogManager.GetLogger(Global.CallerName());

            log.Info(string.Format("ReadyState={0} Transport.Writeable={1} Upgrading={2} WriteBuffer.Count={3}",ReadyState,Transport.Writable,Upgrading, WriteBuffer.Count));
            if (ReadyState != ReadyStateEnum.CLOSED && this.Transport.Writable && !Upgrading && WriteBuffer.Count != 0)
            {
                log.Info(string.Format("Flush {0} packets in socket", WriteBuffer.Count));
                PrevBufferLen = WriteBuffer.Count;
                Transport.Send(WriteBuffer);
                Emit(EVENT_FLUSH);
                return true;
            }
            else
            {
                log.Info(string.Format("Flush Not Send"));
                return false;
            }
        }

        public void OnPacket(Packet packet)
        {
            var log = LogManager.GetLogger(Global.CallerName());


            if (ReadyState == ReadyStateEnum.OPENING || ReadyState == ReadyStateEnum.OPEN)
            {
                log.Info(string.Format("socket received: type '{0}', data '{1}'", packet.Type, packet.Data));

                Emit(EVENT_PACKET, packet);
                Emit(EVENT_HEARTBEAT);

                if (packet.Type == Packet.OPEN)
                {
                    OnHandshake(new HandshakeData((string)packet.Data));

                }
                else if (packet.Type == Packet.PONG)
                {
                    this.SetPing();
                }
                else if (packet.Type == Packet.ERROR)
                {
                    var err = new EngineIOException("server error")
                    {
                        code = packet.Data
                    };
                    this.Emit(EVENT_ERROR, err);
                }
                else if (packet.Type == Packet.MESSAGE)
                {
                    Emit(EVENT_DATA, packet.Data);
                    Emit(EVENT_MESSAGE, packet.Data);
                }
            }
            else
            {
                log.Info(string.Format("OnPacket packet received with socket readyState '{0}'", ReadyState));
            }

        }

        private void OnHandshake(HandshakeData handshakeData)
        {
            var log = LogManager.GetLogger(Global.CallerName());
            log.Info(nameof(OnHandshake));
            Emit(EVENT_HANDSHAKE, handshakeData);
            Id = handshakeData.Sid;
            Transport.Query.Add("sid", handshakeData.Sid);
            Upgrades = FilterUpgrades(handshakeData.Upgrades);
            PingInterval = handshakeData.PingInterval;
            PingTimeout = handshakeData.PingTimeout;
            OnOpen();
            // In case open handler closes socket
            if (ReadyStateEnum.CLOSED == this.ReadyState)
            {
                return;
            }
            this.SetPing();

            this.Off(EVENT_HEARTBEAT, new OnHeartbeatAsListener(this));
            this.On(EVENT_HEARTBEAT, new OnHeartbeatAsListener(this));

        }

        private class OnHeartbeatAsListener : IListener
        {
            private Socket socket;

            public OnHeartbeatAsListener(Socket socket)
            {
                this.socket = socket;
            }

            void IListener.Call(params object[] args)
            {
                socket.OnHeartbeat(args.Length > 0 ? (long)args[0] : 0);
            }

            public int CompareTo(IListener other)
            {
                return this.GetId().CompareTo(other.GetId());
            }

            public int GetId()
            {
                return 0;
            }
        }



        private void SetPing()
        {
            //var log = LogManager.GetLogger(Global.CallerName());

            if (this.PingIntervalTimer != null)
            {
                PingIntervalTimer.Stop();
            }
            var log = LogManager.GetLogger(Global.CallerName());
            log.Info(string.Format("writing ping packet - expecting pong within {0}ms", PingTimeout));

            PingIntervalTimer = EasyTimer.SetTimeout(() =>
            {
                var log2 = LogManager.GetLogger(Global.CallerName());
                log2.Info("EasyTimer SetPing start");

                if (Upgrading)
                {
                    // skip this ping during upgrade
                    SetPing();
                    log2.Info("skipping Ping during upgrade");
                }
                else
                {
                    Ping();
                    OnHeartbeat(PingTimeout);
                    log2.Info("EasyTimer SetPing finish");
                }
            }, (int)PingInterval);
        }

        private void Ping()
        {
            //Send("primus::ping::" + GetJavaTime());
            SendPacket(Packet.PING);
        }

        //private static string GetJavaTime()
        //{
        //    var st = new DateTime(1970, 1, 1);
        //    var t = (DateTime.Now.ToUniversalTime() - st);
        //    var returnstring = t.TotalMilliseconds.ToString();
        //    returnstring = returnstring.Replace(".", "-");
        //    return returnstring;
        //}

        public void Write(string msg, Action fn = null)
        {
            Send(msg, fn);
        }

        public void Write(byte[] msg, Action fn = null)
        {
            Send(msg, fn);
        }

        public void Send(string msg, Action fn = null)
        {
            SendPacket(Packet.MESSAGE, msg, fn);
        }

        public void Send(byte[] msg, Action fn = null)
        {
            SendPacket(Packet.MESSAGE, msg, fn);
        }



        private void SendPacket(string type)
        {
            SendPacket(new Packet(type), null);
        }

        private void SendPacket(string type, string data, Action fn)
        {
            SendPacket(new Packet(type, data), fn);
        }

        private void SendPacket(string type, byte[] data, Action fn)
        {
            SendPacket(new Packet(type, data), fn);
        }

        private void SendPacket(Packet packet, Action fn)
        {
            if (fn == null)
            {
                fn = () => { };
            }

            if (Upgrading)
            {
                WaitForUpgrade().Wait();
            }

            Emit(EVENT_PACKET_CREATE, packet);
            //var log = LogManager.GetLogger(Global.CallerName());
            //log.Info(string.Format("SendPacket WriteBuffer.Add(packet) packet ={0}",packet.Type));
            WriteBuffer = WriteBuffer.Add(packet);
            CallbackBuffer = CallbackBuffer.Add(fn);
            Flush();
        }

        private Task WaitForUpgrade()
        {
            var log = LogManager.GetLogger(Global.CallerName());

            var tcs = new TaskCompletionSource<object>();
            const int TIMEOUT = 1000;
            var sw = new System.Diagnostics.Stopwatch();

            try
            {
                sw.Start();
                while (Upgrading)
                {
                    if (sw.ElapsedMilliseconds > TIMEOUT)
                    {
                        log.Info("Wait for upgrade timeout");
                        break;
                    }                   
                }
                tcs.SetResult(null);
            }
            finally
            {
                sw.Stop();
            }
            
            return tcs.Task;
        }

        private void OnOpen()
        {
            var log = LogManager.GetLogger(Global.CallerName());

            //log.Info("socket open before call to flush()");
            ReadyState = ReadyStateEnum.OPEN;
            PriorWebsocketSuccess = WebSocket.NAME == Transport.Name;

            Flush();
            Emit(EVENT_OPEN);


            if (ReadyState == ReadyStateEnum.OPEN && Upgrade && Transport is Polling)
            //if (ReadyState == ReadyStateEnum.OPEN && Upgrade && this.Transport)
            {
                log.Info("OnOpen starting upgrade probes");
                _errorCount = 0;
                foreach (var upgrade in Upgrades)
                {
                    Probe(upgrade);
                }
            }
        }

        private void Probe(string name)
        {
            var log = LogManager.GetLogger(Global.CallerName());

            log.Info(string.Format("Probe probing transport '{0}'", name));

            PriorWebsocketSuccess = false;

            var transport = CreateTransport(name);
            var parameters = new ProbeParameters
            {
                Transport = ImmutableList<Transport>.Empty.Add(transport),
                Failed = ImmutableList<bool>.Empty.Add(false),
                Cleanup = ImmutableList<Action>.Empty,
                Socket = this
            };

            var onTransportOpen = new OnTransportOpenListener(parameters);
            var freezeTransport = new FreezeTransportListener(parameters);

            // Handle any error that happens while probing
            var onError = new ProbingOnErrorListener(this, parameters.Transport, freezeTransport);
            var onTransportClose = new ProbingOnTransportCloseListener(onError);

            // When the socket is closed while we're probing
            var onClose = new ProbingOnCloseListener(onError);

            var onUpgrade = new ProbingOnUpgradeListener(freezeTransport, parameters.Transport);



            parameters.Cleanup = parameters.Cleanup.Add(() =>
            {
                if (parameters.Transport.Count < 1)
                {
                    return;
                }

                parameters.Transport[0].Off(Transport.EVENT_OPEN, onTransportOpen);
                parameters.Transport[0].Off(Transport.EVENT_ERROR, onError);
                parameters.Transport[0].Off(Transport.EVENT_CLOSE, onTransportClose);
                Off(EVENT_CLOSE, onClose);
                Off(EVENT_UPGRADING, onUpgrade);
            });

            parameters.Transport[0].Once(Transport.EVENT_OPEN, onTransportOpen);
            parameters.Transport[0].Once(Transport.EVENT_ERROR, onError);
            parameters.Transport[0].Once(Transport.EVENT_CLOSE, onTransportClose);

            this.Once(EVENT_CLOSE, onClose);
            this.Once(EVENT_UPGRADING, onUpgrade);

            parameters.Transport[0].Open();
        }

        private class ProbeParameters
        {
            public ImmutableList<Transport> Transport { get; set; }
            public ImmutableList<bool> Failed { get; set; }
            public ImmutableList<Action> Cleanup { get; set; }
            public Socket Socket { get; set; }
        }

        private class OnTransportOpenListener : IListener
        {
            private ProbeParameters Parameters;


            public OnTransportOpenListener(ProbeParameters parameters)
            {
                this.Parameters = parameters;
            }

            void IListener.Call(params object[] args)
            {
                if (Parameters.Failed[0])
                {
                    return;
                }

                var packet = new Packet(Packet.PING, "probe");
                Parameters.Transport[0].Once(Client.Transport.EVENT_PACKET, new ProbeEventPacketListener(this));
                Parameters.Transport[0].Send(ImmutableList<Packet>.Empty.Add(packet));
            }

            private class ProbeEventPacketListener : IListener
            {
                private OnTransportOpenListener _onTransportOpenListener;

                public ProbeEventPacketListener(OnTransportOpenListener onTransportOpenListener)
                {
                    this._onTransportOpenListener = onTransportOpenListener;
                }


                void IListener.Call(params object[] args)
                {
                    if (_onTransportOpenListener.Parameters.Failed[0])
                    {
                        return;
                    }
                    var log = LogManager.GetLogger(Global.CallerName());

                    var msg = (Packet) args[0];
                    if (Packet.PONG == msg.Type && "probe" == (string) msg.Data)
                    {
                        //log.Info(
                        //    string.Format("probe transport '{0}' pong",
                        //        _onTransportOpenListener.Parameters.Transport[0].Name));

                        _onTransportOpenListener.Parameters.Socket.Upgrading = true;
                        _onTransportOpenListener.Parameters.Socket.Emit(EVENT_UPGRADING,
                            _onTransportOpenListener.Parameters.Transport[0]);
                        Socket.PriorWebsocketSuccess = WebSocket.NAME ==
                                                       _onTransportOpenListener.Parameters.Transport[0].Name;

                        //log.Info(
                        //    string.Format("pausing current transport '{0}'",
                        //        _onTransportOpenListener.Parameters.Socket.Transport.Name));
                        ((Polling) _onTransportOpenListener.Parameters.Socket.Transport).Pause(
                            () =>
                            {
                                if (_onTransportOpenListener.Parameters.Failed[0])
                                {
                                    // reset upgrading flag and resume polling
                                    ((Polling)_onTransportOpenListener.Parameters.Socket.Transport).Resume();
                                    _onTransportOpenListener.Parameters.Socket.Upgrading = false;
                                    _onTransportOpenListener.Parameters.Socket.Flush();
                                    return;
                                }
                                if (ReadyStateEnum.CLOSED == _onTransportOpenListener.Parameters.Socket.ReadyState ||
                                    ReadyStateEnum.CLOSING == _onTransportOpenListener.Parameters.Socket.ReadyState)
                                {
                                    return;
                                }

                                log.Info("changing transport and sending upgrade packet");

                                _onTransportOpenListener.Parameters.Cleanup[0]();

                                _onTransportOpenListener.Parameters.Socket.SetTransport(
                                    _onTransportOpenListener.Parameters.Transport[0]);
                                var packetList =
                                    ImmutableList<Packet>.Empty.Add(new Packet(Packet.UPGRADE));
                                try
                                {
                                    _onTransportOpenListener.Parameters.Transport[0].Send(packetList);

                                    _onTransportOpenListener.Parameters.Socket.Upgrading = false;
                                    _onTransportOpenListener.Parameters.Socket.Flush();

                                    _onTransportOpenListener.Parameters.Socket.Emit(EVENT_UPGRADE,
                                        _onTransportOpenListener.Parameters.Transport[0]);
                                    _onTransportOpenListener.Parameters.Transport =
                                        _onTransportOpenListener.Parameters.Transport.RemoveAt(0);

                                }
                                catch (Exception e)
                                {
                                    log.Error("",e);
                                }

                            });

                    }
                    else
                    {
                        log.Info(string.Format("probe transport '{0}' failed",
                            _onTransportOpenListener.Parameters.Transport[0].Name));

                        var err = new EngineIOException("probe error");
                        _onTransportOpenListener.Parameters.Socket.Emit(EVENT_UPGRADE_ERROR, err);
                    }

                }


                public int CompareTo(IListener other)
                {
                    return this.GetId().CompareTo(other.GetId());
                }

                public int GetId()
                {
                    return 0;
                }
            }

            public int CompareTo(IListener other)
            {
                return this.GetId().CompareTo(other.GetId());
            }

            public int GetId()
            {
                return 0;
            }
        }

        private class FreezeTransportListener : IListener
        {
            private ProbeParameters Parameters;

            public FreezeTransportListener(ProbeParameters parameters)
            {
                this.Parameters = parameters;
            }

            void IListener.Call(params object[] args)
            {
                if (Parameters.Failed[0])
                {
                    return;
                }

                Parameters.Failed = Parameters.Failed.SetItem(0, true);

                Parameters.Cleanup[0]();

                if (Parameters.Transport.Count < 1)
                {
                    return;
                }

                Parameters.Transport[0].Close();
                Parameters.Transport = ImmutableList<Transport>.Empty;
            }

            public int CompareTo(IListener other)
            {
                return this.GetId().CompareTo(other.GetId());
            }

            public int GetId()
            {
                return 0;
            }
        }

        private class ProbingOnErrorListener : IListener
        {
            private readonly Socket _socket;
            private readonly ImmutableList<Transport> _transport;
            private readonly IListener _freezeTransport;

            public ProbingOnErrorListener(Socket socket, ImmutableList<Transport> transport, IListener freezeTransport)
            {
                this._socket = socket;
                this._transport = transport;
                this._freezeTransport = freezeTransport;
            }

            void IListener.Call(params object[] args)
            {
                var err = args[0];
                EngineIOException error;
                if (err is Exception)
                {
                    error = new EngineIOException("probe error", (Exception)err);
                }
                else if (err is string)
                {
                    error = new EngineIOException("probe error: " + (string)err);
                }
                else
                {
                    error = new EngineIOException("probe error");
                }
                error.Transport = _transport[0].Name;

                _freezeTransport.Call();

                var log = LogManager.GetLogger(Global.CallerName());

                log.Info(string.Format("probe transport \"{0}\" failed because of error: {1}", error.Transport, err));
                _socket.Emit(EVENT_UPGRADE_ERROR, error);
            }

            public int CompareTo(IListener other)
            {
                return this.GetId().CompareTo(other.GetId());
            }

            public int GetId()
            {
                return 0;
            }
        }

        private class ProbingOnTransportCloseListener : IListener
        {
            private readonly IListener _onError;

            public ProbingOnTransportCloseListener(ProbingOnErrorListener onError)
            {
                this._onError = onError;
            }

            void IListener.Call(params object[] args)
            {
                _onError.Call("transport closed");
            }

            public int CompareTo(IListener other)
            {
                return this.GetId().CompareTo(other.GetId());
            }

            public int GetId()
            {
                return 0;
            }
        }

        private class ProbingOnCloseListener : IListener
        {
            private IListener _onError;

            public ProbingOnCloseListener(ProbingOnErrorListener onError)
            {
                this._onError = onError;
            }

            void IListener.Call(params object[] args)
            {
                _onError.Call("socket closed");
            }

            public int CompareTo(IListener other)
            {
                return this.GetId().CompareTo(other.GetId());
            }

            public int GetId()
            {
                return 0;
            }
        }

        private class ProbingOnUpgradeListener : IListener
        {
            private readonly IListener _freezeTransport;
            private readonly ImmutableList<Transport> _transport;

            public ProbingOnUpgradeListener(FreezeTransportListener freezeTransport, ImmutableList<Transport> transport)
            {
                this._freezeTransport = freezeTransport;
                this._transport = transport;
            }

            void IListener.Call(params object[] args)
            {
                var to = (Transport)args[0];
                if (_transport[0] != null && to.Name != _transport[0].Name)
                {
                    var log = LogManager.GetLogger(Global.CallerName());

                    log.Info(string.Format("'{0}' works - aborting '{1}'", to.Name, _transport[0].Name));
                    _freezeTransport.Call();
                }
            }

            public int CompareTo(IListener other)
            {
                return this.GetId().CompareTo(other.GetId());
            }

            public int GetId()
            {
                return 0;
            }
        }

        public Socket Close()
        {
            if (this.ReadyState == ReadyStateEnum.OPENING || this.ReadyState == ReadyStateEnum.OPEN)
            {
                var log = LogManager.GetLogger(Global.CallerName());
                log.Info("Start");                
                this.OnClose("forced close");

                log.Info("socket closing - telling transport to close");
                Transport.Close();

            }
            return this;
        }

        private void OnClose(string reason, Exception desc = null)
        {
            if (this.ReadyState == ReadyStateEnum.OPENING || this.ReadyState == ReadyStateEnum.OPEN)
            {
                var log = LogManager.GetLogger(Global.CallerName());

                log.Info(string.Format("OnClose socket close with reason: {0}", reason));

                // clear timers
                if (this.PingIntervalTimer != null)
                {
                    this.PingIntervalTimer.Stop();
                }
                if (this.PingTimeoutTimer != null)
                {
                    this.PingTimeoutTimer.Stop();
                }
                

                //WriteBuffer = WriteBuffer.Clear();
                //CallbackBuffer = CallbackBuffer.Clear();
                //PrevBufferLen = 0;

                EasyTimer.SetTimeout(() =>
                {
                    WriteBuffer = ImmutableList<Packet>.Empty;
                    CallbackBuffer = ImmutableList<Action>.Empty;
                    PrevBufferLen = 0;
                }, 1);

              
                if (this.Transport != null)
                {
                    // stop event from firing again for transport
                    this.Transport.Off(EVENT_CLOSE);

                    // ensure transport won't stay open
                    this.Transport.Close();

                    // ignore further transport communication
                    this.Transport.Off();
                }

                // set ready state
                this.ReadyState = ReadyStateEnum.CLOSED;

                // clear session id
                this.Id = null;

                // emit close events
                this.Emit(EVENT_CLOSE, reason, desc);
            }
        }

        public ImmutableList<string> FilterUpgrades(IEnumerable<string> upgrades)
        {
            var filterUpgrades = ImmutableList<string>.Empty;
            foreach (var upgrade in upgrades)
            {
                if (Transports.Contains(upgrade))
                {
                    filterUpgrades = filterUpgrades.Add(upgrade);
                }
            }
            return filterUpgrades;
        }



        internal void OnHeartbeat(long timeout)
        {
            if (this.PingTimeoutTimer != null)
            {
                PingTimeoutTimer.Stop();
                PingTimeoutTimer = null;
            }

            if (timeout <= 0)
            {
                timeout = this.PingInterval + this.PingTimeout;
            }

            PingTimeoutTimer = EasyTimer.SetTimeout(() =>
            {
                var log2 = LogManager.GetLogger(Global.CallerName());
                log2.Info("EasyTimer OnHeartbeat start");
                if (ReadyState == ReadyStateEnum.CLOSED)
                {
                    log2.Info("EasyTimer OnHeartbeat ReadyState == ReadyStateEnum.CLOSED finish");
                    return;
                }
                OnClose("ping timeout");
                log2.Info("EasyTimer OnHeartbeat finish");
            },(int) timeout);

        }

        private int _errorCount = 0;

        internal void OnError(Exception exception)
        {
            var log = LogManager.GetLogger(Global.CallerName());

            log.Error("socket error", exception);
            PriorWebsocketSuccess = false;

            //prevent endless loop
            if (_errorCount == 0)
            {
                _errorCount++;
                Emit(EVENT_ERROR, exception);
                OnClose("transport error", exception);                
            }
        }


    }
}
