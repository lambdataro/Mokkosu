namespace Mokkosu
{
    abstract class Value
    {
    }

    class ValueRef : Value
    {
        public Value Value { get; set; }

        public ValueRef()
        {
            Value = null;
        }
    }

    class IntValue : Value
    {
        public int Value { get; private set; }

        public IntValue(int value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    class FunValue : Value
    {
        public string VarName { get; private set; }
        public SExpr Body { get; private set; }
        public Env<Value> Env { get; private set; }

        public FunValue(string var_name, SExpr body, Env<Value> env)
        {
            VarName = var_name;
            Body = body;
            Env = env;
        }

        public override string ToString()
        {
            return "<Closure>";
        }
    }
}
