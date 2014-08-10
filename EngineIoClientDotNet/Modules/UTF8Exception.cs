using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quobject.EngineIoClientDotNet.Modules
{
    public class UTF8Exception : Exception
    {
        public UTF8Exception(string message) : base(message)
        {}

    }
}
