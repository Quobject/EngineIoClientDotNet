using Quobject.EngineIoClientDotNet.Client;
using System.Collections.Concurrent;
using System.Threading;
using Xunit;

namespace Quobject.EngineIoClientDotNet_Tests.ClientTests
{
    public class BinaryWebSocketTest : Connection
    {
        private ManualResetEvent _manualResetEvent = null;

        [Fact]
        public void ReceiveBinaryData()
        {
            _manualResetEvent = new ManualResetEvent(false);

            var events = new ConcurrentQueue<object>();

            var binaryData = new byte[5];
            for (int i = 0; i < binaryData.Length; i++)
            {
                binaryData[i] = (byte)(i + 0);
            }

            var options = CreateOptions();

            var socket = new Socket(options);

            socket.On(Socket.EVENT_OPEN, () =>
            {
              //log.Info(Socket.EVENT_OPEN);
            });

            socket.On(Socket.EVENT_UPGRADE, () =>
            {
              //log.Info(Socket.EVENT_UPGRADE);
                socket.Send(binaryData);
            });

            socket.On(Socket.EVENT_MESSAGE, (d) =>
            {
                var data = d as string;
              //log.Info(string.Format("EVENT_MESSAGE data ={0} d = {1} ", data, d));

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
          //log.Info("ReceiveBinaryData end");

            var binaryData2 = new byte[5];
            for (int i = 0; i < binaryData2.Length; i++)
            {
                binaryData2[i] = (byte)(i + 1);
            }

            object result;
            events.TryDequeue(out result);
            Assert.Equal(binaryData, result);
        }

        [Fact]
        public void ReceiveBinaryDataAndMultibyteUTF8String()
        {
            _manualResetEvent = new ManualResetEvent(false);

            var events = new ConcurrentQueue<object>();

            var binaryData = new byte[5];
            for (int i = 0; i < binaryData.Length; i++)
            {
                binaryData[i] = (byte)i;
            }
            const string stringData = "Ä ä Ü ü ß";

            var options = CreateOptions();

            var socket = new Socket(options);

            socket.On(Socket.EVENT_OPEN, () =>
            {
            });

            socket.On(Socket.EVENT_UPGRADE, () =>
            {
              //log.Info(Socket.EVENT_UPGRADE);
                socket.Send(binaryData);
                socket.Send(stringData);
            });

            socket.On(Socket.EVENT_MESSAGE, (d) =>
            {
                var data = d as string;
              //log.Info(string.Format("EVENT_MESSAGE data ={0} d = {1} ", data, d));

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
                binaryData2[i] = (byte)(i + 1);
            }

            object result;
            events.TryDequeue(out result);
            Assert.Equal(binaryData, result);
            events.TryDequeue(out result);
            Assert.Equal(stringData, (string)result);
          //log.Info("ReceiveBinaryDataAndMultibyteUTF8String end");
        }
    }
}