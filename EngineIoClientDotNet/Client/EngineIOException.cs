using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quobject.EngineIoClientDotNet.Client
{
    public class EngineIOException : Exception
    {
        public string Transport;
        public object code;

        public EngineIOException(string message) : base(message)
        {
        }


        public EngineIOException(Exception cause)
            : base("", cause)
        {
        }

        public EngineIOException(string message, Exception cause)
            : base(message, cause)
        {
        }


    }
}
