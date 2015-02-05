namespace Mokkosu
{
    class ParseContext
    {
        Lexer _lexer;

        public Token Tkn { get; private set; }

        public ParseContext(Lexer lexer)
        {
            _lexer = lexer;
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

    static class Parser
    {
        public static SExpr Parse(ParseContext ctx)
        {
            return ParseExpr(ctx);
        }

        static SExpr ParseExpr(ParseContext ctx)
        {
            return ParseAddExpr(ctx);
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

            while (ctx.Tkn.Type == TokenType.PLS || ctx.Tkn.Type == TokenType.MNS)
            {
                var type = ctx.Tkn.Type;
                ctx.NextToken();
                var rhs = ParseAppExpr(ctx);
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

        static SExpr ParseAppExpr(ParseContext ctx)
        {
            var lhs = ParseFactor(ctx);

            while (ctx.Tkn.Type == TokenType.LP || ctx.Tkn.Type == TokenType.ID || ctx.Tkn.Type == TokenType.INT)
            {
                var rhs = ParseFactor(ctx);
                lhs = new SApp(lhs, rhs);
            }
            return lhs;
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
                return new SVar(name);
            }
            else
            {
                ctx.SyntaxError();
                return null;
            }
        }
    }
}
