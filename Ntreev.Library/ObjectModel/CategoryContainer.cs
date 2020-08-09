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

using Ntreev.Library.Linq;
using Ntreev.Library.Properties;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Ntreev.Library.ObjectModel
{
    public abstract class CategoryContainer<_I, _C, _IC, _CC, _CT> : ContainerBase<_C>
        where _I : ItemBase<_I, _C, _IC, _CC, _CT>
        where _C : CategoryBase<_I, _C, _IC, _CC, _CT>, new()
        where _IC : ItemContainer<_I, _C, _IC, _CC, _CT>, new()
        where _CC : CategoryContainer<_I, _C, _IC, _CC, _CT>, new()
        where _CT : ItemContext<_I, _C, _IC, _CC, _CT>
    {
        private _CT context;
        private _C root;

        public CategoryContainer()
        {
            this.CreateRoot();
        }

        public string[] GetCategoryNames()
        {
            var query = from item in this
                        orderby item.Path
                        select item.Path;
            return query.ToArray();
        }

        public new _C this[string categoryPath] => base[categoryPath];

        public _C Prepare(string categoryPath)
        {
            var categoryName = new CategoryName(categoryPath);

            if (this.ContainsKey(categoryName.ParentPath) == false)
                this.Prepare(categoryName.ParentPath);
            if (this.ContainsKey(categoryPath) == false)
                return this.BaseAddNew(categoryName.Name, categoryName.ParentPath, null);
            return this[categoryPath];
        }

        public bool Contains(string categoryPath)
        {
            return this.ContainsKey(categoryPath);
        }

        public void Add(_C item)
        {
            if (item.Container != null)
                throw new ArgumentException(string.Format(Resources.Exception_AlreadyExistedItem_Format, item.Name), nameof(item));

            item.Parent = this.Root;
        }

        public void Add(_C item, string parentCategoryPath)
        {
            if (item.Container != null)
                throw new ArgumentException(string.Format(Resources.Exception_AlreadyExistedItem_Format, item.Name), nameof(item));

            var parent = this[parentCategoryPath];
            item.Parent = parent ?? throw new ArgumentException(string.Format(Resources.Exception_NotFoundFolder_Format, parentCategoryPath), nameof(parentCategoryPath));
        }

        public void Remove(_C item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            if (this.Contains(item.Path) == false)
                throw new ArgumentException(string.Format(Resources.Exception_NotFoundItem_Format, item), nameof(item));

            if (item != this.Root)
            {
                item.Parent = null;
            }
            else
            {
                this.RemoveBase(item);
                this.CreateRoot();
            }
        }

        public void Remove(string categoryPath)
        {
            var category = this[categoryPath];
            if (category == null)
                return;

            this.Remove(category);
        }

        public virtual void ValidateAddNew(string name, string parentCategoryPath, object validation, params object[] args)
        {
            if (this.Contains(parentCategoryPath) == false)
                throw new ArgumentException(string.Format(Resources.Exception_NotFoundFolder_Format, parentCategoryPath), nameof(parentCategoryPath));

            var parent = this[parentCategoryPath];
            if (parent.Categories.ContainsKey(name) == true)
                throw new ArgumentException(Resources.Exception_SameFolderInParent);

            if (parent.Items.ContainsKey(name) == true)
                throw new ArgumentException(Resources.Exception_SameItemInParent);

            if (this.Contains(CategoryName.MakePath(parentCategoryPath, name)) == true)
                throw new ArgumentException(string.Format(Resources.Exception_AlreadyExistedItem_Format, name), nameof(name));
        }

        public _C Root
        {
            get
            {
                if (this.root == null)
                {
                    this.CreateRoot();
                }
                return this.root;
            }
        }

        public event CategoryEventHandler<_C> CategoryCreated;

        public event CategoryMovedEventHandler<_C> CategoryMoved;

        public event CategoryRenamedEventHandler<_C> CategoryRenamed;

        public event CategoryDeletedEventHandler<_C> CategoryDeleted;

        internal void InvokeCategoryRenamed(_C category, string oldName, string oldPath)
        {
            this.OnCategoryRenamed(new CategoryRenamedEventArgs<_C>(category, oldName, oldPath));
        }

        internal void InvokeCategoryMoved(_C category, string oldPath, string oldParentPath)
        {
            this.OnCategoryMoved(new CategoryMovedEventArgs<_C>(category, oldPath, oldParentPath));
        }

        internal void InvokeCategoryDeleted(string categoryPath, _C category)
        {
            this.OnCategoryDeleted(new CategoryDeletedEventArgs<_C>(categoryPath, category));
        }

        protected _C BaseAddNew(string name, string parentCategoryPath, object validation, params object[] args)
        {
            this.ValidateAddNew(name, parentCategoryPath, validation, args);

            var parent = this[parentCategoryPath];
            var category = Activator.CreateInstance(typeof(_C), args) as _C;

            category.Name = name;
            category.Parent = parent;
            this.OnCategoryCreated(new CategoryEventArgs<_C>(category));

            return category;
        }

        protected virtual void OnCategoryCreated(CategoryEventArgs<_C> e)
        {
            this.CategoryCreated?.Invoke(this, e);
        }

        protected virtual void OnCategoryMoved(CategoryMovedEventArgs<_C> e)
        {
            this.CategoryMoved?.Invoke(this, e);
        }

        protected virtual void OnCategoryRenamed(CategoryRenamedEventArgs<_C> e)
        {
            this.CategoryRenamed?.Invoke(this, e);
        }

        protected virtual void OnCategoryDeleted(CategoryDeletedEventArgs<_C> e)
        {
            this.CategoryDeleted?.Invoke(this, e);
        }

        protected override sealed void OnInsert(string key, _C value)
        {
            base.OnInsert(key, value);
        }

        protected override sealed void OnInsertComplete(string key, _C value)
        {
            value.Container = this as _CC;
            base.OnInsertComplete(key, value);
            this.AttachEventHandlers(value);
        }

        protected override sealed void OnRemove(string key, _C value)
        {
            this.DetachEventHandlers(value);
            base.OnRemove(key, value);
        }

        protected override sealed void OnRemoveComplete(string key, _C value)
        {
            value.Container = null;
            base.OnRemoveComplete(key, value);
        }

        protected override sealed void OnClear()
        {
            base.OnClear();

            var query = from item in this
                        orderby item.Path descending
                        select item;

            var categories = query.ToArray();

            foreach (var item in categories)
            {
                item.Items.Clear();
                item.Parent = null;
                item.Childs.Clear();
            }

            this.RemoveBase(this.Root);
        }

        protected override sealed void OnClearComplete()
        {
            base.OnClearComplete();
            this.AddBase(this.root);
        }

        protected override sealed void OnReplaceKey(string oldKey, _C value)
        {
            base.OnReplaceKey(oldKey, value);
        }

        protected override void OnReplaceKeyComplete(string oldKey, _C value)
        {
            value.Container = this as _CC;
            base.OnReplaceKeyComplete(oldKey, value);
        }

        protected virtual void OnInitialize()
        {

        }

        protected _IC ItemContainer => this.context.Items;

        public _CT Context
        {
            get => this.context;
            internal set
            {
                this.context = value;
                this.OnInitialize();
            }
        }

        private void AddBase(_C item)
        {
            if (item.IsDisposed == true)
                throw new ObjectDisposedException(item.Name);
            this.AddBase(item.Path, item);
        }

        private void AddBase(IEnumerable<_C> items)
        {
            if (items.Count() == 0)
                return;

            foreach (_C item in items)
            {
                this.AddBase(item.Path, item);
            }
        }

        private void RemoveBase(_C item)
        {
            this.RemoveBase(item.Path);
        }

        private void RemoveBase(IEnumerable<_C> items)
        {
            if (items.Count() == 0)
                return;

            foreach (_C item in items)
            {
                this.RemoveBase(item.Path);
            }
        }

        private void ReplaceKeyBase(_C item)
        {
            item.Container = this as _CC;
            string oldKey = this.GetKeyBase(item);
            this.ReplaceKeyBase(oldKey, item.Path);
        }

        private void ReplaceKeyBase(IEnumerable<_C> items)
        {
            if (items.Count() == 0)
                return;

            foreach (var item in items)
            {
                item.Container = this as _CC;
            }

            var query = from item in items
                        join vp in this on item equals vp
                        select new KeyValuePair<string, _C>(this.GetKeyBase(item), item);

            foreach (var item in query.ToArray())
            {
                this.ReplaceKeyBase(item.Key, item.Value.Path);
            }
        }

        private void Category_ItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        foreach (_I item in e.NewItems)
                        {
                            if (item.Container == null)
                                this.ItemContainer.AddInternal(item);
                            else
                                this.ItemContainer.MoveInternal(item);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (_I item in e.OldItems)
                        {
                            if (item.IsDisposing == true)
                                this.ItemContainer.RemoveInternal(item);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    {
                        foreach (_I item in e.NewItems)
                        {
                            this.ItemContainer.MoveInternal(item);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    {
                        var items = sender as CategoryBase<_I, _C, _IC, _CC, _CT>.ItemCollection;

                        var query = from item in this.ItemContainer
                                    where item.Category == items.Category
                                    select item;

                        this.ItemContainer.RemoveInternal(query);

                        foreach (_I item in query)
                        {
                            item.category = null;
                        }
                    }
                    break;
            }
        }

        private void Category_CategoriesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        foreach (_C item in e.NewItems)
                        {
                            var categories = EnumerableUtility.Descendants(item, i => i.Categories).ToArray();
                            var items = categories.SelectMany(i => i.Items).ToArray();
                            if (item.Container == null)
                            {
                                this.AddBase(item);
                                this.AddBase(categories);
                                this.ItemContainer.AddInternal(item.Items);
                                this.ItemContainer.AddInternal(items);
                            }
                            else
                            {
                                this.ReplaceKeyBase(item);
                                this.ReplaceKeyBase(categories.Reverse());
                                this.ItemContainer.MoveInternal(item.Items.Reverse());
                                this.ItemContainer.MoveInternal(items.Reverse());
                            }
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (_C item in e.OldItems)
                        {
                            if (item.IsDisposing == true)
                            {
                                var categories = EnumerableUtility.Descendants(item, i => i.Categories).ToArray();
                                var items = categories.SelectMany(i => i.Items);
                                this.ItemContainer.RemoveInternal(items);
                                this.ItemContainer.RemoveInternal(item.Items);
                                this.RemoveBase(categories.Reverse());
                                this.RemoveBase(item);
                            }
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    {
                        foreach (_C item in e.NewItems)
                        {
                            var categories = EnumerableUtility.Descendants(item, i => i.Categories).ToArray();
                            var items = categories.SelectMany(i => i.Items).ToArray();

                            this.ReplaceKeyBase(item);
                            this.ReplaceKeyBase(categories.Reverse());
                            this.ItemContainer.MoveInternal(item.Items.Reverse());
                            this.ItemContainer.MoveInternal(items.Reverse());
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    {
                        var categories = sender as CategoryBase<_I, _C, _IC, _CC, _CT>.CategoryCollection;

                        var query = from item in this
                                    where item.Parent == categories.Category
                                    select item;

                        this.RemoveBase(query);
                        foreach (_C item in query)
                        {
                            item.parent = null;
                        }
                    }
                    break;
            }
        }

        private void Category_Disposed(object sender, EventArgs e)
        {
            this.Remove(sender as _C);
        }

        private void AttachEventHandlers(_C category)
        {
            category.Categories.CollectionChanged += Category_CategoriesChanged;
            category.Items.CollectionChanged += Category_ItemsChanged;
            category.Deleted += Category_Disposed;
        }

        private void DetachEventHandlers(_C category)
        {
            category.Deleted -= Category_Disposed;
            category.Items.CollectionChanged -= Category_ItemsChanged;
            category.Categories.CollectionChanged -= Category_CategoriesChanged;
        }

        private void CreateRoot()
        {
            this.root = new _C();
            (this.root as IItemAttributeProvider).ItemAttributes = ItemAttributes.Root | ItemAttributes.UniqueName;
            this.AddBase(this.root);
            this.root.InvokePathChanged();
        }
    }
}
