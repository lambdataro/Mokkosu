using Mokkosu.Input;
using Mokkosu.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mokkosu.Lexer
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
            };
        }

        void InitSymbols()
        {
            _symbols = new Dictionary<char, TokenType>()
            {
            };
        }

        public string Pos
        {
            get
            {
                return _strm.Pos;
            }
        }

        public Token NextToken()
        {
            _strm.SkipSpace();

            if (_strm.IsEof())
            {
                return new Token(TokenType.EOF);
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

            return new Token(TokenType.INT, num);
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
                return new Token(TokenType.DOUBLE, num);
            }
            else
            {
                var num = int.Parse(sb.ToString());
                return new Token(TokenType.INT, num);
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
            return new Token(TokenType.INT, num);
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
            return new Token(TokenType.INT, num);
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
            return new Token(TokenType.ID, str);
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
            else if (_strm.Char == '\'')
            {
                _strm.NextChar();
                var c = EscapeChar();
                if (_strm.Char != '\'')
                {
                    throw new MError(_strm.Pos + ": 文字定数が閉じていない");
                }
                _strm.NextChar();
                return new Token(TokenType.CHAR, c);
            }
            else if (_symbols.ContainsKey(_strm.Char))
            {
                var type = _symbols[_strm.Char];
                _strm.NextChar();
                return new Token(type);
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
            return new Token(TokenType.STR, sb.ToString());
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
