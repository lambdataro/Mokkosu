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
    using Mokkosu.TypeInference;
    
    static class CodeGenerator
    {
        static int _count = 0;

        static Dictionary<string, MethodBuilder> _function_table;

        static FieldInfo _value_ref_field;
        static Type _value_ref_type;

        static FieldInfo _tag_id;
        static FieldInfo _tag_args;
        static Type _tag_type;

        static MethodInfo _string_equal =
            CodeGeneratorCommon.SystemMethod(
                "mscorlib.dll", "System.String", "Equals", 
                new Type[] { typeof(string), typeof(string) });

        static MethodInfo _object_equal =
            CodeGeneratorCommon.
                SystemMethod("mscorlib.dll", "System.Object", "Equals",
                new Type[] { typeof(object) });

        static MethodInfo _delegate_create =
            CodeGeneratorCommon.
                SystemMethod("mscorlib.dll", "System.Delegate", "CreateDelegate",
                new Type[] { typeof(Type), typeof(MethodInfo) });

        static ConstructorInfo _application_exception =
            CodeGeneratorCommon.
                SystemConstructor("mscorlib.dll", "System.ApplicationException", new Type[] { typeof(string) });

        static MethodInfo _println;
        static MethodInfo _print;
        static MethodInfo _tostring;

        static MethodInfo _concat;

        static MethodInfo _add;
        static MethodInfo _sub;
        static MethodInfo _mul;
        static MethodInfo _div;
        static MethodInfo _mod;

        static MethodInfo _band;
        static MethodInfo _bor;
        static MethodInfo _bxor;
        static MethodInfo _bnot;
        static MethodInfo _bshr;
        static MethodInfo _bshl;
        static MethodInfo _bshrun;

        static MethodInfo _fadd;
        static MethodInfo _fsub;
        static MethodInfo _fmul;
        static MethodInfo _fdiv;

        static MethodInfo _eq;
        static MethodInfo _ne;
        static MethodInfo _lt;
        static MethodInfo _gt;
        static MethodInfo _le;
        static MethodInfo _ge;

        static MethodInfo _ref;
        static MethodInfo _deref;
        static MethodInfo _assign;

        static MethodInfo _error;

        static TypeBuilder _type_builder;

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
            _tag_args = tag.DefineField("args", typeof(object), FieldAttributes.Public);
            _tag_type = tag.CreateType();

            // MokkosuProgram クラス
            var type_builder = module_builder.DefineType("MokkosuProgram");
            _type_builder = type_builder;

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
            var ilgen = _function_table["MokkosuMain"].GetILGenerator();
            Compile(ilgen, cc_result.Main, new LEnv());
            ilgen.Emit(OpCodes.Ret);

            // MokkosuEntryPoint
            var builder = type_builder.DefineMethod("MokkosuEntryPoint",
                MethodAttributes.Static, typeof(void), new Type[] { });
            ilgen = builder.GetILGenerator();
            ilgen.Emit(OpCodes.Ldnull);
            ilgen.Emit(OpCodes.Ldnull);
            ilgen.Emit(OpCodes.Call, _function_table["MokkosuMain"]);
            ilgen.Emit(OpCodes.Pop);
            ilgen.Emit(OpCodes.Ret);

            type_builder.CreateType();
            assembly_builder.SetEntryPoint(builder);
            return assembly_builder;
        }

        static string GenDelegateName()
        {
            return string.Format("delegate@{0:000}", ++_count);
        }

        static string GenFieldName()
        {
            return string.Format("field@{0:000}", ++_count);
        }

        static void DefinePrimFun(TypeBuilder type_builder)
        {
            _println = PrimitiveFunctions.DefinePrintLn(type_builder);
            _print = PrimitiveFunctions.DefinePrint(type_builder);
            _tostring = PrimitiveFunctions.DefineToString(type_builder);

            _concat = PrimitiveFunctions.DefineConcat(type_builder);

            _add = PrimitiveFunctions.DefineAdd(type_builder);
            _sub = PrimitiveFunctions.DefineSub(type_builder);
            _mul = PrimitiveFunctions.DefineMul(type_builder);
            _div = PrimitiveFunctions.DefineDiv(type_builder);
            _mod = PrimitiveFunctions.DefineMod(type_builder);

            _band = PrimitiveFunctions.DefineBAnd(type_builder);
            _bor = PrimitiveFunctions.DefineBOr(type_builder);
            _bxor = PrimitiveFunctions.DefineBXor(type_builder);
            _bnot = PrimitiveFunctions.DefineBNot(type_builder);
            _bshr = PrimitiveFunctions.DefineBShr(type_builder);
            _bshl = PrimitiveFunctions.DefineBShl(type_builder);
            _bshrun = PrimitiveFunctions.DefineBShrUn(type_builder);

            _fadd = PrimitiveFunctions.DefineFAdd(type_builder);
            _fsub = PrimitiveFunctions.DefineFSub(type_builder);
            _fmul = PrimitiveFunctions.DefineFMul(type_builder);
            _fdiv = PrimitiveFunctions.DefineFDiv(type_builder);

            _eq = PrimitiveFunctions.DefineEq(type_builder);
            _ne = PrimitiveFunctions.DefineNe(type_builder);
            _lt = PrimitiveFunctions.DefineLt(type_builder);
            _gt = PrimitiveFunctions.DefineGt(type_builder);
            _le = PrimitiveFunctions.DefineLe(type_builder);
            _ge = PrimitiveFunctions.DefineGe(type_builder);

            _error = PrimitiveFunctions.DefineError(type_builder);

            _ref = PrimitiveFunctions.DefineRef(type_builder, _tag_type, _tag_args);
            _deref = PrimitiveFunctions.DefineDeRef(type_builder, _tag_args);
            _assign = PrimitiveFunctions.DefineAssign(type_builder, _tag_args);
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
                il.Emit(OpCodes.Box, typeof(bool));
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
                var ary = il.DeclareLocal(typeof(object));
                il.Emit(OpCodes.Stloc, ary);

                var lbl1 = il.DefineLabel();
                var lbl2 = il.DefineLabel();

                il.Emit(OpCodes.Ldloc, ary);
                il.Emit(OpCodes.Isinst, _tag_type);
                il.Emit(OpCodes.Brfalse, lbl1);
                il.Emit(OpCodes.Ldloc, ary);
                Compile(il, e.ArgExpr, env);
                il.Emit(OpCodes.Stfld, _tag_args);
                il.Emit(OpCodes.Ldloc, ary);
                il.Emit(OpCodes.Br, lbl2);

                il.MarkLabel(lbl1);
                Compile(il, e.ArgExpr, env);
                il.Emit(OpCodes.Ldloc, ary);
                il.Emit(OpCodes.Ldc_I4_1);
                il.Emit(OpCodes.Ldelem_Ref);
                il.Emit(OpCodes.Ldloc, ary);
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Ldelem_Ref);
                if (e.TailCall)
                {
                    il.Emit(OpCodes.Tailcall);
                }
                il.EmitCalli(OpCodes.Calli,
                    CallingConventions.Standard,
                    typeof(object),
                    new Type[] { typeof(object), typeof(object[]) },
                    null);
                il.MarkLabel(lbl2);
            }
            else if (expr is MIf)
            {
                var e = (MIf)expr;
                Compile(il, e.CondExpr, env);
                var lbl1 = il.DefineLabel();
                var lbl2 = il.DefineLabel();
                il.Emit(OpCodes.Unbox_Any, typeof(bool));
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
                var lbl1 = il.DefineLabel();
                Compile(il, e.Expr, env);
                var env2 = CompilePat(il, e.Pat, fail_label);
                var env3 = env2.Append(env);
                Compile(il, e.Guard, env3);
                il.Emit(OpCodes.Unbox_Any, typeof(bool));
                il.Emit(OpCodes.Brfalse, fail_label);
                Compile(il, e.ThenExpr, env3);
                il.Emit(OpCodes.Br, lbl1);
                il.MarkLabel(fail_label);
                Compile(il, e.ElseExpr, env);
                il.MarkLabel(lbl1);
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
                    il.Emit(OpCodes.Stloc, locals[i]);
                }
                for (var i = 0; i < locals.Length; i++)
                {
                    il.Emit(OpCodes.Ldloc, locals[i]);
                    Compile(il, e.Items[i].Expr, env2);
                    il.Emit(OpCodes.Stfld, _value_ref_field);
                }
                Compile(il, e.E2, env2);
            }
            else if (expr is MRuntimeError)
            {
                var e = (MRuntimeError)expr;
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
            else if (expr is MCallStatic)
            {
                var e = (MCallStatic)expr;

                for (int i = 0; i < e.Args.Count; i++)
                {
                    Compile(il, e.Args[i], env);
                    var t = Typeinf.ReduceType(e.Types[i]);
                    if (t is IntType)
                    {
                        il.Emit(OpCodes.Unbox_Any, typeof(int));
                    }
                    else if (t is DoubleType)
                    {
                        il.Emit(OpCodes.Unbox_Any, typeof(double));
                    }
                    else if (t is CharType)
                    {
                        il.Emit(OpCodes.Unbox_Any, typeof(char));
                    }
                    else if (t is BoolType)
                    {
                        il.Emit(OpCodes.Unbox_Any, typeof(bool));
                    }
                    else if (t is DotNetType)
                    {
                        var dotnettype = (DotNetType)t;
                        if (dotnettype.Type.IsValueType)
                        {
                            il.Emit(OpCodes.Unbox_Any, dotnettype.Type);
                        }
                    }
                }

                il.Emit(OpCodes.Call, e.Info);

                if (e.Info.ReturnType == typeof(void))
                {
                    il.Emit(OpCodes.Ldc_I4_0);
                }
                else if (e.Info.ReturnType.IsValueType)
                {
                    il.Emit(OpCodes.Box, e.Info.ReturnType);
                }
            }
            else if (expr is MCast)
            {
                var e = (MCast)expr;
                Compile(il, e.Expr, env);
                il.Emit(OpCodes.Castclass, e.DstType);
            }
            else if (expr is MNewClass)
            {
                var e = (MNewClass)expr;

                for (int i = 0; i < e.Args.Count; i++)
                {
                    Compile(il, e.Args[i], env);
                    var t = Typeinf.ReduceType(e.Types[i]);
                    if (t is IntType)
                    {
                        il.Emit(OpCodes.Unbox_Any, typeof(int));
                    }
                    else if (t is DoubleType)
                    {
                        il.Emit(OpCodes.Unbox_Any, typeof(double));
                    }
                    else if (t is CharType)
                    {
                        il.Emit(OpCodes.Unbox_Any, typeof(char));
                    }
                    else if (t is BoolType)
                    {
                        il.Emit(OpCodes.Unbox_Any, typeof(bool));
                    }
                    else if (t is DotNetType)
                    {
                        var dotnettype = (DotNetType)t;
                        if (dotnettype.Type.IsValueType)
                        {
                            il.Emit(OpCodes.Unbox_Any, dotnettype.Type);
                        }
                    }
                }

                il.Emit(OpCodes.Newobj, e.Info);

                if (e.Info.DeclaringType.IsValueType)
                {
                    il.Emit(OpCodes.Box, e.Info.DeclaringType);
                }
            }
            else if (expr is MInvoke)
            {
                var e = (MInvoke)expr;

                Compile(il, e.Expr, env);

                var t = Typeinf.ReduceType(e.ExprType);

                if (t is IntType)
                {
                    il.Emit(OpCodes.Unbox_Any, typeof(int));
                }
                else if (t is DoubleType)
                {
                    il.Emit(OpCodes.Unbox_Any, typeof(double));
                }
                else if (t is CharType)
                {
                    il.Emit(OpCodes.Unbox_Any, typeof(char));
                }
                else if (t is BoolType)
                {
                    il.Emit(OpCodes.Unbox_Any, typeof(bool));
                }
                else if (t is DotNetType)
                {
                    var dotnettype = (DotNetType)t;
                    if (dotnettype.Type.IsValueType)
                    {
                        il.Emit(OpCodes.Unbox_Any, dotnettype.Type);
                    }
                }

                for (int i = 0; i < e.Args.Count; i++)
                {
                    Compile(il, e.Args[i], env);
                    var tt = Typeinf.ReduceType(e.Types[i]);
                    if (tt is IntType)
                    {
                        il.Emit(OpCodes.Unbox_Any, typeof(int));
                    }
                    else if (tt is DoubleType)
                    {
                        il.Emit(OpCodes.Unbox_Any, typeof(double));
                    }
                    else if (tt is CharType)
                    {
                        il.Emit(OpCodes.Unbox_Any, typeof(char));
                    }
                    else if (tt is BoolType)
                    {
                        il.Emit(OpCodes.Unbox_Any, typeof(bool));
                    }
                    else if (tt is DotNetType)
                    {
                        var dotnettype = (DotNetType)tt;
                        if (dotnettype.Type.IsValueType)
                        {
                            il.Emit(OpCodes.Unbox_Any, dotnettype.Type);
                        }
                    }
                }

                var dotnet = TypeinfDotNet.MokkosuTypeToDotNetType("", t);
                if (dotnet.IsValueType)
                {
                    var loc = il.DeclareLocal(dotnet);
                    il.Emit(OpCodes.Stloc, loc);
                    il.Emit(OpCodes.Ldloca, loc);
                    il.Emit(OpCodes.Call, e.Info);
                }
                else
                {
                    il.Emit(OpCodes.Callvirt, e.Info);
                }

                if (e.Info.ReturnType == typeof(void))
                {
                    il.Emit(OpCodes.Ldc_I4_0);
                }
                else if (e.Info.ReturnType.IsValueType)
                {
                    il.Emit(OpCodes.Box, e.Info.ReturnType);
                }
            }
            else if (expr is MDelegate)
            {
                var e = (MDelegate)expr;

                // メソッドを生成
                var method = _type_builder.DefineMethod(GenDelegateName(), 
                    MethodAttributes.Static, typeof(void), e.ParamType);
                var delil = method.GetILGenerator();

                var field = _type_builder.DefineField(GenFieldName(), 
                    typeof(object[]), FieldAttributes.Static);

                Compile(il, e.Expr, env);
                il.Emit(OpCodes.Stsfld, field);

                delil.Emit(OpCodes.Ldsfld, field);
                var fun = delil.DeclareLocal(typeof(object));
                delil.Emit(OpCodes.Stloc, fun);

                var ary = delil.DeclareLocal(typeof(object[]));
                delil.Emit(OpCodes.Ldc_I4, e.ParamType.Length);
                delil.Emit(OpCodes.Newarr, typeof(object));
                delil.Emit(OpCodes.Stloc, ary);
                for (int i = 0; i < e.ParamType.Length; i++)
                {
                    delil.Emit(OpCodes.Ldloc, ary);
                    delil.Emit(OpCodes.Ldc_I4, i);
                    delil.Emit(OpCodes.Ldarg, i);
                    delil.Emit(OpCodes.Stelem, typeof(object));
                }
                delil.Emit(OpCodes.Ldloc, ary);
                delil.Emit(OpCodes.Ldloc, fun);
                delil.Emit(OpCodes.Ldc_I4_1);
                delil.Emit(OpCodes.Ldelem_Ref);
                delil.Emit(OpCodes.Ldloc, fun);
                delil.Emit(OpCodes.Ldc_I4_0);
                delil.Emit(OpCodes.Ldelem_Ref);
                delil.EmitCalli(OpCodes.Calli,
                    CallingConventions.Standard,
                    typeof(object),
                    new Type[] { typeof(object), typeof(object[]) },
                    null);
                delil.Emit(OpCodes.Pop);
                delil.Emit(OpCodes.Ret);
                // メソッドここまで

                il.Emit(OpCodes.Ldnull);
                il.Emit(OpCodes.Ldftn, method);
                il.Emit(OpCodes.Newobj, e.CstrInfo);
            }
            else if (expr is MSet)
            {
                var e = (MSet)expr;

                Compile(il, e.Expr, env);

                var t = Typeinf.ReduceType(e.ExprType);

                if (t is IntType)
                {
                    il.Emit(OpCodes.Unbox_Any, typeof(int));
                }
                else if (t is DoubleType)
                {
                    il.Emit(OpCodes.Unbox_Any, typeof(double));
                }
                else if (t is CharType)
                {
                    il.Emit(OpCodes.Unbox_Any, typeof(char));
                }
                else if (t is BoolType)
                {
                    il.Emit(OpCodes.Unbox_Any, typeof(bool));
                }
                else if (t is DotNetType)
                {
                    var dotnettype = (DotNetType)t;
                    if (dotnettype.Type.IsValueType)
                    {
                        il.Emit(OpCodes.Unbox_Any, dotnettype.Type);
                    }
                }

                Compile(il, e.Arg, env);

                var tt = Typeinf.ReduceType(e.ArgType);

                if (tt is IntType)
                {
                    il.Emit(OpCodes.Unbox_Any, typeof(int));
                }
                else if (tt is DoubleType)
                {
                    il.Emit(OpCodes.Unbox_Any, typeof(double));
                }
                else if (tt is CharType)
                {
                    il.Emit(OpCodes.Unbox_Any, typeof(char));
                }
                else if (tt is BoolType)
                {
                    il.Emit(OpCodes.Unbox_Any, typeof(bool));
                }
                else if (t is DotNetType)
                {
                    var dotnettype = (DotNetType)tt;
                    if (dotnettype.Type.IsValueType)
                    {
                        il.Emit(OpCodes.Unbox_Any, dotnettype.Type);
                    }
                }

                il.Emit(OpCodes.Stfld, e.Info);

                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Box, typeof(int));
            }
            else if (expr is MGet)
            {
                var e = (MGet)expr;

                Compile(il, e.Expr, env);

                var t = Typeinf.ReduceType(e.ExprType);

                if (t is IntType)
                {
                    il.Emit(OpCodes.Unbox_Any, typeof(int));
                }
                else if (t is DoubleType)
                {
                    il.Emit(OpCodes.Unbox_Any, typeof(double));
                }
                else if (t is CharType)
                {
                    il.Emit(OpCodes.Unbox_Any, typeof(char));
                }
                else if (t is BoolType)
                {
                    il.Emit(OpCodes.Unbox_Any, typeof(bool));
                }
                else if (t is DotNetType)
                {
                    var dotnettype = (DotNetType)t;
                    if (dotnettype.Type.IsValueType)
                    {
                        il.Emit(OpCodes.Unbox_Any, dotnettype.Type);
                    }
                }

                il.Emit(OpCodes.Ldfld, e.Info);

                if (e.Info.FieldType.IsValueType)
                {
                    il.Emit(OpCodes.Box, e.Info.FieldType);
                }
            }
            else if (expr is MSSet)
            {
                var e = (MSSet)expr;

                Compile(il, e.Arg, env);

                var tt = Typeinf.ReduceType(e.ArgType);

                if (tt is IntType)
                {
                    il.Emit(OpCodes.Unbox_Any, typeof(int));
                }
                else if (tt is DoubleType)
                {
                    il.Emit(OpCodes.Unbox_Any, typeof(double));
                }
                else if (tt is CharType)
                {
                    il.Emit(OpCodes.Unbox_Any, typeof(char));
                }
                else if (tt is BoolType)
                {
                    il.Emit(OpCodes.Unbox_Any, typeof(bool));
                }
                else if (tt is DotNetType)
                {
                    var dotnettype = (DotNetType)tt;
                    if (dotnettype.Type.IsValueType)
                    {
                        il.Emit(OpCodes.Unbox_Any, dotnettype.Type);
                    }
                }

                il.Emit(OpCodes.Stsfld, e.Info);

                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Box, typeof(int));
            }
            else if (expr is MSGet)
            {
                var e = (MSGet)expr;
                if (e.Info.IsLiteral)
                {
                    if (e.Info.FieldType == typeof(int))
                    {
                        il.Emit(OpCodes.Ldc_I4, (int)e.Info.GetRawConstantValue());
                    }
                    else if (e.Info.FieldType == typeof(double))
                    {
                        il.Emit(OpCodes.Ldc_R8, (double)e.Info.GetRawConstantValue());
                    }
                    else
                    {
                        var n = e.Info.GetValue(null);
                        il.Emit(OpCodes.Ldc_I4, (int)n);
                    }
                }
                else
                {
                    il.Emit(OpCodes.Ldsfld, e.Info);
                }
                if (e.Info.FieldType.IsValueType)
                {
                    il.Emit(OpCodes.Box, e.Info.FieldType);
                }
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
                if (p.IsTag)
                {
                    il.Emit(OpCodes.Isinst, _tag_type);
                    il.Emit(OpCodes.Ldfld, _tag_id);
                    il.Emit(OpCodes.Ldc_I4, p.TagIndex);
                    il.Emit(OpCodes.Bne_Un, fail_label);
                    return new LEnv();
                }
                else
                {
                    var loc = il.DeclareLocal(typeof(object));
                    il.Emit(OpCodes.Stloc, loc);
                    return new LEnv().Cons(p.Name, loc);
                }
            }
            else if (pat is PWild)
            {
                il.Emit(OpCodes.Pop);
                return new LEnv();
            }
            else if (pat is PFource)
            {
                var p = (PFource)pat;
                return CompilePat(il, p.Pat, fail_label);
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
            else if (pat is PUserTag)
            {
                var p = (PUserTag)pat;
                var lbl1 = il.DefineLabel();
                var lbl2 = il.DefineLabel();

                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Isinst, _tag_type);
                il.Emit(OpCodes.Ldfld, _tag_id);
                il.Emit(OpCodes.Ldc_I4, p.TagIndex);
                il.Emit(OpCodes.Bne_Un, lbl1);

                var env = new LEnv();
                if (p.Args.Count == 1)
                {
                    il.Emit(OpCodes.Ldfld, _tag_args);
                    env = CompilePat(il, p.Args[0], fail_label).Append(env);
                    il.Emit(OpCodes.Br, lbl2);
                }
                else
                {
                    for (var i = 0; i < p.Args.Count; i++)
                    {
                        il.Emit(OpCodes.Dup);
                        il.Emit(OpCodes.Ldfld, _tag_args);
                        il.Emit(OpCodes.Ldc_I4, i);
                        il.Emit(OpCodes.Ldelem_Ref);
                        env = CompilePat(il, p.Args[i], lbl1).Append(env);
                    }
                    il.Emit(OpCodes.Pop);
                    il.Emit(OpCodes.Br, lbl2);
                }

                il.MarkLabel(lbl1);
                il.Emit(OpCodes.Pop);
                il.Emit(OpCodes.Br, fail_label);
                il.MarkLabel(lbl2);
                return env;
            }
            else
            {
                throw new NotImplementedException();
            }

        }

        static void CompilePrim(ILGenerator il, string name, List<MType> arg_types, MType ret_type)
        {
            var lbl1 = il.DefineLabel();
            var lbl2 = il.DefineLabel();

            switch (name)
            {
                case "println":
                    il.Emit(OpCodes.Call, _println);
                    break;
                case "print":
                    il.Emit(OpCodes.Call, _print);
                    break;
                case "tostring":
                    il.Emit(OpCodes.Call, _tostring);
                    break;
                case "concat":
                    il.Emit(OpCodes.Call, _concat);
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
                case "div":
                    il.Emit(OpCodes.Call, _div);
                    break;
                case "mod":
                    il.Emit(OpCodes.Call, _mod);
                    break;
                case "band":
                    il.Emit(OpCodes.Call, _band);
                    break;
                case "bor":
                    il.Emit(OpCodes.Call, _bor);
                    break;
                case "bxor":
                    il.Emit(OpCodes.Call, _bxor);
                    break;
                case "bnot":
                    il.Emit(OpCodes.Call, _bnot);
                    break;
                case "bshr":
                    il.Emit(OpCodes.Call, _bshr);
                    break;
                case "bshl":
                    il.Emit(OpCodes.Call, _bshl);
                    break;
                case "bshrun":
                    il.Emit(OpCodes.Call, _bshrun);
                    break;
                case "fadd":
                    il.Emit(OpCodes.Call, _fadd);
                    break;
                case "fsub":
                    il.Emit(OpCodes.Call, _fsub);
                    break;
                case "fmul":
                    il.Emit(OpCodes.Call, _fmul);
                    break;
                case "fdiv":
                    il.Emit(OpCodes.Call, _fdiv);
                    break;
                case "eq":
                    il.Emit(OpCodes.Call, _eq);
                    break;
                case "ne":
                    il.Emit(OpCodes.Call, _ne);
                    break;
                case "lt":
                    il.Emit(OpCodes.Call, _lt);
                    break;
                case "gt":
                    il.Emit(OpCodes.Call, _gt);
                    break;
                case "le":
                    il.Emit(OpCodes.Call, _le);
                    break;
                case "ge":
                    il.Emit(OpCodes.Call, _ge);
                    break;
                case "error":
                    il.Emit(OpCodes.Call, _error);
                    break;
                case "ref":
                    il.Emit(OpCodes.Call, _ref);
                    break;
                case "deref":
                    il.Emit(OpCodes.Call, _deref);
                    break;
                case "assign":
                    il.Emit(OpCodes.Call, _assign);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
