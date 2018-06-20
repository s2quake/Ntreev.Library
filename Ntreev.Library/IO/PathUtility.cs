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

using Ntreev.Library.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Ntreev.Library.IO
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

        /// <summary>
        /// 지정된 경로에 임시 폴더를 생성합니다.
        /// </summary>
        public static string GetTempPath(string targetPath, bool create)
        {
            var tempFileName = Path.GetTempFileName();
            File.Delete(tempFileName);
            Thread.Sleep(1);
            var directoryName = Path.GetDirectoryName(tempFileName);
            var fileName = Path.GetFileNameWithoutExtension(tempFileName);
            var tempPath = Path.Combine(targetPath, fileName);
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
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            if (Path.IsPathRooted(path) == false)
                throw new ArgumentException("path must be full path", nameof(path));
            var root = Path.GetPathRoot(path);

            root = Directory.GetLogicalDrives().First(item => StringComparer.CurrentCultureIgnoreCase.Equals(root, item));

            try
            {
                foreach (var item in path.Substring(root.Length).Split(Path.DirectorySeparatorChar))
                {
                    root = Directory.GetFileSystemEntries(root, item).First();
                }
            }
            catch
            {
                root += path.Substring(root.Length);
            }
            return root;
        }

        public static string GetFullPath(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            if (Environment.OSVersion.Platform == PlatformID.Unix && path.StartsWith("~", StringComparison.CurrentCulture) == true)
            {
                var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                return Regex.Replace(path, "^~(.*)", $"{home}$1");
            }
            else
            {
                return Path.GetFullPath(path);
            }
        }
    }
}
