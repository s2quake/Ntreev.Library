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

        /// <summary>
        /// 占쏙옙占쏙옙 占쌤곤옙 占쏙옙 횟占쏙옙占쏙옙 占쏙옙占쏙옙占쌉곤옙 占쏙옙占시울옙 占쏙옙占쏙옙占쏙옙 占싯몌옙占싹댐옙. Changed 占싱븝옙트占쏙옙 占쌩삼옙占싹몌옙 占쏙옙占쏙옙占쏙옙占?0占쏙옙占쏙옙 占쏙옙占쏙옙占싯니댐옙.
        /// </summary>
        /// <remarks>
        /// 占쌤곤옙 占쏙옙占쏙옙 횟占쏙옙占쏙옙 Next() 호占쏙옙 횟占쏙옙占쏙옙 占싼뱄옙占쏙옙 Complete() 호占쏙옙占쏙옙 占쏙옙占쏙옙 占쏙옙占쌉니댐옙. 
        /// 횟占쏙옙占쏙옙 3占쏙옙占쏙옙 占쏙옙占쏙옙占쏙옙占쏙옙占쏙옙占쏙옙 占싸뱄옙占쏙옙 Next() 占쏙옙 占싼뱄옙占쏙옙 Complete() 占쏙옙 호占쏙옙퓸占쌩몌옙 占쌌니댐옙.
        /// </remarks>
        public void Begin(int steps, string message)
        {
            if (steps <= 0)
                throw new Exception("占쌍쇽옙 1占쌤곤옙 占싱삼옙占쏙옙 占실억옙占?占쌌니댐옙.");
            this.Step = 0;
            this.steps = steps;

            this.Report(0, message);
        }

        public void Begin(int steps, string format, params object[] args)
        {
            this.Begin(steps, string.Format(format, args));
        }

        /// <summary>
        /// 占쏙옙占쏙옙 占쌤계를 占쏙옙占쏙옙占쏙옙 占싹곤옙 占쏙옙占쏙옙 占쌤곤옙占?占쏙옙占쏙옙占쌌니댐옙.
        /// </summary>
        public void Next()
        {
            this.Next(string.Empty);
        }

        public void Next(string message)
        {
            if (this.Step >= this.steps)
                throw new Exception(string.Format("占쌤계가 占쏙옙占쏙옙占쏙옙 횟占쏙옙({0})占쏙옙占쏙옙 占십곤옙占실억옙占쏙옙占싹댐옙.", this.Step));
            this.Step++;
            this.Report((double)this.Step / this.steps, message);
        }

        public void Next(string format, params object[] args)
        {
            this.Next(string.Format(format, args));
        }

        /// <summary>
        /// 占쌤계를 占쏙옙占쏙옙占쏙옙 占쌌니댐옙. 占쏙옙占쏙옙占쏙옙 占쌤곤옙 횟占쏙옙占쏙옙占쏙옙 占쏙옙占쏙옙 占쏙옙占승울옙占쏙옙 호占쏙옙퓸占쏙옙占쏙옙占쏙옙占?占쏙옙占쌤몌옙 占쌩삼옙占쌌니댐옙.
        /// </summary>
        public void Complete()
        {
            this.Complete(string.Empty);
        }

        public void Complete(string message)
        {
            if (this.Step + 1 < this.steps)
                throw new Exception(string.Format("{0} 占쌤곤옙占쏙옙 {1} 占쏙옙占쏙옙占쏙옙 占쏙옙占쏙옙퓸占쏙옙占쏙옙求占?", this.steps, this.Step));
            this.Complete(false, message);
            this.Step = 0;
            this.steps = 0;
        }

        public void Complete(string format, params object[] args)
        {
            this.Complete(string.Format(format, args));
        }

        /// <summary>
        /// 占쌤계를 占실너띰옙 占쏙옙占승뤄옙 占쏙옙占쏙옙占쏙옙 占쌌니댐옙.
        /// </summary>
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
