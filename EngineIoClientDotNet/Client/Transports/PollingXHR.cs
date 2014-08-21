using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;
using Quobject.EngineIoClientDotNet.ComponentEmitter;
using Quobject.EngineIoClientDotNet.Thread;

namespace Quobject.EngineIoClientDotNet.Client.Transports
{
    public class PollingXHR : Polling
    {
        private XHRRequest sendXhr;
        private XHRRequest pollXhr;

        public PollingXHR(Options options) : base(options)
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
            opts.SslContext = SSLContext;

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
            var opts = new XHRRequest.RequestOptions {Method = "POST", Data = data};

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
                Exception err = args.Length > 0 && args[0] is Exception ? (Exception) args[0] : null;
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
            Debug.WriteLine("xhr poll", "PollingXHR fine");
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
            private SSLContext SslContext;
            private HttpWebRequest Xhr;

            public XHRRequest(RequestOptions options)
            {
                Method = options.Method ?? "GET";
                Uri = options.Uri;
                Data = options.Data;
                SslContext = options.SslContext;
            }

            public void Create()
            {
                //if (Data == null)
                //{
                //    return;
                //}

                try
                {
                    Debug.WriteLine(string.Format("xhr open {0}: {0}",Method, Uri), "PollingXHR.Request fine");
                    Xhr = (HttpWebRequest)WebRequest.Create(Uri);
                    Xhr.Method = Method;
                }
                catch (Exception e)
                {
                    OnError(e);
                    return;
                }

                var headers = new Dictionary<string, string>();

                if (Method == "POST")
                {
                    //xhr.setDoOutput(true);
                    Xhr.ContentType = "application/octet-stream";
                    headers.Add("Content-type", "application/octet-stream");
                }

                OnRequestHeaders(headers);
                Debug.WriteLine(string.Format("sending xhr with url {0} | data {1}", Uri, Data), "PollingXHR.Request fine");

                RequestTasks.Exec( n =>
                {                    
                    try
                    {
                        if (Data != null)
                        {
                            Xhr.ContentLength = Data.Length;

                            using (var requestStream = Xhr.GetRequestStream())
                            {
                                requestStream.Write(Data, 0, Data.Length);
                            }                            
                        }

                        using (var res = Xhr.GetResponse())
                        {
                            var responseHeaders = new Dictionary<string, string>();
                            for (int i = 0; i < res.Headers.Count; i++)
                            {
                                Debug.WriteLine(string.Format("Header Name:{0}, Header value :{1}", res.Headers.Keys[i], res.Headers[i]), "PollingXHR.Request fine");
                                responseHeaders.Add(res.Headers.Keys[i], res.Headers[i]);
                            }

                            var contentType = res.Headers["Content-Type"];


                            OnResponseHeaders(headers);

                            using (var resStream = res.GetResponseStream())
                            {
                                Debug.Assert(resStream != null, "resStream != null");
                                if (contentType.Equals("application/octet-stream",
                                    StringComparison.OrdinalIgnoreCase))
                                {
                                    var buffer = new byte[16*1024];
                                    using (var ms = new MemoryStream())
                                    {
                                        int read;
                                        while ((read = resStream.Read(buffer, 0, buffer.Length)) > 0)
                                        {
                                            ms.Write(buffer, 0, read);
                                        }
                                        OnData(ms.ToArray());
                                    }
                                }
                                else
                                {
                                    using (var sr = new StreamReader(resStream))
                                    {
                                        OnData(sr.ReadToEnd());
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        OnError(e);
                    }
                });
            }


            private void OnSuccess()
            {
                this.Emit(EVENT_SUCCESS);
                //this.Cleanup();
            }

            private void OnData(string data)
            {
                this.Emit(EVENT_DATA, data);
                this.OnSuccess();
            }

            private void OnData(byte[] data)
            {
                this.Emit(EVENT_DATA, data);
                this.OnSuccess();
            }

            private void OnError(Exception err)
            {
                this.Emit(EVENT_ERROR, err);
                //this.Cleanup();
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
                public SSLContext SslContext;
            }
        }



    }

}
