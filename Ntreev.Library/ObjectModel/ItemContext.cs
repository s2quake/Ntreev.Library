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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Ntreev.Library.ObjectModel
{
    public abstract class ItemContext<_I, _C, _IC, _CC, _CT> : IEnumerable<IItem>
        where _I : ItemBase<_I, _C, _IC, _CC, _CT>
        where _C : CategoryBase<_I, _C, _IC, _CC, _CT>, new()
        where _IC : ItemContainer<_I, _C, _IC, _CC, _CT>, new()
        where _CC : CategoryContainer<_I, _C, _IC, _CC, _CT>, new()
        where _CT : ItemContext<_I, _C, _IC, _CC, _CT>
    {
        private _IC items;
        private _CC categories;

        public ItemContext()
        {
            this.items = new _IC();
            this.categories = new _CC();
            this.items.Context = this as _CT;
            this.categories.Context = this as _CT;
        }

        public void Clear()
        {
            this.categories.Clear();
        }

        public bool Contains(string itemPath)
        {
            if (NameValidator.VerifyCategoryPath(itemPath) == true)
                return this.Categories.Contains(itemPath);

            var itemName = new ItemName(itemPath);
            if (this.items.SupportsNonUniqueName == false)
                return this.items.Contains(itemName.Name);

            return this.items.Contains(itemName.Name, itemName.CategoryPath);
        }

        public IItem this[string itemPath]
        {
            get
            {
                if (NameValidator.VerifyCategoryPath(itemPath) == true)
                    return this.Categories[itemPath];

                var itemName = new ItemName(itemPath);
                if (this.items.SupportsNonUniqueName == false)
                    return this.items[itemName.Name];

                return this.items[itemName.Name, itemName.CategoryPath];
            }
        }

        public _IC Items
        {
            get { return this.items; }
        }

        public _CC Categories
        {
            get { return this.categories; }
        }

        public _C Root
        {
            get { return this.categories.Root; }
        }

        public event ItemCreatedEventHandler<_I> ItemCreated
        {
            add { this.items.ItemCreated += value; }
            remove { this.items.ItemCreated -= value; }
        }

        public event ItemMovedEventHandler<_I> ItemMoved
        {
            add { this.items.ItemMoved += value; }
            remove { this.items.ItemMoved -= value; }
        }

        public event ItemRenamedEventHandler<_I> ItemRenamed
        {
            add { this.items.ItemRenamed += value; }
            remove { this.items.ItemRenamed -= value; }
        }

        public event ItemDeletedEventHandler<_I> ItemDeleted
        {
            add { this.items.ItemDeleted += value; }
            remove { this.items.ItemDeleted -= value; }
        }

        public event CategoryEventHandler<_C> CategoryCreated
        {
            add { this.categories.CategoryCreated += value; }
            remove { this.categories.CategoryCreated -= value; }
        }

        public event CategoryMovedEventHandler<_C> CategoryMoved
        {
            add { this.categories.CategoryMoved += value; }
            remove { this.categories.CategoryMoved -= value; }
        }

        public event CategoryRenamedEventHandler<_C> CategoryRenamed
        {
            add { this.categories.CategoryRenamed += value; }
            remove { this.categories.CategoryRenamed -= value; }
        }

        public event CategoryDeletedEventHandler<_C> CategoryDeleted
        {
            add { this.categories.CategoryDeleted += value; }
            remove { this.categories.CategoryDeleted -= value; }
        }

        #region IEnumerator

        private IEnumerable<IItem> Descendants(IItem c)
        {
            yield return c;

            foreach (var item in c.Childs)
            {
                foreach (var i in this.Descendants(item))
                {
                    yield return i;
                }
            }
        }

        public IEnumerator<IItem> GetEnumerator()
        {
            foreach (var item in this.Descendants(this.Root))
            {
                yield return item;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            foreach (var item in this.Descendants(this.Root))
            {
                yield return item;
            }
        }

        #endregion IEnumerator
    }
}