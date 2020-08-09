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
using System.ComponentModel;
using System.Data;
using System.Linq;

namespace Ntreev.Library.ObjectModel
{
    public class ItemBase<_I, _C, _IC, _CC, _CT> : IItem
        where _I : ItemBase<_I, _C, _IC, _CC, _CT>
        where _C : CategoryBase<_I, _C, _IC, _CC, _CT>, new()
        where _IC : ItemContainer<_I, _C, _IC, _CC, _CT>, new()
        where _CC : CategoryContainer<_I, _C, _IC, _CC, _CT>, new()
        where _CT : ItemContext<_I, _C, _IC, _CC, _CT>
    {
        private string name;
        private string path;
        internal _C category;
        private _IC container;
        private PropertyCollection extendedProperties;

        public void Dispose()
        {
            this.ValidateDelete();

            var itemPath = this.Path;
            var container = this.container;

            this.IsDisposing = true;
            this.OnDeleted(EventArgs.Empty);
            this.IsDisposing = false;
            this.IsDisposed = true;

            if (container != null)
                container.InvokeItemDeleted(itemPath, this as _I);
        }

        public int CompareTo(object obj)
        {
            var dest = obj as _I;
            if (this.Category == dest.Category)
                return this.Name.CompareTo(dest.Name);
            if (this.Depth == dest.Depth)
                return this.Category.CompareTo(dest.Category);
            return this.Depth.CompareTo(dest.Depth);
        }

        public void ValidateRename(string newName)
        {
            if (newName == null)
                throw new ArgumentNullException(nameof(newName));
            if (NameValidator.VerifyName(newName) == false)
                throw new ArgumentException(string.Format(Resources.Exception_InvalidName_Format, newName), nameof(newName));

            var container = this.container;
            var category = this.category;

            if (container != null)
            {
                var key = this.container.SupportsNonUniqueName == true ? ItemName.MakePath(this.category.Path, newName) : newName;
                if (container.Contains(key) == true && container[key] != this)
                    throw new ArgumentException(string.Format(Resources.Exception_AlreadyExistedItem_Format, newName), nameof(newName));
            }

            if (category != null)
            {
                if (category.Categories.ContainsKey(newName) == true)
                    throw new ArgumentException(Resources.Exception_SameFolderInParent);
            }
        }

        public void ValidateMove(_C category)
        {
            if (category == null)
                return;

            if (this.name == null)
                throw new InvalidOperationException(Resources.Exception_UnnamedFolderCannotHaveParent);

            if (category.Items.ContainsKey(this.name) == true)
                throw new ArgumentException(Resources.Exception_SameItemInParent, nameof(category));

            if (category.Categories.ContainsKey(this.name) == true)
                throw new ArgumentException(Resources.Exception_SameFolderInParent, nameof(category));
        }

        public void ValidateDelete()
        {
            if (this.IsDisposed == true)
                throw new ObjectDisposedException(this.name);
        }

        public override string ToString()
        {
            if (this.container == null || this.container.SupportsNonUniqueName == false)
                return this.Name;
            return this.Path;
        }

        public string Name
        {
            get
            {
                if (this.name == null)
                    return string.Empty;
                return this.name;
            }
            set
            {
                if (this.name == value)
                    return;

                this.ValidateRename(value);

                var container = this.container;
                var category = this.category;

                var oldName = this.Name;
                var oldPath = this.Path;

                this.name = value;
                this.path = null;
                if (category != null)
                {
                    category.Items.Move(oldName, this as _I);
                    category.Childs.Move(oldName, this);
                }

                this.OnRenamed(EventArgs.Empty);
                this.OnPathChanged(oldPath, this.Path);

                if (container != null)
                    container.InvokeItemRenamed(this as _I, oldName, oldPath);
            }
        }

        public string Path
        {
            get
            {
                if (this.path == null)
                {
                    if (this.category == null)
                        this.path = this.Name;
                    else
                        this.path = this.category.Path + this.Name;
                }
                return this.path;
            }
        }

        public bool IsDisposed { get; internal set; }

        public int Depth { get; private set; }

        public _C Category
        {
            get => this.category;
            set
            {
                if (this.category == value)
                    return;

                if (this.IsDisposing == true)
                {
                    if (this.category != null)
                    {
                        this.category.Items.Remove(this as _I);
                        this.category.Childs.Remove(this);
                    }
                    this.category = null;
                    return;
                }

                if (this.category != null && value == null)
                {
                    this.IsDisposing = true;
                    this.category.Items.Remove(this as _I);
                    this.category.Childs.Remove(this);
                    this.category = null;
                    this.IsDisposing = false;
                    return;
                }

                this.ValidateMove(value);

                var oldCategory = this.Category;
                var container = this.container;
                var oldPath = this.Path;

                if (this.category != null)
                {
                    this.category.Items.Remove(this as _I);
                    this.category.Childs.Remove(this);
                }
                this.category = value;
                this.path = null;
                if (this.category != null)
                {
                    this.category.Items.Add(this as _I);
                    this.category.Childs.Add(this);
                }

                this.OnMoved(EventArgs.Empty);
                this.OnPathChanged(oldPath, this.Path);

                if (container != null)
                    container.InvokeItemMoved(this as _I, oldPath, oldCategory?.Path);
            }
        }

        public _IC Container
        {
            get => this.container;
            internal set
            {
                var isSame = this.container == value;

                this.container = value;
                this.Depth = this.Category == null ? 0 : this.Category.Path.Where(item => item == PathUtility.SeparatorChar).Count() + 1;
                this.path = null;

                if (isSame == false)
                {
                    if (this.container == null)
                    {
                        this.OnDetached();
                    }
                    else
                    {
                        this.OnAttached();
                    }
                }
            }
        }

        public _CT Context
        {
            get
            {
                if (this.container == null)
                    return null;
                return this.container.Context;
            }
        }

        [Browsable(false)]
        public PropertyCollection ExtendedProperties
        {
            get
            {
                if (this.extendedProperties == null)
                {
                    this.extendedProperties = new PropertyCollection();
                }
                return this.extendedProperties;
            }
        }

        public virtual ItemAttributes ItemAttributes => ItemAttributes.None;

        public event EventHandler Renamed;

        public event EventHandler Moved;

        public event EventHandler Deleted;

        protected virtual void OnRenamed(EventArgs e)
        {
            this.Renamed?.Invoke(this, e);
        }

        protected virtual void OnMoved(EventArgs e)
        {
            this.Moved?.Invoke(this, e);
        }

        protected virtual void OnDeleted(EventArgs e)
        {
            this.Deleted?.Invoke(this, e);
        }

        protected virtual void OnAttached()
        {

        }

        protected virtual void OnDetached()
        {

        }

        protected virtual void OnPathChanged(string oldPath, string newPath)
        {

        }

        internal void InvokeOnPathChanged(string oldPath, string newPath)
        {
            this.OnPathChanged(oldPath, newPath);
        }

        internal bool IsDisposing { get; private set; }

        #region IItem

        IItem IItem.Parent => this.Category;

        IContainer<IItem> IItem.Childs => EmpryContainer<IItem>.Default;

        event EventHandler IItem.Moved
        {
            add { this.Moved += value; }
            remove { this.Moved -= value; }
        }

        #endregion
    }
}
