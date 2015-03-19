﻿namespace Mokkosu.Lexing
{
    enum TokenType
    {
        // 意味値を持つトークン
        INT, DBL, STR, CHAR, ID,
        // 記号
        COM, COL, BAR, SC, ARROW, 
        AT, BS, UB, QUE, BANG, HAT,
        LT, GT, EQ, LE, GE, EQEQ, LTGT,
        LP, RP, PLS, MNS, AST, SLS, PER, 
        PLSDOT, MNSDOT, ASTDOT, SLSDOT,
        LBK, RBK, COLCOL, AMPAMP, BARBAR,
        AMP, ASTAST, LTLT, GTGT, PLSPLS,
        LTBAR, BARGT, COLEQ, BQ,
        LBR, RBR, TILDA, DOT, DOTDOT,
        TILDAMNS, TILDAMNSDOT, RARROW,
        // ユーザ用オペレータ
        DOLL, GTGTEQ, EQLTLT, EQLT,
        LTASTGT, LTAST, ASTGT,
        LTPLSGT, LTPLS, PLSGT,
        LTMNSGT, LTBARGT,
        LTDOLLGT, LTASTASTGT, LTASTAST, ASTASTGT,
        BANGBANG, QUEQUE, ASTASTAST, AMPAMPAMP,
        PLSPLSPLS, BARBARBAR, MNSMNS, MNSMNSMNS,
        // キーワード
        TYPE, AND, DO, IF, ELSE, PAT,
        TRUE, FALSE, LET, FUN, INCLUDE,
        AS, IN, PRIM, CALL, CAST, IMPORT,
        NEW, GET, SET, SGET, SSET, DELEGATE,
        DEFINE, UNDEFINE, USING, FOR, END,
        ISTYPE, NEWARR, LDELEM, STELEM, TRY,
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
