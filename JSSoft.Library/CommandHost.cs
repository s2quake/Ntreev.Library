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
using System.Threading.Tasks;

namespace JSSoft.Library
{
    public class CommandHost : IEnumerable<object>
    {
        private readonly string filename;
        private readonly string workingPath;
        private readonly string commandName;
        private readonly List<object> items = new();
        private readonly StringBuilder error = new();
        private readonly StringBuilder output = new();
        private Encoding encoding;
        private int exitCode;

        public CommandHost(string filename)
            : this(filename, Directory.GetCurrentDirectory())
        {
        }

        public CommandHost(string filename, string workingPath)
            : this(filename, workingPath, string.Empty)
        {
        }

        public CommandHost(string filename, string workingPath, string commandName)
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

        public bool TryRun()
        {
            var action = this.CreateAction();
            this.error.Clear();
            this.output.Clear();
            this.OnBeforeRun();
            this.exitCode = action();
            this.OnAfterRun();
            if (this.exitCode != 0)
            {
                return false;
            }
            return true;
        }

        public async Task<bool> TryRunAsync()
        {
            var action = this.CreateAction();
            this.error.Clear();
            this.output.Clear();
            this.OnBeforeRun();
            this.exitCode = await Task.Run(action);
            this.OnAfterRun();
            if (this.exitCode != 0)
            {
                return false;
            }
            return true;
        }

        public string Run()
        {
            var action = this.CreateAction();
            this.error.Clear();
            this.output.Clear();
            this.OnBeforeRun();
            this.exitCode = action();
            this.OnAfterRun();
            if (this.exitCode != 0)
            {
                throw new Exception(this.error.ToString());
            }
            return this.output.ToString();
        }

        public async Task<string> RunAsync()
        {
            var action = this.CreateAction();
            this.error.Clear();
            this.output.Clear();
            this.OnBeforeRun();
            this.exitCode = await Task.Run(action);
            this.OnAfterRun();
            if (this.exitCode != 0)
            {
                throw new Exception(this.error.ToString());
            }
            return this.output.ToString();
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

        public Encoding Encoding
        {
            get => this.encoding ?? Encoding.UTF8;
            set => this.encoding = value;
        }

        public string ErrorMessage => this.error.ToString();

        public string Message => this.output.ToString();

        public int ExitCode => this.exitCode;

        public IReadOnlyList<object> Items => this.items;

        public event DataReceivedEventHandler ErrorDataReceived;

        public event DataReceivedEventHandler OutputDataReceived;

        protected virtual void OnBeforeRun()
        {
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

        private string[] GetLines(string text, bool removeEmptyLine)
        {
            using var sr = new StringReader(text);
            var line = null as string;
            var lineList = new List<string>();
            while ((line = sr.ReadLine()) != null)
            {
                if (line.Trim() != string.Empty || removeEmptyLine == false)
                {
                    lineList.Add(line);
                }
            }
            return lineList.ToArray();
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

        private Func<int> CreateAction()
        {
            return new Func<int>(() =>
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
            });
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
