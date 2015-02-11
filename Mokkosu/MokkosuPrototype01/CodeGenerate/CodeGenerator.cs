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

            type_builder.CreateType();
            assembly_builder.SetEntryPoint(builder);
            return assembly_builder;
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
            if (expr is MInt)
            {
                var e = (MInt)expr;
                il.Emit(OpCodes.Ldc_I4, e.Value);
                il.Emit(OpCodes.Box, typeof(int));
            }
            else if (expr is MString)
            {
                var e = (MString)expr;
                il.Emit(OpCodes.Ldstr, e.Value);
            }
            else if (expr is MUnit)
            {
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Box, typeof(int));
            }
            else if (expr is MDo)
            {
                var e = (MDo)expr;
                Compile(il, e.E1, env);
                il.Emit(OpCodes.Pop);
                Compile(il, e.E2, env);
            }
            else if (expr is MPrim)
            {
                var e = (MPrim)expr;
                foreach (var arg in e.Args)
                {
                    Compile(il, arg, env);
                }
                CompilePrim(il, e.Name, e.ArgTypes, e.RetType);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        static void CompilePrim(ILGenerator il, string name, List<MType> arg_types, MType ret_type)
        {
            switch (name)
            {
                case "println":
                    il.Emit(OpCodes.Call, 
                        SystemMethod("mscorlib.dll", "System.Console", "WriteLine",
                        new Type[] { typeof(object) }));
                    il.Emit(OpCodes.Ldc_I4_0);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        static MethodInfo SystemMethod(string assembly, string type_name, string method_name, Type[] arg_types)
        {
            var dll = Assembly.LoadFrom(Path.Combine(_runtime_dir, assembly));
            var type = dll.GetType(type_name);
            var method = type.GetMethod(method_name, arg_types);
            return method;
        }
    }
}
