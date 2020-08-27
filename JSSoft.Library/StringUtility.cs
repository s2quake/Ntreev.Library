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

using JSSoft.Library.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace JSSoft.Library
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0017:개체 초기화 단순화", Justification = "<보류 중>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0063:간단한 'using' 문 사용", Justification = "<보류 중>")]
    public static class StringUtility
    {
        private static readonly string Passphrase = "seckey";

        public static string Encrypt(string text, string key)
        {
            byte[] results;
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();

            // Step 1. We hash the passphrase using MD5
            // We use the MD5 hash generator as the result is a 128 bit byte array
            // which is a valid length for the TripleDES encoder we use below

            MD5CryptoServiceProvider hashProvider = new MD5CryptoServiceProvider();
            byte[] sKey = hashProvider.ComputeHash(encoding.GetBytes(key));

            // Step 2. Create a new TripleDESCryptoServiceProvider object
            TripleDESCryptoServiceProvider algorithm = new TripleDESCryptoServiceProvider();

            // Step 3. Setup the encoder
            algorithm.Key = sKey;
            algorithm.Mode = CipherMode.ECB;
            algorithm.Padding = PaddingMode.PKCS7;

            // Step 4. Convert the input string to a byte[]
            byte[] dataToEncrypt = encoding.GetBytes(text);

            // Step 5. Attempt to encrypt the string
            try
            {
                ICryptoTransform encryptor = algorithm.CreateEncryptor();
                results = encryptor.TransformFinalBlock(dataToEncrypt, 0, dataToEncrypt.Length);
            }
            finally
            {
                // Clear the TripleDes and Hashprovider services of any sensitive information
                algorithm.Clear();
                hashProvider.Clear();
            }

            // Step 6. Return the encrypted string as a base64 encoded string
            return Convert.ToBase64String(results);
        }

        public static string Decrypt(string text, string key)
        {
            byte[] results;
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();

            // Step 1. We hash the passphrase using MD5
            // We use the MD5 hash generator as the result is a 128 bit byte array
            // which is a valid length for the TripleDES encoder we use below

            MD5CryptoServiceProvider hashProvider = new MD5CryptoServiceProvider();
            byte[] sKey = hashProvider.ComputeHash(encoding.GetBytes(key));

            // Step 2. Create a new TripleDESCryptoServiceProvider object
            TripleDESCryptoServiceProvider algorithm = new TripleDESCryptoServiceProvider();

            // Step 3. Setup the decoder
            algorithm.Key = sKey;
            algorithm.Mode = CipherMode.ECB;
            algorithm.Padding = PaddingMode.PKCS7;

            // Step 4. Convert the input string to a byte[]
            byte[] dataToDecrypt = Convert.FromBase64String(text);

            // Step 5. Attempt to decrypt the string
            try
            {
                ICryptoTransform decryptor = algorithm.CreateDecryptor();
                results = decryptor.TransformFinalBlock(dataToDecrypt, 0, dataToDecrypt.Length);
            }
            finally
            {
                // Clear the TripleDes and Hashprovider services of any sensitive information
                algorithm.Clear();
                hashProvider.Clear();
            }

            // Step 6. Return the decrypted string in UTF8 format
            return encoding.GetString(results);
        }

        public static string Encrypt(this string text)
        {
            byte[] results;
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();

            // Step 1. We hash the passphrase using MD5
            // We use the MD5 hash generator as the result is a 128 bit byte array
            // which is a valid length for the TripleDES encoder we use below

            MD5CryptoServiceProvider hashProvider = new MD5CryptoServiceProvider();
            byte[] sKey = hashProvider.ComputeHash(encoding.GetBytes(Passphrase));

            // Step 2. Create a new TripleDESCryptoServiceProvider object
            TripleDESCryptoServiceProvider algorithm = new TripleDESCryptoServiceProvider();

            // Step 3. Setup the encoder
            algorithm.Key = sKey;
            algorithm.Mode = CipherMode.ECB;
            algorithm.Padding = PaddingMode.PKCS7;

            // Step 4. Convert the input string to a byte[]
            byte[] dataToEncrypt = encoding.GetBytes(text);

            // Step 5. Attempt to encrypt the string
            try
            {
                ICryptoTransform encryptor = algorithm.CreateEncryptor();
                results = encryptor.TransformFinalBlock(dataToEncrypt, 0, dataToEncrypt.Length);
            }
            finally
            {
                // Clear the TripleDes and Hashprovider services of any sensitive information
                algorithm.Clear();
                hashProvider.Clear();
            }

            // Step 6. Return the encrypted string as a base64 encoded string
            return Convert.ToBase64String(results);
        }

        public static string Decrypt(this string text)
        {
            byte[] results;
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();

            // Step 1. We hash the passphrase using MD5
            // We use the MD5 hash generator as the result is a 128 bit byte array
            // which is a valid length for the TripleDES encoder we use below

            MD5CryptoServiceProvider hashProvider = new MD5CryptoServiceProvider();
            byte[] sKey = hashProvider.ComputeHash(encoding.GetBytes(Passphrase));

            // Step 2. Create a new TripleDESCryptoServiceProvider object
            TripleDESCryptoServiceProvider algorithm = new TripleDESCryptoServiceProvider();

            // Step 3. Setup the decoder
            algorithm.Key = sKey;
            algorithm.Mode = CipherMode.ECB;
            algorithm.Padding = PaddingMode.PKCS7;

            // Step 4. Convert the input string to a byte[]
            byte[] dataToDecrypt = Convert.FromBase64String(text);

            // Step 5. Attempt to decrypt the string
            try
            {
                ICryptoTransform decryptor = algorithm.CreateDecryptor();
                results = decryptor.TransformFinalBlock(dataToDecrypt, 0, dataToDecrypt.Length);
            }
            finally
            {
                // Clear the TripleDes and Hashprovider services of any sensitive information
                algorithm.Clear();
                hashProvider.Clear();
            }

            // Step 6. Return the decrypted string in UTF8 format
            return encoding.GetString(results);
        }

        public static string Compress(this string text)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            var memoryStream = new MemoryStream();
            using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
            {
                gZipStream.Write(buffer, 0, buffer.Length);
            }

            memoryStream.Position = 0;

            var compressedData = new byte[memoryStream.Length];
            memoryStream.Read(compressedData, 0, compressedData.Length);

            var gZipBuffer = new byte[compressedData.Length + 4];
            Buffer.BlockCopy(compressedData, 0, gZipBuffer, 4, compressedData.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gZipBuffer, 0, 4);
            return Convert.ToBase64String(gZipBuffer);
        }

        public static bool IsBase64String(string s)
        {
            s = s.Trim();
            return (s.Length % 4 == 0) && Regex.IsMatch(s, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None);

        }

        public static string Decompress(this string compressedText)
        {
            if (compressedText == string.Empty)
                return compressedText;
            try
            {
                if (IsBase64String(compressedText) == false)
                    return compressedText;
                byte[] gZipBuffer = Convert.FromBase64String(compressedText);
                using (var memoryStream = new MemoryStream())
                {
                    int dataLength = BitConverter.ToInt32(gZipBuffer, 0);
                    memoryStream.Write(gZipBuffer, 4, gZipBuffer.Length - 4);

                    var buffer = new byte[dataLength];

                    memoryStream.Position = 0;
                    using (var gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                    {
                        gZipStream.Read(buffer, 0, buffer.Length);
                    }

                    return Encoding.UTF8.GetString(buffer);
                }
            }
            catch
            {
                return compressedText;
            }
        }

        /// <summary>
        /// sql의 like문과 같은 동작을 수행합니다.
        /// </summary>
        [Obsolete]
        public static bool Like(this string text, string find)
        {
            if (string.IsNullOrEmpty(find) == true)
                return true;

            Regex regex = new Regex(@"\.|\$|\^|\{|\[|\(|\||\)|\*|\+|\?|\\");
            string pattern = regex.Replace(find, ch => @"\" + ch);
            pattern = pattern.Replace('_', '.');
            pattern = "^" + pattern.Replace("%", ".*") + "$";

            regex = new Regex(pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);

            return regex.IsMatch(text);
        }

        public static string TrimQuot(this string text)
        {
            if (text.StartsWith("\"") == true && text.Length > 2 && text.EndsWith("\"") == true)
            {
                text = text.Substring(1);
                text = text.Remove(text.Length - 1);
            }
            else if (text == "\"\"")
            {
                text = string.Empty;
            }
            else
            {
                text = text.Replace("\"\"", "\"");
            }
            return text;
        }

        public static string WrapQuot(this string text)
        {
            return Wrap(text, '\"');
        }

        public static string WrapSeparator(this string text)
        {
            return Wrap(text, PathUtility.SeparatorChar);
        }

        public static string Wrap(this string text, char c)
        {
            return c + text + c;
        }

        public static string Truncate(this string value, int maxChars)
        {
            return value.Length <= maxChars ? value : value.Substring(0, maxChars - 3) + "...";
        }

        public static string[] Split(string value)
        {
            return Split(value, ' ');
        }

        public static string[] Split(string value, char seperator)
        {
            return Split(value, seperator, true);
        }

        public static string[] Split(string value, char seperator, bool removeEmpty)
        {
            return value.Split(new char[] { seperator }, removeEmpty == true ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None);
        }

        public static string[] SplitPath(string path)
        {
            var pattern = @"^((""[^""]*"")|(\S+))";
            var match = Regex.Match(path, pattern);

            var pathList = new List<string>();
            while (match.Success)
            {
                var item = match.Value.Trim(new char[] { '\"' });
                pathList.Add(item);
                path = path.Substring(match.Length).Trim();
                match = Regex.Match(path, pattern);
            }

            return pathList.ToArray();
        }

        public static bool Glob(this string value, string pattern, bool caseSensitive)
        {
            if (string.IsNullOrEmpty(pattern) == true)
                return true;

            var pos = 0;

            while (pattern.Length != pos)
            {
                switch (pattern[pos])
                {
                    case '?':
                        break;

                    case '*':
                        for (var i = value.Length; i >= pos; i--)
                        {
                            if (Glob(value.Substring(i), pattern.Substring(pos + 1), caseSensitive))
                            {
                                return true;
                            }
                        }
                        return false;

                    default:
                        if (caseSensitive == true)
                        {
                            if (value.Length == pos || pattern[pos] != value[pos])
                            {
                                return false;
                            }
                        }
                        else
                        {
                            if (value.Length == pos || char.ToUpper(pattern[pos]) != char.ToUpper(value[pos]))
                            {
                                return false;
                            }
                        }
                        break;
                }

                pos++;
            }

            return value.Length == pos;
        }

        public static bool Glob(this string value, string pattern)
        {
            return Glob(value, pattern, false);
        }

        public static bool GlobMany(this string value, string pattern)
        {
            return GlobMany(value, pattern, false);
        }

        public static bool GlobMany(this string value, string pattern, bool caseSensitive)
        {
            if (string.IsNullOrEmpty(pattern) == true)
                return true;

            var patterns = Split(pattern, ';');
            foreach (var item in patterns)
            {
                if (value.Glob(item, caseSensitive) == true)
                    return true;
            }

            return false;
        }

        public static string EscapeChar(this string value, char c)
        {
            return value.Replace(c.ToString(), $"%{(int)c}");
        }

        public static string UnescapeChar(this string value, char c)
        {
            return value.Replace($"%{(int)c}", c.ToString());
        }

        /// <summary>
        /// https://github.com/JamesNK/Newtonsoft.Json/blob/master/Src/Newtonsoft.Json/Utilities/StringUtils.cs
        /// </summary>
        public static string ToCamelCase(string s)
        {
            if (string.IsNullOrEmpty(s) || !char.IsUpper(s[0]))
            {
                return s;
            }

            char[] chars = s.ToCharArray();

            for (int i = 0; i < chars.Length; i++)
            {
                if (i == 1 && !char.IsUpper(chars[i]))
                {
                    break;
                }

                bool hasNext = (i + 1 < chars.Length);
                if (i > 0 && hasNext && !char.IsUpper(chars[i + 1]))
                {
                    break;
                }

                char c;
#if HAVE_CHAR_TO_STRING_WITH_CULTURE
                c = char.ToLower(chars[i], CultureInfo.InvariantCulture);
#else
                c = char.ToLowerInvariant(chars[i]);
#endif
                chars[i] = c;
            }

            return new string(chars);
        }

        public static SecureString ToSecureString(this string unsecureString)
        {
            if (unsecureString == null)
                throw new ArgumentNullException(nameof(unsecureString));

            return unsecureString.Aggregate(new SecureString(), AppendChar, MakeReadOnly);
        }

        private static SecureString MakeReadOnly(SecureString ss)
        {
            ss.MakeReadOnly();
            return ss;
        }

        private static SecureString AppendChar(SecureString ss, char c)
        {
            ss.AppendChar(c);
            return ss;
        }
    }
}
