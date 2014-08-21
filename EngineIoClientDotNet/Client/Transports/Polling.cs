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

        public void Pause(Action onPause)
        {
            EventTasks.Exec(n =>
            {
                ReadyState = ReadyStateEnum.PAUSED;
                Action pause = () =>
                {
                    Debug.WriteLine("paused", "Polling fine");
                    ReadyState = ReadyStateEnum.PAUSED;
                    onPause();
                };

                if (IsPolling || !Writable)
                {
                    var total = new[] {0};


                    if (IsPolling)
                    {
                        Debug.WriteLine("we are currently polling - waiting to pause", "Polling fine");
                        total[0]++;
                        Once(EVENT_POLL_COMPLETE, new PauseEventPollCompleteListener(total, pause));

                    }

                    if (!Writable)
                    {
                        Debug.WriteLine("we are currently writing - waiting to pause", "Polling fine");
                        total[0]++;
                        Once(EVENT_DRAIN, new PauseEventDrainListener(total, pause));
                    }

                }
                else
                {
                    pause();
                }
            });
        }

        private class PauseEventDrainListener : IListener
        {
            private int[] total;
            private Action pause;

            public PauseEventDrainListener(int[] total, Action pause)
            {
                this.total = total;
                this.pause = pause;
            }

            public void Call(params object[] args)
            {
                Debug.WriteLine("pre-pause writing complete", "Polling fine");
                if (--total[0] == 0)
                {
                    pause();
                }
            }
        }

        class PauseEventPollCompleteListener : IListener
        {
            private int[] total;
            private Action pause;

            public PauseEventPollCompleteListener(int[] total, Action pause)
            {

                this.total = total;
                this.pause = pause;
            }
            
            public void Call(params object[] args)
            {
                Debug.WriteLine("pre-pause polling complete", "Polling fine");
                if (--total[0] == 0)
                {
                    pause();
                }
            }
        }


        private void Poll()
        {
            EventTasks.Exec(n =>
            {
                Debug.WriteLine("polling", "Polling fine");
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
            Debug.WriteLine(string.Format("polling got data {0}",data), "Polling fine");
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
                    Debug.WriteLine(string.Format("ignoring poll - transport state {0}", ReadyState), "Polling fine");                    
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
                Debug.WriteLine("writing close packet", "Polling fine");
                ImmutableList<Packet> packets = ImmutableList<Packet>.Empty;
                packets = packets.Add(new Packet(Packet.CLOSE));
                polling.Write(packets);
            }
        }

        protected override void DoClose()
        {
            var closeListener = new CloseListener(this);

            if (ReadyState == ReadyStateEnum.OPEN)
            {
                Debug.WriteLine("transport open - closing", "Polling fine");
                closeListener.Call();
            }
            else
            {
                // in case we're trying to close while
                // handshaking is in progress (engine.io-client GH-164)
                Debug.WriteLine("transport not open - deferring close", "Polling fine");
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
                polling.DoWrite(byteData, () =>
                {
                    polling.Writable = true;
                    polling.Emit(EVENT_DRAIN);
                });
            }

        }


        protected override void Write(ImmutableList<Packet> packets)
        {
            Writable = false;

            var callback = new SendEncodeCallback(this);
            Parser.Parser.EncodePayload(packets.ToArray(), callback);
        }

        protected string Uri()
        {
            var query = this.Query;
            if (query == null)
            {
                query = ImmutableDictionary<string, string>.Empty;
            }
            string schema = this.Secure ? "https" : "http";
            string portString = "";

            if (this.TimestampRequests)
            {
                query = query.Add(this.TimestampParam, DateTime.Now.Ticks + "-" + Transport.Timestamps++);
            }

            query = query.Add("b64", "1");



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

        protected abstract void DoWrite(byte[] data, Action action);
        protected abstract void DoPoll();




    }
}
