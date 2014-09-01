using System;

namespace Quobject.EngineIoClientDotNet.Modules
{
    public class UTF8Exception : Exception
    {
        public UTF8Exception(string message) : base(message)
        {}
    }
}
