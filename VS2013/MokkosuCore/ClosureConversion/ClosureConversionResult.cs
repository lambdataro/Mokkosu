using Mokkosu.AST;
using System.Collections.Generic;
using System.Text;

namespace Mokkosu.ClosureConversion
{
    class ClosureConversionResult
    {
        public Dictionary<string, MExpr> FunctionTable { get; private set; }
        public MExpr Main { get; private set; }

        public ClosureConversionResult(Dictionary<string, MExpr> table, MExpr main)
        {
            FunctionTable = table;
            Main = main;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var item in FunctionTable)
            {
                sb.AppendFormat("=== {0} ===\n", item.Key);
                sb.Append(item.Value);
                sb.Append("\n");
            }
            sb.Append("=== Main ===\n");
            sb.Append(Main);

            return sb.ToString();
        }
    }
}
