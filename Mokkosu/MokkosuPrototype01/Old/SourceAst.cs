using System.Text;
namespace Mokkosu
{
    /// <summary>
    /// 式
    /// </summary>
    abstract class SExpr
    {
    }

    /// <summary>
    /// 整数定数
    /// </summary>
    class SConstInt : SExpr
    {
        public int Value { get; private set; }

        public SConstInt(int value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    /// <summary>
    /// 二項演算子
    /// </summary>
    abstract class SBinop : SExpr
    {
        public SExpr Lhs { get; private set; }
        public SExpr Rhs { get; private set; }

        public SBinop(SExpr lhs, SExpr rhs)
        {
            Lhs = lhs;
            Rhs = rhs;
        }
    }

    /// <summary>
    /// 整数加算
    /// </summary>
    class SAdd : SBinop
    {
        public SAdd(SExpr lhs, SExpr rhs)
            : base(lhs, rhs)
        {
        }

        public override string ToString()
        {
            return string.Format("({0} + {1})", Lhs, Rhs);
        }
    }

    /// <summary>
    /// 整数減算
    /// </summary>
    class SSub : SBinop
    {
        public SSub(SExpr lhs, SExpr rhs)
            : base(lhs, rhs)
        {
        }

        public override string ToString()
        {
            return string.Format("({0} - {1})", Lhs, Rhs);
        }
    }

    /// <summary>
    /// 整数乗算
    /// </summary>
    class SMul : SBinop
    {
        public SMul(SExpr lhs, SExpr rhs)
            : base(lhs, rhs)
        {
        }

        public override string ToString()
        {
            return string.Format("({0} * {1})", Lhs, Rhs);
        }
    }

    /// <summary>
    /// 整数除算
    /// </summary>
    class SDiv : SBinop
    {
        public SDiv(SExpr lhs, SExpr rhs)
            : base(lhs, rhs)
        {
        }

        public override string ToString()
        {
            return string.Format("({0} / {1})", Lhs, Rhs);
        }
    }

    /// <summary>
    /// 等しい
    /// </summary>
    class SEq : SBinop
    {
        public SEq(SExpr lhs, SExpr rhs)
            : base(lhs, rhs)
        {
        }

        public override string ToString()
        {
            return string.Format("({0} == {1})", Lhs, Rhs);
        }
    }

    /// <summary>
    /// 変数
    /// </summary>
    class SVar : SExpr
    {
        public string Name { get; private set; }
        public Type VarType { get; private set; }

        public SVar(string name)
        {
            Name = name;
            VarType = new TypeVar();
        }

        public SVar(string name, Type type)
        {
            Name = name;
            VarType = type;
        }

        public override string ToString()
        {
            return string.Format("({0} : {1})", Name, VarType);
        }
    }

    class STag : SExpr
    {
        public string Name { get; private set; }
        public Type TagType { get; private set; }
        public int ArgsCount { get; private set; }

        public STag(string name, int args_count)
        {
            Name = name;
            TagType = new TypeVar();
            ArgsCount = args_count;
        }

        public STag(string name, int args_count, Type type)
        {
            Name = name;
            TagType = type;
            ArgsCount = args_count;
        }

        public override string ToString()
        {
            return string.Format("({0} : {1})", Name, TagType);
        }
    }

    /// <summary>
    /// ラムダ式
    /// </summary>
    class SFun : SExpr
    {
        public string ArgName { get; private set; }
        public Type ArgType { get; private set; }
        public SExpr Body { get; private set; }

        public SFun(string arg_name, SExpr body)
        {
            ArgName = arg_name;
            ArgType = new TypeVar();
            Body = body;
        }

        public SFun(string arg_name, Type arg_type, SExpr body)
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

    /// <summary>
    /// 関数適用
    /// </summary>
    class SApp : SExpr
    {
        public SExpr FunExpr { get; private set; }
        public SExpr ArgExpr { get; private set; }

        public SApp(SExpr fun_expr, SExpr arg_expr)
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
    /// let式
    /// </summary>
    class SLet : SExpr
    {
        public string VarName { get; private set; }
        public Type VarType { get; private set; }
        public SExpr E1 { get; private set; }
        public SExpr E2 { get; private set; }

        public SLet(string var_name, SExpr e1, SExpr e2)
        {
            VarName = var_name;
            VarType = new TypeVar();
            E1 = e1;
            E2 = e2;
        }

        public SLet(string var_name, Type var_type, SExpr e1, SExpr e2)
        {
            VarName = var_name;
            VarType = var_type;
            E1 = e1;
            E2 = e2;
        }

        public override string ToString()
        {
            return string.Format("(let {0} : {1} = {2} in {3})", VarName, VarType, E1, E2);
        }
    }

    /// <summary>
    /// rec式
    /// </summary>
    class SRec : SExpr
    {
        public string VarName { get; private set; }
        public Type VarType { get; private set; }
        public SExpr E1 { get; private set; }
        public SExpr E2 { get; private set; }

        public SRec(string var_name, SExpr e1, SExpr e2)
        {
            VarName = var_name;
            VarType = new TypeVar();
            E1 = e1;
            E2 = e2;
        }

        public SRec(string var_name, Type var_type, SExpr e1, SExpr e2)
        {
            VarName = var_name;
            VarType = var_type;
            E1 = e1;
            E2 = e2;
        }

        public override string ToString()
        {
            return string.Format("(rec {0} : {1} = {2} in {3})", VarName, VarType, E1, E2);
        }
    }

    /// <summary>
    /// If式
    /// </summary>
    class SIf : SExpr
    {
        public SExpr CondExpr { get; private set; }
        public SExpr ThenExpr { get; private set; }
        public SExpr ElseExpr { get; private set; }

        public SIf(SExpr cond_expr, SExpr then_expr, SExpr else_expr)
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
    /// Print式
    /// </summary>
    class SPrint : SExpr
    {
        public SExpr Body { get; private set; }

        public SPrint(SExpr body)
        {
            Body = body;
        }

        public override string ToString()
        {
            return string.Format("(print {0})", Body);
        }
    }

    /// <summary>
    /// 引数の値を取得する (クロージャ変換後に利用)
    /// </summary>
    class SGetArg : SExpr
    {
        public override string ToString()
        {
            return "<GetArg>";
        }
    }

    /// <summary>
    /// クロージャによって束縛された値を取得する (クロージャ変換後に利用)
    /// </summary>
    class SGetEnv : SExpr
    {
        public int Index { get; private set; }

        public SGetEnv(int index)
        {
            Index = index;
        }

        public override string ToString()
        {
            return string.Format("<GetEnv {0}>", Index);
        }
    }

    /// <summary>
    /// クロージャを作る (クロージャ変換後に利用)
    /// </summary>
    class SMakeClos : SExpr
    {
        public string ClosName { get; private set; }
        public SExpr[] Args { get; private set; }

        public SMakeClos(string clos_name, SExpr[] args)
        {
            ClosName = clos_name;
            Args = args;
        }

        public override string ToString()
        {
            return string.Format("(MakeClos {0}{1})", ClosName, ArgsToString());
        }

        string ArgsToString()
        {
            var sb = new StringBuilder();
            foreach (var expr in Args)
            {
                sb.AppendFormat(" {0}", expr);
            }
            return sb.ToString();
        }
    }
    
    /// <summary>
    /// クロージャでキャプチャされる変数の値を取得する専用の変数 (クロージャ変換後に利用)
    /// </summary>
    class SVarClos : SExpr
    {
        public string Name { get; private set; }

        public SVarClos(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return string.Format("{0}", Name);
        }
    }
}
