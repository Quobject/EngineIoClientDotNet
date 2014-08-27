

using System;
using System.Threading.Tasks;

//http://msdn.microsoft.com/en-us/library/system.threading.tasks.concurrentexclusiveschedulerpair(v=vs.110).aspx

/**
 * The TaskScheduler for requests. 
 */
using System.Threading.Tasks.Dataflow;

namespace Quobject.EngineIoClientDotNet.Thread
{


    public class RequestTasks
    {
        private static readonly ConcurrentExclusiveSchedulerPair taskSchedulerPair = new ConcurrentExclusiveSchedulerPair();

        public static void Exec(Action<int> action)
        {
            var actionBlock = new ActionBlock<int>(action,
                new ExecutionDataflowBlockOptions {TaskScheduler = taskSchedulerPair.ConcurrentScheduler});
            actionBlock.Post(0);
            //Console.WriteLine("after post");
            //actionBlock.Completion.ContinueWith( n => Console.WriteLine("finished"));
            actionBlock.Complete();

        }

    }
}