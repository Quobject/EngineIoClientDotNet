using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quobject.EngineIoClientDotNet.Modules
{
    /// <remarks>
    /// UTF-8 encoder/decoder ported from utf8.js.
    /// Ported from the JavaScript module.
    /// <see href="https://github.com/mathiasbynens/utf8.js">https://github.com/mathiasbynens/utf8.js</see>
    /// </remarks>
    public class UTF8
    {
        private static List<int> byteArray;
        private static int byteCount;
        private static int byteIndex;

        public static string Encode(string str)
        {
            List<int> codePoints = Ucs2Decode(str);
            var length = codePoints.Count;
            var index = -1;
            var byteString = new StringBuilder();
            while (++index < length)
            {
                var codePoint = codePoints[index];
                byteString.Append( EncodeCodePoint(codePoint));
            }
            return byteString.ToString();
        }
        //
        // Try using UTF8Encoding http://msdn.microsoft.com/en-us/library/system.text.utf8encoding(v=vs.110).aspx
        //

        private static StringBuilder EncodeCodePoint(int codePoint)
        {
            throw new NotImplementedException();
        }

        public static string Decode(string Encoded)
        {
            throw new NotImplementedException();
        }

        private static List<int> Ucs2Decode(string str)
        {
            var output = new List<int>();
            var counter = 0;
            var length = str.Length;

            while (counter < length)
            {
                var value = (int) str[counter++];

                if (value >= 0xD800 && value <= 0xDBFF && counter < length)
                {
                    // high surrogate, and there is a next character
                    var extra = (int) str[counter++];
                    if ((extra & 0xFC00) == 0xDC00)
                    {
                        // low surrogate
                        output.Add(((value & 0x3FF) << 10) + (extra & 0x3FF) + 0x10000);
                    }
                    else
                    {
                        // unmatched surrogate; only append this code unit, in case the next
                        // code unit is the high surrogate of a surrogate pair
                        output.Add(value);
                        counter--;
                    }
                }
                else
                {
                    output.Add(value);
                }
            }
            return output;
        }

        private static string Ucs2Encode(List<int> array)
        {
            var sb = new StringBuilder();
            var index = -1;
            while (++index < array.Count)
            {
                var value = array[index];
                if (value > 0xFFFF)
                {
                    value -= 0x10000;
                    sb.Append((char) (((int) ((uint) value >> 10)) & 0x3FF | 0xD800));
                    value = 0xDC00 | value & 0x3FF;
                }
                sb.Append((char) value);
            }
            return sb.ToString();
        }


    }
}
