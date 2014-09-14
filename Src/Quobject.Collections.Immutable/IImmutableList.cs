//
// IImmutableList.cs
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
    public interface IImmutableList<T> : IReadOnlyList<T>, IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable
    {
        IEqualityComparer<T> ValueComparer
        {
            get;
        }

        IImmutableList<T> Add(T value);

        IImmutableList<T> AddRange(IEnumerable<T> items);

        IImmutableList<T> Clear();

        bool Contains(T value);

        int IndexOf(T value);

        IImmutableList<T> Insert(int index, T element);

        IImmutableList<T> InsertRange(int index, IEnumerable<T> items);

        IImmutableList<T> Remove(T value);

        IImmutableList<T> RemoveAll(Predicate<T> match);

        IImmutableList<T> RemoveAt(int index);

        IImmutableList<T> RemoveRange(int index, int count);

        IImmutableList<T> RemoveRange(IEnumerable<T> items);

        IImmutableList<T> Replace(T oldValue, T newValue);

        IImmutableList<T> SetItem(int index, T value);

        IImmutableList<T> WithComparer(IEqualityComparer<T> equalityComparer);
    }
}
