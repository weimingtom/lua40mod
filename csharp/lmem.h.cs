/*
** $Id: lmem.h,v 1.16 2000/10/30 16:29:59 roberto Exp $
** Interface to Memory Manager
** See Copyright Notice in lua.h
*/

//#ifndef lmem_h
//#define lmem_h


//#include <stddef.h>

//#include "llimits.h"
//#include "lua.h"

namespace lua40mod
{
	public partial class Lua
	{
//void *luaM_realloc (lua_State *L, void *oldblock, lint32 size);
//void *luaM_growaux (lua_State *L, void *block, size_t nelems,
//                    int inc, size_t size, const char *errormsg,
//                    size_t limit);


		public static void luaM_free<T>(lua_State L, T b) { luaM_realloc<T>(L, new T[] {b}, 0); }
		public static T luaM_malloc<T>(lua_State L, int n) { return (T)luaM_realloc<T>(L); }
		public static T luaM_new<T>(lua_State L) { return (T)luaM_malloc<T>(L, 0); }
		public static T[] luaM_newvector<T>(lua_State L, int n) { return null/*luaM_malloc<T>(L, n)*/;}

		public static void luaM_growvector<T>(lua_State L, ref T[] v, int nelems, ref int inc, CharPtr e, int l) {
				v = (T[])luaM_growaux(L, ref v, ref inc, e, l); }

		public static T[] luaM_reallocvector<T>(lua_State L, ref T[] v, int n) {
		        /*v = luaM_realloc<T>(L, v, n); return v;*/ return null; }	


//#ifdef LUA_DEBUG
//extern unsigned long memdebug_numblocks;
//extern unsigned long memdebug_total;
//extern unsigned long memdebug_maxmem;
//extern unsigned long memdebug_memlimit;
//#endif


//#endif
		
	}
}
