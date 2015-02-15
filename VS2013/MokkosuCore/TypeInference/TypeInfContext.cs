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

        // 型環境
        public MEnv<MTypeScheme> TEnv { get; set; }

        public TypeInfContext()
        {
            UserTypes = new MEnv<int>();
            TEnv = new MEnv<MTypeScheme>();
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

            return sb.ToString();
        }
    }
}
