using System;
using System.Collections.Generic;
using System.Linq;

namespace Mokkosu.Utils
{
    class MEnv<T>
    {
        bool _is_empty;
        string _key;
        T _value;
        MEnv<T> _tail;

        public MEnv()
        {
            _is_empty = true;
            _key = "";
            _value = default(T);
            _tail = null;
        }

        public MEnv<T> Cons(string key, T value)
        {
            var env = new MEnv<T>();
            env._is_empty = false;
            env._key = key;
            env._value = value;
            env._tail = this;
            return env;
        }

        public bool IsEmpty()
        {
            return _is_empty;
        }

        public Tuple<string,T> Head
        {
            get
            {
                return new Tuple<string,T>(_key, _value);
            }
        }

        public MEnv<T> Tail
        {
            get
            {
                return _tail;
            }
        }

        public MEnv<T> Append(MEnv<T> other)
        {
            if (_is_empty)
            {
                return other;
            }
            else
            {
                var env = new MEnv<T>();
                env._is_empty = false;
                env._key = _key;
                env._value = _value;
                env._tail = _tail.Append(other);
                return env;
            }
        }

        public bool Lookup( string key, out T value)
        {
            if (_is_empty)
            {
                value = default(T);
                return false;
            }
            else if (_key == key)
            {
                value = _value;
                return true;
            }
            else
            {
                return _tail.Lookup(key, out value);
            }
        }

        public T Lookup(string key)
        {
            T value;
            var b = this.Lookup(key, out value);
            if (b)
            {
                return value;
            }
            else
            {
                throw new MError("Not Found: " + key);
            }
        }

        public bool Contains(string key)
        {
            if (_is_empty)
            {
                return false;
            }
            else if (_key == key)
            {
                return true;
            }
            else
            {
                return _tail.Contains(key);
            }
        }
    }
}
