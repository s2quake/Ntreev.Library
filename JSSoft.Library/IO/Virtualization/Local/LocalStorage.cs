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

namespace JSSoft.Library.IO.Virtualization.Local
{
    public class LocalStorage : ItemContext<LocalFile, LocalFolder, LocalFileCollection, LocalFolderCollection, LocalStorage>, IStorage
    {
        public LocalStorage(Uri uri)
        {
            this.Uri = uri;
            this.LoadCategories(this.Root);
        }

        public override string ToString()
        {
            return this.LocalPath;
        }

        public string LocalPath => this.Uri.LocalPath;

        public string Name => this.LocalPath;

        public Uri Uri { get; }

        internal string GetHashValue(LocalFile file)
        {
            using var hashBuilder = SHA256.Create();
            using var stream = File.OpenRead(file.LocalPath);
            var data = hashBuilder.ComputeHash(stream);

            var sBuilder = new StringBuilder();

            for (var i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            return sBuilder.ToString();
        }

        private void LoadCategories(LocalFolder parentCategory)
        {
            var path = parentCategory.LocalPath;
            var dirs = Directory.GetDirectories(path);
            var files = Directory.GetFiles(path);

            foreach (var item in files)
            {
                var fileInfo = new FileInfo(item);
                var table = new LocalFile()
                {
                    Name = fileInfo.Name,
                    ModifiedDateTime = fileInfo.LastWriteTime,
                    Size = fileInfo.Length,
                };

                table.Category = parentCategory;
            }

            foreach (var item in dirs)
            {
                var dirInfo = new DirectoryInfo(item);
                var category = new LocalFolder()
                {
                    Name = dirInfo.Name,
                    ModifiedDateTime = dirInfo.LastWriteTime,
                    Parent = parentCategory,
                };

                this.LoadCategories(category);
            }
        }

        private void Watcher_Created(object sender, FileSystemEventArgs e)
        {

        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {

        }

        private void Watcher_Renamed(object sender, RenamedEventArgs e)
        {

        }

        private void Watcher_Deleted(object sender, FileSystemEventArgs e)
        {

        }

        #region IStorage

        IFolder IStorage.Root => this.Root;

        IFolderCollection IStorage.Folders => this.Categories;

        IFileCollection IStorage.Files => this.Items;

        #endregion
    }
}
