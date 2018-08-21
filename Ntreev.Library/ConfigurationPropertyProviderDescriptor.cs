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
    sealed class ConfigurationPropertyProviderDescriptor : ConfigurationPropertyDescriptor
    {
        private readonly IConfigurationPropertyProvider target;
        private readonly PropertyDescriptor descriptor;

        internal ConfigurationPropertyProviderDescriptor(IConfigurationPropertyProvider target, PropertyDescriptor descriptor)
        {
            this.target = target;
            this.descriptor = descriptor;

            if (descriptor.Attributes[typeof(ConfigurationPropertyAttribute)] is ConfigurationPropertyAttribute propAttr)
            {
                this.PropertyName = $"{target.Name}.{propAttr.GetPropertyName(descriptor.Name)}";
                this.ScopeType = propAttr.ScopeType;
            }

            if (descriptor.Attributes[typeof(DefaultValueAttribute)] is DefaultValueAttribute defaultAttr)
            {
                this.DefaultValue = defaultAttr.Value;
            }

            this.PropertyType = descriptor.PropertyType;
            if (this.PropertyType.IsArray == true)
            {
                this.IsArray = true;
                this.PropertyType = this.PropertyType.GetElementType();
            }

            this.Comment = descriptor.Description;
        }

        public override Type PropertyType { get; }

        public override string PropertyName { get; }

        public override string Comment { get; }

        public override object DefaultValue { get; } = DBNull.Value;

        public override Type ScopeType { get; }

        public override object Value
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

        public override bool IsArray { get; }

        protected override void OnReset()
        {
            base.OnReset();
            this.descriptor.ResetValue(this.target);
        }
    }
}
