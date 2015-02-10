using Mokkosu.Utils;
using System.Collections.Generic;
using System.Linq;

namespace Mokkosu.AST
{
    class MTypeScheme
    {
        public MSet<int> Bounded { get; private set; }
        public MType Type { get; private set; }

        public bool IsTag { get; set; }
        public int TagIndex { get; set; }
        public int TagSize { get; set; }

        public MTypeScheme(MType type)
        {
            Bounded = new MSet<int>();
            Type = type;
            IsTag = false;
            TagIndex = 0;
            TagSize = 0;
        }

        public MTypeScheme(IEnumerable<int> bounded, MType type)
        {
            Bounded = new MSet<int>(bounded);
            Type = type;
            IsTag = false;
            TagIndex = 0;
            TagSize = 0;
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
