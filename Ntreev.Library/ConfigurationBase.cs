using Ntreev.Library.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Ntreev.Library
{
    public interface IConfigurationSerializer
    {
        void Serialize(Stream stream, IReadOnlyDictionary<string, object> properties);

        void Deserialize(Stream stream, IDictionary<string, object> properties);
    }

    [XmlRoot(RootElement, Namespace = Namespace)]
    public class ConfigurationBase : IReadOnlyDictionary<string, object>
    {
        public const string Namespace = "http://schemas.ntreev.com/configurations";
        public const string RootElement = "configurations";
        public const string SerializableElement = "serializables";

        private readonly Dictionary<string, object> items = new Dictionary<string, object>();
        private readonly Type scopeType;
        private IConfigurationSerializer serializer = new ConfigurationSerializerDeprecated();
        private IConfigurationSerializer serializer1 = new ConfigurationSerializer();

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
                value == typeof(float) || value == typeof(double) || value == typeof(decimal) ||
                value == typeof(bool) || value == typeof(string) ||
                value == typeof(DateTime) || value == typeof(TimeSpan))
            {
                return typeof(decimal);
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
                while (IncrementIndics(indics, lengths))
                {
                    var v = sourceValue.GetValue(indics);
                    var c = ConvertToConfig(v);
                    destValue.SetValue(c, indics);
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
            else if (value is bool b)
            {
                if (destinationType == typeof(bool))
                    return value;
            }
            else if (value is DateTime dt)
            {
                if (destinationType == typeof(DateTime))
                    return value;
            }
            else if (value is TimeSpan ts)
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

            var targetType = target.GetType();
            var section = target.GetType().Name;
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
                            item.SetValue(target, this.items[key]);
                        }
                    }
                    else
                    {
                        var key = $"{target.GetType().FullName}.{attr.GetPropertyName(item.Name)}";
                        if (this.items.ContainsKey(key) == true)
                        {
                            item.SetValue(target, this.items[key]);
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

        public void Write(string filename)
        {
            using var stream = File.OpenWrite(filename);
            using var stream1 = File.Open(filename + "1", FileMode.Create);
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
            //this.serializer.Serialize(stream, properties);
            this.serializer1.Serialize(stream1, properties);
        }

        public void WriteSchema(string filename)
        {
            if (this is IXmlSerializable serializable)
            {
                var schema = serializable.GetSchema();
                var settings = new XmlWriterSettings() { Indent = true };
                using (var stringWriter = new Utf8StringWriter())
                {
                    using (var xmlWriter = XmlWriter.Create(stringWriter, settings))
                    {
                        schema.Write(xmlWriter);
                    }
                    File.WriteAllText(filename, stringWriter.ToString());
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        //public void ReadSchema(string filename)
        //{

        //}

        public void Read(string filename)
        {
            using var stream = File.OpenRead(filename);
            using var stream1 = File.OpenRead(filename + "1");
            var properties = new Dictionary<string, object>();
            this.serializer.Deserialize(stream, properties);
            var properties1 = new Dictionary<string, object>();
            this.serializer1.Deserialize(stream1, properties1);

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

        public int Count => this.items.Count;

        public virtual string Name => RootElement;

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

        private void SetValue(string key, object value)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            this.items[key] = ConvertToConfig(value);
        }

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

        private object ReadValue(XmlReader reader, Type type)
        {
            if (type == typeof(bool))
            {
                return reader.ReadContentAsBoolean();
            }
            else if (type == typeof(string))
            {
                return reader.ReadContentAsString();
            }
            else if (type == typeof(float))
            {
                return reader.ReadContentAsFloat();
            }
            else if (type == typeof(double))
            {
                return reader.ReadContentAsDouble();
            }
            else if (type == typeof(sbyte))
            {
                return (sbyte)reader.ReadContentAsInt();
            }
            else if (type == typeof(byte))
            {
                return (byte)reader.ReadContentAsInt();
            }
            else if (type == typeof(short))
            {
                return (short)reader.ReadContentAsInt();
            }
            else if (type == typeof(ushort))
            {
                return (ushort)reader.ReadContentAsInt();
            }
            else if (type == typeof(int))
            {
                return (int)reader.ReadContentAsInt();
            }
            else if (type == typeof(uint))
            {
                return (uint)reader.ReadContentAsInt();
            }
            else if (type == typeof(long))
            {
                return (long)reader.ReadContentAsLong();
            }
            else if (type == typeof(ulong))
            {
                return (ulong)reader.ReadContentAsLong();
            }
            else if (type == typeof(DateTime))
            {
                return (DateTime)reader.ReadContentAsDateTime();
            }
            else if (type == typeof(TimeSpan))
            {
                return XmlConvert.ToTimeSpan(reader.ReadContentAsString());
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private void WriteComment(XmlSchemaAnnotated annotated, string comment)
        {
            if (string.IsNullOrEmpty(comment) == true)
                return;

            if (annotated.Annotation == null)
            {
                annotated.Annotation = new XmlSchemaAnnotation();
            }

            var annotation = annotated.Annotation;
            {
                var documentation = new XmlSchemaDocumentation();
                {
                    var doc = new XmlDocument();
                    var textNode = doc.CreateTextNode(comment);
                    documentation.Markup = new XmlNode[1] { textNode };
                }
                annotation.Items.Add(documentation);
            }
        }

        private void ReadSerializableElement(XmlReader reader)
        {
            //var items = new Dictionary<string, object>();
            //this.serializer.Deserialize(reader, items);
            //foreach (var item in items)
            //{
            //    this.SetValue(item.Key, item.Value);
            //}
        }

        //#region IXmlSerializable

        //XmlSchema IXmlSerializable.GetSchema()
        //{
        //    var schema = new XmlSchema
        //    {
        //        TargetNamespace = Namespace
        //    };
        //    schema.Namespaces.Add(string.Empty, schema.TargetNamespace);
        //    schema.ElementFormDefault = XmlSchemaForm.Qualified;

        //    var rootElement = new XmlSchemaElement() { Name = RootElement };
        //    var rootComplexType = new XmlSchemaComplexType();
        //    var rootGroup = new XmlSchemaAll() { MinOccurs = 0 } as XmlSchemaGroupBase;
        //    rootComplexType.Particle = rootGroup;
        //    rootElement.SchemaType = rootComplexType;
        //    schema.Items.Add(rootElement);

        //    {
        //        var element = new XmlSchemaElement() { Name = SerializableElement, MinOccurs = 0 };
        //        var complexType = new XmlSchemaComplexType();
        //        var sequence = new XmlSchemaSequence();
        //        var itemElement = new XmlSchemaElement() { Name = "item", MaxOccursString = "unbounded" };
        //        var itemComplexType = new XmlSchemaComplexType();
        //        var itemComplexTypeParticle = new XmlSchemaSequence();
        //        itemComplexTypeParticle.Items.Add(new XmlSchemaAny() { ProcessContents = XmlSchemaContentProcessing.Skip });
        //        itemComplexType.Attributes.Add(new XmlSchemaAttribute() { Name = "name", Use = XmlSchemaUse.Required });
        //        itemComplexType.Attributes.Add(new XmlSchemaAttribute() { Name = "type", Use = XmlSchemaUse.Required });
        //        itemComplexType.Particle = itemComplexTypeParticle;
        //        itemElement.SchemaType = itemComplexType;
        //        sequence.Items.Add(itemElement);
        //        complexType.Particle = sequence;
        //        element.SchemaType = complexType;
        //        rootGroup.Items.Add(element);
        //    }

        //    foreach (var item in this.Descriptors)
        //    {
        //        var group = rootGroup as XmlSchemaGroupBase;
        //        var element = new XmlSchemaElement() { Name = item.PropertyName, MaxOccurs = 1, MinOccurs = 0 };
        //        var propertyType = item.PropertyType;

        //        this.WriteComment(element, item.Comment);

        //        if (item.PropertyType.IsArray == true)
        //        {
        //            var complexType = new XmlSchemaComplexType();
        //            var sequence = new XmlSchemaSequence() { MaxOccursString = "unbounded", MinOccurs = 0 };
        //            complexType.Particle = sequence;
        //            element.SchemaType = complexType;
        //            group.Items.Add(element);
        //            element = new XmlSchemaElement() { Name = "item" };
        //            group = sequence;
        //            propertyType = item.PropertyType.GetElementType();
        //        }

        //        if (propertyType == typeof(bool))
        //        {
        //            element.SchemaTypeName = new XmlQualifiedName("boolean", XmlSchema.Namespace);
        //        }
        //        else if (propertyType == typeof(string))
        //        {
        //            element.SchemaTypeName = new XmlQualifiedName("string", XmlSchema.Namespace);
        //        }
        //        else if (propertyType == typeof(float))
        //        {
        //            element.SchemaTypeName = new XmlQualifiedName("float", XmlSchema.Namespace);
        //        }
        //        else if (propertyType == typeof(double))
        //        {
        //            element.SchemaTypeName = new XmlQualifiedName("double", XmlSchema.Namespace);
        //        }
        //        else if (propertyType == typeof(sbyte))
        //        {
        //            element.SchemaTypeName = new XmlQualifiedName("byte", XmlSchema.Namespace);
        //        }
        //        else if (propertyType == typeof(byte))
        //        {
        //            element.SchemaTypeName = new XmlQualifiedName("unsignedByte", XmlSchema.Namespace);
        //        }
        //        else if (propertyType == typeof(short))
        //        {
        //            element.SchemaTypeName = new XmlQualifiedName("short", XmlSchema.Namespace);
        //        }
        //        else if (propertyType == typeof(ushort))
        //        {
        //            element.SchemaTypeName = new XmlQualifiedName("unsignedShort", XmlSchema.Namespace);
        //        }
        //        else if (propertyType == typeof(int))
        //        {
        //            element.SchemaTypeName = new XmlQualifiedName("int", XmlSchema.Namespace);
        //        }
        //        else if (propertyType == typeof(uint))
        //        {
        //            element.SchemaTypeName = new XmlQualifiedName("unsignedInt", XmlSchema.Namespace);
        //        }
        //        else if (propertyType == typeof(long))
        //        {
        //            element.SchemaTypeName = new XmlQualifiedName("long", XmlSchema.Namespace);
        //        }
        //        else if (propertyType == typeof(ulong))
        //        {
        //            element.SchemaTypeName = new XmlQualifiedName("unsignedLong", XmlSchema.Namespace);
        //        }
        //        else if (propertyType == typeof(DateTime))
        //        {
        //            element.SchemaTypeName = new XmlQualifiedName("dateTime", XmlSchema.Namespace);
        //        }
        //        else if (propertyType == typeof(TimeSpan))
        //        {
        //            element.SchemaTypeName = new XmlQualifiedName("duration", XmlSchema.Namespace);
        //        }

        //        group.Items.Add(element);
        //    }

        //    return schema;
        //}

        //void IXmlSerializable.ReadXml(XmlReader reader)
        //{
        //    reader.MoveToContent();
        //    if (reader.NamespaceURI != Namespace)
        //        return;

        //    reader.ReadStartElement();
        //    reader.MoveToContent();

        //    while (reader.NodeType == XmlNodeType.Element)
        //    {
        //        var name = reader.Name;
        //        if (reader.IsEmptyElement == false)
        //        {
        //            if (name == SerializableElement)
        //            {
        //                this.ReadSerializableElement(reader);
        //            }
        //            else if (ConfigurationItem.VerifyName(name) == true)
        //            {
        //                reader.ReadStartElement();

        //                if (reader.NodeType == XmlNodeType.Text)
        //                {
        //                    if (this.Descriptors.ContainsKey(name) == true)
        //                    {
        //                        var descriptor = this.Descriptors[name];
        //                        descriptor.Value = this.ReadValue(reader, descriptor.PropertyType);
        //                    }
        //                    else
        //                    {
        //                        this[name] = this.ReadValue(reader, typeof(string));
        //                    }
        //                }
        //                else
        //                {

        //                }
        //                reader.ReadEndElement();
        //            }
        //        }
        //        else
        //        {
        //            reader.Skip();
        //        }
        //        reader.MoveToContent();
        //    }
        //}

        //void IXmlSerializable.WriteXml(XmlWriter writer)
        //{
        //    //var serializables = this.items.Where(item => this.Descriptors.ContainsKey(item.Key) == false);
        //    //var properties = this.Descriptors.Where(item => this.items.ContainsKey(item.PropertyName) == false);
        //    //var items = new Dictionary<string, object>(serializables.Count() + properties.Count());
        //    //if (serializables.Any() == true)
        //    //{
        //    //    foreach (var item in serializables)
        //    //    {
        //    //        items.Add(item.Key, item.Value);
        //    //    }
        //    //}
        //    //if (properties.Any() == true)
        //    //{
        //    //    foreach (var item in properties)
        //    //    {
        //    //        if (object.Equals(item.Value, item.DefaultValue) == true)
        //    //            continue;
        //    //        items.Add(item.PropertyName, item.Value);
        //    //    }
        //    //}
        //    //this.serializer.Serialize(writer, items);
        //}

        //#endregion

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
