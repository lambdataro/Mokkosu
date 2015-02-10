using System.Collections.Generic;
using System.Linq;

namespace Mokkosu.AST
{
    /// <summary>
    /// 式の抽象クラス
    /// </summary>
    abstract class MExpr
    {
    }

    /// <summary>
    /// 整数定数
    /// </summary>
    class MInt : MExpr
    {
        public int Value { get; private set; }

        public MInt(int value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    /// <summary>
    /// 浮動小数点数定数
    /// </summary>
    class MDouble : MExpr
    {
        public double Value { get; private set; }

        public MDouble(double value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    /// <summary>
    /// 文字列定数
    /// </summary>
    class MString : MExpr
    {
        public string Value { get; private set; }

        public MString(string value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return "\"" + Value + "\"";
        }
    }

    /// <summary>
    /// 文字定数
    /// </summary>
    class MChar : MExpr
    {
        public char Value { get; private set; }

        public MChar(char value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return "\'" + Value.ToString() + "\'";
        }
    }

    /// <summary>
    /// ユニット定数
    /// </summary>
    class MUnit : MExpr
    {
        public override string ToString()
        {
            return "()";
        }
    }

    /// <summary>
    /// 真偽値定数
    /// </summary>
    class MBool : MExpr
    {
        public bool Value { get; private set; }

        public MBool(bool value)
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
        {
            Name = name;
            Type = new TypeVar();
            IsTag = false;
            TagIndex = 0;
            TagSize = 0;
        }

        public MVar(string name, MType type)
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
    }

    /// <summary>
    /// ラムダ式
    /// </summary>
    class MLambda : MExpr
    {
        public MPat ArgPat { get; private set; }
        public MType ArgType { get; private set; }
        public MExpr Body { get; private set; }

        public MLambda(MPat arg_pat, MExpr body)
        {
            ArgPat = arg_pat;
            ArgType = new TypeVar();
            Body = body;
        }

        public MLambda(MPat arg_pat, MType arg_type, MExpr body)
        {
            ArgPat = arg_pat;
            ArgType = arg_type;
            Body = body;
        }

        public override string ToString()
        {
            return string.Format("(\\{0} : {1} -> {2})", ArgPat, ArgType, Body);
        }
    }

    /// <summary>
    /// 関数適用
    /// </summary>
    class MApp : MExpr
    {
        public MExpr FunExpr { get; private set; }
        public MExpr ArgExpr { get; private set; }

        public MApp(MExpr fun_expr, MExpr arg_expr)
        {
            FunExpr = fun_expr;
            ArgExpr = arg_expr;
        }

        public override string ToString()
        {
            return string.Format("({0} {1})", FunExpr, ArgExpr);
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

        public MIf(MExpr cond_expr, MExpr then_expr, MExpr else_expr)
        {
            CondExpr = cond_expr;
            ThenExpr = then_expr;
            ElseExpr = else_expr;
        }

        public override string ToString()
        {
            return string.Format("(if {0} then {1} else {2})", CondExpr, ThenExpr, ElseExpr);
        }
    }

    /// <summary>
    /// パターンマッチ
    /// </summary>
    class MMatch : MExpr
    {
        public MPat Pat { get; private set; }
        public MExpr Expr { get; private set; }
        public MExpr ThenExpr { get; private set; }
        public MExpr ElseExpr { get; private set; }

        public MMatch(MPat pat, MExpr expr, MExpr then_expr, MExpr else_expr)
        {
            Pat = pat;
            Expr = expr;
            ThenExpr = then_expr;
            ElseExpr = else_expr;
        }

        public override string ToString()
        {
            return string.Format("(pat {0} = {1} -> {2} else {3})", Pat, Expr, ThenExpr, ElseExpr);
        }
    }

    /// <summary>
    /// 空リスト
    /// </summary>
    class MNil : MExpr
    {
        public MType Type { get; private set; }

        public MNil()
        {
            Type = new TypeVar();
        }

        public override string ToString()
        {
            return string.Format("([] : {0})", Type);
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

        public MCons(MExpr head, MExpr tail)
        {
            Head = head;
            Tail = tail;
            ItemType = new TypeVar();
        }

        public override string ToString()
        {
            return string.Format("(({0} :: {1}) : {2})", Head, Tail, ItemType);
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

        public MTuple(List<MExpr> items)
        {
            Size = items.Count;
            Items = items;
            Types = items.Select(item => (MType)(new TypeVar())).ToList();
        }

        public override string ToString()
        {
            return "(" + Utils.Utils.ListToString(Items) + ")";
        }
    }
}
