/*
** $Id: lstring.c,v 2.8.1.1 2007/12/27 13:02:25 roberto Exp $
** String table (keeps all strings handled by Lua)
** See Copyright Notice in lua.h
*/

using System;

namespace lua40mod
{
	using lu_byte = System.Byte;

	public partial class Lua
	{


		public static void luaS_resize (lua_State L, int newsize) {
//		  GCObject[] newhash;
//		  stringtable tb;
//		  int i;
//		  if (G(L).gcstate == GCSsweepstring)
//			return;  /* cannot resize during GC traverse */		  
//
//		  // todo: fix this up
//		  // I'm treating newhash as a regular C# array, but I need to allocate a dummy array
//		  // so that the garbage collector behaves identical to the C version.
//		  //newhash = luaM_newvector<GCObjectRef>(L, newsize);
//		  newhash = new GCObject[newsize];
//		  AddTotalBytes(L, newsize * GetUnmanagedSize(typeof(GCObjectRef)));
//
//		  tb = G(L).strt;
//		  for (i=0; i<newsize; i++) newhash[i] = null;
//
//		  /* rehash */
//		  for (i=0; i<tb.size; i++) {
//			GCObject p = tb.hash[i];
//			while (p != null) {  /* for each node in the list */
//			  GCObject next = p.gch.next;  /* save next */
//			  uint h = gco2ts(p).hash;
//			  int h1 = (int)lmod(h, newsize);  /* new position */
//			  lua_assert((int)(h%newsize) == lmod(h, newsize));
//			  p.gch.next = newhash[h1];  /* chain it */
//			  newhash[h1] = p;
//			  p = next;
//			}
//		  }
//		  //luaM_freearray(L, tb.hash);
//		  if (tb.hash != null)
//			  SubtractTotalBytes(L, tb.hash.Length * GetUnmanagedSize(typeof(GCObjectRef)));
//		  tb.size = newsize;
//		  tb.hash = newhash;
		}


		public static TString newlstr (lua_State L, CharPtr str, uint l,
											   uint h) {
//		  TString ts;
//		  stringtable tb;
//		  if (l+1 > MAX_SIZET /GetUnmanagedSize(typeof(char)))
//		    luaM_toobig(L);
//		  ts = new TString(new char[l+1]);
//		  AddTotalBytes(L, (int)(l + 1) * GetUnmanagedSize(typeof(char)) + GetUnmanagedSize(typeof(TString)));
//		  ts.tsv.len = l;
//		  ts.tsv.hash = h;
//		  ts.tsv.marked = luaC_white(G(L));
//		  ts.tsv.tt = LUA_TSTRING;
//		  ts.tsv.reserved = 0;
//		  //memcpy(ts+1, str, l*GetUnmanagedSize(typeof(char)));
//		  memcpy(ts.str.chars, str.chars, str.index, (int)l);
//		  ts.str[l] = '\0';  /* ending 0 */
//		  tb = G(L).strt;
//		  h = (uint)lmod(h, tb.size);
//		  ts.tsv.next = tb.hash[h];  /* chain new entry */
//		  tb.hash[h] = obj2gco(ts);
//		  tb.nuse++;
//		  if ((tb.nuse > (int)tb.size) && (tb.size <= MAX_INT/2))
//		    luaS_resize(L, tb.size*2);  /* too crowded */
//		  return ts;
			return null;
		}

		public static TString luaS_newlstr (lua_State L, CharPtr str, uint l) {
//		  GCObject o;
//		  uint h = (uint)l;  /* seed */
//		  uint step = (l>>5)+1;  /* if string is too long, don't hash all its chars */
//		  uint l1;
//		  for (l1=l; l1>=step; l1-=step)  /* compute hash */
//		  	h = h ^ (uint)((((ulong)h<<5)+((ulong)h>>2)+(byte)str[l1-1]) & 0xFFFFFFFFL);
//		  for (o = G(L).strt.hash[lmod(h, G(L).strt.size)];
//			   o != null;
//			   o = o.gch.next) {
//			TString ts = rawgco2ts(o);			
//			if (ts.tsv.len == l && (memcmp(str, getstr(ts), l) == 0)) {
//			  /* string may be dead */
//			  if (isdead(G(L), o)) changewhite(o);
//			  return ts;
//			}
//		  }
//		  //return newlstr(L, str, l, h);  /* not found */
//		  TString res = newlstr(L, str, l, h);
//		  return res;
			return null;
		}


		public static object/*Udata*/ luaS_newudata(lua_State L, uint s, /*Table*/object e)
		{
//			Udata u = new Udata();
//			u.uv.marked = luaC_white(G(L));  /* is not finalized */
//			u.uv.tt = LUA_TUSERDATA;
//			u.uv.len = s;
//			u.uv.metatable = null;
//			u.uv.env = e;
//			u.user_data = new byte[s];
//			/* chain it on udata list (after main thread) */
//			u.uv.next = G(L).mainthread.next;
//			G(L).mainthread.next = obj2gco(u);
//			return u;
			return null;
		}

		public static object/*Udata*/ luaS_newudata(lua_State L, Type t, object/*Table*/ e)
		{
//			Udata u = new Udata();
//			u.uv.marked = luaC_white(G(L));  /* is not finalized */
//			u.uv.tt = LUA_TUSERDATA;
//			u.uv.len = 0;
//			u.uv.metatable = null;
//			u.uv.env = e;
//			u.user_data = luaM_realloc_(L, t);
//			AddTotalBytes(L, GetUnmanagedSize(typeof(Udata)));
//			/* chain it on udata list (after main thread) */
//			u.uv.next = G(L).mainthread.next;
//			G(L).mainthread.next = obj2gco(u);
//			return u;
			return null;
		}

	}
}
