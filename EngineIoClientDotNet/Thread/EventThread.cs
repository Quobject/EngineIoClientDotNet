

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


//http://codereview.stackexchange.com/questions/43000/a-taskscheduler-that-always-run-tasks-in-a-specific-thread


//http://msdn.microsoft.com/en-us/library/system.threading.tasks.concurrentexclusiveschedulerpair(v=vs.110).aspx
namespace Quobject.EngineIoClientDotNet.Thread
{


    public class SameThreadTaskScheduler : System.Threading.Tasks.TaskScheduler, IDisposable
    {

        private readonly Queue<Task> scheduledTasks;
        private readonly System.Threading.Thread myThread;
        private readonly string threadName;
        private bool quit;

        #region publics

        public SameThreadTaskScheduler(string name)
        {
            scheduledTasks = new Queue<Task>();
            threadName = name;
            lock (myThread)
            {
                if (myThread == null)
                {
                    myThread = StartThread(threadName);
                }                
            }
        }

        public override int MaximumConcurrencyLevel
        {
            get { return 1; }
        }

        public void Dispose()
        {
            lock (scheduledTasks)
            {
                quit = true;
                Monitor.PulseAll(scheduledTasks);
            }
        }

        #endregion

        #region protected overrides

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            lock (scheduledTasks)
            {
                return scheduledTasks.ToList();
            }
        }

        protected override void QueueTask(Task task)
        {
            if (quit)
            {
                throw new ObjectDisposedException("My thread is not alive, so this object has been disposed!");
            }
            lock (scheduledTasks)
            {
                scheduledTasks.Enqueue(task);
                Monitor.PulseAll(scheduledTasks);
            }
        }

        protected override bool TryExecuteTaskInline(Task task, bool task_was_previously_queued)
        {
            return false;
        }

        #endregion


        private System.Threading.Thread StartThread(string name)
        {
            var t = new System.Threading.Thread(MyThread) { Name = name };
            using (var start = new Barrier(2))
            {
                t.Start(start);
                ReachBarrier(start);
            }
            return t;
        }

        private void MyThread(object o)
        {
            Task tsk;
            lock (scheduledTasks)
            {
                //When reaches the barrier, we know it holds the lock.
                //
                //So there is no Pulse call can trigger until
                //this thread starts to wait for signals.
                //
                //It is important not to call StartThread within a lock.
                //Otherwise, deadlock!
                ReachBarrier(o as Barrier);
                tsk = WaitAndDequeueTask();
            }
            for (;;)
            {
                if (tsk == null)
                    break;
                TryExecuteTask(tsk);
                lock (scheduledTasks)
                {
                    tsk = WaitAndDequeueTask();
                }
            }
        }

        private Task WaitAndDequeueTask()
        {
            while (!scheduledTasks.Any() && !quit)
                Monitor.Wait(scheduledTasks);
            return quit ? null : scheduledTasks.Dequeue();
        }

        private static void ReachBarrier(Barrier b)
        {
            if (b != null)
                b.SignalAndWait();
        }
    }
}