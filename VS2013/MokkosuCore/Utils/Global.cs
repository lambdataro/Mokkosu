using System.Collections.Generic;
using System.Text;

namespace Mokkosu.Utils
{
    static class Global
    {
        static HashSet<string> _defined_list = new HashSet<string>();
        static StringBuilder _string_builder = new StringBuilder();

        public static void DefineKey(string key)
        {
            _defined_list.Add(key);
        }

        public static void UnDefineKey(string key)
        {
            _defined_list.Remove(key);
        }

        public static bool IsDefineKey(string key)
        {
            return _defined_list.Contains(key);
        }

        public static void OutputString(string str)
        {
            _string_builder.AppendLine(str);
        }

        public static string GetOutput()
        {
            return _string_builder.ToString();
        }

        public static void ClearOutput()
        {
            _string_builder.Clear();
        }
    }
}
