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
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace JSSoft.Library.IO.Virtualization.Memory
{
    public class MemoryStorage : ItemContext<MemoryFile, MemoryFolder, MemoryFileCollection, MemoryFolderCollection, MemoryStorage>, IStorage
    {
        public MemoryStorage()
            : this(string.Empty)
        {

        }

        public MemoryStorage(string name)
        {
            this.Name = name ?? string.Empty;
        }

        public override string ToString()
        {
            return this.Name;
        }

        public string Name { get; }

        public Uri Uri { get; } = null;

        internal string GetHashValue(MemoryFile file)
        {
            using SHA256 hashBuilder = SHA256.Create();
            using Stream stream = new MemoryStream(file.Data);
            byte[] data = hashBuilder.ComputeHash(stream);

            StringBuilder sBuilder = new StringBuilder();

            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            return sBuilder.ToString();
        }

        #region IStorage

        IFolder IStorage.Root => this.Root;

        IFolderCollection IStorage.Folders => this.Categories;

        IFileCollection IStorage.Files => this.Items;

        #endregion
    }
}
