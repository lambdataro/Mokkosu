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

        public MEnv<T> Cons(string key, T vlaue)
        {
            var env = new MEnv<T>();
            env._is_empty = false;
            env._key = key;
            env._value = _value;
            env._tail = this;
            return env;
        }

        public bool IsEmpty(MEnv<T> env)
        {
            return env._is_empty;
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

        public bool Lookup(MEnv<T> env, string key, out T value)
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
                return _tail.Lookup(env, key, out value);
            }
        }

        public T Lookup(MEnv<T> env, string key)
        {
            T value;
            var b = this.Lookup(env, key, out value);
            if (b)
            {
                return value;
            }
            else
            {
                throw new MError("Not Found: " + key);
            }
        }
    }
}
