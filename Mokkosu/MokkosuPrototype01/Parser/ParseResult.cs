using Mokkosu.AST;
using System.Collections.Generic;

namespace Mokkosu.Parser
{
    class ParseResult
    {
        public List<MTopExpr> TopExprs { get; private set; }

        public ParseResult(List<MTopExpr> top_exprs)
        {
            TopExprs = top_exprs;
        }
    }
}
