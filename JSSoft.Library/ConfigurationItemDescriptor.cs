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

using System;

namespace JSSoft.Library
{
    sealed class ConfigurationItemDescriptor : ConfigurationPropertyDescriptor
    {
        public ConfigurationItemDescriptor(string name, Type type, string comment, object defaultValue)
        {
            if (ConfigurationBase.CanSupportType(type) == false)
                throw new ArgumentException();
            this.PropertyName = name;
            this.Comment = comment ?? string.Empty;
            this.DefaultValue = defaultValue;
            this.PropertyType = type;
        }

        public ConfigurationItemDescriptor(string name, object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (ConfigurationBase.CanSupportType(value.GetType()) == false)
                throw new ArgumentException();
            this.PropertyName = name;
            this.Comment = string.Empty;
            this.DefaultValue = DBNull.Value;
            this.PropertyType = value.GetType();
        }

        public override string PropertyName { get; }

        public override Type PropertyType { get; }

        public override string Comment { get; }

        public override object DefaultValue { get; }

        public override Type ScopeType => throw new NotImplementedException();

        public override object Value { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
