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
            var ctx = new ClosureConversionContext("", new string[] { }, new List<string>());
            var main = Conv(expr, ctx);
            return new ClosureConversionResult(_function_table, main);
        }

        static string GenFunctionName()
        {
            return string.Format("function@{0:000}", ++_count);
        }

        static string GenArgName()
        {
            return string.Format("arg@{0:000}", ++_count);
        }

        static MExpr Conv(MExpr expr, ClosureConversionContext ctx)
        {
            if (expr is MInt || expr is MDouble || expr is MString || expr is MChar ||
                expr is MUnit || expr is MBool)
            {
                return expr;
            }
            else if (expr is MVar)
            {
                var e = (MVar)expr;
                if (e.IsTag)
                {
                    return expr;
                }
                else if (e.Name == ctx.ArgName)
                {
                    return new MGetArg();
                }
                else
                {
                    var index = ctx.GetCaptureIndex(e.Name);
                    if (index == -1)
                    {
                        return expr;
                    }
                    else
                    {
                        return new MGetEnv(index);
                    }
                }
            }
            else if (expr is MLambda)
            {
                var e = (MLambda)expr;
                var set1 = e.Body.FreeVars().Diff(e.ArgPat.FreeVars());
                var set2 = new MSet<string>(ctx.RecList);
                // var set = set1.Union(set2);
                var set = set1;
                var fv = set.ToArray();
                MExpr body;
                if (e.ArgPat is PVar)
                {
                    var ctx2 = new ClosureConversionContext(((PVar)e.ArgPat).Name, fv, ctx.RecList);
                    body = Conv(e.Body, ctx2);
                }
                else
                {
                    var arg_name = GenArgName();
                    var ctx2 = new ClosureConversionContext(arg_name, fv, ctx.RecList);
                    body = Conv(new MMatch(e.Pos, e.ArgPat, new MBool(e.Pos, true), new MVar(arg_name), e.Body,
                        new MRuntimeError(e.Pos, "パターンマッチ失敗")), ctx2);
                }
                var fun_name = GenFunctionName();
                _function_table.Add(fun_name, body);
                var args = fv.Select(x => Conv(new MVarClos(x), ctx)).ToArray();
                return new MMakeClos(fun_name, args);
            }
            else if (expr is MApp)
            {
                var e = (MApp)expr;
                var fun = Conv(e.FunExpr, ctx);
                var arg = Conv(e.ArgExpr, ctx);
                return new MApp(e.Pos, fun, arg);
            }
            else if (expr is MIf)
            {
                var e = (MIf)expr;
                var cond_expr = Conv(e.CondExpr, ctx);
                var then_expr = Conv(e.ThenExpr, ctx);
                var else_expr = Conv(e.ElseExpr, ctx);
                return new MIf(e.Pos, cond_expr, then_expr, else_expr);
            }
            else if (expr is MMatch)
            {
                var e = (MMatch)expr;
                var expr1 = Conv(e.Expr, ctx);
                var guard = Conv(e.Guard, ctx);
                var then_expr = Conv(e.ThenExpr, ctx);
                var else_expr = Conv(e.ElseExpr, ctx);
                return new MMatch(e.Pos, e.Pat, guard, expr1, then_expr, else_expr);
            }
            else if (expr is MNil)
            {
                return expr;
            }
            else if (expr is MCons)
            {
                var e = (MCons)expr;
                var head = Conv(e.Head, ctx);
                var tail = Conv(e.Tail, ctx);
                return new MCons(e.Pos, head, tail);
            }
            else if (expr is MTuple)
            {
                var e = (MTuple)expr;
                var list =e.Items.Select(x => Conv(x, ctx)).ToList();
                return new MTuple("", list);
            }
            else if (expr is MDo)
            {
                var e = (MDo)expr;
                var e1 = Conv(e.E1, ctx);
                var e2 = Conv(e.E2, ctx);
                return new MDo(e.Pos, e1, e2);
            }
            else if (expr is MLet)
            {
                var e = (MLet)expr;
                var e1 = Conv(e.E1, ctx);
                var e2 = Conv(e.E2, ctx);
                return new MLet(e.Pos, e.Pat, e1, e2);
            }
            else if (expr is MFun)
            {
                var e = (MFun)expr;

                var rec_list = new List<string>(ctx.RecList);

                foreach (var item in e.Items)
                {
                    rec_list.Add(item.Name);
                }

                var ctx2 = new ClosureConversionContext(ctx.ArgName, ctx.Capture, rec_list);
                var items = e.Items.Select(item => 
                    new MFunItem(item.Name, Conv(item.Expr, ctx2))).ToList();
                var e2 = Conv(e.E2, ctx2);
                return new MFun(e.Pos, items, e2);
            }
            else if (expr is MFource)
            {
                var e = (MFource)expr;
                return Conv(e.Expr, ctx);
            }
            else if (expr is MRuntimeError)
            {
                return expr;
            }
            else if (expr is MPrim)
            {
                var e = (MPrim)expr;
                var list = e.Args.Select(x => Conv(x, ctx)).ToList();
                return new MPrim(e.Pos, e.Name, list, e.ArgTypes, e.RetType);
            }
            else if (expr is MCallStatic)
            {
                var e = (MCallStatic)expr;
                var list = e.Args.Select(x => Conv(x, ctx)).ToList();
                return new MCallStatic(e.Pos, e.ClassName, e.MethodName, list, e.Types, e.Info);
            }
            else if (expr is MCast)
            {
                var e = (MCast)expr;
                return new MCast(e.Pos, e.SrcTypeName, e.SrcType, 
                    e.DstTypeName, e.DstType, Conv(e.Expr, ctx));
            }
            else if (expr is MNewClass)
            {
                var e = (MNewClass)expr;
                var list = e.Args.Select(x => Conv(x, ctx)).ToList();
                return new MNewClass(e.Pos, e.ClassName, list, e.Types, e.Info);
            }
            else if (expr is MInvoke)
            {
                var e = (MInvoke)expr;
                var e2 = Conv(e.Expr, ctx);
                var list = e.Args.Select(x => Conv(x, ctx)).ToList();
                return new MInvoke(e.Pos, e2, e.ExprType, e.MethodName, list, e.Types, e.Info);
            }
            else if (expr is MDelegate)
            {
                var e = (MDelegate)expr;
                var e2 = Conv(e.Expr, ctx);
                return new MDelegate(e.Pos, e.ClassName, e2, e.ExprType, 
                    e.ParamType, e.ClassType, e.CstrInfo);
            }
            else if (expr is MVarClos)
            {
                var e = (MVarClos)expr;
                if (e.Name == ctx.ArgName)
                {
                    return new MGetArg();
                }
                else
                {
                    var index = ctx.GetCaptureIndex(e.Name);
                    if (index == -1)
                    {
                        return expr;
                    }
                    else
                    {
                        return new MGetEnv(index);
                    }
                }
            }
            else if (expr is MSet)
            {
                var e = (MSet)expr;
                var e1 = Conv(e.Expr, ctx);
                var e2 = Conv(e.Arg, ctx);
                return new MSet(e.Pos, e1, e.ExprType, e.FieldName, e2, e.ArgType, e.Info);
            }
            else if (expr is MGet)
            {
                var e = (MGet)expr;
                var e1 = Conv(e.Expr, ctx);
                return new MGet(e.Pos, e1, e.ExprType, e.FieldName, e.Info);
            }
            else if (expr is MSSet)
            {
                var e = (MSSet)expr;
                var e1 = Conv(e.Arg, ctx);
                return new MSSet(e.Pos, e.ClassName, e.FieldName, e1, e.ArgType, e.Info);
            }
            else if (expr is MSGet)
            {
                return expr;
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
