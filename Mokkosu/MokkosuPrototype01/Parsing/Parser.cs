﻿using Mokkosu.AST;
using Mokkosu.Lexing;
using Mokkosu.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mokkosu.Parsing
{
    static class Parser
    {
        static int _count = 0;

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
                    ctx.IncludeFile(name);
                    ctx.ReadToken(TokenType.SC);
                }
                else
                {
                    top_exprs.Add(ParseTopDoSimple(ctx));
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

            var pos = ctx.Tkn.Pos;
            ctx.ReadToken(TokenType.TYPE);

            items.Add(ParseDataItem(ctx));

            while (ctx.Tkn.Type == TokenType.AND)
            {
                ctx.ReadToken(TokenType.AND);
                items.Add(ParseDataItem(ctx));
            }

            ctx.ReadToken(TokenType.SC);

            return new MUserTypeDef(pos, items);
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
            var pos = ctx.Tkn.Pos;
            ctx.ReadToken(TokenType.DO);
            var expr = ParseExpr(ctx);
            ctx.ReadToken(TokenType.SC);
            return new MTopDo(pos, expr);
        }

        static MTopDo ParseTopDoSimple(ParseContext ctx)
        {
            var pos = ctx.Tkn.Pos;
            var expr = ParseExpr(ctx);
            ctx.ReadToken(TokenType.SC);
            return new MTopDo(pos, expr);
        }

        #endregion

        #region let文

        //============================================================
        // let文
        //============================================================

        static MTopLet ParseTopLet(ParseContext ctx)
        {
            var pos = ctx.Tkn.Pos;
            ctx.ReadToken(TokenType.LET);
            var pat = ParsePattern(ctx);
            var args = ParseArgList0(ctx, TokenType.EQ);
            ctx.ReadToken(TokenType.EQ);
            var expr = ParseExpr(ctx);
            ctx.ReadToken(TokenType.SC);
            return new MTopLet(pos, pat, ArgsToLambda(args, expr));
        }

        #endregion

        #region fun文

        //============================================================
        // fun文
        //============================================================

        static MTopFun ParseTopFun(ParseContext ctx)
        {
            var pos = ctx.Tkn.Pos;
            ctx.ReadToken(TokenType.FUN);
            var items = new List<MFunItem>();
            items.Add(ParseFunItem(ctx));
            while (ctx.Tkn.Type == TokenType.AND)
            {
                ctx.ReadToken(TokenType.AND);
                items.Add(ParseFunItem(ctx));
            }
            ctx.ReadToken(TokenType.SC);
            return new MTopFun(pos, items);
        }

        static MFunItem ParseFunItem(ParseContext ctx)
        {
            var name = ctx.ReadStrToken(TokenType.ID);
            var args = ParseArgList(ctx, TokenType.EQ);
            ctx.ReadToken(TokenType.EQ);
            var expr = ParseExpr(ctx);
            return new MFunItem(name, ArgsToLambda(args, expr));
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
                var args = ParseArgList(ctx, TokenType.ARROW);
                ctx.ReadToken(TokenType.ARROW);
                var body = ParseFunExpr(ctx);
                return ArgsToLambda(args, body);
            }
            else if (ctx.Tkn.Type == TokenType.IF)
            {
                var pos = ctx.Tkn.Pos;
                ctx.ReadToken(TokenType.IF);
                var cond_expr = ParseFunExpr(ctx);
                ctx.ReadToken(TokenType.ARROW);
                var then_expr = ParseFunExpr(ctx);
                ctx.ReadToken(TokenType.ELSE);
                var else_expr = ParseFunExpr(ctx);
                return new MIf(pos, cond_expr, then_expr, else_expr);
            }
            else if (ctx.Tkn.Type == TokenType.PAT)
            {
                var pos = ctx.Tkn.Pos;
                ctx.ReadToken(TokenType.PAT);
                var pat = ParsePattern(ctx);
                MExpr guard;
                if (ctx.Tkn.Type == TokenType.QUE)
                {
                    ctx.ReadToken(TokenType.QUE);
                    guard = ParseFunExpr(ctx);
                }
                else
                {
                    guard = new MBool(pos, true);
                }
                ctx.ReadToken(TokenType.EQ);
                var expr = ParseFunExpr(ctx);
                ctx.ReadToken(TokenType.ARROW);
                var then_expr = ParseFunExpr(ctx);
                ctx.ReadToken(TokenType.ELSE);
                var else_expr = ParseFunExpr(ctx);
                return new MMatch(pos, pat, guard, expr, then_expr, else_expr);
            }
            else if (ctx.Tkn.Type == TokenType.DO)
            {
                var pos = ctx.Tkn.Pos;
                ctx.ReadToken(TokenType.DO);
                var e1 = ParseFunExpr(ctx);
                ctx.ReadToken(TokenType.IN);
                var e2 = ParseFunExpr(ctx);
                return new MDo(pos, e1, e2);
            }
            else if (ctx.Tkn.Type == TokenType.LET)
            {
                var pos = ctx.Tkn.Pos;
                ctx.ReadToken(TokenType.LET);
                var pat = ParsePattern(ctx);
                var args = ParseArgList0(ctx, TokenType.EQ);
                ctx.ReadToken(TokenType.EQ);
                var e1 = ParseFunExpr(ctx);
                ctx.ReadToken(TokenType.IN);
                var e2 = ParseFunExpr(ctx);
                return new MLet(pos, pat, ArgsToLambda(args, e1), e2);
            }
            else if (ctx.Tkn.Type == TokenType.FUN)
            {
                var pos = ctx.Tkn.Pos;
                ctx.ReadToken(TokenType.FUN);
                var items = new List<MFunItem>();
                items.Add(ParseFunItem(ctx));
                while (ctx.Tkn.Type == TokenType.AND)
                {
                    ctx.ReadToken(TokenType.AND);
                    items.Add(ParseFunItem(ctx));
                }
                ctx.ReadToken(TokenType.IN);
                var e2 = ParseFunExpr(ctx);
                return new MFun(pos, items, e2);
            }
            else
            {
                return ParseAssignExpr(ctx);
            }
        }

        static List<MPat> ParseArgList(ParseContext ctx, TokenType end)
        {
            var list = new List<MPat>();
            list.Add(ParsePatFactor(ctx));
            while (ctx.Tkn.Type != end)
            {
                list.Add(ParsePatFactor(ctx));
            }
            return list;
        }

        static List<MPat> ParseArgList0(ParseContext ctx, TokenType end)
        {
            var list = new List<MPat>();
            while (ctx.Tkn.Type != end)
            {
                list.Add(ParsePatFactor(ctx));
            }
            return list;
        }

        static MExpr ArgsToLambda(List<MPat> args, MExpr body)
        {
            if (args.Count == 0)
            {
                return body;
            }
            else
            {
                var pat = args[0];
                var rest = args.Skip(1).ToList();
                return new MLambda(body.Pos, pat, ArgsToLambda(rest, body));
            }
        }

        static MExpr CreateBinop(string pos, string name, MExpr lhs, MExpr rhs)
        {
            return new MApp(pos, new MApp(pos, new MVar(pos, name), lhs), rhs);
        }

        static MExpr CreateUniop(string pos, string name, MExpr expr)
        {
            return new MApp(pos, new MVar(pos, name), expr);
        }

        static MExpr ParseAssignExpr(ParseContext ctx)
        {
            var lhs = ParseOpFunExpr(ctx);

            if (ctx.Tkn.Type == TokenType.COLEQ || ctx.Tkn.Type == TokenType.LTBAR || ctx.Tkn.Type == TokenType.BARGT)
            {
                var type = ctx.Tkn.Type;
                var pos = ctx.Tkn.Pos;
                ctx.NextToken();
                var rhs = ParseAssignExpr(ctx);
                switch (type)
                {
                    case TokenType.COLEQ:
                        lhs = CreateBinop(pos, "__operator_coleq", lhs, rhs);
                        break;

                    case TokenType.LTBAR:
                        lhs = CreateBinop(pos, "__operator_ltbar", lhs, rhs);
                        break;

                    case TokenType.BARGT:
                        lhs = CreateBinop(pos, "__operator_bargt", lhs, rhs);
                        break;
                }
            }
            return lhs;
        }

        static MExpr ParseOpFunExpr(ParseContext ctx)
        {
            var lhs = ParseOrExpr(ctx);

            while (ctx.Tkn.Type == TokenType.BQ)
            {
                var pos = ctx.Tkn.Pos;
                ctx.ReadToken(TokenType.BQ);
                var name = ctx.ReadStrToken(TokenType.ID);
                ctx.ReadToken(TokenType.BQ);
                var rhs = ParseOrExpr(ctx);
                lhs = CreateBinop(pos, name, lhs, rhs);
            }
            return lhs;
        }

        static MExpr ParseOrExpr(ParseContext ctx)
        {
            var lhs = ParseAndExpr(ctx);

            if (ctx.Tkn.Type == TokenType.BARBAR)
            {
                var type = ctx.Tkn.Type;
                var pos = ctx.Tkn.Pos;
                ctx.NextToken();
                var rhs = ParseOrExpr(ctx);
                switch (type)
                {
                    case TokenType.BARBAR:
                        lhs = new MIf(pos, lhs, new MBool(pos, true), rhs);
                        break;
                }
            }
            return lhs;
        }

        static MExpr ParseAndExpr(ParseContext ctx)
        {
            var lhs = ParseCmpExpr(ctx);

            if (ctx.Tkn.Type == TokenType.AMPAMP)
            {
                var type = ctx.Tkn.Type;
                var pos = ctx.Tkn.Pos;
                ctx.NextToken();
                var rhs = ParseAndExpr(ctx);
                switch (type)
                {
                    case TokenType.AMPAMP:
                        lhs = new MIf(pos, lhs, rhs, new MBool(pos, false));
                        break;
                }
            }
            return lhs;
        }

        static MExpr ParseCmpExpr(ParseContext ctx)
        {
            var lhs = ParseConsExpr(ctx);

            while (ctx.Tkn.Type == TokenType.LT || ctx.Tkn.Type == TokenType.GT ||
                ctx.Tkn.Type == TokenType.LE || ctx.Tkn.Type == TokenType.GE ||
                ctx.Tkn.Type == TokenType.EQEQ || ctx.Tkn.Type == TokenType.LTGT)
            {
                var type = ctx.Tkn.Type;
                var pos = ctx.Tkn.Pos;
                ctx.NextToken();
                var rhs = ParseConsExpr(ctx);
                switch (type)
                {
                    case TokenType.LT:
                        lhs = CreateBinop(pos, "__operator_lt", lhs, rhs);
                        break;
                    case TokenType.GT:
                        lhs = CreateBinop(pos, "__operator_gt", lhs, rhs);
                        break;
                    case TokenType.LE:
                        lhs = CreateBinop(pos, "__operator_le", lhs, rhs);
                        break;
                    case TokenType.GE:
                        lhs = CreateBinop(pos, "__operator_ge", lhs, rhs);
                        break;
                    case TokenType.EQEQ:
                        lhs = CreateBinop(pos, "__operator_eqeq", lhs, rhs);
                        break;
                    case TokenType.LTGT:
                        lhs = CreateBinop(pos, "__operator_ltgt", lhs, rhs);
                        break;
                }
            }
            return lhs;
        }

        static MExpr ParseConsExpr(ParseContext ctx)
        {
            var lhs = ParseAddExpr(ctx);

            if (ctx.Tkn.Type == TokenType.COLCOL || ctx.Tkn.Type == TokenType.PLSPLS || ctx.Tkn.Type == TokenType.HAT)
            {
                var type = ctx.Tkn.Type;
                var pos = ctx.Tkn.Pos;
                ctx.NextToken();
                var rhs = ParseConsExpr(ctx);
                switch (type)
                {
                    case TokenType.COLCOL:
                        lhs = new MCons(pos, lhs, rhs);
                        break;
                    case TokenType.PLSPLS:
                        lhs = CreateBinop(pos, "__operator_plspls", lhs, rhs);
                        break;
                    case TokenType.HAT:
                        lhs = CreateBinop(pos, "__operator_hat", lhs, rhs);
                        break;
                }
            }
            return lhs;
        }

        static MExpr ParseAddExpr(ParseContext ctx)
        {
            var lhs = ParseMulExpr(ctx);

            while (ctx.Tkn.Type == TokenType.PLS || ctx.Tkn.Type == TokenType.MNS ||
                ctx.Tkn.Type == TokenType.PLSDOT || ctx.Tkn.Type == TokenType.MNSDOT)
            {
                var type = ctx.Tkn.Type;
                var pos = ctx.Tkn.Pos;
                ctx.NextToken();
                var rhs = ParseMulExpr(ctx);
                switch (type)
                {
                    case TokenType.PLS:
                        lhs = CreateBinop(pos, "__operator_pls", lhs, rhs);
                        break;
                    case TokenType.MNS:
                        lhs = CreateBinop(pos, "__operator_mns", lhs, rhs);
                        break;
                    case TokenType.PLSDOT:
                        lhs = CreateBinop(pos, "__operator_plsdot", lhs, rhs);
                        break;
                    case TokenType.MNSDOT:
                        lhs = CreateBinop(pos, "__operator_mnsdot", lhs, rhs);
                        break;
                }
            }
            return lhs;
        }

        static MExpr ParseMulExpr(ParseContext ctx)
        {
            var lhs = ParseExpExpr(ctx);

            while (ctx.Tkn.Type == TokenType.AST || ctx.Tkn.Type == TokenType.SLS || ctx.Tkn.Type == TokenType.PER ||
                ctx.Tkn.Type == TokenType.ASTDOT || ctx.Tkn.Type == TokenType.SLSDOT)
            {
                var type = ctx.Tkn.Type;
                var pos = ctx.Tkn.Pos;
                ctx.NextToken();
                var rhs = ParseExpExpr(ctx);
                switch (type)
                {
                    case TokenType.AST:
                        lhs = CreateBinop(pos, "__operator_ast", lhs, rhs);
                        break;
                    case TokenType.SLS:
                        lhs = CreateBinop(pos, "__operator_sls", lhs, rhs);
                        break;
                    case TokenType.PER:
                        lhs = CreateBinop(pos, "__operator_per", lhs, rhs);
                        break;
                    case TokenType.SLSDOT:
                        lhs = CreateBinop(pos, "__operator_slsdot", lhs, rhs);
                        break;
                    case TokenType.ASTDOT:
                        lhs = CreateBinop(pos, "__operator_astdot", lhs, rhs);
                        break;
                }
            }
            return lhs;
        }

        static MExpr ParseExpExpr(ParseContext ctx)
        {
            var lhs = ParseAppExpr(ctx);
            
            if (ctx.Tkn.Type == TokenType.ASTAST || ctx.Tkn.Type == TokenType.LTLT || ctx.Tkn.Type == TokenType.GTGT)
            {
                var type = ctx.Tkn.Type;
                var pos = ctx.Tkn.Pos;
                ctx.NextToken();
                var rhs = ParseExpExpr(ctx);
                switch (type)
                {
                    case TokenType.ASTAST:
                        lhs = CreateBinop(pos, "__operator_astast", lhs, rhs);
                        break;
                    case TokenType.LTLT:
                        lhs = CreateBinop(pos, "__operator_ltlt", lhs, rhs);
                        break;
                    case TokenType.GTGT:
                        lhs = CreateBinop(pos, "__operator_gtgt", lhs, rhs);
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
                ctx.Tkn.Type == TokenType.STR || ctx.Tkn.Type == TokenType.CHAR ||
                ctx.Tkn.Type == TokenType.MNS || ctx.Tkn.Type == TokenType.MNSDOT ||
                ctx.Tkn.Type == TokenType.BANG || ctx.Tkn.Type == TokenType.LBR ||
                ctx.Tkn.Type == TokenType.LBK)
            {
                var pos = ctx.Tkn.Pos;
                var rhs = ParseFactor(ctx);
                lhs = new MApp(pos, lhs, rhs);
            }
            return lhs;
        }

        static MExpr ParseFactor(ParseContext ctx)
        {
            var pos = ctx.Tkn.Pos;
            if (ctx.Tkn.Type == TokenType.LP)
            {
                ctx.ReadToken(TokenType.LP);
                if (ctx.Tkn.Type == TokenType.RP)
                {
                    ctx.ReadToken(TokenType.RP);
                    return new MUnit(pos);
                }
                else
                {
                    var items = ParseExprList(ctx);
                    if (items.Count == 1)
                    {
                        if (ctx.Tkn.Type == TokenType.COL)
                        {
                            ctx.ReadToken(TokenType.COL);
                            var type = ParseType(ctx);
                            ctx.ReadToken(TokenType.RP);
                            return new MFource(pos, items[0], type);
                        }
                        else
                        {
                            ctx.ReadToken(TokenType.RP);
                            return items[0];
                        }
                    }
                    else
                    {
                        ctx.ReadToken(TokenType.RP);
                        return new MTuple(pos, items);
                    }
                }
            }
            else if (ctx.Tkn.Type == TokenType.MNS)
            {
                var e = ParseFactor(ctx);
                return CreateUniop(pos, "__operator_neg", e);
            }
            else if (ctx.Tkn.Type == TokenType.MNSDOT)
            {
                var e = ParseFactor(ctx);
                return CreateUniop(pos, "__operator_negdot", e);
            }
            else if (ctx.Tkn.Type == TokenType.BANG)
            {
                var e = ParseFactor(ctx);
                return CreateUniop(pos, "__operator_bang", e);
            }
            else if (ctx.Tkn.Type == TokenType.ID)
            {
                var str = ctx.ReadStrToken(TokenType.ID);
                return new MVar(pos, str);
            }
            else if (ctx.Tkn.Type == TokenType.INT)
            {
                var num = ctx.ReadIntToken(TokenType.INT);
                return new MInt(pos, num);
            }
            else if (ctx.Tkn.Type == TokenType.DBL)
            {
                var num = ctx.ReadDoubleToken(TokenType.DBL);
                return new MDouble(pos, num);
            }
            else if (ctx.Tkn.Type == TokenType.STR)
            {
                var str = ctx.ReadStrToken(TokenType.STR);
                return new MString(pos, str);
            }
            else if (ctx.Tkn.Type == TokenType.CHAR)
            {
                var ch = ctx.ReadCharToken(TokenType.CHAR);
                return new MChar(pos, ch);
            }
            else if (ctx.Tkn.Type == TokenType.TRUE)
            {
                ctx.ReadToken(TokenType.TRUE);
                return new MBool(pos, true);
            }
            else if (ctx.Tkn.Type == TokenType.FALSE)
            {
                ctx.ReadToken(TokenType.FALSE);
                return new MBool(pos, false);
            }
            else if (ctx.Tkn.Type == TokenType.LBK)
            {
                ctx.ReadToken(TokenType.LBK);
                if (ctx.Tkn.Type == TokenType.RBK)
                {
                    ctx.ReadToken(TokenType.RBK);
                    return new MNil(pos);
                }
                else
                {
                    var list = ParseExprList(ctx);
                    ctx.ReadToken(TokenType.RBK);
                    return ExprListToCons(pos, list);
                }
            }
            else if (ctx.Tkn.Type == TokenType.PRIM)
            {
                ctx.ReadToken(TokenType.PRIM);
                var name = ctx.ReadStrToken(TokenType.STR);
                ctx.ReadToken(TokenType.LP);
                var items = ParseExprList(ctx);
                ctx.ReadToken(TokenType.RP);
                return new MPrim(pos, name, items);
            }
            else if (ctx.Tkn.Type == TokenType.LBR)
            {
                return ParseBlock(ctx);
            }
            else
            {
                ctx.SyntaxError();
                throw new MError();
            }
        }

        static string GenVarName()
        {
            return string.Format("var@{0:000}", ++_count);
        }

        static MExpr ParseBlock(ParseContext ctx)
        {
            ctx.ReadToken(TokenType.LBR);
            var name = GenVarName();
            var pos = ctx.Tkn.Pos;
            var expr = new MLambda(pos, new PVar(pos, name), ParseBlockItem(name, ctx));
            ctx.ReadToken(TokenType.RBR);
            return expr;
        }

        static MExpr ParseBlockItem(string var_name, ParseContext ctx)
        {
            var start_pos = ctx.Tkn.Pos;
            if (ctx.Tkn.Type == TokenType.DO)
            {
                var pos = ctx.Tkn.Pos;
                ctx.ReadToken(TokenType.DO);
                var e1 = ParseExpr(ctx);
                ctx.ReadToken(TokenType.SC);
                var e2 = ParseBlockItem(var_name, ctx);
                return new MDo(pos, e1, e2);
            }
            else if (ctx.Tkn.Type == TokenType.LET)
            {
                var pos = ctx.Tkn.Pos;
                ctx.ReadToken(TokenType.LET);
                var pat = ParsePattern(ctx);
                var args = ParseArgList0(ctx, TokenType.EQ);
                ctx.ReadToken(TokenType.EQ);
                var e1 = ParseExpr(ctx);
                ctx.ReadToken(TokenType.SC);
                var e2 = ParseBlockItem(var_name, ctx);
                return new MLet(pos, pat, ArgsToLambda(args, e1), e2);
            }
            else if (ctx.Tkn.Type == TokenType.FUN)
            {
                var pos = ctx.Tkn.Pos;
                ctx.ReadToken(TokenType.FUN);
                var items = new List<MFunItem>();
                items.Add(ParseFunItem(ctx));
                while (ctx.Tkn.Type == TokenType.AND)
                {
                    ctx.ReadToken(TokenType.AND);
                    items.Add(ParseFunItem(ctx));
                }
                ctx.ReadToken(TokenType.SC);
                var e2 = ParseBlockItem(var_name, ctx);
                return new MFun(pos, items, e2);
            }
            else
            {
                var pat = ParsePattern(ctx);
                MExpr guard;
                if (ctx.Tkn.Type == TokenType.QUE)
                {
                    ctx.ReadToken(TokenType.QUE);
                    guard = ParseExpr(ctx);
                }
                else
                {
                    guard = new MBool(ctx.Tkn.Pos, true);
                }
                ctx.ReadToken(TokenType.ARROW);
                var then_expr = ParseExpr(ctx);
                MExpr else_expr;
                if (ctx.Tkn.Type == TokenType.RBR)
                {
                    else_expr = new MRuntimeError(ctx.Tkn.Pos, "パターンマッチ失敗");
                }
                else
                {
                    ctx.ReadToken(TokenType.SC);
                    else_expr = ParseBlockItem(var_name, ctx);
                }
                return new MMatch(start_pos, pat, guard, new MVar(var_name), then_expr, else_expr);
            }
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

        static MExpr ExprListToCons(string pos, List<MExpr> list)
        {
            MExpr ret_expr = new MNil(pos);
            list.Reverse();
            foreach (var expr in list)
            {
                ret_expr = new MCons(expr.Pos, expr, ret_expr);
            }
            return ret_expr;
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
                var pos = ctx.Tkn.Pos;
                ctx.ReadToken(TokenType.BAR);
                var rhs = ParseAsPat(ctx);
                lhs = new POr(pos, lhs, rhs);
            }

            return lhs;
        }

        static MPat ParseAsPat(ParseContext ctx)
        {
            var pat = ParseConsPat(ctx);

            if (ctx.Tkn.Type == TokenType.AS)
            {
                var pos = ctx.Tkn.Pos;
                ctx.ReadToken(TokenType.AS);
                var name = ctx.ReadStrToken(TokenType.ID);
                return new PAs(pos, pat, name);
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
                var pos = ctx.Tkn.Pos;
                ctx.ReadToken(TokenType.COLCOL);
                var rhs = ParseConsPat(ctx);
                return new PCons(pos, lhs, rhs);
            }
            else
            {
                return lhs;
            }
        }

        static MPat ParsePatFactor(ParseContext ctx)
        {
            var pos = ctx.Tkn.Pos;
            if (ctx.Tkn.Type == TokenType.ID)
            {
                var str = ctx.ReadStrToken(TokenType.ID);
                return new PVar(pos, str);
            }
            else if (ctx.Tkn.Type == TokenType.UB)
            {
                ctx.ReadToken(TokenType.UB);
                return new PWild(pos);
            }
            else if (ctx.Tkn.Type == TokenType.INT)
            {
                var value = ctx.ReadIntToken(TokenType.INT);
                return new PInt(pos, value);
            }
            else if (ctx.Tkn.Type == TokenType.DBL)
            {
                var value = ctx.ReadDoubleToken(TokenType.DBL);
                return new PDouble(pos, value);
            }
            else if (ctx.Tkn.Type == TokenType.STR)
            {
                var value = ctx.ReadStrToken(TokenType.STR);
                return new PString(pos, value);
            }
            else if (ctx.Tkn.Type == TokenType.CHAR)
            {
                var value = ctx.ReadCharToken(TokenType.CHAR);
                return new PChar(pos, value);
            }
            else if (ctx.Tkn.Type == TokenType.LP)
            {
                ctx.ReadToken(TokenType.LP);
                if (ctx.Tkn.Type == TokenType.RP)
                {
                    return new PUnit(pos);
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
                        return new PTuple(pos, pat_list);
                    }
                }
            }
            else if (ctx.Tkn.Type == TokenType.TRUE)
            {
                ctx.ReadToken(TokenType.TRUE);
                return new PBool(pos, true);
            }
            else if (ctx.Tkn.Type == TokenType.FALSE)
            {
                ctx.ReadToken(TokenType.FALSE);
                return new PBool(pos, false);
            }
            else if (ctx.Tkn.Type == TokenType.LBK)
            {
                ctx.ReadToken(TokenType.LBK);
                if (ctx.Tkn.Type == TokenType.RBK)
                {
                    ctx.ReadToken(TokenType.RBK);
                    return new PNil(pos);
                }
                else
                {
                    var list = ParsePatList(ctx);
                    ctx.ReadToken(TokenType.RBK);
                    return PatListToCons(pos, list);
                }
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

        static MPat PatListToCons(string pos, List<MPat> list)
        {
            MPat ret_pat = new PNil(pos);
            list.Reverse();
            foreach (var pat in list)
            {
                ret_pat = new PCons(pat.Pos, pat, ret_pat);
            }
            return ret_pat;
        }

        #endregion
    }
}
