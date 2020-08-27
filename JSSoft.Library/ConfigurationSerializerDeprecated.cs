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

using JSSoft.Library.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace JSSoft.Library
{
    public class ConfigurationSerializerDeprecated : IConfigurationSerializer
    {
        public const string Namespace = "http://schemas.JSSoft.com/configurations";
        public const string RootElement = "configurations";
        public const string SerializableElement = "serializables";

        public bool Verify(Stream stream)
        {
            using var reader = XmlReader.Create(stream);
            //reader.ReadStartElement();
            //reader.MoveToContent();
            //while (reader.NodeType == XmlNodeType.Element)
            //{
            //    this.Deserialize(reader, properties);
            //}
            //reader.ReadEndElement();
            //reader.MoveToContent();

            return false;
        }

        public void Serialize(Stream stream, IReadOnlyDictionary<string, object> properties)
        {
            var settings = new XmlWriterSettings() { Indent = true };
            using var writer = XmlWriter.Create(stream, settings);
            writer.WriteStartElement(RootElement, Namespace);
            this.Serialize(writer, properties);
            writer.WriteEndElement();
        }

        public void Deserialize(Stream stream, IDictionary<string, object> properties)
        {
            using var reader = XmlReader.Create(stream);
            reader.ReadStartElement();
            reader.MoveToContent();
            while (reader.NodeType == XmlNodeType.Element)
            {
                this.Deserialize(reader, properties);
            }
            reader.ReadEndElement();
            reader.MoveToContent();
        }

        public string Name => "Deprecated";

        private void Serialize(XmlWriter writer, IReadOnlyDictionary<string, object> properties)
        {
            writer.WriteStartElement(SerializableElement);
            foreach (var item in properties)
            {
                writer.WriteStartElement("item");
                writer.WriteAttributeString("name", item.Key);
                writer.WriteAttributeString("type", item.Value.GetType().AssemblyQualifiedName);
                XmlSerializerUtility.Write(writer, item.Value);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        private void Deserialize(XmlReader reader, IDictionary<string, object> properties)
        {
            reader.ReadStartElement();
            reader.MoveToContent();

            while (reader.NodeType == XmlNodeType.Element)
            {
                if (reader.IsEmptyElement == false)
                {
                    var name = reader.GetAttribute("name");
                    var type = reader.GetAttribute("type");
                    try
                    {
                        var runtimeType = Type.GetType(type);
                        reader.ReadStartElement();
                        var value = XmlSerializerUtility.Read(reader, runtimeType);
                        reader.ReadEndElement();
                        properties.Add(name, value);
                    }
                    catch
                    {
                        reader.Skip();
                    }
                }
                else
                {
                    reader.Skip();
                }
                reader.MoveToContent();
            }
        }
    }
}
