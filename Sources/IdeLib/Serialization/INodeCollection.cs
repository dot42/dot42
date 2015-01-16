using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Dot42.Ide.Serialization
{
    public interface INodeCollection : INotifyCollectionChanged, INotifyPropertyChanged, ISerializationNodeList, IEnumerable
    {
    }
}
