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
        public const string Namespace = "http://schemas.ntreev.com/configuration";
        public const string RootElement = "configuration";

        private readonly ConfigurationItemDescriptorCollection descriptors;
        private readonly Dictionary<string, object> items = new Dictionary<string, object>();
        private ConfigurationPropertyDescriptorCollection properties;

        public ConfigurationBase()
        {
            this.descriptors = new ConfigurationItemDescriptorCollection(this);
            this.properties = new ConfigurationPropertyDescriptorCollection();
    }

        protected ConfigurationBase(IEnumerable<IConfigurationPropertyProvider> providers)
            : this(typeof(ConfigurationBase), providers)
        {

        }

        protected ConfigurationBase(Type scopeType, IEnumerable<IConfigurationPropertyProvider> providers)
            : this()
        {
            this.properties = new ConfigurationPropertyDescriptorCollection(providers, scopeType);

            foreach (var item in this.properties)
            {
                this.descriptors.Add(new ConfigurationItemDescriptor(item.PropertyName, item.PropertyType, item.Description, item.DefaultValue));
            }

            //foreach (var item in providers)
            //{
            //    foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(item))
            //    {
            //        var attr = descriptor.Attributes[typeof(ConfigurationPropertyAttribute)] as ConfigurationPropertyAttribute;
            //        if (attr == null || attr.ScopeType != scopeType)
            //            continue;


            //        this.ValidatePropertyType(descriptor.PropertyType);

            //        var configDescriptor = new ConfigurationPropertyDescriptor(item, descriptor);
            //        if (this.properties.ContainsKey(configDescriptor.PropertyName) == true)
            //            throw new ArgumentException($"{configDescriptor.PropertyName} property is already registered.");
            //        this.properties.Add(configDescriptor);
            //    }
            //}

            //var fileMap = new ExeConfigurationFileMap()
            //{
            //    ExeConfigFilename = path
            //};
            //this.config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);



            //foreach (var item in this.properties)
            //{
            //    if (this.Contains(item.PropertyName) == true)
            //    {
            //        item.Value = this[item.PropertyName];
            //    }
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
                value == typeof(DateTime) || value == typeof(TimeSpan) || 
                value == typeof(Guid))
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
            //if (target is IXmlSerializable)
            //{
            //    this[target.GetType(), typeof(IXmlSerializable).Name] = target;
            //}

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

                if (this.properties.ContainsKey(configurationItem.Name) == false)
                    continue;

                var value = this.properties[configurationItem.Name];
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

        public void Read(string filename)
        {
            if (this is IXmlSerializable serializable)
            {
                using (var reader = XmlReader.Create(filename))
                {
                    serializable.ReadXml(reader);
                }

                foreach (var item in this.properties)
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

        public ConfigurationItemDescriptorCollection Descriptors => this.descriptors;

        private void SetValue(string key, object value)
        {
            this.items[key] = ConvertValue(value);

            if (this.descriptors.Contains(key) == false)
            {
                this.descriptors.Add(new ConfigurationItemDescriptor(key, this.items[key]));
            }
        }

        private void WriteValue(XmlWriter writer, object value)
        {
            if (value.GetType().IsArray == true)
            {
                var enumrator = (value as IEnumerable).GetEnumerator();
                while (enumrator.MoveNext())
                {
                    writer.WriteStartElement($"item");
                    this.WriteValue(writer, enumrator.Current);
                    writer.WriteEndElement();
                }
            }
            else if (value is bool b)
            {
                writer.WriteValue(b);
            }
            else if (value is string text)
            {
                writer.WriteValue(text);
            }
            else if (value is long l)
            {
                writer.WriteValue(l);
            }
            else if (value is decimal d)
            {
                writer.WriteValue(d);
            }
        }

        private static object ConvertValue(object value)
        {
            if (value is sbyte || value is byte ||
                value is short || value is ushort ||
                value is int || value is uint ||
                value is long || value is ulong)
            {
                return value;
            }
            else if (value is float || value is double || value is decimal)
            {
                return value;
            }
            else if (value is bool b)
            {
                return b;
            }
            else if (value is string text)
            {
                return text;
            }
            else if (value is IEnumerable enumerable)
            {
                var enumrator = enumerable.GetEnumerator();
                var itemList = new List<object>();
                while (enumrator.MoveNext())
                {
                    itemList.Add(enumrator.Current);
                }
                return itemList.ToArray();
            }
            else
            {
                return value.ToString();
            }
        }

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

            foreach (var item in this.descriptors)
            {
                var group = rootGroup as XmlSchemaGroupBase;
                var element = new XmlSchemaElement() { Name = item.Name, MaxOccurs = 1, MinOccurs = 0 };

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

                if (item.Type == typeof(bool))
                {
                    element.SchemaTypeName = new XmlQualifiedName("boolean", XmlSchema.Namespace);
                }
                else if (item.Type == typeof(string))
                {
                    element.SchemaTypeName = new XmlQualifiedName("string", XmlSchema.Namespace);
                }
                else if (item.Type == typeof(float))
                {
                    element.SchemaTypeName = new XmlQualifiedName("float", XmlSchema.Namespace);
                }
                else if (item.Type == typeof(double))
                {
                    element.SchemaTypeName = new XmlQualifiedName("double", XmlSchema.Namespace);
                }
                else if (item.Type == typeof(sbyte))
                {
                    element.SchemaTypeName = new XmlQualifiedName("byte", XmlSchema.Namespace);
                }
                else if (item.Type == typeof(byte))
                {
                    element.SchemaTypeName = new XmlQualifiedName("unsignedByte", XmlSchema.Namespace);
                }
                else if (item.Type == typeof(short))
                {
                    element.SchemaTypeName = new XmlQualifiedName("short", XmlSchema.Namespace);
                }
                else if (item.Type == typeof(ushort))
                {
                    element.SchemaTypeName = new XmlQualifiedName("unsignedShort", XmlSchema.Namespace);
                }
                else if (item.Type == typeof(int))
                {
                    element.SchemaTypeName = new XmlQualifiedName("int", XmlSchema.Namespace);
                }
                else if (item.Type == typeof(uint))
                {
                    element.SchemaTypeName = new XmlQualifiedName("unsignedInt", XmlSchema.Namespace);
                }
                else if (item.Type == typeof(long))
                {
                    element.SchemaTypeName = new XmlQualifiedName("long", XmlSchema.Namespace);
                }
                else if (item.Type == typeof(ulong))
                {
                    element.SchemaTypeName = new XmlQualifiedName("unsignedLong", XmlSchema.Namespace);
                }
                else if (item.Type == typeof(DateTime))
                {
                    element.SchemaTypeName = new XmlQualifiedName("dateTime", XmlSchema.Namespace);
                }
                else if (item.Type == typeof(TimeSpan))
                {
                    element.SchemaTypeName = new XmlQualifiedName("duration", XmlSchema.Namespace);
                }
                else if (item.Type == typeof(Guid))
                {
                    element.SchemaTypeName = new XmlQualifiedName("guid", XmlSchema.Namespace);
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
                throw new NotImplementedException();
                //return (DateTime)reader.ReadContentAsDateTimeOffset();
            }
            else if (type == typeof(Guid))
            {
                return Guid.Parse(reader.ReadContentAsString());
            }
            else
            {
                throw new NotImplementedException();
            }
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
                if (reader.IsEmptyElement == false && ConfigurationItem.VerifyName(name) == true)
                {
                    var descriptor = this.descriptors[name];
                    reader.ReadStartElement();
                    if (reader.HasValue == true)
                    {
                        this[name] = this.ReadValue(reader, descriptor.Type);
                    }
                    else
                    {

                    }
                    reader.ReadEndElement();
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
            foreach (var item in this.items)
            {
                var key = item.Key;
                var value = item.Value;
                writer.WriteStartElement(key);
                this.WriteValue(writer, value);
                writer.WriteEndElement();
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
