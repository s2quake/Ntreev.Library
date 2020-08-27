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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace JSSoft.Library.ObjectModel
{
    public abstract class ContainerBase<T> : IContainer<T>
    {
        private readonly Dictionary<string, T> keyToValue;

        protected ContainerBase()
        {
            this.keyToValue = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
        }

        protected ContainerBase(int capacity)
        {
            this.keyToValue = new Dictionary<string, T>(capacity, StringComparer.OrdinalIgnoreCase);
        }

        public void Clear()
        {
            this.OnClear();
            this.keyToValue.Clear();
            this.OnClearComplete();
        }

        public bool ContainsKey(string key)
        {
            return this.keyToValue.ContainsKey(key);
        }

        public bool TryGetValue(string key, out T value)
        {
            return this.keyToValue.TryGetValue(key, out value);
        }

        public int Count
        {
            get
            {
                if (this.keyToValue != null)
                {
                    return this.keyToValue.Count;
                }
                return 0;
            }
        }

        public T this[string key]
        {
            get
            {
                if (key == null)
                    return default;
                if (this.keyToValue.ContainsKey(key) == false)
                    return default;
                return this.keyToValue[key];
            }
        }

        public IEnumerable<string> Keys => this.keyToValue.Keys;

        public event NotifyCollectionChangedEventHandler CollectionChanging;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        protected virtual void OnCollectionChanging(NotifyCollectionChangedEventArgs e)
        {
            this.CollectionChanging?.Invoke(this, e);
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            this.CollectionChanged?.Invoke(this, e);
        }

        protected virtual object OnGet(string key, T currentValue)
        {
            return currentValue;
        }

        protected virtual void OnInsert(string key, T value)
        {
            if (this.PreventEvent == false)
            {
                this.OnCollectionChanging(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, value));
            }
        }

        protected virtual void OnInsertComplete(string key, T value)
        {
            if (this.PreventEvent == false)
            {
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, value));
            }
        }

        protected virtual void OnRemove(string key, T value)
        {
            if (this.PreventEvent == false)
            {
                this.OnCollectionChanging(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, value));
            }
        }

        protected virtual void OnRemoveComplete(string key, T value)
        {
            if (this.PreventEvent == false)
            {
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, value));
            }
        }

        protected virtual void OnReplaceValue(string key, T newValue, T oldValue)
        {
            if (this.PreventEvent == false)
            {
                this.OnCollectionChanging(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, key, key));
            }
        }

        protected virtual void OnReplaceValueComplete(string key, T value)
        {
            if (this.PreventEvent == false)
            {
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, key, key));
            }
        }

        protected virtual void OnReplaceKey(string oldKey, T value)
        {
            if (this.PreventEvent == false)
            {
                this.OnCollectionChanging(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, oldKey));
            }
        }

        protected virtual void OnReplaceKeyComplete(string oldKey, T value)
        {
            if (this.PreventEvent == false)
            {
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, oldKey));
            }
        }

        protected virtual void OnClear()
        {
            if (this.PreventEvent == false)
            {
                this.OnCollectionChanging(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        protected virtual void OnClearComplete()
        {
            if (this.PreventEvent == false)
            {
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        protected void AddBase(string key, T value)
        {
            lock (this)
            {
                this.OnInsert(key, value);
                this.keyToValue.Add(key, value);
                try
                {
                    this.OnInsertComplete(key, value);
                }
                catch
                {
                    this.keyToValue.Remove(key);
                    throw;
                }
            }
        }

        protected bool RemoveBase(string key)
        {
            if (this.keyToValue.ContainsKey(key))
            {
                var value = this.keyToValue[key];
                this.OnRemove(key, value);
                this.keyToValue.Remove(key);
                try
                {
                    this.OnRemoveComplete(key, value);
                    return true;
                }
                catch
                {
                    this.keyToValue.Add(key, value);
                    throw;
                }
            }
            return false;
        }

        protected void ReplaceValueBase(string key, T value)
        {
            if (this.keyToValue.ContainsKey(key) == false)
                throw new ArgumentException();

            var oldValue = this.keyToValue[key];
            this.OnReplaceValue(key, value, oldValue);
            this.keyToValue[key] = value;
            try
            {
                this.OnRemoveComplete(key, value);
            }
            catch
            {
                this.keyToValue[key] = oldValue;
                throw;
            }
        }

        protected void ReplaceKeyBase(string oldKey, string key)
        {
            var value = this[oldKey];
            this.OnReplaceKey(oldKey, value);
            if (StringComparer.OrdinalIgnoreCase.Compare(oldKey, key) != 0 && this.keyToValue.ContainsKey(key))
                throw new ArgumentException();

            this.keyToValue.Remove(oldKey);
            this.keyToValue.Add(key, value);
            this.OnReplaceKeyComplete(oldKey, value);
        }

        protected bool PreventEvent
        {
            get;
            set;
        }

        protected string GetKeyBase(T value)
        {
            foreach (var item in this.keyToValue)
            {
                if (object.Equals(item.Value, value) == true)
                    return item.Key;
            }
            return null;
        }

        protected IEnumerable<KeyValuePair<string, T>> GetKeyValues()
        {
            return this.keyToValue;
        }

        #region IEnumerable

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.keyToValue.Values.GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this.keyToValue.Values.GetEnumerator();
        }

        #endregion
    }
}
