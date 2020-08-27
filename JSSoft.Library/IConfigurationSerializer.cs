using System.Collections.Generic;
using System.IO;

namespace JSSoft.Library
{
    public interface IConfigurationSerializer
    {
        void Serialize(Stream stream, IReadOnlyDictionary<string, object> properties);

        void Deserialize(Stream stream, IDictionary<string, object> properties);

        string Name { get; }
    }
}
