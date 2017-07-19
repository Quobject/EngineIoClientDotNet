
using System.Text;
using System.Collections.Immutable;
using Quobject.EngineIoClientDotNet.ComponentEmitter;
using Quobject.EngineIoClientDotNet.Modules;
using Quobject.EngineIoClientDotNet.Parser;
using System;
using System.Collections.Generic;


namespace Quobject.EngineIoClientDotNet.Client
{
    public abstract class Transport : Emitter
    {
        protected enum ReadyStateEnum
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
        public static readonly string EVENT_SUCCESS = "success";
        public static readonly string EVENT_DATA = "data";
        public static readonly string EVENT_REQUEST_HEADERS = "requestHeaders";
        public static readonly string EVENT_RESPONSE_HEADERS = "responseHeaders";

        protected static int Timestamps = 0;

        private bool _writeable ;
        public bool Writable {
            get { return _writeable; } 
            set
            {
                var log = LogManager.GetLogger(Global.CallerName());
                log.Info(string.Format("Writable: {0} sid={1}", value, this.Socket.Id));
                _writeable = value;
            } 
        }

        private int myVar;

        public int MyProperty
        {
            get { return myVar; }
            set { myVar = value; }
        }
        
        public string Name;
        public Dictionary<string, string> Query;

        protected bool Secure;
        protected bool TimestampRequests;
        protected int Port;
        protected string Path;
        protected string Hostname;
        protected string TimestampParam;
        protected Socket Socket;
        protected bool Agent = false;
        protected bool ForceBase64 = false;
        protected bool ForceJsonp = false;
        protected string Cookie;

        protected Dictionary<string, string> ExtraHeaders;


        protected ReadyStateEnum ReadyState = ReadyStateEnum.CLOSED;

        protected Transport(Options options)
        {
            this.Path = options.Path;
            this.Hostname = options.Hostname;
            this.Port = options.Port;
            this.Secure = options.Secure;
            this.Query = options.Query;
            this.TimestampParam = options.TimestampParam;
            this.TimestampRequests = options.TimestampRequests;
            this.Socket = options.Socket;
            this.Agent = options.Agent;
            this.ForceBase64 = options.ForceBase64;
            this.ForceJsonp = options.ForceJsonp;
            this.Cookie = options.GetCookiesAsString();
            this.ExtraHeaders = options.ExtraHeaders;
        }

        protected Transport OnError(string message, Exception exception)
        {
            Exception err = new EngineIOException(message, exception);
            this.Emit(EVENT_ERROR, err);
            return this;
        }

        protected void OnOpen()
        {
            ReadyState = ReadyStateEnum.OPEN;
            Writable = true;
            Emit(EVENT_OPEN);
        }

        protected void OnClose()
        {
            ReadyState = ReadyStateEnum.CLOSED;
            Emit(EVENT_CLOSE);
        }


        protected virtual void OnData(string data)
        {
            this.OnPacket(Parser.Parser.DecodePacket(data));
        }

        protected virtual void OnData(byte[] data)
        {
            this.OnPacket(Parser.Parser.DecodePacket(data));
        }

        protected void OnPacket(Packet packet)
        {
            this.Emit(EVENT_PACKET, packet);
        }


        public Transport Open()
        {
            if (ReadyState == ReadyStateEnum.CLOSED)
            {
                ReadyState = ReadyStateEnum.OPENING;
                DoOpen();
            }
            return this;
        }

        public Transport Close()
        {
            if (ReadyState == ReadyStateEnum.OPENING || ReadyState == ReadyStateEnum.OPEN)
            {
                DoClose();
                OnClose();
            }
            return this;
        }

        public Transport Send(ImmutableList<Packet> packets)
        {
            var log = LogManager.GetLogger(Global.CallerName());
            log.Info("Send called with packets.Count: " + packets.Count);
            var count = packets.Count;
            if (ReadyState == ReadyStateEnum.OPEN)
            {
                //PollTasks.Exec((n) =>
                //{
                    Write(packets);
                //});
            }
            else
            {
                throw new EngineIOException("Transport not open");
                //log.Info("Transport not open");
            }
            return this;
        }



        protected abstract void DoOpen();

        protected abstract void DoClose();

        protected abstract void Write(ImmutableList<Packet> packets);


        public class Options
        {
            public bool Agent = false;
            public bool ForceBase64 = false;
            public bool ForceJsonp = false;
            public string Hostname;
            public string Path;
            public string TimestampParam;
            public bool Secure = false;
            public bool TimestampRequests = true;
            public int Port;
            public int PolicyPort;
            public Dictionary<string, string> Query;
            public bool IgnoreServerCertificateValidation = false;
            internal Socket Socket;
            public Dictionary<string, string> Cookies = new Dictionary<string, string>();
            public Dictionary<string, string> ExtraHeaders = new Dictionary<string, string>();

            public string GetCookiesAsString()
            {
                var result = new StringBuilder();
                var first = true;
                foreach (var item in Cookies)
                {
                    if (!first)
                    {
                        result.Append("; ");
                    }
                    result.Append(string.Format("{0}={1}", item.Key, item.Value));
                    first = false;
                }
                return result.ToString();
            }
        }
    }
}
