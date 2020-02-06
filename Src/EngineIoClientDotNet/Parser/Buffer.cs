namespace Quobject.EngineIoClientDotNet.Parser
{
    internal class Buffer
    {

        private Buffer()
        {
        }

        public static byte[] Concat(byte[][] list)
        {
            int length = 0;
            foreach (var buf in list)
            {
                length += buf.Length;
            }

            return Concat(list, length);
        }

        public static byte[] Concat(byte[][] list, int length)
        {
            if (list.Length == 0)
            {
                return new byte[0];
            }
            if (list.Length == 1)
            {
                return list[0];
            }

            ByteBuffer buffer = ByteBuffer.Allocate(length);
            foreach (var buf in list)
            {
                buffer.Put(buf);
            }

            return buffer.Array();
        }
    }
}