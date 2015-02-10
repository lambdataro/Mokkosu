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
}
