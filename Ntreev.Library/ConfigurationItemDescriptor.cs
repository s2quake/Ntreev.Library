using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ntreev.Library
{
    public class ConfigurationItemDescriptor
    {
        internal ConfigurationItemDescriptor(string name, Type type, string comment, object defaultValue)
        {
            this.Name = name;
            this.Comment = comment ?? string.Empty;
            this.DefaultValue = DefaultValue;
            this.Type = type;
            if (this.Type.IsArray == true)
            {
                this.IsArray = true;
                this.Type = this.Type.GetElementType();
            }
        }

        internal ConfigurationItemDescriptor(string name, object value)
        {
            this.Name = name;
            this.Comment = string.Empty;
            this.DefaultValue = DBNull.Value;
            this.Type = value.GetType();
            if (this.Type.IsArray == true)
            {
                this.IsArray = true;
                this.Type = this.Type.GetElementType();
            }
        }

        public override string ToString()
        {
            return this.Name;
        }

        public string Name { get; }

        public Type Type { get; }

        public string Comment { get; }

        public object DefaultValue { get; }

        public bool IsArray { get; }
    }
}
