using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection.Emit;
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
        private ConcurrentDictionary<string, List<IListener>> callbacks = new ConcurrentDictionary<string, List<IListener>>();

        private ConcurrentDictionary<IListener, IListener> _onceCallbacks = new ConcurrentDictionary<IListener, IListener>();

        /// <summary>
        /// Executes each of listeners with the given args.
        /// </summary>
        /// <param name="eventString">an event name.</param>
        /// <param name="args"></param>
        /// <returns>a reference to this object.</returns>
        public Emitter Emit(string eventString, params object[] args)
        {
            if (this.callbacks.ContainsKey(eventString))
            {
                List<IListener> callbacksLocal = this.callbacks[eventString];
                callbacksLocal = new List<IListener>(callbacksLocal);
                foreach (var fn in callbacksLocal)
                {
                    fn.Call(args);
                }                    
            }
            return this;            
        }

        /// <summary>
        ///  Listens on the event.
        /// </summary>
        /// <param name="eventString">event name</param>
        /// <param name="fn"></param>
        /// <returns>a reference to this object</returns>
        public Emitter On(string eventString, IListener fn)
        {
            if (!this.callbacks.ContainsKey(eventString))
            {
                this.callbacks[eventString] = new List<IListener>();            
            }
            List<IListener> callbacksLocal = this.callbacks[eventString];
            callbacksLocal.Add(fn);
            return this;
        }

        /// <summary>
        /// Adds a one time listener for the event.
        /// </summary>
        /// <param name="eventString">an event name.</param>
        /// <param name="fn"></param>
        /// <returns>a reference to this object</returns>
        public Emitter Once(string eventString, IListener fn)
        {
            var on = new OnceListener(eventString, fn, this);

            _onceCallbacks.TryAdd(fn, on);
            return this;

        }

        /// <summary>
        /// Removes all registered listeners.
        /// </summary>
        /// <returns>a reference to this object.</returns>
        public Emitter Off()
        {
            this.callbacks.Clear();
            this._onceCallbacks.Clear();
            return this;
        }

        /// <summary>
        /// Removes all listeners of the specified event.
        /// </summary>
        /// <param name="eventString">an event name</param>
        /// <returns>a reference to this object.</returns>
        public Emitter Off(string eventString)
        {

            List<IListener> retrievedValue;
            if (! this.callbacks.TryRemove(eventString, out retrievedValue))
            {
                Console.WriteLine("Emitter.Off Could not remove {0}",eventString);
            }
            if (retrievedValue != null)
            {
                foreach (var listener in retrievedValue)
                {
                    IListener notUsed;
                    this._onceCallbacks.TryRemove(listener, out notUsed);
                }
            }           
            return this;
        }


        /// <summary>
        /// Removes the listener
        /// </summary>
        /// <param name="eventString">an event name</param>
        /// <param name="fn"></param>
        /// <returns>a reference to this object.</returns>
        public Emitter Off(string eventString, IListener fn)
        {
            if (this.callbacks.ContainsKey(eventString))
            {
                List<IListener> callbacksLocal = this.callbacks[eventString];
                IListener offListener;
                this._onceCallbacks.TryRemove(fn, out offListener);
                callbacksLocal.Remove(offListener ?? fn);                
            }
            return this;
        }

        /// <summary>
        ///  Returns a list of listeners for the specified event.
        /// </summary>
        /// <param name="eventString">an event name.</param>
        /// <returns>a reference to this object</returns>
        public List<IListener> Listeners(string eventString)
        {
            if (this.callbacks.ContainsKey(eventString))
            {
                List<IListener> callbacksLocal = this.callbacks[eventString];
                return callbacksLocal != null ? new List<IListener>(callbacksLocal) : new List<IListener>();
            }
            return new List<IListener>();
        }

        /// <summary>
        /// Check if this emitter has listeners for the specified event.
        /// </summary>
        /// <param name="eventString">an event name</param>
        /// <returns>bool</returns>
        public bool HasListeners(string eventString)
        {
            return this.Listeners(eventString).Count > 0;
        }

    }

    public interface IListener {
        void Call(params object[] args);
    }



    public class OnceListener : IListener
    {
        private readonly string _eventString;
        private readonly IListener _fn;
        private readonly Emitter _emitter;

        public OnceListener(string eventString, IListener fn, Emitter emitter)
        {
            this._eventString = eventString;
            this._fn = fn;
            this._emitter = emitter;
        }

        void IListener.Call(params object[] args)
        {
            _emitter.Off(_eventString, this);
            _fn.Call(args);
        }
    }
}
