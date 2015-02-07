using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace Mokkosu
{
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
                Compile(_function_table[f.Key].GetILGenerator(), f.Value);
            }
            Compile(_function_table["MokkosuMain"].GetILGenerator(), cc_result.Main);

            var builder = type_builder.DefineMethod("MokkosuEntryPoint",
                MethodAttributes.Static, typeof(void), new System.Type[] { });
            var il = builder.GetILGenerator();
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Call, _function_table["MokkosuMain"]);
            il.Emit(OpCodes.Pop);
            il.Emit(OpCodes.Ret);

            type_builder.CreateType();
            assembly_builder.SetEntryPoint(builder);
            return assembly_builder;
        }

        static void DeclareFunction(string name, TypeBuilder type_builder)
        {
            var builder = type_builder.DefineMethod(name,
                MethodAttributes.Static, typeof(object),
                new System.Type[] { typeof(object), typeof(object[]) });
            _function_table.Add(name, builder);
        }

        static void Compile(ILGenerator il, SExpr expr)
        {
            if (expr is SConstInt)
            {
                var e = (SConstInt)expr;
                il.Emit(OpCodes.Ldc_I4, e.Value);
            }
            else if (expr is SBinop)
            {
                var e = (SBinop)expr;
                Compile(il, e.Lhs);
                Compile(il, e.Rhs);
                if (e is SAdd)
                {
                    il.Emit(OpCodes.Add);
                }
                else if (e is SSub)
                {
                    il.Emit(OpCodes.Sub);
                }
                else if (e is SMul)
                {
                    il.Emit(OpCodes.Mul);
                }
                else if (e is SDiv)
                {
                    il.Emit(OpCodes.Div);
                }
                else if (e is SEq)
                {
                    var l1 = il.DefineLabel();
                    var l2 = il.DefineLabel();
                    il.Emit(OpCodes.Beq, l1);
                    il.Emit(OpCodes.Ldc_I4_0);
                    il.Emit(OpCodes.Br, l2);
                    il.MarkLabel(l1);
                    il.Emit(OpCodes.Ldc_I4_1);
                    il.MarkLabel(l2);
                }
            }
            else if (expr is SPrint)
            {
                var e = (SPrint)expr;
                Compile(il, e.Body);
                var mscorlib = Assembly.LoadFrom(Path.Combine(_runtime_dir, "mscorlib.dll"));
                var type = mscorlib.GetType("System.Console");
                var method = type.GetMethod("WriteLine", new System.Type[] { typeof(int) });
                il.Emit(OpCodes.Call, method);
                il.Emit(OpCodes.Ldc_I4_0);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
