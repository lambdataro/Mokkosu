using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace Mokkosu
{
    class CompileContext
    {
        public TypeBuilder TypeBuilder { get; private set; }
        public System.Type ValueRef {get; private set; }
        public FieldInfo ValueRefField { get; private set; }
        public System.Type TagClass { get; private set; }
        public FieldInfo TagName { get; private set; }
        public FieldInfo TagArgs { get; private set; }
        public FieldInfo TagCount { get; private set; }

        public CompileContext(TypeBuilder type_builder, 
            System.Type value_ref, FieldInfo value_ref_field,
            System.Type tag_class, FieldInfo tag_name, FieldInfo tag_args, FieldInfo tag_count)
        {
            TypeBuilder = type_builder;
            ValueRef = value_ref;
            ValueRefField = value_ref_field;
            TagClass = tag_class;
            TagName = tag_name;
            TagArgs = tag_args;
            TagCount = tag_count;
        }
    }

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

            // ValueRefクラス
            var value_ref = module_builder.DefineType("ValueRef");
            var value_ref_field = value_ref.DefineField("value",
                typeof(object), FieldAttributes.Public);
            var value_ref_type = value_ref.CreateType();

            // Tagクラス
            var tag_class = module_builder.DefineType("Tag");
            var tag_class_name = tag_class.DefineField("name",
                typeof(string), FieldAttributes.Public);
            var tag_class_args = tag_class.DefineField("args",
                typeof(object[]), FieldAttributes.Public);
            var tag_class_count = tag_class.DefineField("count",
                typeof(int), FieldAttributes.Public);
            var tag_class_type = tag_class.CreateType();

            // MokkosuProgramクラス
            var type_builder = module_builder.DefineType("MokkosuProgram");

            foreach (var f in cc_result.FunctionTable)
            {
                DeclareFunction(f.Key, type_builder);
            }
            DeclareFunction("MokkosuMain", type_builder);

            var ctx = new CompileContext(type_builder, value_ref_type, value_ref_field,
                tag_class_type, tag_class_name, tag_class_args, tag_class_count);

            foreach (var f in cc_result.FunctionTable)
            {
                var ilgen = _function_table[f.Key].GetILGenerator();
                Compile(ilgen, f.Value, Env<LocalBuilder>.Empty, ctx);
                ilgen.Emit(OpCodes.Ret);
            }
            Compile(_function_table["MokkosuMain"].GetILGenerator(),
                cc_result.Main, Env<LocalBuilder>.Empty, ctx);

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

        static void Compile(ILGenerator il, SExpr expr, Env<LocalBuilder> env, CompileContext ctx)
        {
            if (expr is SConstInt)
            {
                var e = (SConstInt)expr;
                il.Emit(OpCodes.Ldc_I4, e.Value);
                il.Emit(OpCodes.Box, typeof(int));
            }
            else if (expr is SBinop)
            {
                var e = (SBinop)expr;
                Compile(il, e.Lhs, env, ctx);
                il.Emit(OpCodes.Unbox_Any, typeof(int));
                Compile(il, e.Rhs, env, ctx);
                il.Emit(OpCodes.Unbox_Any, typeof(int));
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
                il.Emit(OpCodes.Box, typeof(int));
            }
            else if (expr is SVar)
            {
                var e = (SVar)expr;
                LocalBuilder loc;
                var b = Env<LocalBuilder>.Lookup(env, e.Name, out loc);
                if (b)
                {
                    il.Emit(OpCodes.Ldloc, loc);
                    var lbl1 = il.DefineLabel();
                    var lbl2 = il.DefineLabel();
                    il.MarkLabel(lbl1);
                    il.Emit(OpCodes.Dup);
                    il.Emit(OpCodes.Isinst, ctx.ValueRef);
                    il.Emit(OpCodes.Brfalse, lbl2);
                    il.Emit(OpCodes.Ldfld, ctx.ValueRefField);
                    il.Emit(OpCodes.Br, lbl1);
                    il.MarkLabel(lbl2);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            else if (expr is STag)
            {
                var e = (STag)expr;
                il.Emit(OpCodes.Newobj, ctx.TagClass.GetConstructor(new System.Type[] { }));
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Ldstr, e.Name);
                il.Emit(OpCodes.Stfld, ctx.TagName);
                il.Emit(OpCodes.Ldc_I4, e.ArgsCount);
                il.Emit(OpCodes.Newarr, typeof(object));
                il.Emit(OpCodes.Stfld, ctx.TagArgs);
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Stfld, ctx.TagCount);
            }
            else if (expr is SApp)
            {
                var e = (SApp)expr;
                Compile(il, e.FunExpr, env, ctx);
                var ary = il.DeclareLocal(typeof(object[]));
                il.Emit(OpCodes.Stloc, ary);
                Compile(il, e.ArgExpr, env, ctx);
                il.Emit(OpCodes.Ldloc, ary);
                il.Emit(OpCodes.Ldc_I4_1);
                il.Emit(OpCodes.Ldelem_Ref);
                il.Emit(OpCodes.Ldloc, ary);
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Ldelem_Ref);
                il.EmitCalli(OpCodes.Calli, 
                    CallingConventions.Standard, 
                    typeof(object), 
                    new System.Type[] {typeof(object), typeof(object[])}, 
                    null);
            }
            else if (expr is SLet)
            {
                var e = (SLet)expr;
                Compile(il, e.E1, env, ctx);
                var loc = il.DeclareLocal(typeof(object));
                il.Emit(OpCodes.Stloc, loc);
                Compile(il, e.E2, Env<LocalBuilder>.Cons(e.VarName, loc, env), ctx);
            }
            else if (expr is SRec)
            {
                var e = (SRec)expr;
                il.Emit(OpCodes.Newobj, ctx.ValueRef.GetConstructor(new System.Type[] {}));
                il.Emit(OpCodes.Dup);
                var loc = il.DeclareLocal(typeof(object));
                il.Emit(OpCodes.Stloc, loc);
                var env2 = Env<LocalBuilder>.Cons(e.VarName, loc, env);
                Compile(il, e.E1, env2, ctx);
                il.Emit(OpCodes.Stfld, ctx.ValueRefField);
                Compile(il, e.E2, env2, ctx);
            }
            else if (expr is SIf)
            {
                var e = (SIf)expr;
                Compile(il, e.CondExpr, env, ctx);
                var lbl1 = il.DefineLabel();
                var lbl2 = il.DefineLabel();
                il.Emit(OpCodes.Unbox_Any, typeof(int));
                il.Emit(OpCodes.Brfalse, lbl1);
                Compile(il, e.ThenExpr, env, ctx);
                il.Emit(OpCodes.Br, lbl2);
                il.MarkLabel(lbl1);
                Compile(il, e.ElseExpr, env, ctx);
                il.MarkLabel(lbl2);
            }
            else if (expr is SPrint)
            {
                var e = (SPrint)expr;
                Compile(il, e.Body, env, ctx);
                il.Emit(OpCodes.Unbox_Any, typeof(int));
                var mscorlib = Assembly.LoadFrom(Path.Combine(_runtime_dir, "mscorlib.dll"));
                var type = mscorlib.GetType("System.Console");
                var method = type.GetMethod("WriteLine", new System.Type[] { typeof(int) });
                il.Emit(OpCodes.Call, method);
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Box, typeof(int));
            }
            else if (expr is SGetArg)
            {
                il.Emit(OpCodes.Ldarg_0);
                // deref
                var lbl1 = il.DefineLabel();
                var lbl2 = il.DefineLabel();
                il.MarkLabel(lbl1);
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Isinst, ctx.ValueRef);
                il.Emit(OpCodes.Brfalse, lbl2);
                il.Emit(OpCodes.Ldfld, ctx.ValueRefField);
                il.Emit(OpCodes.Br, lbl1);
                il.MarkLabel(lbl2);
            }
            else if (expr is SGetEnv)
            {
                var e = (SGetEnv)expr;
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldc_I4, e.Index);
                il.Emit(OpCodes.Ldelem_Ref);
                // deref
                var lbl1 = il.DefineLabel();
                var lbl2 = il.DefineLabel();
                il.MarkLabel(lbl1);
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Isinst, ctx.ValueRef);
                il.Emit(OpCodes.Brfalse, lbl2);
                il.Emit(OpCodes.Ldfld, ctx.ValueRefField);
                il.Emit(OpCodes.Br, lbl1);
                il.MarkLabel(lbl2);
            }
            else if (expr is SMakeClos)
            {
                var e = (SMakeClos)expr;
                // 2要素の配列を作る
                il.Emit(OpCodes.Ldc_I4_2);
                il.Emit(OpCodes.Newarr, typeof(object));
                var arr1 = il.DeclareLocal(typeof(object[]));
                il.Emit(OpCodes.Stloc, arr1);
                // 1要素目に関数へのポインタを代入
                il.Emit(OpCodes.Ldloc, arr1);
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Ldftn, _function_table[e.ClosName]);
                il.Emit(OpCodes.Stelem_Ref);
                // キャプチャされた値用の配列を作成
                il.Emit(OpCodes.Ldc_I4, e.Args.Length);
                il.Emit(OpCodes.Newarr, typeof(object));
                var arr2 = il.DeclareLocal(typeof(object[]));
                il.Emit(OpCodes.Stloc, arr2);
                // キャプチャされた値を配列に代入
                for (int i = 0; i < e.Args.Length; i++)
                {
                    il.Emit(OpCodes.Ldloc, arr2);
                    il.Emit(OpCodes.Ldc_I4, i);
                    Compile(il, e.Args[i], env, ctx);
                    il.Emit(OpCodes.Stelem_Ref);
                }
                // できた配列を最初の配列の2要素目に代入
                il.Emit(OpCodes.Ldloc, arr1);
                il.Emit(OpCodes.Ldc_I4_1);
                il.Emit(OpCodes.Ldloc, arr2);
                il.Emit(OpCodes.Stelem_Ref);
                // 2要素の配列をロード
                il.Emit(OpCodes.Ldloc, arr1);
            }
            else if (expr is SVarClos)
            {
                var e = (SVarClos)expr;
                LocalBuilder loc;
                var b = Env<LocalBuilder>.Lookup(env, e.Name, out loc);
                if (b)
                {
                    il.Emit(OpCodes.Ldloc, loc);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
