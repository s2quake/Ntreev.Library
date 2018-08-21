using Ntreev.Library.IO;
using Ntreev.Library.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Ntreev.Library
{
    [XmlRoot(RootElement, Namespace = Namespace)]
    public class ConfigurationBase : IXmlSerializable, IReadOnlyDictionary<string, object>
    {
        public const string Namespace = "http://schemas.ntreev.com/configurations";
        public const string RootElement = "configurations";
        public const string SerializableElement = "serializables";

        private readonly Dictionary<string, object> items = new Dictionary<string, object>();
        private readonly ConfigurationPropertyDescriptorCollection descriptors;

        public ConfigurationBase()
        {
            this.descriptors = new ConfigurationPropertyDescriptorCollection();
        }

        public ConfigurationBase(IEnumerable<IConfigurationPropertyProvider> providers)
            : this(typeof(ConfigurationBase), providers)
        {

        }

        public ConfigurationBase(Type scopeType, IEnumerable<IConfigurationPropertyProvider> providers)
            : this()
        {
            this.descriptors = new ConfigurationPropertyDescriptorCollection(providers, scopeType);

            //foreach (var item in this.properties)
            //{
            //    this.descriptors.Add(new ConfigurationItemDescriptor(item.PropertyName, item.PropertyType, item.Comment, item.DefaultValue));
            //}
        }

        public static bool CanSupportType(Type value)
        {
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
                return false;
            }
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
                if (attr == null)
                    continue;

                //this[target.GetType(), attr.PropertyName ?? item.Name] = item.GetValue(target);
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
                if (item.CanWrite == false)
                    continue;

                var attr = item.GetCustomAttribute<ConfigurationPropertyAttribute>();
                if (attr == null)
                    continue;

                var subsection = string.Empty;
                var categoryAttr = item.GetCustomAttribute<CategoryAttribute>();
                if (categoryAttr != null)
                    subsection = categoryAttr.Category;

                var key = attr.PropertyName ?? item.Name;
                var configurationItem = new ConfigurationItem(section, subsection, key);

                if (this.descriptors.ContainsKey(configurationItem.Name) == false)
                    continue;

                var value = this.descriptors[configurationItem.Name];
                try
                {
                    item.SetValue(target, value);
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
            this.Write(filename, null);
        }

        public void Write(string filename, string schemaLocation)
        {
            if (this is IXmlSerializable serializable)
            {
                var settings = new XmlWriterSettings() { Indent = true };
                using (var stringWriter = new Utf8StringWriter())
                {
                    using (var writer = XmlWriter.Create(stringWriter, settings))
                    {
                        writer.WriteStartElement(RootElement, Namespace);
                        if (schemaLocation != null)
                        {
                            writer.WriteAttributeString("xsi", "schemaLocation", XmlSchema.InstanceNamespace, $"{Namespace} {schemaLocation}");
                        }
                        serializable.WriteXml(writer);
                        writer.WriteEndElement();
                    }
                    File.WriteAllText(filename, stringWriter.ToString());
                }
            }
            else
            {
                throw new NotImplementedException();
            }
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

        public void ReadSchema(string filename)
        {

        }

        public void Read(string filename)
        {
            if (this is IXmlSerializable serializable)
            {
                using (var reader = XmlReader.Create(filename))
                {
                    serializable.ReadXml(reader);
                }

                foreach (var item in this.descriptors)
                {
                    if (this.Contains(item.PropertyName) == true)
                    {
                        item.Value = this[item.PropertyName];
                    }
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public void AddDescriptor(string name, Type type, string comment, object defaultValue)
        {
            if (this.descriptors.ContainsKey(name) == true)
                throw new ArgumentException();

            this.descriptors.Add(new ConfigurationItemDescriptor(name, type, comment, defaultValue));
        }

        public int Count => this.items.Count;

        public virtual string Name => RootElement;

        public object this[string name]
        {
            get => this.items[name];
            set
            {
                var configItem = new ConfigurationItem(name);
                if (value != null)
                    this.SetValue(name, value);
                else
                    this.Remove(name);
            }
        }

        public ConfigurationPropertyDescriptorCollection Descriptors => this.descriptors;

        private void SetValue(string key, object value)
        {
            if (value != null && CanSupportType(value.GetType()) == false)
                throw new ArgumentException();
            this.items[key] = value;

            //if (this.descriptors.Contains(key) == false)
            //{
            //    this.descriptors.Add(new ConfigurationItemDescriptor(key, this.items[key]));
            //}
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

        //private static object ConvertValue(object value)
        //{
        //    if (value is sbyte || value is byte ||
        //        value is short || value is ushort ||
        //        value is int || value is uint ||
        //        value is long || value is ulong)
        //    {
        //        return value;
        //    }
        //    else if (value is float || value is double || value is decimal)
        //    {
        //        return value;
        //    }
        //    else if (value is bool b)
        //    {
        //        return b;
        //    }
        //    else if (value is string text)
        //    {
        //        return text;
        //    }
        //    else if (value is IEnumerable enumerable)
        //    {
        //        var enumrator = enumerable.GetEnumerator();
        //        var itemList = new List<object>();
        //        while (enumrator.MoveNext())
        //        {
        //            itemList.Add(enumrator.Current);
        //        }
        //        return itemList.ToArray();
        //    }
        //    else
        //    {
        //        return value.ToString();
        //    }
        //}

        internal static void WriteComment(XmlSchemaAnnotated annotated, string comment)
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

        #region IXmlSerializable

        XmlSchema IXmlSerializable.GetSchema()
        {
            var schema = new XmlSchema
            {
                TargetNamespace = Namespace
            };
            schema.Namespaces.Add(string.Empty, schema.TargetNamespace);
            schema.ElementFormDefault = XmlSchemaForm.Qualified;

            var rootElement = new XmlSchemaElement() { Name = RootElement };
            var rootComplexType = new XmlSchemaComplexType();
            var rootGroup = new XmlSchemaAll() { MinOccurs = 0 } as XmlSchemaGroupBase;
            rootComplexType.Particle = rootGroup;
            rootElement.SchemaType = rootComplexType;
            schema.Items.Add(rootElement);

            {
                var element = new XmlSchemaElement() { Name = SerializableElement, MinOccurs = 0 };
                var complexType = new XmlSchemaComplexType();
                var sequence = new XmlSchemaSequence();
                var itemElement = new XmlSchemaElement() { Name = "item", MaxOccursString = "unbounded" };
                var itemComplexType = new XmlSchemaComplexType();
                var itemComplexTypeParticle = new XmlSchemaSequence();
                itemComplexTypeParticle.Items.Add(new XmlSchemaAny() { ProcessContents = XmlSchemaContentProcessing.Skip });
                itemComplexType.Attributes.Add(new XmlSchemaAttribute() { Name = "name", Use = XmlSchemaUse.Required });
                itemComplexType.Attributes.Add(new XmlSchemaAttribute() { Name = "type", Use = XmlSchemaUse.Required });
                itemComplexType.Particle = itemComplexTypeParticle;
                itemElement.SchemaType = itemComplexType;
                sequence.Items.Add(itemElement);
                complexType.Particle = sequence;
                element.SchemaType = complexType;
                rootGroup.Items.Add(element);
            }

            foreach (var item in this.descriptors)
            {
                var group = rootGroup as XmlSchemaGroupBase;
                var element = new XmlSchemaElement() { Name = item.PropertyName, MaxOccurs = 1, MinOccurs = 0 };

                WriteComment(element, item.Comment);

                if (item.IsArray == true)
                {
                    var complexType = new XmlSchemaComplexType();
                    var sequence = new XmlSchemaSequence() { MaxOccursString = "unbounded", MinOccurs = 0 };
                    complexType.Particle = sequence;
                    element.SchemaType = complexType;
                    group.Items.Add(element);
                    element = new XmlSchemaElement() { Name = "item" };
                    group = sequence;
                }

                if (item.PropertyType == typeof(bool))
                {
                    element.SchemaTypeName = new XmlQualifiedName("boolean", XmlSchema.Namespace);
                }
                else if (item.PropertyType == typeof(string))
                {
                    element.SchemaTypeName = new XmlQualifiedName("string", XmlSchema.Namespace);
                }
                else if (item.PropertyType == typeof(float))
                {
                    element.SchemaTypeName = new XmlQualifiedName("float", XmlSchema.Namespace);
                }
                else if (item.PropertyType == typeof(double))
                {
                    element.SchemaTypeName = new XmlQualifiedName("double", XmlSchema.Namespace);
                }
                else if (item.PropertyType == typeof(sbyte))
                {
                    element.SchemaTypeName = new XmlQualifiedName("byte", XmlSchema.Namespace);
                }
                else if (item.PropertyType == typeof(byte))
                {
                    element.SchemaTypeName = new XmlQualifiedName("unsignedByte", XmlSchema.Namespace);
                }
                else if (item.PropertyType == typeof(short))
                {
                    element.SchemaTypeName = new XmlQualifiedName("short", XmlSchema.Namespace);
                }
                else if (item.PropertyType == typeof(ushort))
                {
                    element.SchemaTypeName = new XmlQualifiedName("unsignedShort", XmlSchema.Namespace);
                }
                else if (item.PropertyType == typeof(int))
                {
                    element.SchemaTypeName = new XmlQualifiedName("int", XmlSchema.Namespace);
                }
                else if (item.PropertyType == typeof(uint))
                {
                    element.SchemaTypeName = new XmlQualifiedName("unsignedInt", XmlSchema.Namespace);
                }
                else if (item.PropertyType == typeof(long))
                {
                    element.SchemaTypeName = new XmlQualifiedName("long", XmlSchema.Namespace);
                }
                else if (item.PropertyType == typeof(ulong))
                {
                    element.SchemaTypeName = new XmlQualifiedName("unsignedLong", XmlSchema.Namespace);
                }
                else if (item.PropertyType == typeof(DateTime))
                {
                    element.SchemaTypeName = new XmlQualifiedName("dateTime", XmlSchema.Namespace);
                }
                else if (item.PropertyType == typeof(TimeSpan))
                {
                    element.SchemaTypeName = new XmlQualifiedName("duration", XmlSchema.Namespace);
                }

                group.Items.Add(element);
            }

            return schema;
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

        string GetSchmeLocation(XmlReader reader)
        {
            if (reader.MoveToFirstAttribute() == true)
            {
                do
                {
                    if (reader.NamespaceURI == XmlSchema.InstanceNamespace && reader.LocalName == "schemaLocation")
                    {
                        return reader.Value;
                    }
                }
                while (reader.MoveToNextAttribute());
            }
            return null;
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            if (reader.NamespaceURI != Namespace)
                return;

            reader.ReadStartElement();
            reader.MoveToContent();

            while (reader.NodeType == XmlNodeType.Element)
            {
                var name = reader.Name;
                if (reader.IsEmptyElement == false)
                {
                    if (name == SerializableElement)
                    {
                        this.ReadSerializableElement(reader);
                    }
                    else if (ConfigurationItem.VerifyName(name) == true)
                    {
                        var descriptor = this.descriptors[name];
                        reader.ReadStartElement();

                        if (reader.NodeType == XmlNodeType.Text)
                        {
                            this[name] = this.ReadValue(reader, descriptor == null ? typeof(string) : descriptor.PropertyType);
                        }
                        else
                        {

                        }
                        reader.ReadEndElement();
                    }
                }
                else
                {
                    reader.Skip();
                }
                reader.MoveToContent();
            }
        }

        private void ReadSerializableElement(XmlReader reader)
        {
            reader.ReadStartElement();
            reader.MoveToContent();

            while (reader.NodeType == XmlNodeType.Element)
            {
                if (reader.IsEmptyElement == false)
                {
                    var name = reader.GetAttribute("name");
                    var type = reader.GetAttribute("type");
                    var runtimeType = Type.GetType(type);

                    reader.ReadStartElement();
                    var value = XmlSerializerUtility.Read(reader, runtimeType);
                    reader.ReadEndElement();
                    this.SetValue(name, value);
                }
                else
                {
                    reader.Skip();
                }
                reader.MoveToContent();
            }
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            var serializables = this.items.Where(item => this.descriptors.ContainsKey(item.Key) == false);
            if (serializables.Any() == true)
            {
                writer.WriteStartElement(SerializableElement);
                foreach (var item in serializables)
                {
                    writer.WriteStartElement("item");
                    writer.WriteAttributeString("name", item.Key);
                    writer.WriteAttributeString("type", item.Value.GetType().AssemblyQualifiedName);
                    XmlSerializerUtility.Write(writer, item.Value);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }

            var properties = this.items.Where(item => this.descriptors.ContainsKey(item.Key) == true);
            if (properties.Any() == true)
            {
                foreach (var item in properties)
                {
                    var key = item.Key;
                    var value = item.Value;
                    writer.WriteStartElement(key);
                    this.WriteValue(writer, value);
                    writer.WriteEndElement();
                }
            }
        }

        #endregion

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
