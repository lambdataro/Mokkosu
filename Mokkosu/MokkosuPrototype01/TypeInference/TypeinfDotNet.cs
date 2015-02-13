using Mokkosu.AST;
using Mokkosu.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Mokkosu.TypeInference
{
    static class TypeinfDotNet
    {
        static LinkedList<Assembly> _assembly_list = new LinkedList<Assembly>();
        static string _runtime_dir = RuntimeEnvironment.GetRuntimeDirectory();

        static TypeinfDotNet()
        {
            AddAssembly("mscorlib.dll");
            AddAssembly("System.dll");
            AddAssembly("System.Core.dll");
        }

        public static void AddAssembly(string name)
        {
            var asm = Assembly.LoadFrom(Path.Combine(_runtime_dir, name));
            _assembly_list.AddFirst(asm);
        }

        public static Type LookupDotNetClass(string pos, string class_name)
        {
            Type type = null;
            foreach (var asm in _assembly_list)
            {
                type = asm.GetType(class_name);
                if (type != null)
                {
                    break;
                }
            }
            if (type == null)
            {
                throw new MError(pos + ": クラス" + class_name + "が見つかりません。");
            }
            return type;
        }

        public static MethodInfo LookupStaticMethod(string pos, string class_name, string method_name, List<MType> arg_types)
        {
            var t = LookupDotNetClass(pos, class_name);
            var ts = arg_types.Select(typ => MokkosuTypeToDotNetType(pos, typ)).ToArray();
            var method = t.GetMethod(method_name, ts);
            return method;
        }

        static Type MokkosuTypeToDotNetType(string pos, MType mtype)
        {
            if (mtype is TypeVar)
            {
                var t = (TypeVar)mtype;
                if (t.Value == null)
                {
                    throw new MError(pos + ": 引数の型を決定できない");
                }
                else
                {
                    return MokkosuTypeToDotNetType(pos, t.Value);
                }
            }
            else if (mtype is IntType)
            {
                return typeof(int);
            }
            else if (mtype is DoubleType)
            {
                return typeof(double);
            }
            else if (mtype is StringType)
            {
                return typeof(string);
            }
            else if (mtype is CharType)
            {
                return typeof(char);
            }
            else if (mtype is BoolType)
            {
                return typeof(bool);
            }
            else if (mtype is DotNetType)
            {
                var t = (DotNetType)mtype;
                return t.Type;
            }
            else
            {
                throw new MError(pos + ": Mokkosuの型を.NETの型に変換できない");
            }
        }

        public static MType DotNetTypeToMokkosuType(Type type)
        {
            if (type == typeof(int))
            {
                return new IntType();
            }
            else if (type == typeof(double))
            {
                return new DoubleType();
            }
            else if (type == typeof(string))
            {
                return new StringType();
            }
            else if (type == typeof(char))
            {
                return new CharType();
            }
            else if (type == typeof(bool))
            {
                return new BoolType();
            }
            else
            {
                return new DotNetType(type);
            }
        }


    }
}
