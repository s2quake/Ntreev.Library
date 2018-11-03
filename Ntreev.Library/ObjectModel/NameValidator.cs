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

using Ntreev.Library.IO;
using Ntreev.Library.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Ntreev.Library.ObjectModel
{
    public static class NameValidator
    {
        public static bool VerifyName(string name)
        {
            if (string.IsNullOrEmpty(name) == true)
                return false;
            return name.IndexOfAny(InvalidChars) == -1;
        }

        public static void ValidateName(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (name == string.Empty)
                throw new ArgumentException(Resources.Exception_EmptyStringCannotBeUsedAsName);
            if (VerifyName(name) == false)
                throw new ArgumentException(Resources.Exception_InvalidName, nameof(name));
        }

        public static bool VerifyCategoryPath(string categoryPath)
        {
            if (categoryPath == PathUtility.Separator)
                return true;

            if (categoryPath.StartsWith(PathUtility.Separator) == false)
                return false;

            if (categoryPath.EndsWith(PathUtility.Separator) == false)
                return false;

            var ss = StringUtility.Split(categoryPath, PathUtility.SeparatorChar, false);

            foreach (var item in ss.Where(i => i != string.Empty))
            {
                if (VerifyName(item) == false)
                    return false;
            }

            var emptyCount = ss.Where(item => item == string.Empty).Count();

            return emptyCount == 2;
        }

        public static void ValidateCategoryPath(string categoryPath)
        {
            if (categoryPath == null)
                throw new ArgumentNullException(nameof(categoryPath));
            if (VerifyCategoryPath(categoryPath) == false)
                throw new ArgumentException(string.Format(Resources.Exception_InvalidPath_Format, categoryPath), nameof(categoryPath));
        }

        public static bool VerifyItemPath(string itemPath)
        {
            if (itemPath.StartsWith(PathUtility.Separator) == false)
                return false;

            if (itemPath.EndsWith(PathUtility.Separator) == true)
                return false;

            var ss = StringUtility.Split(itemPath, PathUtility.SeparatorChar, false);

            foreach (var item in ss.Where(i => i != string.Empty))
            {
                if (VerifyName(item) == false)
                    return false;
            }

            var emptyCount = ss.Where(item => item == string.Empty).Count();

            return emptyCount == 1;
        }

        public static void ValidateItemPath(string itemPath)
        {
            if (itemPath == null)
                throw new ArgumentNullException(nameof(itemPath));
            if (VerifyItemPath(itemPath) == false)
                throw new ArgumentException(string.Format(Resources.Exception_InvalidPath_Format, itemPath), nameof(itemPath));
        }

        public static bool VerifyPath(string path)
        {
            if (VerifyCategoryPath(path) == true)
                return true;
            if (VerifyItemPath(path) == true)
                return true;
            return false;
        }

        public static void ValidatePath(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            if (VerifyPath(path) == false)
                throw new ArgumentException(string.Format(Resources.Exception_InvalidPath_Format, path), nameof(path));
        }

        public static char[] InvalidChars { get; } = new char[] { '\\', '/', ':', '*', '?', '\"', '<', '>', '|' };
    }
}