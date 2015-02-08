using System.Collections.Generic;
using System.Linq;

namespace Mokkosu.Utils
{
    class MSet<T>
    {
        HashSet<T> _hashset;

        public MSet()
        {
            _hashset = new HashSet<T>();
        }

        public MSet(T item)
        {
            _hashset = new HashSet<T>();
            _hashset.Add(item);
        }

        public MSet(IEnumerable<T> collection)
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
            return _hashset.ToArray();
        }

        public MSet<T> Union(MSet<T> other)
        {
            var set = new HashSet<T>(_hashset);
            set.UnionWith(other._hashset);
            return new MSet<T>(set);
        }

        public MSet<T> Diff(MSet<T> other)
        {
            var set = new HashSet<T>(_hashset);
            set.ExceptWith(other._hashset);
            return new MSet<T>(set);
        }
    }
}
