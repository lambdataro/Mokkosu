using Mokkosu.Utils;
using System.Collections.Generic;
using System.Linq;

namespace Mokkosu.AST
{
    class MTypeScheme
    {
        public MSet<int> Bounded { get; private set; }
        public MType Type { get; private set; }

        public MTypeScheme(MType type)
        {
            Bounded = new MSet<int>();
            Type = type;
        }

        public MTypeScheme(IEnumerable<int> bounded, TypeVar type)
        {
            Bounded = new MSet<int>(bounded);
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
                return string.Format("∀{0}. {1}", 
                    Utils.Utils.ListToString(Bounded.ToArray().ToList()), Type);
            }
        }
    }
}
