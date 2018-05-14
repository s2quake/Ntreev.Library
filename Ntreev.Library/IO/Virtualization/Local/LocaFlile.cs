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

using Ntreev.Library.ObjectModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Ntreev.Library.IO.Virtualization.Local
{
    public class LocalFile : ItemBase<LocalFile, LocalFolder, LocalFileCollection, LocalFolderCollection, LocalStorage>, IFile
    {
        private string hashValue;

        public void Rename(string name)
        {
            var newPath = System.IO.Path.Combine(this.Category.LocalPath, name);
            File.Move(this.LocalPath, newPath);
            this.Name = name;
        }

        public void MoveTo(string folderPath)
        {
            var folder = this.Context.Categories[folderPath];
            var newPath = System.IO.Path.Combine(folder.LocalPath, this.Name);
            File.Move(this.LocalPath, newPath);
            this.Category = folder;
        }

        public void Delete()
        {
            File.Delete(this.LocalPath);
            this.Dispose();
        }

        public Stream OpenRead()
        {
            return File.OpenRead(this.LocalPath);
        }

        public Stream OpenWrite()
        {
            File.Delete(this.LocalPath);
            return File.OpenWrite(this.LocalPath);
        }

        public string LocalPath
        {
            get { return string.Format("{0}{1}", this.Context.LocalPath, this.Path); }
        }

        public long Size { get; set; }

        public DateTime ModifiedDateTime
        {
            get;
            set;
        }

        public string HashValue
        {
            get
            {
                if (this.hashValue == null)
                {
                    this.hashValue = this.Context.GetHashValue(this);
                }
                return this.hashValue;
            }
        }

        #region IFile

        IFolder IFile.Parent
        {
            get { return this.Category; }
        }

        IStorage IFile.Storage
        {
            get { return this.Context; }
        }

        string IFileSystem.Path
        {
            get { return this.Path; }
        }

        #endregion
    }
}
