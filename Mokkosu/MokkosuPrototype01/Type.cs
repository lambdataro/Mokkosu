namespace Mokkosu
{
    abstract class Type
    {
    }

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

    class IntType : Type
    {
        public override string ToString()
        {
            return "Int";
        }
    }

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
