using System.Collections.Generic;
//using log4net;
using System.IO;
using Windows.Storage.Streams;
using EngineIoClientDotNet.Modules;
using Quobject.EngineIoClientDotNet.Modules;
using Quobject.EngineIoClientDotNet.Parser;
using Quobject.EngineIoClientDotNet.Thread;
using System;
using System.Collections.Immutable;
//using WebSocket4Net;
using Windows.Networking.Sockets;

namespace Quobject.EngineIoClientDotNet.Client.Transports
{
    public class WebSocket : Transport
    {
        public static readonly string NAME = "websocket";

        //private WebSocket4Net.WebSocket ws;
        private MessageWebSocket ws;

        public WebSocket(Options opts)
            : base(opts)
        {
            Name = NAME;
        }

        protected override async void DoOpen()
        {
            var log = LogManager.GetLogger(Global.CallerName());
            log.Info("DoOpen uri =" + this.Uri());

            ws = new Windows.Networking.Sockets.MessageWebSocket();
            // MessageWebSocket supports both utf8 and binary messages.
            // When utf8 is specified as the messageType, then the developer
            // promises to only send utf8-encoded data.
            
            ws.Control.MessageType = Windows.Networking.Sockets.SocketMessageType.Utf8;

            ws.MessageReceived += ws_MessageReceived;
            ws.Closed += ws_Closed;  
            
            var serverAddress = new Uri( this.Uri());

            try
            {
                await ws.ConnectAsync(serverAddress);
                ws_Opened();
            }
            catch (Exception e)
            {
                this.OnError("DoOpen", e);
            }                                                          
        }

        void ws_MessageReceived(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args)
        {
            var log = LogManager.GetLogger(Global.CallerName());

            try
            {
                using (var dataReader = args.GetDataReader())
                {
                    // The encoding and byte order need to match the settings of the writer 
                    // we previously used.
                    dataReader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                    dataReader.ByteOrder = Windows.Storage.Streams.ByteOrder.LittleEndian;

                    var data = dataReader.ReadString(dataReader.UnconsumedBufferLength);
                    log.Info("ws_MessageReceived e.Message= " + data);
                    this.OnData(data);
                }
            }
            catch (Exception e)
            {
                this.OnError("ws_MessageReceived", e);
            }
        }
   

        private void ws_Opened()
        {
            var log = LogManager.GetLogger(Global.CallerName());
            log.Info("ws_Opened");
            this.OnOpen();
        }

        void ws_Closed(IWebSocket sender, WebSocketClosedEventArgs args)
        {
            var log = LogManager.GetLogger(Global.CallerName());
            log.Info("ws_Closed");
            this.OnClose();
        }     


        protected override async void Write(System.Collections.Immutable.ImmutableList<Parser.Packet> packets)
        {
            Writable = false;

            try
            {
                foreach (var packet in packets)
                {
                    Parser.Parser.EncodePacket(packet, new WriteEncodeCallback(this));
                }
                //Parser.Parser.EncodePayload(packets.ToArray(), new WriteEncodeCallback(this));

                // fake drain
                // defer to next tick to allow Socket to clear writeBuffer
                await EasyTimer.SetTimeoutAsync(() =>
                {
                    Writable = true;
                    Emit(EVENT_DRAIN);
                }, 1);
            }
            catch (Exception e)
            {
                this.OnError("Write", e);
            }
        }

        public class WriteEncodeCallback : IEncodeCallback
        {
            private readonly WebSocket webSocket;

            public WriteEncodeCallback(WebSocket webSocket)
            {
                this.webSocket = webSocket;
            }

            public async void Call(object data)
            {
                //var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());

                if (data is string)
                {                    
                    var writer = new DataWriter(this.webSocket.ws.OutputStream);
                    writer.WriteString((string)data);
                    await writer.StoreAsync();                                     
                    await writer.FlushAsync();
                }
                else if (data is byte[])
                {
                    var d = (byte[])data;

                    //try
                    //{
                    //    var dataString = BitConverter.ToString(d);
                    //    //log.Info(string.Format("WriteEncodeCallback byte[] data {0}", dataString));
                    //}
                    //catch (Exception e)
                    //{
                    //    log.Error(e);
                    //}

                    //webSocket.ws.Send(d, 0, d.Length);
                    throw new NotImplementedException();
                }
            }
        }



        protected override void DoClose()
        {
            if (ws != null)
            {
               
                try
                {
                    ws.Closed -= ws_Closed;
                    ws.MessageReceived -= ws_MessageReceived;
                    ws.Close(1000, "DoClose");
                    ws.Dispose();
                }
                catch (Exception e)
                {
                    var log = LogManager.GetLogger(Global.CallerName());
                    log.Info("DoClose ws.Close() Exception= " + e.Message);
                }
            }
        }



        public string Uri()
        {
            Dictionary<string, string> query = null;
            query = this.Query == null ? new Dictionary<string, string>() : new Dictionary<string, string>(this.Query);
            string schema = this.Secure ? "wss" : "ws";
            string portString = "";

            if (this.TimestampRequests)
            {
                query.Add(this.TimestampParam, DateTime.Now.Ticks.ToString() + "-" + Transport.Timestamps++);
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
