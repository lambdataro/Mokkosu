using Mokkosu.AST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Mokkosu.Utils;

namespace Mokkosu.CodeGenerate
{
    using LEnv = MEnv<LocalBuilder>;

    abstract class CodeGeneratorCont
    {
    }
    
    class ContEval : CodeGeneratorCont
    {
        public MExpr Expr { get; set; }
        public LEnv Env { get; set; }
    }

    abstract class ContApply : CodeGeneratorCont
    {
    }

    class ContMApp1 : ContApply
    {
        public MApp E { get; set; }
        public LEnv Env { get; set; }
    }

    class ContMApp2 : ContApply
    {
        public MApp E { get; set; }
        public LEnv Env { get; set; }
        public LocalBuilder Ary { get; set; }
        public Label Lbl1 { get; set; }
        public Label Lbl2 { get; set; }
    }

    class ContMApp3 : ContApply
    {
        public MApp E { get; set; }
        public LEnv Env { get; set; }
        public LocalBuilder Ary { get; set; }
        public Label Lbl1 { get; set; }
        public Label Lbl2 { get; set; }
    }

    class ContMIf1 : ContApply
    {
        public MIf E { get; set; }
        public LEnv Env { get; set; }
    }

    class ContMIf2 : ContApply
    {
        public MIf E { get; set; }
        public LEnv Env { get; set; }
        public Label Lbl1 { get; set; }
        public Label Lbl2 { get; set; }
    }

    class ContMIf3 : ContApply
    {
        public MIf E { get; set; }
        public LEnv Env { get; set; }
        public Label Lbl1 { get; set; }
        public Label Lbl2 { get; set; }
    }

    class ContMatch1 : ContApply
    {
        public MMatch E { get; set; }
        public LEnv Env { get; set; }
    }

    class ContMatch2 : ContApply
    {
        public MMatch E { get; set; }
        public LEnv Env { get; set; }
        public Label Lbl1 { get; set; }
        public Label FailLbl { get; set; }
        public LEnv Env3 { get; set; }
    }

    class ContMatch3 : ContApply
    {
        public MMatch E { get; set; }
        public LEnv Env { get; set; }
        public Label Lbl1 { get; set; }
        public Label FailLbl { get; set; }
        public LEnv Env3 { get; set; }
    }

    class ContMatch4 : ContApply
    {
        public MMatch E { get; set; }
        public LEnv Env { get; set; }
        public Label Lbl1 { get; set; }
        public Label FailLbl { get; set; }
        public LEnv Env3 { get; set; }
    }

    class ContMCons1 : ContApply
    {
        public MCons E { get; set; }
        public LEnv Env { get; set; }
        public LocalBuilder Arr { get; set; }
    }

    class ContMCons2 : ContApply
    {
        public MCons E { get; set; }
        public LEnv Env { get; set; }
        public LocalBuilder Arr { get; set; }
    }

    class ContMTuple1 : ContApply
    {
        public MTuple E { get; set; }
        public LEnv Env { get; set; }
        public LocalBuilder Arr { get; set; }
        public int Idx { get; set; }
    }

    class ContMDo1 : ContApply
    {
        public MDo E { get; set; }
        public LEnv Env { get; set; }
    }

    class ContMLet1 : ContApply
    {
        public MLet E { get; set; }
        public LEnv Env { get; set; }
    }

    class ContMFun1 : ContApply
    {
        public MFun E { get; set; }
        public LEnv Env { get; set; }
        public LEnv Env2 { get; set; }
        public int Idx { get; set; }
        public LocalBuilder[] Locals { get; set; }
    }

    class ContMPrim1 : ContApply
    {
        public MPrim E { get; set; }
        public LEnv Env { get; set; }
        public int Idx { get; set; }
    }

    class ContMMakeClos1 : ContApply
    {
        public MMakeClos E { get; set; }
        public LEnv Env { get; set; }
        public int Idx { get; set; }
        public LocalBuilder Arr1 { get; set; }
        public LocalBuilder Arr2 { get; set; }
    }

    class ContMCallStatic1 : ContApply
    {
        public MCallStatic E { get; set; }
        public LEnv Env { get; set; }
        public int Idx { get; set; }
    }

    class ContMCast1 : ContApply
    {
        public MCast E { get; set; }
        public LEnv Env { get; set; }
    }

    class ContMIsType1 : ContApply
    {
        public MIsType E { get; set; }
        public LEnv Env { get; set; }
    }

    class ContMNewClass1 : ContApply
    {
        public MNewClass E { get; set; }
        public LEnv Env { get; set; }
        public int Idx { get; set; }
    }

    class ContMInvoke1 : ContApply
    {
        public MInvoke E { get; set; }
        public LEnv Env { get; set; }
    }

    class ContMInvoke2 : ContApply
    {
        public MInvoke E { get; set; }
        public LEnv Env { get; set; }
        public int Idx { get; set; }
        public MType T { get; set; }
    }

    class ContMDelegate1 : ContApply
    {
        public MDelegate E { get; set; }
        public LEnv Env { get; set; }
        public MethodBuilder Method { get; set; }
        public ILGenerator Delil { get; set; }
        public FieldBuilder Field { get; set; }
    }

    class ContMSet1 : ContApply
    {
        public MSet E { get; set; }
        public LEnv Env { get; set; }
    }

    class ContMSet2 : ContApply
    {
        public MSet E { get; set; }
        public LEnv Env { get; set; }
        public MType T { get; set; }
    }

    class ContMGet1 : ContApply
    {
        public MGet E { get; set; }
        public LEnv Env { get; set; }
    }

    class ContMSSet1 : ContApply
    {
        public MSSet E { get; set; }
        public LEnv Env { get; set; }
    }
}

