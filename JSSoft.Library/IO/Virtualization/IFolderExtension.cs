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
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JSSoft.Library.IO.Virtualization
{
    public static class IFolderExtension
    {
        public static IFile CreateFileAsync(this IFolder folder, string name, Stream stream)
        {
            return folder.CreateFile(name, stream, stream.Length);
        }

        public static IFile CreateFile(this IFolder folder, IFile file, string name)
        {
            using var stream = file.OpenRead();
            return folder.CreateFile(name, stream, file.Size);
        }

        public static IFile CreateFile(this IFolder folder, string name)
        {
            using var stream = new MemoryStream();
            return folder.CreateFile(name, stream, 0);
        }

        public static IFile CreateFile(this IFolder folder, string name, string contents)
        {
            return CreateFile(folder, name, contents, Encoding.UTF8);
        }

        public static IFile CreateFile(this IFolder folder, string name, string contents, Encoding encoding)
        {
            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream, encoding);
            writer.Write(contents);
            writer.Flush();
            stream.Position = 0;
            return folder.CreateFile(name, stream, stream.Length);
        }

        public static IFile CreateFile(this IFolder folder, string name, IEnumerable<string> contents)
        {
            return CreateFile(folder, name, contents, Encoding.UTF8);
        }

        public static IFile CreateFile(this IFolder folder, string name, IEnumerable<string> contents, Encoding encoding)
        {
            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream, encoding);
            foreach (var item in contents)
            {
                writer.WriteLine(item);
            }
            writer.Flush();
            stream.Position = 0;
            return folder.CreateFile(name, stream, stream.Length);
        }

        public static IEnumerable<string> GetPathList(this IFolder folder)
        {
            yield return folder.Path;
            foreach (var item in folder.Folders)
            {
                foreach (var i in GetPathList(item))
                {
                    yield return i;
                }
            }
            foreach (var item in folder.Files)
            {
                yield return item.Path;

            }
        }

        public static IEnumerable<IFile> GetFiles(this IFolder folder)
        {
            foreach (var item in folder.Folders)
            {
                foreach (var i in GetFiles(item))
                {
                    yield return i;
                }
            }
            foreach (var item in folder.Files)
            {
                yield return item;
            }
        }

        public static string ToLocalPath(this IFolder folder)
        {
            return ToLocalPath(folder, folder.Storage.Root);
        }

        public static string ToLocalPath(this IFolder folder, IFolder rootFolder)
        {
            var index = folder.Path.IndexOf(rootFolder.Path, StringComparison.OrdinalIgnoreCase);
            if (index < 0)
                return folder.Path;
            return folder.Path.Remove(index, rootFolder.Path.Length);
        }
    }
}
