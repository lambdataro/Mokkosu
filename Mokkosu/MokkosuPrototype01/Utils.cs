using System.Collections.Generic;
using System.Linq;

namespace Mokkosu
{
    class ImmutableHashSet<T>
    {
        HashSet<T> _hashset;

        public ImmutableHashSet(IEnumerable<T> collection)
        {
            _hashset = new HashSet<T>(collection);
        }

        public bool Contains(T item)
        {
            return _hashset.Contains(item);
        }

        public int Count
        {
            get
            {
                return _hashset.Count;
            }
        }

        public T[] ToArray()
        {
            return _hashset.ToArray<T>();
        }
    }
}
