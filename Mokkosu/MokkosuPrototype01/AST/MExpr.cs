using Mokkosu.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Mokkosu.AST
{
    /// <summary>
    /// 式の抽象クラス
    /// </summary>
    abstract class MExpr
    {
        public abstract MSet<string> FreeVars();

        public string Pos { get; set; }

        public MExpr(string pos)
        {
            Pos = pos;
        }
    }

    /// <summary>
    /// 整数定数
    /// </summary>
    class MInt : MExpr
    {
        public int Value { get; private set; }

        public MInt(string pos, int value)
            : base(pos)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public override MSet<string> FreeVars()
        {
            return new MSet<string>();
        }
    }

    /// <summary>
    /// 浮動小数点数定数
    /// </summary>
    class MDouble : MExpr
    {
        public double Value { get; private set; }

        public MDouble(string pos, double value)
            : base(pos)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public override MSet<string> FreeVars()
        {
            return new MSet<string>();
        }
    }

    /// <summary>
    /// 文字列定数
    /// </summary>
    class MString : MExpr
    {
        public string Value { get; private set; }

        public MString(string pos, string value)
            : base(pos)
        {
            Value = value;
        }

        public override string ToString()
        {
            return "\"" + Value + "\"";
        }

        public override MSet<string> FreeVars()
        {
            return new MSet<string>();
        }
    }

    /// <summary>
    /// 文字定数
    /// </summary>
    class MChar : MExpr
    {
        public char Value { get; private set; }

        public MChar(string pos, char value)
            : base(pos)
        {
            Value = value;
        }

        public override string ToString()
        {
            return "\'" + Value.ToString() + "\'";
        }

        public override MSet<string> FreeVars()
        {
            return new MSet<string>();
        }
    }

    /// <summary>
    /// ユニット定数
    /// </summary>
    class MUnit : MExpr
    {
        public MUnit(string pos)
            : base(pos)
        {
        }

        public override string ToString()
        {
            return "()";
        }

        public override MSet<string> FreeVars()
        {
            return new MSet<string>();
        }
    }

    /// <summary>
    /// 真偽値定数
    /// </summary>
    class MBool : MExpr
    {
        public bool Value { get; private set; }

        public MBool(string pos, bool value)
            : base(pos)
        {
            Value = value;
        }

        public override string ToString()
        {
            if (Value)
            {
                return "true";
            }
            else
            {
                return "false";
            }
        }

        public override MSet<string> FreeVars()
        {
            return new MSet<string>();
        }
    }

    /// <summary>
    /// 変数
    /// </summary>
    class MVar : MExpr
    {
        public string Name { get; private set; }
        public MType Type { get; private set; }

        public bool IsTag { get; set; }
        public int TagIndex { get; set; }
        public int TagSize { get; set; }

        public MVar(string name)
            : base("")
        {
            Name = name;
            Type = new TypeVar();
            IsTag = false;
            TagIndex = 0;
            TagSize = 0;
        }

        public MVar(string pos, string name)
            : base(pos)
        {
            Name = name;
            Type = new TypeVar();
            IsTag = false;
            TagIndex = 0;
            TagSize = 0;
        }

        public MVar(string name, MType type)
            : base("")
        {
            Name = name;
            Type = type;
            IsTag = false;
            TagIndex = 0;
            TagSize = 0;
        }

        public override string ToString()
        {
            if (IsTag)
            {
                return string.Format("({0}({1}, {2}) : {3})", Name, TagIndex, TagSize, Type);
            }
            else
            {
                return string.Format("({0} : {1})", Name, Type);
            }
        }

        public override MSet<string> FreeVars()
        {
            if (IsTag)
            {
                return new MSet<string>();
            }
            else
            {
                return new MSet<string>(Name);
            }
        }
    }

    /// <summary>
    /// ラムダ式
    /// </summary>
    class MLambda : MExpr
    {
        public MPat ArgPat { get; private set; }
        public MType ArgType { get; private set; }
        public MExpr Body { get; private set; }

        public MLambda(string pos, MPat arg_pat, MExpr body)
            : base(pos)
        {
            ArgPat = arg_pat;
            ArgType = new TypeVar();
            Body = body;
        }

        public MLambda(MPat arg_pat, MType arg_type, MExpr body)
            : base("")
        {
            ArgPat = arg_pat;
            ArgType = arg_type;
            Body = body;
        }

        public override string ToString()
        {
            return string.Format("(\\{0} : {1} -> {2})", ArgPat, ArgType, Body);
        }

        public override MSet<string> FreeVars()
        {
            return Body.FreeVars().Diff(ArgPat.FreeVars());
        }
    }

    /// <summary>
    /// 関数適用
    /// </summary>
    class MApp : MExpr
    {
        public MExpr FunExpr { get; private set; }
        public MExpr ArgExpr { get; private set; }
        public bool TailCall { get; set; }

        public MApp(string pos, MExpr fun_expr, MExpr arg_expr)
            : base(pos)
        {
            FunExpr = fun_expr;
            ArgExpr = arg_expr;
            TailCall = false;
        }

        public override string ToString()
        {
            if (TailCall)
            {
                return string.Format("(tail {0} {1})", FunExpr, ArgExpr);
            }
            else
            {
                return string.Format("({0} {1})", FunExpr, ArgExpr);
            }
        }

        public override MSet<string> FreeVars()
        {
            return FunExpr.FreeVars().Union(ArgExpr.FreeVars());
        }
    }

    /// <summary>
    /// 条件分岐
    /// </summary>
    class MIf : MExpr
    {
        public MExpr CondExpr { get; private set; }
        public MExpr ThenExpr { get; private set; }
        public MExpr ElseExpr { get; private set; }

        public MIf(string pos, MExpr cond_expr, MExpr then_expr, MExpr else_expr)
            : base(pos)
        {
            CondExpr = cond_expr;
            ThenExpr = then_expr;
            ElseExpr = else_expr;
        }

        public override string ToString()
        {
            return string.Format("(if {0} then {1} else {2})", CondExpr, ThenExpr, ElseExpr);
        }

        public override MSet<string> FreeVars()
        {
            return CondExpr.FreeVars().Union(ThenExpr.FreeVars().Union(ElseExpr.FreeVars()));
        }
    }

    /// <summary>
    /// パターンマッチ
    /// </summary>
    class MMatch : MExpr
    {
        public MPat Pat { get; private set; }
        public MExpr Guard { get; private set; }
        public MExpr Expr { get; private set; }
        public MExpr ThenExpr { get; private set; }
        public MExpr ElseExpr { get; private set; }

        public MMatch(string pos, MPat pat, MExpr guard, MExpr expr, MExpr then_expr, MExpr else_expr)
            : base(pos)
        {
            Pat = pat;
            Guard = guard;
            Expr = expr;
            ThenExpr = then_expr;
            ElseExpr = else_expr;
        }

        public override string ToString()
        {
            return string.Format("(pat {0} ? {1} = {2} -> {3} else {4})", Pat, Guard, Expr, ThenExpr, ElseExpr);
        }

        public override MSet<string> FreeVars()
        {
            var fv1 = Guard.FreeVars().Union(ThenExpr.FreeVars());
            var fv2 = fv1.Diff(Pat.FreeVars());
            return fv2.Union(Expr.FreeVars()).Union(ElseExpr.FreeVars());
        }
    }

    /// <summary>
    /// 空リスト
    /// </summary>
    class MNil : MExpr
    {
        public MType Type { get; private set; }

        public MNil(string pos)
            : base(pos)
        {
            Type = new TypeVar();
        }

        public override string ToString()
        {
            return string.Format("([] : {0})", Type);
        }

        public override MSet<string> FreeVars()
        {
            return new MSet<string>();
        }
    }

    /// <summary>
    /// コンス
    /// </summary>
    class MCons : MExpr
    {
        public MExpr Head { get; private set; }
        public MExpr Tail { get; private set; }
        public MType ItemType { get; private set; }

        public MCons(string pos, MExpr head, MExpr tail)
            : base(pos)
        {
            Head = head;
            Tail = tail;
            ItemType = new TypeVar();
        }

        public override string ToString()
        {
            return string.Format("(({0} :: {1}) : {2})", Head, Tail, ItemType);
        }

        public override MSet<string> FreeVars()
        {
            return Head.FreeVars().Union(Tail.FreeVars());
        }
    }

    /// <summary>
    /// タプル
    /// </summary>
    class MTuple : MExpr
    {
        public List<MExpr> Items { get; private set; }
        public List<MType> Types { get; private set; }
        public int Size { get; private set; }

        public MTuple(string pos, List<MExpr> items)
            : base(pos)
        {
            Size = items.Count;
            Items = items;
            Types = items.Select(item => (MType)(new TypeVar())).ToList();
        }

        public override string ToString()
        {
            return "(" + Utils.Utils.ListToString(Items) + ")";
        }

        public override MSet<string> FreeVars()
        {
            var set = new MSet<string>();
            foreach (var item in Items)
            {
                set = item.FreeVars().Union(set);
            }
            return set;
        }
    }

    /// <summary>
    /// do式
    /// </summary>
    class MDo : MExpr
    {
        public MExpr E1 { get; private set; }
        public MExpr E2 { get; private set; }
        public MType E1Type { get; private set; }
        public MType E2Type { get; private set; }

        public MDo(string pos, MExpr e1, MExpr e2)
            : base(pos)
        {
            E1 = e1;
            E2 = e2;
            E1Type = new TypeVar();
            E2Type = new TypeVar();
        }

        public override string ToString()
        {
            return string.Format("(do {0}; {1}", E1, E2);
        }

        public override MSet<string> FreeVars()
        {
            return E1.FreeVars().Union(E2.FreeVars());
        }
    }

    /// <summary>
    /// let式
    /// </summary>
    class MLet : MExpr
    {
        public MPat Pat { get; private set; }
        public MExpr E1 { get; private set; }
        public MExpr E2 { get; private set; }
        public MType E1Type { get; private set; }
        public MType E2Type { get; private set; }

        public MLet(string pos, MPat pat, MExpr e1, MExpr e2)
            : base(pos)
        {
            Pat = pat;
            E1 = e1;
            E2 = e2;
            E1Type = new TypeVar();
            E2Type = new TypeVar();
        }

        public override string ToString()
        {
            return string.Format("(let {0} = {1}; {2})", Pat, E1, E2);
        }

        public override MSet<string> FreeVars()
        {
            return E1.FreeVars().Union(E2.FreeVars().Diff(Pat.FreeVars()));
        }
    }

    /// <summary>
    /// fun式
    /// </summary>
    class MFun : MExpr
    {
        public List<MFunItem> Items { get; private set; }
        public MExpr E2 { get; private set; }

        public MFun(string pos, List<MFunItem> items, MExpr e2)
            : base(pos)
        {
            Items = items;
            E2 = e2;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine("\n=== fun ===");

            foreach (var item in Items)
            {
                sb.AppendLine(item.ToString());
            }

            sb.Append("=== end fun. ===\n");
            sb.Append(E2);

            return sb.ToString();
        }

        public override MSet<string> FreeVars()
        {
            var set1 = new MSet<string>();
            var set2 = new MSet<string>();
            foreach (var item in Items)
            {
                set1 = item.Expr.FreeVars().Union(set1);
                set2 = new MSet<string>(item.Name).Union(set2);
            }
            return E2.FreeVars().Diff(set2).Union(set1);
        }
    }

    /// <summary>
    /// fun式で相互再帰する各定義
    /// </summary>
    class MFunItem
    {
        public string Name { get; private set; }
        public MExpr Expr { get; private set; }
        public MType Type { get; private set; }

        public MFunItem(string name, MExpr expr)
        {
            Name = name;
            Expr = expr;
            Type = new TypeVar();
        }

        public override string ToString()
        {
            return string.Format("{0} : {1} = {2};", Name, Type, Expr);
        }
    }

    /// <summary>
    /// 型強制
    /// </summary>
    class MFource : MExpr
    {
        public MExpr Expr { get; private set; }
        public MType Type { get; private set; }

        public MFource(string pos, MExpr expr, MType type)
            : base(pos)
        {
            Expr = expr;
            Type = type;
        }

        public override string ToString()
        {
            return string.Format("({0} : {1})", Expr, Type);
        }

        public override MSet<string> FreeVars()
        {
            return Expr.FreeVars();
        }
    }

    /// <summary>
    /// 実行時エラーを出力する
    /// </summary>
    class MRuntimeError : MExpr
    {
        public string Message { get; private set; }

        public MRuntimeError(string pos, string message)
            : base(pos)
        {
            Message = message;
        }

        public override string ToString()
        {
            return string.Format("runtime_error(\"{0}\")", Message);
        }

        public override MSet<string> FreeVars()
        {
            return new MSet<string>();
        }
    }

    /// <summary>
    /// プリミティブ呼び出し
    /// </summary>
    class MPrim : MExpr
    {
        public string Name { get; private set; }
        public List<MExpr> Args { get; private set; }
        public List<MType> ArgTypes { get; private set; }
        public MType RetType { get; private set; }

        public MPrim(string pos, string name, List<MExpr> items)
            : base(pos)
        {
            Name = name;
            Args = items;
            ArgTypes = items.Select(item => (MType)(new TypeVar())).ToList();
            RetType = new TypeVar();
        }

        public MPrim(string pos, string name, List<MExpr> items, 
            List<MType> arg_types, MType ret_type)
            : base(pos)
        {
            Name = name;
            Args = items;
            ArgTypes = arg_types;
            RetType = ret_type;
        }

        public override string ToString()
        {
            return string.Format("__prim \"{0}\" ({1})", Name, Utils.Utils.ListToString(Args));
        }

        public override MSet<string> FreeVars()
        {
            var set = new MSet<string>();
            foreach (var item in Args)
            {
                set = item.FreeVars().Union(set);
            }
            return set;
        }
    }

    class MCallStatic : MExpr
    {
        public string ClassName { get; private set; }
        public string MethodName { get; private set; }
        public List<MExpr> Args { get; private set; }
        public List<MType> Types { get; private set; }
        public MethodInfo Info { get; set; }

        public MCallStatic(string pos, string class_name, string method_name, List<MExpr> args)
            : base(pos)
        {
            ClassName = class_name;
            MethodName = method_name;
            Args = args;
            Types = args.Select(item => (MType)(new TypeVar())).ToList();
            Info = null;
        }

        public MCallStatic(string pos, string class_name, string method_name, List<MExpr> args,
            List<MType> types, MethodInfo info)
            : base(pos)
        {
            ClassName = class_name;
            MethodName = method_name;
            Args = args;
            Types = types;
            Info = info;
        }

        public override string ToString()
        {
            return string.Format("call {0}::{1}({2})",
                ClassName, MethodName, Utils.Utils.ListToString(Args));
        }

        public override MSet<string> FreeVars()
        {
            var set = new MSet<string>();
            foreach (var item in Args)
            {
                set = item.FreeVars().Union(set);
            }
            return set;
        }
    }

    class MCast : MExpr
    {
        public string SrcTypeName { get; private set; }
        public Type SrcType { get; set; }
        public string DstTypeName { get; private set; }
        public Type DstType { get; set; }
        public MExpr Expr { get; private set; }

        public MCast(string pos, string src_name, string dst_name, MExpr expr)
            : base(pos)
        {
            SrcTypeName = src_name;
            SrcType = null;
            DstTypeName = dst_name;
            DstType = null;
            Expr = expr;
        }

        public MCast(string pos, string src_name, Type src_type, string dst_name, Type dst_type, MExpr expr)
            : base(pos)
        {
            SrcTypeName = src_name;
            SrcType = src_type;
            DstTypeName = dst_name;
            DstType = dst_type;
            Expr = expr;
        }

        public override string ToString()
        {
            return string.Format("cast<{0},{1}>({2})", SrcTypeName, DstTypeName, Expr);
        }

        public override MSet<string> FreeVars()
        {
            return Expr.FreeVars();
        }
    }

    class MNewClass : MExpr
    {
        public string ClassName { get; private set; }
        public List<MExpr> Args { get; private set; }
        public List<MType> Types { get; private set; }
        public ConstructorInfo Info { get; set; }

        public MNewClass(string pos, string class_name, List<MExpr> args)
            : base(pos)
        {
            ClassName = class_name;
            Args = args;
            Types = args.Select(item => (MType)(new TypeVar())).ToList();
            Info = null;
        }

        public MNewClass(string pos, string class_name, List<MExpr> args,
            List<MType> types, ConstructorInfo info)
            : base(pos)
        {
            ClassName = class_name;
            Args = args;
            Types = types;
            Info = info;
        }

        public override string ToString()
        {
            return string.Format("new {0}({1})",
                ClassName,  Utils.Utils.ListToString(Args));
        }

        public override MSet<string> FreeVars()
        {
            var set = new MSet<string>();
            foreach (var item in Args)
            {
                set = item.FreeVars().Union(set);
            }
            return set;
        }
    }

    class MInvoke : MExpr
    {
        public MExpr Expr { get; private set; }
        public MType ExprType { get; private set; }
        public string MethodName { get; private set; }
        public List<MExpr> Args { get; private set; }
        public List<MType> Types { get; private set; }
        public MethodInfo Info { get; set; }

        public MInvoke(string pos, MExpr expr, string method_name, List<MExpr> args)
            : base(pos)
        {
            Expr = expr;
            ExprType = new TypeVar();
            MethodName = method_name;
            Args = args;
            Types = args.Select(item => (MType)(new TypeVar())).ToList();
            Info = null;
        }

        public MInvoke(string pos, MExpr expr, MType expr_type, string method_name, 
            List<MExpr> args, List<MType> types, MethodInfo info)
            : base(pos)
        {
            Expr = expr;
            ExprType = expr_type;
            MethodName = method_name;
            Args = args;
            Types = types;
            Info = info;
        }

        public override string ToString()
        {
            return string.Format("{0}.{1}({2})",
                Expr, MethodName, Utils.Utils.ListToString(Args));
        }

        public override MSet<string> FreeVars()
        {
            var set = Expr.FreeVars();
            foreach (var item in Args)
            {
                set = item.FreeVars().Union(set);
            }
            return set;
        }
    }

    class MDelegate : MExpr
    {
        public string ClassName { get; private set; }
        public MExpr Expr { get; private set; }
        public MType ExprType { get; private set; }
        public Type[] ParamType { get; set; }
        public Type ClassType { get; set; }
        public ConstructorInfo CstrInfo { get; set; }

        public MDelegate(string pos, string class_name, MExpr expr)
            : base(pos)
        {
            ClassName = class_name;
            Expr = expr;
            ExprType = new TypeVar();
            ParamType = null;
            ClassType = null;
            CstrInfo = null;
        }

        public MDelegate(string pos, string class_name, MExpr expr, 
            MType expr_type, Type[] param_type, Type class_type, ConstructorInfo cstr_info)
            : base(pos)
        {
            ClassName = class_name;
            Expr = expr;
            ExprType = expr_type;
            ParamType = param_type;
            ClassType = class_type;
            CstrInfo = cstr_info;
        }

        public override string ToString()
        {
            return string.Format("delegate {0}({1})",
                ClassName, Expr);
        }

        public override MSet<string> FreeVars()
        {
            return Expr.FreeVars();
        }
    }

    class MSet : MExpr
    {
        public MExpr Expr { get; private set; }
        public MType ExprType { get; private set; }
        public string FieldName { get; private set; }
        public MExpr Arg { get; private set; }
        public MType ArgType { get; private set; }
        public FieldInfo Info { get; set; }

        public MSet(string pos, MExpr expr, string field_name, MExpr arg)
            : base(pos)
        {
            Expr = expr;
            ExprType = new TypeVar();
            FieldName = field_name;
            Arg = arg;
            ArgType = new TypeVar();
            Info = null;
        }

        public MSet(string pos, MExpr expr, MType type, string field_name, 
            MExpr arg, MType arg_type, FieldInfo info)
            : base(pos)
        {
            Expr = expr;
            ExprType = type;
            FieldName = field_name;
            Arg = arg;
            ArgType = arg_type;
            Info = info;
        }

        public override string ToString()
        {
            return string.Format("set {0}.{1} = {2}", Expr, FieldName, Arg);
        }

        public override MSet<string> FreeVars()
        {
            var set1 = Expr.FreeVars();
            var set2 = Arg.FreeVars();
            return set1.Union(set2);
        }
    }

    class MGet : MExpr
    {
        public MExpr Expr { get; private set; }
        public MType ExprType { get; private set; }
        public string FieldName { get; private set; }
        public FieldInfo Info { get; set; }

        public MGet(string pos, MExpr expr, string field_name)
            : base(pos)
        {
            Expr = expr;
            ExprType = new TypeVar();
            FieldName = field_name;
            Info = null;
        }

        public MGet(string pos, MExpr expr, MType type, string field_name, FieldInfo info)
            : base(pos)
        {
            Expr = expr;
            ExprType = type;
            FieldName = field_name;
            Info = info;
        }

        public override string ToString()
        {
            return string.Format("get {0}.{1}", Expr, FieldName);
        }

        public override MSet<string> FreeVars()
        {
            return Expr.FreeVars();
        }
    }

    class MSSet : MExpr
    {
        public string ClassName { get; private set; }
        public string FieldName { get; private set; }
        public MExpr Arg { get; private set; }
        public MType ArgType { get; private set; }
        public FieldInfo Info { get; set; }

        public MSSet(string pos, string class_name, string field_name, MExpr arg)
            : base(pos)
        {
            ClassName = class_name;
            FieldName = field_name;
            Arg = arg;
            ArgType = new TypeVar();
            Info = null;
        }

        public MSSet(string pos, string class_name, string field_name,
            MExpr arg, MType arg_type, FieldInfo info)
            : base(pos)
        {
            ClassName = class_name;
            FieldName = field_name;
            Arg = arg;
            ArgType = arg_type;
            Info = info;
        }

        public override string ToString()
        {
            return string.Format("set {0}.{1} = {2}", ClassName, FieldName, Arg);
        }

        public override MSet<string> FreeVars()
        {
            return Arg.FreeVars();
        }
    }

    class MSGet : MExpr
    {
        public string ClassName { get; private set; }
        public string FieldName { get; private set; }
        public FieldInfo Info { get; set; }

        public MSGet(string pos, string class_name, string field_name)
            : base(pos)
        {
            ClassName = class_name;
            FieldName = field_name;
            Info = null;
        }

        public MSGet(string pos, string class_name, string field_name, FieldInfo info)
            : base(pos)
        {
            ClassName = class_name;
            FieldName = field_name;
            Info = info;
        }

        public override string ToString()
        {
            return string.Format("get {0}.{1}", ClassName, FieldName);
        }

        public override MSet<string> FreeVars()
        {
            return new MSet<string>();
        }
    }

    /// <summary>
    /// 引数の値を取得する (クロージャ変換後に利用)
    /// </summary>
    class MGetArg : MExpr
    {
        public MGetArg()
            : base("") 
        { 
        }

        public override string ToString()
        {
            return "<GetArg>";
        }

        public override MSet<string> FreeVars()
        {
            throw new System.NotImplementedException();
        }
    }

    /// <summary>
    /// クロージャによって束縛された値を取得する (クロージャ変換後に利用)
    /// </summary>
    class MGetEnv : MExpr
    {
        public int Index { get; private set; }

        public MGetEnv(int index)
            : base("")
        {
            Index = index;
        }

        public override string ToString()
        {
            return string.Format("<GetEnv {0}>", Index);
        }

        public override MSet<string> FreeVars()
        {
            throw new System.NotImplementedException();
        }
    }

    /// <summary>
    /// クロージャを作る (クロージャ変換後に利用)
    /// </summary>
    class MMakeClos : MExpr
    {
        public string ClosName { get; private set; }
        public MExpr[] Args { get; private set; }

        public MMakeClos(string clos_name, MExpr[] args)
            : base("")
        {
            ClosName = clos_name;
            Args = args;
        }

        public override string ToString()
        {
            return string.Format("(MakeClos {0}({1})", 
                ClosName, Utils.Utils.ListToString(Args.ToList()));
        }

        public override MSet<string> FreeVars()
        {
            throw new System.NotImplementedException();
        }
    }

    /// <summary>
    /// クロージャでキャプチャされる変数の値を取得する変数 (クロージャ変換後に利用)
    /// </summary>
    class MVarClos : MExpr
    {
        public string Name { get; private set; }

        public MVarClos(string name)
            : base("")
        {
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }

        public override MSet<string> FreeVars()
        {
            return new MSet<string>(Name);
        }
    }
}
