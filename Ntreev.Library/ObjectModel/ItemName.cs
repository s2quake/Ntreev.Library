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
using System;

namespace Ntreev.Library.ObjectModel
{
    public class ItemName
    {
        private string name;
        private string categoryPath;
        private string path;

        public ItemName(string path)
        {
            this.Path = path;
        }

        public ItemName(string categoryPath, string name)
        {
            this.MakePathCore(categoryPath, name);
        }

        public static ItemName Create(params string[] names)
        {
            return new ItemName(PathUtility.Separator + string.Join(PathUtility.Separator, names));
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
                NameValidator.ValidateItemPath(value);

                this.path = value;
                var index = value.LastIndexOf(PathUtility.SeparatorChar);
                this.categoryPath = value.Substring(0, index + 1);
                if (this.categoryPath == string.Empty)
                    this.categoryPath = PathUtility.Separator;
                this.name = value.Substring(index + 1);
            }
        }

        public string CategoryPath => this.categoryPath;

        public string Name
        {
            get => this.name;
            set
            {
                this.name = value;
                this.path = MakePath(this.categoryPath, this.name);
            }
        }

        public string[] Segments => this.path.Split(new char[] { PathUtility.SeparatorChar }, StringSplitOptions.RemoveEmptyEntries);

        public static implicit operator string(ItemName itemName)
        {
            return itemName.path;
        }

        internal static string MakePath(string categoryPath, string name)
        {
            NameValidator.ValidateCategoryPath(categoryPath);
            NameValidator.ValidateName(name);
            return categoryPath + name;
        }

        private void MakePathCore(string categoryPath, string name)
        {
            NameValidator.ValidateCategoryPath(categoryPath);
            NameValidator.ValidateName(name);
            this.categoryPath = categoryPath;
            this.name = name;
            this.path = categoryPath + name;
        }
    }
}
