using System;
using System.ComponentModel;
using System.Threading;

namespace Quobject.EngineIoClientDotNet.Thread
{
    public class TriggeredLoopTimer
    {
        private ManualResetEvent trigger;
        private CancellationTokenSource ts;

        private TriggeredLoopTimer()
        {
            trigger = new ManualResetEvent(false);
            ts = new CancellationTokenSource();
        }

        public static TriggeredLoopTimer Start (Action method, int delayInMilliseconds)
        {
            TriggeredLoopTimer ping = new TriggeredLoopTimer();
            ping.Run (method, delayInMilliseconds);
            return ping;
        }


        public void Trigger()
        {
            trigger.Set();
        }

        private void Run (Action method, int delayInMilliseconds)
        {
            var worker = new BackgroundWorker();

            worker.DoWork += (s, e) =>
            {
                while (!ts.IsCancellationRequested)
                {
                    System.Threading.Thread.Sleep (delayInMilliseconds);
                    if (!ts.IsCancellationRequested)
                    {
                        method();
                        trigger.WaitOne();
                        trigger.Reset();
                    }
                }
            };

            worker.RunWorkerAsync();
        }

        public void Stop()
        {
            if (ts != null)
            {
                ts.Cancel();
                trigger.Set();
            }
        }
    }
}