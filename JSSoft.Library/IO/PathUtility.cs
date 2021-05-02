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

using JSSoft.Library.Properties;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace JSSoft.Library.IO
{
    public static class PathUtility
    {
        public static readonly string Separator = Path.AltDirectorySeparatorChar.ToString();
        public static readonly char SeparatorChar = Path.AltDirectorySeparatorChar;
        public static readonly string Root = Separator;

        public static string ChangeDirectory(string path, string srcPath, string toPath)
        {
            if (path.IndexOf(srcPath, StringComparison.CurrentCultureIgnoreCase) < 0)
                throw new ArgumentException(Resources.Exception_NotIncludedParentPath);

            if (srcPath.Last() != Path.DirectorySeparatorChar)
                srcPath += Path.DirectorySeparatorChar;
            if (toPath.Last() != Path.DirectorySeparatorChar)
                toPath += Path.DirectorySeparatorChar;

            var pathUri = new Uri(path);
            var srcUri = new Uri(srcPath);
            var toUri = new Uri(toPath);
            var relativeUri = srcUri.MakeRelativeUri(pathUri);
            var relativePath = Uri.UnescapeDataString(relativeUri.ToString()).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            return Path.Combine(toUri.LocalPath, relativePath);
        }

        public static string GetTempPath(bool create)
        {
            return GetTempPath(Path.GetTempPath(), create);
        }

        public static string GetTempPath(string targetPath, bool create)
        {
            var tempFileName = Path.GetTempFileName();
            File.Delete(tempFileName);
            Thread.Sleep(1);
            var fileName = Path.GetFileNameWithoutExtension(tempFileName);
            var tempPath = Path.Combine(targetPath, fileName);
            if (Directory.Exists(tempPath) == true)
                Directory.Delete(tempPath, true);
            if (create == true)
                Directory.CreateDirectory(tempPath);
            return tempPath;
        }

        public static string GetTempFileName()
        {
            var tempPath = Path.GetTempFileName();
            File.Delete(tempPath);
            Thread.Sleep(1);
            return Path.Combine(Path.GetTempPath(), tempPath);
        }

        public static string ConvertFromUri(string uri)
        {
            var obj = new Uri(uri, UriKind.RelativeOrAbsolute);

            if (obj.IsAbsoluteUri == true)
                return obj.LocalPath;

            return uri.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        }

        public static string GetCaseSensitivePath(string path)
        {
            if (path is null)
                throw new ArgumentNullException(nameof(path));
            if (Path.IsPathRooted(path) == false)
                throw new ArgumentException("path must be full path", nameof(path));

            var root = Path.GetPathRoot(path);
            var suffix = GetSuffix();
            var localPath = path.Substring(0, path.Length - suffix.Length);
            var fullPath = Directory.GetLogicalDrives().First(item => StringComparer.CurrentCultureIgnoreCase.Equals(root, item));
            try
            {
                var separator = new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
                var items = localPath.Substring(fullPath.Length).Split(separator);
                foreach (var item in items)
                {
                    fullPath = Directory.GetFileSystemEntries(fullPath, item).First();
                }
            }
            catch
            {
                fullPath += path.Substring(fullPath.Length);
            }
            return fullPath + suffix;

            string GetSuffix()
            {
                if (path.EndsWith($"{Path.AltDirectorySeparatorChar}") == true)
                    return $"{Path.AltDirectorySeparatorChar}";
                else if (path.EndsWith($"{Path.DirectorySeparatorChar}") == true)
                    return $"{Path.DirectorySeparatorChar}";
                return string.Empty;
            }
        }

        public static string GetFullPath(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            if (Environment.OSVersion.Platform == PlatformID.Unix && path.StartsWith("~", StringComparison.CurrentCulture) == true)
            {
                var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                path = Regex.Replace(path, "^~(.*)", $"{home}$1");
            }
            path = Environment.ExpandEnvironmentVariables(path);
            path = Path.GetFullPath(path);
            return path;
        }

        public static string GetFullPath(string path, string baseDirectory)
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var exists = Directory.Exists(baseDirectory);
            try
            {
                if (exists == false)
                {
                    Directory.CreateDirectory(baseDirectory);
                }
                Directory.SetCurrentDirectory(baseDirectory);
                return GetFullPath(path);
            }
            finally
            {
                Directory.SetCurrentDirectory(currentDirectory);
                if (exists == false)
                {
                    Directory.Delete(baseDirectory);
                }
            }
        }

        public static string GetDirectoryName(string path)
        {
            if (path.EndsWith(Path.AltDirectorySeparatorChar.ToString()))
                path = path.Remove(path.Length - Path.AltDirectorySeparatorChar.ToString().Length);
            else if (path.EndsWith(Path.DirectorySeparatorChar.ToString()))
                path = path.Remove(path.Length - Path.DirectorySeparatorChar.ToString().Length);
            return Path.GetDirectoryName(path);
        }
    }
}
