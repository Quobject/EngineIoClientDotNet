using System;
using System.Timers;

namespace Quobject.EngineIoClientDotNet.Thread
{
    //from http://www.dailycoding.com/Posts/easytimer__javascript_style_settimeout_and_setinterval_in_c.aspx
    public class EasyTimer
    {
        private Timer timer;

        public EasyTimer(Timer timer)
        {
            this.timer = timer;
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

        public static EasyTimer SetTimeout(Action method, long delayInMilliseconds)
        {
            var timer = new System.Timers.Timer(delayInMilliseconds);
            timer.Elapsed += (source, e) => method();

            timer.AutoReset = false;
            timer.Enabled = true;
            timer.Start();

            // Returns a stop handle which can be used for stopping
            // the timer, if required
            return new EasyTimer(timer);
        }

        internal void Stop()
        {
            this.timer.Stop();
        }
    }


}
