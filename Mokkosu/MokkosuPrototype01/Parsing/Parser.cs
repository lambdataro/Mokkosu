using Mokkosu.AST;
using Mokkosu.Lexing;
using Mokkosu.Utils;
using System;
using System.Collections.Generic;

namespace Mokkosu.Parsing
{
    static class Parser
    {
        public static ParseResult Start(ParseContext ctx)
        {
            var top_exprs = new List<MTopExpr>();

            while (ctx.Tkn.Type != TokenType.EOF)
            {
                if (ctx.Tkn.Type == TokenType.TYPE)
                {
                    top_exprs.Add(ParseData(ctx));
                }
                else if (ctx.Tkn.Type == TokenType.DO)
                {
                    top_exprs.Add(ParseTopDo(ctx));
                }
                else if (ctx.Tkn.Type == TokenType.LET)
                {
                    top_exprs.Add(ParseTopLet(ctx));
                }
                else if (ctx.Tkn.Type == TokenType.FUN)
                {
                    top_exprs.Add(ParseTopFun(ctx));
                }
                else if (ctx.Tkn.Type == TokenType.INCLUDE)
                {
                    ctx.ReadToken(TokenType.INCLUDE);
                    var name = ctx.ReadStrToken(TokenType.STR);
                    ctx.ReadToken(TokenType.SC);
                    ctx.IncludeFile(name);
                }
                else
                {
                    ctx.SyntaxError();
                    throw new MError();
                }
            }

            return new ParseResult(top_exprs);
        }

        #region Data文

        //============================================================
        // Data文
        //============================================================

        static MUserTypeDef ParseData(ParseContext ctx)
        {
            var items = new List<MUserTypeDefItem>();

            ctx.ReadToken(TokenType.TYPE);

            items.Add(ParseDataItem(ctx));

            while (ctx.Tkn.Type == TokenType.AND)
            {
                ctx.ReadToken(TokenType.AND);
                items.Add(ParseDataItem(ctx));
            }

            ctx.ReadToken(TokenType.SC);

            return new MUserTypeDef(items);
        }

        static MUserTypeDefItem ParseDataItem(ParseContext ctx)
        {
            var name = ctx.ReadStrToken(TokenType.ID);
            var type_params = new List<string>();

            if (ctx.Tkn.Type == TokenType.LT)
            {
                ctx.ReadToken(TokenType.LT);
                type_params = ParseIdList(ctx);
                ctx.ReadToken(TokenType.GT);
            }

            ctx.ReadToken(TokenType.EQ);

            var tags = new List<TagDef>();
            tags.Add(ParseTagDef(ctx));

            while (ctx.Tkn.Type == TokenType.BAR)
            {
                ctx.ReadToken(TokenType.BAR);
                tags.Add(ParseTagDef(ctx));
            }

            return new MUserTypeDefItem(name, type_params, tags);
        }

        static TagDef ParseTagDef(ParseContext ctx)
        {
            var name = ctx.ReadStrToken(TokenType.ID);
            var args = new List<MType>();
            if (ctx.Tkn.Type == TokenType.LP)
            {
                ctx.ReadToken(TokenType.LP);
                args.Add(ParseType(ctx));
                while (ctx.Tkn.Type != TokenType.RP)
                {
                    ctx.ReadToken(TokenType.COM);
                    args.Add(ParseType(ctx));
                }
                ctx.ReadToken(TokenType.RP);
            }
            return new TagDef(name, args);
        }

        static List<string> ParseIdList(ParseContext ctx)
        {
            var list = new List<string>();
            list.Add(ctx.ReadStrToken(TokenType.ID));
            while (ctx.Tkn.Type == TokenType.COM)
            {
                ctx.ReadToken(TokenType.COM);
                list.Add(ctx.ReadStrToken(TokenType.ID));
            }
            return list;
        }

        static MType ParseType(ParseContext ctx)
        {
            var t1 = ParseTypeFactor(ctx);

            if (ctx.Tkn.Type == TokenType.ARROW)
            {
                ctx.ReadToken(TokenType.ARROW);
                var t2 = ParseType(ctx);
                t1 = new FunType(t1, t2);
            }

            return t1;
        }

        static MType ParseTypeFactor(ParseContext ctx)
        {
            if (ctx.Tkn.Type == TokenType.LP)
            {
                ctx.ReadToken(TokenType.LP);
                if (ctx.Tkn.Type == TokenType.RP)
                {
                    return new UnitType();
                }
                else
                {
                    var t = ParseType(ctx);
                    ctx.ReadToken(TokenType.RP);
                    return t;
                }
            }
            else if (ctx.Tkn.Type == TokenType.ID)
            {
                var name = ctx.ReadStrToken(TokenType.ID);
                switch (name)
                {
                    case "Int":
                        return new IntType();
                    case "Double":
                        return new DoubleType();
                    case "Char":
                        return new CharType();
                    case "String":
                        return new StringType();
                    default:
                        if (ctx.Tkn.Type == TokenType.LT)
                        {
                            ctx.ReadToken(TokenType.LT);
                            var args = ParseTypeList(ctx);
                            ctx.ReadToken(TokenType.GT);
                            return new UserType(name, args);
                        }
                        else
                        {
                            return new UserType(name);
                        }
                }
            }
            else
            {
                ctx.SyntaxError();
                throw new MError();
            }
        }

        static List<MType> ParseTypeList(ParseContext ctx)
        {
            var list = new List<MType>();
            list.Add(ParseType(ctx));
            while (ctx.Tkn.Type == TokenType.COM)
            {
                ctx.ReadToken(TokenType.COM);
                list.Add(ParseType(ctx));
            }
            return list;
        }

        #endregion

        #region do文

        //============================================================
        // do文
        //============================================================

        static MTopDo ParseTopDo(ParseContext ctx)
        {
            ctx.ReadToken(TokenType.DO);
            var expr = ParseExpr(ctx);
            ctx.ReadToken(TokenType.SC);
            return new MTopDo(expr);
        }

        #endregion

        #region let文

        //============================================================
        // let文
        //============================================================

        static MTopLet ParseTopLet(ParseContext ctx)
        {
            ctx.ReadToken(TokenType.LET);
            var pat = ParsePattern(ctx);
            ctx.ReadToken(TokenType.EQ);
            var expr = ParseExpr(ctx);
            ctx.ReadToken(TokenType.SC);
            return new MTopLet(pat, expr);
        }

        #endregion

        #region fun文

        //============================================================
        // fun文
        //============================================================

        static MTopFun ParseTopFun(ParseContext ctx)
        {
            ctx.ReadToken(TokenType.FUN);
            var items = new List<MTopFunItem>();
            items.Add(ParseTopFunItem(ctx));
            while (ctx.Tkn.Type == TokenType.AND)
            {
                ctx.ReadToken(TokenType.AND);
                items.Add(ParseTopFunItem(ctx));
            }
            ctx.ReadToken(TokenType.SC);
            return new MTopFun(items);
        }

        static MTopFunItem ParseTopFunItem(ParseContext ctx)
        {
            var name = ctx.ReadStrToken(TokenType.ID);
            ctx.ReadToken(TokenType.EQ);
            var expr = ParseExpr(ctx);
            return new MTopFunItem(name, expr);
        }


        #endregion

        #region 式

        //============================================================
        // 式
        //============================================================

        static MExpr ParseExpr(ParseContext ctx)
        {
            return ParseFunExpr(ctx);
        }

        static MExpr ParseFunExpr(ParseContext ctx)
        {
            if (ctx.Tkn.Type == TokenType.BS)
            {
                ctx.ReadToken(TokenType.BS);
                var arg_pat = ParsePatFactor(ctx);
                ctx.ReadToken(TokenType.ARROW);
                var body = ParseFunExpr(ctx);
                return new MLambda(arg_pat, body);
            }
            else if (ctx.Tkn.Type == TokenType.IF)
            {
                ctx.ReadToken(TokenType.IF);
                var cond_expr = ParseFunExpr(ctx);
                ctx.ReadToken(TokenType.ARROW);
                var then_expr = ParseFunExpr(ctx);
                ctx.ReadToken(TokenType.ELSE);
                var else_expr = ParseFunExpr(ctx);
                return new MIf(cond_expr, then_expr, else_expr);
            }
            else if (ctx.Tkn.Type == TokenType.PAT)
            {
                ctx.ReadToken(TokenType.PAT);
                var pat = ParsePattern(ctx);
                ctx.ReadToken(TokenType.EQ);
                var expr = ParseFunExpr(ctx);
                ctx.ReadToken(TokenType.ARROW);
                var then_expr = ParseFunExpr(ctx);
                ctx.ReadToken(TokenType.ELSE);
                var else_expr = ParseFunExpr(ctx);
                return new MMatch(pat, expr, then_expr, else_expr);
            }
            else
            {
                return ParseConsExpr(ctx);
            }
        }

        static MExpr CreateBinop(string name, MExpr lhs, MExpr rhs)
        {
            return new MApp(new MApp(new MVar(name), lhs), rhs);
        }

        static MExpr ParseConsExpr(ParseContext ctx)
        {
            var lhs = ParseAddExpr(ctx);

            while (ctx.Tkn.Type == TokenType.COLCOL)
            {
                var type = ctx.Tkn.Type;
                ctx.NextToken();
                var rhs = ParseAddExpr(ctx);
                switch (type)
                {
                    case TokenType.COLCOL:
                        lhs = new MCons(lhs, rhs);
                        break;
                }
            }
            return lhs;
        }

        static MExpr ParseAddExpr(ParseContext ctx)
        {
            var lhs = ParseMulExpr(ctx);

            while (ctx.Tkn.Type == TokenType.PLS || ctx.Tkn.Type == TokenType.MNS)
            {
                var type = ctx.Tkn.Type;
                ctx.NextToken();
                var rhs = ParseMulExpr(ctx);
                switch (type)
                {
                    case TokenType.PLS:
                        lhs = CreateBinop("__operator_pls", lhs, rhs);
                        break;
                    case TokenType.MNS:
                        lhs = CreateBinop("__operator_mns", lhs, rhs);
                        break;
                }
            }
            return lhs;
        }

        static MExpr ParseMulExpr(ParseContext ctx)
        {
            var lhs = ParseAppExpr(ctx);

            while (ctx.Tkn.Type == TokenType.AST || ctx.Tkn.Type == TokenType.SLS)
            {
                var type = ctx.Tkn.Type;
                ctx.NextToken();
                var rhs = ParseAppExpr(ctx);
                switch (type)
                {
                    case TokenType.AST:
                        lhs = CreateBinop("__operator_ast", lhs, rhs);
                        break;
                    case TokenType.SLS:
                        lhs = CreateBinop("__operator_sls", lhs, rhs);
                        break;
                }
            }
            return lhs;
        }

        static MExpr ParseAppExpr(ParseContext ctx)
        {
            var lhs = ParseFactor(ctx);

            while (ctx.Tkn.Type == TokenType.LP || ctx.Tkn.Type == TokenType.ID ||
                ctx.Tkn.Type == TokenType.INT || ctx.Tkn.Type == TokenType.DBL ||
                ctx.Tkn.Type == TokenType.STR || ctx.Tkn.Type == TokenType.CHAR)
            {
                var rhs = ParseFactor(ctx);
                lhs = new MApp(lhs, rhs);
            }
            return lhs;
        }

        static List<MExpr> ParseExprList(ParseContext ctx)
        {
            var list = new List<MExpr>();
            list.Add(ParseExpr(ctx));
            while (ctx.Tkn.Type == TokenType.COM)
            {
                ctx.ReadToken(TokenType.COM);
                list.Add(ParseExpr(ctx));
            }
            return list;
        }


        static MExpr ParseFactor(ParseContext ctx)
        {
            if (ctx.Tkn.Type == TokenType.LP)
            {
                ctx.ReadToken(TokenType.LP);
                if (ctx.Tkn.Type == TokenType.RP)
                {
                    ctx.ReadToken(TokenType.RP);
                    return new MUnit();
                }
                else
                {
                    var items = ParseExprList(ctx);
                    ctx.ReadToken(TokenType.RP);
                    if (items.Count == 1)
                    {
                        return items[0];
                    }
                    else
                    {
                        return new MTuple(items);
                    }
                }
            }
            else if (ctx.Tkn.Type == TokenType.ID)
            {
                var str = ctx.ReadStrToken(TokenType.ID);
                return new MVar(str);
            }
            else if (ctx.Tkn.Type == TokenType.INT)
            {
                var num = ctx.ReadIntToken(TokenType.INT);
                return new MInt(num);
            }
            else if (ctx.Tkn.Type == TokenType.DBL)
            {
                var num = ctx.ReadDoubleToken(TokenType.DBL);
                return new MDouble(num);
            }
            else if (ctx.Tkn.Type == TokenType.STR)
            {
                var str = ctx.ReadStrToken(TokenType.STR);
                return new MString(str);
            }
            else if (ctx.Tkn.Type == TokenType.CHAR)
            {
                var ch = ctx.ReadCharToken(TokenType.CHAR);
                return new MChar(ch);
            }
            else if (ctx.Tkn.Type == TokenType.TRUE)
            {
                ctx.ReadToken(TokenType.TRUE);
                return new MBool(true);
            }
            else if (ctx.Tkn.Type == TokenType.FALSE)
            {
                ctx.ReadToken(TokenType.FALSE);
                return new MBool(false);
            }
            else if (ctx.Tkn.Type == TokenType.LBK)
            {
                ctx.ReadToken(TokenType.LBK);
                ctx.ReadToken(TokenType.RBK);
                return new MNil();
            }
            else
            {
                ctx.SyntaxError();
                throw new MError();
            }
        }

        #endregion

        #region パターン

        //============================================================
        // パターン
        //============================================================

        static MPat ParsePattern(ParseContext ctx)
        {
            return ParseOrPat(ctx);
        }
        
        static MPat ParseOrPat(ParseContext ctx)
        {
            var lhs = ParseAsPat(ctx);

            while (ctx.Tkn.Type == TokenType.BAR)
            {
                ctx.ReadToken(TokenType.BAR);
                var rhs = ParseAsPat(ctx);
                lhs = new POr(lhs, rhs);
            }

            return lhs;
        }

        static MPat ParseAsPat(ParseContext ctx)
        {
            var pat = ParseConsPat(ctx);

            if (ctx.Tkn.Type == TokenType.AS)
            {
                ctx.ReadToken(TokenType.AS);
                var name = ctx.ReadStrToken(TokenType.ID);
                return new PAs(pat, name);
            }
            else
            {
                return pat;
            }
        }

        static MPat ParseConsPat(ParseContext ctx)
        {
            var lhs = ParsePatFactor(ctx);

            if (ctx.Tkn.Type == TokenType.COLCOL)
            {
                ctx.ReadToken(TokenType.COLCOL);
                var rhs = ParseConsPat(ctx);
                return new PCons(lhs, rhs);
            }
            else
            {
                return lhs;
            }
        }

        static MPat ParsePatFactor(ParseContext ctx)
        {
            if (ctx.Tkn.Type == TokenType.ID)
            {
                var str = ctx.ReadStrToken(TokenType.ID);
                return new PVar(str);
            }
            else if (ctx.Tkn.Type == TokenType.UB)
            {
                ctx.ReadToken(TokenType.UB);
                return new PWild();
            }
            else if (ctx.Tkn.Type == TokenType.INT)
            {
                var value = ctx.ReadIntToken(TokenType.INT);
                return new PInt(value);
            }
            else if (ctx.Tkn.Type == TokenType.DBL)
            {
                var value = ctx.ReadDoubleToken(TokenType.DBL);
                return new PDouble(value);
            }
            else if (ctx.Tkn.Type == TokenType.STR)
            {
                var value = ctx.ReadStrToken(TokenType.STR);
                return new PString(value);
            }
            else if (ctx.Tkn.Type == TokenType.CHAR)
            {
                var value = ctx.ReadCharToken(TokenType.CHAR);
                return new PChar(value);
            }
            else if (ctx.Tkn.Type == TokenType.LP)
            {
                ctx.ReadToken(TokenType.LP);
                if (ctx.Tkn.Type == TokenType.RP)
                {
                    return new PUnit();
                }
                else
                {
                    var pat_list = ParsePatList(ctx);
                    ctx.ReadToken(TokenType.RP);
                    if (pat_list.Count == 1)
                    {
                        return pat_list[0];
                    }
                    else
                    {
                        return new PTuple(pat_list);
                    }
                }
            }
            else if (ctx.Tkn.Type == TokenType.TRUE)
            {
                ctx.ReadToken(TokenType.TRUE);
                return new PBool(true);
            }
            else if (ctx.Tkn.Type == TokenType.FALSE)
            {
                ctx.ReadToken(TokenType.FALSE);
                return new PBool(false);
            }
            else if (ctx.Tkn.Type == TokenType.LBK)
            {
                ctx.ReadToken(TokenType.LBK);
                ctx.ReadToken(TokenType.RBK);
                return new PNil();
            }
            else
            {
                ctx.SyntaxError();
                throw new MError();
            }
        }

        static List<MPat> ParsePatList(ParseContext ctx)
        {
            var list = new List<MPat>();
            list.Add(ParsePattern(ctx));
            while (ctx.Tkn.Type == TokenType.COM)
            {
                ctx.ReadToken(TokenType.COM);
                list.Add(ParsePattern(ctx));
            }
            return list;
        }

        #endregion

    }
}
