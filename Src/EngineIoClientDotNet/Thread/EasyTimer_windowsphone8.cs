using System;
using System.Timers;

namespace Quobject.EngineIoClientDotNet.Thread
{
    //from http://www.dailycoding.com/Posts/easytimer__javascript_style_settimeout_and_setinterval_in_c.aspx
    public static class EasyTimer
    {
        public static Timer SetInterval(Action method, long delayInMilliseconds)
        {
            var timer = new Timer(delayInMilliseconds);
            timer.Elapsed += (source, e) => method();

            timer.Enabled = true;
            timer.Start();

            // Returns a stop handle which can be used for stopping
            // the timer, if required
            return timer;
        }

        public static Timer SetTimeout(Action method, long delayInMilliseconds)
        {
            var timer = new System.Timers.Timer(delayInMilliseconds);
            timer.Elapsed += (source, e) => method();

            timer.AutoReset = false;
            timer.Enabled = true;
            timer.Start();

            // Returns a stop handle which can be used for stopping
            // the timer, if required
            return timer;
        }
    }
}
