//using log4net;

using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Quobject.Collections.Immutable;
using Quobject.EngineIoClientDotNet.Client;
using Quobject.EngineIoClientDotNet.Client.Transports;



using System.IO;
using System;
using Quobject.EngineIoClientDotNet.Modules;

namespace Quobject.EngineIoClientDotNet_Tests.ClientTests
{
    [TestClass]
    public class BinaryPollingTest : Connection
    {

        AutoResetEvent _autoResetEvent;

        [TestMethod]
        public void ReceiveBinaryData()
        {
            LogManager.SetupLogManager();
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");
            this._autoResetEvent = new AutoResetEvent(false);

            var events = new Queue<object>();

            var binaryData = new byte[5];
            for (int i = 0; i < binaryData.Length; i++)
            {
                binaryData[i] = (byte)i;
            }


            var options = CreateOptions();
            options.Transports = ImmutableList.Create<string>(Polling.NAME);


            var socket = new Socket(options);

            socket.On(Socket.EVENT_OPEN, () =>
            {

                log.Info("EVENT_OPEN");
                socket.On(Socket.EVENT_MESSAGE, (d) =>
                {

                    var data = d as string;
                    log.Info(string.Format("EVENT_MESSAGE data ={0} d = {1} ", data, d));

                    if (data == "hi")
                    {
                        return;
                    }
                    events.Enqueue(d);
                    this._autoResetEvent.Set(); 
                });
                socket.Send(binaryData);
                //socket.Send("cash money €€€");
            });

            socket.Open();
            this._autoResetEvent.WaitOne();
            socket.Close();

            log.Info("ReceiveBinaryData end");

            var binaryData2 = new byte[5];
            for (int i = 0; i < binaryData2.Length; i++)
            {
                binaryData2[i] = (byte)(i + 1);
            }

            object result;
            result = events.Dequeue();
            CollectionAssert.AreEqual(binaryData, (byte[])result);
        }


        [TestMethod]
        public void ReceiveBinaryDataAndMultibyteUTF8String()
        {
            LogManager.SetupLogManager();
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");
            this._autoResetEvent = new AutoResetEvent(false);

            var events = new Queue<object>();

            var binaryData = new byte[5];
            for (int i = 0; i < binaryData.Length; i++)
            {
                binaryData[i] = (byte)i;
            }
            const string stringData = "cash money €€€";

            var options = CreateOptions();
            options.Transports = ImmutableList.Create<string>(Polling.NAME);


            var socket = new Socket(options);

            socket.On(Socket.EVENT_OPEN, () =>
            {

                log.Info("EVENT_OPEN");
                socket.On(Socket.EVENT_MESSAGE, (d) =>
                {

                    var data = d as string;
                    log.Info(string.Format("EVENT_MESSAGE data ={0} d = {1} ", data, d));

                    if (data == "hi")
                    {
                        return;
                    }
                    events.Enqueue(d);
                    if (events.Count > 1)
                    {
                        this._autoResetEvent.Set(); 
                    }
                });
                socket.Send(binaryData);
                socket.Send(stringData);
            });

            socket.Open();
            this._autoResetEvent.WaitOne();
            socket.Close();


            var binaryData2 = new byte[5];
            for (int i = 0; i < binaryData2.Length; i++)
            {
                binaryData2[i] = (byte)(i + 1);
            }

            object result;
            result = events.Dequeue();
            CollectionAssert.AreEqual(binaryData, (byte[])result);
            result = events.Dequeue();
            Assert.AreEqual(stringData, (string)result);
            socket.Close();

        }


    }
}