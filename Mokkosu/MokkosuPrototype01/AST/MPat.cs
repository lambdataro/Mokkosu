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
    }

    /// <summary>
    /// 変数パターン
    /// </summary>
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

        public PWild()
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
    /// 整数パターン
    /// </summary>
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

        public PDouble(double value)
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

        public PString(string value)
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

        public PChar(char value)
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

        public PNil()
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

        public PCons(MPat head, MPat tail)
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

        public PTuple(List<MPat> items)
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

        public PAs(MPat pat, string name)
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

        public POr(MPat pat1, MPat pat2)
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
}
