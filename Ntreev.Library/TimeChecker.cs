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
    [Obsolete]
    public class TimeChecker : IDisposable
    {
        private readonly DateTime time;
        private readonly ReportAction reportAction;
        private DateTime lastestTime;

        public TimeChecker(ReportAction reportAction)
        {
            this.time = this.lastestTime = DateTime.Now;
            this.reportAction = reportAction;
        }

        public TimeChecker()
            : this(Report)
        {

        }

        public void Lap()
        {
            this.Lap(string.Empty);

        }

        public void Lap(string message)
        {
            DateTime time = DateTime.Now;
            this.reportAction(message, this.time, this.lastestTime, time);
            this.lastestTime = time;
        }

        public void Dispose()
        {
            this.Lap("Ended");
        }

        public delegate void ReportAction(string message, DateTime startTime, DateTime latestTime, DateTime time);

        private static void Report(string message, DateTime startTime, DateTime latestTime, DateTime time)
        {
            if (string.IsNullOrEmpty(message) == false)
                Console.WriteLine("{0}({1}) : {2}", time - startTime, time - latestTime, message);
            else
                Console.WriteLine("{0}({1})", time - startTime, time - latestTime);
        }
    }
}
