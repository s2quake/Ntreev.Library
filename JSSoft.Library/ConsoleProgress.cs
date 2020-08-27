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
using System.IO;

namespace JSSoft.Library
{
    public class ConsoleProgress : Progress
    {
        private readonly TextWriter writer;

        public ConsoleProgress()
            : this(Console.Out)
        {

        }

        public ConsoleProgress(TextWriter writer)
        {
            this.writer = writer;
            this.Style = ConsoleProgressStyle.Percent;
        }

        public static string GetProgressString(int index, int count)
        {
            var len = $"{count}".Length;
            var f = string.Format("{{0,{0}}}", len);
            return string.Format(f, index) + "/" + string.Format(f, count);
        }

        public ConsoleProgressStyle Style
        {
            get; set;
        }

        protected override void OnChanged(ProgressChangedEventArgs e)
        {
            base.OnChanged(e);

            var progressString = this.GetPrgressStringInternal(e.Value) ?? string.Empty;
            if (progressString != string.Empty)
                progressString += " ";
            var message = $"{progressString}{e.Message}";
            if (message != string.Empty)
                this.writer.WriteLine(message);
        }

        protected virtual string GetCustomPrgressString(double value)
        {
            return this.GetPercentString(value);
        }

        protected string GetPercentString(double value)
        {
            return $"[{(value * 100),3}%]";
        }

        protected string GetProgressString(double value, int length)
        {
            var ss = length * value;
            var fill = string.Empty.PadRight((int)ss, '#');
            var empty = string.Empty.PadRight(length - fill.Length, '.');
            return $"[{fill}{empty}]";
        }

        private string GetPrgressStringInternal(double value)
        {
            if (this.Style == ConsoleProgressStyle.None)
            {
                return string.Empty;
            }
            else if (this.Style == ConsoleProgressStyle.Percent)
            {
                return this.GetPercentString(value);
            }
            else if (this.Style == ConsoleProgressStyle.Progress)
            {
                return this.GetProgressString(value, 10);
            }
            else
            {
                return this.GetCustomPrgressString(value);
            }
        }
    }
}
