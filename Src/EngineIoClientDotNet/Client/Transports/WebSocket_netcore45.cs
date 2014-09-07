using System.Collections.Generic;
//using log4net;
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

        public WebSocket(Options opts)
            : base(opts)
        {
            Name = NAME;
        }

        protected override async void DoOpen()
        {
            var log = LogManager.GetLogger(Global.CallerName());
            log.Info("DoOpen uri =" + this.Uri());

            var webSocket = new Windows.Networking.Sockets.MessageWebSocket();
            // MessageWebSocket supports both utf8 and binary messages.
            // When utf8 is specified as the messageType, then the developer
            // promises to only send utf8-encoded data.
            
            webSocket.Control.MessageType = Windows.Networking.Sockets.SocketMessageType.Utf8;

            webSocket.MessageReceived += webSocket_MessageReceived;
            webSocket.Closed += webSocket_Closed;  
            
             var serverAddress = new Uri( this.Uri());

      try
      {
          var t = webSocket.ConnectAsync(serverAddress);
          t.GetResults();
         
         ConnectAsync(serverAddress);
         
         /Completed(function () {
         var messageWebSocket = webSocket;
         // The default DataWriter encoding is utf8.
         messageWriter = new Windows.Storage.Streams.DataWriter(webSocket.OutputStream);
         messageWriter.writeString(document.getElementById("inputField").value);
         messageWriter.storeAsync().done("", sendError);

      }, function (error) {
         // The connection failed; add your own code to log or display 
         // the error, or take a specific action.
         });
      } catch (error) {
         // An error occurred while trying to connect; add your own code to  
         // log or display the error, or take a specific action.
      }
                                          
        }

        void webSocket_Closed(IWebSocket sender, WebSocketClosedEventArgs args)
        {
            var log = LogManager.GetLogger(Global.CallerName());
            log.Info("ws_Closed");
            this.OnClose();
        }

        void webSocket_MessageReceived(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args)
        {
            var log = LogManager.GetLogger(Global.CallerName());
            log.Info("ws_MessageReceived e.Message= " + args.Message);
            this.OnData(e.Message);
        }

        void ws_DataReceived(object sender, DataReceivedEventArgs e)
        {
            var log = LogManager.GetLogger(Global.CallerName());
            log.Info("ws_DataReceived " + e.Data);
            this.OnData(e.Data);
        }

        private void ws_Opened(object sender, EventArgs e)
        {
            var log = LogManager.GetLogger(Global.CallerName());
            log.Info("ws_Opened " + ws.SupportBinary);
            this.OnOpen();
        }

     

    

        void ws_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            this.OnError("websocket error", e.Exception);
        }

        protected override void Write(System.Collections.Immutable.ImmutableList<Parser.Packet> packets)
        {
            Writable = false;
            foreach (var packet in packets)
            {
                Parser.Parser.EncodePacket(packet, new WriteEncodeCallback(this));
            }
            //Parser.Parser.EncodePayload(packets.ToArray(), new WriteEncodeCallback(this));

            // fake drain
            // defer to next tick to allow Socket to clear writeBuffer
            EasyTimer.SetTimeout(() =>
            {
                Writable = true;
                Emit(EVENT_DRAIN);
            }, 1);
        }

        public class WriteEncodeCallback : IEncodeCallback
        {
            private WebSocket webSocket;

            public WriteEncodeCallback(WebSocket webSocket)
            {
                this.webSocket = webSocket;
            }

            public void Call(object data)
            {
                //var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());

                if (data is string)
                {                    
                    webSocket.ws.Send((string)data);
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

                    webSocket.ws.Send(d, 0, d.Length);
                }
            }
        }



        protected override void DoClose()
        {
            if (ws != null)
            {
                ws.Opened -= ws_Opened;
                ws.Closed -= ws_Closed;
                ws.MessageReceived -= ws_MessageReceived;
                ws.DataReceived -= ws_DataReceived;
                ws.Error -= ws_Error;
                //try
                //{
                //    ws.Close();
                //}
                //catch (Exception e)
                //{
                //    var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
                //    log.Info("DoClose ws.Close() Exception= " + e.Message);                                          
                //}
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
