//
// ImmutableStack.cs
//
// Author:
//       Mike Krüger <mkrueger@xamarin.com>
//
// Copyright (c) 2013 Xamarin Inc. (http://xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Quobject.Collections.Immutable
{
    public class ImmutableStack<T> : IImmutableStack<T>
    {
        readonly T head;
        readonly ImmutableStack<T> tail;

        internal ImmutableStack()
        {
        }

        ImmutableStack(T head, ImmutableStack<T> tail)
        {
            this.head = head;
            this.tail = tail;
        }
        #region IImmutableStack implementation
        internal static readonly ImmutableStack<T> Empty = new ImmutableStack<T>();

        public bool IsEmpty
        {
            get { return tail == null; }
        }

        public ImmutableStack<T> Clear()
        {
            return Empty;
        }

        IImmutableStack<T> IImmutableStack<T>.Clear()
        {
            return Empty;
        }

        public T Peek()
        {
            if (IsEmpty)
                throw new InvalidOperationException("Stack is empty.");
            return head;
        }

        public ImmutableStack<T> Pop()
        {
            if (IsEmpty)
                throw new InvalidOperationException("Stack is empty.");
            return tail;
        }

        public ImmutableStack<T> Pop(out T value)
        {
            value = Peek();
            return Pop();
        }

        IImmutableStack<T> IImmutableStack<T>.Pop()
        {
            return Pop();
        }

        public ImmutableStack<T> Push(T value)
        {
            return new ImmutableStack<T>(value, this);
        }

        IImmutableStack<T> IImmutableStack<T>.Push(T value)
        {
            return Push(value);
        }
        #endregion

        #region IEnumerable<T> implementation

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }

        struct Enumerator : IEnumerator<T>
        {
            readonly ImmutableStack<T> start;
            IImmutableStack<T> current;

            public Enumerator(ImmutableStack<T> stack)
            {
                this.start = stack;
                this.current = null;
            }
            #region IEnumerator implementation

            bool IEnumerator.MoveNext()
            {
                if (current == null)
                {
                    current = this.start;
                }
                else if (!current.IsEmpty)
                {
                    current = current.Pop();
                }

                return !current.IsEmpty;
            }

            void IEnumerator.Reset()
            {
                current = null;
            }

            object IEnumerator.Current
            {
                get { return this.Current; }
            }
            #endregion

            #region IDisposable implementation
            void IDisposable.Dispose()
            {
            }
            #endregion

            #region IEnumerator implementation

            public T Current
            {
                get
                {
                    return current != null ? current.Peek() : default(T);
                }
            }
            #endregion
        }
        #endregion

        #region IEnumerable implementation

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
        #endregion
    }

    public static class ImmutableStack
    {
        public static ImmutableStack<T> Create<T>()
        {
            return ImmutableStack<T>.Empty;
        }

        public static ImmutableStack<T> Create<T>(T item)
        {
            return Create<T>().Push(item);
        }

        public static ImmutableStack<T> Create<T>(IEnumerable<T> items)
        {
            var result = ImmutableStack<T>.Empty;
            foreach (var item in items)
                result = result.Push(item);
            return result;
        }

        public static ImmutableStack<T> Create<T>(params T[] items)
        {
            return Create((IEnumerable<T>)items);
        }

        public static IImmutableStack<T> Pop<T>(this IImmutableStack<T> stack, out T value)
        {
            if (stack == null)
                throw new ArgumentNullException("stack");
            value = stack.Peek();
            return stack.Pop();
        }
    }
}
