using System;
using System.IO;

namespace Quobject.EngineIoClientDotNet.Parser
{
    public class ByteBuffer
    {
        private readonly MemoryStream _memoryStream;

        private long _limit = 0;


        public ByteBuffer(int length)
        {
            this._memoryStream = new MemoryStream();
            _memoryStream.SetLength(length);
            _memoryStream.Capacity = length;
            _limit = length;
        }
 

        public static ByteBuffer Allocate(int length)
        {
            return new ByteBuffer(length);
        }

        internal void Put(byte[] buf)
        {           
            _memoryStream.Write(buf,0,buf.Length);
        }

        internal byte[] Array()
        {
            return _memoryStream.ToArray();
        }

        internal static ByteBuffer Wrap(byte[] data)
        {
            var result = new ByteBuffer(data.Length);
            result.Put(data);
            return result;
        }

        /// <summary>
        /// A buffer's capacity is the number of elements it contains. The capacity of a 
        /// buffer is never negative and never changes.
        /// </summary>
        public int Capacity
        {
            get { return _memoryStream.Capacity; }
        }

        /// <summary>
        /// Absolute get method. Reads the byte at the given index.
        /// </summary>
        /// <param name="index">The index from which the byte will be read</param>
        /// <returns>The byte at the given index</returns>
        internal byte Get(long index)
        {
            if (index > Capacity)
            {
                throw new IndexOutOfRangeException();
            }

            _memoryStream.Position = index;
            return (byte) _memoryStream.ReadByte();
        }

        /// <summary>
        /// Relative bulk get method.
        /// 
        /// This method transfers bytes from this buffer into the given destination array. If there are fewer bytes 
        /// remaining in the buffer than are required to satisfy the request, that is, if length > remaining(), then 
        /// no bytes are transferred and a BufferUnderflowException is thrown.
        /// 
        /// Otherwise, this method copies length bytes from this buffer into the given array, starting at the current 
        /// position of this buffer and at the given offset in the array. The position of this buffer is then 
        /// incremented by length.
        /// 
        /// In other words, an invocation of this method of the form src.get(dst, off, len) has exactly the same effect as the loop
        /// <code>
        ///     for (int i = off; i &lt; off + len; i++)
        ///         dst[i] = src.get(); 
        /// </code>
        /// except that it first checks that there are sufficient bytes in this buffer and it is potentially much more efficient.
        /// </summary>
        /// <param name="dst"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns>This buffer</returns>
        internal ByteBuffer Get(byte[] dst, int offset, int length)
        {
            _memoryStream.Read(dst, offset, length);
            return this;
        }


        /// <summary>
        /// Relative bulk get method.
        /// 
        /// This method transfers bytes from this buffer into the given destination array. 
        /// An invocation of this method of the form src.get(a) behaves in exactly the same 
        /// way as the invocation src.get(a, 0, a.length)
        /// </summary>
        /// <param name="dst"></param>
        /// <returns>This buffer</returns>
        internal ByteBuffer Get(byte[] dst)
        {
            return Get(dst, 0, dst.Length);
        }

        /// <summary>
        /// Sets this buffer's position. If the mark is defined and larger than the new 
        /// position then it is discarded.       
        /// </summary>
        /// <param name="newPosition">The new position value; must be non-negative and no larger than the current limit</param>
        internal void Position(long newPosition)
        {
            _memoryStream.Position = newPosition;
        }


        /// <summary>
        /// Sets this buffer's limit. If the position is larger than the new limit then it is set to the new limit. 
        /// If the mark is defined and larger than the new limit then it is discarded.
        /// 
        /// A buffer's limit is the index of the first element that should not be read or written. A buffer's limit is never negative and is never greater than its capacity.
        /// </summary>
        /// <param name="newLimit">The new limit value; must be non-negative and no larger than this buffer's capacity</param>
        internal void Limit(long newLimit)
        {
            _limit = newLimit;
            if (_memoryStream.Position > newLimit)
            {
                _memoryStream.Position = newLimit;
            }
        }

        /// <summary>
        /// Returns the number of elements between the current position and the limit.
        /// </summary>
        /// <returns>The number of elements remaining in this buffer</returns>
        internal long Remaining()
        {
            return (_limit - _memoryStream.Position);
        }



        /// <summary>
        /// Clears this buffer. The position is set to zero, the limit is set to the capacity, and the mark is discarded.
        /// 
        /// This method does not actually erase the data in the buffer, but it is named as if 
        /// it did because it will most often be used in situations in which that might as well be the case.
        /// </summary>
        internal void Clear()
        {
            Position(0);
            Limit(Capacity);
        }
    }
}