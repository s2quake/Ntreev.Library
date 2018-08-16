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

using Ntreev.Library.IO;
using Ntreev.Library.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Ntreev.Library
{
    public abstract class ConfigurationBaseLegacy
    {
        private Configuration config;
        private readonly ConfigurationPropertyDescriptorCollection properties = new ConfigurationPropertyDescriptorCollection();

        protected ConfigurationBaseLegacy(string path)
            : this(path, null, Enumerable.Empty<IConfigurationPropertyProvider>())
        {

        }

        protected ConfigurationBaseLegacy(string path, IEnumerable<IConfigurationPropertyProvider> providers)
            : this(path, null, providers)
        {

        }

        protected ConfigurationBaseLegacy(string path, Type scopeType, IEnumerable<IConfigurationPropertyProvider> providers)
        {
            foreach (var item in providers)
            {
                foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(item))
                {
                    var attr = descriptor.Attributes[typeof(ConfigurationPropertyAttribute)] as ConfigurationPropertyAttribute;
                    if (attr == null || attr.ScopeType != scopeType)
                        continue;

                    this.ValidatePropertyType(descriptor.PropertyType);

                    var configDescriptor = new ConfigurationPropertyDescriptor(item, descriptor);
                    if (this.properties.ContainsKey(configDescriptor.PropertyName) == true)
                        throw new ArgumentException($"{configDescriptor.PropertyName} property is already registered.");
                    this.properties.Add(configDescriptor);
                }
            }

            var fileMap = new ExeConfigurationFileMap()
            {
                ExeConfigFilename = path
            };
            this.config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);

            

            foreach (var item in this.properties)
            {
                if (this.Contains(typeof(ConfigurationBaseLegacy), item.PropertyName) == true)
                {
                    item.Value = this[typeof(ConfigurationBaseLegacy), item.PropertyName];
                }
            }
        }

        public object this[Type type, string key]
        {
            get
            {
                if (this.ContainsSection(type) == false)
                    return null;

                var section = this.GetSection(type);
                var element = section.Settings.Get(key);

                if (element == null)
                    return null;

                var valueXml = element.Value.ValueXml;

                var typeValue = valueXml.Attributes["type"];
                var valueType = typeValue == null ? typeof(string) : Type.GetType(typeValue.Value);
                if (element.SerializeAs == SettingsSerializeAs.String)
                {
                    var converter = TypeDescriptor.GetConverter(valueType);
                    return converter.ConvertFromString(element.Value.ValueXml.InnerText);
                }

                if (typeof(IXmlSerializable).IsAssignableFrom(valueType) == true)
                {
                    return element.Value.ValueXml.InnerXml;
                }

                return XmlSerializerUtility.ReadString(element.Value.ValueXml.InnerXml, valueType);
            }
            set
            {
                var section = this.GetSection(type);
                var element = section.Settings.Get(key);

                if (value == null && element != null)
                {
                    section.Settings.Remove(element);
                    return;
                }

                if (element == null)
                {
                    element = new SettingElement(key, SettingsSerializeAs.String);
                    section.Settings.Add(element);
                }

                var doc = new XmlDocument();
                var textNode = doc.CreateElement("value");
                doc.AppendChild(textNode);

                var converter = TypeDescriptor.GetConverter(value);
                if (converter.CanConvertFrom(typeof(string)) == true)
                {
                    var textValue = converter.ConvertToString(value);
                    if (textValue != string.Empty)
                        textNode.AppendChild(doc.CreateTextNode(textValue));
                    element.SerializeAs = SettingsSerializeAs.String;
                }
                else if (value is IXmlSerializable)
                {
                    var s = value as IXmlSerializable;
                    var settings = new XmlWriterSettings() { Indent = true, Encoding = Encoding.UTF8, OmitXmlDeclaration = true };
                    using (var sw = new Utf8StringWriter())
                    using (var writer = XmlWriter.Create(sw, settings))
                    {
                        s.WriteXml(writer);
                        writer.Close();
                        textNode.InnerXml = sw.ToString();
                        element.SerializeAs = SettingsSerializeAs.Xml;
                    }
                }
                else
                {
                    textNode.InnerXml = XmlSerializerUtility.GetString(value, true, true);
                    element.SerializeAs = SettingsSerializeAs.Xml;
                }

                if (value is string == false)
                {
                    var attribute = doc.CreateAttribute("type");
                    attribute.Value = value.GetType().AssemblyQualifiedName;
                    textNode.Attributes.Append(attribute);
                }

                element.Value = new SettingValueElement() { ValueXml = textNode };
                ConfigurationManager.RefreshSection(section.SectionInformation.SectionName);
            }
        }

        public bool Contains(Type type, string key)
        {
            if (this.ContainsSection(type) == false)
                return false;

            var section = this.GetSection(type);
            var element = section.Settings.Get(key);

            return element != null;
        }

        public void Commit()
        {
            try
            {
                this.Remove(typeof(ConfigurationBaseLegacy));
                foreach (var item in this.properties)
                {
                    if (item.ShouldSerializeValue == true)
                    {
                        this[typeof(ConfigurationBaseLegacy), item.PropertyName] = item.Value;
                    }
                }
                this.config.Save();
            }
            catch
            {

            }
        }

        public void Commit(object target)
        {
            if (target is IXmlSerializable)
            {
                this[target.GetType(), typeof(IXmlSerializable).Name] = target;
            }

            var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            foreach (var item in target.GetType().GetProperties(bindingFlags))
            {
                if (item.CanRead == false || item.CanWrite == false)
                    continue;

                var attr = item.GetCustomAttribute<ConfigurationPropertyAttribute>();
                if (attr == null)
                    continue;

                this[target.GetType(), attr.PropertyName ?? item.Name] = item.GetValue(target);
            }
        }

        public bool TryParse<T>(Type type, string key, out T value)
        {
            value = default(T);
            try
            {
                if (this.Contains(type, key) == false)
                    return false;

                value = (T)this[type, key];
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Update(object target)
        {
            if (target is IXmlSerializable == true)
            {
                var s = target as IXmlSerializable;
                if (this[target.GetType(), typeof(IXmlSerializable).Name] is string xml)
                {
                    using (var sr = new StringReader(xml))
                    using (var reader = XmlReader.Create(sr))
                    {
                        s.ReadXml(reader);
                    }
                }
            }

            var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            foreach (var item in target.GetType().GetProperties(bindingFlags))
            {
                if (item.CanWrite == false)
                    continue;

                var attr = item.GetCustomAttribute<ConfigurationPropertyAttribute>();
                if (attr == null)
                    continue;

                var propertyName = attr.PropertyName ?? item.Name;

                if (this.Contains(target.GetType(), propertyName) == false)
                    continue;

                var value = this[target.GetType(), propertyName];
                try
                {
                    item.SetValue(target, value);
                }
                catch
                {

                }
            }
        }

        public void Remove(Type type)
        {
            if (this.ContainsSection(type) == false)
                return;

            var group = this.GetGroup(type);
            var sectionName = type.FullName + ".Settings";
            group.Sections.Remove(sectionName);
        }

        public ConfigurationPropertyDescriptorCollection Properties
        {
            get { return this.properties; }
        }

        public abstract string Name
        {
            get;
        }

        private void ValidatePropertyExist(string propertyName)
        {
            if (this.properties.ContainsKey(propertyName) == false)
                throw new ArgumentException($"{propertyName} is not existed property.");
        }

        private void ValidatePropertyType(Type type)
        {
            if (type.IsEnum == true)
                return;
            if (type.IsArray == true && XmlConvertUtility.IsBaseType(type.GetElementType()) == true)
                return;
            if (XmlConvertUtility.IsBaseType(type) == true)
                return;
            throw new ArgumentException("${type} can not use by property type.");
        }

        private ConfigurationSectionGroup GetGroup(Type type)
        {
            var assemblyName = type.Assembly.GetName();
            var groupName = assemblyName.Name + "Settings";

            var group = this.config.GetSectionGroup(groupName);

            if (group == null)
            {
                group = new ConfigurationSectionGroup();
                this.config.SectionGroups.Add(groupName, group);
            }

            return group;
        }

        private ClientSettingsSection GetSection(Type type)
        {
            var group = this.GetGroup(type);
            var sectionName = type.FullName + ".Settings";

            var section = group.Sections[sectionName];

            if (section == null)
            {
                section = new ClientSettingsSection();
                group.Sections.Add(sectionName, section);
            }

            return section as ClientSettingsSection;
        }

        private bool ContainsGroup(Type type)
        {
            var assemblyName = type.Assembly.GetName();
            var groupName = assemblyName.Name + "Settings";

            var group = this.config.GetSectionGroup(groupName);
            return group != null;
        }

        private bool ContainsSection(Type type)
        {
            if (this.ContainsGroup(type) == false)
                return false;

            try
            {
                var group = this.GetGroup(type);
                var sectionName = type.FullName + ".Settings";
                var section = group.Sections[sectionName];
                return section != null;
            }
            catch
            {
                var fileMap = new ExeConfigurationFileMap()
                {
                    ExeConfigFilename = this.config.FilePath
                };
                this.config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
                var group = this.GetGroup(type);
                var sectionName = type.FullName + ".Settings";
                var section = group.Sections[sectionName];
                return section != null;
            }
        }

        private XmlNode CreateTextNode(string textValue)
        {
            var doc = new XmlDocument();
            var element = doc.CreateElement("value");
            element.InnerXml = textValue;
            return element;
        }
    }
}
