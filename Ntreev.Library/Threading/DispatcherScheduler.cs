using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ntreev.Library.Threading
{
    class DispatcherScheduler : TaskScheduler
    {
        private static readonly object lockobj = new object();
        private readonly Dispatcher dispatcher;
        private readonly CancellationToken cancellation;
        private readonly BlockingCollection<Task> taskQueue = new BlockingCollection<Task>();
        private readonly ManualResetEvent eventSet = new ManualResetEvent(false);
        private bool isExecuting;

        public DispatcherScheduler(Dispatcher dispatcher, CancellationToken cancellation)
        {
            this.dispatcher = dispatcher;
            this.cancellation = cancellation;
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return null;
        }

        protected override void QueueTask(Task task)
        {
            this.taskQueue.Add(task, this.cancellation);
            this.eventSet.Set();
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            if (taskWasPreviouslyQueued)
                return false;

            return this.isExecuting && TryExecuteTask(task);
        }

        internal void Proceed()
        {
            this.eventSet.Set();
        }

        internal bool Continue { get; set; } = true;

        internal void Run()
        {
            while (true)
            {
                if (this.taskQueue.TryTake(out var task) == true)
                {
                    this.isExecuting = true;
                    this.TryExecuteTask(task);
                    //this.WaitForInnerTask(task);
                    this.isExecuting = false;
                    this.eventSet.Set();
                }
                else if (this.Continue == false)
                {
                    break;
                }
                else
                {
                    this.eventSet.WaitOne();
                    this.eventSet.Reset();
                }
            }
        }

        private void WaitForInnerTask(Task task)
        {
            var taskType = task.GetType();
            if (task.CreationOptions == TaskCreationOptions.AttachedToParent && taskType.IsGenericType && (taskType.GetGenericTypeDefinition() == typeof(Task<>)))
            {
                var genericType = taskType.GetGenericArguments()[0];
                if (genericType == typeof(Task))
                {
                    var innerTask = ((Task<Task>)task).Result;
                    innerTask.Wait();
                }
                else if (genericType.IsSubclassOf(typeof(Task)))
                {
                    var resultProperty = taskType.GetProperty("Result");
                    var result = resultProperty.GetValue(task);
                    var innerTask = (Task)result;
                    innerTask.Wait();
                    this.WaitForInnerTask(innerTask);
                }
            }
        }
    }
}
