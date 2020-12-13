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
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace JSSoft.Library.Serialization
{
    public static class IXmlSerializableUtility
    {
        public static void Write(XmlWriter writer, IXmlSerializable obj)
        {
            var properties = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            foreach (var item in properties)
            {
                var attr = item.GetCustomAttribute(typeof(XmlIgnoreAttribute));
                if (attr != null)
                    continue;
                if (item.CanWrite == false)
                    continue;
                IXmlSerializableUtility.WriteElement(writer, item.GetValue(obj), item.Name, item.PropertyType, false);
            }
        }

        public static void WriteElement(XmlWriter writer, object value, string elementName, Type type, bool allowNullValue)
        {
            if (value == null)
            {
                if (allowNullValue == true)
                {
                    writer.WriteStartElement(elementName);
                    writer.WriteEndElement();
                }
                return;
            }


            writer.WriteStartElement(elementName);
            if (value.GetType() != type)
            {
                writer.WriteAttributeString("type", value.GetType().AssemblyQualifiedName);
            }

            if (type == typeof(byte[]))
            {
                var buffer = (byte[])value;
                writer.WriteAttributeString("length", $"{buffer.Length}");
                writer.WriteBinHex(buffer, 0, buffer.Length);
            }
            else if (type.IsArray == true)
            {
                writer.WriteAttributeString("type", type.GetElementType().AssemblyQualifiedName);
                if (value is Array items)
                {
                    foreach (var item in items)
                    {
                        WriteElement(writer, item, "Item", items.GetType().GetElementType(), true);
                    }
                }
            }
            else if (XmlConvertUtility.IsBaseType(value.GetType()))
            {
                writer.WriteString(XmlConvertUtility.ToString(value));
            }
            else
            {
                XmlSerializerUtility.Write(writer, value);
            }
            writer.WriteEndElement();
        }

        public static T Read<T>(XmlReader reader, T obj) where T : IXmlSerializable
        {
            if (reader.IsEmptyElement == false)
            {
                reader.ReadStartElement();
                reader.MoveToContent();
                var o = typeof(T).IsClass == true ? obj : (object)Activator.CreateInstance(typeof(T));
                while (reader.NodeType == XmlNodeType.Element)
                {
                    var property = typeof(T).GetProperty(reader.Name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    var value = IXmlSerializableUtility.ReadElement(reader, property.PropertyType);
                    property.SetValue(o, value);
                }
                reader.MoveToContent();
                reader.ReadEndElement();
                return (T)o;
            }
            else
            {
                reader.Skip();
                return obj;
            }
        }

        public static object ReadElement(XmlReader reader, Type type)
        {
            var value = null as object;

            if (reader.IsEmptyElement == false)
            {
                reader.ReadStartElement(reader.Name);
                if (type == typeof(byte[]))
                {
                    var buffer = new byte[1024];
                    using (var stream = new MemoryStream())
                    {
                        var readBytes = 0;
                        while ((readBytes = reader.ReadContentAsBinHex(buffer, 0, 1024)) > 0)
                        {
                            stream.Write(buffer, 0, readBytes);
                        }
                        stream.Position = 0;
                        buffer = new byte[stream.Length];
                        stream.Read(buffer, 0, buffer.Length);
                    }
                    value = buffer;
                    reader.MoveToContent();
                }
                else if (type.IsArray == true)
                {
                    var itemList = new List<object>();
                    while (reader.NodeType == XmlNodeType.Element)
                    {
                        var itemType = type.GetElementType();
                        if (reader.HasAttributes == true)
                        {
                            var typeName = reader.GetAttribute("type");
                            if (typeName != null)
                                itemType = Type.GetType(typeName);
                        }
                        var item = IXmlSerializableUtility.ReadElement(reader, itemType);
                        itemList.Add(item);
                    }
                    var arrayList = Array.CreateInstance(type.GetElementType(), itemList.Count);
                    for (var i = 0; i < itemList.Count; i++)
                    {
                        arrayList.SetValue(itemList[i], i);
                    }
                    value = arrayList;
                }
                else if (XmlConvertUtility.IsBaseType(type) == true)
                {
                    value = XmlConvertUtility.ToValue(reader.ReadContentAsString(), type);
                }
                else
                {
                    value = XmlSerializerUtility.Read(reader, type);
                }
                reader.ReadEndElement();
            }
            else
            {
                if (type.IsArray == true)
                {
                    value = Array.CreateInstance(type.GetElementType(), 0);
                }

                reader.Skip();
            }
            reader.MoveToContent();
            return value;
        }
    }
}
