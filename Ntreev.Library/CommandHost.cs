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

        public CommandHost(string filename, string workingPath, string commandName)
        {
            this.filename = filename;
            this.workingPath = workingPath;
            this.commandName = commandName;
        }

        public override string ToString()
        {
            return $"{this.filename} {this.commandName} {string.Join(" ", this.items)}";
        }

        public void Add(object item)
        {
            this.items.Add(item);
        }

        public string Run()
        {
            var sb = new StringBuilder();
            var process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.FileName = this.filename;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.WorkingDirectory = this.workingPath;
            process.StartInfo.Arguments = $"{this.commandName} {string.Join(" ", this.items)}";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
            process.OutputDataReceived += (s, e) =>
            {
                sb.AppendLine(e.Data);
            };
            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit();

            if (process.ExitCode != 0)
                throw new Exception(sb.ToString());

            return sb.ToString();
        }

        public string ReadLine()
        {
            var lines = this.ReadLines(true);
            return lines.Single();
        }

        public string[] ReadLines()
        {
            return this.ReadLines(false);
        }

        public string[] ReadLines(bool removeEmptyLine)
        {
            var lines = this.Run();
            return this.GetLines(lines, removeEmptyLine);
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
