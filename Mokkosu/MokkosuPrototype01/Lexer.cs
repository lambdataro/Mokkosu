using System.Collections.Generic;
using System.Text;

namespace Mokkosu
{
    enum TokenType
    {
        // 意味値をもつトークン
        ID, INT,
        // 記号
        LP, RP,
        PLS, MNS, AST, SLS,
        EQEQ,
        BS, ARROW, EQ,
        // キーワード
        PRINT, LET, IN,
        IF, THEN, ELSE,
        // 制御記号
        EOF
    }
    
    class Token
    {
        public TokenType Type { get; private set; }
        public int IntVal { get; private set; }
        public string StrVal { get; private set; }

        public Token(TokenType type)
        {
            Type = type;
        }

        public Token(TokenType type, int int_val)
            : this(type)
        {
            IntVal = int_val;
        }

        public Token(TokenType type, string str_val)
            : this(type)
        {
            StrVal = str_val;
        }
    }

    class Lexer
    {
        InputStream _strm;
        Dictionary<string, TokenType> keywords;
        Dictionary<char, TokenType> symbols;

        public Lexer(InputStream strm)
        {
            _strm = strm;
            strm.NextChar();
            InitKeywords();
            InitSymbols();
        }

        void InitSymbols()
        {
            keywords = new Dictionary<string, TokenType>()
            {
                { "print", TokenType.PRINT },
                { "let", TokenType.LET },
                { "in", TokenType.IN },
                { "if", TokenType.IF },
                { "then", TokenType.THEN },
                { "else", TokenType.ELSE },
            };
        }

        void InitKeywords()
        {
            symbols = new Dictionary<char, TokenType>()
            {
                { '(', TokenType.LP },
                { ')', TokenType.RP },
                { '+', TokenType.PLS },
                { '*', TokenType.AST },
                { '/', TokenType.SLS },
                { '\\', TokenType.BS },
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
            var sb = new StringBuilder();
            while (_strm.IsDigit())
            {
                sb.Append(_strm.Char);
                _strm.NextChar();
            }
            return new Token(TokenType.INT, int.Parse(sb.ToString()));
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
            if (keywords.ContainsKey(str))
            {
                return new Token(keywords[str]);
            }
            else
            {
                return new Token(TokenType.ID, str);
            }
        }

        Token NextSymbolToken()
        {
            if (symbols.ContainsKey(_strm.Char))
            {
                var type = symbols[_strm.Char];
                _strm.NextChar();
                return new Token(type);
            }
            else
            {
                switch (_strm.Char)
                {
                    case '-':
                        _strm.NextChar();
                        if (_strm.Char == '>')
                        {
                            _strm.NextChar();
                            return new Token(TokenType.ARROW);
                        }
                        else
                        {
                            return new Token(TokenType.MNS);
                        }

                    case '=':
                        _strm.NextChar();
                        if (_strm.Char == '=')
                        {
                            _strm.NextChar();
                            return new Token(TokenType.EQEQ);
                        }
                        else
                        {
                            return new Token(TokenType.EQ);
                        }

                    default:
                        throw new Error("構文エラー (不明な文字)");
                }
            }
        }
    }
}
