namespace Mokkosu.AST
{
    abstract class MType
    {
    }

    class TypeVar : MType
    {
        static int var_id_counter = 0;

        public int Id { get; private set; }
        public MType Value { get; set; }

        public TypeVar()
        {
            Id = var_id_counter++;
            Value = null;
        }

        public override string ToString()
        {
            if (Value == null)
            {
                return string.Format("<{0}>", Id);
            }
            else
            {
                return Value.ToString();
            }
        }
    }

    class UserType : MType
    {
        public string Name { get; private set; }

        public UserType(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    class IntType : MType
    {
        public override string ToString()
        {
            return "Int";
        }
    }

    class DoubleType : MType
    {
        public override string ToString()
        {
            return "Double";
        }
    }

    class StringType : MType
    {
        public override string ToString()
        {
            return "String";
        }
    }

    class CharType : MType
    {
        public override string ToString()
        {
            return "Char";
        }
    }

    class UnitType : MType
    {
        public override string ToString()
        {
            return "()";
        }
    }
}
