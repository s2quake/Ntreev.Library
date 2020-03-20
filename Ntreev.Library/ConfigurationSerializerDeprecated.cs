using Ntreev.Library.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Ntreev.Library
{
    public class ConfigurationSerializerDeprecated : IConfigurationSerializer
    {
        public const string Namespace = "http://schemas.ntreev.com/configurations";
        public const string RootElement = "configurations";
        public const string SerializableElement = "serializables";

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

        public void Deserialize(XmlReader reader, IDictionary<string, object> properties)
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
