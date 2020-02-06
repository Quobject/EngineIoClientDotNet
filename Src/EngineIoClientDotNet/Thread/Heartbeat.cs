using System;
using System.ComponentModel;
using System.Threading;

namespace Quobject.EngineIoClientDotNet.Thread
{
    public class Heartbeat
    {
        private volatile bool gotHeartbeat = false;
        private BackgroundWorker heartBeatTimer;
        private CancellationTokenSource ts;

        private Heartbeat()
        {
            ts = new CancellationTokenSource();
        }

        public static Heartbeat Start(Action onTimeout, int timeout)
        {
            Heartbeat heartbeat = new Heartbeat();
            heartbeat.Run(onTimeout, timeout);
            return heartbeat;            
        }

        public void OnHeartbeat()
        {
            gotHeartbeat = true;
        }

        private void Run(Action onTimeout, int timeout)
        {
            heartBeatTimer = new BackgroundWorker();

            heartBeatTimer.DoWork += (s, e) =>
            {
                while (!ts.IsCancellationRequested)
                {
                    System.Threading.Thread.Sleep(timeout);
                    if (!gotHeartbeat && !ts.IsCancellationRequested)
                    {
                        onTimeout();
                        break;
                    }
                }
            };

            heartBeatTimer.RunWorkerAsync();
        }

        public void Stop()
        {
            ts.Cancel();
        }
    }
}