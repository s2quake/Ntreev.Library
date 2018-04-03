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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ntreev.Library
{
    public class Progress : IProgress
    {
        private readonly IProgress progress;

        public Progress()
        {

        }

        public Progress(IProgress progress)
        {
            this.progress = progress;
        }

        public event ProgressChangedEventHandler Changed;

        public void Report(double value)
        {
            this.Report(value, string.Empty);
        }

        public void Report(double value, string message)
        {
            if (this.progress != null)
            {
                this.progress.Report(value, message);
            }

            this.OnChanged(new ProgressChangedEventArgs(value, message));
        }

        public void Report(double value, string format, params object[] args)
        {
            this.Report(value, string.Format(format, args));
        }

        public void Complete()
        {
            this.Complete(string.Empty);
        }

        public void Complete(string message)
        {
            if (this.progress != null)
            {
                this.progress.Complete(message);
            }
            this.OnChanged(new ProgressChangedEventArgs(false, message));
        }

        public void Complete(string format, params object[] args)
        {
            this.Complete(string.Format(format, args));
        }

        public void Fail()
        {
            this.Fail(string.Empty);
        }

        public void Fail(string message)
        {
            if (this.progress != null)
            {
                this.progress.Fail(message);
            }
            this.OnChanged(new ProgressChangedEventArgs(true, message));
        }

        public void Fail(string format, params object[] args)
        {
            this.Fail(string.Format(format, args));
        }

        protected virtual void OnChanged(ProgressChangedEventArgs e)
        {
            this.Changed?.Invoke(this, e);
        }
    }
}
