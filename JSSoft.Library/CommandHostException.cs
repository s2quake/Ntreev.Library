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
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace JSSoft.Library
{
    public class CommandHostException : Exception
    {
        public CommandHostException(string errorMessage, string commandLine, string workingDirectory)
            : this(errorMessage, commandLine, workingDirectory, null)
        {

        }

        public CommandHostException(string errorMessage, string commandLine, string workingDirectory, Exception innerException)
            : base(GenearteMessage(errorMessage, commandLine, workingDirectory), innerException)
        {
            this.ErrorMessage = errorMessage ?? throw new ArgumentNullException(nameof(errorMessage));
            this.CommandLine = commandLine ?? throw new ArgumentNullException(nameof(commandLine));
            this.WorkingDirectory = workingDirectory ?? throw new ArgumentNullException(nameof(workingDirectory));
        }

        public string ErrorMessage { get; }

        public string CommandLine { get; }

        public string WorkingDirectory { get; }

        private static string GenearteMessage(string errorMessage, string commandLine, string workingDirectory)
        {
            var sb = new StringBuilder();
            sb.AppendLine(errorMessage);
            sb.AppendLine();
            sb.AppendLine($"CommandLine: {commandLine}");
            sb.AppendLine($"WorkingDirectory: {workingDirectory}");
            return sb.ToString();
        }
    }
}
