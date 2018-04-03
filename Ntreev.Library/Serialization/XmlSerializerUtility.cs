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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Ntreev.Library.Serialization
{
    public static class XmlSerializerUtility
    {
        private static Dictionary<Type, XmlSerializer> serializers = new Dictionary<Type, XmlSerializer>();

        public static XmlSerializer GetSerializer(Type type)
        {
            lock (serializers)
            {
                if (serializers.ContainsKey(type) == false)
                {
                    var s = XmlSerializer.FromTypes(new Type[] { type })[0];
                    serializers.Add(type, s);
                }

                return serializers[type];
            }
        }

        public static string GetString(object obj)
        {
            return GetString(obj, false);
        }

        public static string GetString(object obj, bool indent)
        {
            return GetString(obj, indent, false);
        }

        public static string GetString(object obj, bool indent, bool omitXmlDeclaration)
        {
            var options = XmlSerializerOptions.None;
            if (indent == true)
                options |= XmlSerializerOptions.Indent;
            if (omitXmlDeclaration == true)
                options |= XmlSerializerOptions.OmitXmlDeclaration;
            return GetString(obj, options);
        }

        public static string GetString(object obj, XmlSerializerOptions options)
        {
            var serializer = GetSerializer(obj.GetType());
            var settings = new XmlWriterSettings() { Encoding = Encoding.UTF8 };
            if (options.HasFlag(XmlSerializerOptions.Indent))
                settings.Indent = true;
            if (options.HasFlag(XmlSerializerOptions.OmitXmlDeclaration))
                settings.OmitXmlDeclaration = true;

            using (var sw = new Utf8StringWriter())
            using (var writer = XmlWriter.Create(sw, settings))
            {
                if (options.HasFlag(XmlSerializerOptions.OmitNamespace))
                {
                    var ns = new XmlSerializerNamespaces();
                    ns.Add(string.Empty, string.Empty);
                    serializer.Serialize(writer, obj, ns);
                }
                else
                {
                    serializer.Serialize(writer, obj);
                }
                writer.Close();
                return sw.ToString();
            }
        }

        public static void Write(XmlWriter writer, object obj)
        {
            var serializer = GetSerializer(obj.GetType());
            serializer.Serialize(writer, obj);
        }

        public static void Write(Stream stream, object obj)
        {
            Write(stream, obj, false);
        }

        public static void Write(Stream stream, object obj, bool indent)
        {
            Write(stream, obj, indent, false);
        }

        public static void Write(Stream stream, object obj, bool indent, bool omitXmlDeclaration)
        {
            var options = XmlSerializerOptions.None;
            if (indent == true)
                options |= XmlSerializerOptions.Indent;
            if (omitXmlDeclaration == true)
                options |= XmlSerializerOptions.OmitXmlDeclaration;
            Write(stream, obj, options);
        }

        public static void Write(Stream stream, object obj, XmlSerializerOptions options)
        {
            var serializer = GetSerializer(obj.GetType());
            var settings = new XmlWriterSettings() { Encoding = Encoding.UTF8 };
            if (options.HasFlag(XmlSerializerOptions.Indent))
                settings.Indent = true;
            if (options.HasFlag(XmlSerializerOptions.OmitXmlDeclaration))
                settings.OmitXmlDeclaration = true;

            using (var writer = XmlWriter.Create(stream, settings))
            {
                if (options.HasFlag(XmlSerializerOptions.OmitNamespace))
                {
                    var ns = new XmlSerializerNamespaces();
                    ns.Add(string.Empty, string.Empty);
                    serializer.Serialize(writer, obj, ns);
                }
                else
                {
                    serializer.Serialize(writer, obj);
                }
            }
        }

        public static void Write(string filename, object obj)
        {
            Write(filename, obj, false);
        }

        public static void Write(string filename, object obj, bool indent)
        {
            using (var stream = FileUtility.OpenWrite(filename))
            {
                Write(stream, obj, indent);
            }
        }

        public static void Write(string filename, object obj, XmlSerializerOptions options)
        {
            using (var stream = FileUtility.OpenWrite(filename))
            {
                Write(stream, obj, options);
            }
        }

        public static object Read(XmlReader reader, Type type)
        {
            var serializer = GetSerializer(type);
            return serializer.Deserialize(reader);
        }

        public static object Read(Stream stream, Type type)
        {
            var serializer = GetSerializer(type);
            return serializer.Deserialize(stream);
        }

        public static object Read(string filename, Type type)
        {
            using (var stream = File.OpenRead(filename))
            {
                return Read(stream, type);
            }
        }

        public static T Read<T>(XmlReader reader)
        {
            return (T)Read(reader, typeof(T));
        }

        public static T Read<T>(Stream stream)
        {
            return (T)Read(stream, typeof(T));
        }

        public static T Read<T>(string filename)
        {
            return (T)Read(filename, typeof(T));
        }

        public static object ReadString(string text, Type type)
        {
            using (var sr = new StringReader(text))
            using (var reader = XmlReader.Create(sr))
            {
                return Read(reader, type);
            }
        }

        public static T ReadString<T>(string text)
        {
            return (T)ReadString(text, typeof(T));
        }

        /// <param name="obj">인스턴스를 설정하여 타입 명시를 하지 않기 위해 쓰는 용도임</param>
        public static T ReadString<T>(T obj, string text)
        {
            return (T)ReadString(text, typeof(T));
        }
    }
}
