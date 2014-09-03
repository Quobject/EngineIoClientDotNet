//using log4net;

using System.Diagnostics;
using EngineIoClientDotNet.Modules;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Quobject.EngineIoClientDotNet.Client;
using Quobject.EngineIoClientDotNet.Client.Transports;

using System.Collections.Immutable;

using System.IO;
using System;

namespace Quobject.EngineIoClientDotNet_Tests.ClientTests
{
    [TestClass]
    public class BinaryPollingTest : Connection
    {

        [TestMethod]
        public void ReceiveBinaryData()
        {
            LogManager.SetupLogManager();
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");
            var events = new ConcurrentQueue<object>();

            var binaryData = new byte[5];
            for (int i = 0; i < binaryData.Length; i++)
            {
                binaryData[i] = (byte) i;
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
                    socket.Close();
                });
                socket.Send(binaryData);
                //socket.Send("cash money €€€");
            });

            socket.Open();

            log.Info("ReceiveBinaryData end");

            var binaryData2 = new byte[5];
            for (int i = 0; i < binaryData2.Length; i++)
            {
                binaryData2[i] = (byte) (i + 1);
            }

            object result;
            events.TryDequeue(out result);
            Assert.AreEqual(binaryData, result);
        }


        [TestMethod]
        public void ReceiveBinaryDataAndMultibyteUTF8String()
        {
            LogManager.SetupLogManager();
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");
            Trace.WriteLine("eeeee");


            var events = new ConcurrentQueue<object>();

            var binaryData = new byte[5];
            for (int i = 0; i < binaryData.Length; i++)
            {
                binaryData[i] = (byte) i;
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
                        socket.Close();
                    }
                });
                socket.Send(binaryData);
                socket.Send(stringData);
            });

            socket.Open();

            var binaryData2 = new byte[5];
            for (int i = 0; i < binaryData2.Length; i++)
            {
                binaryData2[i] = (byte) (i + 1);
            }

            object result;
            events.TryDequeue(out result);
            Assert.AreEqual(binaryData, result);
            events.TryDequeue(out result);
            Assert.AreEqual(stringData, (string) result);
            socket.Close();

        }


    }
}
