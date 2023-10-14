/*
** $Id: lauxlib.h,v 1.30 2000/10/30 12:38:50 roberto Exp $
** Auxiliary functions for building Lua libraries
** See Copyright Notice in lua.h
*/

//#ifndef lauxlib_h
//#define lauxlib_h
//
//
//#include <stddef.h>
//#include <stdio.h>
//
//#include "lua.h"

namespace lua40mod
{
//	using lua_Number = System.Double;
	using lua_Integer = System.Int32;
	
	public partial class Lua
	{
		
//#ifndef LUALIB_API
//#define LUALIB_API	extern
//#endif


		public class luaL_reg {
		  public luaL_reg(CharPtr name, lua_CFunction func) {
			  this.name = name;
			  this.func = func;
		  }

		  public CharPtr name;
		  public lua_CFunction func;
		};


//LUALIB_API void luaL_openlib (lua_State *L, const struct luaL_reg *l, int n);
//LUALIB_API void luaL_argerror (lua_State *L, int numarg, const char *extramsg);
//LUALIB_API const char *luaL_check_lstr (lua_State *L, int numArg, size_t *len);
//LUALIB_API const char *luaL_opt_lstr (lua_State *L, int numArg, const char *def, size_t *len);
//LUALIB_API double luaL_check_number (lua_State *L, int numArg);
//LUALIB_API double luaL_opt_number (lua_State *L, int numArg, double def);
//
//LUALIB_API void luaL_checkstack (lua_State *L, int space, const char *msg);
//LUALIB_API void luaL_checktype (lua_State *L, int narg, int t);
//LUALIB_API void luaL_checkany (lua_State *L, int narg);
//
//LUALIB_API void luaL_verror (lua_State *L, const char *fmt, ...);
//LUALIB_API int luaL_findstring (const char *name, const char *const list[]);



		/*
		** ===============================================================
		** some useful macros
		** ===============================================================
		*/

		public static void luaL_argcheck(lua_State L, bool cond, int numarg, string extramsg) { if (!cond)
				luaL_argerror(L, numarg, extramsg);}
		public static CharPtr luaL_checkstring(lua_State L, int n) { uint len; return luaL_check_lstr(L, n, out len); }
		public static CharPtr luaL_optstring(lua_State L, int n, CharPtr d) { uint len; return luaL_optlstring(L, n, d, out len); }
		public static int luaL_check_int(lua_State L, int n) {return (int)luaL_check_number(L, n);}
		public static long luaL_check_long(lua_State L, int n)	{return (long)luaL_check_number(L, n);}
		public static int luaL_optint(lua_State L, int n, lua_Integer d)	{return (int)luaL_opt_number(L, n, d);}
		public static long luaL_opt_long(lua_State L, int n, lua_Integer d)	{return (long)luaL_opt_number(L, n, d);}
		public static void luaL_openl(lua_State L, luaL_reg[] a)		 { luaL_openlib(L, a, a.Length); }


/*
** {======================================================
** Generic Buffer manipulation
** =======================================================
*/


//		#ifndef LUAL_BUFFERSIZE
		public const int LUAL_BUFFERSIZE = BUFSIZ;
//		#endif

		public class luaL_Buffer {
		  public CharPtr p;			/* current position in buffer */
		  public int level;
		  public lua_State L;
		  public CharPtr buffer = new char[LUAL_BUFFERSIZE];
		};

		public static void luaL_putchar(luaL_Buffer B, char c) {
			if (B.p < B.buffer.add(LUAL_BUFFERSIZE)) luaL_prepbuffer(B);
			B.p[0] = (char)(c); B.p.inc();
		}
		
		public void luaL_addsize(luaL_Buffer B, char n)	{ B.p += (n); }
		
//		LUALIB_API void luaL_buffinit (lua_State *L, luaL_Buffer *B);
//		LUALIB_API char *luaL_prepbuffer (luaL_Buffer *B);
//		LUALIB_API void luaL_addlstring (luaL_Buffer *B, const char *s, size_t l);
//		LUALIB_API void luaL_addstring (luaL_Buffer *B, const char *s);
//		LUALIB_API void luaL_addvalue (luaL_Buffer *B);
//		LUALIB_API void luaL_pushresult (luaL_Buffer *B);

/* }====================================================== */


//#endif



	}
}

