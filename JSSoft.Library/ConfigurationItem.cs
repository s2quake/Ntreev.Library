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
