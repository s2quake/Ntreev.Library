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
using System.Collections;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace JSSoft.Library
{
    public static class HashUtility
    {
        public static string GetHashValue(Stream stream)
        {
            using var algorithm = SHA256.Create();
            return GetHashValue(algorithm, stream);
        }

        public static string GetHashValue(HashAlgorithm algorithm, Stream stream)
        {
            var data = algorithm.ComputeHash(stream);
            var sBuilder = new StringBuilder();
            for (var i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }

        public static string GetHashValueFromFile(string filename)
        {
            using var stream = File.OpenRead(filename);
            return GetHashValue(stream);
        }

        public static string GetHashValueFromFile(HashAlgorithm algorithm, string filename)
        {
            using var stream = File.OpenRead(filename);
            return GetHashValue(algorithm, stream);
        }

        public static string GetHashValue(byte[] buffer)
        {
            using var algorithm = SHA256.Create();
            return GetHashValue(algorithm, buffer);
        }

        public static string GetHashValue(HashAlgorithm algorithm, byte[] buffer)
        {
            var data = algorithm.ComputeHash(buffer, 0, buffer.Length);
            var sBuilder = new StringBuilder();
            for (var i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }

        public static string GetHashValue(params object[] args)
        {
            using var algorithm = SHA256.Create();
            return GetHashValue(algorithm, args);
        }

        public static string GetHashValue(HashAlgorithm algorithm, params object[] args)
        {
            var text = args.Any() == true ? args.Select(item => item == null ? "(null)" : item.ToString()).Aggregate((t, i) => t += i) : string.Empty;
            var data = algorithm.ComputeHash(Encoding.UTF8.GetBytes(text));
            var sBuilder = new StringBuilder();
            for (var i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }

        public static int GetHashCode<T>(T[] obj)
        {
            if (obj == null)
                return 0;
            return obj.Aggregate(0, (code, item) => code ^ item.GetHashCode());
        }

        public static int GetHashCode<T>(T obj)
        {
            if (obj == null)
                return 0;
            return obj.GetHashCode();
        }

        public static bool Equals<T>(T[] left, T[] right)
        {
            if (left == null && right == null)
            {
                return true;
            }
            if (left == null || right == null)
            {
                return false;
            }
            return left.SequenceEqual(right);
        }

        public static int GetHashCode(params object[] args)
        {
            var hash = 0;
            foreach (var item in args)
            {
                if (item == null)
                {
                    hash ^= 0;
                }
                else if (item is IEnumerable)
                {
                    hash ^= GetHashCode((item as IEnumerable).Cast<object>().ToArray());
                }
                else
                {
                    hash ^= item.GetHashCode();
                }
            }
            return hash;
        }
    }
}
