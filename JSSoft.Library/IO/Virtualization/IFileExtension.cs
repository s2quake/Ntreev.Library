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
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JSSoft.Library.IO.Virtualization
{
    public static class IFileExtension
    {
        public static string ReadAllText(this IFile file)
        {
            return ReadAllText(file, Encoding.UTF8);
        }

        public static string ReadAllText(this IFile file, Encoding encoding)
        {
            using var stream = file.OpenRead();
            using var reader = new StreamReader(stream, encoding);
            return reader.ReadToEnd();
        }

        public static IEnumerable<string> ReadAllLines(this IFile file)
        {
            return ReadAllLines(file, Encoding.UTF8);
        }

        public static IEnumerable<string> ReadAllLines(this IFile file, Encoding encoding)
        {
            var lines = new List<string>();
            using (var reader = new StringReader(file.ReadAllText(encoding)))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    lines.Add(line);
                }
            }

            return lines;
        }

        public static void WriteAllText(this IFile file, string contents)
        {
            WriteAllText(file, contents, Encoding.UTF8);
        }

        public static void WriteAllText(this IFile file, string contents, Encoding encoding)
        {
            using var stream = file.OpenWrite();
            using var writer = new StreamWriter(stream, encoding);
            writer.Write(contents);
        }

        public static void WriteAllLines(this IFile file, IEnumerable<string> contents)
        {
            WriteAllLines(file, contents, Encoding.UTF8);
        }

        public static void WriteAllLines(this IFile file, IEnumerable<string> contents, Encoding encoding)
        {
            using var stream = file.OpenWrite();
            using var writer = new StreamWriter(stream, encoding);
            foreach (var item in contents)
            {
                writer.WriteLine(item);
            }
        }

        public static void Modify(this IFile file, IFile sourceFile)
        {
            using var readStream = sourceFile.OpenRead();
            using var writeStream = file.OpenWrite();
            readStream.CopyTo(writeStream);
        }

        public static string ToLocalPath(this IFile file)
        {
            return ToLocalPath(file, file.Storage.Root);
        }

        public static string ToLocalPath(this IFile file, IFolder rootFolder)
        {
            var index = file.Path.IndexOf(rootFolder.Path, StringComparison.OrdinalIgnoreCase);
            if (index < 0)
                return file.Path;
            return file.Path.Remove(index, rootFolder.Path.Length);
        }

        public static string GetRepositoryPath(this IFile file)
        {
            return file.ToLocalPath().Replace(PathUtility.Separator, "_");
        }
    }
}
