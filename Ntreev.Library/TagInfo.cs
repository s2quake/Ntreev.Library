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

using Ntreev.Library;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Ntreev.Library
{
    [TypeConverter(typeof(TagInfoTypeConverter))]
    [DataContract(Namespace = SchemaUtility.Namespace)]
    [Serializable]
    public struct TagInfo
    {
        public const char Separator = ',';
        private const string unusedString = "Unused";
        private const string allString = "All";
        private static string[] allValue = null;
        private static string[] unusedValue = new string[] { };

        private static Dictionary<string, string> textToColor = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);

        private string[] value;
        private int hash;

        static TagInfo()
        {
            textToColor.Add(unusedString, "#717171");
        }

        public TagInfo(string value)
        {
            this.value = FromString(value);
            this.hash = GetHashCode(this.value);
        }

        public TagInfo(params string[] values)
        {
            this.value = FromString(values);
            this.hash = GetHashCode(this.value);
        }

        public static void SetColor(string name, string colorName)
        {
            if (string.IsNullOrEmpty(colorName) == false)
                textToColor[name] = colorName;
            else
                textToColor[name] = null;
        }

        public static explicit operator string(TagInfo tags)
        {
            return tags.ToString();
        }

        public static explicit operator TagInfo(string value)
        {
            return new TagInfo(value);
        }

        public static TagInfo operator &(TagInfo t1, TagInfo t2)
        {
            var items = And(t1.value, t2.value);
            return new TagInfo(items);
        }

        public static TagInfo operator |(TagInfo t1, TagInfo t2)
        {
            var items = Or(t1.value, t2.value);
            return new TagInfo(items);
        }

        public static bool operator ==(TagInfo t1, TagInfo t2)
        {
            var h1 = t1.hash != 0 ? t1.hash : All.hash;
            var h2 = t2.hash != 0 ? t2.hash : All.hash;
            return h1 == h2;
        }

        public static bool operator !=(TagInfo t1, TagInfo t2)
        {
            return !(t1 == t2);
        }

        public override bool Equals(object obj)
        {
            if (obj is TagInfo == false)
                return false;
            return this.hash == ((TagInfo)obj).hash;
        }

        public override int GetHashCode()
        {
            return this.hash;
        }

        [DataMember]
        public string[] Value
        {
            get { return value; }
            set
            {
                this.value = FromString(value);
                this.hash = GetHashCode(this.value);
            }
        }

        public string Color
        {
            get
            {
                var text = this.ToString();
                if (textToColor.ContainsKey(text) == false)
                    return null;
                return textToColor[text];
            }
        }

        public override string ToString()
        {
            return ToString(this.value);
        }

        public readonly static TagInfo All = new TagInfo(allString) { value = allValue, hash = allString.ToLower().GetHashCode() };

        public readonly static TagInfo Unused = new TagInfo(unusedString) { value = unusedValue, hash = unusedString.ToLower().GetHashCode() };

        public readonly static TagInfo Empty = Unused;

        private static string[] FromString(string value)
        {
            if (value == null)
                return allValue;
            if (value.Trim() == string.Empty)
                return unusedValue;
            var items = value.Split(new char[] { Separator }, StringSplitOptions.RemoveEmptyEntries);
            if (items.Contains(allString, StringComparer.CurrentCultureIgnoreCase) == true || items.Contains("Both", StringComparer.CurrentCultureIgnoreCase) == true)
                return allValue;
            else if (items.Contains(unusedString, StringComparer.CurrentCultureIgnoreCase) == true)
                return unusedValue;
            foreach (var item in items)
            {
                IdentifierValidator.Validate(item.Trim());
            }
            return items.Select(item => item.Trim())
                        .Distinct(StringComparer.CurrentCultureIgnoreCase)
                        .OrderBy(item => item.ToLower())
                        .ToArray();
        }

        private static string[] FromString(string[] items)
        {
            if (items == null)
                return allValue;
            if (items.Length == 0)
                return unusedValue;
            if (items.Contains(allString, StringComparer.CurrentCultureIgnoreCase) == true || items.Contains("Both", StringComparer.CurrentCultureIgnoreCase) == true)
                return allValue;
            else if (items.Contains(unusedString, StringComparer.CurrentCultureIgnoreCase) == true)
                return unusedValue;
            foreach (var item in items)
            {
                IdentifierValidator.Validate(item.Trim());
            }
            return items.Select(item => item.Trim())
                        .Where(item => string.IsNullOrEmpty(item) == false)
                        .Distinct(StringComparer.CurrentCultureIgnoreCase)
                        .OrderBy(item => item.ToLower())
                        .ToArray();
        }

        private static string[] And(string[] items1, string[] items2)
        {
            if (items1 == allValue)
                return items2;
            if (items2 == allValue)
                return items1;
            return items1.Intersect(items2, StringComparer.CurrentCultureIgnoreCase).ToArray();
        }

        private static string[] Or(string[] items1, string[] items2)
        {
            if (items1 == allValue)
                return items1;
            if (items2 == allValue)
                return items2;
            return items1.Union(items2, StringComparer.CurrentCultureIgnoreCase).ToArray();
        }

        private static string ToString(string[] items)
        {
            if (items == TagInfo.Unused.value)
                return unusedString;
            else if (items == TagInfo.All.value)
                return allString;
            return string.Join($"{Separator} ", items);
        }

        private static int GetHashCode(string[] items)
        {
            return ToString(items).ToLower().GetHashCode();
        }
    }
}
