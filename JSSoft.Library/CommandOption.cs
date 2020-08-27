using System;

namespace Ntreev.Library
{
    public class CommandOption
    {
        private readonly object key;
        private readonly object value;

        public CommandOption(string name)
        {
            this.key = name;
        }

        public CommandOption(char name)
        {
            this.key = name;
        }

        public CommandOption(string name, object value)
        {
            this.key = name;
            this.value = value;
        }

        public CommandOption(char name, object value)
        {
            this.key = name;
            this.value = value;
        }

        public override string ToString()
        {
            if (this.key is string)
            {
                if (this.value == null)
                    return $"--{this.key}";
                else
                    return $"--{this.key} {this.value}";
            }
            else if (this.key is char)
            {
                if (this.value == null)
                    return $"-{this.key}";
                else
                    return $"-{this.key} {this.value}";
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
