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
    public abstract class CommandHostBase : IEnumerable<object>
    {
        private readonly string filename;
        private readonly string workingPath;
        private readonly string commandName;
        private readonly List<object> items = new();
        private readonly StringBuilder error = new();
        private readonly StringBuilder output = new();
        private Encoding encoding = Console.OutputEncoding;

        protected CommandHostBase(string filename)
            : this(filename, Directory.GetCurrentDirectory())
        {
        }

        protected CommandHostBase(string filename, string workingPath)
            : this(filename, workingPath, string.Empty)
        {
        }

        protected CommandHostBase(string filename, string workingPath, string commandName)
        {
            this.filename = filename;
            this.workingPath = workingPath;
            this.commandName = commandName;
        }

        public override string ToString()
        {
            return $"{GenerateFilename()} {GenerateArguments()}";
        }

        public void Add(object item)
        {
            this.items.Add(item);
        }

        public void Remove(object item)
        {
            this.items.Remove(item);
        }

        public void Clear()
        {
            this.items.Clear();
        }

        public Encoding Encoding
        {
            get => this.encoding;
            set => this.encoding = value ?? throw new ArgumentNullException(nameof(value));
        }

        public string ErrorMessage => this.error.ToString();

        public string Message => this.output.ToString();

        public int ExitCode { get; private set; }

        public IReadOnlyList<object> Items => this.items;

        public event DataReceivedEventHandler ErrorDataReceived;

        public event DataReceivedEventHandler OutputDataReceived;

        protected virtual void OnBeforeRun()
        {
        }

        protected virtual void OnRun()
        {
            this.error.Clear();
            this.output.Clear();
            this.ExitCode = this.StartProcess();
        }

        protected virtual void OnAfterRun()
        {
        }

        protected virtual void OnErrorDataReceived(DataReceivedEventArgs e)
        {
            this.error.AppendLine(e.Data);
            this.ErrorDataReceived?.Invoke(this, e);
        }

        protected virtual void OnOutputDataReceived(DataReceivedEventArgs e)
        {
            this.output.AppendLine(e.Data);
            this.OutputDataReceived?.Invoke(this, e);
        }

        private string GenerateFilename()
        {
            if (Regex.IsMatch(this.filename, @"\s") == true)
                return this.filename.WrapQuot();
            return this.filename;
        }

        private string GenerateArguments()
        {
            if (this.commandName == string.Empty)
                return $"{string.Join(" ", this.items.Where(item => item != null))}";
            return $"{this.commandName} {string.Join(" ", this.items.Where(item => item != null))}"; ;
        }

        private int StartProcess()
        {
            var process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.FileName = this.filename;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.WorkingDirectory = this.workingPath;
            process.StartInfo.Arguments = this.GenerateArguments();
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.StandardOutputEncoding = this.Encoding;
            process.StartInfo.StandardErrorEncoding = this.Encoding;

            process.OutputDataReceived += (s, e) => this.OnOutputDataReceived(e);
            process.ErrorDataReceived += (s, e) => this.OnErrorDataReceived(e);
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
            return process.ExitCode;
        }

        protected void InvokeRun()
        {
            this.OnBeforeRun();
            this.OnRun();
            this.OnAfterRun();
        }

        #region IEnumerable

        IEnumerator<object> IEnumerable<object>.GetEnumerator()
        {
            foreach (var item in this.items)
            {
                yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var item in this.items)
            {
                yield return item;
            }
        }

        #endregion
    }
}
