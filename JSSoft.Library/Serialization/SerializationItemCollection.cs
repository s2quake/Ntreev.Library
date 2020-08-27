using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;

namespace JSSoft.Library.Serialization
{
    [CollectionDataContract(ItemName = "Item", Namespace = SchemaUtility.Namespace)]
    public class SerializationItemCollection<T> : Collection<T>
    {
        public SerializationItemCollection()
        {

        }

        public SerializationItemCollection(IEnumerable<T> items)
            : base(items.ToList())
        {

        }
    }
}
