using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Web;

namespace Quobject.EngineIoClientDotNet.Modules
{
    public static class Global
    {
        public static string EncodeURIComponent(string str)
        {
            //http://stackoverflow.com/a/4550600/1109316
            return Uri.EscapeDataString(str);
        }

        public static string DecodeURIComponent(string str)
        {
            return Uri.UnescapeDataString(str);
        }

        public static string CallerName(string caller = "", int number = 0, string path = "")
        {
            var s = path.Split('\\');
            var fileName = s.LastOrDefault();
            if (path.Contains("SocketIoClientDotNet.Tests"))
            {
                path = "SocketIoClientDotNet.Tests";
            }
            else if (path.Contains("SocketIoClientDotNet"))
            {
                path = "SocketIoClientDotNet";
            }
            else if (path.Contains("EngineIoClientDotNet"))
            {
                path = "EngineIoClientDotNet";
            }

            return string.Format("{0}-{1}:{2}#{3}",path, fileName, caller, number);
        }

        //from http://stackoverflow.com/questions/8767103/how-to-remove-invalid-code-points-from-a-string
        public static string StripInvalidUnicodeCharacters(string str)
        {
            var invalidCharactersRegex = new Regex("([\ud800-\udbff](?![\udc00-\udfff]))|((?<![\ud800-\udbff])[\udc00-\udfff])");
            return invalidCharactersRegex.Replace(str, "");
        }
    }
}
