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
using System.Threading.Tasks;

namespace Ntreev.Library.IO.Virtualization.Local
{
    public class LocalFolder : CategoryBase<LocalFile, LocalFolder, LocalFileCollection, LocalFolderCollection, LocalStorage>, IFolder
    {
        public LocalFolder CreateFolder(string name)
        {
            string path = this.GenerateFolderPath(name);
            Directory.CreateDirectory(path);
            return this.Container.AddNew(this, name);
        }

        public LocalFile CreateFile(string name, Stream stream, long length, IProgress progress)
        {
            string path = this.GenerateFilePath(name);
            using(Stream writeStream = File.Create(path))
            {
                stream.CopyTo(length, writeStream, progress);
            }
            LocalFile file = this.Context.Items.AddNew(this, name);

            file.Size = length;
            file.ModifiedDateTime = File.GetLastWriteTime(path);

            return file;
        }

        public void Rename(string name)
        {
            string newPath = System.IO.Path.Combine(this.Parent.LocalPath, name);
            Directory.Move(this.LocalPath, newPath);
            this.Name = name;
        }

        public void Delete()
        {
            Directory.Delete(this.LocalPath, true);
            this.Dispose();
        }

        public void MoveTo(string folderPath)
        {
            LocalFolder parentFolder = this.Container[folderPath];
            string newPath = System.IO.Path.Combine(parentFolder.LocalPath, this.Name);
            Directory.Move(this.LocalPath, newPath);
            this.Parent = parentFolder;
        }

        public string GenerateFolderPath(string name)
        {
            return System.IO.Path.Combine(this.LocalPath, name);
        }

        public string GenerateFilePath(string name)
        {
            return System.IO.Path.Combine(this.LocalPath, name);
        }

        public string LocalPath
        {
            get 
            {
                return string.Format("{0}{1}", this.Context.LocalPath, this.Path); 
            }
        }

        public DateTime ModifiedDateTime
        {
            get;
            set;
        }

        #region IFolder

        IFolder IFolder.CreateFolder(string name)
        {
            return this.CreateFolder(name);
        }

        IFile IFolder.CreateFile(string name, Stream stream, long length, IProgress progress)
        {
            return this.CreateFile(name, stream, length, progress);
        }

        IFolder IFolder.Parent
        {
            get { return this.Parent; }
        }

        IContainer<IFolder> IFolder.Folders
        {
            get { return this.Categories; }
        }

        IContainer<IFile> IFolder.Files
        {
            get { return this.Items; }
        }

        IStorage IFolder.Storage
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
