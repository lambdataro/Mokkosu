using Mokkosu.Utils;
using System;
using System.Collections.Generic;
using System.Text;

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

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("type:\n");
            Items.ForEach(item => { sb.Append(item); sb.Append("\n"); });
            sb.Append("end type.");
            return sb.ToString();
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

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(Name);
            sb.Append("<");
            sb.Append(Utils.Utils.ListToString(TypeParams));
            sb.Append("> = ");
            Tags.ForEach(def => sb.Append(def));
            return sb.ToString();
        }
    }

    class TagDef
    {
        public string Name { get; private set; }
        public List<MType> Args { get; private set; }

        public TagDef(string name, List<MType> args)
        {
            Name = name;
            Args = args;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(" | ");
            sb.Append(Name);
            sb.Append("(");
            sb.Append(Utils.Utils.ListToString(Args));
            sb.Append(")");
            return sb.ToString();
        }
    }

    class MTopDo : MTopExpr
    {
        public MExpr Expr { get; private set; }
        public MType Type { get; private set; }

        public MTopDo(MExpr expr)
        {
            Expr = expr;
            Type = new TypeVar();
        }

        public override string ToString()
        {
            return string.Format("do {0};", Expr);
        }
    }
}
