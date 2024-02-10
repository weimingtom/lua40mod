/*
** $Id: lobject.h,v 1.82 2000/10/30 17:49:19 roberto Exp $
** Type definitions for Lua objects
** See Copyright Notice in lua.h
*/

//#ifndef lobject_h
//#define lobject_h


//#include "llimits.h"
//#include "lua.h"

using System;

namespace lua40mod
{
	using TValue = Lua.Value;
	using StkId = Lua.Value;
	using lu_byte = System.Byte;
	using Number = System.Double;
	using Instruction = System.UInt32;
	
	public partial class Lua
	{	
//#ifdef LUA_DEBUG
//#undef NDEBUG
//#include <assert.h>
//#define LUA_INTERNALERROR(s)	assert(((void)s,0))
//#define LUA_ASSERT(c,s)		assert(((void)s,(c)))
//#else
//#define LUA_INTERNALERROR(s)	/* empty */
//#define LUA_ASSERT(c,s)		/* empty */
//#endif


//#ifdef LUA_DEBUG
/* to avoid warnings, and make sure value is really unused */
//#define UNUSED(x)	(x=0, (void)(x))
//#else
//#define UNUSED(x)	((void)(x))	/* to avoid warnings */
//#endif


		/* mark for closures active in the stack */
		public const int LUA_TMARK	= 6;
		
		
		/* tags for values visible from Lua == first user-created tag */
		public const int NUM_TAGS	= 6;


		/* check whether `t' is a mark */
		public static bool is_T_MARK(int t)	{ return ((t) == LUA_TMARK); }


		public class Value {
		  public TString ts;	/* LUA_TSTRING, LUA_TUSERDATA */
		  public Closure cl;	/* LUA_TFUNCTION */
		  public Hash a;	/* LUA_TTABLE */
		  public CallInfo i;	/* LUA_TLMARK */
		  public Number n;		/* LUA_TNUMBER */
		};

		/* Macros to access values */
		public static int    ttype(TObject o) { return o.ttype; }
		public static Number nvalue(TObject o) { return o.value.n; }
		public static TString tsvalue(TObject o) { return ((o).value.ts); }
		public static Closure clvalue(TObject o) { return ((o).value.cl); }
		public static Hash hvalue(TObject o) { return ((o).value.a); }
		public static CallInfo infovalue(TObject o) { return ((o).value.i); }
		public static char[] svalue(TObject o) { return (tsvalue(o).str); }

		public class TObject {
		  public int ttype;
		  public Value value;
		};
		
		
		/*
		** String headers for string table
		*/
		
		/*
		** most `malloc' libraries allocate memory in blocks of 8 bytes. TSPACK
		** tries to make sizeof(TString) a multiple of this granularity, to reduce
		** waste of space.
		*/
		public const int TSPACK = 4;//((int)sizeof(int));
		public class TString {
			public class TString_u {
			    public struct TString_u_s {  /* for strings */
			      public ulong hash;
			      public int constindex;  /* hint to reuse constants */
			    }; public TString_u_s s;
			    public struct TString_u_d {  /* for userdata */
			      public int tag;
			      public object value;
				}; public TString_u_d d = new TString_u_d();
			}; public TString_u u = new TString_u();
		  	public uint len;
		  	public TString nexthash;  /* chain for hash table */
		  	public int marked;
		  	public char[] str = new char[TSPACK];   /* variable length string!! must be the last field! */
		};


		/*
		** Function Prototypes
		*/
		public class Proto : GCObject {
		  public Number[] knum;  /* Number numbers used by the function */
		  public int nknum;  /* size of `knum' */
		  public TString[] kstr;  /* strings used by the function */
		  public int nkstr;  /* size of `kstr' */
		  public Proto[] kproto;  /* functions defined inside the function */
		  public int nkproto;  /* size of `kproto' */
		  public Instruction code;
		  public int ncode;  /* size of `code'; when 0 means an incomplete `Proto' */
		  public short numparams;
		  public short is_vararg;
		  public short maxstacksize;
		  public short marked;
		  public Proto next;
		  /* debug information */
		  public int[] lineinfo;  /* map from opcodes to source lines */
		  public int nlineinfo;  /* size of `lineinfo' */
		  public int nlocvars;
		  public LocVar locvars;  /* information about local variables */
		  public int lineDefined;
		  public TString  source;
		};

		public class LocVar {
		  public TString varname;
		  public int startpc;  /* first point where variable is active */
		  public int endpc;    /* first point where variable is dead */
		};

		/*
		** Closures
		*/
		public class Closure {
		  public struct Closure_f {
		    public lua_CFunction c;  /* C functions */
		    public Proto l;  /* Lua functions */
		  };public Closure_f f = new Closure_f();
		  public Closure next;
		  public Closure mark;  /* marked closures (point to itself when not marked) */
		  public short isC;  /* 0 for Lua functions, 1 for C functions */
		  public short nupvalues;
		  public TObject[] upvalue = new TObject[1];
		};

		public static bool iscfunction(TObject o)	{ return (ttype(o) == LUA_TFUNCTION && clvalue(o).isC!=0); }


		public class Node {
		  public TObject key = new TObject();
		  public TObject val = new TObject();
		  public Node next;  /* for chaining */
		};

		public class Hash {
		  public Node node;
		  public int htag;
		  public int size;
		  public Node firstfree;  /* this position is free; all positions after it are full */
		  public Hash next;
		  public Hash mark;  /* marked tables (point to itself when not marked) */
		};


		/* unmarked tables and closures are represented by pointing `mark' to
		** themselves
		*/
		public static bool ismarked(Hash x) { return ((x).mark != (x)); }


		/*
		** informations about a call (for debugging)
		*/
		public class CallInfo {
		  public Closure func;  /* function being called */
		  public Instruction[] pc;  /* current pc of called function */
		  public int lastpc;  /* last pc traced */
		  public int line;  /* current line */
		  public int refi;  /* current index in `lineinfo' */
		};


//extern const TObject luaO_nilobject;
//extern const char *const luaO_typenames[];


		public static CharPtr luaO_typename(TObject o) { return (luaO_typenames[ttype(o)]); }

		
//lint32 luaO_power2 (lint32 n);
//char *luaO_openspace (lua_State *L, size_t n);

//int luaO_equalObj (const TObject *t1, const TObject *t2);
//int luaO_str2d (const char *s, Number *result);

//void luaO_verror (lua_State *L, const char *fmt, ...);
//void luaO_chunkid (char *out, const char *source, int len);


//#endif
	}
}
