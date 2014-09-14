using System;
using System.Threading;
using System.Threading.Tasks;
using EngineIoClientDotNet.Modules;
using Quobject.EngineIoClientDotNet.Modules;
using Timer = System.Timers.Timer;

namespace Quobject.EngineIoClientDotNet.Thread
{
    //from http://www.dailycoding.com/Posts/easytimer__javascript_style_settimeout_and_setinterval_in_c.aspx
    public class EasyTimer
    {
        private Timer timer;
        private CancellationTokenSource ts;

        public EasyTimer(Timer timer)
        {
            this.timer = timer;
        }

        public EasyTimer(CancellationTokenSource ts)
        {
            this.ts = ts;
        }


        public static EasyTimer SetInterval(Action method, long delayInMilliseconds)
        {
            var timer = new System.Timers.Timer(delayInMilliseconds);
            timer.Elapsed += (source, e) => method();

            timer.Enabled = true;
            timer.Start();

            // Returns a stop handle which can be used for stopping
            // the timer, if required

            return new EasyTimer( timer);
        }

        //public static EasyTimer SetTimeout(Action method, long delayInMilliseconds)
        //{
        //    var timer = new System.Timers.Timer(delayInMilliseconds);
        //    timer.Elapsed += (source, e) => method();

        //    timer.AutoReset = false;
        //    timer.Enabled = true;
        //    timer.Start();

        //    // Returns a stop handle which can be used for stopping
        //    // the timer, if required
        //    return new EasyTimer(timer);
        //}

        public static EasyTimer SetTimeout(Action method, int delayInMilliseconds)
        {
            var ts = new CancellationTokenSource();
            CancellationToken ct = ts.Token;
            var task = Task.Delay(delayInMilliseconds,ct);
            var awaiter = task.GetAwaiter();

            awaiter.OnCompleted(
                () =>
                {
                    if (!ts.IsCancellationRequested)
                    {
                        method();
                    }
            });
           
            
            // Returns a stop handle which can be used for stopping
            // the timer, if required
            return new EasyTimer(ts);
        }

        internal void Stop()
        {
            var log = LogManager.GetLogger(Global.CallerName());
            log.Info("EasyTimer stop");
            if (ts != null)
            {
                ts.Cancel();                
            }
            else
            {
                this.timer.Stop();                
            }
        }
    }


}
