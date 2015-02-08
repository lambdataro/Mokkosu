using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mokkosu
{
    static class Evaluator
    {
        public static void Start(SExpr expr)
        {
            Eval(expr, Env<Value>.Empty);
        }

        static Value Eval(SExpr expr, Env<Value> env)
        {
            if (expr is SConstInt)
            {
                var e = (SConstInt)expr;
                return new IntValue(e.Value);
            }
            else if (expr is SBinop)
            {
                var e = (SBinop)expr;
                var v1 = Eval(e.Lhs, env);
                var v2 = Eval(e.Rhs, env);
                if (e is SAdd || e is SSub || e is SMul || e is SDiv)
                {
                    var i1 = (IntValue)v1;
                    var i2 = (IntValue)v2;
                    if (e is SAdd)
                    {
                        return new IntValue(i1.Value + i2.Value);
                    }
                    else if (e is SSub)
                    {
                        return new IntValue(i1.Value - i2.Value);
                    }
                    else if (e is SMul)
                    {
                        return new IntValue(i1.Value * i2.Value);
                    }
                    else if (e is SDiv)
                    {
                        return new IntValue(i1.Value / i2.Value);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                else if (e is SEq)
                {
                    var i1 = (IntValue)v1;
                    var i2 = (IntValue)v2;
                    if (e is SEq)
                    {
                        return i1.Value == i2.Value ? new IntValue(1) : new IntValue(0);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            else if (expr is SVar)
            {
                var e = (SVar)expr;
                Value v;
                Env<Value>.Lookup(env, e.Name, out v);
                while (v is ValueRef)
                {
                    v = ((ValueRef)v).Value;
                }
                return v;
            }
            else if (expr is SFun)
            {
                var e = (SFun)expr;
                return new FunValue(e.ArgName, e.Body, env);
            }
            else if (expr is SApp)
            {
                var e = (SApp)expr;
                var v1 = Eval(e.FunExpr, env);
                var v2 = Eval(e.ArgExpr, env);
                if (v1 is FunValue)
                {
                    var f = (FunValue)v1;
                    var env2 = Env<Value>.Cons(f.VarName, v2, f.Env);
                    return Eval(f.Body, env2);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            else if (expr is SLet)
            {
                var e = (SLet)expr;
                var v1 = Eval(e.E1, env);
                var env2 = Env<Value>.Cons(e.VarName, v1, env);
                return Eval(e.E2, env2);
            }
            else if (expr is SRec)
            {
                var e = (SRec)expr;
                var v = new ValueRef();
                var env2 = Env<Value>.Cons(e.VarName, v, env);
                v.Value = Eval(e.E1, env2);
                return Eval(e.E2, env2);
            }
            else if (expr is SIf)
            {
                var e = (SIf)expr;
                var i1 = (IntValue)Eval(e.CondExpr, env);
                if (i1.Value != 0)
                {
                    return Eval(e.ThenExpr, env);
                }
                else
                {
                    return Eval(e.ElseExpr, env);
                }
            }
            else if (expr is SPrint)
            {
                var e = (SPrint)expr;
                var v = Eval(e.Body, env);
                System.Console.WriteLine(v);
                return new IntValue(0);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
