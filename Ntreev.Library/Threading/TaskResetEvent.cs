using Ntreev.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ntreev.Library.Threading
{
    public sealed class TaskResetEvent<T>
    {
        private readonly Dictionary<T, Task> setsByID = new Dictionary<T, Task>();

        public TaskResetEvent(Dispatcher dispatcher)
        {
            this.Dispatcher = dispatcher;
        }

        public async Task WaitAsync(T id)
        {
            var task = await this.Dispatcher.InvokeAsync(() =>
            {
                if (this.setsByID.ContainsKey(id) == false)
                {
                    this.setsByID.Add(id, new Task(() => { }));
                }
                return this.setsByID[id];
            });
            await task;
        }

        public async void SetAsync(T id)
        {
            await this.Dispatcher.InvokeAsync(() =>
            {
                if (this.setsByID.ContainsKey(id) == true)
                {
                    this.setsByID[id].Start();
                }
                else
                {
                    this.setsByID.Add(id, Task.Run(() => { }));
                }
            });
        }

        public void Set(T id)
        {
            this.Dispatcher.VerifyAccess();
            if (this.setsByID.ContainsKey(id) == true)
            {
                var task = this.setsByID[id];
                if (task.Status == TaskStatus.Created)
                {
                    task.Start();
                }
            }
            else
            {
                this.setsByID.Add(id, Task.Run(() => { }));
            }
        }

        public void Reset(T id)
        {
            this.Dispatcher.VerifyAccess();
            if (this.setsByID.ContainsKey(id) == true)
            {
                this.setsByID.Remove(id);
            }
        }

        private Dispatcher Dispatcher { get; }
    }
}
