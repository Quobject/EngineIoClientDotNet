

using System;
using System.Threading.Tasks;

//http://msdn.microsoft.com/en-us/library/system.threading.tasks.concurrentexclusiveschedulerpair(v=vs.110).aspx

/**
 * The TaskScheduler for requests. 
 */
using System.Threading.Tasks.Dataflow;
using log4net;

namespace Quobject.EngineIoClientDotNet.Thread
{


    public class RequestTasks
    {
        private static readonly LimitedConcurrencyLevelTaskScheduler limitedConcurrencyLevelTaskScheduler = new LimitedConcurrencyLevelTaskScheduler(4);

        public static void Exec(Action<int> action)
        {
            var log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            log.Info("RequestTasks Exec 0");

            var actionBlock = new ActionBlock<int>(action,
                new ExecutionDataflowBlockOptions { TaskScheduler = limitedConcurrencyLevelTaskScheduler });

            log.Info("RequestTasks Exec 1" + actionBlock );

            actionBlock.Post(0);
            log.Info("RequestTasks Exec 2");

            //Console.WriteLine("after post");
            //actionBlock.Completion.ContinueWith( n => Console.WriteLine("finished"));
            actionBlock.Complete();

        }

    }
}