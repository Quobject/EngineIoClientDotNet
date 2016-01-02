

using System.Collections.Concurrent;
using System.Collections.Generic;
using Quobject.EngineIoClientDotNet.Modules;
using System;

namespace Quobject.EngineIoClientDotNet.ComponentEmitter
{

    /// <remarks>
    /// The event emitter which is ported from the JavaScript module.
    /// <see href="https://github.com/component/emitter">https://github.com/component/emitter</see>
    /// </remarks>
    public class Emitter
    {
        private ConcurrentDictionary<string, List<IListener>> callbacks;

        private ConcurrentDictionary<IListener, IListener> _onceCallbacks; 


        public Emitter()
        {
            this.Off();
        }

        /// <summary>
        /// Executes each of listeners with the given args.
        /// </summary>
        /// <param name="eventString">an event name.</param>
        /// <param name="args"></param>
        /// <returns>a reference to this object.</returns>
        public virtual Emitter Emit(string eventString, params object[] args) 
        {
            //var log = LogManager.GetLogger(Global.CallerName());
            //log.Info("Emitter emit event = " + eventString);
            if (this.callbacks.ContainsKey(eventString))
            {
                List<IListener> callbacksLocal = this.callbacks[eventString];
                var listCopy = new List<IListener>();
                listCopy.AddRange(callbacksLocal);
                foreach (var fn in listCopy)
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
                //this.callbacks[eventString] = List<IListener>();
                this.callbacks.TryAdd(eventString,new List<IListener>());
            }
            List<IListener> callbacksLocal = this.callbacks[eventString];
            callbacksLocal.Add(fn);
            //this.callbacks[eventString] = callbacksLocal;
            List<IListener> outref;
            this.callbacks.TryRemove(eventString, out outref); 
            this.callbacks.TryAdd(eventString, callbacksLocal);
  
            return this;
        }

        /// <summary>
        ///  Listens on the event.
        /// </summary>
        /// <param name="eventString">event name</param>
        /// <param name="fn"></param>
        /// <returns>a reference to this object</returns>
        public Emitter On(string eventString, Action fn)
        {
            var listener = new ListenerImpl(fn);
            return this.On(eventString, listener);
        }

        /// <summary>
        ///  Listens on the event.
        /// </summary>
        /// <param name="eventString">event name</param>
        /// <param name="fn"></param>
        /// <returns>a reference to this object</returns>
        public Emitter On(string eventString, Action<object> fn)
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
        /// Adds a one time listener for the event.
        /// </summary>
        /// <param name="eventString">an event name.</param>
        /// <param name="fn"></param>
        /// <returns>a reference to this object</returns>
        public Emitter Once(string eventString, Action fn)
        {
            var listener = new ListenerImpl(fn);
            return this.Once(eventString, listener);
        }

        /// <summary>
        /// Removes all registered listeners.
        /// </summary>
        /// <returns>a reference to this object.</returns>
        public Emitter Off()
        {
            callbacks = new ConcurrentDictionary<string, List<IListener>>();
            _onceCallbacks = new ConcurrentDictionary<IListener, IListener>();
            return this;
        }

        /// <summary>
        /// Removes all listeners of the specified event.
        /// </summary>
        /// <param name="eventString">an event name</param>
        /// <returns>a reference to this object.</returns>
        public Emitter Off(string eventString)
        {
            try
            {
                List<IListener> retrievedValue;
                if (!callbacks.TryGetValue(eventString, out retrievedValue))
                {
                    var log = LogManager.GetLogger(Global.CallerName());
                    log.Info(string.Format("Emitter.Off Could not remove {0}", eventString));
                }

                if (retrievedValue != null)
                {
                    List<IListener> outref;
                    IListener ilistenerOutRef;
                    callbacks.TryRemove(eventString, out outref);

                    foreach (var listener in retrievedValue)
                    {
                        _onceCallbacks.TryRemove(listener, out ilistenerOutRef);
                    }
                }
            }
            catch (Exception)
            {
                this.Off();
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
            try
            {
                if (this.callbacks.ContainsKey(eventString))
                {
                    List<IListener> callbacksLocal = this.callbacks[eventString];
                    IListener offListener;
                    //_onceCallbacks.TryGetValue(fn,out offListener);

                    _onceCallbacks.TryRemove(fn, out offListener);


                    if (callbacksLocal.Count > 0 && callbacksLocal.Contains(offListener ?? fn))
                    {
                        callbacksLocal.Remove(offListener ?? fn);
                        List<IListener> outref;
                        this.callbacks.TryRemove(eventString, out outref);
                        this.callbacks.TryAdd(eventString, callbacksLocal);
                    }
                }
            }
            catch (Exception)
            {
                this.Off();
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
                return callbacksLocal ?? new List<IListener>();
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

    public interface IListener: System.IComparable<IListener>
    {
        int GetId();
        void Call(params object[] args);
    }

    public class ListenerImpl : IListener
    {
        private static int id_counter = 0;
        private int Id;
        private readonly Action fn1; 
        private readonly Action<object> fn;

        public ListenerImpl(Action<object> fn)
        {

            this.fn = fn;
            this.Id = id_counter++;
        }

        public ListenerImpl(Action fn)
        {

            this.fn1 = fn;
            this.Id = id_counter++;
        }

        public void Call(params object[] args)
        {
            if (fn != null)
            {
                var arg = args.Length > 0 ? args[0] : null;
                fn(arg);
            }
            else
            {
                fn1();
            }
        }

        

        public int CompareTo(IListener other)
        {
            return this.GetId().CompareTo(other.GetId());
        }

        public int GetId()
        {
            return Id;
        }
    }

    public class OnceListener : IListener
    {
        private static int id_counter = 0;
        private int Id;
        private readonly string _eventString;
        private readonly IListener _fn;
        private readonly Emitter _emitter;

        public OnceListener(string eventString, IListener fn, Emitter emitter)
        {
            this._eventString = eventString;
            this._fn = fn;
            this._emitter = emitter;
            Id = id_counter++;
        }

        void IListener.Call(params object[] args)
        {
            _emitter.Off(_eventString, this);
            _fn.Call(args);
        }

        public int CompareTo(IListener other)
        {
            return this.GetId().CompareTo(other.GetId());
        }

        public int GetId()
        {
            return Id;
        }
    }
}
