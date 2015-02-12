namespace Mokkosu.Lexing
{
    enum TokenType
    {
        // 意味値を持つトークン
        INT, DBL, STR, CHAR, ID,
        // 記号
        COM, COL, BAR, SC, ARROW, 
        AT, BS, UB, QUE,
        LT, GT, EQ, LE, GE, EQEQ, LTGT,
        LP, RP, PLS, MNS, AST, SLS,
        LBK, RBK, COLCOL,
        // キーワード
        TYPE, AND, DO, IF, ELSE, PAT,
        TRUE, FALSE, LET, FUN, INCLUDE,
        AS, IN, PRIM,
        // 制御記号
        EOF
    }

    class Token
    {
        public TokenType Type { get; private set; }
        public int IntVal { get; private set; }
        public char CharVal { get; private set; }
        public double DoubleVal { get; private set; }
        public string StrVal { get; private set; }
        public string Pos { get; private set; }

        public Token(string pos, TokenType type)
        {
            Type = type;
            Pos = pos;
        }

        public Token(string pos, TokenType type, int int_val)
            : this(pos, type)
        {
            IntVal = int_val;
        }

        public Token(string pos, TokenType type, char char_val)
            : this(pos, type)
        {
            CharVal = char_val;
        }

        public Token(string pos, TokenType type, double double_val)
            : this(pos, type)
        {
            DoubleVal = double_val;
        }

        public Token(string Pos, TokenType type, string str_val)
            : this(Pos, type)
        {
            StrVal = str_val;
        }
    }
}
