using Mokkosu.AST;
using Mokkosu.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mokkosu.ClosureConversion
{
    static class ClosureConverter
    {
        static int _count = 0;
        static Dictionary<string, MExpr> _function_table;

        public static ClosureConversionResult Start(MExpr expr)
        {
            _function_table = new Dictionary<string, MExpr>();
            var ctx = new ClosureConversionContext("", new string[] { });
            var main = Conv(expr, ctx);
            return new ClosureConversionResult(_function_table, main);
        }

        static string GenName()
        {
            return string.Format("function@{0:000}", ++_count);
        }

        static MExpr Conv(MExpr expr, ClosureConversionContext ctx)
        {
            throw new NotImplementedException();
        }
    }
}
