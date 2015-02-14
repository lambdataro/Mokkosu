using System.Collections.Generic;

namespace Mokkosu.Utils
{
    static class Global
    {
        static HashSet<string> _defined_list = new HashSet<string>();

        public static void DefineKey(string key)
        {
            _defined_list.Add(key);
        }

        public static void UnDefineKey(string key)
        {
            _defined_list.Remove(key);
        }

        public static bool IdDefineKey(string key)
        {
            return _defined_list.Contains(key);
        }
    }
}
