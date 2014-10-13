using Quobject.Collections.Immutable;
using Quobject.EngineIoClientDotNet.Modules;
using Quobject.EngineIoClientDotNet.Parser;
using Quobject.EngineIoClientDotNet.Thread;
using System;
using System.Collections.Generic;

using System.IO;
//using WebSocket4Net;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace Quobject.EngineIoClientDotNet.Client.Transports
{
    public class WebSocket : Transport
    {
        public static readonly string NAME = "websocket";

        // How to connect with a MessageWebSocket http://msdn.microsoft.com/en-us/library/windows/apps/xaml/hh994397.aspx
        private MessageWebSocket ws;
        private DataWriter dataWriter;
        private string CookieHeaderValue;

        public WebSocket(Options opts)
            : base(opts)
        {
            Name = NAME;
            CookieHeaderValue = opts.GetCookiesAsString();
        }

        protected override void DoOpen()
        {
            var log = LogManager.GetLogger(Global.CallerName());
            log.Info("DoOpen uri =" + this.Uri());

            //How to connect with a StreamWebSocket (XAML) http://msdn.microsoft.com/en-us/library/ie/hh994398
            ws = new Windows.Networking.Sockets.MessageWebSocket();

            if (!string.IsNullOrEmpty(CookieHeaderValue))
            {
                ws.SetRequestHeader("cookie", CookieHeaderValue);                
            }           
            ws.Control.MessageType = SocketMessageType.Utf8;
            ws.Closed += ws_Closed;
            ws.MessageReceived += ws_MessageReceived;
       
            var serverAddress = new Uri( this.Uri());

            try
            {
                //await ws.ConnectAsync(serverAddress);
                //  http://stackoverflow.com/questions/13027449/wait-for-a-thread-to-complete-in-metro               
                //Task.WaitAny(ws.ConnectAsync(serverAddress).AsTask());
                
                var task =ws.ConnectAsync(serverAddress).AsTask();
                task.Wait();
                if (task.IsFaulted)
                {
                    throw new EngineIOException(task.Exception.Message,task.Exception);
                }

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
                if (args.MessageType == SocketMessageType.Utf8)
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
                }else if  (args.MessageType == SocketMessageType.Binary)
                {
                    using (var dataReader = args.GetDataReader())
                    {
                        dataReader.ByteOrder = Windows.Storage.Streams.ByteOrder.LittleEndian;
                        byte[] bytes = new byte[dataReader.UnconsumedBufferLength];
                        dataReader.ReadBytes(bytes);

                        log.Info("ws_MessageReceived e.Message= " + bytes);
                        this.OnData(bytes);
                    }
                }
                else
                {
                    throw new InvalidDataException("MessageWebSocketMessageReceivedEventArgs wrong MessageType ");
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


        protected override void Write(ImmutableList<Parser.Packet> packets)
        {
            Writable = false;

            try
            {
                foreach (var packet in packets)
                {
                    Parser.Parser.EncodePacket(packet, new WriteEncodeCallback(this));
                }

                // fake drain
                // defer to next tick to allow Socket to clear writeBuffer
                EasyTimer.SetTimeout(() =>
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

            public void Call(object data)
            {
                //var log = LogManager.GetLogger(Global.CallerName());

                var writer = webSocket.dataWriter;

                if (writer == null)
                {
                    webSocket.dataWriter = new DataWriter(this.webSocket.ws.OutputStream);

                    webSocket.dataWriter.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                    webSocket.dataWriter.ByteOrder = Windows.Storage.Streams.ByteOrder.LittleEndian;

                    writer = webSocket.dataWriter;
                }

                if (data is string)
                {                    
                    
                    writer.WriteString((string) data);
                    //writer.wr
                    //writer.FlushAsync()

                    var task = writer.StoreAsync().AsTask();
                    task.Wait();
                    if (task.IsFaulted)
                    {
                        throw new EngineIOException(task.Exception.Message,task.Exception);
                    }
                    //var task2 = writer.FlushAsync().AsTask();
                    //task2.Wait();
                    //if (task2.IsFaulted)
                    //{
                    //    throw new EngineIOException(task2.Exception.Message, task2.Exception);
                    //}                    
                }
                else if (data is byte[])
                {
                    var d = (byte[])data;

                    //webSocket.ws.Send(d, 0, d.Length);
                    
                    // Buffer any data we want to send.
                    writer.WriteBytes(d);

                    // Send the data as one complete message.
                    //await messageWriter.StoreAsync();

                    var task = writer.StoreAsync().AsTask();
                    task.Wait();
                    if (task.IsFaulted)
                    {
                        throw new EngineIOException(task.Exception.Message, task.Exception);
                    }  

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
                    //ws.MessageReceived -= ws_MessageReceived;
                   
                    if (dataWriter != null)
                    {
                        dataWriter.Dispose();
                        dataWriter = null;
                    }

                    ws.Close(1000, "DoClose");
                    ws.Dispose();
                    ws = null;
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
