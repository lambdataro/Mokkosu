using System;
using System.Collections.Generic;
using System.Linq;

namespace Mokkosu
{
    using TEnv = Env<TypeScheme>;

    /// <summary>
    /// 型推論器
    /// </summary>
    static class Typeinf
    {
        public static void Start(SExpr expr)
        {
        }

        /// <summary>
        /// 出現検査
        /// </summary>
        /// <param name="id">型変数ID</param>
        /// <param name="type">型</param>
        /// <returns>型に型変数IDが含まれていれば真そうでなければ偽</returns>
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

        /// <summary>
        /// 単一化
        /// </summary>
        /// <param name="type1">型1</param>
        /// <param name="type2">型2</param>
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

        /// <summary>
        /// 型中に自由に出現する型変数の集合を返す
        /// </summary>
        /// <param name="type">型</param>
        /// <returns>自由に出現する型変数の集合</returns>
        static ImmutableHashSet<int> FreeVars(Type type)
        {
            if (type is TypeVar)
            {
                var t = (TypeVar)type;
                if (t.Var == null)
                {
                    return new ImmutableHashSet<int>(t.Id);
                }
                else
                {
                    return FreeVars(t.Var);
                }
            }
            else if (type is IntType)
            {
                return new ImmutableHashSet<int>();
            }
            else if (type is FunType)
            {
                var t = (FunType)type;
                var set1 = FreeVars(t.ArgType);
                var set2 = FreeVars(t.RetType);
                return ImmutableHashSet<int>.Union(set1, set2);
            }
            else
            {
                throw new NotImplementedException("Typeinf.FreeVars");
            }
        }

        /// <summary>
        /// 型スキーム中で自由に出現する型変数の集合を返す
        /// </summary>
        /// <param name="typescheme">型スキーム</param>
        /// <returns>自由に出現する型変数の集合</returns>
        static ImmutableHashSet<int> FreeVars(TypeScheme typescheme)
        {
            var set = FreeVars(typescheme.Type);
            return ImmutableHashSet<int>.Diff(set, typescheme.Bounded);
        }

        /// <summary>
        /// 型環境中で自由に出現する型変数の集合を返す
        /// </summary>
        /// <param name="tenv">型環境</param>
        /// <returns>自由に出現する型変数の集合</returns>
        static ImmutableHashSet<int> FreeVars(TEnv tenv)
        {
            if (TEnv.IsEmpty(tenv))
            {
                return new ImmutableHashSet<int>();
            }
            else
            {
                var set1 = FreeVars(tenv.Value);
                var set2 = FreeVars(tenv.Tail);
                return ImmutableHashSet<int>.Union(set1, set2);
            }
        }

        /// <summary>
        /// 型に量化子(∀)を付ける
        /// </summary>
        /// <param name="tenv">型環境</param>
        /// <param name="type">型</param>
        /// <returns>型スキーム</returns>
        static TypeScheme Generalize(TEnv tenv, Type type)
        {
            var tenv_fvs = FreeVars(tenv);
            var fvs = FreeVars(type);
            var bounded = ImmutableHashSet<int>.Diff(fvs, tenv_fvs);
            return new TypeScheme(bounded.ToArray(), type);
        }

        /// <summary>
        /// 型スキームからインスタンスを作成
        /// </summary>
        /// <param name="type_scheme">型スキーム</param>
        /// <returns>新しい型</returns>
        static Type Instantiate(TypeScheme type_scheme)
        {
            var map = new Dictionary<int, Type>();
            foreach (var id in type_scheme.Bounded.ToArray())
            {
                map.Add(id, new TypeVar());
            }
            return MapTypeVar(map, type_scheme.Type);
        }

        /// <summary>
        /// 写像にしたがって型変数を新しいものに置き換える
        /// </summary>
        /// <param name="map">型変数IDから型への写像</param>
        /// <param name="type">型</param>
        /// <returns>新しい型</returns>
        static Type MapTypeVar(Dictionary<int, Type> map, Type type)
        {
            if (type is TypeVar)
            {
                var t = (TypeVar)type;
                if (t.Var == null)
                {
                    if (map.ContainsKey(t.Id))
                    {
                        return map[t.Id];
                    }
                    else
                    {
                        return type;
                    }
                }
                else
                {
                    return MapTypeVar(map, t.Var);
                }
            }
            else if (type is IntType)
            {
                return type;
            }
            else if (type is FunType)
            {
                var t = (FunType)type;
                var arg_t = MapTypeVar(map, t.ArgType);
                var ret_t = MapTypeVar(map, t.RetType);
                return new FunType(arg_t, ret_t);
            }
            else
            {
                throw new NotImplementedException("Typeinf.MapTypeVar");
            }
        }

        /// <summary>
        /// 型推論 (Algorithm M)
        /// </summary>
        /// <param name="expr">型を推論する式</param>
        /// <param name="type">文脈の型</param>
        /// <param name="tenv">型環境</param>
        static void Inference(SExpr expr, Type type, TEnv tenv)
        {
            if (expr is SConstInt)
            {
                Unification(type, IntType.Type);
            }
            else if (expr is SBinop)
            {
                var e = (SBinop)expr;
                if (e is SAdd || e is SSub || e is SMul || e is SDiv)
                {
                    Inference(e.Lhs, IntType.Type, tenv);
                    Inference(e.Rhs, IntType.Type, tenv);
                    Unification(type, IntType.Type);
                }
                else if (e is SEq)
                {
                    Inference(e.Lhs, IntType.Type, tenv);
                    Inference(e.Rhs, IntType.Type, tenv);
                    Unification(type, IntType.Type);
                }
                else
                {
                    throw new NotImplementedException("Inference.Inference");
                }
            }
            else if (expr is SVar)
            {
                var e = (SVar)expr;
                TypeScheme typescheme;
                if (TEnv.Lookup(tenv, e.Name, out typescheme))
                {
                    var t = Instantiate(typescheme);
                    Unification(e.VarType, t);
                    Unification(type, t);
                }
                else
                {
                    throw new Error(string.Format("変数{0}は未定義です", e.Name));
                }
            }
            else if (expr is SFun)
            {
                var e = (SFun)expr;
                var tenv2 = TEnv.Cons(e.ArgName, new TypeScheme(e.ArgType), tenv);
                var ret_type = new TypeVar();
                Inference(e.Body, ret_type, tenv2);
                var fun_type = new FunType(e.ArgType, ret_type);
                Unification(type, fun_type);
            }
            else if (expr is SApp)
            {
                var e = (SApp)expr;
                var arg_type = new TypeVar();
                var fun_type = new FunType(arg_type, type);
                Inference(e.FunExpr, fun_type, tenv);
                Inference(e.ArgExpr, arg_type, tenv);
            }
            else if (expr is SLet)
            {
                var e = (SLet)expr;
                Inference(e.E1, e.VarType, tenv);
                var tenv2 = TEnv.Cons(e.VarName, new TypeScheme(e.VarType), tenv);
                Inference(e.E2, type, tenv2);
            }
            else if (expr is SIf)
            {
                var e = (SIf)expr;
                Inference(e.CondExpr, IntType.Type, tenv);
                Inference(e.ThenExpr, type, tenv);
                Inference(e.ElseExpr, type, tenv);
            }
            else if (expr is SPrint)
            {
                var e = (SPrint)expr;
                Inference(e.Body, IntType.Type, tenv);
                Unification(type, IntType.Type);
            }
            else
            {
                throw new NotImplementedException("Inference.Inference");
            }
        }
    }
}
