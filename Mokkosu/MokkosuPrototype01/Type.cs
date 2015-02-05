namespace Mokkosu
{
    /// <summary>
    /// 型の抽象クラス
    /// </summary>
    abstract class Type
    {
    }

    /// <summary>
    /// 型変数
    /// </summary>
    class TypeVar : Type
    {
        static int var_id = 0;

        public int Id { get; private set; }
        public Type Var { get; set; }

        public TypeVar()
        {
            Id = var_id++;
            Var = null;
        }

        public override string ToString()
        {
            return string.Format("<{0}>", Id);
        }
    }

    /// <summary>
    /// 整数型
    /// </summary>
    class IntType : Type
    {
        public override string ToString()
        {
            return "Int";
        }
    }

    /// <summary>
    /// 関数型
    /// </summary>
    class FunType : Type
    {
        public Type ArgType { get; private set; }
        public Type RetType { get; private set; }

        public FunType(Type arg_type, Type ret_type)
        {
            ArgType = arg_type;
            RetType = ret_type;
        }

        public override string ToString()
        {
            return string.Format("({0} -> {1})", ArgType, RetType);
        }
    }
}
