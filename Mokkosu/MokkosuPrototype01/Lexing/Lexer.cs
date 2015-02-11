using Mokkosu.Input;
using Mokkosu.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mokkosu.Lexing
{
    class Lexer
    {
        InputStream _strm;
        Dictionary<string, TokenType> _keywords;
        Dictionary<char, TokenType> _symbols;

        public Lexer(InputStream strm)
        {
            _strm = strm;
            _strm.NextChar();
            InitKeywords();
            InitSymbols();
        }

        void InitKeywords()
        {
            _keywords = new Dictionary<string, TokenType>()
            {
                { "type", TokenType.TYPE },
                { "and", TokenType.AND },
                { "do", TokenType.DO },
                { "if", TokenType.IF },
                { "else", TokenType.ELSE },
                { "pat", TokenType.PAT },
                { "true", TokenType.TRUE },
                { "false", TokenType.FALSE },
                { "let", TokenType.LET },
                { "fun", TokenType.FUN },
                { "include", TokenType.INCLUDE },
                { "as", TokenType.AS },
                { "in", TokenType.IN },
            };
        }

        void InitSymbols()
        {
            _symbols = new Dictionary<char, TokenType>()
            {
                { ',', TokenType.COM },
                { '|', TokenType.BAR },
                { ';', TokenType.SC },
                { '<', TokenType.LT },
                { '>', TokenType.GT },
                { '=', TokenType.EQ },
                { '(', TokenType.LP },
                { ')', TokenType.RP },
                { '\\', TokenType.BS },
                { '+', TokenType.PLS },
                { '*', TokenType.AST },
                { '/', TokenType.SLS },
                { '_', TokenType.UB },
                { '?', TokenType.QUE },
                { '[', TokenType.LBK },
                { ']', TokenType.RBK },
            };
        }

        public string Pos
        {
            get
            {
                return _strm.Pos;
            }
        }

        public void IncludeFile(string name)
        {
            _strm.IncludeSourceFile(name);
        }

        public Token NextToken()
        {
            _strm.SkipSpace();

            if (_strm.IsEof())
            {
                return new Token(Pos, TokenType.EOF);
            }
            else if (_strm.IsDigit())
            {
                return NextNumToken();
            }
            else if (_strm.IsIdStartChar())
            {
                return NextStrToken();
            }
            else
            {
                return NextSymbolToken();
            }
        }

        Token NextNumToken()
        {
            if (_strm.Char == '0')
            {
                _strm.NextChar();
                if (_strm.Char == 'x' || _strm.Char == 'X')
                {
                    _strm.NextChar();
                    return NextHexToken();
                }
                else if (_strm.Char == 'o' || _strm.Char == 'O')
                {
                    _strm.NextChar();
                    return NextOctToken();
                }
                else if (_strm.Char == 'b' || _strm.Char == 'B')
                {
                    _strm.NextChar();
                    return NextBinToken();
                }
                else
                {
                    return NextDecToken();
                }
            }
            else
            {
                return NextDecToken();
            }
        }

        Token NextHexToken()
        {
            var sb = new StringBuilder();
            while (_strm.IsHexDigit())
            {
                sb.Append(_strm.Char);
                _strm.NextChar();
            }
            var num = Convert.ToInt32(sb.ToString(), 16);

            return new Token(Pos, TokenType.INT, num);
        }

        Token NextDecToken()
        {
            var sb = new StringBuilder();
            bool is_double = false;
            while (_strm.IsDigit())
            {
                sb.Append(_strm.Char);
                _strm.NextChar();
            }
            if (_strm.Char == '.')
            {
                sb.Append('.');
                _strm.NextChar();
                while (_strm.IsDigit())
                {
                    sb.Append(_strm.Char);
                    _strm.NextChar();
                }
                is_double = true;
            }
            if (_strm.Char == 'e' || _strm.Char == 'E')
            {
                sb.Append('e');
                _strm.NextChar();
                while (_strm.IsDigit())
                {
                    sb.Append(_strm.Char);
                    _strm.NextChar();
                }
                is_double = true;
            }
            if (is_double)
            {
                var num = double.Parse(sb.ToString());
                return new Token(Pos, TokenType.DBL, num);
            }
            else
            {
                if (sb.ToString() == "")
                {
                    return new Token(Pos, TokenType.INT, 0);
                }
                else
                {
                    var num = int.Parse(sb.ToString());
                    return new Token(Pos, TokenType.INT, num);
                }
            }
        }

        Token NextOctToken()
        {
            var sb = new StringBuilder();
            while (_strm.IsOctDigit())
            {
                sb.Append(_strm.Char);
                _strm.NextChar();
            }
            var num = Convert.ToInt32(sb.ToString(), 8);
            return new Token(Pos, TokenType.INT, num);
        }

        Token NextBinToken()
        {
            var sb = new StringBuilder();
            while (_strm.IsBinDigit())
            {
                sb.Append(_strm.Char);
                _strm.NextChar();
            }
            var num = Convert.ToInt32(sb.ToString(), 2);
            return new Token(Pos, TokenType.INT, num);
        }

        Token NextStrToken()
        {
            var sb = new StringBuilder();
            while (_strm.IsIdChar())
            {
                sb.Append(_strm.Char);
                _strm.NextChar();
            }
            var str = sb.ToString();
            if (_keywords.ContainsKey(str))
            {
                return new Token(Pos, _keywords[str]);
            }
            else
            {
                return new Token(Pos, TokenType.ID, str);
            }
        }

        Token NextSymbolToken()
        {
            if (_strm.Char == '#')
            {
                while (!_strm.IsEof() && _strm.Char != '\n')
                {
                    _strm.NextChar();
                }
                return NextToken();
            }
            else if (_strm.Char == '\"')
            {
                _strm.NextChar();
                return NextStringLiteral();
            }
            else if (_strm.Char == '@')
            {
                _strm.NextChar();
                if (_strm.Char == '\"')
                {
                    _strm.NextChar();
                    return NextVerbatimStringLiteral();
                }
                else
                {
                    return new Token(Pos, TokenType.AT);
                }
            }
            else if (_strm.Char == '\'')
            {
                _strm.NextChar();
                char ch;
                if (_strm.Char == '\\')
                {
                    _strm.NextChar();
                    ch = EscapeChar();
                    _strm.NextChar();
                }
                else
                {
                    ch = _strm.Char;
                    _strm.NextChar();
                }
                if (_strm.Char != '\'')
                {
                    throw new MError(_strm.Pos + ": 文字定数が閉じていない");
                }
                _strm.NextChar();
                return new Token(Pos, TokenType.CHAR, ch);                
            }
            else if (_strm.Char == '-')
            {
                _strm.NextChar();
                if (_strm.Char == '>')
                {
                    _strm.NextChar();
                    return new Token(Pos, TokenType.ARROW);
                }
                else
                {
                    return new Token(Pos, TokenType.MNS);
                }
            }
            else if (_strm.Char == ':')
            {
                _strm.NextChar();
                if (_strm.Char == ':')
                {
                    _strm.NextChar();
                    return new Token(Pos, TokenType.COLCOL);
                }
                else
                {
                    return new Token(Pos, TokenType.COL);
                }
            }
            else if (_symbols.ContainsKey(_strm.Char))
            {
                var type = _symbols[_strm.Char];
                _strm.NextChar();
                return new Token(Pos, type);
            }
            else
            {
                throw new MError(_strm.Pos + ": 構文エラー (不明な文字)");
            }
        }

        Token NextStringLiteral()
        {
            var sb = new StringBuilder();
            while (_strm.Char != '\"')
            {
                if (_strm.Char == '\\')
                {
                    _strm.NextChar();
                    sb.Append(EscapeChar());
                }
                else
                {
                    sb.Append(_strm.Char);
                }
                _strm.NextChar();
            }
            _strm.NextChar();
            return new Token(Pos, TokenType.STR, sb.ToString());
        }

        Token NextVerbatimStringLiteral()
        {
            var sb = new StringBuilder();
            while (_strm.Char != '\"')
            {
                sb.Append(_strm.Char);
                _strm.NextChar();
            }
            _strm.NextChar();
            return new Token(Pos, TokenType.STR, sb.ToString());
        }

        char EscapeChar()
        {
            switch (_strm.Char)
            {
                case '\'':
                    return '\'';
                case '\"':
                    return '\"';
                case '\\':
                    return '\\';
                case '0':
                    return '\0';
                case 'a':
                    return '\a';
                case 'b':
                    return '\b';
                case 'f':
                    return '\f';
                case 'n':
                    return '\n';
                case 'r':
                    return '\r';
                case 't':
                    return '\t';
                case 'v':
                    return '\v';
                case 'u':
                    return ReadUnicode();
                default:
                    throw new MError(_strm.Pos + ": エスケープシーケンスが不正");
            }
        }

        char ReadUnicode()
        {
            _strm.NextChar();
            var sb = new StringBuilder();
            for (int i = 0; i < 4; i++)
            {
                if (!_strm.IsHexDigit())
                {
                    throw new MError(_strm.Pos + ": Unicodeエスケープシーケンスが不正");
                }
                sb.Append(_strm.Char);
            }
            var int_char = Convert.ToUInt32(sb.ToString(), 16);
            return Convert.ToChar(int_char);
        }
    }
}
