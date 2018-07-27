﻿using Ntreev.Library.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ntreev.Library
{
    public class CommandHost : IEnumerable<object>
    {
        private readonly string filename;
        private readonly string workingPath;
        private readonly string commandName;
        private readonly List<object> items = new List<object>();
        private List<string> outputList;
        private List<string> errorList;
        private Encoding encoding;
        private Func<object> action;
        private object result;

        public CommandHost(string filename, string workingPath, string commandName)
        {
            this.filename = filename;
            this.workingPath = workingPath;
            this.commandName = commandName;
            this.ThrowOnError = true;
        }

        public override string ToString()
        {
            return $"{this.filename} {this.commandName} {string.Join(" ", this.items)}";
        }

        public void Add(object item)
        {
            this.items.Add(item);
        }

        public bool WriteAllText(string path)
        {
            this.action = new Func<object>(() =>
            {
                var process = new Process();
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.FileName = this.filename;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.WorkingDirectory = this.workingPath;
                process.StartInfo.Arguments = $"{this.commandName} {string.Join(" ", this.items.Where(item => item != null))}";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.StandardOutputEncoding = this.Encoding;
                process.StartInfo.StandardErrorEncoding = this.Encoding;
                process.Start();
                var outputText = process.StandardOutput.ReadToEnd();
                var errorText = process.StandardError.ReadToEnd();
                FileUtility.WriteAllText(outputText, this.Encoding, path);
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    if (this.ThrowOnError == true)
                        throw new Exception(errorText);
                    else
                        return false;
                }
                return true;
            });
            this.OnRun();
            return (bool)this.result;
        }

        public string Run()
        {
            this.action = new Func<object>(() =>
            {
                this.outputList = new List<string>();
                this.errorList = new List<string>();
                var process = new Process();
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.FileName = this.filename;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.WorkingDirectory = this.workingPath;
                process.StartInfo.Arguments = $"{this.commandName} {string.Join(" ", this.items.Where(item => item != null))}";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.StandardOutputEncoding = this.Encoding;
                process.StartInfo.StandardErrorEncoding = this.Encoding;

                process.OutputDataReceived += (s, e) =>
                {
                    this.outputList.Add(e.Data);
                    this.OutputDataReceived?.Invoke(this, e);
                };
                process.ErrorDataReceived += (s, e) =>
                {
                    this.errorList.Add(e.Data);
                    this.ErrorDataReceived?.Invoke(this, e);
                };
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    if (this.ThrowOnError == true)
                        throw new Exception(string.Join(Environment.NewLine, this.errorList.Where(item => item != null)));
                    else
                        return null as string;
                }

                return string.Join(Environment.NewLine, this.outputList.Where(item => item != null));
            });
            this.OnRun();
            return (string)this.result;
        }

        public string ReadLine()
        {
            var lines = this.ReadLines(true);
            return lines?.Single();
        }

        public string[] ReadLines()
        {
            return this.ReadLines(false);
        }

        public string[] ReadLines(bool removeEmptyLine)
        {
            var lines = this.Run();
            if (lines == null)
                return null;
            return this.GetLines(lines, removeEmptyLine);
        }

        public Encoding Encoding
        {
            get => this.encoding ?? Encoding.UTF8;
            set => this.encoding = value;
        }

        public bool ThrowOnError { get; set; }

        public string ErrorMessage { get => this.errorList == null ? string.Empty : string.Join(Environment.NewLine, this.errorList.Where(item => item != null)); }

        public string Message { get => this.outputList == null ? string.Empty : string.Join(Environment.NewLine, this.outputList.Where(item => item != null)); }

        public IReadOnlyList<object> Items => this.items;

        public event DataReceivedEventHandler ErrorDataReceived;

        public event DataReceivedEventHandler OutputDataReceived;

        protected virtual void OnRun()
        {
            this.result = this.action();
        }

        private string[] GetLines(string text, bool removeEmptyLine)
        {
            using (var sr = new StringReader(text))
            {
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
