using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Quobject.EngineIoClientDotNet.Client;
using Quobject.EngineIoClientDotNet.Client.Transports;
using Xunit;

namespace Quobject.EngineIoClientDotNet_Tests.ClientTests
{
    public class BinaryPollingTest : Connection
    {
        [Fact]
        public void ReceiveBinaryData()
        {
            Socket.SetupLog4Net();

            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
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
                    log.Info(string.Format("EVENT_MESSAGE data ={0} d = {1} " , data,d));

                    if (data == "hi")
                    {
                        return;
                    }
                    events.Enqueue(d);
                });
                //System.Threading.Thread.Sleep(TimeSpan.FromSeconds(3));
                //log.Info("EVENT_OPEN 2");             
                socket.Send(binaryData);
            });
           
            socket.Open();
            //System.Threading.Thread.Sleep(TimeSpan.FromSeconds(3));
            //socket.Send(binaryData);
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(2));


            socket.Close();

            object result;
            events.TryDequeue(out result);
            Assert.Equal(binaryData, result);
            
        }

    }
}
