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
using System.Text;
using System.IO;
using System.Linq;

namespace Ntreev.Library.IO
{
    public static class DirectoryUtility
    {
        private const string backupPostFix = "$backup$";

        public static void Copy(string sourceFolder, string destFolder)
        {
            var source = new DirectoryInfo(sourceFolder);
            var destination = new DirectoryInfo(destFolder);

            if (destination.Exists == false)
            {
                destination.Create();
            }

            var files = source.GetFiles();
            foreach (var item in files)
            {
                if ((item.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    item.Attributes = FileAttributes.Archive;

                item.CopyTo(Path.Combine(destination.FullName,
                    item.Name), true);
            }

            var dirs = source.GetDirectories();
            foreach (var item in dirs)
            {
                var destinationDir = Path.Combine(destination.FullName, item.Name);
                Copy(item.FullName, destinationDir);
            }
        }

        public static void Create(string path)
        {
            while (Directory.Exists(path) == false)
            {
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch { }
                finally
                {
                    System.Threading.Thread.Sleep(1);
                }
            }
        }

        public static void Move(string sourceDirName, string destDirName)
        {
            var count = 0;
            while (Directory.Exists(sourceDirName) == true && count < 10)
            {
                try
                {
                    ++count;
                    Directory.Move(sourceDirName, destDirName);
                }
                catch
                {
                    if (count >= 10)
                        throw;
                }
                finally
                {
                    System.Threading.Thread.Sleep(1);
                }
            }
        }

        public static void Delete(string path)
        {
            if (Directory.Exists(path) == true)
                FileUtility.SetAttribute(path, FileAttributes.Archive);

            var count = 0;
            while (Directory.Exists(path) == true && count < 10)
            {
                try
                {
                    ++count;
                    Directory.Delete(path, true);
                }
                catch { }
                finally
                {
                    System.Threading.Thread.Sleep(1);
                }
            }
        }

        public static void Delete(params string[] paths)
        {
            Delete(Path.Combine(paths));
        }

        public static bool Exists(string path)
        {
            return Directory.Exists(path);
        }

        public static void PrepareFile(string filename)
        {
            var directory = Path.GetDirectoryName(filename);
            if (Directory.Exists(directory) == false)
                Directory.CreateDirectory(directory);
        }

        public static void Empty(params string[] paths)
        {
            DirectoryUtility.Empty(Path.Combine(paths));
        }

        public static void Empty(string path)
        {
            if (Directory.Exists(path) == false)
                return;

            foreach (var item in Directory.GetFiles(path))
            {
                FileUtility.Delete(item);
            }

            foreach (var item in Directory.GetDirectories(path))
            {
                DirectoryUtility.Delete(item);
            }
        }

        public static bool IsDirectory(string path)
        {
            try
            {
                var attr = File.GetAttributes(path);
                return (attr & FileAttributes.Directory) == FileAttributes.Directory;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsEmpty(string path)
        {
            if (Directory.Exists(path) == false)
                throw new DirectoryNotFoundException(string.Format("'{0}' 경로를 찾을 수 없습니다.", path));

            if (Directory.GetDirectories(path).Any() == true)
                return false;

            return Directory.GetFiles(path).Any() == false;
        }

        public static void Prepare(string path)
        {
            if (Directory.Exists(path) == false)
                Directory.CreateDirectory(path);
        }

        /// <summary>
        /// 주어진 인자를 결합하여 디렉토리 경로를 만들고 해당 디렉토리가 존재하지 않으면 생성합니다.
        /// </summary>
        public static string Prepare(params string[] paths)
        {
            var path = Path.Combine(paths);
            if (Directory.Exists(path) == false)
                Directory.CreateDirectory(path);
            return path;
        }

        public static void Clean(string path)
        {
            var backupPath = Path.Combine(Path.GetDirectoryName(path), Path.GetFileName(path) + backupPostFix);
            DirectoryUtility.Delete(backupPath);
        }

        public static void Backup(string path)
        {
            var backupPath = Path.Combine(Path.GetDirectoryName(path), Path.GetFileName(path) + backupPostFix);
            if (Directory.Exists(path) == false)
                return;
            DirectoryUtility.Clean(path);
            DirectoryUtility.Move(path, backupPath);
            DirectoryUtility.Delete(path);
        }

        public static void Restore(string path)
        {
            var backupPath = Path.Combine(Path.GetDirectoryName(path), Path.GetFileName(path) + backupPostFix);
            if (Directory.Exists(backupPath) == false)
                return;
            DirectoryUtility.Delete(path);
            Directory.Move(backupPath, path);
            DirectoryUtility.Delete(backupPath);
        }

        public static void Copy(string path, Action action)
        {
            DirectoryUtility.Backup(path);
            DirectoryUtility.Prepare(path);

            try
            {
                action();
                DirectoryUtility.Clean(path);
            }
            catch (Exception e)
            {
                DirectoryUtility.Restore(path);
                throw e;
            }
        }

        /// <summary>
        /// 지정된 경로에 상대 또는 절대 경로에 대해서 절대 경로를 계산해 반환합니다.
        /// </summary>
        public static string GetAbsolutePath(string sourcePath, string relativeOrAbsolutePath)
        {
            var sourceDirInfo = new DirectoryInfo(sourcePath);
            var dirInfo = new DirectoryInfo(Path.Combine(sourceDirInfo.FullName, relativeOrAbsolutePath));
            return dirInfo.FullName;
        }

        public static string[] GetAllFiles(string path)
        {
            return DirectoryUtility.GetAllFiles(path, "*.*");
        }

        public static string[] GetAllFiles(string path, string searchPattern)
        {
            return Directory.GetFiles(path, searchPattern, SearchOption.AllDirectories);
        }

        public static string[] GetAllDirectories(string path)
        {
            return DirectoryUtility.GetAllDirectories(path, "*.*");
        }

        public static string[] GetAllDirectories(string path, string searchPattern)
        {
            return Directory.GetDirectories(path, searchPattern, SearchOption.AllDirectories);
        }

        public static string[] GetAllDirectories(string path, string searchPattern, bool removeHiddenDirectory)
        {
            return GetDirectories(path, searchPattern, removeHiddenDirectory, true).ToArray();
        }

        private static IEnumerable<string> GetDirectories(string path, string searchPattern, bool removeHiddenDirectory, bool recursive)
        {
            foreach (var item in Directory.GetDirectories(path, searchPattern, SearchOption.TopDirectoryOnly))
            {
                if (removeHiddenDirectory == true && new DirectoryInfo(item).Attributes.HasFlag(FileAttributes.Hidden) == true)
                    continue;
                yield return item;

                if (recursive == true)
                {
                    foreach (var i in GetDirectories(item, searchPattern, removeHiddenDirectory, recursive))
                    {
                        yield return i;
                    }
                }
            }
        }
    }
}
