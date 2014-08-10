using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quobject.EngineIoClientDotNet.ComponentEmitter
{

    /// <remarks>
    /// The event emitter which is ported from the JavaScript module.
    /// <see href="https://github.com/component/emitter">https://github.com/component/emitter</see>
    /// </remarks>
    public class Emitter
    {
        private ConcurrentDictionary<string, ConcurrentQueue<IListener>>  callbacks = new ConcurrentDictionary<string, ConcurrentQueue<IListener>>();

        private ConcurrentDictionary<IListener, IListener> _onceCallbacks = new ConcurrentDictionary<IListener, IListener>();

        /// <summary>
        ///  Listens on the event.
        /// </summary>
        /// <param name="event1">event name</param>
        /// <param name="fn"></param>
        /// <returns>a reference to this object</returns>
        public Emitter On(string event1, IListener fn)
        {
            ConcurrentQueue<IListener> callbacksLocal = this.callbacks[event1];
            if (callbacksLocal == null)
            {
                callbacksLocal = new ConcurrentQueue<IListener>();
                this.callbacks[event1] = callbacksLocal;
            }
            callbacksLocal.Enqueue(fn);
            return this;
        }


    }

    public interface IListener {
        void Call(params object[] args);
    }
}
