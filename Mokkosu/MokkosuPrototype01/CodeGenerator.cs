using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Mokkosu
{
    static class CodeGenerator
    {
        static Dictionary<string, MethodBuilder> _function_table;

        public static void Start(string name, ClosureConversionResult cc_result)
        {
            var assembly_name = new AssemblyName(name);
            var assembly_builder = AppDomain.CurrentDomain.DefineDynamicAssembly(
                assembly_name, AssemblyBuilderAccess.RunAndSave);
            var module_builder = assembly_builder.DefineDynamicModule(name, name + ".exe");
            var type_builder = module_builder.DefineType("Program");

            foreach (var f in cc_result.FunctionTable)
            {
                DeclareFunction(f.Key, type_builder);
            }
            DeclareFunction("Main", type_builder);

            foreach (var f in cc_result.FunctionTable)
            {
                CompileFunction(_function_table[f.Key].GetILGenerator(), f.Value);
            }

        }

        static void DeclareFunction(string name, TypeBuilder type_builder)
        {
            var builder = type_builder.DefineMethod(name,
                MethodAttributes.Static, typeof(object),
                new System.Type[] { typeof(object), typeof(object[]) });
            _function_table.Add(name, builder);
        }

        static void CompileFunction(ILGenerator il, SExpr expr)
        {
            
        }
    }
}
