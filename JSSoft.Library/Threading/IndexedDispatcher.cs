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
// 
// Forked from https://github.com/NtreevSoft/Ntreev.Library
// Namespaces and files starting with "Ntreev" have been renamed to "JSSoft".

using JSSoft.Library.Properties;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace JSSoft.Library.Threading
{
    public class IndexedDispatcher
    {
        private readonly CancellationTokenSource cancellationQueue;
        private readonly Dictionary<long, Task> taskByIndex = new Dictionary<long, Task>();
        private readonly ManualResetEvent eventSet = new ManualResetEvent(false);
        private long currentIndex;
        private long maxIndex;
        private bool isProceedable = true;

        public IndexedDispatcher(object owner)
        {
            this.cancellationQueue = new CancellationTokenSource();
            this.Owner = owner;
            this.Thread = new Thread(() =>
            {
                this.Run();
                this.OnDisposed(EventArgs.Empty);
            })
            {
                Name = $"IndexedDispatcher: {owner}"
            };
            this.Thread.Start();
        }

        public override string ToString()
        {
            return this.Thread.Name;
        }

        public void VerifyAccess()
        {
            if (!this.CheckAccess())
            {
                throw new InvalidOperationException(Resources.Exception_ThreadCannotAccess);
            }
        }

        public bool CheckAccess()
        {
            return this.Thread == Thread.CurrentThread;
        }

        public Task InvokeAsync(long index, Action action)
        {
            if (this.cancellationQueue.IsCancellationRequested == true)
                throw new OperationCanceledException();
            var task = new Task(action);
            this.AddTask(index, task);
            return task;
        }

        public Task<TResult> InvokeAsync<TResult>(long index, Func<TResult> callback)
        {
            if (this.cancellationQueue.IsCancellationRequested == true)
                throw new OperationCanceledException();
            var task = new Task<TResult>(callback);
            this.AddTask(index, task);
            return task;
        }

        public void Dispose()
        {
            if (this.cancellationQueue.IsCancellationRequested == true)
                throw new OperationCanceledException();
            this.cancellationQueue.Cancel();
            this.isProceedable = false;
            this.eventSet.Set();
        }

        public async Task DisposeAsync()
        {
            if (this.cancellationQueue.IsCancellationRequested == true)
                throw new OperationCanceledException();
            this.cancellationQueue.Cancel();
            var task = new Task(() => { });
            lock (this.taskByIndex)
            {
                this.taskByIndex.Add(this.maxIndex, task);
            }
            this.isProceedable = false;
            this.eventSet.Set();
            await task;
        }

        public string Name => this.Owner.ToString();

        public object Owner { get; }

        public Thread Thread { get; }

        public event EventHandler Disposed;

        protected virtual void OnDisposed(EventArgs e)
        {
            this.Disposed?.Invoke(this, e);
        }

        private void Run()
        {
            while (true)
            {
                if (this.GetTask() is Task task)
                {
                    task.RunSynchronously();
                    continue;
                }

                if (this.isProceedable == false)
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

        private Task GetTask()
        {
            lock (this.taskByIndex)
            {
                if (this.taskByIndex.ContainsKey(this.currentIndex) == true)
                {
                    var task = this.taskByIndex[this.currentIndex];
                    this.taskByIndex.Remove(this.currentIndex);
                    this.currentIndex++;
                    return task;
                }
                return null;
            }
        }

        private void AddTask(long index, Task task)
        {
            lock (taskByIndex)
            {
                this.taskByIndex.Add(index, task);
                this.maxIndex = Math.Max(index + 1, this.maxIndex);
                this.eventSet.Set();
            }
        }
    }
}
