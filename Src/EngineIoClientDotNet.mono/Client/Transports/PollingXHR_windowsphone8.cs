//using log4net;

using Quobject.EngineIoClientDotNet.ComponentEmitter;
using Quobject.EngineIoClientDotNet.Modules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Quobject.EngineIoClientDotNet.Client.Transports
{
    public class PollingXHR : Polling
    {
        private XHRRequest sendXhr;
        public static ManualResetEvent allDone = new ManualResetEvent(false);
        const int BUFFER_SIZE = 1024;

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

        private class EventRequestHeadersListener : IListener
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

        private class EventResponseHeadersListener : IListener
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
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
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
            sendXhr.Create().Wait();
        }

        private class SendEventErrorListener : IListener
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

        private class SendEventSuccessListener : IListener
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
            sendXhr.Create().Wait();
        }

        private class DoPollEventDataListener : IListener
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
                    pollingXHR.OnData((string) arg);
                }
                else if (arg is byte[])
                {
                    pollingXHR.OnData((byte[]) arg);
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

        private class DoPollEventErrorListener : IListener
        {
            private PollingXHR pollingXHR;

            public DoPollEventErrorListener(PollingXHR pollingXHR)
            {
                this.pollingXHR = pollingXHR;
            }

            public void Call(params object[] args)
            {
                Exception err = args.Length > 0 && args[0] is Exception ? (Exception) args[0] : null;
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


        //http://msdn.microsoft.com/en-us/library/windows/apps/system.net.httpwebrequest.begingetresponse(v=vs.105).aspx
        public class RequestState
        {
            // This class stores the State of the request.
            const int BUFFER_SIZE = 1024;
            public StringBuilder requestData;
            public byte[] BufferRead;

            public HttpWebRequest request;
            public HttpWebResponse response;
            public Stream streamResponse;
            public Exception error_exception { get; set; }

            public RequestState()
            {
                BufferRead = new byte[BUFFER_SIZE];
                requestData = new StringBuilder("");
                request = null;
                streamResponse = null;
            }
        }


        public class XHRRequest : Emitter
        {
            private string Method;
            private string Uri;
            private byte[] Data;
            private HttpWebRequest myHttpWebRequest1;
            private string CookieHeaderValue;

            public XHRRequest(RequestOptions options)
            {
                Method = options.Method ?? "GET";
                Uri = options.Uri;
                Data = options.Data;
                CookieHeaderValue = options.CookieHeaderValue;

            }

            public async Task Create()
            {
                var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());

                try
                {
                    log.Info(string.Format("xhr open {0}: {1}", Method, Uri));
                    myHttpWebRequest1 = (HttpWebRequest)WebRequest.Create(Uri);
                    myHttpWebRequest1.Method = Method;
                    if (!string.IsNullOrEmpty(CookieHeaderValue))
                    {
                        myHttpWebRequest1.Headers["Cookie"] = CookieHeaderValue;
                    }

                    if (Method == "POST")
                    {
                        myHttpWebRequest1.ContentType = "application/octet-stream";
                    }                 
                }
                catch (Exception e)
                {
                    log.Error(e);
                    OnError(e);
                    return;
                }



                try
                {
                    if (Data != null)
                    {
                        myHttpWebRequest1.ContentLength = Data.Length;
                        //http://stackoverflow.com/questions/14344029/system-net-httpwebrequest-does-not-contain-a-definition-for-getrequeststream
                        using (
                            var requestStream =
                                await
                                    Task.Factory.FromAsync<Stream>(myHttpWebRequest1.BeginGetRequestStream, myHttpWebRequest1.EndGetRequestStream,
                                        null))
                        {
                            requestStream.Write(Data, 0, Data.Length);
                        }


                    }

                    //myHttpWebRequest1.BeginGetResponse(new AsyncCallback(FinishWebRequest), null);

                    // Create an instance of the RequestState and assign the previous myHttpWebRequest1
                    // object to it's request field.  
                    var myRequestState = new RequestState();
                    myRequestState.request = myHttpWebRequest1;

                    // Start the asynchronous request.
                    IAsyncResult result =
                    (IAsyncResult)myHttpWebRequest1.BeginGetResponse(new AsyncCallback(RespCallback), myRequestState);

                    allDone.WaitOne();
                    log.Info("after allDone.WaitOne()");
                    // Release the HttpWebResponse resource.
                    if (myRequestState.response != null)
                    {
                        myRequestState.response.Close();
                    }
                    else
                    {
                        log.Info("myRequestState.response != null");
                    }
                    if (myRequestState.error_exception != null)
                    {
                        OnError(myRequestState.error_exception);
                    }
                }
                catch (System.IO.IOException e)
                {
                    log.Error("IOException Create call failed", e);
                    OnError(e);
                }
                catch (System.Net.WebException e)
                {
                    log.Error("WebException Create call failed", e);
                    OnError(e);
                }
                catch (Exception e)
                {
                    log.Error("Create call failed", e);
                    OnError(e);
                }

            }

            private void RespCallback(IAsyncResult asynchronousResult)
            {
                var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
                log.Info("start");
                var myRequestState = (RequestState)asynchronousResult.AsyncState;
                try
                {
                    // State of request is asynchronous.

                    HttpWebRequest myHttpWebRequest2 = myRequestState.request;
                    myRequestState.response = (HttpWebResponse) myHttpWebRequest2.EndGetResponse(asynchronousResult);

                    // Read the response into a Stream object.
                    Stream responseStream = myRequestState.response.GetResponseStream();
                    myRequestState.streamResponse = responseStream;

                    // Begin the Reading of the contents of the HTML page and print it to the console.
                    IAsyncResult asynchronousInputRead = responseStream.BeginRead(myRequestState.BufferRead, 0,
                        BUFFER_SIZE, new AsyncCallback(ReadCallBack), myRequestState);
                }
                catch (WebException e)
                {
                    log.Error("", e);
                    myRequestState.error_exception = e;
                    allDone.Set();
                }
            }

            private void ReadCallBack(IAsyncResult asyncResult)
            {
                var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
                log.Info("start");
                var myRequestState = (RequestState)asyncResult.AsyncState;
                try
                {
                    Stream responseStream = myRequestState.streamResponse;
                    int read = responseStream.EndRead(asyncResult);

                    // Read the HTML page and then do something with it
                    if (read > 0)
                    {
                        myRequestState.requestData.Append(Encoding.UTF8.GetString(myRequestState.BufferRead, 0, read));
                        IAsyncResult asynchronousResult = responseStream.BeginRead(myRequestState.BufferRead, 0,
                            BUFFER_SIZE, new AsyncCallback(ReadCallBack), myRequestState);
                    }
                    else
                    {
                        if (myRequestState.requestData.Length > 1)
                        {
                            string stringContent;
                            stringContent = myRequestState.requestData.ToString();
                            OnData(stringContent);
                        }

                        responseStream.Close();
                    }
                }
                catch (Exception e)
                {
                    log.Error(e);
                    myRequestState.error_exception = e;
                }
                finally
                {
                    allDone.Set();                    
                }
            }


            private void OnSuccess()
            {
                this.Emit(EVENT_SUCCESS);
            }

            private void OnData(string data)
            {
                var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
                log.Info("OnData string = " + data);
                this.Emit(EVENT_DATA, data);
                this.OnSuccess();
            }

            private void OnData(byte[] data)
            {
                var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
                log.Info("OnData byte[] =" + Parser.Packet.ByteArrayToString(data));
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
