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

        public SConstInt(int vlaue)
        {
            Value = Value;
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
    }

    /// <summary>
    /// 変数
    /// </summary>
    class SVar : SExpr
    {
        public string Name { get; private set; }

        public SVar(string name)
        {
            Name = name;
        }
    }

    /// <summary>
    /// ラムダ式
    /// </summary>
    class SFun : SExpr
    {
        public string VarName { get; private set; }
        public SExpr Body { get; private set; }

        public SFun(string var_name, SExpr body)
        {
            VarName = var_name;
            Body = body;
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
    }
}
