// Released under the MIT License.
// 
// Copyright (c) 2018 Ntreev Soft co., Ltd.
// Copyright (c) 2020 Jeesu Choi
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation the
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit
// persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the
// Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace JSSoft.Library.Threading
{
    public sealed class DispatcherScheduler : TaskScheduler
    {
        private static readonly object lockobj = new object();
        private readonly Dispatcher dispatcher;
        private readonly CancellationToken cancellation;
        private readonly BlockingCollection<Task> taskQueue = new BlockingCollection<Task>();
        private readonly ManualResetEvent eventSet = new ManualResetEvent(false);
        private bool isExecuting;
        private bool isRunning;

        internal DispatcherScheduler(Dispatcher dispatcher, CancellationToken cancellation)
        {
            this.dispatcher = dispatcher;
            this.cancellation = cancellation;
        }

        public int ProcessAll()
        {
            return this.Process(int.MaxValue);
        }

        public int Process(int milliseconds)
        {
            this.dispatcher.VerifyAccess();
            if (this.isRunning == true)
                throw new InvalidOperationException("scheduler is already running.");
            if (this.eventSet.WaitOne(0) == false)
                return 0;

            var dateTime = DateTime.Now;
            var completion = 0;
            var count = this.taskQueue.Count;
            while (this.taskQueue.TryTake(out var task))
            {
                this.isExecuting = true;
                this.TryExecuteTask(task);
                this.isExecuting = false;
                completion++;
                var span = DateTime.Now - dateTime;
                if (span.TotalMilliseconds > milliseconds || completion >= count)
                    break;
            }
            return completion;
        }

        public bool ProcessOnce()
        {
            this.dispatcher.VerifyAccess();
            if (this.isRunning == true)
                throw new InvalidOperationException("scheduler is already running.");
            if (this.eventSet.WaitOne(0) == false)
                return false;
            if (this.taskQueue.TryTake(out var task) == true)
            {
                this.isExecuting = true;
                this.TryExecuteTask(task);
                this.isExecuting = false;
            }
            return this.taskQueue.Count != 0;
        }

        public new static DispatcherScheduler Current => Dispatcher.Current.Scheduler;

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return null;
        }

        protected override void QueueTask(Task task)
        {
            lock (lockobj)
            {
                this.taskQueue.Add(task, this.cancellation);
                this.eventSet.Set();
            }
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
#if DEBUG
            // 프로그램 종료시에도 Dispatcher가 여전히 실행중이라면 아래의 변수를 참고하여 
            // Dispatcher가 정상적으로 Dispose 되는지 확인합니다.
            var owner = this.dispatcher.Owner;
            var stackStace = this.dispatcher.StackTrace;
#endif
            this.isRunning = true;
            while (true)
            {
                if (this.taskQueue.TryTake(out var task) == true)
                {
                    this.isExecuting = true;
                    this.TryExecuteTask(task);
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
            this.isRunning = false;
        }
    }
}
