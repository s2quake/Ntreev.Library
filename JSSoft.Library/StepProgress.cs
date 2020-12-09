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

namespace JSSoft.Library
{
    public class StepProgress : IProgress
    {
        private readonly IProgress progress;
        private int steps;

        public StepProgress()
        {

        }

        public StepProgress(IProgress progress)
        {
            this.progress = progress;
        }

        public void Begin(int steps)
        {
            this.Begin(steps, string.Empty);
        }

        public void Begin(int steps, string message)
        {
            if (steps <= 0)
                throw new ArgumentException(Resources.Exception_AtLeastStep1, nameof(steps));
            this.Step = 0;
            this.steps = steps;

            this.Report(0, message);
        }

        public void Begin(int steps, string format, params object[] args)
        {
            this.Begin(steps, string.Format(format, args));
        }

        public void Next()
        {
            this.Next(string.Empty);
        }

        public void Next(string message)
        {
            if (this.Step >= this.steps)
                throw new Exception(string.Format(Resources.Exception_OverStep_Format, this.Step));
            this.Step++;
            this.Report((double)this.Step / this.steps, message);
        }

        public void Next(string format, params object[] args)
        {
            this.Next(string.Format(format, args));
        }

        public void Complete()
        {
            this.Complete(string.Empty);
        }

        public void Complete(string message)
        {
            if (this.Step + 1 < this.steps)
                throw new Exception(string.Format(Resources.Exception_LessStep_Format, this.steps, this.Step));
            this.Complete(false, message);
            this.Step = 0;
            this.steps = 0;
        }

        public void Complete(string format, params object[] args)
        {
            this.Complete(string.Format(format, args));
        }

        public void Skip()
        {
            this.Skip(string.Empty);
        }

        public void Skip(string message)
        {
            this.Complete(false, message);
        }

        public void Skip(string format, params object[] args)
        {
            this.Skip(string.Format(format, args));
        }

        public void Show(string message)
        {
            this.Report((double)this.Step / this.steps, message);
        }

        public void Show(string format, params object[] args)
        {
            this.Show(string.Format(format, args));
        }

        public void Fail()
        {
            this.Fail(string.Empty);
        }

        public void Fail(string message)
        {
            this.Complete(true, message);
            this.Step = 0;
            this.steps = 0;
        }

        public void Fail(string format, params object[] args)
        {
            this.Fail(string.Format(format, args));
        }

        public void Fail(Exception e)
        {
            this.Fail(e.Message);
        }

        public int Step { get; private set; }

        public event ProgressChangedEventHandler Changed;

        protected virtual void OnChanged(ProgressChangedEventArgs e)
        {
            this.Changed?.Invoke(this, e);
        }

        private void Complete(bool isFailed, string message)
        {
            var e = new ProgressChangedEventArgs(isFailed, message);
            this.OnChanged(e);

            if (this.progress != null)
            {
                if (isFailed == true)
                    this.progress.Fail(message);
                else
                    this.progress.Complete(message);
            }
        }

        private void Report(double value, string message)
        {
            this.OnChanged(new ProgressChangedEventArgs(value, message));

            if (this.progress != null)
            {
                this.progress.Report(value, message);
            }
        }

        #region IProgress

        void IProgress.Complete(string message)
        {
            var v = (double)this.Step / this.steps + (1.0 / this.steps);
            this.OnChanged(new ProgressChangedEventArgs(v, message));

            if (this.progress != null)
            {
                this.progress.Report(1.0, message);
            }
        }

        void IProgress.Fail(string message)
        {
            var v = (double)this.Step / this.steps + (1.0 / this.steps);
            this.OnChanged(new ProgressChangedEventArgs(v, message));

            if (this.progress != null)
            {
                this.progress.Report(1.0, message);
            }
        }

        void IProgress.Report(double value, string message)
        {
            var v = (double)this.Step / this.steps + (1.0 / this.steps) * value;
            this.OnChanged(new ProgressChangedEventArgs(v, message));

            if (this.progress != null)
            {
                this.progress.Report(value, message);
            }
        }

        #endregion
    }
}
