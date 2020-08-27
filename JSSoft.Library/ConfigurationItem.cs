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
using System.Text.RegularExpressions;

namespace JSSoft.Library
{
    [Obsolete]
    public struct ConfigurationItem
    {
        private const string keySection = "section";
        private const string keySubSection = "subsection";
        private const string keyKey = "key";
        private static readonly string pattern = $"(?<{keySection}>[^.]+)[.]?(?<{keySubSection }>.*)[.](?<{keyKey}>.+)";

        public ConfigurationItem(string name)
        {
            var match = Regex.Match(name, pattern, RegexOptions.ExplicitCapture);
            this.Section = match.Groups[keySection].Value;
            this.SubSection = match.Groups[keySubSection].Value;
            this.Key = match.Groups[keyKey].Value;
        }

        public ConfigurationItem(string section, string key)
            : this(section, string.Empty, key)
        {

        }

        public ConfigurationItem(string section, string subSection, string key)
        {
            this.Section = section ?? throw new ArgumentNullException(nameof(section));
            this.SubSection = subSection ?? throw new ArgumentNullException(nameof(subSection));
            this.Key = key ?? throw new ArgumentNullException(nameof(key));
        }

        public override string ToString()
        {
            return this.Name;
        }

        public static void ValidateName(string name)
        {
            var match = Regex.Match(name, pattern, RegexOptions.ExplicitCapture);
            if (match.Success == false)
                throw new ArgumentException("invalid name");
        }

        public static bool VerifyName(string name)
        {
            var match = Regex.Match(name, pattern, RegexOptions.ExplicitCapture);
            return match.Success;
        }

        public string Section { get; private set; }

        public string SubSection { get; private set; }

        public string Key { get; private set; }

        public string Name
        {
            get
            {
                if (this.SubSection == string.Empty)
                    return $"{this.Section}.{this.Key}";
                return $"{this.Section}.{this.SubSection}.{this.Key}";
            }
        }

        public static implicit operator string(ConfigurationItem configurationItem)
        {
            return configurationItem.Name;
        }
    }
}
