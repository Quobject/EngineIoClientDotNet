using System.IO;
using System.Net;
using System.Threading;
using Quobject.EngineIoClientDotNet.ComponentEmitter;
using Quobject.EngineIoClientDotNet.Modules;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            var opts = new XHRRequest.RequestOptions { Method = "POST", Data = data, CookieHeaderValue = Cookie};
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
            log.Info("xhr poll");
            var opts = new XHRRequest.RequestOptions { CookieHeaderValue = Cookie };
            sendXhr = Request(opts);
            sendXhr.On(EVENT_DATA, new DoPollEventDataListener(this));
            sendXhr.On(EVENT_ERROR, new DoPollEventErrorListener(this));
            //sendXhr.Create();
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
            private HttpWebRequest httpWebRequest;
            private ManualResetEvent allDone;
            //private HttpClient httpClient;

            public XHRRequest(RequestOptions options)
            {
                Method = options.Method ?? "GET";
                Uri = options.Uri;
                Data = options.Data;
                CookieHeaderValue = options.CookieHeaderValue;
            }


            //public void Create()
            //{
            //    var log = LogManager.GetLogger(Global.CallerName());

            //    try
            //    {
            //        log.Info(string.Format("xhr open {0}: {1}", Method, Uri));
            //        httpClient = new HttpClient();
            //        httpClient.DefaultRequestHeaders.Add("user-agent",
            //            "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");
            //        httpClient.DefaultRequestHeaders.Add("Cookie",CookieHeaderValue);

            //        httpClient.MaxResponseContentBufferSize = 256000;


            //        HttpResponseMessage response = null;
            //        if (Method == "POST")
            //        {

            //            if (Data == null)
            //            {
            //                return;
            //            }
            //            HttpContent content = new ByteArrayContent(Data);
            //            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            //            var task = httpClient.PostAsync(Uri, content);
            //            task.Wait();
            //            if (task.IsFaulted)
            //            {
            //                throw new Exception(task.Exception.Message);
            //            }
            //            response = task.Result;
            //        }
            //        else if (Method == "GET")
            //        {
            //            var task = httpClient.GetAsync(Uri);
            //            task.Wait();
            //            if (task.IsFaulted)
            //            {
            //                throw new Exception(task.Exception.Message);
            //            }
            //            response = task.Result;
            //        }
            //        if (response == null)
            //        {
            //            log.Info("Response == null");
            //            return;
            //        }
            //        response.EnsureSuccessStatusCode();
            //        log.Info("Xhr.GetResponse ");

            //        var t = response.Headers;

            //        var responseHeaders = new Dictionary<string, string>();
            //        foreach (var h in response.Headers)
            //        {
            //            string value = "";
            //            foreach (var c in h.Value)
            //            {
            //                value += c;
            //            }

            //            responseHeaders.Add(h.Key, value);
            //        }
            //        OnResponseHeaders(responseHeaders);

            //        var contentType = responseHeaders.ContainsKey("Content-Type")
            //            ? responseHeaders["Content-Type"]
            //            : null;

            //        if (contentType != null &&
            //            contentType.Equals("application/octet-stream", StringComparison.OrdinalIgnoreCase))
            //        {
            //            var task = response.Content.ReadAsByteArrayAsync();
            //            task.ConfigureAwait(false);
            //            task.Wait();
            //            var responseBodyAsByteArray = task.Result;
            //            Task.Run(() => OnData(responseBodyAsByteArray)).Wait();
            //            //OnData(responseBodyAsByteArray);
            //        }
            //        else
            //        {
            //            var task = response.Content.ReadAsStringAsync();
            //            task.ConfigureAwait(false);
            //            task.Wait();
            //            var responseBodyAsText = task.Result;
            //            Task.Run(() => OnData(responseBodyAsText)).Wait();
            //            //OnData(responseBodyAsText);
            //        }

            //    }
            //    catch (Exception e)
            //    {
            //        log.Error(e);
            //        OnError(e);
            //        return;
            //    }
            //}




            public void Create()
            {
                var log = LogManager.GetLogger(Global.CallerName());

                try
                {
                    log.Info(string.Format("xhr open {0}: {1}", Method, Uri));

                    //http://msdn.microsoft.com/en-us/library/system.net.httpwebrequest.begingetrequeststream.aspx
                    httpWebRequest = (HttpWebRequest)WebRequest.Create(Uri);
                    //httpWebRequest.Headers["user-agent"] = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)";
                    // cannot do user agent??? see: http://stackoverflow.com/q/26249265/1109316


                    if (!string.IsNullOrEmpty(CookieHeaderValue))
                    {
                        httpWebRequest.Headers["Cookie"] = CookieHeaderValue;
                    }

                    httpWebRequest.Method = Method;

                    allDone = new ManualResetEvent(false);

                    if (Method == "POST")
                    {
                        if (Data == null)
                        {
                            return;
                        }
                        httpWebRequest.ContentType = "application/octet-stream";

                        httpWebRequest.BeginGetRequestStream(GetRequestStreamCallback, this);
                    }
                    else if (Method == "GET")
                    {
                        httpWebRequest.BeginGetResponse(GetResponseCallback, this);
                    }
                    allDone.WaitOne();
                }
                catch (Exception e)
                {
                    log.Error(e);
                    OnError(e);
                }
            }


            private void GetRequestStreamCallback(IAsyncResult asynchronousResult)
            {
                var xHRRequest = (XHRRequest)asynchronousResult.AsyncState;
                var request = xHRRequest.httpWebRequest;
                if (Method == "POST")
                {
                    using (var postStream = request.EndGetRequestStream(asynchronousResult))
                    {
                        postStream.Write(Data, 0, Data.Length);
                        postStream.Flush();
                        postStream.Dispose();
                    }
                }
                request.BeginGetResponse(GetResponseCallback, xHRRequest);
            }

            private void GetResponseCallback(IAsyncResult asynchronousResult)
            {
                var log = LogManager.GetLogger(Global.CallerName());

                var xHRRequest = (XHRRequest)asynchronousResult.AsyncState;
                var request = xHRRequest.httpWebRequest;

                try
                {
                    using (var response = (HttpWebResponse) request.EndGetResponse(asynchronousResult))
                    {
                        log.Info("Xhr.GetResponseCallback ");

                        var responseHeaders = new Dictionary<string, string>();
                        foreach (var key in response.Headers.AllKeys)
                        {
                            string value = response.Headers[key];
                            responseHeaders.Add(key, value);
                        }
                        OnResponseHeaders(responseHeaders);


                        using (var streamResponse = response.GetResponseStream())
                        {
                            var contentType = response.ContentType;
                            if (contentType != null &&
                                contentType.Equals("application/octet-stream", StringComparison.OrdinalIgnoreCase))
                            {
                                using (var memoryStream = new MemoryStream())
                                {
                                    streamResponse.CopyTo(memoryStream);
                                    var responseBodyAsByteArray = memoryStream.ToArray();
                                    Task.Run(() => OnData(responseBodyAsByteArray)).Wait();
                                }
                            }
                            else
                            {
                                using (var streamReader = new StreamReader(streamResponse))
                                {
                                    var responseBodyAsText = streamReader.ReadToEnd();
                                    Task.Run(() => OnData(responseBodyAsText)).Wait();
                                }
                            }
                        }
                    }
                    xHRRequest.allDone.Set();

                }
                catch (Exception e)
                {
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
                public string CookieHeaderValue;
            }
        }



    }

}
