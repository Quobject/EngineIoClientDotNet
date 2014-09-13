

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

//http://msdn.microsoft.com/en-us/library/system.threading.tasks.concurrentexclusiveschedulerpair(v=vs.110).aspx

/**
 * The TaskScheduler for event loop. All non-background tasks run within the same thread.
 */
using System.Threading.Tasks.Dataflow;

namespace Quobject.EngineIoClientDotNet.Thread
{


    public class EventTasks
    {
        private static readonly ConcurrentExclusiveSchedulerPair taskSchedulerPair = new ConcurrentExclusiveSchedulerPair();

        public static void Exec(Action<int> action)
        {
            var actionBlock = new ActionBlock<int>(action,
                new ExecutionDataflowBlockOptions { TaskScheduler = taskSchedulerPair.ExclusiveScheduler });
            actionBlock.Post(0);
            //Console.WriteLine("after post");
            //actionBlock.Completion.ContinueWith( n => Console.WriteLine("finished"));
            actionBlock.Complete();

        }

    }
}