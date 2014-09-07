//using log4net;

using System.Linq;
using System.Text;
using System.Threading;
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

            private ManualResetEvent allDone = new ManualResetEvent(false);
            
            //http://msdn.microsoft.com/en-us/library/windows/apps/system.net.httpwebrequest.aspx
            private HttpWebRequest Xhr;

            public XHRRequest(RequestOptions options)
            {
                Method = options.Method ?? "GET";
                Uri = options.Uri;
                Data = options.Data;
            }

            public void Create()
            {
                var log = LogManager.GetLogger(Global.CallerName());

                try
                {
                    log.Info(string.Format("xhr open {0}: {1}", Method, Uri));
                    Xhr = (HttpWebRequest) WebRequest.Create(Uri);
                    Xhr.Method = Method;
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
                        // start the asynchronous operation
                        Xhr.BeginGetRequestStream(GetRequestStreamCallback, Xhr);

                        // Keep the main thread from continuing while the asynchronous 
                        // operation completes. A real world application 
                        // could do something useful such as updating its user interface. 
                        allDone.WaitOne();                     
                    }                 
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

            // from http://msdn.microsoft.com/query/dev11.query?appId=Dev11IDEF1&l=EN-US&k=k(System.Net.HttpWebRequest.BeginGetRequestStream);k(BeginGetRequestStream);k(TargetFrameworkMoniker-.NETCore,Version%3Dv4.5);k(DevLang-csharp)&rd=true
            
            private void GetRequestStreamCallback(IAsyncResult asynchronousResult)
            {
                try
                {
                    HttpWebRequest request = (HttpWebRequest)asynchronousResult.AsyncState;

                    // End the operation
                    Stream postStream = request.EndGetRequestStream(asynchronousResult);

                    // Write to the request stream.
                    postStream.Write(Data, 0, Data.Length);
                    postStream.Flush();

                    // Start the asynchronous operation to get the response
                    request.BeginGetResponse(new AsyncCallback(GetResponseCallback), request);
                }
                catch (Exception e)
                {
                    var log = LogManager.GetLogger(Global.CallerName());
                    log.Error("GetRequestStreamCallback", e);
                    OnError(e);
                }
            }

            private void GetResponseCallback(IAsyncResult asynchronousResult)
            {
                try
                {
                    var log = LogManager.GetLogger(Global.CallerName());

                    var request = (HttpWebRequest) asynchronousResult.AsyncState;

                    // End the operation
                    var response = (HttpWebResponse) request.EndGetResponse(asynchronousResult);
                    var streamResponse = response.GetResponseStream();
                    var streamRead = new StreamReader(streamResponse);
                    string responseString = streamRead.ReadToEnd();

                    var responseHeaders = response.Headers.AllKeys.ToDictionary(key => key, key => response.Headers[key]);
                    OnResponseHeaders(responseHeaders);


                    var contentType = response.Headers["Content-Type"];
                    OnData(responseString);


                    // Close the stream object
                    streamResponse.Dispose();
                    streamRead.Dispose();

                    // Release the HttpWebResponse
                    response.Dispose();
                    allDone.Set();
                }
                catch (Exception e)
                {
                    var log = LogManager.GetLogger(Global.CallerName());
                    log.Error("GetResponseCallback", e);
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
                //var log = LogManager.GetLogger(Global.CallerName());
                //log.Info("OnData byte[] =" + System.Text.UTF8Encoding.UTF8.GetString(data));

                //this.Emit(EVENT_DATA, data);
                //this.OnSuccess();
                throw new NotImplementedException();
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
