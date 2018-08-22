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
            if (ConfigurationBase.CanSupportType(type) == false)
                throw new ArgumentException();
            this.PropertyName = name;
            this.Comment = comment ?? string.Empty;
            this.DefaultValue = DefaultValue;
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
