//
// ImmutableList.cs
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
using System.Diagnostics;
using System.Linq;

namespace Quobject.Collections.Immutable
{
    public class ImmutableList<T> : IImmutableList<T>, IList<T>, IList
    {
        public static readonly ImmutableList<T> Empty = new ImmutableList<T>();
        readonly AvlNode<T> root = AvlNode<T>.Empty;
        readonly IEqualityComparer<T> valueComparer;

        internal ImmutableList()
        {
            this.valueComparer = EqualityComparer<T>.Default;
        }

        internal ImmutableList(AvlNode<T> root, IEqualityComparer<T> equalityComparer)
        {
            this.root = root;
            this.valueComparer = equalityComparer;
        }

        public void CopyTo(int index, T[] array, int arrayIndex, int count)
        {
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException("index");
            if (array == null)
                throw new ArgumentNullException("array");
            if (arrayIndex < 0 || arrayIndex + count > array.Length)
                throw new ArgumentOutOfRangeException("arrayIndex");
            if (count < 0 || index + count > Count)
                throw new ArgumentOutOfRangeException("count");
            foreach (var item in root.Enumerate (index, count, false))
            {
                array[arrayIndex++] = item;
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            if (arrayIndex < 0 || arrayIndex + Count > array.Length)
                throw new ArgumentOutOfRangeException("arrayIndex");
            CopyTo(0, array, 0, Count);
        }

        public void CopyTo(T[] array)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            CopyTo(array, 0);
        }

        public bool Exists(Predicate<T> match)
        {
            if (match == null)
                throw new ArgumentNullException("match");
            return this.Any(i => match(i));
        }

        public T Find(Predicate<T> match)
        {
            if (match == null)
                throw new ArgumentNullException("match");
            return this.FirstOrDefault(i => match(i));
        }

        public ImmutableList<T> FindAll(Predicate<T> match)
        {
            if (match == null)
                throw new ArgumentNullException("match");
            var builder = Clear().ToBuilder();
            foreach (var item in this)
            {
                if (match(item))
                    builder.Add(item);
            }
            return builder.ToImmutable();
        }

        public int FindIndex(int startIndex, int count, Predicate<T> match)
        {
            if (startIndex < 0 || startIndex >= Count)
                throw new ArgumentOutOfRangeException("startIndex");
            if (count < 0 || startIndex + count > Count)
                throw new ArgumentOutOfRangeException("count");
            if (match == null)
                throw new ArgumentNullException("match");

            int i = startIndex;
            foreach (var item in root.Enumerate (startIndex, count, false))
            {
                if (match(item))
                    return i;
                i++;
            }
            return -1;
        }

        public int FindIndex(Predicate<T> match)
        {
            if (match == null)
                throw new ArgumentNullException("match");
            return FindIndex(0, Count, match);
        }

        public int FindIndex(int startIndex, Predicate<T> match)
        {
            if (startIndex < 0 || startIndex >= Count)
                throw new ArgumentOutOfRangeException("startIndex");
            if (match == null)
                throw new ArgumentNullException("match");
            return FindIndex(startIndex, Count - startIndex, match);
        }

        public T FindLast(Predicate<T> match)
        {
            if (match == null)
                throw new ArgumentNullException("match");
            return this.LastOrDefault(i => match(i));
        }

        public int FindLastIndex(Predicate<T> match)
        {
            if (match == null)
                throw new ArgumentNullException("match");
            return FindLastIndex(Count - 1, Count, match);
        }

        public int FindLastIndex(int startIndex, Predicate<T> match)
        {
            if (startIndex < 0 || startIndex >= Count)
                throw new ArgumentOutOfRangeException("startIndex");
            if (match == null)
                throw new ArgumentNullException("match");
            return FindLastIndex(startIndex, startIndex + 1, match);
        }

        public int FindLastIndex(int startIndex, int count, Predicate<T> match)
        {
            if (startIndex < 0 || startIndex >= Count)
                throw new ArgumentOutOfRangeException("startIndex");
            if (count > Count || startIndex - count + 1 < 0)
                throw new ArgumentOutOfRangeException("count");
            if (match == null)
                throw new ArgumentNullException("match");

            int index = startIndex;
            foreach (var item in root.Enumerate (startIndex, count, true))
            {
                if (match(item))
                    return index;
                index--;
            }
            return -1;
        }

        public void ForEach(Action<T> action)
        {
            if (action == null)
                throw new ArgumentNullException("action");
            foreach (var item in this)
            {
                action(item);
            }
        }

        public ImmutableList<T> GetRange(int index, int count)
        {
            return ImmutableList.Create(valueComparer, root.Enumerate(index, count, false));
        }

        public int IndexOf(T value)
        {
            return IndexOf(value, 0, Count);
        }

        public int IndexOf(T value, int index)
        {
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException("index");
            return IndexOf(value, 0, Count - index);
        }

        public int IndexOf(T value, int index, int count)
        {
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException("index");
            if (count < 0 || index + count > Count)
                throw new ArgumentOutOfRangeException("count");

            return FindIndex(index, count, i => valueComparer.Equals(value, i));
        }

        public int LastIndexOf(T item, int index)
        {
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException("index");
            return LastIndexOf(item, index, index + 1);
        }

        public int LastIndexOf(T item)
        {
            return LastIndexOf(item, Count - 1, Count);
        }

        public int LastIndexOf(T item, int index, int count)
        {
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException("index");
            if (count > Count || index - count + 1 < 0)
                throw new ArgumentOutOfRangeException("count");
            return FindLastIndex(index, count, i => valueComparer.Equals(item, i));
        }
		#region IList implementation

        int IList.Add(object value)
        {
            throw new NotSupportedException();
        }

        void IList.Clear()
        {
            throw new NotSupportedException();
        }

        bool IList.Contains(object value)
        {
            return Contains((T)value);
        }

        int IList.IndexOf(object value)
        {
            return IndexOf((T)value);
        }

        void IList.Insert(int index, object value)
        {
            throw new NotSupportedException();
        }

        void IList.Remove(object value)
        {
            throw new NotSupportedException();
        }

        void IList.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        bool IList.IsFixedSize
        {
            get
            {
                return true;
            }
        }

        object IList.this [int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        bool IList.IsReadOnly
        {
            get
            {
                return true;
            }
        }
		#endregion

		#region ICollection implementation

        void ICollection.CopyTo(Array array, int index)
        {
            foreach (var item in this)
                array.SetValue(item, index++);
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return true;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return this;
            }
        }
		#endregion

		#region IList<T> implementation
        T IList<T>.this [int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                throw new NotSupportedException();
            }
        }
        void IList<T>.Insert(int index, T item)
        {
            throw new NotSupportedException();
        }

        void IList<T>.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }
		#endregion

		#region ICollection<T> implementation

        void ICollection<T>.Add(T item)
        {
            throw new NotSupportedException();
        }

        void ICollection<T>.Clear()
        {
            throw new NotSupportedException();
        }

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            CopyTo(array, arrayIndex);
        }

        bool ICollection<T>.Remove(T item)
        {
            throw new NotSupportedException();
        }

        bool ICollection<T>.IsReadOnly
        {
            get
            {
                return true;
            }
        }
		#endregion

		#region IEnumerable implementation

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
		#endregion

		#region IImmutableList<T> implementation

        public ImmutableList<T> Add(T value)
        {
            return Insert(Count, value);
        }

        IImmutableList<T> IImmutableList<T>.Add(T value)
        {
            return Add(value);
        }

        public ImmutableList<T> AddRange(IEnumerable<T> items)
        {
            return InsertRange(Count, items);
        }

        IImmutableList<T> IImmutableList<T>.AddRange(IEnumerable<T> items)
        {
            return AddRange(items);
        }

        public ImmutableList<T> Clear()
        {
            return Empty.WithComparer(valueComparer);
        }

        IImmutableList<T> IImmutableList<T>.Clear()
        {
            return Clear();
        }

        public bool Contains(T value)
        {
            return IndexOf(value) != -1;
        }

        public ImmutableList<T> Insert(int index, T element)
        {
            if (index > Count)
                throw new ArgumentOutOfRangeException("index");
            return new ImmutableList<T>(root.InsertIntoNew(index, element), valueComparer);
        }

        IImmutableList<T> IImmutableList<T>.Insert(int index, T element)
        {
            return Insert(index, element);
        }

        public ImmutableList<T> InsertRange(int index, IEnumerable<T> items)
        {
            var result = this;
            foreach (var item in items)
            {
                result = result.Insert(index++, item);
            }
            return result;
        }

        IImmutableList<T> IImmutableList<T>.InsertRange(int index, IEnumerable<T> items)
        {
            return InsertRange(index, items);
        }

        public ImmutableList<T> Remove(T value)
        {
            int loc = IndexOf(value);
            if (loc != -1)
                return RemoveAt(loc);

            return this;
        }

        IImmutableList<T> IImmutableList<T>.Remove(T value)
        {
            return Remove(value);
        }

        public ImmutableList<T> RemoveAll(Predicate<T> match)
        {
            if (match == null)
                throw new ArgumentNullException("match");
            var result = this;
            for (int i = 0; i < result.Count; i++)
            {
                if (match(result[i]))
                {
                    result = result.RemoveAt(i);
                    i--;
                    continue;
                }
            }
            return result;
        }

        IImmutableList<T> IImmutableList<T>.RemoveAll(Predicate<T> match)
        {
            return RemoveAll(match);
        }

        public ImmutableList<T> RemoveAt(int index)
        {
            bool found;
            return new ImmutableList<T>(root.RemoveFromNew(index, out found), valueComparer);
        }

        IImmutableList<T> IImmutableList<T>.RemoveAt(int index)
        {
            return RemoveAt(index);
        }

        void CheckRange(int idx, int count)
        {
            if (idx < 0)
                throw new ArgumentOutOfRangeException("index");

            if (count < 0)
                throw new ArgumentOutOfRangeException("count");

            if ((uint)idx + (uint)count > (uint)this.Count)
                throw new ArgumentException("index and count exceed length of list");
        }

        public ImmutableList<T> RemoveRange(int index, int count)
        {
            CheckRange(index, count);
            var result = this;
            while (count-- > 0)
            {
                result = result.RemoveAt(index);
            }
            return result;
        }

        IImmutableList<T> IImmutableList<T>.RemoveRange(int index, int count)
        {
            return RemoveRange(index, count);
        }

        public ImmutableList<T> RemoveRange(IEnumerable<T> items)
        {
            var result = this;
            foreach (var item in items)
            {
                result = result.Remove(item);
            }
            return result;
        }

        IImmutableList<T> IImmutableList<T>.RemoveRange(IEnumerable<T> items)
        {
            return RemoveRange(items);
        }

        public ImmutableList<T> Replace(T oldValue, T newValue)
        {
            var idx = IndexOf(oldValue);
            if (idx < 0)
                return this;
            return SetItem(idx, newValue);
        }

        IImmutableList<T> IImmutableList<T>.Replace(T oldValue, T newValue)
        {
            return Replace(oldValue, newValue);
        }

        public ImmutableList<T> SetItem(int index, T value)
        {
            if (index > Count)
                throw new ArgumentOutOfRangeException("index");
            return new ImmutableList<T>(root.SetItem(index, value), valueComparer);
        }

        IImmutableList<T> IImmutableList<T>.SetItem(int index, T value)
        {
            return SetItem(index, value);
        }

        public ImmutableList<T> WithComparer(IEqualityComparer<T> equalityComparer)
        {
            return new ImmutableList<T>(root, equalityComparer);
        }

        IImmutableList<T> IImmutableList<T>.WithComparer(IEqualityComparer<T> equalityComparer)
        {
            return WithComparer(equalityComparer);
        }

        public IEqualityComparer<T> ValueComparer
        {
            get
            {
                return valueComparer;
            }
        }
		#endregion

		#region IEnumerable<T> implementation

        public IEnumerator<T> GetEnumerator()
        {
            return root.GetEnumerator(false);
        }
		#endregion

		#region IReadOnlyList<T> implementation

        public T this [int index]
        {
            get
            {
                if (index >= Count)
                    throw new ArgumentOutOfRangeException("index");
                return root.GetNodeAt(index).Value;
            }
        }
		#endregion

		#region IReadOnlyCollection implementation

        public int Count
        {
            get
            {
                return root.Count;
            }
        }
		#endregion

		#region Builder
        public Builder ToBuilder()
        {
            return new Builder(root, valueComparer);
        }

        public class Builder
        {
            AvlNode<T> root;
            readonly IEqualityComparer<T> valueComparer;

            internal Builder(AvlNode<T> immutableRoot, IEqualityComparer<T> comparer)
            {
                root = immutableRoot.ToMutable();
                valueComparer = comparer;
            }

            public ImmutableList<T> ToImmutable()
            {
                return new ImmutableList<T>(root.ToImmutable(), valueComparer);
            }

            public void Add(T value)
            {
                Insert(Count, value);
            }

            public void Insert(int index, T element)
            {
                if (index > Count)
                    throw new ArgumentOutOfRangeException("index");
                root = root.InsertIntoNew(index, element);
                Debug.Assert(root.IsMutable);
            }

            public int Count
            {
                get
                {
                    return root.Count;
                }
            }
        }
		#endregion
    }

    public static class ImmutableList
    {
        public static ImmutableList<T> Create<T>()
        {
            return ImmutableList<T>.Empty;
        }

        public static ImmutableList<T> Create<T>(IEqualityComparer<T> equalityComparer, params T[] items)
        {
            return ImmutableList<T>.Empty.WithComparer(equalityComparer).AddRange(items);
        }

        public static ImmutableList<T> Create<T>(params T[] items)
        {
            return Create(EqualityComparer<T>.Default, items);
        }

        public static ImmutableList<T> Create<T>(IEqualityComparer<T> equalityComparer, IEnumerable<T> items)
        {
            return Create(equalityComparer, items.ToArray());
        }

        public static ImmutableList<T> Create<T>(IEnumerable<T> items)
        {
            return Create(items.ToArray());
        }

        public static ImmutableList<T> Create<T>(IEqualityComparer<T> equalityComparer, T item)
        {
            return ImmutableList<T>.Empty.WithComparer(equalityComparer).Add(item);
        }

        public static ImmutableList<T> Create<T>(T item)
        {
            return Create(EqualityComparer<T>.Default, item);
        }

        public static ImmutableList<T> Create<T>(IEqualityComparer<T> equalityComparer)
        {
            return Create<T>().WithComparer(equalityComparer);
        }

        public static ImmutableList<T> ToImmutableList<T>(this IEnumerable<T> source)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            return Create<T>().AddRange(source);
        }

        public static ImmutableList<T> ToImmutableList<T>(this IEnumerable<T> source, IEqualityComparer<T> equalityComparer)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            return Create<T>().WithComparer(equalityComparer).AddRange(source);
        }
    }
}
