//
// AvlNode.cs
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
using System.Collections.Generic;

namespace Quobject.Collections.Immutable
{
    class AvlNode<T>
    {
        public static readonly AvlNode<T> Empty = new NullNode();

        sealed class NullNode : AvlNode<T>
        {
            public override bool IsEmpty
            {
                get
                {
                    return true;
                }
            }

            public override AvlNode<T> NewOrMutate(T newValue, AvlNode<T> newLeft, AvlNode<T> newRight)
            {
                throw new NotSupportedException();
            }

            public override AvlNode<T> ToMutable()
            {
                return this;
            }
        }

        public T Value;
        AvlNode<T> left;
        AvlNode<T> right;
        int _count;
        int _depth;

        public virtual bool IsEmpty { get { return false; } }

        public AvlNode<T> Left { get { return left; } }

        public AvlNode<T> Right { get { return right; } }

        public int Count
        {
            get
            {
                return _count;
            }
        }

        int Balance
        {
            get
            {
                if (IsEmpty)
                {
                    return 0;
                }
                return left._depth - right._depth;
            }
        }

        int Depth
        {
            get
            {
                return _depth;
            }
        }

        public AvlNode()
        {
            right = Empty;
            left = Empty;
        }

        public AvlNode(T val) : this(val, Empty, Empty)
        {
        }

        AvlNode(T val, AvlNode<T> lt, AvlNode<T> gt)
        {
            Value = val;
            left = lt;
            right = gt;
            _count = 1 + left._count + right._count;
            _depth = 1 + Math.Max(left._depth, right._depth);
        }

        /// <summary>
        /// Return the subtree with the min value at the root, or Empty if Empty
        /// </summary>
        AvlNode<T> Min
        {
            get
            {
                if (IsEmpty)
                    return Empty;
                var dict = this;
                var next = dict.left;
                while (next != Empty)
                {
                    dict = next;
                    next = dict.left;
                }
                return dict;
            }
        }

        /// <summary>
        /// Fix the root balance if LTDict and GTDict have good balance
        /// Used to keep the depth less than 1.44 log_2 N (AVL tree)
        /// </summary>
        AvlNode<T> FixRootBalance()
        {
            int bal = Balance;
            if (Math.Abs(bal) < 2)
                return this;

            if (bal == 2)
            {
                if (left.Balance == 1 || left.Balance == 0)
                {
                    //Easy case:
                    return this.RotateToGT();
                }
                if (left.Balance == -1)
                {
                    //Rotate LTDict:
                    var newlt = ToMutableIfNecessary(left).RotateToLT();
                    var newroot = NewOrMutate(Value, newlt, right);
                    return newroot.RotateToGT();
                }
                throw new Exception(String.Format("LTDict too unbalanced: {0}", left.Balance));
            }
            if (bal == -2)
            {
                if (right.Balance == -1 || right.Balance == 0)
                {
                    //Easy case:
                    return this.RotateToLT();
                }
                if (right.Balance == 1)
                {
                    //Rotate GTDict:
                    var newgt = ToMutableIfNecessary(right).RotateToGT();
                    var newroot = NewOrMutate(Value, left, newgt);
                    return newroot.RotateToLT();
                }
                throw new Exception(String.Format("LTDict too unbalanced: {0}", left.Balance));
            }
            //In this case we can show: |bal| > 2
            //if( Math.Abs(bal) > 2 ) {
            throw new Exception(String.Format("Tree too out of balance: {0}", Balance));
        }

        public AvlNode<T> SearchNode(T value, Comparison<T> comparer)
        {
            var dict = this;
            while (dict != Empty)
            {
                int comp = comparer(dict.Value, value);
                if (comp < 0)
                {
                    dict = dict.right;
                }
                else if (comp > 0)
                {
                    dict = dict.left;
                }
                else
                {
                    //Awesome:
                    return dict;
                }
            }
            return Empty;
        }

        /// <summary>
        /// Return a new tree with the key-value pair inserted
        /// If the key is already present, it replaces the value
        /// This operation is O(Log N) where N is the number of keys
        /// </summary>
        public AvlNode<T> InsertIntoNew(int index, T val)
        {
            if (IsEmpty)
                return new AvlNode<T>(val);

            AvlNode<T> newlt = left;
            AvlNode<T> newgt = right;

            if (index <= left._count)
            {
                newlt = ToMutableIfNecessary(left).InsertIntoNew(index, val);
            }
            else
            {
                newgt = ToMutableIfNecessary(right).InsertIntoNew(index - left._count - 1, val);
            }

            var newroot = NewOrMutate(Value, newlt, newgt);
            return newroot.FixRootBalance();
        }

        /// <summary>
        /// Return a new tree with the key-value pair inserted
        /// If the key is already present, it replaces the value
        /// This operation is O(Log N) where N is the number of keys
        /// </summary>
        public AvlNode<T> InsertIntoNew(T val, Comparison<T> comparer)
        {
            if (IsEmpty)
                return new AvlNode<T>(val);
            
            AvlNode<T> newlt = left;
            AvlNode<T> newgt = right;
            
            int comp = comparer(Value, val);
            T newv = Value;
            if (comp < 0)
            {
                //Let the GTDict put it in:
                newgt = ToMutableIfNecessary(right).InsertIntoNew(val, comparer);
            }
            else if (comp > 0)
            {
                //Let the LTDict put it in:
                newlt = ToMutableIfNecessary(left).InsertIntoNew(val, comparer);
            }
            else
            {
                //Replace the current value:
                newv = val;
            }
            var newroot = NewOrMutate(newv, newlt, newgt);
            return newroot.FixRootBalance();
        }

        public AvlNode<T> SetItem(int index, T val)
        {
            AvlNode<T> newlt = left;
            AvlNode<T> newgt = right;

            if (index < left._count)
            {
                newlt = ToMutableIfNecessary(left).SetItem(index, val);
            }
            else if (index > left._count)
            {
                newgt = ToMutableIfNecessary(right).SetItem(index - left._count - 1, val);
            }
            else
            {
                return NewOrMutate(val, newlt, newgt);
            }
            return NewOrMutate(Value, newlt, newgt);
        }

        public AvlNode<T> GetNodeAt(int index)
        {
            if (index < left._count) 
                return left.GetNodeAt(index);
            if (index > left._count) 
                return right.GetNodeAt(index - left._count - 1);
            return this;
        }

        /// <summary>
        /// Try to remove the key, and return the resulting Dict
        /// if the key is not found, old_node is Empty, else old_node is the Dict
        /// with matching Key
        /// </summary>
        public AvlNode<T> RemoveFromNew(int index, out bool found)
        {
            if (IsEmpty)
            {
                found = false;
                return Empty;
            }

            if (index < left._count)
            {
                var newlt = ToMutableIfNecessary(left).RemoveFromNew(index, out found);
                if (!found)
                {
                    //Not found, so nothing changed
                    return this;
                }
                var newroot = NewOrMutate(Value, newlt, right);
                return newroot.FixRootBalance();
            }

            if (index > left._count)
            {
                var newgt = ToMutableIfNecessary(right).RemoveFromNew(index - left._count - 1, out found);
                if (!found)
                {
                    //Not found, so nothing changed
                    return this;
                }
                var newroot = NewOrMutate(Value, left, newgt);
                return newroot.FixRootBalance();
            }

            //found it
            found = true;
            return RemoveRoot();
        }

        /// <summary>
        /// Try to remove the key, and return the resulting Dict
        /// if the key is not found, old_node is Empty, else old_node is the Dict
        /// with matching Key
        /// </summary>
        public AvlNode<T> RemoveFromNew(T val, Comparison<T> comparer, out bool found)
        {
            if (IsEmpty)
            {
                found = false;
                return Empty;
            }
            int comp = comparer(Value, val);
            if (comp < 0)
            {
                var newgt = ToMutableIfNecessary(right).RemoveFromNew(val, comparer, out found);
                if (!found)
                {
                    //Not found, so nothing changed
                    return this;
                }
                var newroot = NewOrMutate(Value, left, newgt);
                return newroot.FixRootBalance();
            }
            if (comp > 0)
            {
                var newlt = ToMutableIfNecessary(left).RemoveFromNew(val, comparer, out found);
                if (!found)
                {
                    //Not found, so nothing changed
                    return this;
                }
                var newroot = NewOrMutate(Value, newlt, right);
                return newroot.FixRootBalance();
            }
            //found it
            found = true;
            return RemoveRoot();
        }

        AvlNode<T> RemoveMax(out AvlNode<T> max)
        {
            if (IsEmpty)
            {
                max = Empty;
                return Empty;
            }
            if (right.IsEmpty)
            {
                //We are the max:
                max = this;
                return left;
            }
            else
            {
                //Go down:
                var newgt = ToMutableIfNecessary(right).RemoveMax(out max);
                var newroot = NewOrMutate(Value, left, newgt);
                return newroot.FixRootBalance();
            }
        }

        AvlNode<T> RemoveMin(out AvlNode<T> min)
        {
            if (IsEmpty)
            {
                min = Empty;
                return Empty;
            }
            if (left.IsEmpty)
            {
                //We are the minimum:
                min = this;
                return right;
            }
            //Go down:
            var newlt = ToMutableIfNecessary(left).RemoveMin(out min);
            var newroot = NewOrMutate(Value, newlt, right);
            return newroot.FixRootBalance();
        }

        /// <summary>
        /// Return a new dict with the root key-value pair removed
        /// </summary>
        AvlNode<T> RemoveRoot()
        {
            if (IsEmpty)
            {
                return this;
            }
            if (left.IsEmpty)
            {
                return right;
            }
            if (right.IsEmpty)
            {
                return left;
            }
            //Neither are empty:
            if (left._count < right._count)
            {
                //LTDict has fewer, so promote from GTDict to minimize depth
                AvlNode<T> min;
                var newgt = ToMutableIfNecessary(right).RemoveMin(out min);
                var newroot = NewOrMutate(min.Value, left, newgt);
                return newroot.FixRootBalance();
            }
            else
            {
                AvlNode<T> max;
                var newlt = ToMutableIfNecessary(left).RemoveMax(out max);
                var newroot = NewOrMutate(max.Value, newlt, right);
                return newroot.FixRootBalance();
            }
        }

        /// <summary>
        /// Move the Root into the GTDict and promote LTDict node up
        /// If LTDict is empty, this operation returns this
        /// </summary>
        AvlNode<T> RotateToGT()
        {
            if (left.IsEmpty || IsEmpty)
            {
                return this;
            }
            var newLeft = ToMutableIfNecessary(left);
            var lL = newLeft.left;
            var lR = newLeft.right;
            var newRight = NewOrMutate(Value, lR, right);
            return newLeft.NewOrMutate(newLeft.Value, lL, newRight);
        }

        /// <summary>
        /// Move the Root into the LTDict and promote GTDict node up
        /// If GTDict is empty, this operation returns this
        /// </summary>
        AvlNode<T> RotateToLT()
        {
            if (right.IsEmpty || IsEmpty)
            {
                return this;
            }
            var newRight = ToMutableIfNecessary(right);
            var rL = newRight.left;
            var rR = newRight.right;
            var newLeft = NewOrMutate(Value, left, rL);
            return newRight.NewOrMutate(newRight.Value, newLeft, rR);
        }

        /// <summary>
        /// Enumerate from largest to smallest key
        /// </summary>
        public IEnumerator<T> GetEnumerator(bool reverse)
        {
            var to_visit = new Stack<AvlNode<T>>();
            to_visit.Push(this);
            while (to_visit.Count > 0)
            {
                var this_d = to_visit.Pop();
                continue_without_pop:
                if (this_d.IsEmpty)
                {
                    continue;
                }
                if (reverse)
                {
                    if (this_d.right.IsEmpty)
                    {
                        //This is the next biggest value in the Dict:
                        yield return this_d.Value;
                        this_d = this_d.left;
                        goto continue_without_pop;
                    }
                    else
                    {
                        //Break it up
                        to_visit.Push(this_d.left);
                        to_visit.Push(new AvlNode<T>(this_d.Value));
                        this_d = this_d.right;
                        goto continue_without_pop;
                    }
                }
                else
                {
                    if (this_d.left.IsEmpty)
                    {
                        //This is the next biggest value in the Dict:
                        yield return this_d.Value;
                        this_d = this_d.right;
                        goto continue_without_pop;
                    }
                    else
                    {
                        //Break it up
                        if (!this_d.right.IsEmpty)
                            to_visit.Push(this_d.right);
                        to_visit.Push(new AvlNode<T>(this_d.Value));
                        this_d = this_d.left;
                        goto continue_without_pop;
                    }
                }
            }
        }

        public IEnumerable<T> Enumerate(int index, int count, bool reverse)
        {
            // TODO!
            int i;
            var e = GetEnumerator(reverse);
            if (!reverse)
            {
                i = 0;
                while (e.MoveNext ())
                {
                    if (index <= i)
                        yield return e.Current;
                    i++;
                    if (i >= index + count)
                        break;
                }
            }
            else
            {
                i = Count - 1;
                while (e.MoveNext ())
                {
                    if (i <= index)
                        yield return e.Current;
                    i--;
                    if (i <= index - count)
                        break;
                }
            }
        }

        public virtual AvlNode<T> ToImmutable()
        {
            return this;
        }

        public virtual AvlNode<T> NewOrMutate(T newValue, AvlNode<T> newLeft, AvlNode<T> newRight)
        {
            return new AvlNode<T>(newValue, newLeft, newRight);
        }

        public virtual AvlNode<T> ToMutable()
        {
            //throw new NotImplementedException ();
            return new MutableAvlNode(Value, left, right);
        }

        public virtual AvlNode<T> ToMutableIfNecessary(AvlNode<T> node)
        {
            return node;
        }

        public virtual bool IsMutable { get { return false; } }

        sealed class MutableAvlNode : AvlNode<T>
        {
            public MutableAvlNode(T val, AvlNode<T> lt, AvlNode<T> gt) : base (val, lt, gt)
            {
            }

            public override AvlNode<T> ToImmutable()
            {
                return new AvlNode<T>(Value, left.ToImmutable(), right.ToImmutable());
            }

            public override AvlNode<T> NewOrMutate(T newValue, AvlNode<T> newLeft, AvlNode<T> newRight)
            {
                Value = newValue;
                left = newLeft;
                right = newRight;
                _count = 1 + left._count + right._count;
                _depth = 1 + Math.Max(left._depth, right._depth);
                return this;
            }

            public override AvlNode<T> ToMutable()
            {
                return this;
            }

            public override AvlNode<T> ToMutableIfNecessary(AvlNode<T> node)
            {
                return node.ToMutable();
            }

            public override bool IsMutable { get { return true; } }
        }
    }
}
