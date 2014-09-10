using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EngineIoClientDotNet.Modules
{
    public class LogManager
    {
        private const string myFileName = "XunitTrace.txt";
        private string MyType;
        private static TextWriterTraceListener myTextListener = null;
        private static readonly LogManager EmptyLogger = new LogManager(null);

        #region Statics

        public static void SetupLogManager()
        {
            if (myTextListener == null)
            {
                var myOutputWriter = new StreamWriter(myFileName, true);
                myTextListener = new TextWriterTraceListener(myOutputWriter);

            }
            Trace.Listeners.Add(myTextListener);
        }

        public static LogManager GetLogger(string type)
        {
            var result = new LogManager(type);
            return result;
        }

        public static LogManager GetLogger(Type type)
        {
            return GetLogger(type.ToString());
        }

        public static LogManager GetLogger(System.Reflection.MethodBase methodBase)
        {
#if DEBUG
            var type = methodBase.DeclaringType == null ? "" : methodBase.DeclaringType.ToString();
            var type1 = string.Format("{0}#{1}", type, methodBase.Name);
            return GetLogger(type1);
#else
            return EmptyLogger;
#endif
        }

        #endregion

        public LogManager(string type)
        {
            this.MyType = type;
        }

        [Conditional("DEBUG")]
        public void Info(string msg)
        {
            Trace.WriteLine(string.Format("{0} [{3}] {1} - {2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff"), MyType, msg, System.Threading.Thread.CurrentThread.ManagedThreadId));
            myTextListener.Flush();
        }

        [Conditional("DEBUG")]
        internal void Error(string p, Exception exception)
        {
            this.Info(string.Format("ERROR {0} {1} {2}", p, exception.Message, exception.StackTrace));
        }

        [Conditional("DEBUG")]
        internal void Error(Exception e)
        {
            this.Error("", e);
        }
    }
}