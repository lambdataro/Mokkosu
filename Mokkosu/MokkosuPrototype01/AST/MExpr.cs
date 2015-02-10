using System.Collections.Generic;
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
    /// タグ
    /// </summary>
    class MTag : MExpr
    {
        public string Name { get; private set; }
        public List<MExpr> Args { get; private set; }
        public int Index { get; set; }

        public MTag(string name, List<MExpr> args)
        {
            Name = name;
            Args = args;
            Index = 0;
        }

        public override string ToString()
        {
            if (Args.Count == 0)
            {
                return Name;
            }
            else
            {
                return string.Format("{0}({1})", Name, Utils.Utils.ListToString(Args));
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

        public MVar(string name)
        {
            Name = name;
            Type = new TypeVar();
        }

        public MVar(string name, MType type)
        {
            Name = name;
            Type = type;
        }

        public override string ToString()
        {
            return string.Format("({0} : {1})", Name, Type);
        }
    }

    class MLambda : MExpr
    {
        public string ArgName { get; private set; }
        public MType ArgType { get; private set; }
        public MExpr Body { get; private set; }

        public MLambda(string arg_name, MExpr body)
        {
            ArgName = arg_name;
            ArgType = new TypeVar();
            Body = body;
        }

        public MLambda(string arg_name, MType arg_type, MExpr body)
        {
            ArgName = arg_name;
            ArgType = arg_type;
            Body = body;
        }

        public override string ToString()
        {
            return string.Format("(\\{0} : {1} -> {2})", ArgName, ArgType, Body);
        }
    }

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
}
