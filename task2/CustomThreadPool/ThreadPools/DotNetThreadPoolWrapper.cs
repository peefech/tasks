using System;
using System.Collections.Generic;
using System.Threading;

namespace CustomThreadPool
{
    public class DotNetThreadPoolWrapper : IThreadPool
    {
        private long processedTask = 0L;

        public void EnqueueAction(Action action)
        {
            ThreadPool.UnsafeQueueUserWorkItem(delegate
            {
                action.Invoke();
                Interlocked.Increment(ref processedTask);
            }, null);
        }

        public long GetTasksProcessedCount() => processedTask;
    }

    public class NewThreadPool : IThreadPool // на 1 балл
    {
        private long tasksProcessedCount = 0;
        private readonly int threadsCount;
        private readonly List<object> lockers = new List<object>();
        private readonly List<Action> works = new List<Action>();
        private readonly Queue<Action> actions = new Queue<Action>();

        public NewThreadPool()
        {
            threadsCount = 80;
            for (var i = 0; i < threadsCount; i++)
            {
                lockers.Add(new object());
                works.Add(null);
                var thread = new Thread((threadNumber) =>
                {
                    while (true)
                    {
                        Monitor.Enter(lockers[(int)threadNumber]);
                        works[(int)threadNumber] = null;
                        Monitor.Wait(lockers[(int)threadNumber]); 
                        Monitor.Exit(lockers[(int)threadNumber]);
                        works[(int)threadNumber].Invoke();
                        Interlocked.Increment(ref tasksProcessedCount);
                    }
                });
                thread.Start(i);
            }
            var mainThread = new Thread(() => { DoAction(); });
            mainThread.Start();
        }

        public void DoAction()
        {
            int actionsCount; 
            while (true)
            {
                actionsCount = actions.Count;
                if (actionsCount == 0)
                    continue;
                for (var i = 0; i < threadsCount; i++)
                {
                    if (works[i] != null)
                        continue;
                    Monitor.Enter(actions);
                    works[i] = actions.Dequeue();
                    Monitor.Exit(actions);
                    Monitor.Enter(lockers[i]);
                    Monitor.Pulse(lockers[i]);
                    Monitor.Exit(lockers[i]);
                    Interlocked.Decrement(ref actionsCount);
                    if (actionsCount == 0)
                        break;
                }
            }
        }

        public void EnqueueAction(Action action)
        {
            Monitor.Enter(actions);
            actions.Enqueue(action);
            Monitor.Exit(actions);
        }

        public long GetTasksProcessedCount()
        {
            return tasksProcessedCount;
        }
    }
}