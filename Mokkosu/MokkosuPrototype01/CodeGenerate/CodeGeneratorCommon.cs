using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Mokkosu.CodeGenerate
{
    static class CodeGeneratorCommon
    {
        static string _runtime_dir = RuntimeEnvironment.GetRuntimeDirectory();

        public static MethodInfo SystemMethod(string assembly, string type_name, string method_name, Type[] arg_types)
        {
            var dll = Assembly.LoadFrom(Path.Combine(_runtime_dir, assembly));
            var type = dll.GetType(type_name);
            var method = type.GetMethod(method_name, arg_types);
            return method;
        }

        public static ConstructorInfo SystemConstructor(string assembly, string type_name, Type[] arg_types)
        {
            var dll = Assembly.LoadFrom(Path.Combine(_runtime_dir, assembly));
            var type = dll.GetType(type_name);
            var constructor = type.GetConstructor(arg_types);
            return constructor;
        }
    }
}
