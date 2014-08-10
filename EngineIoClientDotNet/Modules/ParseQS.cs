using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quobject.EngineIoClientDotNet.Modules
{
    /// <remarks>
    /// Provides methods for parsing a query string into an object, and vice versa.
    /// Ported from the JavaScript module.
    /// <see href="https://www.npmjs.org/package/parseqs">https://www.npmjs.org/package/parseqs</see>
    /// </remarks>
    public class ParseQS
    {


        /// <summary>
        /// Compiles a querystring
        /// Returns string representation of the object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string Encode(Dictionary<string, string> obj)
        {
            var sb = new StringBuilder();
            foreach (var key in obj.Keys)
            {
                if (sb.Length > 0)
                {
                    sb.Append("&");
                }
                sb.Append(Global.EncodeURIComponent(key));
                sb.Append("=");
                sb.Append(Global.EncodeURIComponent(obj[key]));
            }
            return sb.ToString();
        }



        /// <summary>
        /// Parses a simple querystring into an object
        /// </summary>
        /// <param name="qs"></param>
        /// <returns></returns>
        public static Dictionary<string, string> Decode(string qs)
        {
            Dictionary<string,string> qry = new Dictionary<string, string>();
            var pairs = qs.Split('&');
            for (int i = 0; i < pairs.Length; i++)
            {
                var pair = pairs[i].Split('=');

                qry.Add(Global.DecodeURIComponent(pair[0]), Global.DecodeURIComponent(pair[1]));
            }
            return qry;
        }
    }
}
