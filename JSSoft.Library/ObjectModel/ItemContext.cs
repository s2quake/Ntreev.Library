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

using System.Collections.Generic;

namespace JSSoft.Library.ObjectModel
{
    public abstract class ItemContext<_I, _C, _IC, _CC, _CT> : IEnumerable<IItem>
        where _I : ItemBase<_I, _C, _IC, _CC, _CT>
        where _C : CategoryBase<_I, _C, _IC, _CC, _CT>, new()
        where _IC : ItemContainer<_I, _C, _IC, _CC, _CT>, new()
        where _CC : CategoryContainer<_I, _C, _IC, _CC, _CT>, new()
        where _CT : ItemContext<_I, _C, _IC, _CC, _CT>
    {
        public ItemContext()
        {
            this.Items = new _IC();
            this.Categories = new _CC();
            this.Items.Context = this as _CT;
            this.Categories.Context = this as _CT;
        }

        public void Clear()
        {
            this.Categories.Clear();
        }

        public bool Contains(string itemPath)
        {
            if (NameValidator.VerifyCategoryPath(itemPath) == true)
                return this.Categories.Contains(itemPath);

            if (this.Items.SupportsUniqueName == true && NameValidator.VerifyItemPath(itemPath) == false)
                return this.Items.Contains(itemPath);

            var itemName = new ItemName(itemPath);
            return this.Items.Contains(itemName.Name, itemName.CategoryPath);
        }

        public IItem this[string itemPath]
        {
            get
            {
                if (NameValidator.VerifyCategoryPath(itemPath) == true)
                    return this.Categories[itemPath];

                var itemName = new ItemName(itemPath);
                if (this.Items.SupportsNonUniqueName == false)
                    return this.Items[itemName.Name];

                return this.Items[itemName.Name, itemName.CategoryPath];
            }
        }

        public _IC Items { get; }

        public _CC Categories { get; }

        public _C Root => this.Categories.Root;

        public event ItemCreatedEventHandler<_I> ItemCreated
        {
            add { this.Items.ItemCreated += value; }
            remove { this.Items.ItemCreated -= value; }
        }

        public event ItemMovedEventHandler<_I> ItemMoved
        {
            add { this.Items.ItemMoved += value; }
            remove { this.Items.ItemMoved -= value; }
        }

        public event ItemRenamedEventHandler<_I> ItemRenamed
        {
            add { this.Items.ItemRenamed += value; }
            remove { this.Items.ItemRenamed -= value; }
        }

        public event ItemDeletedEventHandler<_I> ItemDeleted
        {
            add { this.Items.ItemDeleted += value; }
            remove { this.Items.ItemDeleted -= value; }
        }

        public event CategoryEventHandler<_C> CategoryCreated
        {
            add { this.Categories.CategoryCreated += value; }
            remove { this.Categories.CategoryCreated -= value; }
        }

        public event CategoryMovedEventHandler<_C> CategoryMoved
        {
            add { this.Categories.CategoryMoved += value; }
            remove { this.Categories.CategoryMoved -= value; }
        }

        public event CategoryRenamedEventHandler<_C> CategoryRenamed
        {
            add { this.Categories.CategoryRenamed += value; }
            remove { this.Categories.CategoryRenamed -= value; }
        }

        public event CategoryDeletedEventHandler<_C> CategoryDeleted
        {
            add { this.Categories.CategoryDeleted += value; }
            remove { this.Categories.CategoryDeleted -= value; }
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
