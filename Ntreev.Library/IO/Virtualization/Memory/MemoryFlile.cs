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
using System.IO;

namespace Ntreev.Library.IO.Virtualization.Memory
{
    public class MemoryFile : ItemBase<MemoryFile, MemoryFolder, MemoryFileCollection, MemoryFolderCollection, MemoryStorage>, IFile
    {
        private string hashValue;

        public void Rename(string name)
        {
            this.Name = name;
        }

        public void MoveTo(string folderPath)
        {
            var folder = this.Context.Categories[folderPath];
            this.Category = folder;
        }

        public void Delete()
        {
            this.Dispose();
        }

        public Stream OpenRead()
        {
            return new MemoryStream(this.Data);
        }

        public Stream OpenWrite()
        {
            return new InternalStream(this);
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

        internal byte[] Data { get; set; } = new byte[] { };

        #region IFile

        IFolder IFile.Parent => this.Category;

        IStorage IFile.Storage => this.Context;

        string IFileSystem.Path => this.Path;

        #endregion

        #region classes

        class InternalStream : MemoryStream
        {
            private readonly MemoryFile file;

            public InternalStream(MemoryFile file)
            {
                this.file = file;
            }

            public override void Close()
            {
                var data = new byte[this.Length];
                Buffer.BlockCopy(this.GetBuffer(), 0, data, 0, (int)this.Length);
                this.file.Data = data;
                base.Close();
            }
        }

        #endregion
    }
}