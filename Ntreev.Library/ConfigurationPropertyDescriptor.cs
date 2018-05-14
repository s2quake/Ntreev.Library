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

using Ntreev.Library.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Ntreev.Library
{
    public sealed class ConfigurationPropertyDescriptor
    {
        private readonly IConfigurationPropertyProvider target;
        private readonly PropertyDescriptor descriptor;
        private string propertyName;

        internal ConfigurationPropertyDescriptor(IConfigurationPropertyProvider target, PropertyDescriptor descriptor)
        {
            this.target = target;
            this.descriptor = descriptor;
            var attr = descriptor.Attributes[typeof(ConfigurationPropertyAttribute)] as ConfigurationPropertyAttribute;
            this.propertyName = $"{target.Name}.{attr.PropertyName ?? StringUtility.ToCamelCase(descriptor.Name)}";
            this.ScopeType = attr.ScopeType;
        }

        public void Reset()
        {
            this.descriptor.ResetValue(this.target);
        }

        internal bool ShouldSerializeValue
        {
            get => this.descriptor.ShouldSerializeValue(this.target);
        }

        public object Value
        {
            get => this.descriptor.GetValue(this.target);
            set
            {
                if (value == null || value.GetType() == this.descriptor.PropertyType)
                {
                    this.descriptor.SetValue(this.target, value);
                }
                else
                {
                    this.descriptor.SetValue(this.target, XmlConvertUtility.ToValue($"{value}", this.descriptor.PropertyType));
                }
            }
        }

        public Type PropertyType
        {
            get => this.descriptor.PropertyType;
        }

        public string PropertyName
        {
            get => this.propertyName;
        }

        public string Description
        {
            get => this.descriptor.Description;
        }

        public string Category
        {
            get => this.descriptor.Category;
        }

        public Type ScopeType
        {
            get; private set;
        }
    }
}
