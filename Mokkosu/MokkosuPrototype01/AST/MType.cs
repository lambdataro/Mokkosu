namespace Mokkosu.AST
{
    /// <summary>
    /// 型の抽象クラス
    /// </summary>
    abstract class MType
    {
    }

    /// <summary>
    /// 型変数
    /// </summary>
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

    /// <summary>
    /// ユーザ定義型
    /// </summary>
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

    /// <summary>
    /// 整数型
    /// </summary>
    class IntType : MType
    {
        public override string ToString()
        {
            return "Int";
        }
    }

    /// <summary>
    /// 倍精度浮動小数点型
    /// </summary>
    class DoubleType : MType
    {
        public override string ToString()
        {
            return "Double";
        }
    }

    /// <summary>
    /// 文字列型
    /// </summary>
    class StringType : MType
    {
        public override string ToString()
        {
            return "String";
        }
    }

    /// <summary>
    /// 文字型
    /// </summary>
    class CharType : MType
    {
        public override string ToString()
        {
            return "Char";
        }
    }

    /// <summary>
    /// ユニット型
    /// </summary>
    class UnitType : MType
    {
        public override string ToString()
        {
            return "()";
        }
    }
}
