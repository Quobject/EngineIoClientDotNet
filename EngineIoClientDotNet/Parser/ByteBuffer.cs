using System.IO;

namespace Quobject.EngineIoClientDotNet.Parser
{
    public class ByteBuffer
    {
        private readonly MemoryStream _memoryStream;

        public ByteBuffer(int length)
        {
            this._memoryStream = new MemoryStream();
            _memoryStream.SetLength(length);
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
    }
}