using Mokkosu.AST;
using System.Collections.Generic;
using System.Text;

namespace Mokkosu.Parsing
{
    class ParseResult
    {
        public List<MTopExpr> TopExprs { get; private set; }

        public ParseResult(List<MTopExpr> top_exprs)
        {
            TopExprs = top_exprs;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            foreach (var expr in TopExprs)
            {
                sb.AppendLine(expr.ToString());
            }

            return sb.ToString();
        }
    }
}
