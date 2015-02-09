using Mokkosu.Lexer;
using Mokkosu.Utils;
using System;

namespace Mokkosu.Parser
{
    class ParseContext
    {
        Lexer.Lexer _lexer;

        public Token Tkn { get; private set; }

        public ParseContext(Lexer.Lexer lexer)
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

        string TypeToString(TokenType type)
        {
            switch (type)
            {
                case TokenType.INT:
                    return "整数値";
                case TokenType.DBL:
                    return "浮動小数点数値";
                case TokenType.STR:
                    return "文字列";
                case TokenType.CHAR:
                    return "文字";
                case TokenType.ID:
                    return "識別子";
                case TokenType.EOF:
                    return "ファイル終端";
                default:
                    throw new NotImplementedException();
            }
        }

        public void SyntaxError()
        {
            throw new MError(string.Format("{0}: 構文エラー", _lexer.Pos));
        }
    }
}
