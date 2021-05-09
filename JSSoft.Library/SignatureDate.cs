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
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace JSSoft.Library
{
    [DataContract(Namespace = SchemaUtility.Namespace)]
    [Serializable]
    public struct SignatureDate
    {
        private string id;
        private DateTime dateTime;

        public SignatureDate(string user)
        {
            this.id = user ?? throw new ArgumentNullException(nameof(user));
            this.dateTime = DateTime.UtcNow;
        }

        public SignatureDate(DateTime dateTime)
        {
            this.id = string.Empty;
            this.dateTime = dateTime;
        }

        public SignatureDate(string user, DateTime dateTime)
        {
            this.id = user ?? throw new ArgumentNullException(nameof(user));
            this.dateTime = dateTime;
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}", this.id ?? "(null)", this.dateTime);
        }

        public string ToString(IFormatProvider provider)
        {
            return string.Format("{0}, {1}", this.id ?? "(null)", this.dateTime.ToString(provider));
        }

        public static SignatureDate Parse(string text)
        {
            var match = Regex.Match(text, "(?<id>.+)[,] (?<date>.+)", RegexOptions.ExplicitCapture);
            if (match.Success == false)
                throw new FormatException();
            var id = match.Groups["id"].Value;
            return new SignatureDate()
            {
                id = id == "(null)" ? null : id,
                dateTime = DateTime.Parse(match.Groups["date"].Value)
            };
        }

        public static SignatureDate Parse(string text, IFormatProvider provider)
        {
            var match = Regex.Match(text, "(?<id>.+)[,] (?<date>.+)", RegexOptions.ExplicitCapture);
            if (match.Success == false)
                throw new FormatException();
            var id = match.Groups["id"].Value;
            return new SignatureDate()
            {
                id = id == "(null)" ? null : id,
                dateTime = DateTime.Parse(match.Groups["date"].Value, provider)
            };
        }

        public SignatureDate ToLocalValue()
        {
#if DEBUG
            if (this.dateTime != DateTime.MinValue && this.dateTime.Kind != DateTimeKind.Utc)
            {
                throw new InvalidOperationException();
            }
#endif
            return new SignatureDate(this.id, this.dateTime.ToLocalTime());
        }

        public SignatureDate ToUniversalValue()
        {
#if DEBUG
            if (this.dateTime != DateTime.MinValue && this.dateTime.Kind == DateTimeKind.Utc)
            {
                throw new InvalidOperationException();
            }
#endif
            return new SignatureDate(this.id, this.dateTime.ToUniversalTime());
        }

        [DataMember(EmitDefaultValue = false)]
        public DateTime DateTime
        {
            get => this.dateTime;
            set => this.dateTime = value;
        }

        [DataMember(EmitDefaultValue = false)]
        public string ID
        {
            get => this.id;
            set => this.id = value;
        }

        public static bool operator ==(SignatureDate left, SignatureDate right)
        {
            return (left.id == right.id) && (left.dateTime == right.dateTime);
        }

        public static bool operator !=(SignatureDate left, SignatureDate right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            if (obj is SignatureDate == false)
                return false;
            return this == (SignatureDate)obj;
        }

        public override int GetHashCode()
        {
            return HashUtility.GetHashCode(this.id) ^ HashUtility.GetHashCode(this.dateTime);
        }

        public static readonly SignatureDate Empty = new(string.Empty, DateTime.MinValue);
    }
}
