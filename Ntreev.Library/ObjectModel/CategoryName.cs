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
using Ntreev.Library.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ntreev.Library.ObjectModel
{
    public class CategoryName
    {
        private string name;
        private string parentPath;
        private string path;

        public CategoryName(string path)
        {
            this.Path = path;
        }

        public CategoryName(string parentPath, string name)
        {
            this.MakePathCore(parentPath, name);
        }

        public static CategoryName Create(params string[] names)
        {
            if (names.Length == 0)
                return new CategoryName(PathUtility.Separator);
            if (names.Length == 1 && names[0] == string.Empty)
                return new CategoryName(PathUtility.Separator);
            return new CategoryName(PathUtility.Separator + string.Join(PathUtility.Separator, names) + PathUtility.Separator);
        }

        public override string ToString()
        {
            return this.path ?? string.Empty;
        }

        public string Path
        {
            get => this.path;
            set
            {
                NameValidator.ValidateCategoryPath(value);

                this.path = value;
                value = value.Remove(value.Length - 1);
                var index = value.LastIndexOf(PathUtility.SeparatorChar);
                this.parentPath = value.Substring(0, index + 1);
                if (this.parentPath == string.Empty)
                    this.parentPath = PathUtility.Separator;
                this.name = value.Substring(index + 1);
            }
        }

        public string ParentPath => this.parentPath;

        public string Name
        {
            get => this.name;
            set
            {
                this.name = value;
                this.path = MakePath(this.parentPath, this.name);
            }
        }

        public string[] Segments => this.path.Split(new char[] { PathUtility.SeparatorChar }, StringSplitOptions.RemoveEmptyEntries);

        public static implicit operator string(CategoryName categoryName)
        {
            return categoryName.path;
        }

        internal static string MakePath(string parentPath, string name)
        {
            NameValidator.ValidateCategoryPath(parentPath);
            NameValidator.ValidateName(name);
            return parentPath + name + PathUtility.Separator;
        }

        private void MakePathCore(string parentPath, string name)
        {
            NameValidator.ValidateCategoryPath(parentPath);
            NameValidator.ValidateName(name);
            this.parentPath = parentPath;
            this.name = name;
            this.path = parentPath + name + PathUtility.Separator;
        }

        public static string[] MakeItemList(string[] items)
        {
            return MakeItemList(items, false);
        }

        public static string[] MakeItemList(string[] items, bool categoryOnly)
        {
            var query = from item in items
                        from parent in QueryParents(item)
                        select parent;

            var result = query.Concat(items)
                              .Distinct()
                              .Where(item => categoryOnly != true || NameValidator.VerifyCategoryPath(item))
                              .OrderBy(item => item)
                              .ToArray();

            if (result.Any() == true)
                return result;
            return new string[] { PathUtility.Separator };

            static IEnumerable<string> QueryParents(string path)
            {
                return EnumerableUtility.Ancestors(path, item =>
                {
                    if (item == PathUtility.Separator)
                        return null;
                    if (NameValidator.VerifyItemPath(item) == true)
                        return new ItemName(item).CategoryPath;
                    return new CategoryName(item).ParentPath;
                });
            }
        }
    }
}
