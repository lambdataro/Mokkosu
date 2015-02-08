namespace Mokkosu.Lexer
{
    enum TokenType
    {
        // 意味値を持つトークン
        INT,
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
}
