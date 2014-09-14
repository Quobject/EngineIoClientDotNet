//using log4net;

using System;
using System.Collections.Generic;
using System.Threading;
using EngineIoClientDotNet.Modules;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Quobject.EngineIoClientDotNet.Client;
using Quobject.EngineIoClientDotNet.Client.Transports;

using Quobject.Collections.Immutable;
using System.Threading.Tasks;
using Quobject.EngineIoClientDotNet.Modules;


namespace Quobject.EngineIoClientDotNet_Tests.ClientTests
{
    [TestClass]
    public class BinaryWebSocketTest : Connection
    {

        private ManualResetEvent _manualResetEvent = null;

        [TestMethod]
        public void ReceiveBinaryData()
        {
            var log = LogManager.GetLogger(Global.CallerName());
            log.Info("Start");
            _manualResetEvent = new ManualResetEvent(false);

            var events = new Queue<object>();

            var binaryData = new byte[5];
            for (int i = 0; i < binaryData.Length; i++)
            {
                binaryData[i] = (byte) (i + 0);
            }


            var options = CreateOptions();
            var socket = new Socket(options);

            socket.On(Socket.EVENT_OPEN, () =>
            {
                log.Info(Socket.EVENT_OPEN);
            });

            socket.On(Socket.EVENT_UPGRADE, () =>
            {
                log.Info(Socket.EVENT_UPGRADE);
                socket.Send(binaryData);
                //socket.Send("why");
            });

            socket.On(Socket.EVENT_MESSAGE, (d) =>
            {
                var data = d as string;
                log.Info(string.Format("EVENT_MESSAGE data ={0} d = {1} ", data, d));

                if (data == "hi")
                {
                    return;
                }
                events.Enqueue(d);
                _manualResetEvent.Set();
            });

            socket.Open();
            _manualResetEvent.WaitOne();
            socket.Close();

            var binaryData2 = new byte[5];
            for (int i = 0; i < binaryData.Length; i++)
            {
                binaryData2[i] = (byte) (i + 1);
            }

            object result = events.Dequeue();
            CollectionAssert.AreEqual(binaryData, (byte[]) result);

        }


        [TestMethod]
        public void ReceiveBinaryDataAndMultibyteUTF8String()
        {

            var log = LogManager.GetLogger(Global.CallerName());
            log.Info("Start");
            _manualResetEvent = new ManualResetEvent(false);

            var events = new Queue<object>();

            var binaryData = new byte[5];
            for (int i = 0; i < binaryData.Length; i++)
            {
                binaryData[i] = (byte) i;
            }
            const string stringData = "cash money €€€";

            var options = CreateOptions();           
            var socket = new Socket(options);

            socket.On(Socket.EVENT_OPEN, () =>
            {

                log.Info("EVENT_OPEN");

                socket.Send(binaryData);
                socket.Send(stringData);

            });

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
                    _manualResetEvent.Set();
                }
            });

            socket.Open();
            _manualResetEvent.WaitOne();
            socket.Close();	            

            var binaryData2 = new byte[5];
            for (int i = 0; i < binaryData2.Length; i++)
            {
                binaryData2[i] = (byte) (i + 1);
            }

            object result;
            result = events.Dequeue();            
            CollectionAssert.AreEqual(binaryData, (byte[])result);

            result = events.Dequeue();
            Assert.AreEqual(stringData, (string) result);
            log.Info("ReceiveBinaryDataAndMultibyteUTF8String end");
        }


    }
}
