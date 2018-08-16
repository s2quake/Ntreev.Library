using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ntreev.Library
{
    public sealed class ConfigurationItemDescriptorCollection : ICollection, IEnumerable<ConfigurationItemDescriptor>
    {
        private readonly ConfigurationBase owner;
        private readonly List<ConfigurationItemDescriptor> items = new List<ConfigurationItemDescriptor>();

        internal ConfigurationItemDescriptorCollection(ConfigurationBase owner)
        {
            this.owner = owner;
        }

        public void Add(ConfigurationItemDescriptor descriptor)
        {
            this.ValidateAdd(descriptor);
            this.items.Add(descriptor);
        }

        public void Remove(ConfigurationItemDescriptor descriptor)
        {
            for (var i = 0; i < this.items.Count; i++)
            {
                var item = this.items[i];
                if (item == descriptor)
                {
                    this.items.Remove(item);
                    return;
                }
            }
        }

        public void Remove(string name)
        {
            for (var i = 0; i < this.items.Count; i++)
            {
                var item = this.items[i];
                if (item.Name == name)
                {
                    this.items.Remove(item);
                    return;
                }
            }
        }

        public bool Contains(string name)
        {
            foreach (var item in this.items)
            {
                if (item.Name == name)
                {
                    return true;
                }
            }
            return false;
        }

        public bool Contains(ConfigurationItemDescriptor descriptor)
        {
            foreach (var item in this.items)
            {
                if (item == descriptor)
                {
                    return true;
                }
            }
            return false;
        }

        public ConfigurationItemDescriptor this[int index]
        {
            get => this.items[index] as ConfigurationItemDescriptor;
        }

        public ConfigurationItemDescriptor this[string name]
        {
            get
            {
                foreach (var item in this.items)
                {
                    if (item.Name == name)
                    {
                        return item;
                    }
                }
                return null;
            }
        }

        public int Count => this.items.Count;

        private void ValidateAdd(ConfigurationItemDescriptor descriptor)
        {
            if (this.Contains(descriptor.Name) == true)
                throw new ArgumentException(nameof(descriptor));
        }

        private void ValidateRemove(ConfigurationItemDescriptor descriptor)
        {
            if (this.owner.Contains(descriptor.Name) == true)
                throw new ArgumentException(nameof(descriptor));
        }

        #region ICollection

        object ICollection.SyncRoot
        {
            get
            {
                if (this.items is ICollection collection)
                {
                    return collection.SyncRoot;
                }
                throw new NotImplementedException();
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                if (this.items is ICollection collection)
                {
                    return collection.IsSynchronized;
                }
                throw new NotImplementedException();
            }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            if (this.items is ICollection collection)
            {
                collection.CopyTo(array, index);
            }
        }

        #endregion

        #region IEnumerable

        IEnumerator<ConfigurationItemDescriptor> IEnumerable<ConfigurationItemDescriptor>.GetEnumerator()
        {
            foreach (var item in this.items)
            {
                yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var item in this.items)
            {
                yield return item;
            }
        }

        #endregion
    }
}
