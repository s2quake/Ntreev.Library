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

namespace Ntreev.Library
{
    public class StepProgress : IProgress
    {
        private readonly IProgress progress;
        private int steps;
        private int step;

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
        /// 진행 단계 총 횟수를 설정함과 동시에 시작을 알립니다. Changed 이벤트가 발생하며 진행률은 0으로 설정됩니다.
        /// </summary>
        /// <remarks>
        /// 단계 설정 횟수는 Next() 호출 횟수와 한번의 Complete() 호출을 합한 값입니다. 
        /// 횟수를 3으로 설정했을때는 두번의 Next() 와 한번의 Complete() 가 호출되어야만 합니다.
        /// </remarks>
        public void Begin(int steps, string message)
        {
            if (steps <= 0)
                throw new Exception("최소 1단계 이상이 되어야 합니다.");
            this.step = 0;
            this.steps = steps;

            this.Report(0, message);
        }

        public void Begin(int steps, string format, params object[] args)
        {
            this.Begin(steps, string.Format(format, args));
        }

        /// <summary>
        /// 이전 단계를 마무리 하고 다음 단계로 진입합니다.
        /// </summary>
        public void Next()
        {
            this.Next(string.Empty);
        }

        public void Next(string message)
        {
            if (this.step >= this.steps)
                throw new Exception(string.Format("단계가 지정된 횟수({0})보다 초과되었습니다.", this.step));
            this.step++;
            this.Report((double)this.step / this.steps, message);
        }

        public void Next(string format, params object[] args)
        {
            this.Next(string.Format(format, args));
        }

        /// <summary>
        /// 단계를 마무리 합니다. 지정된 단계 횟수보다 적은 상태에서 호출되었을때는 예외를 발생합니다.
        /// </summary>
        public void Complete()
        {
            this.Complete(string.Empty);
        }

        public void Complete(string message)
        {
            if (this.step + 1 < this.steps)
                throw new Exception(string.Format("{0} 단계중 {1} 까지만 진행되었습니다.", this.steps, this.step));
            this.Complete(false, message);
            this.step = 0;
            this.steps = 0;
        }

        public void Complete(string format, params object[] args)
        {
            this.Complete(string.Format(format, args));
        }

        /// <summary>
        /// 단계를 건너뛴 상태로 마무리 합니다.
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
            this.Report((double)this.step / this.steps, message);
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
            this.step = 0;
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

        public int Step => this.step;

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
            var v = (double)this.step / this.steps + (1.0 / this.steps);
            this.OnChanged(new ProgressChangedEventArgs(v, message));

            if (this.progress != null)
            {
                this.progress.Report(1.0, message);
            }
        }

        void IProgress.Fail(string message)
        {
            var v = (double)this.step / this.steps + (1.0 / this.steps);
            this.OnChanged(new ProgressChangedEventArgs(v, message));

            if (this.progress != null)
            {
                this.progress.Report(1.0, message);
            }
        }

        void IProgress.Report(double value, string message)
        {
            var v = (double)this.step / this.steps + (1.0 / this.steps) * value;
            this.OnChanged(new ProgressChangedEventArgs(v, message));

            if (this.progress != null)
            {
                this.progress.Report(value, message);
            }
        }

        #endregion
    }
}
