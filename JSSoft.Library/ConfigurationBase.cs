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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace JSSoft.Library
{
    public class ConfigurationBase : IReadOnlyDictionary<string, object>
    {
        private readonly Dictionary<string, object> items = new Dictionary<string, object>();
        private readonly Type scopeType;

        public ConfigurationBase()
        {
            this.scopeType = typeof(ConfigurationBase);
            this.Descriptors = new ConfigurationPropertyDescriptorCollection();
        }

        public ConfigurationBase(IEnumerable<IConfigurationPropertyProvider> providers)
            : this(typeof(ConfigurationBase), providers)
        {

        }

        public ConfigurationBase(Type scopeType, IEnumerable<IConfigurationPropertyProvider> providers)
            : this()
        {
            this.scopeType = scopeType;
            this.Descriptors = new ConfigurationPropertyDescriptorCollection(providers, scopeType);
        }

        public static bool CanSupportType(Type value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (value == typeof(sbyte) || value == typeof(byte) ||
                value == typeof(short) || value == typeof(ushort) ||
                value == typeof(int) || value == typeof(uint) ||
                value == typeof(long) || value == typeof(ulong) ||
                value == typeof(float) || value == typeof(double) || value == typeof(decimal) ||
                value == typeof(bool) || value == typeof(string) ||
                value == typeof(DateTime) || value == typeof(TimeSpan))
            {
                return true;
            }
            else if (value.IsArray == true && CanSupportType(value.GetElementType()) == true)
            {
                return true;
            }
            else
            {
                var converter = TypeDescriptor.GetConverter(value);
                if (converter.CanConvertTo(typeof(string)) == false)
                    return false;
                if (converter.CanConvertFrom(typeof(string)) == false)
                    return false;
                return true;
            }
        }

        public static Type ConvertToConfigType(Type value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (value == typeof(sbyte) || value == typeof(byte) ||
                value == typeof(short) || value == typeof(ushort) ||
                value == typeof(int) || value == typeof(uint) ||
                value == typeof(long) || value == typeof(ulong) ||
                value == typeof(float) || value == typeof(double) || value == typeof(decimal))
            {
                return typeof(decimal);
            }
            else if (value == typeof(bool) || value == typeof(string) ||
                value == typeof(DateTime) || value == typeof(TimeSpan))
            {
                return value;
            }
            else if (value.IsArray == true && CanSupportType(value.GetElementType()) == true)
            {
                var elementType = ConvertToConfigType(value.GetElementType());
                return elementType.MakeArrayType(value.GetArrayRank());
            }
            else
            {
                var converter = TypeDescriptor.GetConverter(value);
                if (converter.CanConvertTo(typeof(string)) == false)
                    throw new NotSupportedException("cannot supported type");
                if (converter.CanConvertFrom(typeof(string)) == false)
                    throw new NotSupportedException("cannot supported type");
                return typeof(string);
            }
        }

        public static object ConvertToConfig(object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            var type = value.GetType();
            if (CanSupportType(type) == false)
                throw new NotSupportedException("cannot supported type");

            if (type == typeof(sbyte) || type == typeof(byte) ||
                type == typeof(short) || type == typeof(ushort) ||
                type == typeof(int) || type == typeof(uint) ||
                type == typeof(long) || type == typeof(ulong) ||
                type == typeof(float) || type == typeof(double) || type == typeof(decimal))
            {
                if (value is IConvertible c)
                {
                    return c.ToDecimal(CultureInfo.CurrentCulture);
                }
                throw new NotImplementedException();
            }
            else if (value is DateTime dateTime)
            {
                return dateTime.ToUniversalTime();
            }
            else if (type == typeof(bool) || type == typeof(string) || type == typeof(TimeSpan))
            {
                return value;
            }
            else if (type.IsArray == true)
            {
                var sourceValue = value as Array;
                var rank = type.GetArrayRank();
                var lengths = new int[rank];
                for (var i = 0; i < rank; i++)
                {
                    lengths[i] = sourceValue.GetLength(i);
                }
                var elementType = ConvertToConfigType(type.GetElementType());
                var destValue = Array.CreateInstance(elementType, lengths);
                var indics = new int[lengths.Length];
                if (sourceValue.Length > 0)
                {
                    do
                    {
                        var v = sourceValue.GetValue(indics);
                        var c = ConvertToConfig(v);
                        destValue.SetValue(c, indics);
                    } while (IncrementIndics(indics, lengths));
                }
                return destValue;
            }
            else
            {
                var converter = TypeDescriptor.GetConverter(value);
                return converter.ConvertTo(null, CultureInfo.CurrentCulture, value, typeof(string));
            }

            static bool IncrementIndics(int[] indics, int[] lengths)
            {
                indics[lengths.Length - 1]++;
                for (var i = indics.Length - 1; i >= 0; i--)
                {
                    if (indics[i] >= lengths[i])
                    {
                        indics[i] = 0;
                        if (i == 0)
                            return false;
                        indics[i - 1]++;
                    }
                }
                return true;
            }
        }

        public static object ConvertFromConfig(object value, Type destinationType)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (destinationType == null)
                throw new ArgumentNullException(nameof(destinationType));

            if (value is decimal d)
            {
                if (d is IConvertible c)
                {
                    if (destinationType == typeof(sbyte))
                    {
                        return c.ToSByte(CultureInfo.CurrentCulture);
                    }
                    else if (destinationType == typeof(byte))
                    {
                        return c.ToByte(CultureInfo.CurrentCulture);
                    }
                    else if (destinationType == typeof(short))
                    {
                        return c.ToInt16(CultureInfo.CurrentCulture);
                    }
                    else if (destinationType == typeof(ushort))
                    {
                        return c.ToUInt16(CultureInfo.CurrentCulture);
                    }
                    else if (destinationType == typeof(int))
                    {
                        return c.ToInt32(CultureInfo.CurrentCulture);
                    }
                    else if (destinationType == typeof(uint))
                    {
                        return c.ToUInt32(CultureInfo.CurrentCulture);
                    }
                    else if (destinationType == typeof(long))
                    {
                        return c.ToInt64(CultureInfo.CurrentCulture);
                    }
                    else if (destinationType == typeof(ulong))
                    {
                        return c.ToUInt64(CultureInfo.CurrentCulture);
                    }
                    else if (destinationType == typeof(float))
                    {
                        return c.ToSingle(CultureInfo.CurrentCulture);
                    }
                    else if (destinationType == typeof(double))
                    {
                        return c.ToDouble(CultureInfo.CurrentCulture);
                    }
                    else if (destinationType == typeof(decimal))
                    {
                        return d;
                    }
                }
            }
            else if (value is bool)
            {
                if (destinationType == typeof(bool))
                    return value;
            }
            else if (value is DateTime)
            {
                if (destinationType == typeof(DateTime))
                    return value;
            }
            else if (value is TimeSpan)
            {
                if (destinationType == typeof(TimeSpan))
                    return value;
            }
            else if (value is string s)
            {
                var converter = TypeDescriptor.GetConverter(destinationType);
                return converter.ConvertFromString(s);
            }
            throw new NotImplementedException();
        }

        public void Commit(object target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            foreach (var item in target.GetType().GetProperties(bindingFlags))
            {
                if (item.CanRead == false || item.CanWrite == false)
                    continue;

                var attr = item.GetCustomAttribute<ConfigurationPropertyAttribute>();
                if (attr == null || attr.ScopeType != this.scopeType)
                    continue;

                if (target is IConfigurationPropertyProvider propertyProvider)
                {
                    var key = $"{propertyProvider.Name}{attr.GetPropertyName(item.Name)}";
                    this.SetValue(key, item.GetValue(target));
                }
                else
                {
                    var key = $"{target.GetType().FullName}.{attr.GetPropertyName(item.Name)}";
                    this.SetValue(key, item.GetValue(target));
                }
            }
        }

        public void Update(object target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            foreach (var item in target.GetType().GetProperties(bindingFlags))
            {
                if (item.CanRead == false || item.CanWrite == false)
                    continue;

                var attr = item.GetCustomAttribute<ConfigurationPropertyAttribute>();
                if (attr == null || attr.ScopeType != this.scopeType)
                    continue;

                try
                {
                    if (target is IConfigurationPropertyProvider propertyProvider)
                    {
                        var key = $"{propertyProvider.Name}{attr.GetPropertyName(item.Name)}";
                        if (this.items.ContainsKey(key) == true)
                        {
                            var value = this.GetValue(key, item.PropertyType);
                            item.SetValue(target, value);
                        }
                    }
                    else
                    {
                        var key = $"{target.GetType().FullName}.{attr.GetPropertyName(item.Name)}";
                        if (this.items.ContainsKey(key) == true)
                        {
                            var value = this.GetValue(key, item.PropertyType);
                            item.SetValue(target, value);
                        }
                    }
                }
                catch
                {

                }
            }
        }

        public bool Contains(string name)
        {
            return this.items.ContainsKey(name);
        }

        public void Remove(string name)
        {
            this.items.Remove(name);
        }

        public void Write(Stream stream, IConfigurationSerializer serializer)
        {
            var serializables = this.items.Where(item => this.Descriptors.ContainsKey(item.Key) == false);
            var descriptors = this.Descriptors.Where(item => this.items.ContainsKey(item.PropertyName) == false);
            var properties = new Dictionary<string, object>(serializables.Count() + descriptors.Count());
            if (serializables.Any() == true)
            {
                foreach (var item in serializables)
                {
                    properties.Add(item.Key, item.Value);
                }
            }
            if (descriptors.Any() == true)
            {
                foreach (var item in descriptors)
                {
                    if (object.Equals(item.Value, item.DefaultValue) == true)
                        continue;
                    properties.Add(item.PropertyName, item.Value);
                }
            }
            serializer.Serialize(stream, properties);
        }

        public void Read(Stream stream, IConfigurationSerializer serializer)
        {
            var properties = new Dictionary<string, object>();
            serializer.Deserialize(stream, properties);
            foreach (var item in properties)
            {
                this.SetValue(item.Key, item.Value);
            }
            foreach (var item in this.Descriptors)
            {
                if (this.Contains(item.PropertyName) == true)
                {
                    item.Value = this[item.PropertyName];
                }
            }
        }

        public void AddDescriptor(string name, Type type, string comment, object defaultValue)
        {
            if (this.Descriptors.ContainsKey(name) == true)
                throw new ArgumentException();

            this.Descriptors.Add(new ConfigurationItemDescriptor(name, type, comment, defaultValue));
        }

        public object GetValue(string key, Type destinationType)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            return ConvertFromConfig(this[key], destinationType);
        }

        public void SetValue(string key, object value)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            this.items[key] = ConvertToConfig(value);
        }

        public int Count => this.items.Count;

        public virtual string Name => "configurations";

        public object this[string name]
        {
            get => this.items[name];
            set
            {
                if (value != null)
                    this.SetValue(name, value);
                else
                    this.Remove(name);
            }
        }

        public ConfigurationPropertyDescriptorCollection Descriptors { get; private set; }

        private void WriteValue(XmlWriter writer, object value)
        {
            if (value.GetType().IsArray == true)
            {
                var enumrator = (value as IEnumerable).GetEnumerator();
                while (enumrator.MoveNext())
                {
                    writer.WriteStartElement("item");
                    this.WriteValue(writer, enumrator.Current);
                    writer.WriteEndElement();
                }
            }
            else
            {
                writer.WriteValue(value);
            }
        }

        #region IReadOnlyDictionary

        IEnumerable<string> IReadOnlyDictionary<string, object>.Keys => this.items.Keys;

        IEnumerable<object> IReadOnlyDictionary<string, object>.Values => this.items.Values;

        object IReadOnlyDictionary<string, object>.this[string key] => this.items[key];

        bool IReadOnlyDictionary<string, object>.ContainsKey(string key)
        {
            return this.items.ContainsKey(key);
        }

        bool IReadOnlyDictionary<string, object>.TryGetValue(string key, out object value)
        {
            return this.items.TryGetValue(key, out value);
        }

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
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
