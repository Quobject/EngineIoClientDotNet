//using log4net;

using System.Net.Http;
using EngineIoClientDotNet.Modules;
using Quobject.EngineIoClientDotNet.ComponentEmitter;
using Quobject.EngineIoClientDotNet.Modules;
using Quobject.EngineIoClientDotNet.Thread;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace Quobject.EngineIoClientDotNet.Client.Transports
{
    public class PollingXHR : Polling
    {
        private XHRRequest sendXhr;

        public PollingXHR(Options options)
            : base(options)
        {

        }

        protected XHRRequest Request()
        {
            return Request(null);
        }



        protected XHRRequest Request(XHRRequest.RequestOptions opts)
        {
            if (opts == null)
            {
                opts = new XHRRequest.RequestOptions();
            }
            opts.Uri = Uri();


            XHRRequest req = new XHRRequest(opts);

            req.On(EVENT_REQUEST_HEADERS, new EventRequestHeadersListener(this)).
                On(EVENT_RESPONSE_HEADERS, new EventResponseHeadersListener(this));


            return req;
        }

        class EventRequestHeadersListener : IListener
        {
            private PollingXHR pollingXHR;

            public EventRequestHeadersListener(PollingXHR pollingXHR)
            {

                this.pollingXHR = pollingXHR;
            }

            public void Call(params object[] args)
            {
                // Never execute asynchronously for support to modify headers.
                pollingXHR.Emit(EVENT_RESPONSE_HEADERS, args[0]);
            }
        }

        class EventResponseHeadersListener : IListener
        {
            private PollingXHR pollingXHR;

            public EventResponseHeadersListener(PollingXHR pollingXHR)
            {
                this.pollingXHR = pollingXHR;
            }
            public void Call(params object[] args)
            {
                pollingXHR.Emit(EVENT_REQUEST_HEADERS, args[0]);
            }
        }


        protected override void DoWrite(byte[] data, Action action)
        {
            var opts = new XHRRequest.RequestOptions { Method = "POST", Data = data };
            var log = LogManager.GetLogger(Global.CallerName());
            log.Info("DoWrite data = " + data);
            //try
            //{
            //    var dataString = BitConverter.ToString(data);
            //    log.Info(string.Format("DoWrite data {0}", dataString));
            //}
            //catch (Exception e)
            //{
            //    log.Error(e);
            //}

            sendXhr = Request(opts);
            sendXhr.On(EVENT_SUCCESS, new SendEventSuccessListener(action));
            sendXhr.On(EVENT_ERROR, new SendEventErrorListener(this));
            sendXhr.Create();
        }

        class SendEventErrorListener : IListener
        {
            private PollingXHR pollingXHR;

            public SendEventErrorListener(PollingXHR pollingXHR)
            {
                this.pollingXHR = pollingXHR;
            }

            public void Call(params object[] args)
            {
                Exception err = args.Length > 0 && args[0] is Exception ? (Exception)args[0] : null;
                pollingXHR.OnError("xhr post error", err);
            }
        }

        class SendEventSuccessListener : IListener
        {
            private Action action;

            public SendEventSuccessListener(Action action)
            {
                this.action = action;
            }

            public void Call(params object[] args)
            {
                action();
            }
        }


        protected override void DoPoll()
        {
            //var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());

            //log.Info("xhr poll");
            sendXhr = Request();
            sendXhr.On(EVENT_DATA, new DoPollEventDataListener(this));
            sendXhr.On(EVENT_ERROR, new DoPollEventErrorListener(this));
            sendXhr.Create();
        }

        class DoPollEventDataListener : IListener
        {
            private PollingXHR pollingXHR;

            public DoPollEventDataListener(PollingXHR pollingXHR)
            {
                this.pollingXHR = pollingXHR;
            }


            public void Call(params object[] args)
            {
                object arg = args.Length > 0 ? args[0] : null;
                if (arg is string)
                {
                    pollingXHR.OnData((string)arg);
                }
                else if (arg is byte[])
                {
                    pollingXHR.OnData((byte[])arg);
                }
            }
        }

        class DoPollEventErrorListener : IListener
        {
            private PollingXHR pollingXHR;

            public DoPollEventErrorListener(PollingXHR pollingXHR)
            {
                this.pollingXHR = pollingXHR;
            }

            public void Call(params object[] args)
            {
                Exception err = args.Length > 0 && args[0] is Exception ? (Exception)args[0] : null;
                pollingXHR.OnError("xhr poll error", err);
            }
        }


        public class XHRRequest : Emitter
        {
            private string Method;
            private string Uri;
            private byte[] Data;
            private HttpClient httpClient;

            public XHRRequest(RequestOptions options)
            {
                Method = options.Method ?? "GET";
                Uri = options.Uri;
                Data = options.Data;
            }

            public async void Create()
            {
                var log = LogManager.GetLogger(Global.CallerName());

                try
                {
                    log.Info(string.Format("xhr open {0}: {1}", Method, Uri));
                    httpClient = new HttpClient();
                    httpClient.MaxResponseContentBufferSize = 256000;
                    httpClient.DefaultRequestHeaders.Add("user-agent",
                        "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");
                    //httpClient.Method = Method;


          
                    if (Data != null)
                    {
                        //httpClient.ContentType = "application/octet-stream";
                        //httpClient.ContentLength = Data.Length;

                        //using (var requestStream = httpClient.GetRequestStream())
                        //{
                        //    requestStream.Write(Data, 0, Data.Length);

                        //}
                    }

                    if (Method == "GET")
                    {
                        HttpResponseMessage response = await httpClient.GetAsync(Uri);
                        response.EnsureSuccessStatusCode();

                        var t = response.Headers;

                        var responseHeaders = new Dictionary<string, string>();
                        foreach (var h in response.Headers)
                        {
                            string value = "";
                            foreach (var c in h.Value)
                            {
                                value += c;
                            }

                            responseHeaders.Add(h.Key, value);
                        }
                        OnResponseHeaders(responseHeaders);

                        var contentType = responseHeaders.ContainsKey("Content-Type")
                                ? responseHeaders["Content-Type"]
                                : null;

                        if (contentType != null && contentType.Equals("application/octet-stream", StringComparison.OrdinalIgnoreCase))
                        {
                            var responseBodyAsByteArray = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                            OnData(responseBodyAsByteArray);                                                      
                        }
                        else
                        {
                            var responseBodyAsText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);  
                            OnData(responseBodyAsText);                          
                        }
                    }          
                }
                catch (Exception e)
                {
                    log.Error(e);
                    OnError(e);
                    return;
                }                
            }


            private void OnSuccess()
            {
                this.Emit(EVENT_SUCCESS);
            }

            private void OnData(string data)
            {
                var log = LogManager.GetLogger(Global.CallerName());
                log.Info("OnData string = " + data);
                this.Emit(EVENT_DATA, data);
                this.OnSuccess();
            }

            private void OnData(byte[] data)
            {
                var log = LogManager.GetLogger(Global.CallerName());
                log.Info(string.Format("OnData byte[] ={0}", System.Text.Encoding.UTF8.GetString(data,0,data.Length)));
                this.Emit(EVENT_DATA, data);
                this.OnSuccess();
            }

            private void OnError(Exception err)
            {
                this.Emit(EVENT_ERROR, err);
            }

            private void OnRequestHeaders(Dictionary<string, string> headers)
            {
                this.Emit(EVENT_REQUEST_HEADERS, headers);
            }

            private void OnResponseHeaders(Dictionary<string, string> headers)
            {
                this.Emit(EVENT_RESPONSE_HEADERS, headers);
            }

            public class RequestOptions
            {
                public string Uri;
                public string Method;
                public byte[] Data;
            }
        }



    }

}
