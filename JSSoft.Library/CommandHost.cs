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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace JSSoft.Library
{
    public class CommandHost : CommandHostBase
    {
        protected CommandHost(string filename)
            : base(filename)
        {
        }

        protected CommandHost(string filename, string workingPath)
            : base(filename, workingPath)
        {
        }

        protected CommandHost(string filename, string workingPath, string commandName)
            : base(filename, workingPath, commandName)
        {
        }

        public bool TryRun()
        {
            this.InvokeRun();
            if (this.ExitCode != 0)
            {
                return false;
            }
            return true;
        }

        public Task<bool> TryRunAsync()
        {
            return Task.Run(() =>
            {
                this.InvokeRun();
                if (this.ExitCode != 0)
                {
                    return false;
                }
                return true;
            });
        }

        public string Run()
        {
            this.InvokeRun();
            if (this.ExitCode != 0)
            {
                throw new Exception(this.ErrorMessage);
            }
            return this.Message;
        }

        public Task<string> RunAsync()
        {
            return Task.Run(() =>
            {
                this.InvokeRun();
                if (this.ExitCode != 0)
                {
                    throw new Exception(this.ErrorMessage);
                }
                return this.Message;
            });
        }

        public string ReadLine()
        {
            return this.Run();
        }

        public string[] ReadLines()
        {
            return this.ReadLines(false);
        }

        public string[] ReadLines(bool removeEmptyLine)
        {
            if (this.Run() is string text)
                return text.Split(new string[] { Environment.NewLine }, removeEmptyLine == true ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None);
            return null;
        }

        public Task<string> ReadLineAsync()
        {
            return this.RunAsync();
        }

        public Task<string[]> ReadLinesAsync()
        {
            return this.ReadLinesAsync(false);
        }

        public async Task<string[]> ReadLinesAsync(bool removeEmptyLine)
        {
            if (await this.RunAsync() is string text)
                return text.Split(new string[] { Environment.NewLine }, removeEmptyLine == true ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None);
            return null;
        }
    }
}
