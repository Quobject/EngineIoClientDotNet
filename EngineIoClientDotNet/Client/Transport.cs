using System;
using System.Collections.Generic;
using Quobject.EngineIoClientDotNet.ComponentEmitter;
using System.IO;
using System.Net;
using System.Net.Sockets;


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
        public static readonly string EVENT_REQUEST_HEADERS = "requestHeaders";
        public static readonly string EVENT_RESPONSE_HEADERS = "responseHeaders";

        protected static int Timestamps = 0;

        public bool Writable;
        public string Name;
        public Dictionary<string, string> Query;

        protected bool Secure;
        protected bool TimestampRequests;
        protected int Port;
        protected string Path;
        protected string Hostname;
        protected string TimestampParam;
        public SSLContext SSLContext;
        protected System.Net.Sockets.Socket Socket;

        protected ReadyStateEnum ReadyState;

        public Transport(Options options)
        {
            this.Path = options.Path;
            this.Hostname = options.Hostname;
            this.Port = options.Port;
            this.Secure = options.Secure;
            this.Query = options.Query;
            this.TimestampParam = options.TimestampParam;
            this.TimestampRequests = options.TimestampRequests;
            this.SSLContext = options.SSLContext;
            this.Socket = options.Socket;
        }

        protected Transport OnError(string message, Exception exception)
        {
            Exception err = new EngineIOException(message, exception);
            this.Emit(EVENT_ERROR, err);
            return this;
        }

        public Transport Open()
        {
            return null;
        }



        public class Options
        {

            public string Hostname;
            public string Path;
            public string TimestampParam;
            public bool Secure;
            public bool TimestampRequests;
            public int Port;
            public int PolicyPort;
            public Dictionary<string, string> Query;
            public SSLContext SSLContext;
            internal System.Net.Sockets.Socket Socket;
        }


    }
}
