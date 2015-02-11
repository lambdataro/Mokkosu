using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using Mokkosu.ClosureConversion;
using Mokkosu.Utils;
using Mokkosu.AST;

namespace Mokkosu.CodeGenerate
{
    using LEnv = MEnv<LocalBuilder>;
    
    static class CodeGenerator
    {
        static Dictionary<string, MethodBuilder> _function_table;
        static string _runtime_dir = RuntimeEnvironment.GetRuntimeDirectory();

        public static AssemblyBuilder Start(string name, ClosureConversionResult cc_result)
        {
            _function_table = new Dictionary<string, MethodBuilder>();

            var assembly_name = new AssemblyName(name);
            var assembly_builder = AppDomain.CurrentDomain.DefineDynamicAssembly(
                assembly_name, AssemblyBuilderAccess.RunAndSave);
            var module_builder = assembly_builder.DefineDynamicModule(name, name + ".exe");

            var type_builder = module_builder.DefineType("MokkosuProgram");

            foreach (var f in cc_result.FunctionTable)
            {
                DeclareFunction(f.Key, type_builder);
            }
            DeclareFunction("MokkosuMain", type_builder);

            foreach (var f in cc_result.FunctionTable)
            {
                var il = _function_table[f.Key].GetILGenerator();
                Compile(il, f.Value, new LEnv());
            }
            Compile(_function_table["MokkosuMain"].GetILGenerator(),
                cc_result.Main, new LEnv());

            var builder = type_builder.DefineMethod("MokkosuEntryPoint",
                MethodAttributes.Static, typeof(void), new Type[] { });
            var ilgen = builder.GetILGenerator();
            ilgen.Emit(OpCodes.Ldnull);
            ilgen.Emit(OpCodes.Ldnull);
            ilgen.Emit(OpCodes.Call, _function_table["MokkosuMain"]);
            ilgen.Emit(OpCodes.Pop);
            ilgen.Emit(OpCodes.Ret);
        }

        static void DeclareFunction(string name, TypeBuilder type_builder)
        {
            var builder = type_builder.DefineMethod(name,
                MethodAttributes.Static, typeof(object),
                new Type[] { typeof(object), typeof(object[]) });
            _function_table.Add(name, builder);
        }

        static void Compile(ILGenerator il, MExpr expr, LEnv env)
        {

        }
    }
}
