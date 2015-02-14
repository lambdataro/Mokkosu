using Mokkosu.AST;
using Mokkosu.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mokkosu.TypeInference
{
    static class ShowType
    {
        public static void ShowTEnv(MEnv<MTypeScheme> tenv)
        {
            if (tenv.IsEmpty())
            {
                return;
            }
            else
            {
                ShowTEnv(tenv.Tail);
                var str = ShowTypeScheme(tenv.Head.Item1, tenv.Head.Item2);
                Console.WriteLine("{0} : {1}", tenv.Head.Item1, str);
            }
        }

        static string ShowTypeScheme(string name, MTypeScheme typescheme)
        {
            var bounded = typescheme.Bounded;
            var dict = NameTypeVar(bounded.ToArray());
            return Show(name, typescheme.Type, dict, false);
        }

        static Dictionary<int, string> NameTypeVar(int[] bounded)
        {
            var dict = new Dictionary<int, string>();

            for (var i = 0; i < bounded.Length; i++)
            {
                dict.Add(bounded[i], GetTypeVarName(i));
            }

            return dict;
        }

        static string[] greek = new string[] 
        {
            "α", "β", "γ", "δ", "ε", "ζ", "η", "θ",
            "ι", "κ", "λ", "μ", "ν", "ξ", "ο", "π",
            "ρ", "σ", "τ", "υ", "φ", "χ", "ψ", "ω"
        };

        static string GetTypeVarName(int n)
        {
            var mod = n % 24;
            var div = n / 24;

            if (div == 0)
            {
                return greek[mod];
            }
            else
            {
                return string.Format("{0}{1}", greek[mod], div);
            }
        }

        static string Show(string name, MType type, Dictionary<int, string> typevars, bool parens)
        {
            if (type is TypeVar)
            {
                var t = (TypeVar)type;
                if (t.Value == null)
                {
                    if (typevars.ContainsKey(t.Id))
                    {
                        return typevars[t.Id];
                    }
                    else
                    {
                        throw new MError(name + "の型が定まらない");
                    }
                }
                else
                {
                    return Show(name, t.Value, typevars, parens);
                }
            }
            else if (type is UserType)
            {
                var t = (UserType)type;
                if (t.Args.Count == 0)
                {
                    return t.Name;
                }
                else
                {
                    return string.Format("{0}<{1}>", t.Name, 
                        ShowTypeList(name, t.Args, typevars));
                }
            }
            else if (type is IntType)
            {
                return "Int";
            }
            else if (type is DoubleType)
            {
                return "Double";
            }
            else if (type is StringType)
            {
                return "String";
            }
            else if (type is CharType)
            {
                return "Char";
            }
            else if (type is UnitType)
            {
                return "()";
            }
            else if (type is BoolType)
            {
                return "Bool";
            }
            else if (type is FunType)
            {
                var t = (FunType)type;
                if (parens)
                {
                    return string.Format("({0} -> {1})",
                        Show(name, t.ArgType, typevars, true),
                        Show(name, t.RetType, typevars, false));
                }
                else
                {
                    return string.Format("{0} -> {1}",
                        Show(name, t.ArgType, typevars, true),
                        Show(name, t.RetType, typevars, false));
                }
            }
            else if (type is ListType)
            {
                var t = (ListType)type;
                return string.Format("[{1}]", Show(name, t.ElemType, typevars, false));
            }
            else if (type is RefType)
            {
                var t = (RefType)type;
                return string.Format("Ref<{0}>", Show(name, t.ElemType, typevars, false));
            }
            else if (type is TupleType)
            {
                var t = (TupleType)type;
                return string.Format("({0})", ShowTypeList(name, t.Types, typevars));
            }
            else if (type is DotNetType)
            {
                var t = (DotNetType)type;
                return string.Format("{{0}}", t.Type.ToString());
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        static string ShowTypeList(string name, List<MType> types, Dictionary<int, string> typevars)
        {
            if (types == null || types.Count == 0)
            {
                return "";
            }
            else
            {
                var sb = new StringBuilder();
                sb.Append(Show(name, types.First(), typevars, false));
                foreach (var item in types.Skip(1))
                {
                    sb.AppendFormat(", {0}", Show(name, item, typevars, false));
                }
                return sb.ToString();
            }
        }
    }
}
