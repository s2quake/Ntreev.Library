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
using System.ComponentModel;
using System.Reflection;

namespace Ntreev.Library
{
    sealed class ConfigurationPropertyProviderDescriptor : ConfigurationPropertyDescriptor
    {
        private readonly IConfigurationPropertyProvider target;
        private readonly PropertyInfo propertyInfo;

        internal ConfigurationPropertyProviderDescriptor(IConfigurationPropertyProvider target, PropertyInfo propertyInfo)
        {
            this.target = target;
            this.propertyInfo = propertyInfo;

            if (propertyInfo.GetCustomAttribute<ConfigurationPropertyAttribute>() is ConfigurationPropertyAttribute propAttr)
            {
                this.PropertyName = $"{target.Name}.{propAttr.GetPropertyName(propertyInfo.Name)}";
                this.ScopeType = propAttr.ScopeType;
            }

            if (propertyInfo.GetCustomAttribute<DefaultValueAttribute>() is DefaultValueAttribute defaultAttr)
            {
                this.DefaultValue = defaultAttr.Value;
            }

            this.PropertyType = propertyInfo.PropertyType;

            if (propertyInfo.GetCustomAttribute<DescriptionAttribute>() is DescriptionAttribute descriptionAttr)
            {
                this.Comment = descriptionAttr.Description;
            }
        }

        public override Type PropertyType { get; }

        public override string PropertyName { get; }

        public override string Comment { get; } = string.Empty;

        public override object DefaultValue { get; } = DBNull.Value;

        public override Type ScopeType { get; }

        public override object Value
        {
            get => this.propertyInfo.GetValue(this.target);
            set
            {
                if (value == null || value.GetType() == this.propertyInfo.PropertyType)
                {
                    this.propertyInfo.SetValue(this.target, value);
                }
                else
                {
                    this.propertyInfo.SetValue(this.target, XmlConvertUtility.ToValue($"{value}", this.propertyInfo.PropertyType));
                }
            }
        }
    }
}
