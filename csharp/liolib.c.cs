/*
** $Id: liolib.c,v 2.73.1.3 2008/01/18 17:47:43 roberto Exp $
** Standard I/O (and system) library
** See Copyright Notice in lua.h
*/

namespace lua40mod
{
	using lua_Number = System.Double;
	using lua_Integer = System.Int32;

	public class FilePtr
	{
		public Lua.StreamProxy file;
	}

	public partial class Lua
	{
		public const int IO_INPUT	= 1;
		public const int IO_OUTPUT	= 2;

		private static readonly string[] fnames = { "input", "output" };


		private static int pushresult (lua_State L, int i, CharPtr filename) {
		  int en = errno;  /* calls to Lua API may change this value */
		  if (i != 0) {
			lua_pushboolean(L, 1);
			return 1;
		  }
		  else {
			lua_pushnil(L);
			if (filename != null)
				lua_pushfstring(L, "%s: %s", filename, strerror(en));
			else
				lua_pushfstring(L, "%s", strerror(en));
			lua_pushinteger(L, en);
			return 3;
		  }
		}


		private static void fileerror (lua_State L, int arg, CharPtr filename) {
		  lua_pushfstring(L, "%s: %s", filename, strerror(errno));
		  luaL_argerror(L, arg, lua_tostring(L, -1));
		}


		public static FilePtr tofilep(lua_State L) { return (FilePtr)luaL_checkudata(L, 1, LUA_FILEHANDLE); }


		private static int io_type (lua_State L) {
		  object ud;
		  luaL_checkany(L, 1);
		  ud = lua_touserdata(L, 1);
		  lua_getfield(L, LUA_REGISTRYINDEX, LUA_FILEHANDLE);
		  if (ud == null || (lua_getmetatable(L, 1)==0) || (lua_rawequal(L, -2, -1)==0))
			lua_pushnil(L);  /* not a file */
		  else if ( (ud as FilePtr).file == null)
			lua_pushliteral(L, "closed file");
		  else
			lua_pushliteral(L, "file");
		  return 1;
		}


		private static StreamProxy tofile (lua_State L) {
		  FilePtr f = tofilep(L);
		  if (f.file == null)
			luaL_error(L, "attempt to use a closed file");
		  return f.file;
		}



		/*
		** When creating file files, always creates a `closed' file file
		** before opening the actual file; so, if there is a memory error, the
		** file is not left opened.
		*/
		private static FilePtr newfile (lua_State L) {

		  FilePtr pf = (FilePtr)lua_newuserdata(L, typeof(FilePtr));
		  return pf;
		}


		/*
		** function to (not) close the standard files stdin, stdout, and stderr
		*/
		private static int io_noclose (lua_State L) {
		  lua_pushnil(L);
		  lua_pushliteral(L, "cannot close standard file");
		  return 2;
		}


		/*
		** function to close 'popen' files
		*/
		private static int io_pclose (lua_State L) {
		  FilePtr p = tofilep(L);
		  int ok = (lua_pclose(L, p.file) == 0) ? 1 : 0;
		  p.file = null;
		  return pushresult(L, ok, null);
		}


		/*
		** function to close regular files
		*/
		private static int io_fclose (lua_State L) {
		  FilePtr p = tofilep(L);
		  int ok = (fclose(p.file) == 0) ? 1 : 0;
		  p.file = null;
		  return pushresult(L, ok, null);
		}


		private static int aux_close (lua_State L) {
		  lua_getfenv(L, 1);
		  lua_getfield(L, -1, "__close");
		  return (lua_tocfunction(L, -1))(L);
		}


		private static int io_close (lua_State L) {
		  if (lua_isnone(L, 1))
			lua_rawgeti(L, LUA_ENVIRONINDEX, IO_OUTPUT);
		  tofile(L);  /* make sure argument is a file */
		  return aux_close(L);
		}


		private static int io_gc (lua_State L) {
		  StreamProxy f = tofilep(L).file;
		  /* ignore closed files */
		  if (f != null)
			aux_close(L);
		  return 0;
		}


		private static int io_tostring (lua_State L) {
		  StreamProxy f = tofilep(L).file;
		  if (f == null)
			lua_pushliteral(L, "file (closed)");
		  else
			lua_pushfstring(L, "file (%p)", f);
		  return 1;
		}


		private static int io_open (lua_State L) {
		  CharPtr filename = luaL_checkstring(L, 1);
		  CharPtr mode = luaL_optstring(L, 2, "r");
		  FilePtr pf = newfile(L);
		  pf.file = fopen(filename, mode);
		  return (pf.file == null) ? pushresult(L, 0, filename) : 1;
		}


		/*
		** this function has a separated environment, which defines the
		** correct __close for 'popen' files
		*/
		private static int io_popen (lua_State L) {
		  CharPtr filename = luaL_checkstring(L, 1);
		  CharPtr mode = luaL_optstring(L, 2, "r");
		  FilePtr pf = newfile(L);
		  pf.file = lua_popen(L, filename, mode);
		  return (pf.file == null) ? pushresult(L, 0, filename) : 1;
		}


		private static int io_tmpfile (lua_State L) {
		  FilePtr pf = newfile(L);
		  pf.file = tmpfile();
		  return (pf.file == null) ? pushresult(L, 0, null) : 1;
		}


		private static StreamProxy getiofile (lua_State L, int findex) {
		  StreamProxy f;
		  lua_rawgeti(L, LUA_ENVIRONINDEX, findex);
		  f = (lua_touserdata(L, -1) as FilePtr).file;
		  if (f == null)
			luaL_error(L, "standard %s file is closed", fnames[findex - 1]);
		  return f;
		}


		private static int g_iofile (lua_State L, int f, CharPtr mode) {
		  if (!lua_isnoneornil(L, 1)) {
			CharPtr filename = lua_tostring(L, 1);
			if (filename != null) {
			  FilePtr pf = newfile(L);
			  pf.file = fopen(filename, mode);
			  if (pf.file == null)
				fileerror(L, 1, filename);
			}
			else {
			  tofile(L);  /* check that it's a valid file file */
			  lua_pushvalue(L, 1);
			}
			lua_rawseti(L, LUA_ENVIRONINDEX, f);
		  }
		  /* return current value */
		  lua_rawgeti(L, LUA_ENVIRONINDEX, f);
		  return 1;
		}


		private static int io_input (lua_State L) {
		  return g_iofile(L, IO_INPUT, "r");
		}


		private static int io_output (lua_State L) {
		  return g_iofile(L, IO_OUTPUT, "w");
		}

		private static void aux_lines (lua_State L, int idx, int toclose) {
		  lua_pushvalue(L, idx);
		  lua_pushboolean(L, toclose);  /* close/not close file when finished */
		  lua_pushcclosure(L, io_readline, 2);
		}


		private static int f_lines (lua_State L) {
		  tofile(L);  /* check that it's a valid file file */
		  aux_lines(L, 1, 0);
		  return 1;
		}


		private static int io_lines (lua_State L) {
		  if (lua_isnoneornil(L, 1)) {  /* no arguments? */
			/* will iterate over default input */
			lua_rawgeti(L, LUA_ENVIRONINDEX, IO_INPUT);
			return f_lines(L);
		  }
		  else {
			CharPtr filename = luaL_checkstring(L, 1);
			FilePtr pf = newfile(L);
			pf.file = fopen(filename, "r");
			if (pf.file == null)
			  fileerror(L, 1, filename);
			aux_lines(L, lua_gettop(L), 1);
			return 1;
		  }
		}


		/*
		** {======================================================
		** READ
		** =======================================================
		*/


		private static int read_number (lua_State L, StreamProxy f) {
		  //lua_Number d;
			object[] parms = { (object)(double)0.0 };
			if (fscanf(f, LUA_NUMBER_SCAN, parms) == 1)
			{
				lua_pushnumber(L, (double)parms[0]);
				return 1;
			}
			else return 0;  /* read fails */
		}


		private static int test_eof (lua_State L, StreamProxy f) {
		  int c = getc(f);
		  ungetc(c, f);
		  lua_pushlstring(L, null, 0);
		  return (c != EOF) ? 1 : 0;
		}


		private static int read_line (lua_State L, StreamProxy f) {
		  luaL_Buffer b = new luaL_Buffer();
		  luaL_buffinit(L, b);
		  return 0;
		}


		private static int read_chars (lua_State L, StreamProxy f, uint n) {
			return 0;
		}


		private static int g_read (lua_State L, StreamProxy f, int first) {
		  int nargs = lua_gettop(L) - 1;
		  int success;
		  int n;
		  clearerr(f);
		  if (nargs == 0) {  /* no arguments? */
			success = read_line(L, f);
			n = first+1;  /* to return 1 result */
		  }
		  else {  /* ensure stack space for all results and for auxlib's buffer */
			luaL_checkstack(L, nargs+LUA_MINSTACK, "too many arguments");
			success = 1;
			for (n = first; (nargs-- != 0) && (success!=0); n++) {
			  if (lua_type(L, n) == LUA_TNUMBER) {
				uint l = (uint)lua_tointeger(L, n);
				success = (l == 0) ? test_eof(L, f) : read_chars(L, f, l);
			  }
			  else {
				CharPtr p = lua_tostring(L, n);
				luaL_argcheck(L, (p!=null) && (p[0] == '*'), n, "invalid option");
				switch (p[1]) {
				  case 'n':  /* number */
					success = read_number(L, f);
					break;
				  case 'l':  /* line */
					success = read_line(L, f);
					break;
				  case 'a':  /* file */
					read_chars(L, f, ~((uint)0));  /* read MAX_uint chars */
					success = 1; /* always success */
					break;
				  default:
					return luaL_argerror(L, n, "invalid format");
				}
			  }
			}
		  }
		  if (ferror(f)!=0)
			return pushresult(L, 0, null);
		  if (success==0) {
			lua_pop(L, 1);  /* remove last result */
			lua_pushnil(L);  /* push nil instead */
		  }
		  return n - first;
		}


		private static int io_read (lua_State L) {
		  return g_read(L, getiofile(L, IO_INPUT), 1);
		}


		private static int f_read (lua_State L) {
		  return g_read(L, tofile(L), 2);
		}


		private static int io_readline (lua_State L) {
		  StreamProxy f = (lua_touserdata(L, lua_upvalueindex(1)) as FilePtr).file;
		  int sucess;
		  if (f == null)  /* file is already closed? */
			luaL_error(L, "file is already closed");
		  sucess = read_line(L, f);
		  if (ferror(f)!=0)
			return luaL_error(L, "%s", strerror(errno));
		  if (sucess != 0) return 1;
		  else {  /* EOF */
			if (lua_toboolean(L, lua_upvalueindex(2)) != 0) {  /* generator created file? */
			  lua_settop(L, 0);
			  lua_pushvalue(L, lua_upvalueindex(1));
			  aux_close(L);  /* close it */
			}
			return 0;
		  }
		}

		/* }====================================================== */


		private static int g_write (lua_State L, StreamProxy f, int arg) {
		  return 0;
		}


		private static int io_write (lua_State L) {
		  return g_write(L, getiofile(L, IO_OUTPUT), 1);
		}


		private static int f_write (lua_State L) {
		  return g_write(L, tofile(L), 2);
		}

		

		private static int f_seek (lua_State L) {
			return 0;
		}

		private static int f_setvbuf (lua_State L) {
		  CharPtr[] modenames = { "no", "full", "line", null };
		  int[] mode = { _IONBF, _IOFBF, _IOLBF };
		  StreamProxy f = tofile(L);
		  int op = luaL_checkoption(L, 2, null, modenames);
		  lua_Integer sz = luaL_optinteger(L, 3, LUAL_BUFFERSIZE);
		  int res = setvbuf(f, null, mode[op], (uint)sz);
		  return pushresult(L, (res == 0) ? 1 : 0, null);
		}



		private static int io_flush (lua_State L) {
			return pushresult(L, fflush(getiofile(L, IO_OUTPUT)) == 0 ? 1 : 0, null);
		}


		private static int f_flush (lua_State L) {
			return pushresult(L, fflush(tofile(L)) == 0 ? 1 : 0, null);
		}


		private readonly static luaL_reg[] iolib = {
		  new luaL_reg("close", io_close),
		  new luaL_reg("flush", io_flush),
		  new luaL_reg("input", io_input),
		  new luaL_reg("lines", io_lines),
		  new luaL_reg("open", io_open),
		  new luaL_reg("output", io_output),
		  new luaL_reg("popen", io_popen),
		  new luaL_reg("read", io_read),
		  new luaL_reg("tmpfile", io_tmpfile),
		  new luaL_reg("type", io_type),
		  new luaL_reg("write", io_write),
		  new luaL_reg(null, null)
		};


		private readonly static luaL_reg[] flib = {
		  new luaL_reg("close", io_close),
		  new luaL_reg("flush", f_flush),
		  new luaL_reg("lines", f_lines),
		  new luaL_reg("read", f_read),
		  new luaL_reg("seek", f_seek),
		  new luaL_reg("setvbuf", f_setvbuf),
		  new luaL_reg("write", f_write),
		  new luaL_reg("__gc", io_gc),
		  new luaL_reg("__tostring", io_tostring),
		  new luaL_reg(null, null)
		};


		private static void createmeta (lua_State L) {
		  luaL_newmetatable(L, LUA_FILEHANDLE);  /* create metatable for file files */
		  lua_pushvalue(L, -1);  /* push metatable */
		  lua_setfield(L, -2, "__index");  /* metatable.__index = metatable */
		}


		private static void createstdfile (lua_State L, StreamProxy f, int k, CharPtr fname) {
		  newfile(L).file = f;
		  if (k > 0) {
			lua_pushvalue(L, -1);
			lua_rawseti(L, LUA_ENVIRONINDEX, k);
		  }
		  lua_pushvalue(L, -2);  /* copy environment */
		  lua_setfenv(L, -2);  /* set it */
		  lua_setfield(L, -3, fname);
		}


		private static void newfenv (lua_State L, lua_CFunction cls) {
		  lua_createtable(L, 0, 1);
		  lua_pushcfunction(L, cls);
		  lua_setfield(L, -2, "__close");
		}


		public static int luaopen_io (lua_State L) {
		  createmeta(L);
		  /* create (private) environment (with fields IO_INPUT, IO_OUTPUT, __close) */
		  newfenv(L, io_fclose);
		  lua_replace(L, LUA_ENVIRONINDEX);
		  /* open library */
		  /* create (and set) default files */
		  newfenv(L, io_noclose);  /* close function for default files */
		  createstdfile(L, stdin, IO_INPUT, "stdin");
		  createstdfile(L, stdout, IO_OUTPUT, "stdout");
		  createstdfile(L, stderr, 0, "stderr");
		  lua_pop(L, 1);  /* pop environment for default files */
		  lua_getfield(L, -1, "popen");
		  newfenv(L, io_pclose);  /* create environment for 'popen' */
		  lua_setfenv(L, -2);  /* set fenv for 'popen' */
		  lua_pop(L, 1);  /* pop 'popen' */
		  return 1;
		}

	}
}
