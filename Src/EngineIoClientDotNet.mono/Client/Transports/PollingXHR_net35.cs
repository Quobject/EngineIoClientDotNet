


using Quobject.EngineIoClientDotNet.ComponentEmitter;
using Quobject.EngineIoClientDotNet.Modules;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Quobject.EngineIoClientDotNet.Client.Transports
{
    public class PollingXHR : Polling
    {
        private XHRRequest sendXhr;

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

            public int CompareTo(IListener other)
            {
                return this.GetId().CompareTo(other.GetId());
            }

            public int GetId()
            {
                return 0;
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

            public int CompareTo(IListener other)
            {
                return this.GetId().CompareTo(other.GetId());
            }

            public int GetId()
            {
                return 0;
            }
        }


        protected override void DoWrite(byte[] data, Action action)
        {
            var opts = new XHRRequest.RequestOptions { Method = "POST", Data = data, CookieHeaderValue = Cookie };
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
                Exception err = args.Length > 0 && args[0] is Exception ? (Exception) args[0] : null;
                pollingXHR.OnError("xhr post error", err);
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

            public int CompareTo(IListener other)
            {
                return this.GetId().CompareTo(other.GetId());
            }

            public int GetId()
            {
                return 0;
            }
        }


        protected override void DoPoll()
        {
            var log = LogManager.GetLogger(Global.CallerName());
            log.Info("xhr DoPoll");
            var opts = new XHRRequest.RequestOptions { CookieHeaderValue = Cookie };

            sendXhr = Request(opts);
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

            public int CompareTo(IListener other)
            {
                return this.GetId().CompareTo(other.GetId());
            }

            public int GetId()
            {
                return 0;
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

            public int CompareTo(IListener other)
            {
                return this.GetId().CompareTo(other.GetId());
            }

            public int GetId()
            {
                return 0;
            }
        }


        public class XHRRequest : Emitter
        {
            private string Method;
            private string Uri;
            private byte[] Data;
            private string CookieHeaderValue;
            private HttpWebRequest Xhr;

            public XHRRequest(RequestOptions options)
            {
                Method = options.Method ?? "GET";
                Uri = options.Uri;
                Data = options.Data;
                CookieHeaderValue = options.CookieHeaderValue;
            }

            public void Create()
            {
                var log = LogManager.GetLogger(Global.CallerName());

                try
                {
                    log.Info(string.Format("xhr open {0}: {1}", Method, Uri));
                    Xhr = (HttpWebRequest) WebRequest.Create(Uri);
                    Xhr.Method = Method;
                    if (CookieHeaderValue != null)
                    {
                        Xhr.Headers.Add("Cookie", CookieHeaderValue);
                        log.Info("added header " + CookieHeaderValue);
                    }
                    else
                    {
                        log.Info("not added header " + CookieHeaderValue);
                    }
                }
                catch (Exception e)
                {
                    log.Error(e);
                    OnError(e);
                    return;
                }


                if (Method == "POST")
                {
                    Xhr.ContentType = "application/octet-stream";
                }

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

                    Task.Run(() =>
                    {
                        var log2 = LogManager.GetLogger(Global.CallerName());
                        log2.Info("Task.Run Create start");
                        using (var res = Xhr.GetResponse())
                        {
                            log.Info("Xhr.GetResponse ");

                            var responseHeaders = new Dictionary<string, string>();
                            for (int i = 0; i < res.Headers.Count; i++)
                            {
                                responseHeaders.Add(res.Headers.Keys[i], res.Headers[i]);
                            }
                            OnResponseHeaders(responseHeaders);

                            var contentType = res.Headers["Content-Type"];



                            using (var resStream = res.GetResponseStream())
                            {
                                Debug.Assert(resStream != null, "resStream != null");
                                if (contentType.Equals("application/octet-stream",
                                    StringComparison.OrdinalIgnoreCase))
                                {
                                    var buffer = new byte[16 * 1024];
                                    using (var ms = new MemoryStream())
                                    {
                                        int read;
                                        while ((read = resStream.Read(buffer, 0, buffer.Length)) > 0)
                                        {
                                            ms.Write(buffer, 0, read);
                                        }
                                        var a = ms.ToArray();
                                        OnData(a);
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
                        log2.Info("Task.Run Create finish");

                    }).Wait();

                }
                catch (System.IO.IOException e)
                {
                    log.Error("Create call failed", e);
                    OnError(e);
                }
                catch (System.Net.WebException e)
                {
                    log.Error("Create call failed", e);
                    OnError(e);
                }
                catch (Exception e)
                {
                    log.Error("Create call failed", e);
                    OnError(e);
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
                log.Info("OnData byte[] =" + System.Text.UTF8Encoding.UTF8.GetString(data));
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
                public string CookieHeaderValue;
            }
        }



    }

}
