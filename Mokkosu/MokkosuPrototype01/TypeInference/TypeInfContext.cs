using Mokkosu.AST;
using Mokkosu.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mokkosu.TypeInference
{
    class TypeInfContext
    {
        // 定義済みのユーザ定義型の名前とカインドを記録
        public MEnv<int> UserTypes { get; set; }

        // 利用可能なタグ名と詳細を記録
        public MEnv<Tag> TagEnv { get; set; }

        public TypeInfContext()
        {
            UserTypes = new MEnv<int>();
            TagEnv = new MEnv<Tag>();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine("=== UserTypes ===");
            var user_types = UserTypes;
            while (!user_types.IsEmpty())
            {
                var head = user_types.Head;
                sb.AppendFormat("{0}({1})\n", head.Item1, head.Item2);
                user_types = user_types.Tail;
            }

            sb.AppendLine("=== TagEnv ===");
            var tag_env = TagEnv;
            while (!tag_env.IsEmpty())
            {
                var head = tag_env.Head;
                sb.AppendFormat("{0}: {1}({2}) ", head.Item1, head.Item2.Name, head.Item2.Index);
                sb.AppendFormat("Bounded = {0}, ", 
                    Utils.Utils.ListToString(head.Item2.Bounded.ToArray().ToList()));
                sb.AppendFormat("ArgTypes = {0}, ",
                    Utils.Utils.ListToString(head.Item2.ArgTypes));
                sb.AppendFormat("Type = {0}\n", head.Item2.Type);
                tag_env = tag_env.Tail;
            }

            return sb.ToString();
        }
    }
}
