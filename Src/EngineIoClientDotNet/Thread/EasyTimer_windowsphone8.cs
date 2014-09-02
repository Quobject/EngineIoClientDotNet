
using System;
using System.Windows.Threading;

namespace Quobject.EngineIoClientDotNet.Thread
{
    //from http://www.dailycoding.com/Posts/easytimer__javascript_style_settimeout_and_setinterval_in_c.aspx
    public class EasyTimer
    {
        private DispatcherTimer timer;

        public EasyTimer(DispatcherTimer timer)
        {
            this.timer = timer;
        }


        public static EasyTimer SetInterval(Action method, long delayInMilliseconds)
        {
            var timer = new DispatcherTimer();

            timer.Interval = TimeSpan.FromMilliseconds(delayInMilliseconds);
            timer.Tick += (source, e) => method();
            
            timer.Start();

            // Returns a stop handle which can be used for stopping
            // the timer, if required
            return new EasyTimer(timer);
        }

        public static EasyTimer SetTimeout(Action method, long delayInMilliseconds)
        {
            var timer = new DispatcherTimer();

            timer.Interval = TimeSpan.FromMilliseconds(delayInMilliseconds);
            timer.Tick += (source, e) => { timer.Stop(); method();  };
            
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

