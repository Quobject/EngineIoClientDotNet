using System.Collections.Generic;
using System.Text;

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
                byteString.Append(EncodeCodePoint(codePoint));
            }
            return byteString.ToString();
        }

        public static string Decode(string byteString)
        {
            byteArray = Ucs2Decode(byteString);
            byteCount = byteArray.Count;
            byteIndex = 0;

            var codePoints = new List<int>();
            int tmp;
            while ((tmp = DecodeSymbol()) != -1)
            {
                codePoints.Add(tmp);
            }
            return Ucs2Encode(codePoints);
        }

        private static int DecodeSymbol()
        {
            int byte1;
            int byte2;
            int byte3;
            int byte4;
            int codePoint;

            if (byteIndex > byteCount)
            {
                throw new UTF8Exception("Invalid byte index");
            }

            if (byteIndex == byteCount)
            {
                return -1;
            }

            byte1 = byteArray[byteIndex] & 0xFF;
            byteIndex++;

            if ((byte1 & 0x80) == 0)
            {
                return byte1;
            }

            if ((byte1 & 0xE0) == 0xC0)
            {
                byte2 = ReadContinuationByte();
                codePoint = ((byte1 & 0x1F) << 6) | byte2;
                if (codePoint >= 0x80)
                {
                    return codePoint;
                }
                else
                {
                    throw new UTF8Exception("Invalid continuation byte");
                }
            }

            if ((byte1 & 0xF0) == 0xE0)
            {
                byte2 = ReadContinuationByte();
                byte3 = ReadContinuationByte();
                codePoint = ((byte1 & 0x0F) << 12) | (byte2 << 6) | byte3;
                if (codePoint >= 0x0800)
                {
                    return codePoint;
                }
                else
                {
                    throw new UTF8Exception("Invalid continuation byte");
                }
            }

            if ((byte1 & 0xF8) == 0xF0)
            {
                byte2 = ReadContinuationByte();
                byte3 = ReadContinuationByte();
                byte4 = ReadContinuationByte();
                codePoint = ((byte1 & 0x0F) << 0x12) | (byte2 << 0x0C) | (byte3 << 0x06) | byte4;
                if (codePoint >= 0x010000 && codePoint <= 0x10FFFF)
                {
                    return codePoint;
                }
            }

            throw new UTF8Exception("Invalid continuation byte");
        }

        private static int ReadContinuationByte()
        {
            if (byteIndex >= byteCount)
            {
                throw new UTF8Exception("Invalid byte index");
            }

            int continuationByte = byteArray[byteIndex] & 0xFF;
            byteIndex++;

            if ((continuationByte & 0xC0) == 0x80)
            {
                return continuationByte & 0x3F;
            }

            throw new UTF8Exception("Invalid continuation byte");
        }


        private static string EncodeCodePoint(int codePoint)
        {
            var sb = new StringBuilder();
            if ((codePoint & 0xFFFFFF80) == 0)
            {
                // 1-byte sequence
                sb.Append((char) codePoint);
                return sb.ToString();
            }
            if ((codePoint & 0xFFFFF800) == 0)
            {
                // 2-byte sequence
                sb.Append((char) (((codePoint >> 6) & 0x1F) | 0xC0));
            }
            else if ((codePoint & 0xFFFF0000) == 0)
            {
                // 3-byte sequence
                sb.Append((char) (((codePoint >> 12) & 0x0F) | 0xE0));
                sb.Append( CreateByte(codePoint, 6));
            }
            else if ((codePoint & 0xFFE00000) == 0)
            {
                // 4-byte sequence
                sb.Append((char) (((codePoint >> 18) & 0x07) | 0xF0));
                sb.Append( CreateByte(codePoint, 12));
                sb.Append( CreateByte(codePoint, 6));
            }
            sb.Append((char) ((codePoint & 0x3F) | 0x80));
            return sb.ToString();
        }

        private static char CreateByte(int codePoint, int shift)
        {
            return (char)(((codePoint >> shift) & 0x3F) | 0x80);
        }



        private static List<int> Ucs2Decode(string str)
        {
            var output = new List<int>();
            var counter = 0;
            var length = str.Length;

            while (counter < length)
            {
                var value = (int)str[counter++];

                if (value >= 0xD800 && value <= 0xDBFF && counter < length)
                {
                    // high surrogate, and there is a next character
                    var extra = (int)str[counter++];
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
                    sb.Append((char)(((int)((uint)value >> 10)) & 0x3FF | 0xD800));
                    value = 0xDC00 | value & 0x3FF;
                }
                sb.Append((char)value);
            }
            return sb.ToString();
        }


    }
}
