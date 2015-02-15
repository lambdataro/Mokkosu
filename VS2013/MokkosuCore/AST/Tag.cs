using Mokkosu.Utils;
using System.Collections.Generic;

namespace Mokkosu.AST
{
    /// <summary>
    /// タグ情報
    /// </summary>
    class Tag
    {
        public string Name { get; private set; }
        public int Index { get; private set; }
        public MSet<int> Bounded { get; private set; }
        public List<MType> ArgTypes { get; private set; }
        public MType Type { get; private set; }

        public Tag(string name, int index, MSet<int> bounded, List<MType> arg_types, MType type)
        {
            Name = name;
            Index = index;
            Bounded = bounded;
            ArgTypes = arg_types;
            Type = type;
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
