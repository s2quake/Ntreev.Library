//Released under the MIT License.
//
//Copyright (c) 2018 Ntreev Soft co., Ltd.
//
//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
//documentation files (the "Software"), to deal in the Software without restriction, including without limitation the 
//rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit 
//persons to whom the Software is furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the 
//Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE 
//WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR 
//COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR 
//OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

#pragma warning disable 0612
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ntreev.Library
{
    public class Dispatcher
    {
        private readonly DispatcherScheduler scheduler;
        private readonly TaskFactory factory;
        private readonly CancellationTokenSource cancellationQueue;
        private readonly CancellationTokenSource cancellationExecution;
        private readonly DispatcherSynchronizationContext context;

        public Dispatcher(object owner)
        {
            this.cancellationQueue = new CancellationTokenSource();
            this.cancellationExecution = new CancellationTokenSource();
            this.scheduler = new DispatcherScheduler(this, this.cancellationExecution.Token);
            this.factory = new TaskFactory(new CancellationToken(false), TaskCreationOptions.None, TaskContinuationOptions.None, this.scheduler);
            this.context = new DispatcherSynchronizationContext(this.factory);
            this.Owner = owner;
            this.Thread = new Thread(() =>
            {
                this.scheduler.Run();
                this.OnDisposed(EventArgs.Empty);
            })
            {
                Name = owner.ToString()
            };
            this.Thread.Start();
        }

        public override string ToString()
        {
            return $"{this.Owner}";
        }

        public void VerifyAccess()
        {
            if (!this.CheckAccess())
            {
                throw new InvalidOperationException("The calling thread cannot access this object because a different thread owns it.");
            }
        }

        public bool CheckAccess()
        {
            return this.Thread == Thread.CurrentThread;
        }

        public void Invoke(Action action)
        {
            if (this.cancellationQueue.IsCancellationRequested == true)
                throw new OperationCanceledException();
            if (this.CheckAccess() == true)
            {
                action();
            }
            else
            {
                var task = this.factory.StartNew(action);
                task.Wait();
            }
        }

        public Task InvokeAsync(Action action)
        {
            if (this.cancellationQueue.IsCancellationRequested == true)
                throw new OperationCanceledException();
            return this.factory.StartNew(action);
        }

        public TResult Invoke<TResult>(Func<TResult> callback)
        {
            if (this.cancellationQueue.IsCancellationRequested == true)
                throw new OperationCanceledException();
            if (this.CheckAccess() == true)
            {
                return callback();
            }
            else
            {
                var task = this.factory.StartNew(callback);
                task.Wait();
                return task.Result;
            }
        }

        public Task<TResult> InvokeAsync<TResult>(Func<TResult> callback)
        {
            if (this.cancellationQueue.IsCancellationRequested == true)
                throw new OperationCanceledException();
            return this.factory.StartNew(callback);
        }

        public void Dispose()
        {
            if (this.cancellationQueue.IsCancellationRequested == true)
                throw new OperationCanceledException();
            this.cancellationQueue.Cancel();
            this.scheduler.Continue = false;
            this.scheduler.Proceed();
        }

        public async Task DisposeAsync()
        {
            if (this.cancellationQueue.IsCancellationRequested == true)
                throw new OperationCanceledException();
            var task = this.factory.StartNew(() => { });
            this.cancellationQueue.Cancel();
            this.scheduler.Continue = false;
            this.scheduler.Proceed();
            await task;
        }

        public string Name => this.Owner.ToString();

        public object Owner { get; }

        public Thread Thread { get; }

        public SynchronizationContext SynchronizationContext => this.context;

        public event EventHandler Disposed;

        protected virtual void OnDisposed(EventArgs e)
        {
            this.Disposed?.Invoke(this, e);
        }
    }

    public sealed class DispatcherSynchronizationContext : SynchronizationContext
    {
        private readonly TaskFactory factory;

        internal DispatcherSynchronizationContext(TaskFactory factory)
        {
            this.factory = factory;
        }

        public override void Send(SendOrPostCallback d, object state)
        {
            base.Send(d, state);
        }

        public override void Post(SendOrPostCallback d, object state)
        {
            this.factory.StartNew(() => d(state));
        }
    }
}
