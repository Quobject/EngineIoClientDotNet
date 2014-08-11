using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quobject.EngineIoClientDotNet.ComponentEmitter;
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
        }

        [Fact]
        public void On()
        {            
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
            Assert.Equal(calls.ToArray(), expected);
        }
    }
}
