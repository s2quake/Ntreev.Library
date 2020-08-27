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

using System.Collections.Generic;
using System.Threading.Tasks;

namespace JSSoft.Library.Threading
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

        public void Set(params T[] ids)
        {
            this.Dispatcher.VerifyAccess();
            foreach (var item in ids)
            {
                if (this.setsByID.ContainsKey(item) == true)
                {
                    var task = this.setsByID[item];
                    if (task.Status == TaskStatus.Created)
                    {
                        task.Start();
                    }
                }
                else
                {
                    this.setsByID.Add(item, Task.Run(() => { }));
                }
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
