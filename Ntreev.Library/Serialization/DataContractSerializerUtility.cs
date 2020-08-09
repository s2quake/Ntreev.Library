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
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace Ntreev.Library.Serialization
{
    public static class DataContractSerializerUtility
    {
        private static readonly Dictionary<Type, DataContractSerializer> serializers = new Dictionary<Type, DataContractSerializer>();

        public static DataContractSerializer GetSerializer(Type type)
        {
            lock (serializers)
            {
                if (serializers.ContainsKey(type) == false)
                {
                    serializers.Add(type, new DataContractSerializer(type));
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
            using var stream = new MemoryStream();
            Write(stream, obj, indent);
            stream.Position = 0;

            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        public static void Write(XmlWriter writer, object obj)
        {
            var serializer = GetSerializer(obj.GetType());
            serializer.WriteObject(writer, obj);
        }

        public static void Write(Stream stream, object obj)
        {
            Write(stream, obj, false);
        }

        public static void Write(Stream stream, object obj, bool indent)
        {
            var settings = new XmlWriterSettings() { Indent = indent, Encoding = Encoding.UTF8, };
            using var writer = XmlWriter.Create(stream, settings);
            Write(writer, obj);
        }

        public static void Write(string filename, object obj)
        {
            Write(filename, obj, false);
        }

        public static void Write(string filename, object obj, bool indent)
        {
            using var stream = FileUtility.OpenWrite(filename);
            Write(stream, obj, indent);
        }

        public static object Read(XmlReader reader, Type type)
        {
            var serializer = GetSerializer(type);
            return serializer.ReadObject(reader);
        }

        public static object Read(Stream stream, Type type)
        {
            var serializer = GetSerializer(type);
            return serializer.ReadObject(stream);
        }

        public static object Read(string filename, Type type)
        {
            using var stream = File.OpenRead(filename);
            return Read(stream, type);
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
            using var sr = new StringReader(text);
            using var reader = XmlReader.Create(sr);
            return Read(reader, type);
        }

        public static T ReadString<T>(string text)
        {
            return (T)ReadString(text, typeof(T));
        }
    }
}
