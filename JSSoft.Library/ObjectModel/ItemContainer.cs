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
using System.Collections.Specialized;
using System.Linq;

namespace Ntreev.Library.ObjectModel
{
    public abstract class ItemContainer<_I, _C, _IC, _CC, _CT> : ContainerBase<_I>
        where _I : ItemBase<_I, _C, _IC, _CC, _CT>
        where _C : CategoryBase<_I, _C, _IC, _CC, _CT>, new()
        where _IC : ItemContainer<_I, _C, _IC, _CC, _CT>, new()
        where _CC : CategoryContainer<_I, _C, _IC, _CC, _CT>, new()
        where _CT : ItemContext<_I, _C, _IC, _CC, _CT>
    {
        private _CT context;

        public ItemContainer()
        {

        }

        public bool Contains(string name)
        {
            return this.ContainsKey(name);
        }

        public bool Contains(string name, string categoryPath)
        {
            if (this.SupportsNonUniqueName == false)
            {
                var item = this[name];
                if (item == null)
                    return false;
                return item.Category.Path == categoryPath;
            }
            return this.ContainsKey(ItemName.MakePath(categoryPath, name));
        }

        public void Add(_I item, string categoryPath)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            if (item.Container != null)
                throw new ArgumentException(string.Format(Resources.Exception_AlreadyExistedItem_Format, item.Name), nameof(item));

            var category = this.CategoryContainer[categoryPath];
            item.Category = category ?? throw new ArgumentException(string.Format(Resources.Exception_NotFoundFolder_Format, categoryPath), nameof(categoryPath));
        }

        public void Add(_I item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            if (item.Container != null)
                throw new ArgumentException(string.Format(Resources.Exception_AlreadyExistedItem_Format, item.Name), nameof(item));

            item.Category = this.CategoryContainer.Root;
        }

        public void Remove(_I item)
        {
            item.Category = null;
        }

        public void Remove(string name)
        {
            if (this.ContainsKey(name) == false)
                return;

            var item = this[name];
            item.Category = null;
        }

        public string[] GetItemPaths()
        {
            var query = from item in this
                        orderby item.Path
                        select item.Path;
            return query.ToArray();
        }

        public string[] GetItemNames()
        {
            var query = from item in this
                        orderby item.Name
                        select item.Name;
            return query.ToArray();
        }

        public new _I this[string name] => base[name];

        public _I this[string name, string categoryPath]
        {
            get
            {
                if (this.SupportsNonUniqueName == false)
                {
                    var item = this[name];
                    if (item == null)
                        return null;
                    if (item.Category.Path != categoryPath)
                        return null;
                    return item;
                }
                return base[ItemName.MakePath(categoryPath, name)];
            }
        }

        public virtual bool SupportsNonUniqueName => false;

        public event ItemCreatedEventHandler<_I> ItemCreated;

        public event ItemMovedEventHandler<_I> ItemMoved;

        public event ItemRenamedEventHandler<_I> ItemRenamed;

        public event ItemDeletedEventHandler<_I> ItemDeleted;

        internal void AddInternal(_I item)
        {
            this.PreventEvent = true;
            try
            {
                this.OnCollectionChanging(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
                this.AddBase(this.GetItemKey(item), item);
                item.Container = this as _IC;
                item.Deleted += Item_Deleted;

                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
            }
            finally
            {
                this.PreventEvent = false;
            }
        }

        internal void AddInternal(IEnumerable<_I> items)
        {
            if (items.Count() == 0)
                return;

            this.PreventEvent = true;
            try
            {
                this.OnCollectionChanging(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items.ToList()));

                foreach (var item in items)
                {
                    var key = this.SupportsNonUniqueName == true ? item.Path : item.Name;
                    this.AddBase(key, item);
                    item.Container = this as _IC;
                    item.Deleted += Item_Deleted;
                }

                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items.ToList()));
            }
            finally
            {
                this.PreventEvent = false;
            }
        }

        internal void RemoveInternal(_I item)
        {
            this.PreventEvent = true;
            try
            {
                this.OnCollectionChanging(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
                string key = this.SupportsNonUniqueName == true ? item.Path : item.Name;
                this.RemoveBase(key);
                item.Deleted -= Item_Deleted;
                item.Container = null;

                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
            }
            finally
            {
                this.PreventEvent = false;
            }
        }

        internal void RemoveInternal(IEnumerable<_I> items)
        {
            if (items.Count() == 0)
                return;

            var itemsArray = items.ToArray();

            this.PreventEvent = true;
            try
            {
                this.OnCollectionChanging(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, itemsArray));

                foreach (var item in itemsArray)
                {
                    string key = this.SupportsNonUniqueName == true ? item.Path : item.Name;
                    this.RemoveBase(key);
                    item.Deleted -= Item_Deleted;
                    item.Container = null;
                }

                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, itemsArray));
            }
            finally
            {
                this.PreventEvent = false;
            }
        }

        internal void MoveInternal(_I item)
        {
            item.Container = this as _IC;

            var oldKey = this.GetKeyBase(item);

            this.PreventEvent = true;
            try
            {
                this.OnCollectionChanging(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, oldKey));
                var key = this.GetItemKey(item);
                this.ReplaceKeyBase(oldKey, key);
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, oldKey));
            }
            finally
            {
                this.PreventEvent = false;
            }
        }

        internal void MoveInternal(IEnumerable<_I> items)
        {
            if (items.Count() == 0)
                return;

            foreach (var item in items)
            {
                item.Container = this as _IC;
            }

            var query = from item in items
                        select new KeyValuePair<string, _I>(this.GetKeyBase(item), item);

            var keys = query.Select(item => item.Key).ToList();
            var values = query.Select(item => item.Value).ToList();

            this.PreventEvent = true;
            try
            {
                this.OnCollectionChanging(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, values, keys));
                foreach (var item in query)
                {
                    var key = this.GetItemKey(item.Value);
                    this.ReplaceKeyBase(item.Key, key);
                }
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, values, keys));
            }
            finally
            {
                this.PreventEvent = false;
            }
        }

        internal void InvokeItemMoved(_I item, string oldPath, string oldFullCategoryName)
        {
            this.OnItemMoved(new ItemMovedEventArgs<_I>(item, oldPath, oldFullCategoryName));
        }

        internal void InvokeItemRenamed(_I item, string oldName, string oldPath)
        {
            this.OnItemRenamed(new ItemRenamedEventArgs<_I>(item, oldName, oldPath));
        }

        internal void InvokeItemDeleted(string itemPath, _I item)
        {
            this.OnItemDeleted(new ItemDeletedEventArgs<_I>(itemPath, item));
        }

        protected virtual _I NewItem(params object[] args)
        {
            return Activator.CreateInstance(typeof(_I), args) as _I;
        }

        protected _I BaseAddNew(string name, string categoryPath, object validation, params object[] args)
        {
            this.ValidateAddNew(name, categoryPath, validation);

            var category = this.CategoryContainer[categoryPath];
            var item = this.NewItem(args);

            try
            {
                item.Name = name;
                item.Category = category;
                this.OnItemCreated(new ItemCreatedEventArgs<_I>(item, args));
            }
            catch (Exception)
            {
                this.OnItemCreated(new ItemCreatedEventArgs<_I>(null));
            }

            return item;
        }

        protected virtual void ValidateAddNew(string name, string categoryPath, object validation)
        {
            if (this.CategoryContainer.Contains(categoryPath) == false)
                throw new ArgumentException(string.Format(Resources.Exception_NotFoundFolder_Format, categoryPath), categoryPath);

            var parent = this.CategoryContainer[categoryPath];
            if (parent.Categories.ContainsKey(name) == true)
                throw new ArgumentException(Resources.Exception_SameFolderInParent);

            if (parent.Items.ContainsKey(name) == true)
                throw new ArgumentException(Resources.Exception_SameItemInParent);

            var key = this.SupportsNonUniqueName == true ? categoryPath + name : name;

            if (this.Contains(key) == true)
                throw new ArgumentException(string.Format(Resources.Exception_AlreadyExistedItem_Format, key));
        }

        protected override sealed void OnClear()
        {
            base.OnClear();

            var items = this.ToArray();
            foreach (var item in items)
            {
                item.Category = null;
            }
        }

        protected override sealed void OnClearComplete()
        {
            base.OnClearComplete();
        }

        protected override sealed void OnInsert(string key, _I value)
        {
            base.OnInsert(key, value);
        }

        protected override sealed void OnInsertComplete(string key, _I value)
        {
            base.OnInsertComplete(key, value);
        }

        protected override sealed void OnRemove(string key, _I value)
        {
            base.OnRemove(key, value);
        }

        protected override sealed void OnRemoveComplete(string key, _I value)
        {
            base.OnRemoveComplete(key, value);
        }

        protected virtual void OnItemCreated(ItemCreatedEventArgs<_I> e)
        {
            this.ItemCreated?.Invoke(this, e);
        }

        protected virtual void OnItemMoved(ItemMovedEventArgs<_I> e)
        {
            this.ItemMoved?.Invoke(this, e);
        }

        protected virtual void OnItemRenamed(ItemRenamedEventArgs<_I> e)
        {
            this.ItemRenamed?.Invoke(this, e);
        }

        protected virtual void OnItemDeleted(ItemDeletedEventArgs<_I> e)
        {
            this.ItemDeleted?.Invoke(this, e);
        }

        protected virtual void OnInitialize()
        {

        }

        protected _C GetCategory(string categoryPath)
        {
            return this.Context.Categories[categoryPath];
        }

        public _CT Context
        {
            get => this.context;
            internal set
            {
                this.context = value;
                this.OnInitialize();
            }
        }

        private _CC CategoryContainer => this.context.Categories;

        private void Item_Deleted(object sender, EventArgs e)
        {
            var item = sender as _I;
            item.Category = null;
        }

        private string GetItemKey(_I item)
        {
            return this.SupportsNonUniqueName == true ? item.Path : item.Name;
        }
    }
}