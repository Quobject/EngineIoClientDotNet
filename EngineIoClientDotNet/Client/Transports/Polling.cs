using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Quobject.EngineIoClientDotNet.ComponentEmitter;
using Quobject.EngineIoClientDotNet.Parser;
using Quobject.EngineIoClientDotNet.Thread;
using Quobject.EngineIoClientDotNet.Modules;

namespace Quobject.EngineIoClientDotNet.Client.Transports
{
    abstract public class Polling : Transport
    {
        public static readonly string NAME = "polling";
        public static readonly string EVENT_POLL = "poll";
        public static readonly string EVENT_POLL_COMPLETE = "pollComplete";

        private bool IsPolling = false;

        protected Polling(Options opts) : base(opts)
        {
            Name = NAME;
        }

        protected override void DoOpen()
        {
            Poll();
        }

        private void Poll()
        {
            EventTasks.Exec(n =>
            {
                Debug.WriteLine("polling", "fine");
                IsPolling = true;
                DoPoll();
                Emit(EVENT_POLL);
            });
        }



        protected override void OnData(string data)
        {
            _onData(data);
        }

        protected override void OnData(byte[] data)
        {
            _onData(data);
        }


        private class DecodePayloadCallback : IDecodePayloadCallback
        {
            private Polling polling;

            public DecodePayloadCallback(Polling polling)
            {
                this.polling = polling;
            }
            public bool Call(Packet packet, int index, int total)
            {
                if (polling.ReadyState == ReadyStateEnum.OPENING)
                {
                    polling.OnOpen();
                }

                if (packet.Type == Packet.CLOSE)
                {
                    polling.OnClose();
                    return false;
                }

                polling.OnPacket(packet);
                return true;
            }
        }


        private void _onData(object data)
        {
            Debug.WriteLine(string.Format("polling got data {0}",data), "fine");
            var callback = new DecodePayloadCallback(this);
            if (data is string)
            {
                Parser.Parser.DecodePayload((string)data, callback);
            }
            else if (data is byte[])
            {
                Parser.Parser.DecodePayload((byte[])data, callback);                
            }

            if (ReadyState != ReadyStateEnum.CLOSED)
            {
                IsPolling = false;
                Emit(EVENT_POLL_COMPLETE);

                if (ReadyState == ReadyStateEnum.OPEN)
                {
                    Poll();
                }
                else
                {
                    Debug.WriteLine(string.Format("ignoring poll - transport state {0}", ReadyState), "fine");                    
                }
            }

        }

        private class CloseListener : IListener
        {
            private Polling polling;

            public CloseListener(Polling polling)
            {
                this.polling = polling;
            }

            public void Call(params object[] args)
            {
                Debug.WriteLine("writing close packet", "fine");
                polling.Send(new Packet[] {new Packet(Packet.CLOSE)});
            }
        }

        protected override void DoClose()
        {
            var closeListener = new CloseListener(this);

            if (ReadyState == ReadyStateEnum.OPEN)
            {
                Debug.WriteLine("transport open - closing", "fine");
                closeListener.Call();
            }
            else
            {
                // in case we're trying to close while
                // handshaking is in progress (engine.io-client GH-164)
                Debug.WriteLine("transport not open - deferring close", "fine");
                this.Once(EVENT_OPEN, closeListener);
            }
        }


        public class SendEncodeCallback : IEncodeCallback
        {
            private Polling polling;

            public SendEncodeCallback(Polling polling)
            {
                this.polling = polling;
            }

            public void Call(object data)
            {
                var byteData = (byte[]) data;
                polling.DoSend(byteData, () =>
                {
                    polling.Writable = true;
                    polling.Emit(EVENT_DRAIN);
                });
            }

        }

        internal void Send(Packet[] packets)
        {
            Writable = false;            

            var callback = new SendEncodeCallback(this);
            Parser.Parser.EncodePayload(packets, callback);
        }

        protected string Uri()
        {
            Dictionary<string, string> query = this.Query;
            if (query == null)
            {
                query = new Dictionary<string, string>();
            }
            string schema = this.Secure ? "https" : "http";
            string portString = "";

            if (this.TimestampRequests)
            {
                query.Add(this.TimestampParam, DateTime.Now + "-" + Transport.Timestamps++);
            }

            string _query = ParseQS.Encode(query);

            if (this.Port > 0 && (("https" == schema && this.Port != 443)
                    || ("http" == schema && this.Port != 80)))
            {
                portString = ":" + this.Port;
            }

            if (_query.Length > 0)
            {
                _query = "?" + _query;
            }

            return schema + "://" + this.Hostname + portString + this.Path + _query;
        }

        protected abstract void DoSend(byte[] data, Action action);
        protected abstract void DoPoll();


    }
}
