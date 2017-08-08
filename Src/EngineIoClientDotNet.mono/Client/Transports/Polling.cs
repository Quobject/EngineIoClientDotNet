
using System.Collections.Immutable;
using Quobject.EngineIoClientDotNet.ComponentEmitter;
using Quobject.EngineIoClientDotNet.Modules;
using Quobject.EngineIoClientDotNet.Parser;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Quobject.EngineIoClientDotNet.Client.Transports
{
    public class Polling : Transport
    {
        public static readonly string NAME = "polling";
        public static readonly string EVENT_POLL = "poll";
        public static readonly string EVENT_POLL_COMPLETE = "pollComplete";

        private bool IsPolling = false;

        public Polling(Options opts) : base(opts)
        {
            Name = NAME;
        }


        protected override void DoOpen()
        {
            Poll();
        }

        public void Pause(Action onPause)
        {
            //var log = LogManager.GetLogger(Global.CallerName());

            ReadyState = ReadyStateEnum.PAUSED;
            Action pause = () =>
            {
                //log.Info("paused");
                ReadyState = ReadyStateEnum.PAUSED;
                onPause();
            };

            if (IsPolling || !Writable)
            {
                var total = new[] {0};


                if (IsPolling)
                {
                    //log.Info("we are currently polling - waiting to pause");
                    total[0]++;
                    Once(EVENT_POLL_COMPLETE, new PauseEventPollCompleteListener(total, pause));

                }

                if (!Writable)
                {
                    //log.Info("we are currently writing - waiting to pause");
                    total[0]++;
                    Once(EVENT_DRAIN, new PauseEventDrainListener(total, pause));
                }

            }
            else
            {
                pause();
            }
        }

        public void Resume()
        {
            if (ReadyState == ReadyStateEnum.PAUSED)
                ReadyState = ReadyStateEnum.OPEN;
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
                //var log = LogManager.GetLogger(Global.CallerName());

                //log.Info("pre-pause writing complete");
                if (--total[0] == 0)
                {
                    pause();
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
                //var log = LogManager.GetLogger(Global.CallerName());

                //log.Info("pre-pause polling complete");
                if (--total[0] == 0)
                {
                    pause();
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


        private void Poll()
        {
            //var log = LogManager.GetLogger(Global.CallerName());

            //log.Info("polling");
            IsPolling = true;
            DoPoll();
            Emit(EVENT_POLL);
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
            var log = LogManager.GetLogger(Global.CallerName());

            log.Info(string.Format("polling got data {0}",data));
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
                log.Info("ReadyState != ReadyStateEnum.CLOSED");
                Emit(EVENT_POLL_COMPLETE);

                if (ReadyState == ReadyStateEnum.OPEN)
                {
                    Poll();
                }
                else
                {
                    log.Info(string.Format("ignoring poll - transport state {0}", ReadyState));                    
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
                //var log = LogManager.GetLogger(Global.CallerName());

                //log.Info("writing close packet");
                ImmutableList<Packet> packets = ImmutableList<Packet>.Empty;
                packets = packets.Add(new Packet(Packet.CLOSE));
                polling.Write(packets);
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

        protected override void DoClose()
        {
            var log = LogManager.GetLogger(Global.CallerName());

            var closeListener = new CloseListener(this);

            if (ReadyState == ReadyStateEnum.OPEN)
            {                      
                log.Info("transport open - closing");
                closeListener.Call();
            }
            else
            {
                // in case we're trying to close while
                // handshaking is in progress (engine.io-client GH-164)
                log.Info("transport not open - deferring close");
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
                //var log = LogManager.GetLogger(Global.CallerName());
                //log.Info("SendEncodeCallback data = " + data);

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
            var log = LogManager.GetLogger(Global.CallerName());
            log.Info("Write packets.Count = " + packets.Count);

            Writable = false;

            var callback = new SendEncodeCallback(this);
            Parser.Parser.EncodePayload(packets.ToArray(), callback);
        }

        public string Uri()
        {
            //var query = this.Query;
            var query = new Dictionary<string, string>(Query);
            //if (Query == null)
            //{
            //    query = new Dictionary<string, string>();
            //}
            string schema = this.Secure ? "https" : "http";
            string portString = "";

            if (this.TimestampRequests)
            {
                query.Add(this.TimestampParam, DateTime.Now.Ticks + "-" + Transport.Timestamps++);
            }

            query.Add("b64", "1");



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

        protected virtual void DoWrite(byte[] data, Action action)
        {
            
        }

        protected virtual void DoPoll()
        {
            
        }




    }
}
