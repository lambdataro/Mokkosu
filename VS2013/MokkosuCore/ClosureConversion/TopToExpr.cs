using Mokkosu.AST;
using Mokkosu.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mokkosu.ClosureConversion
{
    static class TopToExpr
    {
        public static MExpr Start(ParseResult parse_result)
        {
            var list = parse_result.TopExprs.Reverse<MTopExpr>();
            MExpr expr = new MUnit("");

            foreach (var topexpr in list)
            {
                if (topexpr is MUserTypeDef)
                {
                    // 何もしない
                }
                else if (topexpr is MTopDo)
                {
                    var top = (MTopDo)topexpr;
                    expr = new MDo(top.Pos, top.Expr, expr);
                }
                else if (topexpr is MTopLet)
                {
                    var top = (MTopLet)topexpr;
                    expr = new MLet(top.Pos, top.Pat, top.Expr, expr);
                }
                else if (topexpr is MTopFun)
                {
                    var top = (MTopFun)topexpr;
                    expr = new MFun(top.Pos, top.Items, expr);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            return expr;
        }
    }
}
