
using System;
using System.Windows;
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


        //public static EasyTimer SetInterval(Action method, long delayInMilliseconds)
        //{
        //    var timer = new DispatcherTimer();

        //    timer.Interval = TimeSpan.FromMilliseconds(delayInMilliseconds);
        //    timer.Tick += (source, e) => method();
            
        //    timer.Start();

        //    // Returns a stop handle which can be used for stopping
        //    // the timer, if required
        //    return new EasyTimer(timer);
        //}

        public static EasyTimer SetTimeout(Action method, long delayInMilliseconds)
        {
            var dispatcher = Deployment.Current.Dispatcher;
            EasyTimer result = null;
            dispatcher.BeginInvoke(() =>
            {
                var timer1 = new DispatcherTimer();

                timer1.Interval = TimeSpan.FromMilliseconds(delayInMilliseconds);
                timer1.Tick += (source, e) =>
                {
                    timer1.Stop();                   
                    dispatcher.BeginInvoke(method);
                };

                timer1.Start();
                result = new EasyTimer(timer1);
            });
            return result;
        }

        internal void Stop()
        {
            this.timer.Stop();
        }
    }


}

