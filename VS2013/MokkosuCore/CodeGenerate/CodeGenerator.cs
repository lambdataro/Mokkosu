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

        public static AssemblyBuilder Start(string directory, string name,
            ClosureConversionResult cc_result, bool is_dynamic)
        {
            _function_table = new Dictionary<string, MethodBuilder>();

            var assembly_name = new AssemblyName(name);
            var assembly_builder = AppDomain.CurrentDomain.DefineDynamicAssembly(
                assembly_name, AssemblyBuilderAccess.RunAndSave, directory);
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

            TypeBuilder type_builder;
            // MokkosuProgram クラス
            if (is_dynamic)
            {
                type_builder = module_builder.DefineType("MokkosuProgram",
                                TypeAttributes.Public, typeof(object), new Type[] { typeof(IMokkosuProgram) });
            }
            else
            {
                type_builder = module_builder.DefineType("MokkosuProgram", TypeAttributes.Public);
            }
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
                Compile(il, new ContEval() { Expr = f.Value, Env = new LEnv() });
                il.Emit(OpCodes.Ret);
            }
            var ilgen = _function_table["MokkosuMain"].GetILGenerator();
            Compile(ilgen, new ContEval() { Expr = cc_result.Main, Env = new LEnv() });
            ilgen.Emit(OpCodes.Ret);

            // MokkosuEntryPoint

            MethodBuilder builder;
            if (is_dynamic)
            {
                builder = type_builder.DefineMethod("MokkosuEntryPoint",
                    MethodAttributes.Virtual,
                    typeof(void), new Type[] { });
                type_builder.DefineMethodOverride(builder,
                    typeof(IMokkosuProgram).GetMethod("MokkosuEntryPoint"));
            }
            else
            {
                builder = type_builder.DefineMethod("MokkosuStaticEntryPoint",
                    MethodAttributes.Static, typeof(void), new Type[] { });
            }

            if (Global.IsDefineKey("STA_THREAD"))
            {
                var stathread = Type.GetType("System.STAThreadAttribute").GetConstructor(new Type[] { });
                builder.SetCustomAttribute(new CustomAttributeBuilder(stathread, new Type[] { }));
            }

            ilgen = builder.GetILGenerator();
            ilgen.Emit(OpCodes.Ldnull);
            ilgen.Emit(OpCodes.Ldnull);
            ilgen.Emit(OpCodes.Call, _function_table["MokkosuMain"]);
            ilgen.Emit(OpCodes.Pop);
            ilgen.Emit(OpCodes.Ret);

            type_builder.CreateType();
            if (Global.IsDefineKey("CONSOLE_APPLICATION"))
            {
                assembly_builder.SetEntryPoint(builder, PEFileKinds.ConsoleApplication);
            }
            else
            {
                assembly_builder.SetEntryPoint(builder, PEFileKinds.WindowApplication);
            }

            return assembly_builder;
        }

        static string GenDelegateName()
        {
            _count++;
            return string.Format("delegate@{0:000}", _count);
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

        static void Compile(ILGenerator il, CodeGeneratorCont start_cont)
        {
            var compiler_stack = new Stack<CodeGeneratorCont>();
            compiler_stack.Push(start_cont);

            while (compiler_stack.Count > 0)
            {
                var cont = compiler_stack.Pop();

                if (cont is ContEval)
                {
                    var k = (ContEval)cont;

                    if (k.Expr is MInt)
                    {
                        var e = (MInt)k.Expr;
                        il.Emit(OpCodes.Ldc_I4, e.Value);
                        il.Emit(OpCodes.Box, typeof(int));
                    }
                    else if (k.Expr is MDouble)
                    {
                        var e = (MDouble)k.Expr;
                        il.Emit(OpCodes.Ldc_R8, e.Value);
                        il.Emit(OpCodes.Box, typeof(double));
                    }
                    else if (k.Expr is MString)
                    {
                        var e = (MString)k.Expr;
                        il.Emit(OpCodes.Ldstr, e.Value);
                    }
                    else if (k.Expr is MChar)
                    {
                        var e = (MChar)k.Expr;
                        il.Emit(OpCodes.Ldc_I4, e.Value);
                        il.Emit(OpCodes.Box, typeof(char));
                    }
                    else if (k.Expr is MUnit)
                    {
                        il.Emit(OpCodes.Ldc_I4_0);
                        il.Emit(OpCodes.Box, typeof(int));
                    }
                    else if (k.Expr is MBool)
                    {
                        var e = (MBool)k.Expr;
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
                    else if (k.Expr is MVar)
                    {
                        var e = (MVar)k.Expr;
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
                            var loc = k.Env.Lookup(e.Name);
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
                    else if (k.Expr is MApp)
                    {
                        var e = (MApp)k.Expr;
                        compiler_stack.Push(new ContMApp1() { E = e, Env = k.Env });
                        compiler_stack.Push(new ContEval() { Expr = e.FunExpr, Env = k.Env });
                    }
                    else if (k.Expr is MIf)
                    {
                        var e = (MIf)k.Expr;
                        compiler_stack.Push(new ContMIf1() { E = e, Env = k.Env });
                        compiler_stack.Push(new ContEval() { Expr = e.CondExpr, Env = k.Env });
                    }
                    else if (k.Expr is MMatch)
                    {
                        var e = (MMatch)k.Expr;
                        compiler_stack.Push(new ContMatch1() { E = e, Env = k.Env });
                        compiler_stack.Push(new ContEval() { Expr = e.Expr, Env = k.Env });
                    }
                    else if (k.Expr is MNil)
                    {
                        var e = (MNil)k.Expr;
                        il.Emit(OpCodes.Newobj, _tag_type.GetConstructor(new Type[] { }));
                        il.Emit(OpCodes.Dup);
                        il.Emit(OpCodes.Dup);
                        il.Emit(OpCodes.Ldc_I4_0);
                        il.Emit(OpCodes.Stfld, _tag_id);
                        il.Emit(OpCodes.Ldnull);
                        il.Emit(OpCodes.Stfld, _tag_args);
                    }
                    else if (k.Expr is MCons)
                    {
                        var e = (MCons)k.Expr;
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
                        compiler_stack.Push(new ContMCons1() { E = e, Env = k.Env, Arr = arr });
                        compiler_stack.Push(new ContEval() { Expr = e.Head, Env = k.Env });
                    }
                    else if (k.Expr is MTuple)
                    {
                        var e = (MTuple)k.Expr;
                        il.Emit(OpCodes.Ldc_I4, e.Size);
                        il.Emit(OpCodes.Newarr, typeof(object));
                        var arr = il.DeclareLocal(typeof(object[]));
                        il.Emit(OpCodes.Stloc, arr);

                        il.Emit(OpCodes.Ldloc, arr);
                        il.Emit(OpCodes.Ldc_I4, 0);

                        compiler_stack.Push(new ContMTuple1() { E = e, Env = k.Env, Arr = arr, Idx = 1 });
                        compiler_stack.Push(new ContEval() { Expr = e.Items[0], Env = k.Env });
                    }
                    else if (k.Expr is MDo)
                    {
                        var e = (MDo)k.Expr;
                        compiler_stack.Push(new ContMDo1() { E = e, Env = k.Env });
                        compiler_stack.Push(new ContEval() { Expr = e.E1, Env = k.Env });
                    }
                    else if (k.Expr is MLet)
                    {
                        var e = (MLet)k.Expr;
                        compiler_stack.Push(new ContMLet1() { E = e, Env = k.Env });
                        compiler_stack.Push(new ContEval() { Expr = e.E1, Env = k.Env });
                    }
                    else if (k.Expr is MFun)
                    {
                        var e = (MFun)k.Expr;

                        var locals = e.Items.Select(item => il.DeclareLocal(typeof(object))).ToArray();
                        var env2 = k.Env;
                        for (var i = 0; i < e.Items.Count; i++)
                        {
                            env2 = env2.Cons(e.Items[i].Name, locals[i]);
                        }
                        for (var i = 0; i < locals.Length; i++)
                        {
                            il.Emit(OpCodes.Newobj, _value_ref_type.GetConstructor(new Type[] { }));
                            il.Emit(OpCodes.Stloc, locals[i]);
                        }
                        il.Emit(OpCodes.Ldloc, locals[0]);
                        compiler_stack.Push(new ContMFun1()
                        {
                            E = e,
                            Env = k.Env,
                            Env2 = env2,
                            Idx = 1,
                            Locals = locals
                        });
                        compiler_stack.Push(new ContEval() { Expr = e.Items[0].Expr, Env = env2 });
                    }
                    else if (k.Expr is MRuntimeError)
                    {
                        var e = (MRuntimeError)k.Expr;
                        il.Emit(OpCodes.Ldstr, e.Pos + ": " + e.Message);
                        il.Emit(OpCodes.Newobj, _application_exception);
                        il.Emit(OpCodes.Throw);
                    }
                    else if (k.Expr is MPrim)
                    {
                        var e = (MPrim)k.Expr;
                        if (e.Args.Count > 0)
                        {
                            compiler_stack.Push(new ContMPrim1() { E = e, Env = k.Env, Idx = 1 });
                            compiler_stack.Push(new ContEval() { Expr = e.Args[0], Env = k.Env });
                        }
                        else
                        {
                            compiler_stack.Push(new ContMPrim1() { E = e, Env = k.Env, Idx = 0 });
                        }
                    }
                    else if (k.Expr is MGetArg)
                    {
                        var e = (MGetArg)k.Expr;
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
                    else if (k.Expr is MGetEnv)
                    {
                        var e = (MGetEnv)k.Expr;
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
                    else if (k.Expr is MMakeClos)
                    {
                        var e = (MMakeClos)k.Expr;

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

                        if (e.Args.Length > 0)
                        {
                            // キャプチャされた値を配列に代入
                            il.Emit(OpCodes.Ldloc, arr2);
                            il.Emit(OpCodes.Ldc_I4, 0);

                            compiler_stack.Push(new ContMMakeClos1()
                            {
                                E = e,
                                Env = k.Env,
                                Arr1 = arr1,
                                Arr2 = arr2,
                                Idx = 1
                            });
                            compiler_stack.Push(new ContEval() { Expr = e.Args[0], Env = k.Env });
                        }
                        else
                        {
                            compiler_stack.Push(new ContMMakeClos1()
                            {
                                E = e,
                                Env = k.Env,
                                Arr1 = arr1,
                                Arr2 = arr2,
                                Idx = 0
                            });
                        }
                    }
                    else if (k.Expr is MVarClos)
                    {
                        var e = (MVarClos)k.Expr;
                        var loc = k.Env.Lookup(e.Name);
                        il.Emit(OpCodes.Ldloc, loc);
                    }
                    else if (k.Expr is MCallStatic)
                    {
                        var e = (MCallStatic)k.Expr;

                        compiler_stack.Push(new ContMCallStatic1()
                        {
                            E = e,
                            Env = k.Env,
                            Idx = 1
                        });
                        compiler_stack.Push(new ContEval() { Expr = e.Args[0], Env = k.Env });
                    }
                    else if (k.Expr is MCast)
                    {
                        var e = (MCast)k.Expr;
                        compiler_stack.Push(new ContMCast1() { E = e, Env = k.Env });
                        compiler_stack.Push(new ContEval() { Expr = e.Expr, Env = k.Env });
                    }
                    else if (k.Expr is MIsType)
                    {
                        var e = (MIsType)k.Expr;
                        compiler_stack.Push(new ContMIsType1() { E = e, Env = k.Env });
                        compiler_stack.Push(new ContEval() { Expr = e.Expr, Env = k.Env });
                    }
                    else if (k.Expr is MNewClass)
                    {
                        var e = (MNewClass)k.Expr;
                        if (e.Args.Count > 0)
                        {
                            compiler_stack.Push(new ContMNewClass1()
                            {
                                E = e,
                                Env = k.Env,
                                Idx = 1
                            });
                            compiler_stack.Push(new ContEval() { Expr = e.Args[0], Env = k.Env });
                        }
                        else
                        {
                            compiler_stack.Push(new ContMNewClass1()
                            {
                                E = e,
                                Env = k.Env,
                                Idx = 0
                            });
                        }
                    }
                    else if (k.Expr is MInvoke)
                    {
                        var e = (MInvoke)k.Expr;
                        compiler_stack.Push(new ContMInvoke1()
                        {
                            E = e,
                            Env = k.Env,
                        });
                        compiler_stack.Push(new ContEval() { Expr = e.Expr, Env = k.Env });
                    }
                    else if (k.Expr is MDelegate)
                    {
                        var e = (MDelegate)k.Expr;

                        // メソッドを生成
                        var method = _type_builder.DefineMethod(GenDelegateName(),
                            MethodAttributes.Static, typeof(void), e.ParamType);
                        var delil = method.GetILGenerator();

                        var field = _type_builder.DefineField(GenFieldName(),
                            typeof(object[]), FieldAttributes.Static);

                        compiler_stack.Push(new ContMDelegate1()
                        {
                            E = e,
                            Env = k.Env,
                            Method = method, 
                            Delil = delil,
                            Field = field
                        });
                        compiler_stack.Push(new ContEval() { Expr = e.Expr, Env = k.Env });
                    }
                    else if (k.Expr is MSet)
                    {
                        var e = (MSet)k.Expr;
                        compiler_stack.Push(new ContMSet1() { E = e, Env = k.Env });
                        compiler_stack.Push(new ContEval() { Expr = e.Expr, Env = k.Env });
                    }
                    else if (k.Expr is MGet)
                    {
                        var e = (MGet)k.Expr;
                        compiler_stack.Push(new ContMGet1() { E = e, Env = k.Env });
                        compiler_stack.Push(new ContEval() { Expr = e.Expr, Env = k.Env });
                    }
                    else if (k.Expr is MSSet)
                    {
                        var e = (MSSet)k.Expr;
                        compiler_stack.Push(new ContMSSet1() { E = e, Env = k.Env });
                        compiler_stack.Push(new ContEval() { Expr = e.Arg, Env = k.Env });
                    }
                    else if (k.Expr is MSGet)
                    {
                        var e = (MSGet)k.Expr;
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
                else if (cont is ContMApp1)
                {
                    var k = (ContMApp1)cont;
                    var ary = il.DeclareLocal(typeof(object));
                    il.Emit(OpCodes.Stloc, ary);

                    var lbl1 = il.DefineLabel();
                    var lbl2 = il.DefineLabel();

                    il.Emit(OpCodes.Ldloc, ary);
                    il.Emit(OpCodes.Isinst, _tag_type);
                    il.Emit(OpCodes.Brfalse, lbl1);
                    il.Emit(OpCodes.Ldloc, ary);
                    compiler_stack.Push(new ContMApp2()
                    {
                        E = k.E,
                        Env = k.Env,
                        Ary = ary,
                        Lbl1 = lbl1,
                        Lbl2 = lbl2
                    });
                    compiler_stack.Push(new ContEval()
                    {
                        Expr = k.E.ArgExpr,
                        Env = k.Env
                    });
                }
                else if (cont is ContMApp2)
                {
                    var k = (ContMApp2)cont;
                    il.Emit(OpCodes.Stfld, _tag_args);
                    il.Emit(OpCodes.Ldloc, k.Ary);
                    il.Emit(OpCodes.Br, k.Lbl2);
                    il.MarkLabel(k.Lbl1);
                    compiler_stack.Push(new ContMApp3()
                    {
                        E = k.E,
                        Env = k.Env,
                        Ary = k.Ary,
                        Lbl1 = k.Lbl1,
                        Lbl2 = k.Lbl2
                    });
                    compiler_stack.Push(new ContEval()
                    {
                        Expr = k.E.ArgExpr,
                        Env = k.Env
                    });
                }
                else if (cont is ContMApp3)
                {
                    var k = (ContMApp3)cont;
                    il.Emit(OpCodes.Ldloc, k.Ary);
                    il.Emit(OpCodes.Ldc_I4_1);
                    il.Emit(OpCodes.Ldelem_Ref);
                    il.Emit(OpCodes.Ldloc, k.Ary);
                    il.Emit(OpCodes.Ldc_I4_0);
                    il.Emit(OpCodes.Ldelem_Ref);
                    if (k.E.TailCall)
                    {
                        il.Emit(OpCodes.Tailcall);
                    }
                    il.EmitCalli(OpCodes.Calli,
                        CallingConventions.Standard,
                        typeof(object),
                        new Type[] { typeof(object), typeof(object[]) },
                        null);
                    if (k.E.TailCall)
                    {
                        il.Emit(OpCodes.Ret);
                        il.MarkLabel(k.Lbl2);
                    }
                    else
                    {
                        il.MarkLabel(k.Lbl2);
                    }
                }
                else if (cont is ContMIf1)
                {
                    var k = (ContMIf1)cont;
                    var lbl1 = il.DefineLabel();
                    var lbl2 = il.DefineLabel();
                    il.Emit(OpCodes.Unbox_Any, typeof(bool));
                    il.Emit(OpCodes.Brfalse, lbl1);
                    compiler_stack.Push(new ContMIf2()
                    {
                        E = k.E,
                        Env = k.Env,
                        Lbl1 = lbl1,
                        Lbl2 = lbl2
                    });
                    compiler_stack.Push(new ContEval() { Expr = k.E.ThenExpr, Env = k.Env });
                }
                else if (cont is ContMIf2)
                {
                    var k = (ContMIf2)cont;
                    il.Emit(OpCodes.Br, k.Lbl2);
                    il.MarkLabel(k.Lbl1);
                    compiler_stack.Push(new ContMIf3()
                    {
                        E = k.E,
                        Env = k.Env,
                        Lbl1 = k.Lbl1,
                        Lbl2 = k.Lbl2
                    });
                    compiler_stack.Push(new ContEval() { Expr = k.E.ElseExpr, Env = k.Env });
                }
                else if (cont is ContMIf3)
                {
                    var k = (ContMIf3)cont;
                    il.MarkLabel(k.Lbl2);
                }
                else if (cont is ContMatch1)
                {
                    var k = (ContMatch1)cont;
                    var fail_label = il.DefineLabel();
                    var lbl1 = il.DefineLabel();
                    var env2 = CompilePat(il, k.E.Pat, fail_label);
                    var env3 = env2.Append(k.Env);
                    compiler_stack.Push(new ContMatch2()
                    {
                        E = k.E,
                        Env = k.Env,
                        Lbl1 = lbl1,
                        FailLbl = fail_label,
                        Env3 = env3
                    });
                    compiler_stack.Push(new ContEval() { Expr = k.E.Guard, Env = env3 });
                }
                else if (cont is ContMatch2)
                {
                    var k = (ContMatch2)cont;
                    il.Emit(OpCodes.Unbox_Any, typeof(bool));
                    il.Emit(OpCodes.Brfalse, k.FailLbl);
                    compiler_stack.Push(new ContMatch3()
                    {
                        E = k.E,
                        Env = k.Env,
                        Lbl1 = k.Lbl1,
                        FailLbl = k.FailLbl,
                        Env3 = k.Env3
                    });
                    compiler_stack.Push(new ContEval() { Expr = k.E.ThenExpr, Env = k.Env3 });
                }
                else if (cont is ContMatch3)
                {
                    var k = (ContMatch3)cont;
                    il.Emit(OpCodes.Br, k.Lbl1);
                    il.MarkLabel(k.FailLbl);
                    compiler_stack.Push(new ContMatch4()
                    {
                        E = k.E,
                        Env = k.Env,
                        Lbl1 = k.Lbl1,
                        FailLbl = k.FailLbl,
                        Env3 = k.Env3
                    });
                    compiler_stack.Push(new ContEval() { Expr = k.E.ElseExpr, Env = k.Env });
                }
                else if (cont is ContMatch4)
                {
                    var k = (ContMatch4)cont;
                    il.MarkLabel(k.Lbl1);
                }
                else if (cont is ContMCons1)
                {
                    var k = (ContMCons1)cont;

                    il.Emit(OpCodes.Stelem_Ref);

                    il.Emit(OpCodes.Ldloc, k.Arr);
                    il.Emit(OpCodes.Ldc_I4_1);
                    compiler_stack.Push(new ContMCons2() { E = k.E, Env = k.Env, Arr = k.Arr });
                    compiler_stack.Push(new ContEval() { Expr = k.E.Tail, Env = k.Env });
                }
                else if (cont is ContMCons2)
                {
                    var k = (ContMCons2)cont;
                    il.Emit(OpCodes.Stelem_Ref);

                    il.Emit(OpCodes.Ldloc, k.Arr);
                    il.Emit(OpCodes.Stfld, _tag_args);
                }
                else if (cont is ContMTuple1)
                {
                    var k = (ContMTuple1)cont;
                    il.Emit(OpCodes.Stelem_Ref);
                    if (k.Idx < k.E.Size)
                    {
                        il.Emit(OpCodes.Ldloc, k.Arr);
                        il.Emit(OpCodes.Ldc_I4, k.Idx);
                        compiler_stack.Push(new ContMTuple1()
                        {
                            E = k.E,
                            Env = k.Env,
                            Arr = k.Arr,
                            Idx = k.Idx + 1
                        });
                        compiler_stack.Push(new ContEval() { Expr = k.E.Items[k.Idx], Env = k.Env });
                    }
                    else
                    {
                        il.Emit(OpCodes.Ldloc, k.Arr);
                    }
                }
                else if (cont is ContMDo1)
                {
                    var k = (ContMDo1)cont;
                    il.Emit(OpCodes.Pop);
                    compiler_stack.Push(new ContEval() { Expr = k.E.E2, Env = k.Env });
                }
                else if (cont is ContMLet1)
                {
                    var k = (ContMLet1)cont;
                    var fail_label = il.DefineLabel();
                    var lbl = il.DefineLabel();
                    var env2 = CompilePat(il, k.E.Pat, fail_label);

                    il.Emit(OpCodes.Br, lbl);
                    il.MarkLabel(fail_label);
                    il.Emit(OpCodes.Ldstr, k.E.Pos + ": パターンマッチ失敗");
                    il.Emit(OpCodes.Newobj, _application_exception);
                    il.Emit(OpCodes.Throw);

                    il.MarkLabel(lbl);

                    var env3 = env2.Append(k.Env);

                    compiler_stack.Push(new ContEval() { Expr = k.E.E2, Env = env3 });
                }
                else if (cont is ContMFun1)
                {
                    var k = (ContMFun1)cont;
                    il.Emit(OpCodes.Stfld, _value_ref_field);
                    if (k.Idx < k.Locals.Length)
                    {
                        il.Emit(OpCodes.Ldloc, k.Locals[k.Idx - 1]);
                        compiler_stack.Push(new ContMFun1()
                        {
                            E = k.E,
                            Env = k.Env,
                            Env2 = k.Env2,
                            Idx = k.Idx + 1,
                            Locals = k.Locals
                        });
                        compiler_stack.Push(new ContEval() { Expr = k.E.Items[k.Idx].Expr, Env = k.Env2 });
                    }
                    else
                    {
                        compiler_stack.Push(new ContEval() { Expr = k.E.E2, Env = k.Env2 });
                    }
                }
                else if (cont is ContMPrim1)
                {
                    var k = (ContMPrim1)cont;
                    if (k.Idx < k.E.Args.Count)
                    {
                        compiler_stack.Push(new ContMPrim1() { E = k.E, Env = k.Env, Idx = k.Idx + 1 });
                        compiler_stack.Push(new ContEval() { Expr = k.E.Args[k.Idx], Env = k.Env });
                    }
                    else
                    {
                        CompilePrim(il, k.E.Name, k.E.ArgTypes, k.E.RetType);
                    }
                }
                else if (cont is ContMMakeClos1)
                {
                    var k = (ContMMakeClos1)cont;

                    if (k.Idx != 0)
                    {
                        il.Emit(OpCodes.Stelem_Ref);
                    }

                    if (k.Idx < k.E.Args.Length)
                    {
                        il.Emit(OpCodes.Ldloc, k.Arr2);
                        il.Emit(OpCodes.Ldc_I4, k.Idx);
                        compiler_stack.Push(new ContMMakeClos1()
                        {
                            E = k.E,
                            Env = k.Env,
                            Arr1 = k.Arr1,
                            Arr2 = k.Arr2,
                            Idx = k.Idx + 1
                        });
                        compiler_stack.Push(new ContEval() { Expr = k.E.Args[k.Idx], Env = k.Env });
                    }
                    else
                    {
                        // できた配列を最初の配列の2要素目に代入
                        il.Emit(OpCodes.Ldloc, k.Arr1);
                        il.Emit(OpCodes.Ldc_I4_1);
                        il.Emit(OpCodes.Ldloc, k.Arr2);
                        il.Emit(OpCodes.Stelem_Ref);
                        // 2要素の配列をロード
                        il.Emit(OpCodes.Ldloc, k.Arr1);
                    }
                }
                else if (cont is ContMCallStatic1)
                {
                    var k = (ContMCallStatic1)cont;

                    var t = Typeinf.ReduceType(k.E.Types[k.Idx - 1]);
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

                    if (k.Idx < k.E.Args.Count)
                    {
                        compiler_stack.Push(new ContMCallStatic1()
                        {
                            E = k.E,
                            Env = k.Env,
                            Idx = k.Idx + 1
                        });
                        compiler_stack.Push(new ContEval() { Expr = k.E.Args[k.Idx], Env = k.Env });
                    }
                    else
                    {
                        il.Emit(OpCodes.Call, k.E.Info);

                        if (k.E.Info.ReturnType == typeof(void))
                        {
                            il.Emit(OpCodes.Ldc_I4_0);
                        }
                        else if (k.E.Info.ReturnType.IsValueType)
                        {
                            il.Emit(OpCodes.Box, k.E.Info.ReturnType);
                        }
                    }
                }
                else if (cont is ContMCast1)
                {
                    var k = (ContMCast1)cont;
                    il.Emit(OpCodes.Castclass, k.E.DstType);
                }
                else if (cont is ContMIsType1)
                {
                    var k = (ContMIsType1)cont;
                    var lbl1 = il.DefineLabel();
                    var lbl2 = il.DefineLabel();
                    il.Emit(OpCodes.Isinst, k.E.Type);
                    il.Emit(OpCodes.Brfalse, lbl1);
                    il.Emit(OpCodes.Ldc_I4_1);
                    il.Emit(OpCodes.Br, lbl2);
                    il.MarkLabel(lbl1);
                    il.Emit(OpCodes.Ldc_I4_0);
                    il.MarkLabel(lbl2);
                    il.Emit(OpCodes.Box, typeof(bool));
                }
                else if (cont is ContMNewClass1)
                {
                    var k = (ContMNewClass1)cont;

                    if (k.Idx != 0)
                    {
                        var t = Typeinf.ReduceType(k.E.Types[k.Idx - 1]);
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

                    if (k.Idx < k.E.Args.Count)
                    {
                        compiler_stack.Push(new ContMNewClass1()
                        {
                            E = k.E,
                            Env = k.Env,
                            Idx = k.Idx + 1
                        });
                        compiler_stack.Push(new ContEval() { Expr = k.E.Args[k.Idx], Env = k.Env });
                    }
                    else
                    {
                        il.Emit(OpCodes.Newobj, k.E.Info);

                        if (k.E.Info.DeclaringType.IsValueType)
                        {
                            il.Emit(OpCodes.Box, k.E.Info.DeclaringType);
                        }
                    }
                }
                else if (cont is ContMInvoke1)
                {
                    var k = (ContMInvoke1)cont;

                    var t = Typeinf.ReduceType(k.E.ExprType);

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

                    if (k.E.Args.Count > 0)
                    {
                        compiler_stack.Push(new ContMInvoke2()
                        {
                            E = k.E,
                            Env = k.Env,
                            Idx = 1,
                            T = t
                        });
                        compiler_stack.Push(new ContEval() { Expr = k.E.Args[0], Env = k.Env });
                    }
                    else
                    {
                        compiler_stack.Push(new ContMInvoke2()
                        {
                            E = k.E,
                            Env = k.Env,
                            Idx = 0,
                            T = t
                        });
                    }
                }
                else if (cont is ContMInvoke2)
                {
                    var k = (ContMInvoke2)cont;

                    if (k.E.Args.Count > 0)
                    {
                        var tt = Typeinf.ReduceType(k.E.Types[k.Idx - 1]);
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

                    if (k.Idx < k.E.Args.Count)
                    {
                        compiler_stack.Push(new ContMInvoke2()
                        {
                            E = k.E,
                            Env = k.Env,
                            Idx = k.Idx + 1,
                            T = k.T,
                        });
                        compiler_stack.Push(new ContEval() { Expr = k.E.Args[k.Idx], Env = k.Env });
                    }
                    else
                    {
                        var dotnet = TypeinfDotNet.MokkosuTypeToDotNetType("", k.T);
                        if (dotnet.IsValueType)
                        {
                            var loc = il.DeclareLocal(dotnet);
                            il.Emit(OpCodes.Stloc, loc);
                            il.Emit(OpCodes.Ldloca, loc);
                            il.Emit(OpCodes.Call, k.E.Info);
                        }
                        else
                        {
                            il.Emit(OpCodes.Callvirt, k.E.Info);
                        }

                        if (k.E.Info.ReturnType == typeof(void))
                        {
                            il.Emit(OpCodes.Ldc_I4_0);
                        }
                        else if (k.E.Info.ReturnType.IsValueType)
                        {
                            il.Emit(OpCodes.Box, k.E.Info.ReturnType);
                        }
                    }
                }
                else if (cont is ContMDelegate1)
                {
                    var k = (ContMDelegate1)cont;

                    var delil = k.Delil;

                    il.Emit(OpCodes.Stsfld, k.Field);

                    delil.Emit(OpCodes.Ldsfld, k.Field);
                    var fun = delil.DeclareLocal(typeof(object));
                    delil.Emit(OpCodes.Stloc, fun);

                    var ary = delil.DeclareLocal(typeof(object[]));
                    delil.Emit(OpCodes.Ldc_I4, k.E.ParamType.Length);
                    delil.Emit(OpCodes.Newarr, typeof(object));
                    delil.Emit(OpCodes.Stloc, ary);
                    for (int i = 0; i < k.E.ParamType.Length; i++)
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
                    il.Emit(OpCodes.Ldftn, k.Method);
                    il.Emit(OpCodes.Newobj, k.E.CstrInfo);
                }
                else if (cont is ContMSet1)
                {
                    var k = (ContMSet1)cont;

                    var t = Typeinf.ReduceType(k.E.ExprType);

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

                    compiler_stack.Push(new ContMSet2() { E = k.E, Env = k.Env });
                    compiler_stack.Push(new ContEval() { Expr = k.E.Arg, Env = k.Env });
                }
                else if (cont is ContMSet2)
                {
                    var k = (ContMSet2)cont;

                    var tt = Typeinf.ReduceType(k.E.ArgType);

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
                    else if (k.T is DotNetType)
                    {
                        var dotnettype = (DotNetType)tt;
                        if (dotnettype.Type.IsValueType)
                        {
                            il.Emit(OpCodes.Unbox_Any, dotnettype.Type);
                        }
                    }

                    il.Emit(OpCodes.Stfld, k.E.Info);

                    il.Emit(OpCodes.Ldc_I4_0);
                    il.Emit(OpCodes.Box, typeof(int));
                }
                else if (cont is ContMGet1)
                {
                    var k = (ContMGet1)cont;

                    var t = Typeinf.ReduceType(k.E.ExprType);

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

                    il.Emit(OpCodes.Ldfld, k.E.Info);

                    if (k.E.Info.FieldType.IsValueType)
                    {
                        il.Emit(OpCodes.Box, k.E.Info.FieldType);
                    }
                }
                else if (cont is ContMSSet1)
                {
                    var k = (ContMSSet1)cont;

                    var tt = Typeinf.ReduceType(k.E.ArgType);

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

                    il.Emit(OpCodes.Stsfld, k.E.Info);

                    il.Emit(OpCodes.Ldc_I4_0);
                    il.Emit(OpCodes.Box, typeof(int));
                }
                else
                {
                    throw new NotImplementedException();
                }
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
                return env2.Append(env1);
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
                    env = env.Tail;
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
                case "loadnull":
                    il.Emit(OpCodes.Ldnull);
                    break;
                case "intequal":
                    {
                        var loc1 = il.DeclareLocal(typeof(int));
                        il.Emit(OpCodes.Unbox_Any, typeof(int));
                        il.Emit(OpCodes.Stloc, loc1);
                        il.Emit(OpCodes.Unbox_Any, typeof(int));
                        il.Emit(OpCodes.Ldloc, loc1);
                        il.Emit(OpCodes.Beq, lbl1);
                        il.Emit(OpCodes.Ldc_I4_0);
                        il.Emit(OpCodes.Br, lbl2);
                        il.MarkLabel(lbl1);
                        il.Emit(OpCodes.Ldc_I4_1);
                        il.MarkLabel(lbl2);
                        il.Emit(OpCodes.Box, typeof(bool));
                    }
                    break;

                case "box":
                    // 何もしない
                    break;
                    
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
