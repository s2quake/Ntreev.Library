using System.Collections.Generic;
using System.IO;

namespace Ntreev.Library
{
    public interface IConfigurationSerializer
    {
        bool Verify(Stream stream);

        void Serialize(Stream stream, IReadOnlyDictionary<string, object> properties);

        void Deserialize(Stream stream, IDictionary<string, object> properties);

        string Name { get; }
    }
}
