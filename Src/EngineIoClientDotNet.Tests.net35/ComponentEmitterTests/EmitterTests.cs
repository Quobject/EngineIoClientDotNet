using Quobject.EngineIoClientDotNet.ComponentEmitter;
using Quobject.EngineIoClientDotNet.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Quobject.EngineIoClientDotNet_Tests.ComponentEmitterTests
{
    public class EmitterTests
    {
        public class TestListener1 : IListener
        {
            private readonly List<object> _calls;

            public TestListener1(List<object> calls)
            {
                this._calls = calls;
            }

            public void Call(params object[] args)
            {
                _calls.Add("one");
                _calls.Add(args[0]);
            }

            public int CompareTo(IListener other)
            {
                return this.GetId().CompareTo(other.GetId());
            }

            public int GetId()
            {
                return 0;
            }
        }

        public class TestListener2 : IListener
        {
            private readonly List<object> _calls;

            public TestListener2(List<object> calls)
            {
                this._calls = calls;
            }

            public void Call(params object[] args)
            {
                _calls.Add("two");
                _calls.Add(args[0]);
            }

            public int CompareTo(IListener other)
            {
                return this.GetId().CompareTo(other.GetId());
            }

            public int GetId()
            {
                return 0;
            }

        }


        [Fact]
        public void On()
        {
            LogManager.SetupLogManager();
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");

            var emitter = new Emitter();
            var calls = new List<object>();

            var listener1 = new TestListener1(calls);
            emitter.On("foo", listener1);

            var listener2 = new TestListener2(calls);
            emitter.On("foo", listener2);

            emitter.Emit("foo", 1);
            emitter.Emit("bar", 1);
            emitter.Emit("foo", 2);

            var expected = new Object[] {"one", 1, "two", 1, "one", 2, "two", 2};
            Assert.Equal(expected, calls.ToArray());
        }

        [Fact]
        public void Once()
        {
            LogManager.SetupLogManager();
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");

            var emitter = new Emitter();
            var calls = new List<object>();

            var listener1 = new TestListener1(calls);
            emitter.Once("foo", listener1);

            emitter.Emit("foo", 1);
            emitter.Emit("foo", 2);
            emitter.Emit("foo", 3);
            emitter.Emit("bar", 1);

            var expected = new Object[] {"one", 1};
            Assert.Equal(expected, calls.ToArray());
        }


        public class TestListener3 : IListener
        {
            private readonly List<object> _calls;

            public TestListener3(List<object> calls)
            {
                this._calls = calls;
            }

            public void Call(params object[] args)
            {
                _calls.Add("one");
            }

            public int CompareTo(IListener other)
            {
                return this.GetId().CompareTo(other.GetId());
            }

            public int GetId()
            {
                return 0;
            }

        }

        public class TestListener4 : IListener
        {
            private readonly List<object> _calls;

            public TestListener4(List<object> calls)
            {
                this._calls = calls;
            }

            public void Call(params object[] args)
            {
                _calls.Add("two");
            }

            public int CompareTo(IListener other)
            {
                return this.GetId().CompareTo(other.GetId());
            }

            public int GetId()
            {
                return 0;
            }

        }

        [Fact]
        public void Off()
        {
            LogManager.SetupLogManager();
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");

            var emitter = new Emitter();
            var calls = new List<object>();

            var listener3 = new TestListener3(calls);
            emitter.On("foo", listener3);

            var listener4 = new TestListener4(calls);
            emitter.On("foo", listener4);
            emitter.Off("foo", listener4);

            emitter.Emit("foo");

            var expected = new Object[] {"one"};
            Assert.Equal(expected, calls.ToArray());
        }

        [Fact]
        public void OffWithOnce()
        {
            LogManager.SetupLogManager();
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");

            var emitter = new Emitter();
            var calls = new List<object>();

            var listener3 = new TestListener3(calls);

            emitter.Once("foo", listener3);
            emitter.Off("foo", listener3);

            emitter.Emit("foo");

            var expected = new Object[] {};
            Assert.Equal(expected, calls.ToArray());
        }


        public class TestListener5 : IListener
        {
            private readonly List<bool> _called;

            public TestListener5(List<bool> called)
            {
                this._called = called;
            }

            public void Call(params object[] args)
            {
                this._called[0] = true;
            }

            public int CompareTo(IListener other)
            {
                return this.GetId().CompareTo(other.GetId());
            }

            public int GetId()
            {
                return 0;
            }

        }

        public class TestListener6 : IListener
        {
            private readonly Emitter _emitter;
            private readonly IListener _bListener;

            public TestListener6(Emitter emitter, IListener bListener)
            {
                this._emitter = emitter;
                this._bListener = bListener;
            }

            public void Call(params object[] args)
            {
                _emitter.Off("tobi", _bListener);
            }

            public int CompareTo(IListener other)
            {
                return this.GetId().CompareTo(other.GetId());
            }

            public int GetId()
            {
                return 0;
            }

        }

        [Fact]
        public void OffWhenCalledfromEvent()
        {
            LogManager.SetupLogManager();
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");

            var emitter = new Emitter();
            var called = new List<bool>() {false};


            var listener5 = new TestListener5(called);
            var listener6 = new TestListener6(emitter, listener5);
            emitter.On("tobi", listener6);

            emitter.Once("tobi", listener5);
            emitter.Emit("tobi");
            Assert.True(called[0]);
            called[0] = false;
            emitter.Emit("tobi");
            Assert.False(called[0]);
        }

        [Fact]
        public void OffEvent()
        {
            LogManager.SetupLogManager();
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");

            var emitter = new Emitter();
            var calls = new List<object>();

            var listener3 = new TestListener3(calls);
            emitter.On("foo", listener3);

            var listener4 = new TestListener4(calls);

            emitter.On("foo", listener3);
            emitter.On("foo", listener4);
            emitter.Off("foo");

            emitter.Emit("foo");
            emitter.Emit("foo");

            var expected = new Object[] {};
            Assert.Equal(expected, calls.ToArray());
        }


        [Fact]
        public void OffAll()
        {
            LogManager.SetupLogManager();
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");

            var emitter = new Emitter();
            var calls = new List<object>();

            var listener3 = new TestListener3(calls);

            var listener4 = new TestListener4(calls);



            emitter.On("foo", listener3);
            emitter.On("bar", listener4);

            emitter.Emit("foo");
            emitter.Emit("bar");

            emitter.Off();

            emitter.Emit("foo");
            emitter.Emit("bar");


            var expected = new Object[] {"one", "two"};
            Assert.Equal(expected, calls.ToArray());
        }

        [Fact]
        public void Listeners()
        {
            LogManager.SetupLogManager();
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");

            var emitter = new Emitter();
            var calls = new List<object>();

            var listener3 = new TestListener3(calls);
            emitter.On("foo", listener3);
            var expected = new IListener[] {listener3};
            Assert.Equal(expected, emitter.Listeners("foo").ToArray());
        }

        [Fact]
        public void ListenersWithoutHandlers()
        {
            LogManager.SetupLogManager();
            var emitter = new Emitter();
            var expected = new IListener[] {};
            Assert.Equal(expected, emitter.Listeners("foo").ToArray());
        }

        [Fact]
        public void HasListeners()
        {
            LogManager.SetupLogManager();
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");

            var emitter = new Emitter();
            var calls = new List<object>();
            Assert.False(emitter.HasListeners("foo"));

            var listener3 = new TestListener3(calls);
            emitter.On("foo", listener3);
            Assert.True(emitter.HasListeners("foo"));
        }

        [Fact]
        public void HasListenersWithoutHandlers()
        {
            LogManager.SetupLogManager();
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod());
            log.Info("Start");

            var emitter = new Emitter();
            Assert.False(emitter.HasListeners("foo"));
        }

    }
}
