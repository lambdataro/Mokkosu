using System.Collections.Generic;
namespace Mokkosu.AST
{
    abstract class MExpr
    {
    }

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

    class MUnit : MExpr
    {
        public override string ToString()
        {
            return "()";
        }
    }

    class MTag : MExpr
    {
        public string Name { get; private set; }
        public List<MExpr> Args { get; private set; }

        public MTag(string name, List<MExpr> args)
        {
            Name = name;
            Args = args;
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
}
