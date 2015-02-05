using System.Collections.Generic;
using System.Text;

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

    /// <summary>
    /// 型スキーム
    /// </summary>
    class TypeScheme
    {
        public ImmutableHashSet<int> Bounded { get; private set; }
        public Type Type { get; private set; }

        public TypeScheme(IEnumerable<int> bounded, Type type)
        {
            Bounded = new ImmutableHashSet<int>(bounded);
            Type = type;
        }

        public override string ToString()
        {
            if (Bounded.Count == 0)
            {
                return Type.ToString();
            }
            else
            {
                return string.Format("∀{0}.{1}", BoundedToString(), Type);
            }
        }

        string BoundedToString()
        {
            var sb = new StringBuilder();
            foreach (var id in Bounded.ToArray())
            {
                sb.Append(id);
                sb.Append(", ");
            }
            return sb.ToString();
        }
    }
}
