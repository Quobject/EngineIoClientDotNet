using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Immutable;

namespace Quobject.EngineIoClientDotNet.ComponentEmitter
{

    /// <remarks>
    /// The event emitter which is ported from the JavaScript module.
    /// <see href="https://github.com/component/emitter">https://github.com/component/emitter</see>
    /// </remarks>
    public class Emitter
    {
        private ConcurrentDictionary<string, ImmutableList<IListener>> callbacks = new ConcurrentDictionary<string, ImmutableList<IListener>>();

        private ConcurrentDictionary<IListener, IListener> _onceCallbacks = new ConcurrentDictionary<IListener, IListener>();

        /// <summary>
        /// Executes each of listeners with the given args.
        /// </summary>
        /// <param name="eventString">an event name.</param>
        /// <param name="args"></param>
        /// <returns>a reference to this object.</returns>
        public Emitter Emit(string eventString, params object[] args)
        {
            //Debug.WriteLine("evenstring: " + eventString, "Emmiter Emit fine");
            if (this.callbacks.ContainsKey(eventString))
            {
                ImmutableList<IListener> callbacksLocal = this.callbacks[eventString];                
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
                this.callbacks[eventString] = ImmutableList<IListener>.Empty;            
            }
            ImmutableList<IListener> callbacksLocal = this.callbacks[eventString];
            callbacksLocal = callbacksLocal.Add(fn);
            this.callbacks[eventString] = callbacksLocal;
            return this;
        }

        public Emitter On(string eventString, Action<string> fn)
        {
            var listener = new ListenerImpl(fn);
            return this.On(eventString, listener);
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
            this.On(eventString, on);
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

            ImmutableList<IListener> retrievedValue;
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
                ImmutableList<IListener> callbacksLocal = this.callbacks[eventString];
                IListener offListener;
                this._onceCallbacks.TryRemove(fn, out offListener);
                this.callbacks[eventString] = callbacksLocal.Remove(offListener ?? fn);                
            }
            return this;
        }

        /// <summary>
        ///  Returns a list of listeners for the specified event.
        /// </summary>
        /// <param name="eventString">an event name.</param>
        /// <returns>a reference to this object</returns>
        public ImmutableList<IListener> Listeners(string eventString)
        {
            if (this.callbacks.ContainsKey(eventString))
            {
                ImmutableList<IListener> callbacksLocal = this.callbacks[eventString];
                return callbacksLocal ?? ImmutableList<IListener>.Empty;
            }
            return ImmutableList<IListener>.Empty;
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

    public class ListenerImpl : IListener
    {
        private Action<string> fn;

        public ListenerImpl(Action<string> fn)
        {
           
            this.fn = fn;
        }

        public void Call(params object[] args)
        {
            fn((string)args[0]);
        }
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
