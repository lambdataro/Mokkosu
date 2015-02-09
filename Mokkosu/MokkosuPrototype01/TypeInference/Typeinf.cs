using Mokkosu.AST;
using Mokkosu.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mokkosu.TypeInference
{
    static class Typeinf
    {
        /// <summary>
        /// 出現検査
        /// </summary>
        /// <param name="id">型変数ID</param>
        /// <param name="type">型</param>
        /// <returns>型に型変数IDが含まれていれば真そうでなければ偽</returns>
        static bool OccursCheck(int id, MType type)
        {
            if (type is TypeVar)
            {
                var t = (TypeVar)type;
                if (t.Id == id)
                {
                    return true;
                }
                else if (t.Value == null)
                {
                    return false;
                }
                else
                {
                    return OccursCheck(id, t.Value);
                }
            }
            else if (type is FunType)
            {
                var t = (FunType)type;
                return OccursCheck(id, t.ArgType) || OccursCheck(id, t.RetType);
            }
            else if (type is IntType || type is DoubleType ||
                type is StringType || type is CharType || type is UnitType)
            {
                return false;
            }
            else if (type is UserType)
            {
                var t = (UserType)type;
                bool b = false;
                foreach (var arg in t.Args)
                {
                    if (OccursCheck(id, arg))
                    {
                        b = true;
                        break;
                    }
                }
                return b;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// 単一化
        /// </summary>
        /// <param name="type1">型1</param>
        /// <param name="type2">型2</param>
        static void Unification(MType type1, MType type2)
        {
            if (type1 is TypeVar)
            {
                var t1 = (TypeVar)type1;
                if (type2 is TypeVar && t1.Id == ((TypeVar)type2).Id)
                {
                    return;
                }
                else if (t1.Value == null)
                {
                    if (OccursCheck(t1.Id, type2))
                    {
                        throw new MError("型エラー (出現違反)");
                    }
                    else
                    {
                        t1.Value = type2;
                    }
                }
                else
                {
                    Unification(t1.Value, type2);
                }
            }
            else if (type2 is TypeVar)
            {
                var t2 = (TypeVar)type2;
                if (t2.Value == null)
                {
                    if (OccursCheck(t2.Id, type1))
                    {
                        throw new MError("型エラー (出現違反)");
                    }
                    else
                    {
                        t2.Value = type1;
                    }
                }
                else
                {
                    Unification(t2.Value, type1);
                }
            }
            else if (type1 is UserType && type2 is UserType)
            {
                var t1 = (UserType)type1;
                var t2 = (UserType)type2;
                if (t1.Name == t2.Name && t1.Args.Count == t2.Args.Count)
                {
                    for (int i = 0; i < t1.Args.Count; i++)
                    {
                        Unification(t1.Args[i], t2.Args[i]);
                    }
                }
                else
                {
                    throw new MError("型エラー (単一化エラー)");
                }
            }
            else if (type1 is FunType && type2 is FunType)
            {
                var t1 = (FunType)type1;
                var t2 = (FunType)type2;
                Unification(t1.ArgType, t2.ArgType);
                Unification(t1.RetType, t2.RetType);
            }
            else if (type1 is IntType && type2 is IntType)
            {
                return;
            }
            else if (type1 is DoubleType && type2 is DoubleType)
            {
                return;
            }
            else if (type1 is StringType && type2 is StringType)
            {
                return;
            }
            else if (type1 is CharType && type2 is CharType)
            {
                return;
            }
            else if (type1 is UnitType && type2 is UnitType)
            {
                return;
            }
            else
            {
                throw new MError("型エラー (単一化エラー)");
            }
        }
    }
}
