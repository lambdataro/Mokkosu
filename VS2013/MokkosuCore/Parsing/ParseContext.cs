using Mokkosu.Lexing;
using Mokkosu.Utils;
using System;
using System.Collections.Generic;

namespace Mokkosu.Parsing
{
    class ParseContext
    {
        Lexer _lexer;
        HashSet<string> _tag_names;

        public Token Tkn { get; private set; }

        public ParseContext(Lexer lexer)
        {
            _lexer = lexer;
            _tag_names = new HashSet<string>();
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
                throw new MError(string.Format("{0}: 構文エラー ({1}が必要)",
                    _lexer.Pos, TypeToString(type)));
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

        public double ReadDoubleToken(TokenType type)
        {
            EnsureToken(type);
            var num = Tkn.DoubleVal;
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

        public char ReadCharToken(TokenType type)
        {
            EnsureToken(type);
            var ch = Tkn.CharVal;
            NextToken();
            return ch;
        }

        Dictionary<TokenType, string> _token_name = new Dictionary<TokenType, string>()
        {
            { TokenType.INT, "整数値" },
            { TokenType.DBL, "浮動小数点数値" },
            { TokenType.STR, "文字列値" },
            { TokenType.CHAR, "文字値" },
            { TokenType.ID, "識別子 "},
            { TokenType.COM, "(,)" },
            { TokenType.COL, "(:)" },
            { TokenType.BAR, "(|)" },
            { TokenType.SC, "(;)" },
            { TokenType.ARROW, "(->)" },
            { TokenType.AT, "(@)" },
            { TokenType.BS, "(\\)" },
            { TokenType.UB, "(_)" },
            { TokenType.QUE, "(?)" },
            { TokenType.BANG, "(!)" },
            { TokenType.HAT, "(^)"},
            { TokenType.LT, "(<)" },
            { TokenType.GT, "(>)" },
            { TokenType.EQ, "(=)" },
            { TokenType.LE, "(<=)" },
            { TokenType.GE, "(>=)" },
            { TokenType.EQEQ, "(==)" },
            { TokenType.LTGT, "(<>)" },
            { TokenType.LP, "開き括弧" },
            { TokenType.RP, "閉じ括弧" },
            { TokenType.PLS, "(+)"},
            { TokenType.MNS, "(-)"},
            { TokenType.AST, "(*)"},
            { TokenType.SLS, "(/)"},
            { TokenType.PER, "(%)"},
            { TokenType.PLSDOT, "(+.)"},
            { TokenType.MNSDOT, "(-.)"},
            { TokenType.ASTDOT, "(*.)"},
            { TokenType.SLSDOT, "(/.)"},
            { TokenType.LBK, "([)"},
            { TokenType.RBK, "(])"},
            { TokenType.COLCOL, "(::)"},
            { TokenType.AMPAMP, "(&&)"},
            { TokenType.BARBAR, "(||)"},
            { TokenType.AMP, "(&)"},
            { TokenType.ASTAST, "(**)"},
            { TokenType.LTLT, "(<<)"},
            { TokenType.GTGT, "(>>)"},
            { TokenType.PLSPLS, "(++)"},
            { TokenType.LTBAR, "(<|)"},
            { TokenType.BARGT, "(|>)"},
            { TokenType.COLEQ, "(:=)"},
            { TokenType.BQ, "(`)"},
            { TokenType.LBR, "({)" },
            { TokenType.RBR, "(})" },
            { TokenType.TILDA, "(~)" },
            { TokenType.DOT, "(.)" },
            { TokenType.DOTDOT, "(..)" },
            { TokenType.TILDAMNS, "(~-)" },
            { TokenType.TILDAMNSDOT, "(~-.)"},
            { TokenType.RARROW, "(<-)"},
            { TokenType.TYPE, "type" },
            { TokenType.AND, "and" },
            { TokenType.DO, "do" },
            { TokenType.IF, "if" },
            { TokenType.ELSE, "else" },
            { TokenType.PAT, "pat" },
            { TokenType.TRUE, "true" },
            { TokenType.FALSE, "false" },
            { TokenType.LET, "let" },
            { TokenType.FUN, "fun" },
            { TokenType.INCLUDE, "include" },
            { TokenType.AS, "as" },
            { TokenType.IN, "in" },
            { TokenType.PRIM, "__prim" },
            { TokenType.CALL, "call" },
            { TokenType.CAST, "cast" },
            { TokenType.IMPORT, "import" },
            { TokenType.NEW, "new" },
            { TokenType.GET, "get" },
            { TokenType.SET, "set" },
            { TokenType.SGET, "sget" },
            { TokenType.SSET, "sset" },
            { TokenType.DELEGATE, "delegate" },
            { TokenType.DEFINE, "__define" },
            { TokenType.UNDEFINE, "__undefine" },
            { TokenType.USING, "using" },
            { TokenType.FOR, "for" },
            { TokenType.END, "end" },
            { TokenType.ISTYPE, "istype" },
            { TokenType.NEWARR, "newarr" },
            { TokenType.LDELEM, "ldelem" },
            { TokenType.STELEM, "stelem" },
            { TokenType.TRY, "try" }
        };

        string TypeToString(TokenType type)
        {
            if (_token_name.ContainsKey(type))
            {
                return _token_name[type];
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public void SyntaxError()
        {
            throw new MError(string.Format("{0}: 構文エラー", _lexer.Pos));
        }

        public void IncludeFile(string name)
        {
            _lexer.IncludeFile(name);
        }
    }
}
