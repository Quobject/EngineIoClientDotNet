
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Quobject.EngineIoClientDotNet.Modules;
using System;



namespace Quobject.EngineIoClientDotNet.Thread
{
    public class EasyTimer
    {


        private CancellationTokenSource ts;


        public EasyTimer(CancellationTokenSource ts)
        {
            this.ts = ts;
        }

        public static EasyTimer SetTimeout(Action method, int delayInMilliseconds)
        {
            var ts = new CancellationTokenSource();
            var ct = ts.Token;

            
            var worker = new BackgroundWorker();

            worker.DoWork += (s, e) => System.Threading.Thread.Sleep(delayInMilliseconds);

            worker.RunWorkerCompleted += (s, e) =>
            {
                if (!ts.IsCancellationRequested)
                {
                    Task.Factory.StartNew(method, ct, TaskCreationOptions.None, TaskScheduler.Default);
                }
            };

            worker.RunWorkerAsync();

          

            // Returns a stop handle which can be used for stopping
            // the timer, if required
            return new EasyTimer(ts);
        }

        public void Stop()
        {
            var log = LogManager.GetLogger(Global.CallerName());
            log.Info("EasyTimer stop");
            if (ts != null)
            {
                ts.Cancel();
            }          
        }

        public static void TaskRun(Action action)
        {
            var t = new Task(action);
            t.RunSynchronously();
            if (t.IsFaulted)
            {
                if (t.Exception != null)
                {
                    throw t.Exception;
                }
                throw new Exception();
            }
            //Task.Run(action).Wait();
        }

        public static Task TaskRunNoWait(Action action)
        {
            var t = new Task(action);
            t.Start();
            return t;
        }

    }


}

