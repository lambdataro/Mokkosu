using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mokkosu.Utils
{
    static class Utils
    {
        public static string ListToString<T>(List<T> list)
        {
            if (list == null || list.Count == 0)
            {
                return "";
            }
            else
            {
                var sb = new StringBuilder();
                sb.Append(list.First());
                foreach (var item in list.Skip(1))
                {
                    sb.AppendFormat(", {0}", item);
                }
                return sb.ToString();
            }
        }
    }
}
