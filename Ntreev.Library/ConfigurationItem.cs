using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ntreev.Library
{
    public struct ConfigurationItem
    {
        private const string keySection = "section";
        private const string keySubSection = "subsection";
        private const string keyKey = "key";
        private static readonly string pattern = $"(?<{keySection}>[^.]+)[.]?(?<{keySubSection }>.*)[.](?<{keyKey}>.+)";
        private string section;
        private string subSection;
        private string key;

        public ConfigurationItem(string name)
        {
            var match = Regex.Match(name, pattern, RegexOptions.ExplicitCapture);
            this.section = match.Groups[keySection].Value;
            this.subSection = match.Groups[keySubSection].Value;
            this.key = match.Groups[keyKey].Value;
        }

        public ConfigurationItem(string section, string key)
            : this(section, string.Empty, key)
        {

        }

        public ConfigurationItem(string section, string subSection, string key)
        {
            this.section = section ?? throw new ArgumentNullException(nameof(section));
            this.subSection = subSection ?? throw new ArgumentNullException(nameof(subSection));
            this.key = key ?? throw new ArgumentNullException(nameof(key));
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

        public string Section => this.section;

        public string SubSection => this.subSection;

        public string Key => this.key;

        public string Name
        {
            get
            {
                if (this.subSection == string.Empty)
                    return $"{this.section}.{this.key}";
                return $"{this.section}.{this.subSection}.{this.key}";
            }
        }

        public static implicit operator string(ConfigurationItem configurationItem)
        {
            return configurationItem.Name;
        }
    }
}
