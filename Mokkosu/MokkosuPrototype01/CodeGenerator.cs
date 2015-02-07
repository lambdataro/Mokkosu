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
                Compile(_function_table[f.Key].GetILGenerator(), f.Value);
            }

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
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
