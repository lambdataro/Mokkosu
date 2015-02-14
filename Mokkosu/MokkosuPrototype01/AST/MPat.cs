using Mokkosu.Utils;
using System.Collections.Generic;
using System.Linq;

namespace Mokkosu.AST
{
    /// <summary>
    /// パターンの抽象クラス
    /// </summary>
    abstract class MPat
    {
        public abstract MSet<string> FreeVars();

        public string Pos { get; set; }

        public MPat(string pos)
        {
            Pos = pos;
        }
    }

    /// <summary>
    /// 変数パターン
    /// </summary>
    class PVar : MPat
    {
        public string Name { get; private set; }
        public MType Type { get; private set; }

        public bool IsTag { get; set; }
        public int TagIndex { get; set; }

        public PVar(string pos, string name)
            : base(pos)
        {
            Name = name;
            Type = new TypeVar();
            IsTag = false;
            TagIndex = 0;
        }

        public override string ToString()
        {
            return Name;
        }

        public override MSet<string> FreeVars()
        {
            return new MSet<string>(Name);
        }
    }

    /// <summary>
    /// ワイルドカードパターン
    /// </summary>
    class PWild : MPat
    {
        public MType Type { get; private set; }

        public PWild(string pos)
            : base(pos)
        {
            Type = new TypeVar();
        }

        public override string ToString()
        {
            return "_";
        }

        public override MSet<string> FreeVars()
        {
            return new MSet<string>();
        }
    }

    /// <summary>
    /// 型強制
    /// </summary>
    class PFource : MPat
    {
        public MPat Pat { get; private set; }
        public MType Type { get; private set; }

        public PFource(string pos, MPat pat, MType type)
            : base(pos)
        {
            Pat = pat;
            Type = type;
        }

        public override string ToString()
        {
            return string.Format("({0} : {1})", Pat, Type);
        }

        public override MSet<string> FreeVars()
        {
            return Pat.FreeVars();
        }
    }

    /// <summary>
    /// 整数パターン
    /// </summary>
    class PInt : MPat
    {
        public int Value { get; private set; }

        public PInt(string pos, int value)
            : base(pos)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public override MSet<string> FreeVars()
        {
            return new MSet<string>();
        }
    }

    /// <summary>
    /// 浮動小数点パターン
    /// </summary>
    class PDouble : MPat
    {
        public double Value { get; private set; }

        public PDouble(string pos, double value)
            : base(pos)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public override MSet<string> FreeVars()
        {
            return new MSet<string>();
        }
    }

    /// <summary>
    /// 文字列パターン
    /// </summary>
    class PString : MPat
    {
        public string Value { get; private set; }

        public PString(string pos, string value)
            : base(pos)
        {
            Value = value;
        }

        public override string ToString()
        {
            return "\"" + Value + "\"";
        }

        public override MSet<string> FreeVars()
        {
            return new MSet<string>();
        }
    }

    /// <summary>
    /// 文字パターン
    /// </summary>
    class PChar : MPat
    {
        public char Value { get; private set; }

        public PChar(string pos, char value)
            : base(pos)
        {
            Value = value;
        }

        public override string ToString()
        {
            return "\'" + Value.ToString() + "\'";
        }

        public override MSet<string> FreeVars()
        {
            return new MSet<string>();
        }
    }

    /// <summary>
    /// ユニットパターン
    /// </summary>
    class PUnit : MPat
    {
        public PUnit(string pos)
            : base(pos)
        {
        }

        public override string ToString()
        {
            return "()";
        }

        public override MSet<string> FreeVars()
        {
            return new MSet<string>();
        }
    }

    /// <summary>
    /// 真偽値パターン
    /// </summary>
    class PBool : MPat
    {
        public bool Value { get; private set; }

        public PBool(string pos, bool value)
            : base(pos)
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

        public override MSet<string> FreeVars()
        {
            return new MSet<string>();
        }
    }

    /// <summary>
    /// 空リストパターン
    /// </summary>
    class PNil : MPat
    {
        public MType ItemType { get; private set; }

        public PNil(string pos)
            : base(pos)
        {
            ItemType = new TypeVar();
        }

        public override string ToString()
        {
            return "[]";
        }

        public override MSet<string> FreeVars()
        {
            return new MSet<string>();
        }
    }

    /// <summary>
    /// コンスパターン
    /// </summary>
    class PCons : MPat
    {
        public MPat Head { get; private set; }
        public MPat Tail { get; private set; }
        public MType ItemType { get; private set; }

        public PCons(string pos, MPat head, MPat tail)
            : base(pos)
        {
            Head = head;
            Tail = tail;
            ItemType = new TypeVar();
        }

        public override string ToString()
        {
            return string.Format("({0} :: {1})", Head, Tail);
        }

        public override MSet<string> FreeVars()
        {
            return Head.FreeVars().Union(Tail.FreeVars());
        }
    }

    /// <summary>
    /// タプルパターン
    /// </summary>
    class PTuple : MPat
    {
        public List<MPat> Items { get; private set; }
        public List<MType> Types { get; private set; }
        public int Size { get; private set; }

        public PTuple(string pos, List<MPat> items)
            : base(pos)
        {
            Items = items;
            Types = items.Select(item => (MType)(new TypeVar())).ToList();
            Size = items.Count;
        }

        public override string ToString()
        {
            return "(" + Utils.Utils.ListToString(Items) + ")";
        }

        public override MSet<string> FreeVars()
        {
            var set = new MSet<string>();
            foreach (var pat in Items)
            {
                set = pat.FreeVars().Union(set);
            }
            return set;
        }
    }

    /// <summary>
    /// asパターン
    /// </summary>
    class PAs : MPat
    {
        public MPat Pat { get; private set; }
        public string Name { get; private set; }
        public MType Type { get; private set; }

        public PAs(string pos, MPat pat, string name)
            : base(pos)
        {
            Pat = pat;
            Name = name;
            Type = new TypeVar();
        }

        public override string ToString()
        {
            return string.Format("({0} as {1})", Pat, Name);
        }

        public override MSet<string> FreeVars()
        {
            return Pat.FreeVars().Union(new MSet<string>(Name));
        }
    }

    /// <summary>
    /// orパターン
    /// </summary>
    class POr : MPat
    {
        public MPat Pat1 { get; private set; }
        public MPat Pat2 { get; private set; }
        public MType Type { get; private set; }

        public POr(string pos, MPat pat1, MPat pat2)
            : base(pos)
        {
            Pat1 = pat1;
            Pat2 = pat2;
            Type = new TypeVar();
        }

        public override string ToString()
        {
            return string.Format("({0} | {1})", Pat1, Pat2);
        }

        public override MSet<string> FreeVars()
        {
            return Pat1.FreeVars().Union(Pat2.FreeVars());
        }
    }

    /// <summary>
    /// ユーザタグパターン
    /// </summary>
    class PUserTag : MPat
    {
        public string Name { get; private set; }
        public List<MPat> Args { get; private set; }
        public List<MType> Types { get; private set; }
        public int TagIndex { get; set; }

        public PUserTag(string pos, string name, List<MPat> args)
            : base(pos)
        {
            Name = name;
            Args = args;
            Types = args.Select(item => (MType)(new TypeVar())).ToList();
            TagIndex = 0;
        }

        public override string ToString()
        {
            return string.Format("({0}({1})", Name, Utils.Utils.ListToString(Args));
        }

        public override MSet<string> FreeVars()
        {
            var set = new MSet<string>();
            foreach (var pat in Args)
            {
                set = pat.FreeVars().Union(set);
            }
            return set;
        }
    }
}
