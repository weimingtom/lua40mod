/*
** $Id: llex.h,v 1.31 2000/09/27 17:41:58 roberto Exp $
** Lexical Analyzer
** See Copyright Notice in lua.h
*/

//#ifndef llex_h
//#define llex_h

//#include "lobject.h"
//#include "lzio.h"



namespace lua40mod
{
	using Number = System.Double;
	
	public partial class Lua
	{
		public const int FIRST_RESERVED	= 257;

		/* maximum length of a reserved word (+1 for final 0) */
		public const int TOKEN_LEN	= 15;


		/*
		* WARNING: if you change the order of this enumeration,
		* grep "ORDER RESERVED"
		*/
		public enum RESERVED {
		  /* terminal symbols denoted by reserved words */
		  TK_AND = FIRST_RESERVED, TK_BREAK,
          TK_DO, TK_ELSE, TK_ELSEIF, TK_END, TK_FOR, TK_FUNCTION, TK_IF, TK_LOCAL,
          TK_NIL, TK_NOT, TK_OR, TK_REPEAT, TK_RETURN, TK_THEN, TK_UNTIL, TK_WHILE,
          /* other terminal symbols */
          TK_NAME, TK_CONCAT, TK_DOTS, TK_EQ, TK_GE, TK_LE, TK_NE, TK_NUMBER,
          TK_STRING, TK_EOS
		};

		/* number of reserved words */
		public const int NUM_RESERVED = (int)RESERVED.TK_WHILE - FIRST_RESERVED + 1;

		public class SemInfo {
			public SemInfo() { }
			public SemInfo(SemInfo copy)
			{
				this.r = copy.r;
				this.ts = copy.ts;
			}
			public Number r;
			public TString ts;
		} ;  /* semantics information */

		public class Token {
			public Token() { }
			public Token(Token copy)
			{
				this.token = copy.token;
				this.seminfo = new SemInfo(copy.seminfo);
			}
			public int token;
			public SemInfo seminfo = new SemInfo();
		};


		public class LexState {
			public int current;  /* current character */
			public Token t = new Token();  /* current token */
			public Token lookahead = new Token();  /* look ahead token */
			public FuncState fs;  /* `FuncState' is private to the parser */
			public lua_State L;
			public zio z;  /* input stream */
			public int linenumber;  /* input line counter */
			public int lastline;  /* line of last token `consumed' */
			public TString source;  /* current source name */
		};
		
//void luaX_init (lua_State *L);
//void luaX_setinput (lua_State *L, LexState *LS, ZIO *z, TString *source);
//int luaX_lex (LexState *LS, SemInfo *seminfo);
//void luaX_checklimit (LexState *ls, int val, int limit, const char *msg);
//void luaX_syntaxerror (LexState *ls, const char *s, const char *token);
//void luaX_error (LexState *ls, const char *s, int token);
//void luaX_token2str (int token, char *s);		


//#endif
	}
}
