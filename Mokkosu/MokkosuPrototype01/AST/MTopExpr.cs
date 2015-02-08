using System;
using System.Collections.Generic;
namespace Mokkosu.AST
{
    abstract class MTopExpr
    {
    }

    class MUserTypeDef : MTopExpr
    {
        public List<MUserTypeDefItem> Items { get; private set; }
        
        public MUserTypeDef(List<MUserTypeDefItem> items)
        {
            Items = items;
        }
    }

    class MUserTypeDefItem
    {
        public string Name { get; private set; }
        public List<string> TypeParams { get; private set; }
        public List<TagDef> Tags { get; private set; }

        public MUserTypeDefItem(string name, List<string> type_params, List<TagDef> tags)
        {
            Name = name;
            TypeParams = type_params;
            Tags = tags;
        }
    }

    class TagDef
    {
        public string Name { get; private set; }
        public List<Tuple<string, MType>> Args { get; private set; }

        public TagDef(string name, List<Tuple<string, MType>> args)
        {
            Name = name;
            Args = args;
        }
    }
}
