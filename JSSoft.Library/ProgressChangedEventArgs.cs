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

namespace JSSoft.Library
{
    public class ProgressChangedEventArgs : EventArgs
    {
        public ProgressChangedEventArgs(bool isFailed, string message)
        {
            this.State = isFailed ? ProgressChangeState.Failed : ProgressChangeState.Completed;
            this.Value = isFailed ? double.MinValue : 1.0;
            this.Message = message;
        }

        public ProgressChangedEventArgs(bool isFailed, string format, params object[] args)
            : this(isFailed, string.Format(format, args))
        {

        }

        public ProgressChangedEventArgs(double value, string message)
        {
            if (value > 1.0 || value < 0.0)
                throw new ArgumentOutOfRangeException("value", value, "0 과 1사이의 값을 사용해야 합니다.");
            this.State = ProgressChangeState.Changed;
            this.Value = value;
            this.Message = message;
        }

        public ProgressChangedEventArgs(double value, string format, params object[] args)
            : this(value, string.Format(format, args))
        {

        }

        public ProgressChangeState State { get; }

        public double Value { get; }

        public string Message { get; }
    }
}
