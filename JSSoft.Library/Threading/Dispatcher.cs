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

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#if DEBUG
using System.Diagnostics;
#endif

namespace JSSoft.Library.Threading
{
    public class Dispatcher
    {
        private static readonly Dictionary<Thread, Dispatcher> dispatcherByThread = new Dictionary<Thread, Dispatcher>();
        private readonly TaskFactory factory;
        private readonly CancellationTokenSource cancellationQueue;
        private readonly CancellationTokenSource cancellationExecution;
        private readonly DispatcherSynchronizationContext context;
#if DEBUG
        private readonly StackTrace stackTrace;
#endif
        private Dispatcher(Thread thread)
        {
            this.cancellationQueue = new CancellationTokenSource();
            this.cancellationExecution = new CancellationTokenSource();
            this.Scheduler = new DispatcherScheduler(this, cancellationExecution.Token);
            this.factory = new TaskFactory(new CancellationToken(false), TaskCreationOptions.None, TaskContinuationOptions.None, Scheduler);
            this.context = new DispatcherSynchronizationContext(factory);
            this.Thread = thread;
        }

        public Dispatcher(object owner)
        {
            this.cancellationQueue = new CancellationTokenSource();
            this.cancellationExecution = new CancellationTokenSource();
            this.Scheduler = new DispatcherScheduler(this, cancellationExecution.Token);
            this.factory = new TaskFactory(new CancellationToken(false), TaskCreationOptions.None, TaskContinuationOptions.None, Scheduler);
            this.context = new DispatcherSynchronizationContext(factory);
            this.Owner = owner ?? throw new ArgumentNullException(nameof(owner));
#if DEBUG
            this.stackTrace = new StackTrace(true);
#endif            
            this.Thread = new Thread(() =>
            {
                this.Scheduler.Run();
                this.OnDisposed(EventArgs.Empty);
            })
            {
                Name = owner.ToString()
            };
            this.Thread.Start();
        }

        public static Task<Dispatcher> CreateAsync(object owner)
        {
            return Task.Run(() => new Dispatcher(owner));
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

        public Task InvokeAsync(Task task)
        {
            if (this.cancellationQueue.IsCancellationRequested == true)
                throw new OperationCanceledException();
            task.Start(this.Scheduler);
            return task;
        }

        public Task<TResult> InvokeAsync<TResult>(Task<TResult> task)
        {
            if (this.cancellationQueue.IsCancellationRequested == true)
                throw new OperationCanceledException();
            task.Start(this.Scheduler);
            return task;
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

        public TResult Invoke<TResult>(Func<TResult> callback, TaskCreationOptions creationOptions)
        {
            if (this.cancellationQueue.IsCancellationRequested == true)
                throw new OperationCanceledException();
            if (this.CheckAccess() == true)
            {
                return callback();
            }
            else
            {
                var task = this.factory.StartNew(callback, creationOptions);
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

        public Task<TResult> InvokeAsync<TResult>(Func<TResult> callback, TaskCreationOptions creationOptions)
        {
            if (this.cancellationQueue.IsCancellationRequested == true)
                throw new OperationCanceledException();
            return this.factory.StartNew(callback, creationOptions);
        }

        public void Dispose()
        {
            if (this.Owner == null)
                throw new InvalidOperationException("this is an object that cannot be disposed.");
            if (this.cancellationQueue.IsCancellationRequested == true)
                throw new OperationCanceledException();
            this.cancellationQueue.Cancel();
            this.Scheduler.Continue = false;
            this.Scheduler.Proceed();
        }

        public async Task DisposeAsync()
        {
            if (this.Owner == null)
                throw new InvalidOperationException("this is an object that cannot be disposed.");
            if (this.cancellationQueue.IsCancellationRequested == true)
                throw new OperationCanceledException();
            var task = this.factory.StartNew(() => { });
            this.cancellationQueue.Cancel();
            this.Scheduler.Continue = false;
            this.Scheduler.Proceed();
            await task;
        }

        public string Name => this.Owner.ToString();

        public object Owner { get; }

        public Thread Thread { get; }

        public SynchronizationContext SynchronizationContext => this.context;

        public static Dispatcher Current
        {
            get
            {
                var thread = Thread.CurrentThread;
                if (dispatcherByThread.ContainsKey(thread) == false)
                {
                    dispatcherByThread.Add(thread, new Dispatcher(thread));
                }
                return dispatcherByThread[thread];
            }
        }

        public event EventHandler Disposed;

#if DEBUG
        internal string StackTrace => $"{this.stackTrace}";
#endif

        internal DispatcherScheduler Scheduler { get; private set; }

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
            this.factory.StartNew(() => d(state));
        }

        public override void Post(SendOrPostCallback d, object state)
        {
            this.factory.StartNew(() => d(state));
        }
    }
}
