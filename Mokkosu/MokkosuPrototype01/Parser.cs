using System.Collections.Generic;
using System.Linq;

namespace Mokkosu
{
    class ParseContext
    {
        Lexer _lexer;

        public Token Tkn { get; private set; }
        public List<string> Tags { get; set; } 

        public ParseContext(Lexer lexer)
        {
            _lexer = lexer;
            Tags = new List<string>();
            NextToken();
        }

        public void NextToken()
        {
            Tkn = _lexer.NextToken();
        }

        void EnsureToken(TokenType type)
        {
            if (Tkn.Type != type)
            {
                SyntaxError();
            }
        }

        public void ReadToken(TokenType type)
        {
            EnsureToken(type);
            NextToken();
        }

        public int ReadIntToken(TokenType type)
        {
            EnsureToken(type);
            var num = Tkn.IntVal;
            NextToken();
            return num;
        }

        public string ReadStrToken(TokenType type)
        {
            EnsureToken(type);
            var str = Tkn.StrVal;
            NextToken();
            return str;
        }

        public void SyntaxError()
        {
            throw new Error(string.Format("{0}: 構文エラー", _lexer.Pos));
        }
    }

    class Tag
    {
        public string Name { get; private set; }
        public Type Type { get; private set; }
        public List<Type> Args { get; private set; }

        public Tag(string name, Type type, List<Type> args)
        {
            Name = name;
            Type = type;
            Args = args;
        }
    }

    class ParseResult
    {
        public List<string> UserDefinedTypes { get; private set; }
        public List<Tag> Tags { get; private set; }
        public SExpr Main { get; private set; }

        public ParseResult(List<string> user_defined_types, List<Tag> tags, SExpr main)
        {
            UserDefinedTypes = user_defined_types;
            Tags = tags;
            Main = main;
        }
    }

    static class Parser
    {
        public static ParseResult Parse(ParseContext ctx)
        {
            var user_defined_type = new List<string>();
            var tags = new List<Tag>();
            ParseData(ctx, out user_defined_type, out tags);
            ctx.Tags = TagNames(tags);
            var main = ParseExpr(ctx);
            ctx.ReadToken(TokenType.EOF);
            return new ParseResult(user_defined_type, tags, main);
        }

        static List<string> TagNames(List<Tag> tags)
        {
            return tags.Select<Tag, string>(tag => tag.Name).ToList<string>();
        }

        static void ParseData(ParseContext ctx, out List<string> types_out, out List<Tag> tags_out)
        {
            var types = new List<string>();
            var tags = new List<Tag>();

            while (ctx.Tkn.Type == TokenType.DATA)
            {
                ctx.ReadToken(TokenType.DATA);
                var name = ctx.ReadStrToken(TokenType.ID);
                ctx.ReadToken(TokenType.EQ);
                types.Add(name);
                while (true)
                {
                    var tag = ParseDataLine(ctx, name);
                    tags.Add(tag);
                    if (ctx.Tkn.Type == TokenType.SC)
                    {
                        ctx.ReadToken(TokenType.SC);
                        break;
                    }
                    ctx.ReadToken(TokenType.BAR);
                }
            }

            types_out = types;
            tags_out = tags;
        }

        static Tag ParseDataLine(ParseContext ctx, string type_name)
        {
            var name = ctx.ReadStrToken(TokenType.ID);
            var args = new List<Type>();
            while (ctx.Tkn.Type != TokenType.BAR && ctx.Tkn.Type != TokenType.SC)
            {
                var type = ParseTypeFactor(ctx);
                args.Add(type);
            }
            return new Tag(name, new UserType(type_name), args);
        }

        static Type ParseTypeFactor(ParseContext ctx)
        {
            if (ctx.Tkn.Type == TokenType.LP)
            {
                var type = ParseTypeTerm(ctx);
                ctx.ReadToken(TokenType.RP);
                return type;
            }
            else if (ctx.Tkn.Type == TokenType.ID)
            {
                var id = ctx.ReadStrToken(TokenType.ID); 
                switch (id)
                {
                    case "Int":
                        return new IntType();
                    default:
                        return new UserType(id);
                }
            }
            else
            {
                throw new Error("構文エラー");
            }
        }

        static Type ParseTypeTerm(ParseContext ctx)
        {
            var t1 = ParseTypeFactor(ctx);
            if (ctx.Tkn.Type == TokenType.ARROW)
            {
                var t2 = ParseTypeTerm(ctx);
                return new FunType(t1, t2);
            }
            else
            {
                return t1;
            }
        }

        static SExpr ParseExpr(ParseContext ctx)
        {
            return ParseFunExpr(ctx);
        }

        static SExpr ParseFunExpr(ParseContext ctx)
        {
            if (ctx.Tkn.Type == TokenType.BS)
            {
                ctx.ReadToken(TokenType.BS);
                var arg_name = ctx.ReadStrToken(TokenType.ID);
                ctx.ReadToken(TokenType.ARROW);
                var body = ParseFunExpr(ctx);
                return new SFun(arg_name, body);
            }
            else if (ctx.Tkn.Type == TokenType.IF)
            {
                ctx.ReadToken(TokenType.IF);
                var cond_expr = ParseFunExpr(ctx);
                ctx.ReadToken(TokenType.THEN);
                var then_expr = ParseFunExpr(ctx);
                ctx.ReadToken(TokenType.ELSE);
                var else_expr = ParseFunExpr(ctx);
                return new SIf(cond_expr, then_expr, else_expr);
            }
            else if (ctx.Tkn.Type == TokenType.LET)
            {
                ctx.ReadToken(TokenType.LET);
                var var_name = ctx.ReadStrToken(TokenType.ID);
                ctx.ReadToken(TokenType.EQ);
                var e1 = ParseFunExpr(ctx);
                ctx.ReadToken(TokenType.IN);
                var e2 = ParseFunExpr(ctx);
                return new SLet(var_name, e1, e2);
            }
            else if (ctx.Tkn.Type == TokenType.REC)
            {
                ctx.ReadToken(TokenType.REC);
                var var_name = ctx.ReadStrToken(TokenType.ID);
                ctx.ReadToken(TokenType.EQ);
                var e1 = ParseFunExpr(ctx);
                ctx.ReadToken(TokenType.IN);
                var e2 = ParseFunExpr(ctx);
                return new SRec(var_name, e1, e2);
            }
            else
            {
                return ParseCmpExpr(ctx);
            }
        }

        static SExpr ParseCmpExpr(ParseContext ctx)
        {
            var lhs = ParseAddExpr(ctx);

            while (ctx.Tkn.Type == TokenType.EQEQ)
            {
                var type = ctx.Tkn.Type;
                ctx.NextToken();
                var rhs = ParseAddExpr(ctx);
                switch (type)
                {
                    case TokenType.EQEQ:
                        lhs = new SEq(lhs, rhs);
                        break;
                }
            }
            return lhs;
        }

        static SExpr ParseAddExpr(ParseContext ctx)
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
                        lhs = new SAdd(lhs, rhs);
                        break;
                    case TokenType.MNS:
                        lhs = new SSub(lhs, rhs);
                        break;
                }
            }
            return lhs;
        }

        static SExpr ParseMulExpr(ParseContext ctx)
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
                        lhs = new SMul(lhs, rhs);
                        break;
                    case TokenType.SLS:
                        lhs = new SDiv(lhs, rhs);
                        break;
                }
            }
            return lhs;
        }

        static SExpr ParseAppExpr(ParseContext ctx)
        {
            if (ctx.Tkn.Type == TokenType.PRINT)
            {
                ctx.ReadToken(TokenType.PRINT);
                var arg = ParseFactor(ctx);
                return new SPrint(arg);
            }
            else
            {
                var lhs = ParseFactor(ctx);

                while (ctx.Tkn.Type == TokenType.LP || ctx.Tkn.Type == TokenType.ID || ctx.Tkn.Type == TokenType.INT)
                {
                    if (ctx.Tkn.Type == TokenType.PRINT)
                    {
                        ctx.ReadToken(TokenType.PRINT);
                        var arg = ParseFactor(ctx);
                        lhs = new SPrint(arg);
                    }
                    else
                    {
                        var rhs = ParseFactor(ctx);
                        lhs = new SApp(lhs, rhs);
                    }
                }
                return lhs;
            }
        }

        static SExpr ParseFactor(ParseContext ctx)
        {
            if (ctx.Tkn.Type == TokenType.LP)
            {
                ctx.ReadToken(TokenType.LP);
                var expr = ParseExpr(ctx);
                ctx.ReadToken(TokenType.RP);
                return expr;
            }
            else if (ctx.Tkn.Type == TokenType.INT)
            {
                var value = ctx.ReadIntToken(TokenType.INT);
                return new SConstInt(value);
            }
            else if (ctx.Tkn.Type == TokenType.ID)
            {
                var name = ctx.ReadStrToken(TokenType.ID);
                if (ctx.Tags.Contains(name))
                {
                    return new STag(name);
                }
                else
                {
                    return new SVar(name);
                }
            }
            else
            {
                ctx.SyntaxError();
                return null;
            }
        }
    }
}
