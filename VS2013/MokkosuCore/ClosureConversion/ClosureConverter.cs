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
            var main = Conv(expr, ctx, new MSet<string>(), false);
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

        static MSet<string> FunPats(MFun expr)
        {
            var pats = new MSet<string>();
            
            foreach (var item in expr.Items)
            {
                pats = pats.Union(new MSet<string>(item.Name));
            }

            return pats;
        }

        static MExpr Conv(MExpr expr, ClosureConversionContext ctx, MSet<string> locals, bool istail)
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
                    if (locals.Contains(e.Name))
                    {
                        return expr;
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
            }
            else if (expr is MLambda)
            {
                var e = (MLambda)expr;
                var set = e.Body.FreeVars().Diff(e.ArgPat.FreeVars());
                var fv = set.ToArray();
                MExpr body;
                if (e.ArgPat is PVar)
                {
                    var ctx2 = new ClosureConversionContext(((PVar)e.ArgPat).Name, fv);
                    body = Conv(e.Body, ctx2, new MSet<string>(), true);
                }
                else
                {
                    var arg_name = GenArgName();
                    var ctx2 = new ClosureConversionContext(arg_name, fv);
                    body = Conv(new MMatch(e.Pos, e.ArgPat, new MBool(e.Pos, true), new MVar(arg_name), e.Body,
                        new MRuntimeError(e.Pos, "パターンマッチ失敗")), ctx2, new MSet<string>(), true);
                }
                var fun_name = GenFunctionName();
                _function_table.Add(fun_name, body);
                var args = fv.Select(x => Conv(new MVarClos(x), ctx, new MSet<string>(), false)).ToArray();
                return new MMakeClos(fun_name, args);
            }
            else if (expr is MApp)
            {
                var e = (MApp)expr;
                var fun = Conv(e.FunExpr, ctx, locals, false);
                var arg = Conv(e.ArgExpr, ctx, locals, false);
                var app = new MApp(e.Pos, fun, arg);
                app.TailCall = istail;
                return app;
            }
            else if (expr is MIf)
            {
                var e = (MIf)expr;
                var cond_expr = Conv(e.CondExpr, ctx, locals, false);
                var then_expr = Conv(e.ThenExpr, ctx, locals, istail);
                var else_expr = Conv(e.ElseExpr, ctx, locals, istail);
                return new MIf(e.Pos, cond_expr, then_expr, else_expr);
            }
            else if (expr is MMatch)
            {
                var e = (MMatch)expr;
                var locals2 = locals.Union(e.Pat.FreeVars());
                var expr1 = Conv(e.Expr, ctx, locals, false);
                var guard = Conv(e.Guard, ctx, locals2, false);
                var then_expr = Conv(e.ThenExpr, ctx, locals2, istail);
                var else_expr = Conv(e.ElseExpr, ctx, locals, istail);
                return new MMatch(e.Pos, e.Pat, guard, expr1, then_expr, else_expr);
            }
            else if (expr is MNil)
            {
                return expr;
            }
            else if (expr is MCons)
            {
                var e = (MCons)expr;
                var head = Conv(e.Head, ctx, locals, false);
                var tail = Conv(e.Tail, ctx, locals, false);
                return new MCons(e.Pos, head, tail);
            }
            else if (expr is MTuple)
            {
                var e = (MTuple)expr;
                var list = e.Items.Select(x => Conv(x, ctx, locals, false)).ToList();
                return new MTuple("", list);
            }
            else if (expr is MDo)
            {
                var e = (MDo)expr;
                var e1 = Conv(e.E1, ctx, locals, false);
                var e2 = Conv(e.E2, ctx, locals, istail);
                return new MDo(e.Pos, e1, e2);
            }
            else if (expr is MLet)
            {
                var e = (MLet)expr;
                var locals2 = locals.Union(e.Pat.FreeVars());
                var e1 = Conv(e.E1, ctx, locals, false);
                var e2 = Conv(e.E2, ctx, locals2, istail);
                return new MLet(e.Pos, e.Pat, e1, e2);
            }
            else if (expr is MFun)
            {
                var e = (MFun)expr;
                var locals2 = FunPats(e).Union(locals);
                var items = e.Items.Select(item =>
                    new MFunItem(item.Name, Conv(item.Expr, ctx, locals2, true))).ToList();
                var e2 = Conv(e.E2, ctx, locals2, istail);
                return new MFun(e.Pos, items, e2);
            }
            else if (expr is MFource)
            {
                var e = (MFource)expr;
                return Conv(e.Expr, ctx, locals, istail);
            }
            else if (expr is MRuntimeError)
            {
                return expr;
            }
            else if (expr is MPrim)
            {
                var e = (MPrim)expr;
                var list = e.Args.Select(x => Conv(x, ctx, locals, false)).ToList();
                return new MPrim(e.Pos, e.Name, list, e.ArgTypes, e.RetType);
            }
            else if (expr is MCallStatic)
            {
                var e = (MCallStatic)expr;
                var list = e.Args.Select(x => Conv(x, ctx, locals, false)).ToList();
                return new MCallStatic(e.Pos, e.ClassName, e.MethodName, list, e.Types, e.Info);
            }
            else if (expr is MCast)
            {
                var e = (MCast)expr;
                return new MCast(e.Pos, e.SrcTypeName, e.SrcType,
                    e.DstTypeName, e.DstType, Conv(e.Expr, ctx, locals, false));
            }
            else if (expr is MIsType)
            {
                var e = (MIsType)expr;
                return new MIsType(e.Pos, e.TypeName, e.Type, Conv(e.Expr, ctx, locals, false));
            }
            else if (expr is MNewClass)
            {
                var e = (MNewClass)expr;
                var list = e.Args.Select(x => Conv(x, ctx, locals, false)).ToList();
                return new MNewClass(e.Pos, e.ClassName, list, e.Types, e.Info);
            }
            else if (expr is MInvoke)
            {
                var e = (MInvoke)expr;
                var e2 = Conv(e.Expr, ctx, locals, false);
                var list = e.Args.Select(x => Conv(x, ctx, locals, false)).ToList();
                return new MInvoke(e.Pos, e2, e.ExprType, e.MethodName, list, e.Types, e.Info);
            }
            else if (expr is MDelegate)
            {
                var e = (MDelegate)expr;
                var e2 = Conv(e.Expr, ctx, locals, false);
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
                var e1 = Conv(e.Expr, ctx, locals, false);
                var e2 = Conv(e.Arg, ctx, locals, false);
                return new MSet(e.Pos, e1, e.ExprType, e.FieldName, e2, e.ArgType, e.Info);
            }
            else if (expr is MGet)
            {
                var e = (MGet)expr;
                var e1 = Conv(e.Expr, ctx, locals, false);
                return new MGet(e.Pos, e1, e.ExprType, e.FieldName, e.Info);
            }
            else if (expr is MSSet)
            {
                var e = (MSSet)expr;
                var e1 = Conv(e.Arg, ctx, locals, false);
                return new MSSet(e.Pos, e.ClassName, e.FieldName, e1, e.ArgType, e.Info);
            }
            else if (expr is MSGet)
            {
                return expr;
            }
            else if (expr is MNewArr)
            {
                var e = (MNewArr)expr;
                return new MNewArr(e.Pos, e.TypeName, e.Type, Conv(e.Size, ctx, locals, false));
            }
            else if (expr is MLdElem)
            {
                var e = (MLdElem)expr;
                var ary = Conv(e.Ary, ctx, locals, false);
                var idx = Conv(e.Idx, ctx, locals, false);
                return new MLdElem(e.Pos, e.TypeName, e.Type, ary, idx);
            }
            else if (expr is MStElem)
            {
                var e = (MStElem)expr;
                var ary = Conv(e.Ary, ctx, locals, false);
                var idx = Conv(e.Idx, ctx, locals, false);
                var val = Conv(e.Val, ctx, locals, false);
                return new MStElem(e.Pos, e.TypeName, e.Type, ary, idx, val);
            }
            else if (expr is MTry)
            {
                var e = (MTry)expr;
                var e2 = Conv(e.Expr, ctx, locals, false);
                var e3 = Conv(e.Handler, ctx, locals, false);
                return new MTry(e.Pos, e2, e3);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
