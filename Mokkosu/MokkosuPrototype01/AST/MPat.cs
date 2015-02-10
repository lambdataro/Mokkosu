using System.Collections.Generic;
namespace Mokkosu.AST
{
    abstract class MPat
    {
    }

    class PVar : MPat
    {
        public string Name { get; private set; }
        public MType Type { get; private set; }

        public PVar(string name)
        {
            Name = name;
            Type = new TypeVar();
        }

        public override string ToString()
        {
            return Name;
        }
    }

    class PWild : MPat
    {
        public MType Type { get; private set; }

        public PWild()
        {
            Type = new TypeVar();
        }

        public override string ToString()
        {
            return "_";
        }
    }

    class PInt : MPat
    {
        public int Value { get; private set; }

        public PInt(int value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    class PDouble : MPat
    {
        public double Value { get; private set; }

        public PDouble(double value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    class PString : MPat
    {
        public string Value { get; private set; }

        public PString(string value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return "\"" + Value + "\"";
        }
    }

    class PChar : MPat
    {
        public char Value { get; private set; }

        public PChar(char value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return "\'" + Value.ToString() + "\'";
        }
    }

    class PUnit : MPat
    {
        public override string ToString()
        {
            return "()";
        }
    }

    class PBool : MPat
    {
        public bool Value { get; private set; }

        public PBool(bool value)
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

    class PNil : MPat
    {
        public override string ToString()
        {
            return "[]";
        }
    }

    class PCons : MPat
    {
        public MPat Head { get; private set; }
        public MPat Tail { get; private set; }

        public PCons(MPat head, MPat tail)
        {
            Head = head;
            Tail = tail;
        }

        public override string ToString()
        {
            return string.Format("({0} :: {1})", Head, Tail);
        }
    }

    class PTuple : MPat
    {
        public List<MPat> Items { get; private set; }

        public PTuple(List<MPat> items)
        {
            Items = items;
        }

        public override string ToString()
        {
            return "(" + Utils.Utils.ListToString(Items) + ")";
        }
    }

    class PAs : MPat
    {
        public MPat Pat { get; private set; }
        public string Name { get; private set; }

        public PAs(MPat pat, string name)
        {
            Pat = pat;
            Name = name;
        }

        public override string ToString()
        {
            return string.Format("({0} as {1})", Pat, Name);
        }
    }

    class POr : MPat
    {
        public MPat Pat1 { get; private set; }
        public MPat Pat2 { get; private set; }

        public POr(MPat pat1, MPat pat2)
        {
            Pat1 = pat1;
            Pat2 = pat2;
        }

        public override string ToString()
        {
            return string.Format("({0} | {1})", Pat1, Pat2);
        }
    }
}
