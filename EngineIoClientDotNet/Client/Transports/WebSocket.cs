using Quobject.EngineIoClientDotNet.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocket4Net;

namespace Quobject.EngineIoClientDotNet.Client.Transports
{
    public class WebSocket : Transport
    {
        public static readonly string NAME = "websocket";

        private WebSocket4Net.WebSocket ws;

        protected WebSocket(Options opts)
            : base(opts)
        {
            Name = NAME;
        }


        protected override void DoOpen()
        {
            var headers = new Dictionary<string, string>();
            Emit(EVENT_REQUEST_HEADERS, headers);
            var wsHeaders = new List<KeyValuePair<string, string>>();

            ws = new WebSocket4Net.WebSocket(this.Uri(),"",null,wsHeaders,"","",WebSocketVersion.Rfc6455);


        }

        protected override void DoClose()
        {
            throw new NotImplementedException();
        }

        protected override void Write(System.Collections.Immutable.ImmutableList<Parser.Packet> packets)
        {
            throw new NotImplementedException();
        }

        protected string Uri()
        {
            Dictionary<string, string> query = this.Query;
            if (query == null)
            {
                query = new Dictionary<string, string>();
            }
            string schema = this.Secure ? "wss" : "ws";
            string portString = "";

            if (this.TimestampRequests)
            {
                query.Add(this.TimestampParam, DateTime.Now + "-" + Transport.Timestamps++);
            }

            string _query = ParseQS.Encode(query);

            if (this.Port > 0 && (("wss" == schema && this.Port != 443)
                    || ("ws" == schema && this.Port != 80)))
            {
                portString = ":" + this.Port;
            }

            if (_query.Length > 0)
            {
                _query = "?" + _query;
            }

            return schema + "://" + this.Hostname + portString + this.Path + _query;
        }
    }
}
