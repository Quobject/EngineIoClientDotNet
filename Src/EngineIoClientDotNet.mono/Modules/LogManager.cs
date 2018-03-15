using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace Quobject.EngineIoClientDotNet.Modules
{
    public class LogManager
    {
        private const string LogFilePath = "XunitTrace.log";

        private static readonly LogManager EmptyLogger = new LogManager(null);

        private static StreamWriter writer;

        private readonly string type;

        #region Statics

        public static void SetupLogManager()
        {}

        public static LogManager GetLogger(string type)
        {
            return new LogManager(type);
        }

        public static LogManager GetLogger(Type type)
        {
            return GetLogger(type.ToString());
        }

        public static LogManager GetLogger(MethodBase methodBase)
        {
#if DEBUG
            string declaringType = methodBase.DeclaringType != null
                ? methodBase.DeclaringType.ToString()
                : string.Empty;
            string fullType = string.Format("{0}#{1}", declaringType, methodBase.Name);
            return GetLogger(fullType);
#else
            return EmptyLogger;
#endif
        }

        #endregion

        public LogManager(string type)
        {
            this.type = type;
        }

        public static bool Enabled { get; set; }

        private static StreamWriter Writer
        {
            get
            {
                if (writer == null)
                {
                    FileStream fs = new FileStream(
                        LogFilePath, FileMode.Append, FileAccess.Write, FileShare.Read);
                    writer = new StreamWriter(fs, Encoding.UTF8)
                    {
                        AutoFlush = true
                    };
                }

                return writer;
            }
        }

        [Conditional("DEBUG")]
        public void Info(string msg)
        {
            if (!Enabled)
            {
                return;
            }

            Writer.WriteLine(
                "{0:yyyy-MM-dd HH:mm:ss fff} [] {1} {2}",
                DateTime.Now,
                this.type,
                Global.StripInvalidUnicodeCharacters(msg));
        }

        [Conditional("DEBUG")]
        public void Error(string p, Exception exception)
        {
            this.Info($"ERROR {p} {exception.Message} {exception.StackTrace}");
            if (exception.InnerException != null)
            {
                this.Info($"ERROR exception.InnerException {p} {exception.InnerException.Message} {exception.InnerException.StackTrace}");
            }
        }


        [Conditional("DEBUG")]
        internal void Error(Exception e)
        {
            this.Error("", e);
        }
    }
}