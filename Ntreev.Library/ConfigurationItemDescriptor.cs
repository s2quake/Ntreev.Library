using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ntreev.Library
{
    sealed class ConfigurationItemDescriptor : ConfigurationPropertyDescriptor
    {
        public ConfigurationItemDescriptor(string name, Type type, string comment, object defaultValue)
        {
            this.PropertyName = name;
            this.Comment = comment ?? string.Empty;
            this.DefaultValue = DefaultValue;
            this.PropertyType = type;
            if (this.PropertyType.IsArray == true)
            {
                this.IsArray = true;
                this.PropertyType = this.PropertyType.GetElementType();
            }
        }

        public ConfigurationItemDescriptor(string name, object value)
        {
            this.PropertyName = name;
            this.Comment = string.Empty;
            this.DefaultValue = DBNull.Value;
            this.PropertyType = value.GetType();
            if (this.PropertyType.IsArray == true)
            {
                this.IsArray = true;
                this.PropertyType = this.PropertyType.GetElementType();
            }
        }

        public override string PropertyName { get; }

        public override Type PropertyType { get; }

        public override string Comment { get; }

        public override object DefaultValue { get; }

        public override bool IsArray { get; }

        public override Type ScopeType => throw new NotImplementedException();

        public override object Value { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
