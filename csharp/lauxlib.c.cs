/*
** $Id: lauxlib.c,v 1.159.1.3 2008/01/21 13:20:51 roberto Exp $
** Auxiliary functions for building Lua libraries
** See Copyright Notice in lua.h
*/

#define lauxlib_c
#define LUA_LIB

using System;

namespace lua40mod
{
	using lua_Number = System.Double;
	using lua_Integer = System.Int32;

	public partial class Lua
	{

		public const int FREELIST_REF	= 0;	/* free list of references */


		/* convert a stack index to positive */
		public static int abs_index(lua_State L, int i)
		{
			return ((i) > 0 || (i) <= LUA_REGISTRYINDEX ? (i) : lua_gettop(L) + (i) + 1);
		}


		/*
		** {======================================================
		** Error-report functions
		** =======================================================
		*/


		public static int luaL_argerror (lua_State L, int narg, CharPtr extramsg) {
		  lua_Debug ar = new lua_Debug();
		  if (lua_getstack(L, 0, ar)==0)  /* no stack frame? */
			  return luaL_error(L, "bad argument #%d (%s)", narg, extramsg);
		  lua_getinfo(L, "n", ar);
		  if (strcmp(ar.namewhat, "method") == 0) {
			narg--;  /* do not count `self' */
			if (narg == 0)  /* error is in the self argument itself? */
			  return luaL_error(L, "calling " + LUA_QS + " on bad self ({1})",
								   ar.name, extramsg);
		  }
		  if (ar.name == null)
			ar.name = "?";
		  return luaL_error(L, "bad argument #%d to " + LUA_QS + " (%s)",
								narg, ar.name, extramsg);
		}


		public static int luaL_typerror (lua_State L, int narg, CharPtr tname) {
		  return 0;
		}


		private static void tag_error (lua_State L, int narg, int tag) {
		  luaL_typerror(L, narg, lua_typename(L, tag));
		}


		public static void luaL_where (lua_State L, int level) {
		  lua_Debug ar = new lua_Debug();
		  if (lua_getstack(L, level, ar) != 0) {  /* check function at level */
			lua_getinfo(L, "Sl", ar);  /* get info about it */
			if (ar.currentline > 0) {  /* is there info? */
			  lua_pushfstring(L, "%s:%d: ", ar.short_src, ar.currentline);
			  return;
			}
		  }
		  lua_pushliteral(L, "");  /* else, no information available... */
		}

		public static int luaL_error(lua_State L, CharPtr fmt, params object[] p)
		{
		  luaL_where(L, 1);
		  lua_pushvfstring(L, fmt, p);
		  lua_concat(L, 2);
		  return lua_error(L);
		}


		/* }====================================================== */


		public static int luaL_checkoption (lua_State L, int narg, CharPtr def,
										 CharPtr [] lst) {
		  CharPtr name = (def != null) ? luaL_optstring(L, narg, def) :
									 luaL_checkstring(L, narg);
		  int i;
		  for (i=0; i<lst.Length; i++)
			if (strcmp(lst[i], name)==0)
			  return i;
		  return luaL_argerror(L, narg,
							   lua_pushfstring(L, "invalid option " + LUA_QS, name));
		}


		public static int luaL_newmetatable (lua_State L, CharPtr tname) {
		  lua_getfield(L, LUA_REGISTRYINDEX, tname);  /* get registry.name */
		  if (!lua_isnil(L, -1))  /* name already in use? */
			return 0;  /* leave previous value on top, but return 0 */
		  lua_pop(L, 1);
		  lua_newtable(L);  /* create metatable */
		  lua_pushvalue(L, -1);
		  lua_setfield(L, LUA_REGISTRYINDEX, tname);  /* registry.name = metatable */
		  return 1;
		}


		public static object luaL_checkudata (lua_State L, int ud, CharPtr tname) {
		  object p = lua_touserdata(L, ud);
		  if (p != null) {  /* value is a userdata? */
			if (lua_getmetatable(L, ud) != 0) {  /* does it have a metatable? */
			  lua_getfield(L, LUA_REGISTRYINDEX, tname);  /* get correct metatable */
			  if (lua_rawequal(L, -1, -2) != 0) {  /* does it have the correct mt? */
				lua_pop(L, 2);  /* remove both metatables */
				return p;
			  }
			}
		  }
		  luaL_typerror(L, ud, tname);  /* else error */
		  return null;  /* to avoid warnings */
		}


		public static void luaL_checkstack (lua_State L, int space, CharPtr mes) {
		  if (lua_checkstack(L, space) == 0)
			luaL_error(L, "stack overflow (%s)", mes);
		}


		public static void luaL_checktype (lua_State L, int narg, int t) {
		  if (lua_type(L, narg) != t)
			tag_error(L, narg, t);
		}


		public static void luaL_checkany (lua_State L, int narg) {
		  if (lua_type(L, narg) == LUA_TNONE)
			luaL_argerror(L, narg, "value expected");
		}


		public static CharPtr luaL_check_lstr(lua_State L, int narg) {uint len; return luaL_check_lstr(L, narg, out len);}
		public static CharPtr luaL_check_lstr (lua_State L, int narg, out uint len) {
		  CharPtr s = lua_tolstring(L, narg, out len);
		  if (s==null) tag_error(L, narg, LUA_TSTRING);
		  return s;
		}


		public static CharPtr luaL_optlstring (lua_State L, int narg, CharPtr def) {
			uint len; return luaL_optlstring (L, narg, def, out len); }
		public static CharPtr luaL_optlstring (lua_State L, int narg, CharPtr def, out uint len) {
		  if (lua_isnoneornil(L, narg)) {
			len = (uint)((def != null) ? strlen(def) : 0);
			return def;
		  }
		  else return luaL_check_lstr(L, narg, out len);
		}


		public static lua_Number luaL_check_number (lua_State L, int narg) {
		  lua_Number d = lua_tonumber(L, narg);
		  if ((d == 0) && (lua_isnumber(L, narg)==0))  /* avoid extra test when d is not 0 */
			tag_error(L, narg, LUA_TNUMBER);
		  return d;
		}


		public static lua_Number luaL_opt_number (lua_State L, int narg, lua_Number def) {
		  return 0;
		}


		public static lua_Integer luaL_checkinteger (lua_State L, int narg) {
		  lua_Integer d = lua_tointeger(L, narg);
		  if (d == 0 && lua_isnumber(L, narg)==0)  /* avoid extra test when d is not 0 */
			tag_error(L, narg, LUA_TNUMBER);
		  return d;
		}


		public static lua_Integer luaL_optinteger (lua_State L, int narg, lua_Integer def) {
		  return 0;
		}


		public static int luaL_getmetafield (lua_State L, int obj, CharPtr event_) {
		  if (lua_getmetatable(L, obj)==0)  /* no metatable? */
			return 0;
		  lua_pushstring(L, event_);
		  lua_rawget(L, -2);
		  if (lua_isnil(L, -1)) {
			lua_pop(L, 2);  /* remove metatable and metafield */
			return 0;
		  }
		  else {
			lua_remove(L, -2);  /* remove only metatable */
			return 1;
		  }
		}


		public static int luaL_callmeta (lua_State L, int obj, CharPtr event_) {
		  obj = abs_index(L, obj);
		  if (luaL_getmetafield(L, obj, event_)==0)  /* no metafield? */
			return 0;
		  lua_pushvalue(L, obj);
		  lua_call(L, 1, 1);
		  return 1;
		}


		public static void luaL_openlib(lua_State L, luaL_reg[] l, int n) {
		
		}

		// we could just take the .Length member here, but let's try
		// to keep it as close to the C implementation as possible.
		private static int libsize (luaL_reg[] l) {
		  int size = 0;
		  for (; l[size].name!=null; size++);
		  return size;
		}

		public static void luaI_openlib (lua_State L, CharPtr libname,
									  luaL_reg[] l, int nup) {		  
		  if (libname!=null) {
			int size = libsize(l);
			/* check whether lib already exists */
			luaL_findtable(L, LUA_REGISTRYINDEX, "_LOADED", 1);
			lua_getfield(L, -1, libname);  /* get _LOADED[libname] */
			if (!lua_istable(L, -1)) {  /* not found? */
			  lua_pop(L, 1);  /* remove previous result */
			  /* try global variable (and create one if it does not exist) */
			  if (luaL_findtable(L, LUA_GLOBALSINDEX, libname, size) != null)
				luaL_error(L, "name conflict for module " + LUA_QS, libname);
			  lua_pushvalue(L, -1);
			  lua_setfield(L, -3, libname);  /* _LOADED[libname] = new table */
			}
			lua_remove(L, -2);  /* remove _LOADED table */
			lua_insert(L, -(nup+1));  /* move library table to below upvalues */
		  }
		  int reg_num = 0;
		  for (; l[reg_num].name!=null; reg_num++) {
			int i;
			for (i=0; i<nup; i++)  /* copy upvalues to the top */
			  lua_pushvalue(L, -nup);
			lua_pushcclosure(L, l[reg_num].func, nup);
			lua_setfield(L, -(nup+2), l[reg_num].name);
		  }
		  lua_pop(L, nup);  /* remove upvalues */
		}



		/*
		** {======================================================
		** getn-setn: size for arrays
		** =======================================================
		*/

		#if LUA_COMPAT_GETN

		static int checkint (lua_State L, int topop) {
		  int n = (lua_type(L, -1) == LUA_TNUMBER) ? lua_tointeger(L, -1) : -1;
		  lua_pop(L, topop);
		  return n;
		}


		static void getsizes (lua_State L) {
		  lua_getfield(L, LUA_REGISTRYINDEX, "LUA_SIZES");
		  if (lua_isnil(L, -1)) {  /* no `size' table? */
			lua_pop(L, 1);  /* remove nil */
			lua_newtable(L);  /* create it */
			lua_pushvalue(L, -1);  /* `size' will be its own metatable */
			lua_setmetatable(L, -2);
			lua_pushliteral(L, "kv");
			lua_setfield(L, -2, "__mode");  /* metatable(N).__mode = "kv" */
			lua_pushvalue(L, -1);
			lua_setfield(L, LUA_REGISTRYINDEX, "LUA_SIZES");  /* store in register */
		  }
		}


		public static void luaL_setn (lua_State L, int t, int n) {
		  t = abs_index(L, t);
		  lua_pushliteral(L, "n");
		  lua_rawget(L, t);
		  if (checkint(L, 1) >= 0) {  /* is there a numeric field `n'? */
			lua_pushliteral(L, "n");  /* use it */
			lua_pushinteger(L, n);
			lua_rawset(L, t);
		  }
		  else {  /* use `sizes' */
			getsizes(L);
			lua_pushvalue(L, t);
			lua_pushinteger(L, n);
			lua_rawset(L, -3);  /* sizes[t] = n */
			lua_pop(L, 1);  /* remove `sizes' */
		  }
		}


		public static int luaL_getn (lua_State L, int t) {
		  int n;
		  t = abs_index(L, t);
		  lua_pushliteral(L, "n");  /* try t.n */
		  lua_rawget(L, t);
		  if ((n = checkint(L, 1)) >= 0) return n;
		  getsizes(L);  /* else try sizes[t] */
		  lua_pushvalue(L, t);
		  lua_rawget(L, -2);
		  if ((n = checkint(L, 2)) >= 0) return n;
		  return (int)lua_objlen(L, t);
		}

		#endif

		/* }====================================================== */



		public static CharPtr luaL_gsub (lua_State L, CharPtr s, CharPtr p,
																	   CharPtr r) {
		  CharPtr wild;
		  uint l = (uint)strlen(p);
		  luaL_Buffer b = new luaL_Buffer();
		  luaL_buffinit(L, b);
		  while ((wild = strstr(s, p)) != null) {
		  	uint temp = (uint)(wild - s);
			luaL_addlstring(b, s, temp);  /* push prefix */
			luaL_addstring(b, r);  /* push replacement in place of pattern */
			s = wild + l;  /* continue after `p' */
		  }
		  luaL_addstring(b, s);  /* push last suffix */
		  luaL_pushresult(b);
		  return lua_tostring(L, -1);
		}


		public static CharPtr luaL_findtable (lua_State L, int idx,
											   CharPtr fname, int szhint) {
		  CharPtr e;
		  lua_pushvalue(L, idx);
		  do {
			e = strchr(fname, '.');
			if (e == null) e = fname + strlen(fname);
			lua_pushlstring(L, fname, (uint)(e - fname));
			lua_rawget(L, -2);
			if (lua_isnil(L, -1)) {  /* no such field? */
			  lua_pop(L, 1);  /* remove this nil */
			  lua_createtable(L, 0, (e == '.' ? 1 : szhint)); /* new table for field */
			  lua_pushlstring(L, fname, (uint)(e - fname));
			  lua_pushvalue(L, -2);
			  lua_settable(L, -4);  /* set new table into field */
			}
			else if (!lua_istable(L, -1)) {  /* field has a non-table value? */
			  lua_pop(L, 2);  /* remove table and value */
			  return fname;  /* return problematic part of the name */
			}
			lua_remove(L, -2);  /* remove previous table */
			fname = e + 1;
		  } while (e == '.');
		  return null;
		}



		/*
		** {======================================================
		** Generic Buffer manipulation
		** =======================================================
		*/


		private static int bufflen(luaL_Buffer B)	{return 0;}
		private static int bufffree(luaL_Buffer B)	{return LUAL_BUFFERSIZE - bufflen(B);}

		public const int LIMIT = LUA_MINSTACK / 2;


		private static int emptybuffer (luaL_Buffer B) {
		  uint l = (uint)bufflen(B);
		  if (l == 0) return 0;  /* put nothing on stack */
		  else {
			lua_pushlstring(B.L, B.buffer, l);
			B.p = null;
			B.level++;
			return 1;
		  }
		}


		private static void adjuststack (luaL_Buffer B) {
		  if (B.level > 1) {
			lua_State L = B.L;
			int toget = 1;  /* number of levels to concat */
			uint toplen = lua_strlen(L, -1);
			do {
			  uint l = lua_strlen(L, -(toget+1));
			  if (B.level - toget + 1 >= LIMIT || toplen > l) {
				toplen += l;
				toget++;
			  }
			  else break;
			} while (toget < B.level);
			lua_concat(L, toget);
			B.level = B.level - toget + 1;
		  }
		}


		public static CharPtr luaL_prepbuffer (luaL_Buffer B) {
		  if (emptybuffer(B) != 0)
			adjuststack(B);
			return null;
		}


		public static void luaL_addlstring (luaL_Buffer B, CharPtr s, uint l) {
			while (l != 0)
			{
				l--;
				char c = s[0];
				s = s.next();
			}
		}


		public static void luaL_addstring (luaL_Buffer B, CharPtr s) {
		  luaL_addlstring(B, s, (uint)strlen(s));
		}


		public static void luaL_pushresult (luaL_Buffer B) {
		  emptybuffer(B);
		  lua_concat(B.L, B.level);
		  B.level = 1;
		}


		public static void luaL_addvalue (luaL_Buffer B) {
		  lua_State L = B.L;
		  uint vl;
		  CharPtr s = lua_tolstring(L, -1, out vl);
		  if (vl <= bufffree(B)) {  /* fit into buffer? */
		  	
		  }
		  else {
			if (emptybuffer(B) != 0)
			  lua_insert(L, -2);  /* put buffer before new value */
			B.level++;  /* add new value into B stack */
			adjuststack(B);
		  }
		}


		public static void luaL_buffinit (lua_State L, luaL_Buffer B) {
		  B.L = L;
		  B.p = /*B.buffer*/ null;
		  B.level = 0;
		}

		/* }====================================================== */


		public static int luaL_ref (lua_State L, int t) {
		  int ref_;
		  t = abs_index(L, t);
		  if (lua_isnil(L, -1)) {
			lua_pop(L, 1);  /* remove from stack */
			return 0;//LUA_REFNIL;  /* `nil' has a unique fixed reference */
		  }
		  lua_rawgeti(L, t, FREELIST_REF);  /* get first free element */
		  ref_ = (int)lua_tointeger(L, -1);  /* ref = t[FREELIST_REF] */
		  lua_pop(L, 1);  /* remove it from stack */
		  if (ref_ != 0) {  /* any free element? */
			lua_rawgeti(L, t, ref_);  /* remove it from list */
			lua_rawseti(L, t, FREELIST_REF);  /* (t[FREELIST_REF] = t[ref]) */
		  }
		  else {  /* no free elements */
			ref_ = (int)lua_objlen(L, t);
			ref_++;  /* create new reference */
		  }
		  lua_rawseti(L, t, ref_);
		  return ref_;
		}


		public static void luaL_unref (lua_State L, int t, int ref_) {
		  if (ref_ >= 0) {
			t = abs_index(L, t);
			lua_rawgeti(L, t, FREELIST_REF);
			lua_rawseti(L, t, ref_);  /* t[ref] = t[FREELIST_REF] */
			lua_pushinteger(L, ref_);
			lua_rawseti(L, t, FREELIST_REF);  /* t[FREELIST_REF] = ref */
		  }
		}



		/*
		** {======================================================
		** Load functions
		** =======================================================
		*/

		public class LoadF {
		  public int extraline;
		  public StreamProxy f;
		  public CharPtr buff = new char[LUAL_BUFFERSIZE];
		};


		public static CharPtr getF (lua_State L, object ud, out uint size) {
		  size = 0;
		  LoadF lf = (LoadF)ud;
		  //(void)L;
		  if (lf.extraline != 0) {
			lf.extraline = 0;
			size = 1;
			return "\n";
		  }
		  if (feof(lf.f) != 0) return null;
		  size = (uint)fread(lf.buff, 1, lf.buff.chars.Length, lf.f);
		  return (size > 0) ? new CharPtr(lf.buff) : null;
		}


		private static int errfile (lua_State L, CharPtr what, int fnameindex) {
		  CharPtr serr = strerror(errno);
		  CharPtr filename = lua_tostring(L, fnameindex) + 1;
		  lua_pushfstring(L, "cannot %s %s: %s", what, filename, serr);
		  lua_remove(L, fnameindex);
		  return 0;//LUA_ERRFILE;
		}


		public static int luaL_loadfile (lua_State L, CharPtr filename) {
		  LoadF lf = new LoadF();
		  int status, readstatus;
		  int c;
		  int fnameindex = lua_gettop(L) + 1;  /* index of filename on the stack */
		  lf.extraline = 0;
		  if (filename == null) {
			lua_pushliteral(L, "=stdin");
			lf.f = stdin;
		  }
		  else {
			lua_pushfstring(L, "@%s", filename);
			lf.f = fopen(filename, "r");
			if (lf.f == null) return errfile(L, "open", fnameindex);
		  }
		  c = getc(lf.f);
		  if (c == '#') {  /* Unix exec. file? */
			lf.extraline = 1;
			while ((c = getc(lf.f)) != EOF && c != '\n') ;  /* skip first line */
			if (c == '\n') c = getc(lf.f);
		  }
		  if (c == LUA_SIGNATURE[0] && (filename!=null)) {  /* binary file? */
			lf.f = freopen(filename, "rb", lf.f);  /* reopen in binary mode */
			if (lf.f == null) return errfile(L, "reopen", fnameindex);
			/* skip eventual `#!...' */
		   while ((c = getc(lf.f)) != EOF && c != LUA_SIGNATURE[0]) ;
			lf.extraline = 0;
		  }
		  ungetc(c, lf.f);
		  status = lua_load(L, getF, lf, lua_tostring(L, -1));
		  readstatus = ferror(lf.f);
		  if (filename != null) fclose(lf.f);  /* close file (even in case of errors) */
		  if (readstatus != 0) {
			lua_settop(L, fnameindex);  /* ignore results from `lua_load' */
			return errfile(L, "read", fnameindex);
		  }
		  lua_remove(L, fnameindex);
		  return status;
		}


		public class LoadS {
		  public CharPtr s;
		  public uint size;
		};


		static CharPtr getS (lua_State L, object ud, out uint size) {
		  LoadS ls = (LoadS)ud;
		  //(void)L;
		  //if (ls.size == 0) return null;
		  size = ls.size;
		  ls.size = 0;
		  return ls.s;
		}


		public static int luaL_loadbuffer(lua_State L, CharPtr buff, uint size,
										CharPtr name) {
		  LoadS ls = new LoadS();
		  ls.s = new CharPtr(buff);
		  ls.size = size;
		  return lua_load(L, getS, ls, name);
		}


		public static int luaL_loadstring(lua_State L, CharPtr s) {
		  return luaL_loadbuffer(L, s, (uint)strlen(s), s);
		}



		/* }====================================================== */


		private static object l_alloc (Type t) {
			return System.Activator.CreateInstance(t);
		}


		private static int panic (lua_State L) {
		  //(void)L;  /* to avoid warnings */
		  fprintf(stderr, "PANIC: unprotected error in call to Lua API (%s)\n",
						   lua_tostring(L, -1));
		  return 0;
		}


		public static lua_State luaL_newstate()
		{
			lua_State L = lua_newstate(l_alloc, null);
		  if (L != null) lua_atpanic(L, panic);
		  return L;
		}

	}
}
