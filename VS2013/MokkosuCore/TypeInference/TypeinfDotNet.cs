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
        static LinkedList<string> _using_list = new LinkedList<string>();
        static string _runtime_dir = RuntimeEnvironment.GetRuntimeDirectory();
        static string _exe_dir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

        public static void AddAssembly(string name)
        {
            var path1 = Path.Combine(_exe_dir, name);
            var path2 = Path.Combine(_runtime_dir, name);
            if (File.Exists(path1))
            {
                var asm = Assembly.LoadFrom(path1);
                _assembly_list.AddFirst(asm);
            }
            else if (File.Exists(path2))
            {
                var asm = Assembly.LoadFrom(path2);
                _assembly_list.AddFirst(asm);
            }
            else
            {
                throw new MError("DLL" + name + "が見つからない。");
            }
        }

        public static void AddUsing(string name)
        {
            _using_list.AddFirst(name);
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
                foreach (var sp in _using_list)
                {
                    var name = sp + "." + class_name;
                    type = asm.GetType(name);
                    if (type != null)
                    {
                        break;
                    }
                }
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
            if (method == null)
            {
                throw new MError(pos + ": メソッド" + class_name + "::" + method_name + "が見つかりません。");
            }
            return method;
        }

        public static ConstructorInfo LookupConstructor(string pos, string class_name, List<MType> arg_types)
        {
            var t = LookupDotNetClass(pos, class_name);
            var ts = arg_types.Select(typ => MokkosuTypeToDotNetType(pos, typ)).ToArray();
            var constructor = t.GetConstructor(ts);
            if (constructor == null)
            {
                throw new MError(pos + ": コンストラクタ" + class_name +　"が見つかりません。");
            }
            return constructor;
        }

        public static MethodInfo LookupInstanceMethod(string pos, MType object_type, string method_name, List<MType> arg_types)
        {
            var t = MokkosuTypeToDotNetType(pos, object_type);
            var ts = arg_types.Select(typ => MokkosuTypeToDotNetType(pos, typ)).ToArray();
            var method = t.GetMethod(method_name, ts);
            if (method == null)
            {
                throw new MError(pos + ": メソッド" + method_name + "が見つかりません。");
            }
            return method;
        }

        public static FieldInfo LookupField(string pos, MType object_type, string field_name)
        {
            var t = MokkosuTypeToDotNetType(pos, object_type);
            var field = t.GetField(field_name);
            if (field == null)
            {
                throw new MError(pos + ": フィールド" + field_name + "が見つかりません。");
            }
            return field;
        }

        public static Type MokkosuTypeToDotNetType(string pos, MType mtype)
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
