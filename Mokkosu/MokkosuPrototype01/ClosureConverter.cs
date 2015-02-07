using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mokkosu
{
    class ClosureConversionContext
    {
        public string ArgName { get; private set; }
        public string[] Capture { get; private set; }

        public ClosureConversionContext(string arg_name, string[] capture)
        {
            ArgName = arg_name;
            Capture = capture;
        }

        public int GetCaptureIndex(string name)
        {
            for (int i = 0; i < Capture.Length; i++)
            {
                if (Capture[i] == name)
                {
                    return i;
                }
            }

            return -1;
        }
    }

    class ClosureConversionResult
    {
        public Dictionary<string, SExpr> FunctionTable { get; private set; }
        public SExpr Main { get; private set; }

        public ClosureConversionResult(Dictionary<string, SExpr> table, SExpr main)
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
            sb.AppendFormat("=== Main ===\n");
            sb.Append(Main);

            return sb.ToString();
        }
    }

    static class ClosureConverter
    {
        static int _count = 0;
        static Dictionary<string, SExpr> _function_table;

        public static ClosureConversionResult Start(SExpr expr)
        {
            _function_table = new Dictionary<string, SExpr>();
            var ctx = new ClosureConversionContext("", new string[] { });
            var main = Conv(expr, ctx);
            return new ClosureConversionResult(_function_table, main);
        }

        static string GenName()
        {
            return string.Format("function@{0:000}", ++_count);
        }

        static ImmutableHashSet<string> FreeVars(SExpr expr)
        {
            if (expr is SConstInt)
            {
                return new ImmutableHashSet<string>();
            }
            else if (expr is SBinop)
            {
                var e = (SBinop)expr;
                var set1 = FreeVars(e.Lhs);
                var set2 = FreeVars(e.Rhs);
                return ImmutableHashSet<string>.Union(set1, set2);
            }
            else if (expr is SVar)
            {
                var e = (SVar)expr;
                return new ImmutableHashSet<string>(e.Name);
            }
            else if (expr is SFun)
            {
                var e = (SFun)expr;
                var set1 = FreeVars(e.Body);
                var set2 = new ImmutableHashSet<string>(e.ArgName);
                return ImmutableHashSet<string>.Diff(set1, set2);
            }
            else if (expr is SApp)
            {
                var e = (SApp)expr;
                var set1 = FreeVars(e.FunExpr);
                var set2 = FreeVars(e.ArgExpr);
                return ImmutableHashSet<string>.Union(set1, set2);
            }
            else if (expr is SLet)
            {
                var e = (SLet)expr;
                var set1 = FreeVars(e.E1);
                var set2 = FreeVars(e.E2);
                var set3 = new ImmutableHashSet<string>(e.VarName);
                var set4 = ImmutableHashSet<string>.Diff(set2, set3);
                return ImmutableHashSet<string>.Union(set1, set4);
            }
            else if (expr is SRec)
            {
                var e = (SRec)expr;
                var set1 = FreeVars(e.E1);
                var set2 = FreeVars(e.E2);
                var set3 = new ImmutableHashSet<string>(e.VarName);
                var set4 = ImmutableHashSet<string>.Union(set1, set2);
                return ImmutableHashSet<string>.Diff(set4, set3);
            }
            else if (expr is SIf)
            {
                var e = (SIf)expr;
                var set1 = FreeVars(e.CondExpr);
                var set2 = FreeVars(e.ThenExpr);
                var set3 = FreeVars(e.ElseExpr);
                var set4 = ImmutableHashSet<string>.Union(set1, set2);
                return ImmutableHashSet<string>.Union(set3, set4);
            }
            else if (expr is SPrint)
            {
                var e = (SPrint)expr;
                return FreeVars(e.Body);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        static SExpr Conv(SExpr expr, ClosureConversionContext ctx)
        {
            if (expr is SConstInt)
            {
                return expr;
            }
            else if (expr is SBinop)
            {
                var e = (SBinop)expr;
                var lhs = Conv(e.Lhs, ctx);
                var rhs = Conv(e.Rhs, ctx);
                if (e is SAdd)
                {
                    return new SAdd(lhs, rhs);
                }
                else if (e is SSub)
                {
                    return new SSub(lhs, rhs);
                }
                else if (e is SMul)
                {
                    return new SMul(lhs, rhs);
                }
                else if (e is SDiv)
                {
                    return new SDiv(lhs, rhs);
                }
                else if (e is SEq)
                {
                    return new SEq(lhs, rhs);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            else if (expr is SVar)
            {
                var e = (SVar)expr;
                if (e.Name == ctx.ArgName)
                {
                    return new SGetArg();
                }
                else
                {
                    var index = ctx.GetCaptureIndex(e.Name);
                    if (index == -1)
                    {
                        return e;
                    }
                    else
                    {
                        return new SGetEnv(index);
                    }
                }
            }
            else if (expr is SFun)
            {
                var e = (SFun)expr;
                var set1 = FreeVars(e.Body);
                var set2 = new ImmutableHashSet<string>(e.ArgName);
                var set3 = ImmutableHashSet<string>.Diff(set1, set2);
                var fv = set3.ToArray();
                var ctx2 = new ClosureConversionContext(e.ArgName, fv);
                var body = Conv(e.Body, ctx2);
                var name = GenName();
                _function_table.Add(name, body);
                var args = fv.Select(x => Conv(new SVar(x), ctx)).ToArray();
                return new SMakeClos(name, args);
            }
            else if (expr is SApp)
            {
                var e = (SApp)expr;
                var fun = Conv(e.FunExpr, ctx);
                var arg = Conv(e.ArgExpr, ctx);
                return new SApp(fun, arg);
            }
            else if (expr is SLet)
            {
                var e = (SLet)expr;
                var e1 = Conv(e.E1, ctx);
                var e2 = Conv(e.E2, ctx);
                return new SLet(e.VarName, e.VarType, e1, e2);
            }
            else if (expr is SRec)
            {
                var e = (SRec)expr;
                var e1 = Conv(e.E1, ctx);
                var e2 = Conv(e.E2, ctx);
                return new SRec(e.VarName, e.VarType, e1, e2);
            }
            else if (expr is SIf)
            {
                var e = (SIf)expr;
                var cond_expr = Conv(e.CondExpr, ctx);
                var then_expr = Conv(e.ThenExpr, ctx);
                var else_expr = Conv(e.ElseExpr, ctx);
                return new SIf(cond_expr, then_expr, else_expr);
            }
            else if (expr is SPrint)
            {
                var e = (SPrint)expr;
                var e2 = Conv(e.Body, ctx);
                return new SPrint(e2);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}