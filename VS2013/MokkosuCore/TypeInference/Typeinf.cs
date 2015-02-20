using Mokkosu.AST;
using Mokkosu.Utils;
using Mokkosu.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mokkosu.TypeInference
{
    using TEnv = MEnv<MTypeScheme>;

    static class Typeinf
    {
        /// <summary>
        /// 型推論を開始する
        /// </summary>
        /// <param name="parse_result">構文解析の結果</param>
        public static void Start(ParseResult parse_result)
        {
            var ctx = new TypeInfContext();
            ctx.TEnv = InitialEnv();
            parse_result.TopExprs.ForEach(e => TypeinfTopExpr(e, ctx));
        }

        static TEnv InitialEnv()
        {
            var dict = new Dictionary<string, MTypeScheme>()
            {
            };

            var tenv = new TEnv();
            foreach (var kv in dict)
            {
                tenv = tenv.Cons(kv.Key, kv.Value);
            }
            return tenv; 
        }

        /// <summary>
        /// トップレベル式の型検査＆型推論
        /// </summary>
        /// <param name="top_expr">トップレベル式</param>
        /// <param name="ctx">型推論文脈</param>
        static void TypeinfTopExpr(MTopExpr top_expr, TypeInfContext ctx)
        {
            if (top_expr is MUserTypeDef)
            {
                TypeinfUserTypeDef((MUserTypeDef)top_expr, ctx);
                // System.Console.WriteLine(ctx);
            }
            else if (top_expr is MTopDo)
            {
                TypeinfTopDo((MTopDo)top_expr, ctx);
                // System.Console.WriteLine(top_expr);
            }
            else if (top_expr is MTopLet)
            {
                var t = (MTopLet)top_expr;
                var tenv = TypeinfTopLet(t, ctx);
                if (!t.HideType)
                {
                    ShowType.ShowTEnv(tenv);
                }
                // System.Console.WriteLine(top_expr);
            }
            else if (top_expr is MTopFun)
            {
                var t = (MTopFun)top_expr;
                var tenv = TypeinfTopFun(t, ctx);
                if (!t.HideType)
                {
                    ShowType.ShowTEnv(tenv);
                }
                // System.Console.WriteLine(top_expr);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// ユーザ型定義の型検査
        /// </summary>
        /// <param name="typedef">ユーザ型定義</param>
        /// <param name="ctx">型推論文脈</param>
        static void TypeinfUserTypeDef(MUserTypeDef typedef, TypeInfContext ctx)
        {
            var user_types = new MEnv<int>();
            foreach (var def in typedef.Items)
            {
                var name = def.Name;
                var kind = def.TypeParams.Count;
                user_types = user_types.Cons(name, kind);
            }
            ctx.UserTypes = ctx.UserTypes.Append(user_types);

            var tag_env = new MEnv<Tag>();
            foreach (var def in typedef.Items)
            {
                tag_env = tag_env.Append(TypeinfTypeDefItem(def.Tags, def.Name, def.TypeParams, ctx));
            }
            ctx.TEnv = TagEnvToTEnv(tag_env).Append(ctx.TEnv);
        }

        /// <summary>
        /// タグ環境を通常の型環境に変換
        /// </summary>
        /// <param name="tag_env">タグ環境</param>
        /// <returns>型環境</returns>
        static TEnv TagEnvToTEnv(MEnv<Tag> tag_env)
        {
            var tenv = new TEnv();

            while (!tag_env.IsEmpty())
            {
                var name = tag_env.Head.Item1;
                var tag = tag_env.Head.Item2;

                MTypeScheme ts;
                if (tag.ArgTypes.Count == 0)
                {
                    ts = new MTypeScheme(tag.Bounded.ToArray(), tag.Type);
                   
                }
                else if (tag.ArgTypes.Count == 1)
                {
                    ts = new MTypeScheme(tag.Bounded.ToArray(), new FunType(tag.ArgTypes[0], tag.Type));
                }
                else
                {
                    ts = new MTypeScheme(tag.Bounded.ToArray(), 
                        new FunType(new TupleType(tag.ArgTypes), tag.Type));
                }
                ts.IsTag = true;
                ts.TagIndex = tag.Index;
                ts.TagSize = tag.ArgTypes.Count;
                tenv = tenv.Cons(name, ts);
                tag_env = tag_env.Tail;
            }

            return tenv;
        }

        /// <summary>
        /// ユーザ定義型のアイテムごとの型検査
        /// </summary>
        /// <param name="tags">タグの列</param>
        /// <param name="type_name">型名</param>
        /// <param name="type_params">型パラメータ</param>
        /// <param name="ctx">型推論文脈</param>
        /// <returns>タグ環境</returns>
        static MEnv<Tag> TypeinfTypeDefItem(List<TagDef> tags, string type_name, 
            List<string> type_params, TypeInfContext ctx)
        {
            var tag_env = new MEnv<Tag>();

            for (var i = 0; i < tags.Count; i++)
            {
                var name = tags[i].Name;
                var index = i;

                var dict = new Dictionary<string, TypeVar>();
                var bounded = new MSet<int>();
                var type_args = new List<MType>();
                foreach (var p in type_params)
                {
                    var tv = new TypeVar();
                    dict.Add(p, tv);
                    bounded = bounded.Union(new MSet<int>(tv.Id));
                    type_args.Add(tv);
                }

                var arg_types = tags[i].Args.Select(typ => MapTypeParam(typ, dict, ctx));
                var type = new UserType(type_name, type_args);

                var tag = new Tag(name, index, bounded, arg_types.ToList(), type);
                tag_env = tag_env.Cons(name, tag);
            }

            return tag_env;
        }

        /// <summary>
        /// 型中の型パラメータを表すUserTypeを型変数に置換する
        /// </summary>
        /// <param name="type">型</param>
        /// <param name="dict">型パラメータと型変数の対応</param>
        /// <returns>型</returns>
        static MType MapTypeParam(MType type, Dictionary<string, TypeVar> dict, TypeInfContext ctx)
        {
            if (type is TypeVar)
            {
                var t = (TypeVar)type;
                if (t.Value == null)
                {
                    return type;
                }
                else
                {
                    return MapTypeParam(t.Value, dict, ctx);
                }
            }
            else if (type is UserType)
            {
                var t = (UserType)type;

                if (!ctx.UserTypes.Contains(t.Name))
                {
                    throw new MError("型" + t.Name + "は未定義です。");
                }

                if (t.Args.Count == 0 && dict.ContainsKey(t.Name))
                {
                    return dict[t.Name];
                }
                else
                {
                    var args = new List<MType>();
                    foreach (var arg in t.Args)
                    {
                        args.Add(MapTypeParam(arg, dict, ctx));
                    }
                    return new UserType(t.Name, args);
                }
            }
            else if (type is IntType || type is DoubleType || type is StringType ||
                type is CharType || type is UnitType || type is BoolType)
            {
                return type;
            }
            else if (type is FunType)
            {
                var t = (FunType)type;
                var arg = MapTypeParam(t.ArgType, dict, ctx);
                var ret = MapTypeParam(t.RetType, dict, ctx);
                return new FunType(arg, ret);
            }
            else if (type is ListType)
            {
                var t = (ListType)type;
                var elem_type = MapTypeParam(t.ElemType, dict, ctx);
                return new ListType(elem_type);
            }
            else if (type is RefType)
            {
                var t = (RefType)type;
                var elem_type = MapTypeParam(t.ElemType, dict, ctx);
                return new RefType(elem_type);
            }
            else if (type is TupleType)
            {
                var t = (TupleType)type;
                var types = t.Types.Select(typ => MapTypeParam(typ, dict, ctx)).ToList();
                return new TupleType(types);
            }
            else if (type is DotNetType)
            {
                return type;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// トップレベルdoの型推論
        /// </summary>
        /// <param name="top_do">トップレベルdo</param>
        /// <param name="ctx">型推論文脈</param>
        static void TypeinfTopDo(MTopDo top_do, TypeInfContext ctx)
        {
            Inference(top_do.Expr, top_do.Type, ctx.TEnv, ctx);
        }

        /// <summary>
        /// トップレベルletの型推論
        /// </summary>
        /// <param name="top_let">トップレベルlet</param>
        /// <param name="ctx">型推論文脈</param>
        static TEnv TypeinfTopLet(MTopLet top_let, TypeInfContext ctx)
        {
            var tenv1 = InferencePat(top_let.Pat, top_let.Type, ctx.TEnv, ctx);
            Inference(top_let.Expr, top_let.Type, ctx.TEnv, ctx);
            
            if (IsSyntacticValue(top_let.Expr))
            {
                var tenv2 = GeneralizeTypes(ctx.TEnv, tenv1);
                ctx.TEnv = tenv2.Append(ctx.TEnv);
                return tenv2;
            }
            else
            {
                ctx.TEnv = tenv1.Append(ctx.TEnv);
                return tenv1;
            }
        }

        /// <summary>
        /// トップレベルfunの型推論
        /// </summary>
        /// <param name="top_fun">トップレベルfun</param>
        /// <param name="ctx">型推論文脈</param>
        static TEnv TypeinfTopFun(MTopFun top_fun, TypeInfContext ctx)
        {
            var tenv = ctx.TEnv;

            foreach (var item in top_fun.Items)
            {
                tenv = tenv.Cons(item.Name, new MTypeScheme(item.Type));
            }

            foreach (var item in top_fun.Items)
            {
                Inference(item.Expr, item.Type, tenv, ctx);
            }

            var tenv2 = new TEnv();

            foreach (var item in top_fun.Items)
            {
                if (IsSyntacticValue(item.Expr))
                {
                    var ts = Generalize(ctx.TEnv, item.Type);
                    tenv2 = tenv2.Cons(item.Name, ts);
                }
                else
                {
                    tenv2 = tenv2.Cons(item.Name, new MTypeScheme(item.Type));
                }
            }

            ctx.TEnv = tenv2.Append(ctx.TEnv);
            return tenv2;
        }

        /// <summary>
        /// 構文的に値であるかの判定
        /// </summary>
        /// <param name="expr">式</param>
        /// <returns>構文的に値であれば真</returns>
        static bool IsSyntacticValue(MExpr expr)
        {
            if (expr is MInt || expr is MDouble || expr is MString || expr is MChar ||
                expr is MUnit || expr is MBool)
            {
                return true;
            }
            else if (expr is MVar)
            {
                return true;
            }
            else if (expr is MLambda)
            {
                return true;
            }
            else if (expr is MNil)
            {
                return true;
            }
            else if (expr is MApp)
            {
                var app = (MApp)expr;
                if (app.FunExpr is MVar && ((MVar)app.FunExpr).IsTag)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (expr is MCons)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 型推論 (Algorithm M)
        /// </summary>
        /// <param name="expr">型を推論する式</param>
        /// <param name="type">文脈の型</param>
        /// <param name="ctx">型推論文脈</param>
        static void Inference(MExpr expr, MType type, TEnv tenv, TypeInfContext ctx)
        {
            if (expr is MInt)
            {
                Unification(expr.Pos, type, new IntType());
            }
            else if (expr is MDouble)
            {
                Unification(expr.Pos, type, new DoubleType());
            }
            else if (expr is MString)
            {
                Unification(expr.Pos, type, new StringType());
            }
            else if (expr is MChar)
            {
                Unification(expr.Pos, type, new CharType());
            }
            else if (expr is MUnit)
            {
                Unification(expr.Pos, type, new UnitType());
            }
            else if (expr is MBool)
            {
                Unification(expr.Pos, type, new BoolType());
            }
            else if (expr is MVar)
            {
                var e = (MVar)expr;
                MTypeScheme typescheme;
                if (tenv.Lookup(e.Name, out typescheme))
                {
                    var t = Instantiate(typescheme);
                    Unification(expr.Pos, e.Type, t);
                    Unification(expr.Pos, type, t);
                    if (typescheme.IsTag)
                    {
                        e.IsTag = true;
                        e.TagIndex = typescheme.TagIndex;
                        e.TagSize = typescheme.TagSize;
                    }
                }
                else
                {
                    throw new MError(string.Format("{0}: 変数{1}は未定義です", e.Pos, e.Name));
                }
            }
            else if (expr is MLambda)
            {
                var e = (MLambda)expr;
                var tenv2 = InferencePat(e.ArgPat, e.ArgType, tenv, ctx);
                var tenv3 = tenv2.Append(tenv);
                var ret_type = new TypeVar();
                Inference(e.Body, ret_type, tenv3, ctx);
                var fun_type = new FunType(e.ArgType, ret_type);
                Unification(expr.Pos, type, fun_type);
            }
            else if (expr is MApp)
            {
                var e = (MApp)expr;
                var arg_type = new TypeVar();
                var fun_type = new FunType(arg_type, type);
                Inference(e.FunExpr, fun_type, tenv, ctx);
                Inference(e.ArgExpr, arg_type, tenv, ctx);
            }
            else if (expr is MIf)
            {
                var e = (MIf)expr;
                Inference(e.CondExpr, new BoolType(), tenv, ctx);
                Inference(e.ThenExpr, type, tenv, ctx);
                Inference(e.ElseExpr, type, tenv, ctx);
            }
            else if (expr is MMatch)
            {
                var e = (MMatch)expr;
                var t = new TypeVar();
                var tenv2 = InferencePat(e.Pat, t, tenv, ctx);
                Inference(e.Expr, t, tenv, ctx);
                var tenv3 = tenv2.Append(tenv);
                Inference(e.Guard, new BoolType(), tenv3, ctx);
                Inference(e.ThenExpr, type, tenv3, ctx);
                Inference(e.ElseExpr, type, tenv, ctx);
            }
            else if (expr is MNil)
            {
                var e = (MNil)expr;
                Unification(expr.Pos, type, new ListType(e.Type));
            }
            else if (expr is MCons)
            {
                var e = (MCons)expr;
                var list_type = new ListType(e.ItemType);
                Inference(e.Head, e.ItemType, tenv, ctx);
                Inference(e.Tail, list_type, tenv, ctx);
                Unification(expr.Pos, type, list_type);
            }
            else if (expr is MTuple)
            {
                var e = (MTuple)expr;
                for (var i = 0; i < e.Size; i++)
                {
                    Inference(e.Items[i], e.Types[i], tenv, ctx);
                }
                Unification(expr.Pos, type, new TupleType(e.Types));
            }
            else if (expr is MDo)
            {
                var e = (MDo)expr;
                Inference(e.E1, e.E1Type, tenv, ctx);
                Inference(e.E2, e.E2Type, tenv, ctx);
                Unification(expr.Pos, type, e.E2Type);
            }
            else if (expr is MLet)
            {
                var e = (MLet)expr;
                var tenv1 = InferencePat(e.Pat, e.E1Type, tenv, ctx);
                Inference(e.E1, e.E1Type, tenv, ctx);

                if (IsSyntacticValue(e.E1))
                {
                    var tenv2 = GeneralizeTypes(tenv, tenv1);
                    Inference(e.E2, e.E2Type, tenv2.Append(tenv), ctx);
                }
                else
                {
                    Inference(e.E2, e.E2Type, tenv1.Append(tenv), ctx);
                }
                Unification(expr.Pos, type, e.E2Type);
            }
            else if (expr is MFun)
            {
                var e = (MFun)expr;
                var tenv1 = tenv;

                foreach (var item in e.Items)
                {
                    tenv1 = tenv1.Cons(item.Name, new MTypeScheme(item.Type));
                }

                foreach (var item in e.Items)
                {
                    Inference(item.Expr, item.Type, tenv1, ctx);
                }

                var tenv2 = tenv;

                foreach (var item in e.Items)
                {
                    if (IsSyntacticValue(item.Expr))
                    {
                        var ts = Generalize(tenv, item.Type);
                        tenv2 = tenv2.Cons(item.Name, ts);
                    }
                    else
                    {
                        tenv2 = tenv2.Cons(item.Name, new MTypeScheme(item.Type));
                    }
                }

                Inference(e.E2, type, tenv2, ctx);
            }
            else if (expr is MFource)
            {
                var e = (MFource)expr;
                Inference(e.Expr, e.Type, tenv, ctx);
                Unification(expr.Pos, type, e.Type);
            }
            else if (expr is MRuntimeError)
            {
                // 何もしない
            }
            else if (expr is MPrim)
            {
                var e = (MPrim)expr;
                for (var i = 0; i < e.Args.Count; i++)
                {
                    Inference(e.Args[i], e.ArgTypes[i], tenv, ctx);
                }
                Unification(expr.Pos, type, e.RetType);
                InferencePrim(e.Pos, e.Name, e.ArgTypes, e.RetType);
            }
            else if (expr is MCallStatic)
            {
                var e = (MCallStatic)expr;
                for (var i = 0; i < e.Args.Count; i++)
                {
                    Inference(e.Args[i], e.Types[i], tenv, ctx);
                }
                var method = TypeinfDotNet.LookupStaticMethod(e.Pos, e.ClassName, e.MethodName, e.Types);
                e.Info = method;
                if (method.ReturnType == typeof(void))
                {
                    Unification(e.Pos, type, new UnitType());
                }
                else
                {
                    var ret_type = TypeinfDotNet.DotNetTypeToMokkosuType(method.ReturnType);
                    Unification(e.Pos, type, ret_type);
                }
            }
            else if (expr is MCast)
            {
                var e = (MCast)expr;
                var src_type = TypeinfDotNet.LookupDotNetClass(e.Pos, e.SrcTypeName);
                var dst_type = TypeinfDotNet.LookupDotNetClass(e.Pos, e.DstTypeName);
                e.SrcType = src_type;
                e.DstType = dst_type;
                Inference(e.Expr, TypeinfDotNet.DotNetTypeToMokkosuType(src_type), tenv, ctx);
                Unification(e.Pos, type, TypeinfDotNet.DotNetTypeToMokkosuType(dst_type));
            }
            else if (expr is MNewClass)
            {
                var e = (MNewClass)expr;
                for (var i = 0; i < e.Args.Count; i++)
                {
                    Inference(e.Args[i], e.Types[i], tenv, ctx);
                }
                var constructor = TypeinfDotNet.LookupConstructor(e.Pos, e.ClassName, e.Types);
                e.Info = constructor;

                var t = TypeinfDotNet.DotNetTypeToMokkosuType(constructor.DeclaringType);
                Unification(e.Pos, type, t);
            }
            else if (expr is MInvoke)
            {
                var e = (MInvoke)expr;

                Inference(e.Expr, e.ExprType, tenv, ctx);
                for (var i = 0; i < e.Args.Count; i++)
                {
                    Inference(e.Args[i], e.Types[i], tenv, ctx);
                }

                var method = TypeinfDotNet.LookupInstanceMethod(e.Pos, e.ExprType, e.MethodName, e.Types);
                e.Info = method;
                if (method.ReturnType == typeof(void))
                {
                    Unification(e.Pos, type, new UnitType());
                }
                else
                {
                    var ret_type = TypeinfDotNet.DotNetTypeToMokkosuType(method.ReturnType);
                    Unification(e.Pos, type, ret_type);
                }
            }
            else if (expr is MDelegate)
            {
                var e = (MDelegate)expr;
                var t = TypeinfDotNet.LookupDotNetClass(e.Pos, e.ClassName);
                var invoke = t.GetMethod("Invoke");
                var param_info = invoke.GetParameters();
                var param_type = param_info.Select(info => info.ParameterType).ToArray();
                var param_mtype = param_type.Select(typ => 
                    TypeinfDotNet.DotNetTypeToMokkosuType(typ)).ToList();
                Inference(e.Expr, e.ExprType, tenv, ctx);
                Unification(e.Pos, e.ExprType, new FunType(new TupleType(param_mtype), new UnitType()));
                Unification(e.Pos, type, new DotNetType(t));
                e.ParamType = param_type;
                e.ClassType = t;
                e.CstrInfo = t.GetConstructor(new Type[] { typeof(object), typeof(IntPtr) });
            }
            else if (expr is MSet)
            {
                var e = (MSet)expr;
                Inference(e.Expr, e.ExprType, tenv, ctx);
                Inference(e.Arg, e.ArgType, tenv, ctx);
                var field = TypeinfDotNet.LookupField(e.Pos, e.ExprType, e.FieldName);
                e.Info = field;
                var ret_type = TypeinfDotNet.DotNetTypeToMokkosuType(field.FieldType);
                Unification(e.Pos, e.ArgType, ret_type);
                Unification(e.Pos, type, ret_type);
            }
            else if (expr is MGet)
            {
                var e = (MGet)expr;
                Inference(e.Expr, e.ExprType, tenv, ctx);
                var field = TypeinfDotNet.LookupField(e.Pos, e.ExprType, e.FieldName);
                e.Info = field;
                var ret_type = TypeinfDotNet.DotNetTypeToMokkosuType(field.FieldType);
                Unification(e.Pos, type, ret_type);
            }
            else if (expr is MSSet)
            {
                var e = (MSSet)expr;
                Inference(e.Arg, e.ArgType, tenv, ctx);
                var t = TypeinfDotNet.LookupDotNetClass(e.Pos, e.ClassName);
                var field = TypeinfDotNet.LookupField(e.Pos, new DotNetType(t), e.FieldName);
                e.Info = field;
                var ret_type = TypeinfDotNet.DotNetTypeToMokkosuType(field.FieldType);
                Unification(e.Pos, e.ArgType, ret_type);
                Unification(e.Pos, type, ret_type);
            }
            else if (expr is MSGet)
            {
                var e = (MSGet)expr;
                var t = TypeinfDotNet.LookupDotNetClass(e.Pos, e.ClassName);
                var field = TypeinfDotNet.LookupField(e.Pos, new DotNetType(t), e.FieldName);
                e.Info = field;
                var ret_type = TypeinfDotNet.DotNetTypeToMokkosuType(field.FieldType);
                Unification(e.Pos, type, ret_type);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static MType ReduceType(MType type)
        {
            if (type is TypeVar)
            {
                var t = (TypeVar)type;
                if (t.Value == null)
                {
                    return t;
                }
                else
                {
                    return ReduceType(t.Value);
                }
            }
            else if (type is UserType)
            {
                var t = (UserType)type;
                var args = t.Args.Select(typ => ReduceType(typ)).ToList();
                return new UserType(t.Name, args);
            }
            else if (type is IntType || type is DoubleType || type is StringType ||
                type is CharType || type is UnitType || type is BoolType)
            {
                return type;
            }
            else if (type is FunType)
            {
                var t = (FunType)type;
                var arg = ReduceType(t.ArgType);
                var ret = ReduceType(t.RetType);
                return new FunType(arg, ret);
            }
            else if (type is ListType)
            {
                var t = (ListType)type;
                var elem = ReduceType(t.ElemType);
                return new ListType(elem);
            }
            else if (type is RefType)
            {
                var t = (RefType)type;
                var elem = ReduceType(t.ElemType);
                return new RefType(elem);
            }
            else if (type is TupleType)
            {
                var t = (TupleType)type;
                var args = t.Types.Select(typ => ReduceType(typ)).ToList();
                return new TupleType(args);
            }
            else if (type is DotNetType)
            {
                return type;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        static void InferencePrim(string pos, string name, List<MType> args, MType ret)
        {
            args = args.Select(t => ReduceType(t)).ToList();
            ret = ReduceType(ret);

            switch (name)
            {
                case "add":
                case "sub":
                case "mul":
                case "div":
                case "mod":
                case "band":
                case "bor":
                case "bxor":
                case "bshr":
                case "bshl":
                case "bshrun":
                    if (args.Count == 2)
                    {
                        Unification(pos, args[0], new IntType());
                        Unification(pos, args[1], new IntType());
                        Unification(pos, ret, new IntType());
                    }
                    else
                    {
                        throw new MError(pos + ": プリミティブ演算の引数の数が不正。");
                    }
                    break;

                case "bnot":
                    if (args.Count == 1)
                    {
                        Unification(pos, args[0], new IntType());
                        Unification(pos, ret, new IntType());
                    }
                    else
                    {
                        throw new MError(pos + ": プリミティブ演算の引数の数が不正。");
                    }
                    break;

                case "fadd":
                case "fsub":
                case "fmul":
                case "fdiv":
                    if (args.Count == 2)
                    {
                        Unification(pos, args[0], new DoubleType());
                        Unification(pos, args[1], new DoubleType());
                        Unification(pos, ret, new DoubleType());
                    }
                    else
                    {
                        throw new MError(pos + ": プリミティブ演算の引数の数が不正。");
                    }
                    break;

                case "eq":
                case "ne":
                case "lt":
                case "gt":
                case "le":
                case "ge":
                    if (args.Count == 2)
                    {
                        Unification(pos, args[0], args[1]);
                        Unification(pos, ret, new BoolType());
                    }
                    else
                    {
                        throw new MError(pos + ": プリミティブ演算の引数の数が不正。");
                    }
                    break;

                case "concat":
                    if (args.Count == 2)
                    {
                        Unification(pos, args[0], new StringType());
                        Unification(pos, args[1], new StringType());
                        Unification(pos, ret, new StringType());
                    }
                    else
                    {
                        throw new MError(pos + ": プリミティブ演算の引数の数が不正。");
                    }
                    break;

                case "print":
                    if (args.Count == 1)
                    {
                        Unification(pos, ret, new UnitType());
                    }
                    else
                    {
                        throw new MError(pos + ": プリミティブ演算の引数の数が不正。");
                    }
                    break;

                case "println":
                    if (args.Count == 1)
                    {
                        Unification(pos, ret, new UnitType());
                    }
                    else
                    {
                        throw new MError(pos + ": プリミティブ演算の引数の数が不正。");
                    }
                    break;

                case "tostring":
                    if (args.Count == 1)
                    {
                        Unification(pos, ret, new StringType());
                    }
                    else
                    {
                        throw new MError(pos + ": プリミティブ演算の引数の数が不正。");
                    }
                    break;

                case "error":
                    if (args.Count == 1)
                    {
                        Unification(pos, args[0], new StringType());
                    }
                    else
                    {
                        throw new MError(pos + ": プリミティブ演算の引数の数が不正。");
                    }
                    break;

                case "ref":
                    if (args.Count == 1)
                    {
                        Unification(pos, ret, new RefType(args[0]));
                    }
                    else
                    {
                        throw new MError(pos + ": プリミティブ演算の引数の数が不正。");
                    }
                    break;

                case "deref":
                    if (args.Count == 1)
                    {
                        var t = new TypeVar();
                        Unification(pos, new RefType(t), args[0]);
                        Unification(pos, ret, t);
                    }
                    else
                    {
                        throw new MError(pos + ": プリミティブ演算の引数の数が不正。");
                    }
                    break;

                case "assign":
                    if (args.Count == 2)
                    {
                        Unification(pos, args[0], new RefType(args[1]));
                        Unification(pos, ret, new UnitType());
                    }
                    else
                    {
                        throw new MError(pos + ": プリミティブ演算の引数の数が不正。");
                    }
                    break;

                case "loadnull":
                    if (args.Count == 0)
                    {
                        Unification(pos, ret, new DotNetType(typeof(object)));
                    }
                    else
                    {
                        throw new MError(pos + ": プリミティブ演算の引数の数が不正。");
                    }
                    break;

                case "intequal":
                    if (args.Count == 2)
                    {
                        Unification(pos, args[0], new IntType());
                        Unification(pos, args[1], new IntType());
                        Unification(pos, ret, new BoolType());
                    }
                    else
                    {
                        throw new MError(pos + ": プリミティブ演算の引数の数が不正。");
                    }
                    break;

                default:
                    throw new MError(pos + ": プリミティブ演算型エラー");
            }
        }

        /// <summary>
        /// パターンの型推論
        /// </summary>
        /// <param name="pat">パターン</param>
        /// <param name="type">文脈の型</param>
        /// <param name="tenv">型環境</param>
        /// <param name="ctx">型推論文脈</param>
        /// <returns>新たに追加される型環境</returns>
        static TEnv InferencePat(MPat pat, MType type, TEnv tenv, TypeInfContext ctx)
        {
            if (pat is PWild)
            {
                var p = (PWild)pat;
                Unification(pat.Pos, type, p.Type);
                return new TEnv();
            }
            else if (pat is PVar)
            {
                var p = (PVar)pat;
                Unification(pat.Pos, type, p.Type);

                MTypeScheme typescheme;
                if (tenv.Lookup(p.Name, out typescheme))
                {
                    if (typescheme.IsTag)
                    {
                        p.IsTag = true;
                        p.TagIndex = typescheme.TagIndex;
                        return new TEnv();
                    }
                    else
                    {
                        return new TEnv().Cons(p.Name, new MTypeScheme(type));
                    }
                }
                else
                {
                    return new TEnv().Cons(p.Name, new MTypeScheme(type));
                }
            }
            else if (pat is PFource)
            {
                var p = (PFource)pat;
                Unification(pat.Pos, type, p.Type);
                return InferencePat(p.Pat, p.Type, tenv, ctx);
            }
            else if (pat is PInt)
            {
                Unification(pat.Pos, type, new IntType());
                return new TEnv();
            }
            else if (pat is PDouble)
            {
                Unification(pat.Pos, type, new DoubleType());
                return new TEnv();
            }
            else if (pat is PString)
            {
                Unification(pat.Pos, type, new StringType());
                return new TEnv();
            }
            else if (pat is PChar)
            {
                Unification(pat.Pos, type, new CharType());
                return new TEnv();
            }
            else if (pat is PUnit)
            {
                Unification(pat.Pos, type, new UnitType());
                return new TEnv();
            }
            else if (pat is PBool)
            {
                Unification(pat.Pos, type, new BoolType());
                return new TEnv();
            }
            else if (pat is PNil)
            {
                var p = (PNil)pat;
                Unification(pat.Pos, type, new ListType(p.ItemType));
                return new TEnv();
            }
            else if (pat is PCons)
            {
                var p = (PCons)pat;
                var list_type = new ListType(p.ItemType);
                var tenv1 = InferencePat(p.Head, p.ItemType, tenv, ctx);
                var tenv2 = InferencePat(p.Tail, list_type, tenv, ctx);
                Unification(pat.Pos, type, list_type);
                return tenv1.Append(tenv2);
            }
            else if (pat is PTuple)
            {
                var p = (PTuple)pat;
                var ret_tenv = new TEnv();
                for (var i = 0; i < p.Size; i++)
                {
                    var tenv2 = InferencePat(p.Items[i], p.Types[i], tenv, ctx);
                    ret_tenv = tenv2.Append(ret_tenv);
                }
                Unification(pat.Pos, type, new TupleType(p.Types));
                return ret_tenv;
            }
            else if (pat is PAs)
            {
                var p = (PAs)pat;
                var tenv2 = InferencePat(p.Pat, p.Type, tenv, ctx);
                Unification(pat.Pos, type, p.Type);
                return tenv2.Cons(p.Name, new MTypeScheme(p.Type));
            }
            else if (pat is POr)
            {
                var p = (POr)pat;
                var tenv1 = InferencePat(p.Pat1, p.Type, tenv, ctx);
                var tenv2 = InferencePat(p.Pat2, p.Type, tenv, ctx);
                Unification(pat.Pos, type, p.Type);
                CheckOrPattern(pat.Pos, tenv1, tenv2);
                return tenv1;
            }
            else if (pat is PUserTag)
            {
                var p = (PUserTag)pat;
                MTypeScheme typescheme;
                if (tenv.Lookup(p.Name, out typescheme))
                {
                    if (typescheme.IsTag)
                    {
                        if (typescheme.TagSize == p.Args.Count)
                        {
                            if (p.Args.Count == 0)
                            {
                                p.TagIndex = typescheme.TagIndex;
                                var ret_tenv = new TEnv();
                                Unification(pat.Pos, Instantiate(typescheme), type);
                                return ret_tenv;
                            }
                            else
                            {
                                p.TagIndex = typescheme.TagIndex;
                                var ret_tenv = new TEnv();
                                for (var i = 0; i < p.Args.Count; i++)
                                {
                                    var tenv2 = InferencePat(p.Args[i], p.Types[i], tenv, ctx);
                                    ret_tenv = tenv2.Append(ret_tenv);
                                }
                                var arg_type = new TypeVar();
                                var ret_type = new TypeVar();
                                Unification(pat.Pos, Instantiate(typescheme), new FunType(arg_type, ret_type));
                                if (p.Types.Count == 1)
                                {
                                    Unification(pat.Pos, arg_type, p.Types[0]);
                                }
                                else
                                {
                                    Unification(pat.Pos, arg_type, new TupleType(p.Types));
                                }
                                Unification(pat.Pos, type, ret_type);
                                return ret_tenv;
                            }
                        }
                        else
                        {
                            throw new MError(p.Pos + ": タグの引数が不正です。");
                        }
                    }
                    else
                    {
                        throw new MError(p.Pos + ": タグ" + p.Name + "は定義されていません。");
                    }
                }
                else
                {
                    throw new MError(p.Pos + ": タグ" + p.Name + "は定義されていません。");
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Orパターンの整合性検査
        /// </summary>
        /// <param name="tenv1">型環境1</param>
        /// <param name="tenv2">型環境2</param>
        static void CheckOrPattern(string pat, TEnv tenv1, TEnv tenv2)
        {
            if (tenv1.Lenght() != tenv2.Lenght())
            {
                throw new MError(pat + ": Orパターンの変数集合が左右で異なっています。");
            }

            while (!tenv1.IsEmpty())
            {
                var name = tenv1.Head.Item1;
                var ts1 = tenv1.Head.Item2;
                MTypeScheme ts2;
                if (tenv2.Lookup(name, out ts2))
                {
                    Unification(pat, ts1.Type, ts2.Type);
                }
                else
                {
                    throw new MError(pat + ": Orパターンの変数集合が左右で異なっています。");
                }
                tenv1 = tenv1.Tail;
            }
        }

        /// <summary>
        /// 出現検査
        /// </summary>
        /// <param name="id">型変数ID</param>
        /// <param name="type">型</param>
        /// <returns>型に型変数IDが含まれていれば真そうでなければ偽</returns>
        static bool OccursCheck(int id, MType type)
        {
            if (type is TypeVar)
            {
                var t = (TypeVar)type;
                if (t.Id == id)
                {
                    return true;
                }
                else if (t.Value == null)
                {
                    return false;
                }
                else
                {
                    return OccursCheck(id, t.Value);
                }
            }
            else if (type is FunType)
            {
                var t = (FunType)type;
                return OccursCheck(id, t.ArgType) || OccursCheck(id, t.RetType);
            }
            else if (type is ListType)
            {
                var t = (ListType)type;
                return OccursCheck(id, t.ElemType);
            }
            else if (type is RefType)
            {
                var t = (RefType)type;
                return OccursCheck(id, t.ElemType);
            }
            else if (type is TupleType)
            {
                var t = (TupleType)type;
                return t.Types.Exists(typ => OccursCheck(id, typ));
            }
            else if (type is IntType || type is DoubleType || type is StringType ||
                type is CharType || type is UnitType || type is BoolType)
            {
                return false;
            }
            else if (type is UserType)
            {
                var t = (UserType)type;
                bool b = false;
                foreach (var arg in t.Args)
                {
                    if (OccursCheck(id, arg))
                    {
                        b = true;
                        break;
                    }
                }
                return b;
            }
            else if (type is DotNetType)
            {
                return false;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// 単一化
        /// </summary>
        /// <param name="type1">型1</param>
        /// <param name="type2">型2</param>
        static void Unification(string pos, MType type1, MType type2)
        {
            if (type1 is TypeVar && type2 is TypeVar && ((TypeVar)type1).Id == ((TypeVar)type2).Id)
            {
                return;
            }
            else if (type1 is TypeVar && ((TypeVar)type1).Value != null)
            {
                Unification(pos, ((TypeVar)type1).Value, type2);
            }
            else if (type2 is TypeVar && ((TypeVar)type2).Value != null)
            {
                Unification(pos, ((TypeVar)type2).Value, type1);
            }
            else if (type1 is TypeVar)
            {
                var t1 = (TypeVar)type1;
                if (OccursCheck(t1.Id, type2))
                {
                    ShowType.ShowSingleType(type1, type2);
                    throw new MError(pos + ": 型エラー (型がループしている)");
                }
                else
                {
                    t1.Value = type2;
                }
            }
            else if (type2 is TypeVar)
            {
                var t2 = (TypeVar)type2;
                if (OccursCheck(t2.Id, type1))
                {
                    ShowType.ShowSingleType(type1, type2);
                    throw new MError(pos + ": 型エラー (型がループしている)");
                }
                else
                {
                    t2.Value = type1;
                }
            }
            else if (type1 is UserType && type2 is UserType)
            {
                var t1 = (UserType)type1;
                var t2 = (UserType)type2;
                if (t1.Name == t2.Name && t1.Args.Count == t2.Args.Count)
                {
                    for (int i = 0; i < t1.Args.Count; i++)
                    {
                        Unification(pos, t1.Args[i], t2.Args[i]);
                    }
                }
                else
                {
                    ShowType.ShowSingleType(type1, type2);
                    throw new MError(pos + ": 型エラー (単一化エラー)");
                }
            }
            else if (type1 is FunType && type2 is FunType)
            {
                var t1 = (FunType)type1;
                var t2 = (FunType)type2;
                Unification(pos, t1.ArgType, t2.ArgType);
                Unification(pos, t1.RetType, t2.RetType);
            }
            else if (type1 is ListType && type2 is ListType)
            {
                var t1 = (ListType)type1;
                var t2 = (ListType)type2;
                Unification(pos, t1.ElemType, t2.ElemType);
            }
            else if (type1 is RefType && type2 is RefType)
            {
                var t1 = (RefType)type1;
                var t2 = (RefType)type2;
                Unification(pos, t1.ElemType, t2.ElemType);
            }
            else if (type1 is TupleType && type2 is TupleType)
            {
                var t1 = (TupleType)type1;
                var t2 = (TupleType)type2;
                if (t1.Types.Count == t2.Types.Count)
                {
                    for (var i = 0; i < t1.Types.Count; i++)
                    {
                        Unification(pos, t1.Types[i], t2.Types[i]);
                    }
                }
                else
                {
                    ShowType.ShowSingleType(type1, type2);
                    throw new MError(pos + ": 型エラー (単一化エラー)");
                }
            }
            else if (type1 is IntType && type2 is IntType)
            {
                return;
            }
            else if (type1 is DoubleType && type2 is DoubleType)
            {
                return;
            }
            else if (type1 is StringType && type2 is StringType)
            {
                return;
            }
            else if (type1 is CharType && type2 is CharType)
            {
                return;
            }
            else if (type1 is UnitType && type2 is UnitType)
            {
                return;
            }
            else if (type1 is BoolType && type2 is BoolType)
            {
                return;
            }
            else if (type1 is DotNetType && ((DotNetType)type1).Type.IsEnum)
            {
                Unification(pos, new IntType(), type2);
            }
            else if (type2 is DotNetType && ((DotNetType)type2).Type.IsEnum)
            {
                Unification(pos, new IntType(), type1);
            }
            else if (type1 is DotNetType && type2 is DotNetType)
            {
                var t1 = (DotNetType)type1;
                var t2 = (DotNetType)type2;
                if (t1.Type == t2.Type)
                {
                    return;
                }
                else
                {
                    ShowType.ShowSingleType(type1, type2);
                    throw new MError(pos + ": 型エラー (単一化エラー)");
                }
            }
            else
            {
                ShowType.ShowSingleType(type1, type2);
                throw new MError(pos + ": 型エラー (単一化エラー)");
            }
        }

        /// <summary>
        /// 型中に自由に出現する型変数の集合を返す
        /// </summary>
        /// <param name="type">型</param>
        /// <returns>自由に出現する型変数の集合</returns>
        static MSet<int> FreeTypeVars(MType type)
        {
            if (type is TypeVar)
            {
                var t = (TypeVar)type;
                if (t.Value == null)
                {
                    return new MSet<int>(t.Id);
                }
                else
                {
                    return FreeTypeVars(t.Value);
                }
            }
            else if (type is UserType)
            {
                var t = (UserType)type;
                var set = new MSet<int>();
                foreach (var arg in t.Args)
                {
                    set = set.Union(FreeTypeVars(arg));
                }
                return set;
            }
            else if (type is IntType || type is DoubleType || type is StringType ||
                type is CharType || type is UnitType || type is BoolType)
            {
                return new MSet<int>();
            }
            else if (type is FunType)
            {
                var t = (FunType)type;
                var set1 = FreeTypeVars(t.ArgType);
                var set2 = FreeTypeVars(t.RetType);
                return set1.Union(set2);
            }
            else if (type is ListType)
            {
                var t = (ListType)type;
                return FreeTypeVars(t.ElemType);
            }
            else if (type is RefType)
            {
                var t = (RefType)type;
                return FreeTypeVars(t.ElemType);
            }
            else if (type is TupleType)
            {
                var t = (TupleType)type;
                var set = new MSet<int>();
                foreach (var typ in t.Types)
                {
                    set = set.Union(FreeTypeVars(typ));
                }
                return set;
            }
            else if (type is DotNetType)
            {
                return new MSet<int>();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// 型スキーム中で自由に出現する型変数の集合を返す
        /// </summary>
        /// <param name="typescheme">型スキーム</param>
        /// <returns>自由に出現する型変数の集合</returns>
        static MSet<int> FreeTypeVars(MTypeScheme typescheme)
        {
            var set = FreeTypeVars(typescheme.Type);
            return set.Diff(typescheme.Bounded);
        }

        /// <summary>
        /// 型環境中で自由に出現する型変数の集合を返す
        /// </summary>
        /// <param name="tenv">型環境</param>
        /// <returns>自由に出現する型変数の集合</returns>
        static MSet<int> FreeTypeVars(TEnv tenv)
        {
            if (tenv.IsEmpty())
            {
                return new MSet<int>();
            }
            else
            {
                var set1 = FreeTypeVars(tenv.Head.Item2);
                var set2 = FreeTypeVars(tenv.Tail);
                return set1.Union(set2);
            }
        }

        /// <summary>
        /// 型に量化子(∀)を付ける
        /// </summary>
        /// <param name="tenv"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        static MTypeScheme Generalize(TEnv tenv, MType type)
        {
            var tenv_fvs = FreeTypeVars(tenv);
            var fvs = FreeTypeVars(type);
            var bounded = fvs.Diff(tenv_fvs);
            return new MTypeScheme(bounded.ToArray(), type);
        }

        /// <summary>
        /// 複数の型をまとめてGeneralizeする
        /// </summary>
        /// <param name="tenv">型環境</param>
        /// <param name="types">Generalizeする型の集まり</param>
        /// <returns>Generalize後の型</returns>
        static TEnv GeneralizeTypes(TEnv tenv, TEnv types)
        {
            var ret = new TEnv();
            while (!types.IsEmpty())
            {
                var t = types.Head;
                var ts = Generalize(tenv, t.Item2.Type);
                ret = ret.Cons(t.Item1, ts);
                types = types.Tail;
            }
            return ret;
        }

        /// <summary>
        /// 型スキームからインスタンスを作成
        /// </summary>
        /// <param name="typescheme">型スキーム</param>
        /// <returns>新しい型</returns>
        static MType Instantiate(MTypeScheme typescheme)
        {
            var map = new Dictionary<int, MType>();
            foreach (var id in typescheme.Bounded.ToArray())
            {
                map.Add(id, new TypeVar());
            }
            return MapTypeVar(map, typescheme.Type);
        }

        /// <summary>
        /// タグのインスタンスを作成
        /// </summary>
        /// <param name="tag">もととなるタグ</param>
        /// <returns>生成されたタグ</returns>
        static Tag GeneralizeTag(Tag tag)
        {
            var map = new Dictionary<int, MType>();
            foreach (var id in tag.Bounded.ToArray())
            {
                map.Add(id, new TypeVar());
            }
            var arg_types = tag.ArgTypes.Select(t => MapTypeVar(map, t));
            var type = MapTypeVar(map, tag.Type);
            return new Tag(tag.Name, tag.Index, tag.Bounded, arg_types.ToList(), type);
        }

        /// <summary>
        /// 型変数を辞書にしたがって新しいものに置き換える
        /// </summary>
        /// <param name="map">辞書</param>
        /// <param name="type">型</param>
        /// <returns>置換後の型</returns>
        static MType MapTypeVar(Dictionary<int, MType> map, MType type)
        {
            if (type is TypeVar)
            {
                var t = (TypeVar)type;
                if (t.Value == null)
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
                    return MapTypeVar(map, t.Value);
                }
            }
            else if (type is UserType)
            {
                var t = (UserType)type;
                var args = new List<MType>();
                foreach (var arg in t.Args)
                {
                    args.Add(MapTypeVar(map, arg));
                }
                return new UserType(t.Name, args);
            }
            else if (type is IntType || type is DoubleType || type is StringType ||
                type is CharType || type is UnitType || type is BoolType)
            {
                return type;
            }
            else if (type is FunType)
            {
                var t = (FunType)type;
                var arg = MapTypeVar(map, t.ArgType);
                var ret = MapTypeVar(map, t.RetType);
                return new FunType(arg, ret);
            }
            else if (type is ListType)
            {
                var t = (ListType)type;
                var elem_type = MapTypeVar(map, t.ElemType);
                return new ListType(elem_type);
            }
            else if (type is RefType)
            {
                var t = (RefType)type;
                var elem_type = MapTypeVar(map, t.ElemType);
                return new RefType(elem_type);
            }
            else if (type is TupleType)
            {
                var t = (TupleType)type;
                var types = t.Types.Select(typ => MapTypeVar(map, typ)).ToList();
                return new TupleType(types);
            }
            else if (type is DotNetType)
            {
                return type;
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
