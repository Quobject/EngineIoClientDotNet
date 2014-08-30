using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quobject.EngineIoClientDotNet.Immutable
{
    class StringCompare : IEqualityComparer<string>
    {
        public bool Equals(string x, string y)
        {
            return x == y;
        }
        public int GetHashCode(string obj)
        {
            return obj.GetHashCode();
        }
    }
}
