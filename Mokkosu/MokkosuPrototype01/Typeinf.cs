using System;

namespace Mokkosu
{
    /// <summary>
    /// 型推論器
    /// </summary>
    static class Typeinf
    {
        public static void Start(SExpr expr)
        {
        }

        static bool OccursCheck(int id, Type type)
        {
            if (type is TypeVar)
            {
                var t = (TypeVar)type;
                if (t.Id == id)
                {
                    return true;
                }
                else if (t.Var == null)
                {
                    return false;
                }
                else
                {
                    return OccursCheck(id, t.Var);
                }
            }
            else if (type is IntType)
            {
                return false;
            }
            else if (type is FunType)
            {
                var t = (FunType)type;
                return OccursCheck(id, t.ArgType) || OccursCheck(id, t.RetType);
            }
            else
            {
                throw new NotImplementedException("Typeinf.Occur");
            }
        }

        static void Unification(Type type1, Type type2)
        {
            if (type1 is IntType && type2 is IntType)
            {
                // 何もしない
            }
            else if (type1 is FunType && type2 is FunType)
            {
                var t1 = (FunType)type1;
                var t2 = (FunType)type2;
                Unification(t1.ArgType, t2.ArgType);
                Unification(t1.RetType, t2.RetType);
            }
            else if (type1 is TypeVar && type2 is TypeVar)
            {
                var t1 = (TypeVar)type1;
                var t2 = (TypeVar)type2;
                // 同じ型変数
                if (t1.Id == t2.Id)
                {
                    // 何もしない
                }
                // 片方が代入済み
                else if (t1.Var != null)
                {
                    Unification(t1.Var, t2);
                }
                else if (t2.Var != null)
                {
                    Unification(t1, t2.Var);
                }
                // 片方が未代入
                else if (t1.Var == null)
                {
                    if (OccursCheck(t1.Id, t2))
                    {
                        throw new Error("型エラー (出現違反");
                    }
                    else
                    {
                        t1.Var = t2;
                    }
                }
                else if (t2.Var == null)
                {
                    if (OccursCheck(t2.Id, t1))
                    {
                        throw new Error("型エラー (出現違反)");
                    }
                    else
                    {
                        t2.Var = t1;
                    }
                }
            }
            else
            {
                throw new Error("型エラー (単一化エラー)");
            }
        }
    }
}
