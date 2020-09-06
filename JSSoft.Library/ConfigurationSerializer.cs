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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace JSSoft.Library
{
    public class ConfigurationSerializer : IConfigurationSerializer
    {
        public const string Namespace = "http://schemas.jssoft.com/configs";
        public const string Configurations = nameof(Configurations);

        private readonly Dictionary<string, Type> typeByName = new Dictionary<string, Type>()
        {
            { "number", typeof(decimal) },
            { "boolean", typeof(bool) },
            { "datetime", typeof(DateTime) },
            { "timespan", typeof(TimeSpan) },
            { "string", typeof(string) },
        };

        private readonly Dictionary<Type, string> nameByType = new Dictionary<Type, string>()
        {
            { typeof(decimal), "number" },
            { typeof(bool), "boolean" },
            { typeof(DateTime), "datetime" },
            { typeof(TimeSpan), "timespan" },
            { typeof(string), "string" },
        };

        public void Serialize(Stream stream, IReadOnlyDictionary<string, object> properties)
        {
            var settings = new XmlWriterSettings() { Indent = true };
            using var writer = XmlWriter.Create(stream, settings);
            writer.WriteStartElement(Configurations, Namespace);
            this.WriteGroups(writer, properties);
            writer.WriteEndElement();
        }

        public void Deserialize(Stream stream, IDictionary<string, object> properties)
        {
            using var reader = XmlReader.Create(stream);
            reader.MoveToContent();
            if (reader.IsEmptyElement == false)
            {
                reader.ReadStartElement();
                this.ReadGroups(reader, properties);
                reader.ReadEndElement();
            }
            else
            {
                reader.Skip();
            }
        }

        public string Name => "xml";

        private void WriteGroups(XmlWriter writer, IReadOnlyDictionary<string, object> properties)
        {
            var query = from item in properties
                        let keyItem = new KeyItem(item.Key, item.Value)
                        group keyItem by keyItem.Namespace into g
                        select g;

            foreach (var item in query)
            {
                writer.WriteStartElement("Group");
                if (item.Key != string.Empty)
                    writer.WriteAttributeString("Name", item.Key);
                this.WriteGroup(writer, item);
                writer.WriteEndElement();
            }
        }

        private void WriteGroup(XmlWriter writer, IEnumerable<KeyItem> items)
        {
            var query = from item in items
                        group item by item.Type into g
                        select g;

            foreach (var item in query)
            {
                if (item.Key != string.Empty)
                {
                    writer.WriteStartElement(item.Key);
                }
                foreach (var i in item)
                {
                    writer.WriteStartElement(i.Name);
                    this.WriteField(writer, i.Value);
                    writer.WriteEndElement();
                }
                if (item.Key != string.Empty)
                {
                    writer.WriteEndElement();
                }
            }
        }

        private void WriteField(XmlWriter writer, object value)
        {
            if (value.GetType().IsArray == true)
            {
                var arrayValue = value as Array;
                var rank = arrayValue.Rank;
                var indics = new int[rank];
                var lengths = new int[rank];
                for (var i = 0; i < rank; i++)
                {
                    lengths[i] = arrayValue.GetLength(i);
                }
                writer.WriteAttributeString("Length", string.Join(",", lengths));

                this.WriteTypeAttribute(writer, arrayValue.GetType().GetElementType());
                this.WriteArray(writer, arrayValue, indics, 0);
            }
            else
            {
                if (value is decimal d)
                {
                    writer.WriteValue(d);
                }
                else if (value is bool b)
                {
                    writer.WriteValue(b);
                }
                else if (value is DateTime dt)
                {
                    this.WriteTypeAttribute(writer, dt.GetType());
                    writer.WriteValue(dt);
                }
                else if (value is TimeSpan ts)
                {
                    this.WriteTypeAttribute(writer, ts.GetType());
                    writer.WriteValue(ts);
                }
                else if (value is string s)
                {
                    if (decimal.TryParse(s, out _) == true || bool.TryParse(s, out _) == true)
                        this.WriteTypeAttribute(writer, s.GetType());
                    writer.WriteValue(s);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        private void WriteArray(XmlWriter writer, Array arrayValue, int[] indics, int dimension)
        {
            var length = arrayValue.GetLength(dimension);
            var rank = arrayValue.Rank;

            for (var i = 0; i < length; i++)
            {
                indics[dimension] = i;
                if (dimension + 1 < rank)
                {
                    writer.WriteStartElement("Item");
                    //writer.WriteAttributeString("length", $"{arrayValue.GetLength(dimension + 1)}");
                    this.WriteArray(writer, arrayValue, indics, dimension + 1);
                    writer.WriteEndElement();
                }
                else
                {
                    var value = arrayValue.GetValue(indics);
                    writer.WriteStartElement("Item");
                    this.WriteField(writer, value);
                    writer.WriteEndElement();
                }
            }
        }

        private void WriteTypeAttribute(XmlWriter writer, Type type)
        {
            writer.WriteAttributeString("Type", this.nameByType[type]);
        }

        private void ReadGroups(XmlReader reader, IDictionary<string, object> properties)
        {
            reader.MoveToContent();
            while (reader.NodeType == XmlNodeType.Element && reader.Name == "Group")
            {
                if (reader.IsEmptyElement == false)
                {
                    var groupName = reader.GetAttribute("Name");
                    reader.ReadStartElement("Group");
                    if (groupName != null)
                    {
                        this.ReadGroup(reader, groupName, properties);
                    }
                    else
                    {
                        this.ReadItem(reader, null, properties);
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

        private void ReadGroup(XmlReader reader, string parentName, IDictionary<string, object> properties)
        {
            reader.MoveToContent();
            while (reader.NodeType == XmlNodeType.Element)
            {
                if (reader.IsEmptyElement == false)
                {
                    var name = reader.Name;
                    var fullName = parentName != null ? $"{parentName}.{name}" : name;
                    reader.ReadStartElement(name);
                    this.ReadItem(reader, fullName, properties);
                    reader.ReadEndElement();
                }
                else
                {
                    reader.Skip();
                }
                reader.MoveToContent();
            }
        }

        private void ReadItem(XmlReader reader, string parentName, IDictionary<string, object> properties)
        {
            reader.MoveToContent();
            while (reader.NodeType == XmlNodeType.Element)
            {
                if (reader.IsEmptyElement == false)
                {
                    var name = reader.Name;
                    var fullName = parentName != null ? $"{parentName}.{name}" : name;
                    var type = reader.GetAttribute("Type");
                    var length = reader.GetAttribute("Length");
                    var rank = reader.GetAttribute("Rank");
                    reader.ReadStartElement(name);
                    if (length != null)
                    {
                        var elementType = this.typeByName[type];
                        var lengths = length.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(item => int.Parse(item)).ToArray();
                        var arrayValue = Array.CreateInstance(elementType, lengths);
                        var indics = new int[arrayValue.Rank];
                        this.ReadArray(reader, arrayValue, indics, 0);
                        properties.Add(fullName, arrayValue);
                    }
                    else
                    {
                        var value = this.ReadField(reader, type);
                        properties.Add(fullName, value);
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

        private void ReadArray(XmlReader reader, Array arrayValue, int[] indics, int dimension)
        {
            var length = arrayValue.GetLength(dimension);
            var elementType = arrayValue.GetType().GetElementType();
            for (var i = 0; i < length; i++)
            {
                indics[dimension] = i;
                reader.ReadStartElement("Item");
                if (dimension + 1 < indics.Length)
                {
                    this.ReadArray(reader, arrayValue, indics, dimension + 1);
                }
                else
                {
                    var type = this.nameByType[elementType];
                    var value = this.ReadField(reader, type);
                    arrayValue.SetValue(value, indics);
                }
                reader.ReadEndElement();
            }
        }

        private object ReadField(XmlReader reader, string type)
        {
            var value = reader.ReadContentAsString();
            if (type == null)
            {
                if (decimal.TryParse(value, out var d) == true)
                {
                    return d;
                }
                else if (decimal.TryParse(value, out var b) == true)
                {
                    return b;
                }
            }
            else if (type == "number")
            {
                return XmlConvert.ToDecimal(value);
            }
            else if (type == "boolean")
            {
                return XmlConvert.ToBoolean(value);
            }
            else if (type == "datetime")
            {
                return XmlConvert.ToDateTime(value, XmlDateTimeSerializationMode.Utc);
            }
            else if (type == "timespan")
            {
                return XmlConvert.ToTimeSpan(value);
            }
            else if (type == "string")
            {
                return value;
            }
            return value;
        }

        #region KeyItem

        class KeyItem
        {
            public KeyItem(string key, object value)
            {
                var items = new Stack<string>(StringUtility.Split(key, '.'));
                this.Name = items.Pop();
                if (items.Any() == true)
                {
                    this.Type = items.Pop();
                }
                if (items.Any() == true)
                {
                    this.Namespace = string.Join(".", items.Reverse());
                }
                this.Value = value;
            }

            public string Namespace { get; } = string.Empty;

            public string Type { get; } = string.Empty;

            public string Name { get; }

            public object Value { get; }
        }

        #endregion
    }
}
