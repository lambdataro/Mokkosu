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

        static FieldInfo _value_ref_field;
        static Type _value_ref_type;

        static FieldInfo _tag_id;
        static FieldInfo _tag_args;
        static Type _tag_type;

        static MethodInfo _string_equal =
            SystemMethod("mscorlib.dll", "System.String", "Equals", 
                new Type[] { typeof(string), typeof(string) });

        static ConstructorInfo _application_exception =
            SystemConstructor("mscorlib.dll", "System.ApplicationException", new Type[] { typeof(string) });

        static MethodBuilder _add;
        static MethodBuilder _sub;
        static MethodBuilder _mul;
        static MethodBuilder _div;


        public static AssemblyBuilder Start(string name, ClosureConversionResult cc_result)
        {
            _function_table = new Dictionary<string, MethodBuilder>();

            var assembly_name = new AssemblyName(name);
            var assembly_builder = AppDomain.CurrentDomain.DefineDynamicAssembly(
                assembly_name, AssemblyBuilderAccess.RunAndSave);
            var module_builder = assembly_builder.DefineDynamicModule(name, name + ".exe");

            // ValueRef クラス
            var value_ref = module_builder.DefineType("ValueRef");
            _value_ref_field = value_ref.DefineField("value",
                typeof(object), FieldAttributes.Public);
            _value_ref_type = value_ref.CreateType();

            // Tag クラス
            var tag = module_builder.DefineType("Tag");
            _tag_id = tag.DefineField("id", typeof(int), FieldAttributes.Public);
            _tag_args = tag.DefineField("args", typeof(object[]), FieldAttributes.Public);
            _tag_type = tag.CreateType();

            // MokkosuProgram クラス
            var type_builder = module_builder.DefineType("MokkosuProgram");

            foreach (var f in cc_result.FunctionTable)
            {
                DeclareFunction(f.Key, type_builder);
            }
            DeclareFunction("MokkosuMain", type_builder);

            DefinePrimFun(type_builder);

            foreach (var f in cc_result.FunctionTable)
            {
                var il = _function_table[f.Key].GetILGenerator();
                Compile(il, f.Value, new LEnv());
                il.Emit(OpCodes.Ret);
            }
            Compile(_function_table["MokkosuMain"].GetILGenerator(),
                cc_result.Main, new LEnv());

            // MokkosuEntryPoint
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

        static void DefinePrimFun(TypeBuilder type_builder)
        {
            DefineAdd(type_builder);
            DefineSub(type_builder);
            DefineMul(type_builder);
            DefineDiv(type_builder);

        }

        private static void DefineAdd(TypeBuilder type_builder)
        {
            _add = type_builder.DefineMethod("add",
                MethodAttributes.Static, typeof(object),
                new Type[] { typeof(object), typeof(object) });
            var il = _add.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Unbox_Any, typeof(int));
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Unbox_Any, typeof(int));
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Box, typeof(int));
            il.Emit(OpCodes.Ret);
        }

        private static void DefineSub(TypeBuilder type_builder)
        {
            _sub = type_builder.DefineMethod("sub",
                MethodAttributes.Static, typeof(object),
                new Type[] { typeof(object), typeof(object) });
            var il = _add.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Unbox_Any, typeof(int));
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Unbox_Any, typeof(int));
            il.Emit(OpCodes.Sub);
            il.Emit(OpCodes.Box, typeof(int));
            il.Emit(OpCodes.Ret);
        }

        private static void DefineMul(TypeBuilder type_builder)
        {
            _mul = type_builder.DefineMethod("mul",
                MethodAttributes.Static, typeof(object),
                new Type[] { typeof(object), typeof(object) });
            var il = _add.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Unbox_Any, typeof(int));
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Unbox_Any, typeof(int));
            il.Emit(OpCodes.Mul);
            il.Emit(OpCodes.Box, typeof(int));
            il.Emit(OpCodes.Ret);
        }

        private static void DefineDiv(TypeBuilder type_builder)
        {
            _div = type_builder.DefineMethod("div",
                MethodAttributes.Static, typeof(object),
                new Type[] { typeof(object), typeof(object) });
            var il = _add.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Unbox_Any, typeof(int));
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Unbox_Any, typeof(int));
            il.Emit(OpCodes.Div);
            il.Emit(OpCodes.Box, typeof(int));
            il.Emit(OpCodes.Ret);
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
            else if (expr is MDouble)
            {
                var e = (MDouble)expr;
                il.Emit(OpCodes.Ldc_R8, e.Value);
                il.Emit(OpCodes.Box, typeof(double));
            }
            else if (expr is MString)
            {
                var e = (MString)expr;
                il.Emit(OpCodes.Ldstr, e.Value);
            }
            else if (expr is MChar)
            {
                var e = (MChar)expr;
                il.Emit(OpCodes.Ldc_I4, e.Value);
                il.Emit(OpCodes.Box, typeof(char));
            }
            else if (expr is MUnit)
            {
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Box, typeof(int));
            }
            else if (expr is MBool)
            {
                var e = (MBool)expr;
                if (e.Value)
                {
                    il.Emit(OpCodes.Ldc_I4_1);
                }
                else
                {
                    il.Emit(OpCodes.Ldc_I4_0);
                }
                il.Emit(OpCodes.Box, typeof(int));
            }
            else if (expr is MVar)
            {
                var e = (MVar)expr;
                if (e.IsTag)
                {
                    il.Emit(OpCodes.Newobj, _tag_type.GetConstructor(new Type[] { }));
                    il.Emit(OpCodes.Dup);
                    il.Emit(OpCodes.Dup);
                    il.Emit(OpCodes.Ldc_I4, e.TagIndex);
                    il.Emit(OpCodes.Stfld, _tag_id);
                    il.Emit(OpCodes.Ldnull);
                    il.Emit(OpCodes.Stfld, _tag_args);
                }
                else
                {
                    var loc = env.Lookup(e.Name);
                    il.Emit(OpCodes.Ldloc, loc);
                    var lbl1 = il.DefineLabel();
                    var lbl2 = il.DefineLabel();
                    il.MarkLabel(lbl1);
                    il.Emit(OpCodes.Dup);
                    il.Emit(OpCodes.Isinst, _value_ref_type);
                    il.Emit(OpCodes.Brfalse, lbl2);
                    il.Emit(OpCodes.Ldfld, _value_ref_field);
                    il.Emit(OpCodes.Br, lbl1);
                    il.MarkLabel(lbl2);
                }
            }
            else if (expr is MApp)
            {
                var e = (MApp)expr;
                Compile(il, e.FunExpr, env);
                var ary = il.DeclareLocal(typeof(object[]));
                il.Emit(OpCodes.Stloc, ary);
                Compile(il, e.ArgExpr, env);
                il.Emit(OpCodes.Ldloc, ary);
                il.Emit(OpCodes.Ldc_I4_1);
                il.Emit(OpCodes.Ldelem_Ref);
                il.Emit(OpCodes.Ldloc, ary);
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Ldelem_Ref);
                il.EmitCalli(OpCodes.Calli,
                    CallingConventions.Standard,
                    typeof(object),
                    new Type[] { typeof(object), typeof(object[]) },
                    null);
            }
            else if (expr is MIf)
            {
                var e = (MIf)expr;
                Compile(il, e.CondExpr, env);
                var lbl1 = il.DefineLabel();
                var lbl2 = il.DefineLabel();
                il.Emit(OpCodes.Unbox_Any, typeof(int));
                il.Emit(OpCodes.Brfalse, lbl1);
                Compile(il, e.ThenExpr, env);
                il.Emit(OpCodes.Br, lbl2);
                il.MarkLabel(lbl1);
                Compile(il, e.ElseExpr, env);
                il.MarkLabel(lbl2);
            }
            else if (expr is MMatch)
            {
                var e = (MMatch)expr;
                var fail_label = il.DefineLabel();
                var lbl = il.DefineLabel();
                Compile(il, e.Expr, env);
                var env2 = CompilePat(il, e.Pat, fail_label);

                Compile(il, e.ThenExpr, env2.Append(env));
                il.Emit(OpCodes.Br, lbl);
                il.MarkLabel(fail_label);
                Compile(il, e.ElseExpr, env);
                il.MarkLabel(lbl);
            }
            else if (expr is MNil)
            {
                il.Emit(OpCodes.Newobj, _tag_type.GetConstructor(new Type[] { }));
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Stfld, _tag_id);
                il.Emit(OpCodes.Ldnull);
                il.Emit(OpCodes.Stfld, _tag_args);
            }
            else if (expr is MCons)
            {
                var e = (MCons)expr;

                il.Emit(OpCodes.Newobj, _tag_type.GetConstructor(new Type[] { }));
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Dup);

                il.Emit(OpCodes.Ldc_I4_1);
                il.Emit(OpCodes.Stfld, _tag_id);
                
                var arr = il.DeclareLocal(typeof(object[]));
                il.Emit(OpCodes.Ldc_I4_2);
                il.Emit(OpCodes.Newarr, typeof(object));
                il.Emit(OpCodes.Stloc, arr);

                il.Emit(OpCodes.Ldloc, arr);
                il.Emit(OpCodes.Ldc_I4_0);
                Compile(il, e.Head, env);
                il.Emit(OpCodes.Stelem_Ref);

                il.Emit(OpCodes.Ldloc, arr);
                il.Emit(OpCodes.Ldc_I4_1);
                Compile(il, e.Tail, env);
                il.Emit(OpCodes.Stelem_Ref);

                il.Emit(OpCodes.Ldloc, arr);
                il.Emit(OpCodes.Stfld, _tag_args);
            }
            else if (expr is MTuple)
            {
                var e = (MTuple)expr;
                il.Emit(OpCodes.Ldc_I4, e.Size);
                il.Emit(OpCodes.Newarr, typeof(object));
                var arr = il.DeclareLocal(typeof(object[]));
                il.Emit(OpCodes.Stloc, arr);
                for (var i = 0; i < e.Size; i++)
                {
                    il.Emit(OpCodes.Ldloc, arr);
                    il.Emit(OpCodes.Ldc_I4, i);
                    Compile(il, e.Items[i], env);
                    il.Emit(OpCodes.Stelem_Ref);
                }
                il.Emit(OpCodes.Ldloc, arr);
            }
            else if (expr is MDo)
            {
                var e = (MDo)expr;
                Compile(il, e.E1, env);
                il.Emit(OpCodes.Pop);
                Compile(il, e.E2, env);
            }
            else if (expr is MLet)
            {
                var e = (MLet)expr;
                var fail_label = il.DefineLabel();
                var lbl = il.DefineLabel();
                Compile(il, e.E1, env);
                var env2 = CompilePat(il, e.Pat, fail_label);

                il.Emit(OpCodes.Br, lbl);
                il.MarkLabel(fail_label);
                il.Emit(OpCodes.Ldstr, e.Pos + ": パターンマッチ失敗");
                il.Emit(OpCodes.Newobj, _application_exception);
                il.Emit(OpCodes.Throw);

                il.MarkLabel(lbl);
                Compile(il, e.E2, env2.Append(env));
            }
            else if (expr is MFun)
            {
                var e = (MFun)expr;
                var locals = e.Items.Select(item => il.DeclareLocal(typeof(object))).ToArray();
                var env2 = env;
                for (var i = 0; i < e.Items.Count; i++)
                {
                    env2 = env2.Cons(e.Items[i].Name, locals[i]);
                }
                for (var i = 0; i < locals.Length; i++)
                {
                    il.Emit(OpCodes.Newobj, _value_ref_type.GetConstructor(new Type[] { }));
                    il.Emit(OpCodes.Dup);
                    il.Emit(OpCodes.Stloc, locals[i]);
                    Compile(il, e.Items[i].Expr, env2);
                    il.Emit(OpCodes.Stfld, _value_ref_field);
                }
                Compile(il, e.E2, env2);
            }
            else if (expr is RuntimeError)
            {
                var e = (RuntimeError)expr;
                il.Emit(OpCodes.Ldstr, e.Pos + ": " + e.Message);
                il.Emit(OpCodes.Newobj, _application_exception);
                il.Emit(OpCodes.Throw);
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
            else if (expr is MGetArg)
            {
                il.Emit(OpCodes.Ldarg_0);
                // deref
                var lbl1 = il.DefineLabel();
                var lbl2 = il.DefineLabel();
                il.MarkLabel(lbl1);
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Isinst, _value_ref_type);
                il.Emit(OpCodes.Brfalse, lbl2);
                il.Emit(OpCodes.Ldfld, _value_ref_field);
                il.Emit(OpCodes.Br, lbl1);
                il.MarkLabel(lbl2); 
            }
            else if (expr is MGetEnv)
            {
                var e = (MGetEnv)expr;
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldc_I4, e.Index);
                il.Emit(OpCodes.Ldelem_Ref);
                // deref
                var lbl1 = il.DefineLabel();
                var lbl2 = il.DefineLabel();
                il.MarkLabel(lbl1);
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Isinst, _value_ref_type);
                il.Emit(OpCodes.Brfalse, lbl2);
                il.Emit(OpCodes.Ldfld, _value_ref_field);
                il.Emit(OpCodes.Br, lbl1);
                il.MarkLabel(lbl2);
            }
            else if (expr is MMakeClos)
            {
                var e = (MMakeClos)expr;
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
                for (var i = 0; i < e.Args.Length; i++)
                {
                    il.Emit(OpCodes.Ldloc, arr2);
                    il.Emit(OpCodes.Ldc_I4, i);
                    Compile(il, e.Args[i], env);
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
            else if (expr is MVarClos)
            {
                var e = (MVarClos)expr;
                var loc = env.Lookup(e.Name);
                il.Emit(OpCodes.Ldloc, loc);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        static LEnv CompilePat(ILGenerator il, MPat pat, Label fail_label)
        {
            if (pat is PVar)
            {
                var p = (PVar)pat;
                var loc = il.DeclareLocal(typeof(object));
                il.Emit(OpCodes.Stloc, loc);
                return new LEnv().Cons(p.Name, loc);
            }
            else if (pat is PWild)
            {
                il.Emit(OpCodes.Pop);
                return new LEnv();
            }
            else if (pat is PInt)
            {
                var p = (PInt)pat;
                il.Emit(OpCodes.Unbox_Any, typeof(int));
                il.Emit(OpCodes.Ldc_I4, p.Value);
                il.Emit(OpCodes.Bne_Un, fail_label);
                return new LEnv();
            }
            else if (pat is PDouble)
            {
                var p = (PDouble)pat;
                il.Emit(OpCodes.Unbox_Any, typeof(double));
                il.Emit(OpCodes.Ldc_R8, p.Value);
                il.Emit(OpCodes.Bne_Un, fail_label);
                return new LEnv();
            }
            else if (pat is PString)
            {
                var p = (PString)pat;
                il.Emit(OpCodes.Ldstr, p.Value);
                il.Emit(OpCodes.Call, _string_equal);
                il.Emit(OpCodes.Brfalse, fail_label);
                return new LEnv();
            }
            else if (pat is PChar)
            {
                var p = (PChar)pat;
                il.Emit(OpCodes.Unbox_Any, typeof(char));
                il.Emit(OpCodes.Ldc_I4, p.Value);
                il.Emit(OpCodes.Bne_Un, fail_label);
                return new LEnv();
            }
            else if (pat is PUnit)
            {
                il.Emit(OpCodes.Pop);
                return new LEnv();
            }
            else if (pat is PBool)
            {
                var p = (PBool)pat;
                il.Emit(OpCodes.Unbox_Any, typeof(bool));
                if (p.Value)
                {
                    il.Emit(OpCodes.Ldc_I4_1);
                }
                else
                {
                    il.Emit(OpCodes.Ldc_I4_0);
                }
                il.Emit(OpCodes.Bne_Un, fail_label);
                return new LEnv();
            }
            else if (pat is PNil)
            {
                il.Emit(OpCodes.Ldfld, _tag_id);
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Bne_Un, fail_label);
                return new LEnv();
            }
            else if (pat is PCons)
            {
                var p = (PCons)pat;
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Dup);
                var lbl1 = il.DefineLabel();
                var lbl2 = il.DefineLabel();
                var lbl3 = il.DefineLabel();
                il.Emit(OpCodes.Ldfld, _tag_id);
                il.Emit(OpCodes.Ldc_I4_1);
                il.Emit(OpCodes.Bne_Un, lbl1);

                il.Emit(OpCodes.Ldfld, _tag_args);
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Ldelem_Ref);
                var env1 = CompilePat(il, p.Head, lbl2);

                il.Emit(OpCodes.Ldfld, _tag_args);
                il.Emit(OpCodes.Ldc_I4_1);
                il.Emit(OpCodes.Ldelem_Ref);
                var env2 = CompilePat(il, p.Tail, fail_label);
                il.Emit(OpCodes.Br, lbl3);

                il.MarkLabel(lbl1);
                il.Emit(OpCodes.Pop);
                il.MarkLabel(lbl2);
                il.Emit(OpCodes.Pop);
                il.Emit(OpCodes.Br, fail_label);
                il.MarkLabel(lbl3);
                return env1.Append(env2);
            }
            else if (pat is PTuple)
            {
                var p = (PTuple)pat;
                var env = new LEnv();
                var lbl1 = il.DefineLabel();
                var lbl2 = il.DefineLabel();
                for (var i = 0; i < p.Size; i++)
                {
                    il.Emit(OpCodes.Dup);
                    il.Emit(OpCodes.Ldc_I4, i);
                    il.Emit(OpCodes.Ldelem_Ref);
                    env = CompilePat(il, p.Items[i], lbl1).Append(env);
                }
                il.Emit(OpCodes.Pop);
                il.Emit(OpCodes.Br, lbl2);

                il.MarkLabel(lbl1);
                il.Emit(OpCodes.Pop);
                il.Emit(OpCodes.Br, fail_label);
                il.MarkLabel(lbl2);
                return env;
            }
            else if (pat is PAs)
            {
                var p = (PAs)pat;
                var lbl1 = il.DefineLabel();
                var lbl2 = il.DefineLabel();
                il.Emit(OpCodes.Dup);
                var env = CompilePat(il, p.Pat, lbl1);

                var loc = il.DeclareLocal(typeof(object));
                il.Emit(OpCodes.Stloc, loc);
                il.Emit(OpCodes.Br, lbl2);

                il.MarkLabel(lbl1);
                il.Emit(OpCodes.Pop);
                il.Emit(OpCodes.Br, fail_label);
                il.MarkLabel(lbl2);

                return env.Cons(p.Name, loc);
            }
            else if (pat is POr)
            {
                var p = (POr)pat;
                var lbl1 = il.DefineLabel();
                var lbl2 = il.DefineLabel();
                var lbl_end = il.DefineLabel();

                il.Emit(OpCodes.Dup);
                var env1 = CompilePat(il, p.Pat1, lbl1);
                il.Emit(OpCodes.Pop);
                il.Emit(OpCodes.Br, lbl_end);

                il.MarkLabel(lbl1);
                var env2 = CompilePat(il, p.Pat1, fail_label);

                var env = env1;
                while (!env.IsEmpty())
                {
                    var loc1 = env.Head.Item2;
                    var loc2 = env2.Lookup(env.Head.Item1);
                    il.Emit(OpCodes.Ldloc, loc2);
                    il.Emit(OpCodes.Stloc, loc1);
                }
                il.MarkLabel(lbl_end);

                return env1;
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

                case "add":
                    il.Emit(OpCodes.Call, _add);
                    break;

                case "sub":
                    il.Emit(OpCodes.Call, _sub);
                    break;

                case "mul":
                    il.Emit(OpCodes.Call, _mul);
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

        static ConstructorInfo SystemConstructor(string assembly, string type_name, Type[] arg_types)
        {
            var dll = Assembly.LoadFrom(Path.Combine(_runtime_dir, assembly));
            var type = dll.GetType(type_name);
            var constructor = type.GetConstructor(arg_types);
            return constructor;
        }
    }
}
