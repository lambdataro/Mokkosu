using System.Collections.Generic;
using System.Linq;

namespace Mokkosu
{
    class ImmutableHashSet<T>
    {
        HashSet<T> _hashset;

        public ImmutableHashSet()
        {
            _hashset = new HashSet<T>();
        }

        public ImmutableHashSet(T item)
        {
            _hashset = new HashSet<T>();
            _hashset.Add(item);
        }

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

        public static ImmutableHashSet<T> Union(ImmutableHashSet<T> set1, ImmutableHashSet<T> set2)
        {
            var set = new HashSet<T>(set1._hashset);
            set.UnionWith(set2._hashset);
            return new ImmutableHashSet<T>(set);
        }

        public static ImmutableHashSet<T> Diff(ImmutableHashSet<T> set1, ImmutableHashSet<T> set2)
        {
            var set = new HashSet<T>(set1._hashset);
            set.ExceptWith(set2._hashset);
            return new ImmutableHashSet<T>(set);
        }
    }

    class Env<T>
    {
        public string Key { get; private set; }
        public T Value { get; private set; }
        public Env<T> Tail { get; private set; }

        public Env(string key, T value, Env<T> tail)
        {
            Key = key;
            Value = value;
            Tail = tail;
        }

        public static Env<T> Empty()
        {
            return null;
        }

        public static bool IsEmpty(Env<T> assoc_list)
        {
            return assoc_list == null;
        }

        public static Env<T> Cons(string key, T value, Env<T> tail)
        {
            return new Env<T>(key, value, tail);
        }

        public static Env<T> Append(Env<T> list1, Env<T> list2)
        {
            if (IsEmpty(list1))
            {
                return list2;
            }
            else
            {
                return Cons(list1.Key, list1.Value, Append(list1.Tail, list2));
            }
        }

        public static bool Lookup(Env<T> list, string key, out T value)
        {
            if (list == null)
            {
                value = default(T);
                return false;
            }
            else if (list.Key == key)
            {
                value = list.Value;
                return true;
            }
            else
            {
                return Lookup(list.Tail, key, out value);
            }
        }
    }
}
