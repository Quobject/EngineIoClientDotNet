using System;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.IO;

namespace EngineIoClientDotNet.Modules
{
    public class LogManager
    {
        private const string myFileName = "XunitTrace.txt";
        private string MyType;
        private static readonly LogManager EmptyLogger = new LogManager(null);

        #region Statics

        public static void SetupLogManager()
        {
            //if (myTextListener == null)
            //{
            //    var myOutputWriter = new StreamWriter(myFileName, true);
            //    myTextListener = new TextWriterTraceListener(myOutputWriter);
                
            //}
            //Trace.Listeners.Add(myTextListener);
            // First time execution, initialize the logger 
            EventListener verboseListener = new StorageFileEventListener("MyListenerVerbose");
            EventListener informationListener = new StorageFileEventListener("MyListenerInformation");

            verboseListener.EnableEvents(MetroEventSource.Log, EventLevel.Verbose);
            informationListener.EnableEvents(MetroEventSource.Log, EventLevel.Informational);
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
            MetroEventSource.Log.Info(msg);
        }

        [Conditional("DEBUG")]
        internal void Error(string p, Exception exception)
        {
            this.Info(string.Format("ERROR {0} {1} {2}",p,exception.Message, exception.StackTrace));
        }

        [Conditional("DEBUG")]
        internal void Error(Exception e)
        {
            this.Error("", e);
        }
    }

    //from http://code.msdn.microsoft.com/windowsapps/Logging-Sample-for-Windows-0b9dffd7
    sealed class MetroEventSource : EventSource
    {
        public static MetroEventSource Log = new MetroEventSource();

        [Event(1, Level = EventLevel.Verbose)]
        public void Debug(string message)
        {
            this.WriteEvent(1, message);
        }

        [Event(2, Level = EventLevel.Informational)]
        public void Info(string message)
        {
            this.WriteEvent(2, message);
        }

        [Event(3, Level = EventLevel.Warning)]
        public void Warn(string message)
        {
            this.WriteEvent(3, message);
        }

        [Event(4, Level = EventLevel.Error)]
        public void Error(string message)
        {
            this.WriteEvent(4, message);
        }

        [Event(5, Level = EventLevel.Critical)]
        public void Critical(string message)
        {
            this.WriteEvent(5, message);
        }
    }

    /// <summary> 
    /// This is an advanced usage, where you want to intercept the logging messages and devert them somewhere 
    /// besides ETW. 
    /// </summary> 
    sealed class StorageFileEventListener : EventListener
    {
        /// <summary> 
        /// Storage file to be used to write logs 
        /// </summary> 
        private StorageFile m_StorageFile = null;

        /// <summary> 
        /// Name of the current event listener 
        /// </summary> 
        private string m_Name;

        public StorageFileEventListener(string name)
        {
            this.m_Name = name;

            Debug.WriteLine("StorageFileEventListener for {0} has name {1}", GetHashCode(), name);

            AssignLocalFile();
        }

        private async void AssignLocalFile()
        {
            m_StorageFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(m_Name.Replace(" ", "_") + ".log",
                                                                                      CreationCollisionOption.OpenIfExists);
        }

        protected override async void OnEventWritten(EventWrittenEventArgs eventData)
        {
            // from http://msdn.microsoft.com/en-us/library/system.io.windowsruntimestorageextensions(v=vs.110).aspx
            using (StreamWriter writer =
                           new StreamWriter(await m_StorageFile.OpenStreamForWriteAsync()))
            {
                await writer.WriteLineAsync( eventData.Message);
            }
        }
     
    }
}
