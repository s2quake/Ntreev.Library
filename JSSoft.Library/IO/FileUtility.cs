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

using JSSoft.Library.ObjectModel;
using JSSoft.Library.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace JSSoft.Library.IO
{
    public static class FileUtility
    {
        private const string backupPostFix = ".bak";

        public static void Copy(string sourceFileName, string destFileName)
        {
            var isExistPrevImage = false;
            var attr = FileAttributes.Archive;
            if (File.Exists(destFileName) == true)
            {
                attr = File.GetAttributes(destFileName);
                File.SetAttributes(destFileName, FileAttributes.Archive);
                isExistPrevImage = true;
            }

            File.Copy(sourceFileName, destFileName, true);

            if (isExistPrevImage == true)
                File.SetAttributes(destFileName, attr);
        }

        public static void SetAttribute(string path, FileAttributes fileAttributes)
        {
            string[] dirs = Directory.GetDirectories(path);
            foreach (string dir in dirs)
            {
                SetAttribute(dir, fileAttributes);
            }

            string[] files = Directory.GetFiles(path);
            foreach (string file in files)
            {
                File.SetAttributes(file, fileAttributes);
            }
        }

        public static void OverwriteString(string filename, string text)
        {
            FileInfo file = new FileInfo(filename);
            file.MoveTo(file.FullName + ".bak");

            using (StreamWriter sw = new StreamWriter(filename, false, Encoding.UTF8))
            {
                sw.Write(text);
            }

            file.Delete();
        }

        public static string GetString(string filename)
        {
            using StreamReader sr = new StreamReader(filename);
            return sr.ReadToEnd();
        }

        public static string ToAbsolutePath(string path)
        {
            Uri uri = new Uri(path, UriKind.RelativeOrAbsolute);
            if (uri.IsAbsoluteUri == false)
            {
                return Path.Combine(Directory.GetCurrentDirectory(), path);
            }

            return uri.LocalPath;
        }

        public static string ToLocalPath(string path, string dirPath)
        {
            if (path.IndexOf(ToAbsolutePath(dirPath), StringComparison.CurrentCultureIgnoreCase) < 0)
                throw new ArgumentException(string.Format(Resources.Exception_InvalidPath_Format, path), nameof(path));
            if (path == dirPath)
                return string.Empty;

            var pathUri = new Uri(path);
            if (dirPath.Last() != Path.DirectorySeparatorChar)
                dirPath += Path.DirectorySeparatorChar;
            var dirUri = new Uri(dirPath);
            var relativeUri = dirUri.MakeRelativeUri(pathUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString()).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            return relativePath;
        }

        public static bool IsFile(string path)
        {
            return DirectoryUtility.IsDirectory(path) == false;
        }

        public static string GetHash(string filename)
        {
            using var sha = new SHA256CryptoServiceProvider();
            using var stream = File.OpenRead(filename);
            var bytes = sha.ComputeHash(stream);

            var sBuilder = new StringBuilder();
            for (var i = 0; i < bytes.Length; i++)
            {
                sBuilder.Append(bytes[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }

        public static void Prepare(string filename)
        {
            var fileInfo = new FileInfo(filename);
            var directory = Path.GetDirectoryName(fileInfo.FullName);
            if (Directory.Exists(directory) == false)
                Directory.CreateDirectory(directory);
        }

        public static string Prepare(params string[] paths)
        {
            var filename = Path.Combine(paths);
            var fileInfo = new FileInfo(filename);
            var directory = Path.GetDirectoryName(fileInfo.FullName);
            if (Directory.Exists(directory) == false)
                Directory.CreateDirectory(directory);
            return fileInfo.FullName;
        }

        public static string GetFilename(string basePath, IItem item)
        {
            var path1 = basePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar).TrimEnd(Path.DirectorySeparatorChar);
            var path2 = item.Path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar);
            return Path.Combine(path1, path2);
        }

        public static string GetFilename(string basePath, IItem item, string extension)
        {
            var path1 = basePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar).TrimEnd(Path.DirectorySeparatorChar);
            var path2 = item.Path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar);
            return Path.Combine(path1, path2, extension);
        }

        public static string ChangeExtension(string filename, string extension)
        {
            return Path.ChangeExtension(filename, extension);
        }

        public static string RemoveExtension(string filename)
        {
            string extension = Path.GetExtension(filename);
            return filename.Substring(0, filename.Length - extension.Length);
        }

        public static bool IsAbsolute(string filename)
        {
            return new Uri(filename, UriKind.RelativeOrAbsolute).IsAbsoluteUri;
        }

        public static void Delete(string filename)
        {
            if (File.Exists(filename) == true)
                File.Delete(filename);
        }

        public static void Delete(params string[] paths)
        {
            FileUtility.Delete(Path.Combine(paths));
        }

        public static void Clean(string filename)
        {
            var backupPath = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileName(filename) + backupPostFix);
            FileUtility.Delete(backupPath);
        }

        public static void Backup(string filename)
        {
            if (File.Exists(filename) == false)
                return;
            var backupPath = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileName(filename) + backupPostFix);
            FileUtility.Delete(backupPath);
            FileUtility.Copy(filename, backupPath);
            FileUtility.Delete(filename);
        }

        public static void Restore(string filename)
        {
            var backupPath = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileName(filename) + backupPostFix);
            if (File.Exists(backupPath) == false)
                return;
            FileUtility.Delete(filename);
            FileUtility.Copy(backupPath, filename);
            FileUtility.Delete(backupPath);
        }

        public static bool Exists(string filename)
        {
            return File.Exists(filename);
        }

        public static bool Exists(params string[] paths)
        {
            return File.Exists(Path.Combine(paths));
        }

        public static string ReadAllText(string filename)
        {
            return File.ReadAllText(filename);
        }

        public static string ReadAllText(params string[] paths)
        {
            return File.ReadAllText(Path.Combine(paths));
        }

        public static string[] ReadAllLines(string filename)
        {
            return File.ReadAllLines(filename);
        }

        public static string[] ReadAllLines(params string[] paths)
        {
            return File.ReadAllLines(Path.Combine(paths));
        }

        public static string WriteAllLines(string[] contents, params string[] paths)
        {
            var filename = FileUtility.Prepare(paths);
            File.WriteAllLines(filename, contents);
            return filename;
        }

        public static string WriteAllLines(IEnumerable<string> contents, params string[] paths)
        {
            var filename = FileUtility.Prepare(paths);
            File.WriteAllLines(filename, contents);
            return filename;
        }

        public static string WriteAllText(string contents, params string[] paths)
        {
            var filename = FileUtility.Prepare(paths);
            File.WriteAllText(filename, contents);
            return filename;
        }

        public static string WriteAllText(string contents, Encoding encoding, params string[] paths)
        {
            var filename = FileUtility.Prepare(paths);
            File.WriteAllText(filename, contents, encoding);
            return filename;
        }

        public static void SetReadOnly(string filename, bool isReadOnly)
        {
            var fileInfo = new FileInfo(filename);
            if (fileInfo.Exists == false)
                throw new FileNotFoundException(filename);
            if (isReadOnly == true)
                fileInfo.Attributes |= FileAttributes.ReadOnly;
            else
                fileInfo.Attributes &= ~FileAttributes.ReadOnly;
        }

        public static void SetVisible(string filename, bool isVisible)
        {
            var fileInfo = new FileInfo(filename);
            if (fileInfo.Exists == false)
                throw new FileNotFoundException(filename);
            if (isVisible == false)
                fileInfo.Attributes |= FileAttributes.Hidden;
            else
                fileInfo.Attributes &= ~FileAttributes.Hidden;
        }

        public static Stream OpenRead(params string[] paths)
        {
            return File.OpenRead(Path.Combine(paths));
        }

        public static Stream OpenWrite(params string[] paths)
        {
            var filename = Path.Combine(paths);
            return new OpenWriteStream(filename);
        }

        #region classes

        class OpenWriteStream : Stream
        {
            private readonly string filename;
            private readonly Stream stream;

            public OpenWriteStream(string filename)
            {
                FileUtility.Prepare(filename);
                FileUtility.Backup(filename);
                this.stream = File.OpenWrite(filename);
                this.filename = filename;
            }

            public override bool CanRead => this.stream.CanRead;

            public override bool CanSeek => this.stream.CanSeek;

            public override bool CanWrite => this.stream.CanWrite;

            public override long Length => this.stream.Length;

            public override long Position { get => this.stream.Position; set => this.stream.Position = value; }

            public override void Flush()
            {
                this.stream.Flush();
                FileUtility.Clean(this.filename);
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                return this.stream.Read(buffer, offset, count);
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                return this.stream.Seek(offset, origin);
            }

            public override void SetLength(long value)
            {
                this.stream.SetLength(value);
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                this.stream.Write(buffer, offset, count);
            }

            public override void Close()
            {
                this.stream.Close();
                base.Close();
            }
        }

        #endregion
    }
}
