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

using JSSoft.Library.ObjectModel;
using JSSoft.Library.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace JSSoft.Library
{
    public sealed class ConfigurationPropertyDescriptorCollection : ContainerBase<ConfigurationPropertyDescriptor>
    {
        private readonly Type filterType;

        public ConfigurationPropertyDescriptorCollection()
        {

        }

        public ConfigurationPropertyDescriptorCollection(IEnumerable<IConfigurationPropertyProvider> providers)
        {
            foreach (var item in providers)
            {
                this.Initialize(item);
            }
        }

        public ConfigurationPropertyDescriptorCollection(IEnumerable<IConfigurationPropertyProvider> providers, Type scopeType)
        {
            this.filterType = scopeType;
            foreach (var item in providers)
            {
                this.Initialize(item);
            }
        }

        public void Add(ConfigurationPropertyDescriptor item)
        {
            this.AddBase(item.PropertyName, item);
        }

        public void Remove(ConfigurationPropertyDescriptor item)
        {
            this.RemoveBase(item.PropertyName);
        }

        private void ValidateProperty(PropertyInfo propertyInfo)
        {
            if (propertyInfo.CanRead == false)
                throw new ArgumentException("property can not use. becuase property can not read");
            if (propertyInfo.CanWrite == false)
                throw new ArgumentException("property can not use. becuase property can not write");
            this.ValidatePropertyType(propertyInfo.PropertyType);
        }

        private void ValidatePropertyType(Type type)
        {
            if (type.IsEnum == true)
                return;
            if (type.IsArray == true && XmlConvertUtility.IsBaseType(type.GetElementType()) == true)
                return;
            if (XmlConvertUtility.IsBaseType(type) == true)
                return;
            throw new ArgumentException("${type} can not use by property type.");
        }

        private void Initialize(IConfigurationPropertyProvider provider)
        {
            var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            foreach (var item in provider.GetType().GetProperties(bindingFlags))
            {
                var attr = item.GetCustomAttribute<ConfigurationPropertyAttribute>();
                if (attr == null)
                    continue;
                this.ValidateProperty(item);

                if (this.filterType != null && this.filterType != attr.ScopeType)
                    continue;

                var configDescriptor = new ConfigurationPropertyProviderDescriptor(provider, item);
                if (this.ContainsKey(configDescriptor.PropertyName) == true)
                    throw new ArgumentException($"{configDescriptor.PropertyName} property is already registered.");
                this.Add(configDescriptor);
            }
        }
    }
}
