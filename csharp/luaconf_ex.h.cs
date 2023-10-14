using System;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using AT.MIN;

namespace lua40mod
{
	using LUA_INTEGER	= System.Int32;
	using LUA_NUMBER	= System.Double;
	using LUAI_UACNUMBER	= System.Double;
	using LUA_INTFRM_T		= System.Int64;
	using TValue = Lua.lua_TValue;
	//using lua_Number = System.Double;
	
	using lua_Number = System.Double;
	using clock_t = System.Int64;
	
	public partial class Lua
	{
		public const int BUFSIZ = 256;
		
		
	/*
		** ==================================================================
		** Search for "@@" to find all configurable definitions.
		** ===================================================================
		*/


		/*
		@@ LUA_ANSI controls the use of non-ansi features.
		** CHANGE it (define it) if you want Lua to avoid the use of any
		** non-ansi feature or library.
		*/
		//#if defined(__STRICT_ANSI__)
		//#define LUA_ANSI
		//#endif


		//#if !defined(LUA_ANSI) && _WIN32
		//#define LUA_WIN
		//#endif

		//#if defined(LUA_USE_LINUX)
		//#define LUA_USE_POSIX
		//#define LUA_USE_DLOPEN		/* needs an extra library: -ldl */
		//#define LUA_USE_READLINE	/* needs some extra libraries */
		//#endif

		//#if defined(LUA_USE_MACOSX)
		//#define LUA_USE_POSIX
		//#define LUA_DL_DYLD		/* does not need extra library */
		//#endif



		/*
		@@ LUA_USE_POSIX includes all functionallity listed as X/Open System
		@* Interfaces Extension (XSI).
		** CHANGE it (define it) if your system is XSI compatible.
		*/
		//#if defined(LUA_USE_POSIX)
		//#define LUA_USE_MKSTEMP
		//#define LUA_USE_ISATTY
		//#define LUA_USE_POPEN
		//#define LUA_USE_ULONGJMP
		//#endif


		/*
		@@ LUA_PATH and LUA_CPATH are the names of the environment variables that
		@* Lua check to set its paths.
		@@ LUA_INIT is the name of the environment variable that Lua
		@* checks for initialization code.
		** CHANGE them if you want different names.
		*/
		public const string LUA_PATH = "LUA_PATH";
		public const string LUA_CPATH = "LUA_CPATH";
		public const string LUA_INIT = "LUA_INIT";


		/*
		@@ LUA_PATH_DEFAULT is the default path that Lua uses to look for
		@* Lua libraries.
		@@ LUA_CPATH_DEFAULT is the default path that Lua uses to look for
		@* C libraries.
		** CHANGE them if your machine has a non-conventional directory
		** hierarchy or if you want to install your libraries in
		** non-conventional directories.
		*/
		#if _WIN32
		/*
		** In Windows, any exclamation mark ('!') in the path is replaced by the
		** path of the directory of the executable file of the current process.
		*/
		public const string LUA_LDIR = "!\\lua\\";
		public const string LUA_CDIR = "!\\";
		public const string LUA_PATH_DEFAULT =
				".\\?.lua;"  + LUA_LDIR + "?.lua;"  + LUA_LDIR + "?\\init.lua;"
							 + LUA_CDIR + "?.lua;"  + LUA_CDIR + "?\\init.lua";
		public const string LUA_CPATH_DEFAULT =
			".\\?.dll;"  + LUA_CDIR + "?.dll;" + LUA_CDIR + "loadall.dll";

		#else
		public const string LUA_ROOT	= "/usr/local/";
		public const string LUA_LDIR	= LUA_ROOT + "share/lua/5.1/";
		public const string LUA_CDIR	= LUA_ROOT + "lib/lua/5.1/";
		public const string LUA_PATH_DEFAULT  =
				"./?.lua;"  + LUA_LDIR + "?.lua;"  + LUA_LDIR + "?/init.lua;" +
							LUA_CDIR + "?.lua;"  + LUA_CDIR + "?/init.lua";
		public const string LUA_CPATH_DEFAULT =
			"./?.so;"  + LUA_CDIR + "?.so;" + LUA_CDIR + "loadall.so";
#endif


		/*
		@@ LUA_DIRSEP is the directory separator (for submodules).
		** CHANGE it if your machine does not use "/" as the directory separator
		** and is not Windows. (On Windows Lua automatically uses "\".)
		*/
		#if _WIN32
		public const string LUA_DIRSEP = "\\";
		#else
		public const string LUA_DIRSEP = "/";
#endif


		/*
		@@ LUA_PATHSEP is the character that separates templates in a path.
		@@ LUA_PATH_MARK is the string that marks the substitution points in a
		@* template.
		@@ LUA_EXECDIR in a Windows path is replaced by the executable's
		@* directory.
		@@ LUA_IGMARK is a mark to ignore all before it when bulding the
		@* luaopen_ function name.
		** CHANGE them if for some reason your system cannot use those
		** characters. (E.g., if one of those characters is a common character
		** in file/directory names.) Probably you do not need to change them.
		*/
		public const string LUA_PATHSEP = ";";
		public const string LUA_PATH_MARK = "?";
		public const string LUA_EXECDIR = "!";
		public const string LUA_IGMARK = "-";


		/*
		@@ LUA_INTEGER is the integral type used by lua_pushinteger/lua_tointeger.
		** CHANGE that if ptrdiff_t is not adequate on your machine. (On most
		** machines, ptrdiff_t gives a good choice between int or long.)
		*/
		//#define LUA_INTEGER	ptrdiff_t


		/*
		@@ LUA_API is a mark for all core API functions.
		@@ LUALIB_API is a mark for all standard library functions.
		** CHANGE them if you need to define those functions in some special way.
		** For instance, if you want to create one Windows DLL with the core and
		** the libraries, you may want to use the following definition (define
		** LUA_BUILD_AS_DLL to get it).
		*/
		//#if LUA_BUILD_AS_DLL

		//#if defined(LUA_CORE) || defined(LUA_LIB)
		//#define LUA_API __declspec(dllexport)
		//#else
		//#define LUA_API __declspec(dllimport)
		//#endif

		//#else

		//#define LUA_API		extern

		//#endif

		/* more often than not the libs go together with the core */
		//#define LUALIB_API	LUA_API


		/*
		@@ LUAI_FUNC is a mark for all extern functions that are not to be
		@* exported to outside modules.
		@@ LUAI_DATA is a mark for all extern (const) variables that are not to
		@* be exported to outside modules.
		** CHANGE them if you need to mark them in some special way. Elf/gcc
		** (versions 3.2 and later) mark them as "hidden" to optimize access
		** when Lua is compiled as a shared library.
		*/
		//#if defined(luaall_c)
		//#define LUAI_FUNC	static
		//#define LUAI_DATA	/* empty */

		//#elif defined(__GNUC__) && ((__GNUC__*100 + __GNUC_MINOR__) >= 302) && \
		//      defined(__ELF__)
		//#define LUAI_FUNC	__attribute__((visibility("hidden"))) extern
		//#define LUAI_DATA	LUAI_FUNC

		//#else
		//#define LUAI_FUNC	extern
		//#define LUAI_DATA	extern
		//#endif



		/*
		@@ LUA_QL describes how error messages quote program elements.
		** CHANGE it if you want a different appearance.
		*/
		public static CharPtr LUA_QL(string x)	{return "'" + x + "'";}
		public static CharPtr LUA_QS {get {return LUA_QL("%s"); }}


		/*
		@@ LUA_IDSIZE gives the maximum size for the description of the source
		@* of a function in debug information.
		** CHANGE it if you want a different size.
		*/
		public const int LUA_IDSIZE	= 60;


		/*
		** {==================================================================
		** Stand-alone configuration
		** ===================================================================
		*/

		//#if lua_c || luaall_c

		/*
		@@ lua_stdin_is_tty detects whether the standard input is a 'tty' (that
		@* is, whether we're running lua interactively).
		** CHANGE it if you have a better definition for non-POSIX/non-Windows
		** systems.
		*/
		#if LUA_USE_ISATTY
		//#include <unistd.h>
		//#define lua_stdin_is_tty()	isatty(0)
		#elif LUA_WIN
		//#include <io.h>
		//#include <stdio.h>
		//#define lua_stdin_is_tty()	_isatty(_fileno(stdin))
		#else
		public static int lua_stdin_is_tty() { return 1; }  /* assume stdin is a tty */
		#endif


		/*
		@@ LUA_PROMPT is the default prompt used by stand-alone Lua.
		@@ LUA_PROMPT2 is the default continuation prompt used by stand-alone Lua.
		** CHANGE them if you want different prompts. (You can also change the
		** prompts dynamically, assigning to globals _PROMPT/_PROMPT2.)
		*/
		public const string LUA_PROMPT		= "> ";
		public const string LUA_PROMPT2		= ">> ";


		/*
		@@ LUA_PROGNAME is the default name for the stand-alone Lua program.
		** CHANGE it if your stand-alone interpreter has a different name and
		** your system is not able to detect that name automatically.
		*/
		public const string LUA_PROGNAME		= "lua";


		/*
		@@ LUA_MAXINPUT is the maximum length for an input line in the
		@* stand-alone interpreter.
		** CHANGE it if you need longer lines.
		*/
		public const int LUA_MAXINPUT	= 512;


		/*
		@@ lua_readline defines how to show a prompt and then read a line from
		@* the standard input.
		@@ lua_saveline defines how to "save" a read line in a "history".
		@@ lua_freeline defines how to free a line read by lua_readline.
		** CHANGE them if you want to improve this functionality (e.g., by using
		** GNU readline and history facilities).
		*/
#if LUA_USE_READLINE
		//#include <stdio.h>
		//#include <readline/readline.h>
		//#include <readline/history.h>
		//#define lua_readline(L,b,p)	((void)L, ((b)=readline(p)) != null)
		//#define lua_saveline(L,idx) \
		//	if (lua_strlen(L,idx) > 0)  /* non-empty line? */ \
		//	  add_history(lua_tostring(L, idx));  /* add it to history */
		//#define lua_freeline(L,b)	((void)L, free(b))
#else
		public static bool lua_readline(lua_State L, CharPtr b, CharPtr p)
		{
			fputs(p, stdout);
			fflush(stdout);		/* show prompt */
			return (fgets(b, stdin) != null);  /* get line */
		}
		public static void lua_saveline(lua_State L, int idx)	{}
		public static void lua_freeline(lua_State L, CharPtr b)	{}
#endif

//#endif

		/* }================================================================== */


		/*
		@@ LUAI_GCPAUSE defines the default pause between garbage-collector cycles
		@* as a percentage.
		** CHANGE it if you want the GC to run faster or slower (higher values
		** mean larger pauses which mean slower collection.) You can also change
		** this value dynamically.
		*/
		public const int LUAI_GCPAUSE	= 200;  /* 200% (wait memory to double before next GC) */


		/*
		@@ LUAI_GCMUL defines the default speed of garbage collection relative to
		@* memory allocation as a percentage.
		** CHANGE it if you want to change the granularity of the garbage
		** collection. (Higher values mean coarser collections. 0 represents
		** infinity, where each step performs a full collection.) You can also
		** change this value dynamically.
		*/
		public const int LUAI_GCMUL	= 200; /* GC runs 'twice the speed' of memory allocation */

		/*
		@@ LUA_COMPAT_GETN controls compatibility with old getn behavior.
		** CHANGE it (define it) if you want exact compatibility with the
		** behavior of setn/getn in Lua 5.0.
		*/
		//#undef LUA_COMPAT_GETN /* dotnet port doesn't define in the first place */

		/*
		@@ LUA_COMPAT_LOADLIB controls compatibility about global loadlib.
		** CHANGE it to undefined as soon as you do not need a global 'loadlib'
		** function (the function is still available as 'package.loadlib').
		*/
		//#undef LUA_COMPAT_LOADLIB /* dotnet port doesn't define in the first place */

		/*
		@@ LUA_COMPAT_VARARG controls compatibility with old vararg feature.
		** CHANGE it to undefined as soon as your programs use only '...' to
		** access vararg parameters (instead of the old 'arg' table).
		*/
		//#define LUA_COMPAT_VARARG /* defined higher up */

		/*
		@@ LUA_COMPAT_MOD controls compatibility with old math.mod function.
		** CHANGE it to undefined as soon as your programs use 'math.fmod' or
		** the new '%' operator instead of 'math.mod'.
		*/
		//#define LUA_COMPAT_MOD /* defined higher up */

		/*
		@@ LUA_COMPAT_LSTR controls compatibility with old long string nesting
		@* facility.
		** CHANGE it to 2 if you want the old behaviour, or undefine it to turn
		** off the advisory error when nesting [[...]].
		*/
		//#define LUA_COMPAT_LSTR		1
		//#define LUA_COMPAT_LSTR /* defined higher up */

		/*
		@@ LUA_COMPAT_GFIND controls compatibility with old 'string.gfind' name.
		** CHANGE it to undefined as soon as you rename 'string.gfind' to
		** 'string.gmatch'.
		*/
		//#define LUA_COMPAT_GFIND /* defined higher up */

		/*
		@@ LUA_COMPAT_OPENLIB controls compatibility with old 'luaL_openlib'
		@* behavior.
		** CHANGE it to undefined as soon as you replace to 'luaL_register'
		** your uses of 'luaL_openlib'
		*/
		//#define LUA_COMPAT_OPENLIB /* defined higher up */



		/*
		@@ luai_apicheck is the assert macro used by the Lua-C API.
		** CHANGE luai_apicheck if you want Lua to perform some checks in the
		** parameters it gets from API calls. This may slow down the interpreter
		** a bit, but may be quite useful when debugging C code that interfaces
		** with Lua. A useful redefinition is to use assert.h.
		*/
		#if LUA_USE_APICHECK
			public static void luai_apicheck(lua_State L, bool o)	{Debug.Assert(o);}
			public static void luai_apicheck(lua_State L, int o) {Debug.Assert(o != 0);}
		#else
			public static void luai_apicheck(lua_State L, bool o)	{}
			public static void luai_apicheck(lua_State L, int o) { }
		#endif


		/*
		@@ LUAI_BITSINT defines the number of bits in an int.
		** CHANGE here if Lua cannot automatically detect the number of bits of
		** your machine. Probably you do not need to change this.
		*/
		/* avoid overflows in comparison */
		//#if INT_MAX-20 < 32760
		//public const int LUAI_BITSINT	= 16
		//#elif INT_MAX > 2147483640L
		/* int has at least 32 bits */
		public const int LUAI_BITSINT	= 32;
		//#else
		//#error "you must define LUA_BITSINT with number of bits in an integer"
		//#endif


		/*
		@@ LUAI_UINT32 is an unsigned integer with at least 32 bits.
		@@ LUAI_INT32 is an signed integer with at least 32 bits.
		@@ LUAI_UMEM is an unsigned integer big enough to count the total
		@* memory used by Lua.
		@@ LUAI_MEM is a signed integer big enough to count the total memory
		@* used by Lua.
		** CHANGE here if for some weird reason the default definitions are not
		** good enough for your machine. (The definitions in the 'else'
		** part always works, but may waste space on machines with 64-bit
		** longs.) Probably you do not need to change this.
		*/
		//#if LUAI_BITSINT >= 32
		//#define LUAI_UINT32	unsigned int
		//#define LUAI_INT32	int
		//#define LUAI_MAXINT32	INT_MAX
		//#define LUAI_UMEM	uint
		//#define LUAI_MEM	ptrdiff_t
		//#else
		///* 16-bit ints */
		//#define LUAI_UINT32	unsigned long
		//#define LUAI_INT32	long
		//#define LUAI_MAXINT32	LONG_MAX
		//#define LUAI_UMEM	unsigned long
		//#define LUAI_MEM	long
		//#endif


		/*
		@@ LUAI_MAXCALLS limits the number of nested calls.
		** CHANGE it if you need really deep recursive calls. This limit is
		** arbitrary; its only purpose is to stop infinite recursion before
		** exhausting memory.
		*/
		public const int LUAI_MAXCALLS	= 20000;


		/*
		@@ LUAI_MAXCSTACK limits the number of Lua stack slots that a C function
		@* can use.
		** CHANGE it if you need lots of (Lua) stack space for your C
		** functions. This limit is arbitrary; its only purpose is to stop C
		** functions to consume unlimited stack space. (must be smaller than
		** -LUA_REGISTRYINDEX)
		*/
		public const int LUAI_MAXCSTACK	= 8000;



		/*
		** {==================================================================
		** CHANGE (to smaller values) the following definitions if your system
		** has a small C stack. (Or you may want to change them to larger
		** values if your system has a large C stack and these limits are
		** too rigid for you.) Some of these constants control the size of
		** stack-allocated arrays used by the compiler or the interpreter, while
		** others limit the maximum number of recursive calls that the compiler
		** or the interpreter can perform. Values too large may cause a C stack
		** overflow for some forms of deep constructs.
		** ===================================================================
		*/


		/*
		@@ LUAI_MAXCCALLS is the maximum depth for nested C calls (short) and
		@* syntactical nested non-terminals in a program.
		*/
		public const int LUAI_MAXCCALLS		= 200;


		/*
		@@ LUAI_MAXVARS is the maximum number of local variables per function
		@* (must be smaller than 250).
		*/
		public const int LUAI_MAXVARS		= 200;


		/*
		@@ LUAI_MAXUPVALUES is the maximum number of upvalues per function
		@* (must be smaller than 250).
		*/
		public const int LUAI_MAXUPVALUES	= 60;


		/*
		@@ LUAL_BUFFERSIZE is the buffer size used by the lauxlib buffer system.
		*/
//		public const int LUAL_BUFFERSIZE		= 1024; // BUFSIZ; todo: check this - mjf

		/* }================================================================== */




		/*
		** {==================================================================
		@@ LUA_NUMBER is the type of numbers in Lua.
		** CHANGE the following definitions only if you want to build Lua
		** with a number type different from double. You may also need to
		** change lua_number2int & lua_number2integer.
		** ===================================================================
		*/

		//#define LUA_NUMBER_DOUBLE
		//#define LUA_NUMBER	double	/* declared in dotnet build with using statement */

		/*
		@@ LUAI_UACNUMBER is the result of an 'usual argument conversion'
		@* over a number.
		*/
		//#define LUAI_UACNUMBER	double /* declared in dotnet build with using statement */


		/*
		@@ LUA_NUMBER_SCAN is the format for reading numbers.
		@@ LUA_NUMBER_FMT is the format for writing numbers.
		@@ lua_number2str converts a number to a string.
		@@ LUAI_MAXNUMBER2STR is maximum size of previous conversion.
		@@ lua_str2number converts a string to a number.
		*/
		public const string LUA_NUMBER_SCAN = "%lf";
		public const string LUA_NUMBER_FMT = "%.14g";
		public static void lua_number2str(CharPtr s, double n) { sprintf(s, LUA_NUMBER_FMT, n); } //s = String.Format("{0}", n);
		public const int LUAI_MAXNUMBER2STR = 32; /* 16 digits, sign, point, and \0 */
		public static double lua_str2number(CharPtr s, out CharPtr p) { return strtod(s, out p); }


		/*
		@@ The luai_num* macros define the primitive operations over numbers.
		*/
		#if LUA_CORE
		//#include <math.h>
		public delegate lua_Number op_delegate(lua_Number a, lua_Number b);
		public static lua_Number luai_numadd(lua_Number a, lua_Number b) { return ((a) + (b)); }
		public static lua_Number luai_numsub(lua_Number a, lua_Number b) { return ((a) - (b)); }
		public static lua_Number luai_nummul(lua_Number a, lua_Number b) { return ((a) * (b)); }
		public static lua_Number luai_numdiv(lua_Number a, lua_Number b) { return ((a) / (b)); }
		public static lua_Number luai_nummod(lua_Number a, lua_Number b) { return ((a) - floor((a) / (b)) * (b)); }
		public static lua_Number luai_numpow(lua_Number a, lua_Number b) { return (pow(a, b)); }
		public static lua_Number luai_numunm(lua_Number a) { return (-(a)); }
		public static bool luai_numeq(lua_Number a, lua_Number b) { return ((a) == (b)); }
		public static bool luai_numlt(lua_Number a, lua_Number b) { return ((a) < (b)); }
		public static bool luai_numle(lua_Number a, lua_Number b) { return ((a) <= (b)); }
		public static bool luai_numisnan(lua_Number a) { return lua_Number.IsNaN(a); }
		#endif


		/*
		@@ lua_number2int is a macro to convert lua_Number to int.
		@@ lua_number2integer is a macro to convert lua_Number to lua_Integer.
		** CHANGE them if you know a faster way to convert a lua_Number to
		** int (with any rounding method and without throwing errors) in your
		** system. In Pentium machines, a naive typecast from double to int
		** in C is extremely slow, so any alternative is worth trying.
		*/

		/* On a Pentium, resort to a trick */
		//#if defined(LUA_NUMBER_DOUBLE) && !defined(LUA_ANSI) && !defined(__SSE2__) && \
		//	(defined(__i386) || defined (_M_IX86) || defined(__i386__))

		/* On a Microsoft compiler, use assembler */
		//#if defined(_MSC_VER)

		//#define lua_number2int(i,d)   __asm fld d   __asm fistp i
		//#define lua_number2integer(i,n)		lua_number2int(i, n)

		/* the next trick should work on any Pentium, but sometimes clashes
		   with a DirectX idiosyncrasy */
		//#else

		//union luai_Cast { double l_d; long l_l; };
		//#define lua_number2int(i,d) \
		//  { volatile union luai_Cast u; u.l_d = (d) + 6755399441055744.0; (i) = u.l_l; }
		//#define lua_number2integer(i,n)		lua_number2int(i, n)

		//#endif


		/* this option always works, but may be slow */
		//#else
		//#define lua_number2int(i,d)	((i)=(int)(d))
		//#define lua_number2integer(i,d)	((i)=(lua_Integer)(d))

		//#endif

		private static void lua_number2int(out int i,lua_Number d)   {i = (int)d;}
		private static void lua_number2integer(out int i, lua_Number n) { i = (int)n; }

		/* }================================================================== */


		/*
		@@ LUAI_USER_ALIGNMENT_T is a type that requires maximum alignment.
		** CHANGE it if your system requires alignments larger than double. (For
		** instance, if your system supports long doubles and they must be
		** aligned in 16-byte boundaries, then you should add long double in the
		** union.) Probably you do not need to change this.
		*/
		//#define LUAI_USER_ALIGNMENT_T	union { double u; void *s; long l; }

		public class LuaException : System.Exception
		{
			public lua_State L;
			public lua_longjmp c;

			public LuaException(lua_State L, lua_longjmp c) { this.L = L; this.c = c; }
		}

		/*
		@@ LUAI_THROW/LUAI_TRY define how Lua does exception handling.
		** CHANGE them if you prefer to use longjmp/setjmp even with C++
		** or if want/don't to use _longjmp/_setjmp instead of regular
		** longjmp/setjmp. By default, Lua handles errors with exceptions when
		** compiling as C++ code, with _longjmp/_setjmp when asked to use them,
		** and with longjmp/setjmp otherwise.
		*/
		//#if defined(__cplusplus)
		///* C++ exceptions */
		public static void LUAI_THROW(lua_State L, lua_longjmp c)	{throw new LuaException(L, c);}
		//#define LUAI_TRY(L,c,a)	try { a } catch(...) \
		//    { if ((c).status == 0) (c).status = -1; }
		public static void LUAI_TRY(lua_State L, lua_longjmp c, object a) {
			if (c.status == 0) c.status = -1;
		}
		//#define luai_jmpbuf	int  /* dummy variable */

		//#elif defined(LUA_USE_ULONGJMP)
		///* in Unix, try _longjmp/_setjmp (more efficient) */
		//#define LUAI_THROW(L,c)	_longjmp((c).b, 1)
		//#define LUAI_TRY(L,c,a)	if (_setjmp((c).b) == 0) { a }
		//#define luai_jmpbuf	jmp_buf

		//#else
		///* default handling with long jumps */
		//public static void LUAI_THROW(lua_State L, lua_longjmp c) { c.b(1); }
		//#define LUAI_TRY(L,c,a)	if (setjmp((c).b) == 0) { a }
		//#define luai_jmpbuf	jmp_buf

		//#endif


		/*
		@@ LUA_MAXCAPTURES is the maximum number of captures that a pattern
		@* can do during pattern-matching.
		** CHANGE it if you need more captures. This limit is arbitrary.
		*/
		public const int LUA_MAXCAPTURES		= 32;


		/*
		@@ lua_tmpnam is the function that the OS library uses to create a
		@* temporary name.
		@@ LUA_TMPNAMBUFSIZE is the maximum size of a name created by lua_tmpnam.
		** CHANGE them if you have an alternative to tmpnam (which is considered
		** insecure) or if you want the original tmpnam anyway.  By default, Lua
		** uses tmpnam except when POSIX is available, where it uses mkstemp.
		*/
		//#if loslib_c || luaall_c

		#if LUA_USE_MKSTEMP
		//#include <unistd.h>
		public const int LUA_TMPNAMBUFSIZE	= 32;
		//#define lua_tmpnam(b,e)	{ \
		//    strcpy(b, "/tmp/lua_XXXXXX"); \
		//    e = mkstemp(b); \
		//    if (e != -1) close(e); \
		//    e = (e == -1); }

		#else
			public const int LUA_TMPNAMBUFSIZE	= L_tmpnam;
			public static void lua_tmpnam(CharPtr b, ref int e)		{ e = (tmpnam(b) == null) ? 1 : 0; }
		#endif

		//#endif


		/*
		@@ lua_popen spawns a new process connected to the current one through
		@* the file streams.
		** CHANGE it if you have a way to implement it in your system.
		*/
		//#if LUA_USE_POPEN

		//#define lua_popen(L,c,m)	((void)L, fflush(null), popen(c,m))
		//#define lua_pclose(L,file)	((void)L, (pclose(file) != -1))

		//#elif LUA_WIN

		//#define lua_popen(L,c,m)	((void)L, _popen(c,m))
		//#define lua_pclose(L,file)	((void)L, (_pclose(file) != -1))

		//#else

		public static StreamProxy lua_popen(lua_State L, CharPtr c, CharPtr m) { luaL_error(L, LUA_QL("popen") + " not supported"); return null; }
		public static int lua_pclose(lua_State L, StreamProxy file) { return 0; }
	
		//#endif

		/*
		@@ LUA_DL_* define which dynamic-library system Lua should use.
		** CHANGE here if Lua has problems choosing the appropriate
		** dynamic-library system for your platform (either Windows' DLL, Mac's
		** dyld, or Unix's dlopen). If your system is some kind of Unix, there
		** is a good chance that it has dlopen, so LUA_DL_DLOPEN will work for
		** it.  To use dlopen you also need to adapt the src/Makefile (probably
		** adding -ldl to the linker options), so Lua does not select it
		** automatically.  (When you change the makefile to add -ldl, you must
		** also add -DLUA_USE_DLOPEN.)
		** If you do not want any kind of dynamic library, undefine all these
		** options.
		** By default, _WIN32 gets LUA_DL_DLL and MAC OS X gets LUA_DL_DYLD.
		*/
		//#if LUA_USE_DLOPEN
		//#define LUA_DL_DLOPEN
		//#endif

		//#if LUA_WIN
		//#define LUA_DL_DLL
		//#endif


		/*
		@@ LUAI_EXTRASPACE allows you to add user-specific data in a lua_State
		@* (the data goes just *before* the lua_State pointer).
		** CHANGE (define) this if you really need that. This value must be
		** a multiple of the maximum alignment required for your machine.
		*/
		public const int LUAI_EXTRASPACE		= 0;


		/*
		@@ luai_userstate* allow user-specific actions on threads.
		** CHANGE them if you defined LUAI_EXTRASPACE and need to do something
		** extra when a thread is created/deleted/resumed/yielded.
		*/
		public static void luai_userstateopen(lua_State L)					{}
		public static void luai_userstateclose(lua_State L)					{}
		public static void luai_userstatethread(lua_State L, lua_State L1)	{}
		public static void luai_userstatefree(lua_State L)					{}
		public static void luai_userstateresume(lua_State L,int n)			{}
		public static void luai_userstateyield(lua_State L,int n)			{}


		/*
		@@ LUA_INTFRMLEN is the length modifier for integer conversions
		@* in 'string.format'.
		@@ LUA_INTFRM_T is the integer type correspoding to the previous length
		@* modifier.
		** CHANGE them if your system supports long long or does not support long.
		*/

		#if LUA_USELONGLONG

		public const string LUA_INTFRMLEN		= "ll";
		//#define LUA_INTFRM_T		long long

		#else

		public const string LUA_INTFRMLEN = "l";
		//#define LUA_INTFRM_T		long			/* declared in dotnet build with using statement */

		#endif



		/* =================================================================== */

		/*
		** Local configuration. You can use this space to add your redefinitions
		** without modifying the main part of the file.
		*/

		
		
		
		
		
		
		
		public const int TYPE_STDIN = 0;
		public const int TYPE_STDOUT = 1;
		public const int TYPE_STDERR = 2;
		
		public class StreamProxy 
		{	
			private Stream stream;
			private int type = -1;
			
			public StreamProxy(int type)
			{
				this.type = type;
			}
			
			public StreamProxy(Stream stream)
			{
				this.stream = stream;
			}
			
			public void Write(byte[] buffer, int offset, int count)
			{
				if (type >= 0)
				{
					if (libImpl != null)
					{
						libImpl.Write(type, buffer, offset, count);
					}
				}
				else
				{
					if (this.stream != null)
					{
						this.stream.Write(buffer, offset, count);
					}
				}
			}
			
			public int Read(byte[] buffer, int offset, int count)
			{
				if (type >= 0)
				{
					if (libImpl != null)
					{
						return libImpl.Read(type, buffer, offset, count);
					}
					return 0;
				}
				else
				{
					if (stream != null)
					{
						return stream.Read(buffer, offset, count);
					}
					return 0;
				}
			}
			
			public int ReadByte()
			{
				if (type >= 0)
				{
					if (libImpl != null)
					{
						return libImpl.ReadByte(type);
					}
					return 0;
				}
				else
				{
					if (this.stream != null)
					{
						return this.stream.ReadByte();
					}
					return 0;
				}
			}
			
			public void ungetc()
			{
				if (this.stream != null)
				{
					if (this.stream.Position > 0)
					{
						 this.stream.Seek(-1, SeekOrigin.Current);
					}
				}
			}
			
			public bool isEof()
			{
				if (type >= 0)
				{
					if (libImpl != null)
					{
						return libImpl.isEof(type);
					}
					return false;
				}
				else
				{
					if (this.stream != null)
					{
						return stream.Position >= stream.Length;
					}
					return true;
				}
			}
			
			public void close()
			{
				if (stream != null)
				{
					try
					{
						stream.Flush();
						stream.Close();
					}
					catch { }
				}
			}
			
			public int flush()
			{
				if (type >= 0)
				{
					if (libImpl != null)
					{
						libImpl.flush(type);
					}
					return 0;
				}
				else
				{
					int result = 0;
					if (stream != null)
					{
						try
						{
							stream.Flush();
						} 
						catch 
						{
							result = 1;
						}
					}
					else
					{
						result = 1;
					}
					return result;
				}
			}
			
			public int seek(long offset, int origin)
			{
				if (stream != null)
				{
					try
					{
						switch (origin)
						{
							case SEEK_SET: 
								stream.Seek(offset, SeekOrigin.Begin);
								return 0;
							
							case SEEK_CUR:
								stream.Seek(offset, SeekOrigin.Current);
								return 0;
							
							case SEEK_END:
								stream.Seek(offset, SeekOrigin.End);
								return 0;
							
							default:
								return 1;
						}
					}
					catch
					{
						return 1;
					}
				}
				else
				{
					return 1;
				}
			}
			
			public long getPosition()
			{
				if (stream != null)
				{
					return stream.Position;
				}
				return 0;
			}
			
			public void puts(string str)
			{
				if (type >= 0)
				{
					if (libImpl != null)
					{
						libImpl.writeString(str.ToString());
					}
				}
				else
				{
					if (this.stream != null)
					{
						byte[] byteArray = System.Text.Encoding.Default.GetBytes(str);
						stream.Write(byteArray, 0, byteArray.Length);
					}
				}
			}
			
			public string readline()
			{
				if (type >= 0)
				{
					if (libImpl != null)
					{
						return libImpl.readLine(type);
					}
					return null;
				}
				else
				{
					throw new System.NotImplementedException();
				}
			}
		}
		
		public interface LibImpl
		{
			int ReadByte(int type);
			int Read(int type, byte[] buffer, int offset, int count);
			void Write(int type, byte[] buffer, int offset, int count);
			bool isEof(int type);
			void flush(int type);
			void putchar(char ch);
			void writeString(string str);
			string readLine(int type);
		}
		private static LibImpl libImpl = new ConsoleLibImpl();
		public static void setLibImpl(LibImpl _libImpl)
		{
			libImpl = _libImpl;
		}
		
		public class ConsoleLibImpl : LibImpl
		{
#if UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5

#else
			public static Stream stdout_ = Console.OpenStandardOutput();
			public static Stream stdin_ = Console.OpenStandardInput();
			public static Stream stderr_ = Console.OpenStandardError();
#endif

			public int ReadByte(int type)
			{
				if (type == TYPE_STDIN)
				{
#if UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5

#else
					return stdin_.ReadByte();
#endif
				}
				return 0;
			}
			public int Read(int type, byte[] buffer, int offset, int count)
			{
				if (type == TYPE_STDIN)
				{
#if UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5

#else
					return stdin_.Read(buffer, offset, count);
#endif
				}
				return 0;
			}
			public void Write(int type, byte[] buffer, int offset, int count)
			{
				if (type == TYPE_STDOUT || type == TYPE_STDERR)
				{
#if UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5
					string str = "";
					for (int i = 0; i < count; i++)
					{
						char ch = (char)buffer[offset + i];
						str += ch;
					}
					int n = -1;
					for (int i = str.Length - 1; i >= 0; i--)
					{
						char ch = str[i];
						if (ch != '\r' && ch != '\n')
						{
							n = i;
							break;
						}
					}
					if (n >= 0)
					{
						str = str.Substring(0, n+1);
					}
					else
					{
						str = "";
					}
					if (str.Length > 0)
					{
						if (type == TYPE_STDOUT)
						{
							UnityEngine.Debug.Log(str);
						}
						else if (type == TYPE_STDERR)
						{
							UnityEngine.Debug.LogError(str);
						}
					}
#else
					if (type == TYPE_STDOUT)
					{
						stdout_.Write(buffer, offset, count);
					}
					else if (type == TYPE_STDERR)
					{
						stderr_.Write(buffer, offset, count);
					}					
#endif
				}
			}
			public bool isEof(int type)
			{
				return false;
			}
			public void flush(int type)
			{
#if UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5

#else
				if (type == TYPE_STDIN)
				{
					stdin_.Flush();
				} 
				else if (type == TYPE_STDOUT)
				{
					stdout_.Flush();
				}
				else if (type == TYPE_STDERR)
				{
					stderr_.Flush();
				}
#endif
			}
			public void putchar(char ch)
			{
#if UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5
				UnityEngine.Debug.Log(ch);
#else
				Console.Write(ch);
#endif
			}
			public void writeString(string str)
			{
#if UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5
				UnityEngine.Debug.Log(str);
#else
				Console.Write(str);
#endif
			}
			public string readLine(int type)
			{
				if (type == TYPE_STDIN)
				{
#if UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5

#else
					return Console.ReadLine();
#endif
				}
				return null;
			}
		}
		public static StreamProxy stdout = new StreamProxy(TYPE_STDOUT);
		public static StreamProxy stdin = new StreamProxy(TYPE_STDIN);
		public static StreamProxy stderr = new StreamProxy(TYPE_STDERR);
		
		// misc stuff needed for the compile

		public static bool isalpha(char c) { return Char.IsLetter(c); }
		public static bool iscntrl(char c) { return Char.IsControl(c); }
		public static bool isdigit(char c) { return Char.IsDigit(c); }
		public static bool islower(char c) { return Char.IsLower(c); }
		public static bool ispunct(char c) { return Char.IsPunctuation(c); }
		public static bool isspace(char c) { return (c==' ') || (c>=(char)0x09 && c<=(char)0x0D); }
		public static bool isupper(char c) { return Char.IsUpper(c); }
		public static bool isalnum(char c) { return Char.IsLetterOrDigit(c); }
		public static bool isxdigit(char c) { return "0123456789ABCDEFabcdef".IndexOf(c) >= 0; }

		public static bool isalpha(int c) { return Char.IsLetter((char)c); }
		public static bool iscntrl(int c) { return Char.IsControl((char)c); }
		public static bool isdigit(int c) { return Char.IsDigit((char)c); }
		public static bool islower(int c) { return Char.IsLower((char)c); }
		public static bool ispunct(int c) { return ((char)c != ' ') && !isalnum((char)c); } // *not* the same as Char.IsPunctuation
		public static bool isspace(int c) { return ((char)c == ' ') || ((char)c >= (char)0x09 && (char)c <= (char)0x0D); }
		public static bool isupper(int c) { return Char.IsUpper((char)c); }
		public static bool isalnum(int c) { return Char.IsLetterOrDigit((char)c); }

		public static char tolower(char c) { return Char.ToLower(c); }
		public static char toupper(char c) { return Char.ToUpper(c); }
		public static char tolower(int c) { return Char.ToLower((char)c); }
		public static char toupper(int c) { return Char.ToUpper((char)c); }

		public static ulong strtoul(CharPtr s, out CharPtr end, int base_)
		{
			bool is_base_zero = false;
			if (base_ == 0)
			{
				is_base_zero = true;
				base_ = 10;
			}
			try
			{
				end = new CharPtr(s.chars, s.index);

				// skip over any leading whitespace
				while (end[0] == ' ')
					end = end.next();

				// ignore any leading 0x
				if ((end[0] == '0') && (end[1] == 'x')) {
					end = end.next().next(); 
					if (is_base_zero) base_ = 16;
				} else if ((end[0] == '0') && (end[1] == 'X')) {
					end = end.next().next();
					if (is_base_zero) base_ = 16;
				}

				// do we have a leading + or - sign?
				bool negate = false;
				if (end[0] == '+')
					end = end.next();
				else if (end[0] == '-')
				{
					negate = true;
					end = end.next();
				}

				// loop through all chars
				bool invalid = false;
				bool had_digits = false;
				ulong result = 0;
				while (true)
				{
					// get this char
					char ch = end[0];					

					// which digit is this?
					int this_digit = 0;
					if (isdigit(ch))
						this_digit = ch - '0';
					else if (isalpha(ch))
						this_digit = tolower(ch) - 'a' + 10;
					else
						break;

					// is this digit valid?
					if (this_digit >= base_)
						invalid = true;
					else
					{
						had_digits = true;
						result = result * (ulong)base_ + (ulong)this_digit;
					}

					end = end.next();
				}

				// were any of the digits invalid?
				if (invalid || (!had_digits))
				{
					end = s;
					return System.UInt64.MaxValue;
				}

				// if the value was a negative then negate it here
				if (negate)
					result = (ulong)-(long)result;

				// ok, we're done
				return (ulong)result;
			}
			catch
			{
				end = s;
				return 0;
			}
		}

		public static void putchar(char ch)
		{
			if (libImpl != null)
			{
				libImpl.putchar(ch);
			}
		}

		public static void putchar(int ch)
		{
			if (libImpl != null)
			{
				libImpl.putchar((char)ch);
			}
		}

		public static bool isprint(byte c)
		{
			return (c >= (byte)' ') && (c <= (byte)127);
		}

		public static int parse_scanf(string str, CharPtr fmt, params object[] argp)
		{
			int parm_index = 0;
			int index = 0;
			while (fmt[index] != 0)
			{
				if (fmt[index++]=='%')
					switch (fmt[index++])
					{
						case 's':
							{
								argp[parm_index++] = str;
								break;
							}
						case 'c':
							{
								argp[parm_index++] = Convert.ToChar(str);
								break;
							}
						case 'd':
							{
								argp[parm_index++] = Convert.ToInt32(str);
								break;
							}
						case 'l':
							{
								argp[parm_index++] = Convert.ToDouble(str);
								break;
							}
						case 'f':
							{
								argp[parm_index++] = Convert.ToDouble(str);
								break;
							}
						//case 'p':
						//    {
						//        result += "(pointer)";
						//        break;
						//    }
					}
			}
			return parm_index;
		}

		public static void printf(CharPtr str, params object[] argv)
		{
			Tools.printf(str.ToString(), argv);
		}

		//FIXME:see lua_number2str, s = String.Format("{0}", n);
		public static int sprintf(CharPtr buffer, CharPtr str, params object[] argv)
		{
			string temp = Tools.sprintf(str.ToString(), argv);
			strcpy(buffer, temp);
			return strlen(buffer); //FIXME:added
		}

		public static int fprintf(StreamProxy stream, CharPtr str, params object[] argv)
		{
			string result = Tools.sprintf(str.ToString(), argv);
			char[] chars = result.ToCharArray();
			byte[] bytes = new byte[chars.Length];
			for (int i=0; i<chars.Length; i++)
				bytes[i] = (byte)chars[i];
			stream.Write(bytes, 0, bytes.Length);
			return 1;
		}

		public const int EXIT_SUCCESS = 0;
		public const int EXIT_FAILURE = 1;

		public static int errno = -1;
		//FIXME:changed, see upper
//		public static int errno()
//		{
//			return -1;	// todo: fix this - mjf
//		}

		public static CharPtr strerror(int error)
		{
			return String.Format("error #{0}", error); // todo: check how this works - mjf
		}

		public static CharPtr getenv(CharPtr envname)
		{
			// todo: fix this - mjf
			//if (envname == "LUA_PATH)
				//return "MyPath";
			return null;
		}

		public class CharPtr
		{
			public char[] chars;
			public int index;
			
			public char this[int offset]
			{
				get { return chars[index + offset]; }
				set { chars[index + offset] = value; }
			}
			public char this[uint offset]
			{
				get { return chars[index + offset]; }
				set { chars[index + offset] = value; }
			}
			public char this[long offset]
			{
				get { return chars[index + (int)offset]; }
				set { chars[index + (int)offset] = value; }
			}

			public static implicit operator CharPtr(string str) { return new CharPtr(str); }
			public static implicit operator CharPtr(char[] chars) { return new CharPtr(chars); }

			public CharPtr()
			{
				this.chars = null;
				this.index = 0;
			}

			public CharPtr(string str)
			{
				this.chars = (str + '\0').ToCharArray();
				this.index = 0;
			}

			public CharPtr(CharPtr ptr)
			{
				this.chars = ptr.chars;
				this.index = ptr.index;
			}

			public CharPtr(CharPtr ptr, int index)
			{
				this.chars = ptr.chars;
				this.index = index;
			}

			public CharPtr(char[] chars)
			{
				this.chars = chars;
				this.index = 0;
			}

			public CharPtr(char[] chars, int index)
			{
				this.chars = chars;
				this.index = index;
			}

			public CharPtr(IntPtr ptr)
			{
				this.chars = new char[0];
				this.index = 0;
			}

			public static CharPtr operator +(CharPtr ptr, int offset) {return new CharPtr(ptr.chars, ptr.index+offset);}
			public static CharPtr operator -(CharPtr ptr, int offset) {return new CharPtr(ptr.chars, ptr.index-offset);}
			public static CharPtr operator +(CharPtr ptr, uint offset) { return new CharPtr(ptr.chars, ptr.index + (int)offset); }
			public static CharPtr operator -(CharPtr ptr, uint offset) { return new CharPtr(ptr.chars, ptr.index - (int)offset); }

			public void inc() { this.index++; }
			public void dec() { this.index--; }
			public CharPtr next() { return new CharPtr(this.chars, this.index + 1); }
			public CharPtr prev() { return new CharPtr(this.chars, this.index - 1); }
			public CharPtr add(int ofs) { return new CharPtr(this.chars, this.index + ofs); }
			public CharPtr sub(int ofs) { return new CharPtr(this.chars, this.index - ofs); }
			
			public static bool operator ==(CharPtr ptr, char ch) { return ptr[0] == ch; }
			public static bool operator ==(char ch, CharPtr ptr) { return ptr[0] == ch; }
			public static bool operator !=(CharPtr ptr, char ch) { return ptr[0] != ch; }
			public static bool operator !=(char ch, CharPtr ptr) { return ptr[0] != ch; }

			public static CharPtr operator +(CharPtr ptr1, CharPtr ptr2)
			{
				string result = "";
				for (int i = 0; ptr1[i] != '\0'; i++)
					result += ptr1[i];
				for (int i = 0; ptr2[i] != '\0'; i++)
					result += ptr2[i];
				return new CharPtr(result);
			}
			public static int operator -(CharPtr ptr1, CharPtr ptr2) {
				Debug.Assert(ptr1.chars == ptr2.chars); return ptr1.index - ptr2.index; }
			public static bool operator <(CharPtr ptr1, CharPtr ptr2) {
				Debug.Assert(ptr1.chars == ptr2.chars); return ptr1.index < ptr2.index; }
			public static bool operator <=(CharPtr ptr1, CharPtr ptr2) {
				Debug.Assert(ptr1.chars == ptr2.chars); return ptr1.index <= ptr2.index; }
			public static bool operator >(CharPtr ptr1, CharPtr ptr2) {
				Debug.Assert(ptr1.chars == ptr2.chars); return ptr1.index > ptr2.index; }
			public static bool operator >=(CharPtr ptr1, CharPtr ptr2) {
				Debug.Assert(ptr1.chars == ptr2.chars); return ptr1.index >= ptr2.index; }
			public static bool operator ==(CharPtr ptr1, CharPtr ptr2) {
				object o1 = ptr1 as CharPtr;
				object o2 = ptr2 as CharPtr;
				if ((o1 == null) && (o2 == null)) return true;
				if (o1 == null) return false;
				if (o2 == null) return false;
				return (ptr1.chars == ptr2.chars) && (ptr1.index == ptr2.index); }
			public static bool operator !=(CharPtr ptr1, CharPtr ptr2) {return !(ptr1 == ptr2); }

			public override bool Equals(object o)
			{
				return this == (o as CharPtr);
			}

			public override int GetHashCode()
			{
				return 0;
			}
			public override string ToString()
			{
				string result = "";
				for (int i = index; (i<chars.Length) && (chars[i] != '\0'); i++)
					result += chars[i];
				return result;
			}
		}

		public static int memcmp(CharPtr ptr1, CharPtr ptr2, uint size) { return memcmp(ptr1, ptr2, (int)size); }
		public static int memcmp(CharPtr ptr1, CharPtr ptr2, int size)
		{
			for (int i=0; i<size; i++)
				if (ptr1[i]!=ptr2[i])
				{
					if (ptr1[i]<ptr2[i])
						return -1;
					else
						return 1;
				}
			return 0;
		}

		public static CharPtr memchr(CharPtr ptr, char c, uint count)
		{
			for (uint i = 0; i < count; i++)
				if (ptr[i] == c)
					return new CharPtr(ptr.chars, (int)(ptr.index + i));
			return null;
		}

		public static CharPtr strpbrk(CharPtr str, CharPtr charset)
		{
			for (int i=0; str[i] != '\0'; i++)
				for (int j = 0; charset[j] != '\0'; j++)
					if (str[i] == charset[j])
						return new CharPtr(str.chars, str.index + i);
			return null;
		}

		// find c in str
		public static CharPtr strchr(CharPtr str, char c)
		{
			for (int index = str.index; str.chars[index] != 0; index++)
				if (str.chars[index] == c)
					return new CharPtr(str.chars, index);
			return null;
		}

		public static CharPtr strcpy(CharPtr dst, CharPtr src)
		{
			int i;
			for (i = 0; src[i] != '\0'; i++)
				dst[i] = src[i];
			dst[i] = '\0';
			return dst;
		}

		public static CharPtr strcat(CharPtr dst, CharPtr src)
		{
			int dst_index = 0;
			while (dst[dst_index] != '\0')
				dst_index++;
			int src_index = 0;
			while (src[src_index] != '\0')
				dst[dst_index++] = src[src_index++];
			dst[dst_index++] = '\0';
			return dst;
		}

		public static CharPtr strncat(CharPtr dst, CharPtr src, int count)
		{
			int dst_index = 0;
			while (dst[dst_index] != '\0')
				dst_index++;
			int src_index = 0;
			while ((src[src_index] != '\0') && (count-- > 0))
				dst[dst_index++] = src[src_index++];
			return dst;
		}

		public static uint strcspn(CharPtr str, CharPtr charset)
		{
			int index = str.ToString().IndexOfAny(charset.ToString().ToCharArray());
			if (index < 0)
				index = str.ToString().Length;
			return (uint)index;
		}

		public static CharPtr strncpy(CharPtr dst, CharPtr src, int length)
		{
			int index = 0;
			while ((src[index] != '\0') && (index<length))
			{
				dst[index] = src[index];
				index++;
			}
			while (index < length)
				dst[index++] = '\0';
			return dst;
		}

		public static int strlen(CharPtr str)
		{
			int index = 0;
			while (str[index] != '\0')
				index++;
			return index;
		}

		public static lua_Number fmod(lua_Number a, lua_Number b)
		{
			float quotient = (int)Math.Floor(a / b);
			return a - quotient * b;
		}

		public static lua_Number modf(lua_Number a, out lua_Number b)
		{
			b = Math.Floor(a);
			return a - Math.Floor(a);
		}

		public static long lmod(lua_Number a, lua_Number b)
		{
			return (long)a % (long)b;
		}

		public static int getc(StreamProxy f)
		{
			return f.ReadByte();
		}

		public static void ungetc(int c, StreamProxy f)
		{
			f.ungetc();
		}

		public static int EOF = 0xffff; //-1; //FIXME:changed 

		public static void fputs(CharPtr str, StreamProxy stream)
		{
			stream.puts(str.ToString());
		}

		public static int feof(StreamProxy s)
		{
			return (s.isEof()? 1 : 0);
		}

		public static int fread(CharPtr ptr, int size, int num, StreamProxy stream)
		{
			int num_bytes = num * size;
			byte[] bytes = new byte[num_bytes];
			try
			{
				int result = stream.Read(bytes, 0, num_bytes);
				for (int i = 0; i < result; i++)
					ptr[i] = (char)bytes[i];
				return result/size;
			}
			catch
			{
				return 0;
			}
		}

		public static int fwrite(CharPtr ptr, int size, int num, StreamProxy stream)
		{
			int num_bytes = num * size;
			byte[] bytes = new byte[num_bytes];
			for (int i = 0; i < num_bytes; i++)
				bytes[i] = (byte)ptr[i];
			try
			{
				stream.Write(bytes, 0, num_bytes);
			}
			catch
			{
				return 0;
			}
			return num;
		}
		
		//FIXME:???
		public static int fputc(byte c, StreamProxy stream)
		{
			int num_bytes = 1;
			byte[] bytes = new byte[num_bytes];
			bytes[0] = c;
			try
			{
				stream.Write(bytes, 0, num_bytes);
			}
			catch
			{
				return 0;
			}
			return num_bytes;
		}
		
		//FIXME:???
		public static void perror(CharPtr s)
		{
			printf(s);
		}

		public static int strcmp(CharPtr s1, CharPtr s2)
		{
			if (s1 == s2)
				return 0;
			if (s1 == null)
				return -1;
			if (s2 == null)
				return 1;

			for (int i = 0; ; i++)
			{
				if (s1[i] != s2[i])
				{
					if (s1[i] < s2[i])
						return -1;
					else
						return 1;
				}
				if (s1[i] == '\0')
					return 0;
			}
		}

		//from https://github.com/UKMonkey/Psy/blob/master/Psy.Core/Platform.cs
	    public enum PlatformType
	    {
	        Windows,
	        Linux,
	        MacOs
	    }

        public static PlatformType GetExecutingPlatform()
        {
            switch ((int)Environment.OSVersion.Platform)
            {
                case 4:
                    return PlatformType.Linux;
                case 6:
                    return PlatformType.MacOs;
                case 128:
                    return PlatformType.Linux;
                default:
                    return PlatformType.Windows;
            }
        }
		
		public static CharPtr fgets(CharPtr str, StreamProxy stream)
		{
			int index = 0;
			try
			{
				while (true)
				{
					str[index] = (char)stream.ReadByte();
					
					if (str[index] == '\r' || str[index] == '\n')
					{
						PlatformType type = GetExecutingPlatform();
						if (type == PlatformType.Linux)
						{
							if (str[index] == '\r') 
							{
								index--; //ignore
							} 
							else if (str[index] == '\n')
							{
								if (index >= str.chars.Length)
									break;
								index++;									
								str[index] = '\0';
								break;
							}						
						}
						else if (type == PlatformType.MacOs)  //not tested
						{
							if (str[index] == '\n') 
							{
								index--; //ignore
							} 
							else if (str[index] == '\r')
							{
								str[index] = '\n';
								if (index >= str.chars.Length)
									break;
								index++;									
								str[index] = '\0';
								break;
							}						
						}
						else
						{
							if (str[index] == '\r') 
							{
								index--; //ignore
							} 
							else if (str[index] == '\n')
							{
								if (index >= str.chars.Length)
									break;
								index++;									
								str[index] = '\0';
								break;
							}
						}
					}
					else if (str[index] == '\xffff') //Ctrl+Z
					{
						return null;
					}
					if (index >= str.chars.Length)
						break;
					index++;
				}
			}
			catch
			{
			}
			return str;
		}

		public static double frexp(double x, out int expptr)
		{
			expptr = (int)Math.Log(x, 2) + 1;
			double s = x / Math.Pow(2, expptr);
			return s;
		}

		public static double ldexp(double x, int expptr)
		{
			return x * Math.Pow(2, expptr);
		}

		public static CharPtr strstr(CharPtr str, CharPtr substr)
		{
			int index = str.ToString().IndexOf(substr.ToString());
			if (index < 0)
				return null;
			return new CharPtr(str + index);
		}

		public static CharPtr strrchr(CharPtr str, char ch)
		{
			int index = str.ToString().LastIndexOf(ch);
			if (index < 0)
				return null;
			return str + index;
		}

		public static StreamProxy fopen(CharPtr filename, CharPtr mode)
		{
			string str = filename.ToString();			
			FileMode filemode = FileMode.Open;
			FileAccess fileaccess = (FileAccess)0;			
			for (int i=0; mode[i] != '\0'; i++)
				switch (mode[i])
				{
					case 'r': 
						fileaccess = fileaccess | FileAccess.Read;
						if (!File.Exists(str))
							return null;
						break;

					case 'w':
						filemode = FileMode.Create;
						fileaccess = fileaccess | FileAccess.Write;
						break;
				}
			try
			{
				return new StreamProxy(new FileStream(str, filemode, fileaccess));
			}
			catch
			{
				return null;
			}
		}

		public static StreamProxy freopen(CharPtr filename, CharPtr mode, StreamProxy stream)
		{
			stream.close();
			return fopen(filename, mode);
		}
		
		//see below
//		public static void fflush(Stream stream)
//		{
//			stream.Flush();
//		}

		public static int ferror(StreamProxy stream)
		{
			return 0;	// todo: fix this - mjf
		}

		public static int fclose(StreamProxy stream)
		{
			stream.close();
			return 0;
		}

		public static StreamProxy tmpfile()
		{
			return new StreamProxy(new FileStream(Path.GetTempFileName(), FileMode.Create, FileAccess.ReadWrite));
		}

		public static int fscanf(StreamProxy f, CharPtr format, params object[] argp)
		{
			string str = f.readline();
			return parse_scanf(str, format, argp);
		}
		
		public static int fseek(StreamProxy f, long offset, int origin)
		{
			return f.seek(offset, origin);
		}


		public static int ftell(StreamProxy f)
		{
			return (int)f.getPosition();
		}

		public static int clearerr(StreamProxy f)
		{
			//Debug.Assert(false, "clearerr not implemented yet - mjf");
			return 0;
		}

		public static int setvbuf(StreamProxy stream, CharPtr buffer, int mode, uint size)
		{
			Debug.Assert(false, "setvbuf not implemented yet - mjf");
			return 0;
		}

		public static void memcpy<T>(T[] dst, T[] src, int length)
		{
			for (int i = 0; i < length; i++)
				dst[i] = src[i];
		}

		public static void memcpy<T>(T[] dst, int offset, T[] src, int length)
		{
			for (int i=0; i<length; i++)
				dst[offset+i] = src[i];
		}

		public static void memcpy<T>(T[] dst, T[] src, int srcofs, int length)
		{
			for (int i = 0; i < length; i++)
				dst[i] = src[srcofs+i];
		}

		public static void memcpy(CharPtr ptr1, CharPtr ptr2, uint size) { memcpy(ptr1, ptr2, (int)size); }
		public static void memcpy(CharPtr ptr1, CharPtr ptr2, int size)
		{
			for (int i = 0; i < size; i++)
				ptr1[i] = ptr2[i];
		}

		public static object VOID(object f) { return f; }

		public const double HUGE_VAL = System.Double.MaxValue;
		public const uint SHRT_MAX = System.UInt16.MaxValue;

		public const int _IONBF = 0;
		public const int _IOFBF = 1;
		public const int _IOLBF = 2;

		public const int SEEK_SET = 0;
		public const int SEEK_CUR = 1;
		public const int SEEK_END = 2;

		// one of the primary objectives of this port is to match the C version of Lua as closely as
		// possible. a key part of this is also matching the behaviour of the garbage collector, as
		// that affects the operation of things such as weak tables. in order for this to occur the
		// size of structures that are allocated must be reported as identical to their C++ equivelents.
		// that this means that variables such as global_State.totalbytes no longer indicate the true
		// amount of memory allocated.
		public static int GetUnmanagedSize(Type t)
		{
			if (t == typeof(global_State))
				return 228;
			else if (t == typeof(LG))
				return 376;
			else if (t == typeof(CallInfo))
				return 24;
			else if (t == typeof(lua_TValue))
				return 16;
			else if (t == typeof(Table))
				return 32;
			else if (t == typeof(Node))
				return 32;
			else if (t == typeof(GCObject))
				return 120;
			else if (t == typeof(GCObjectRef))
				return 4;
			else if (t == typeof(ArrayRef))
				return 4;
			else if (t == typeof(Closure))
				return 0;	// handle this one manually in the code
			else if (t == typeof(Proto))
				return 76;
			else if (t == typeof(luaL_reg))
				return 8;
			else if (t == typeof(luaL_Buffer))
				return 524;
			else if (t == typeof(lua_State))
				return 120;
			else if (t == typeof(lua_Debug))
				return 100;
			else if (t == typeof(CallS))
				return 8;
			else if (t == typeof(LoadF))
				return 520;
			else if (t == typeof(LoadS))
				return 8;
			else if (t == typeof(lua_longjmp))
				return 72;
			else if (t == typeof(SParser))
				return 20;
			else if (t == typeof(Token))
				return 16;
			else if (t == typeof(LexState))
				return 52;
			else if (t == typeof(FuncState))
				return 572;
			else if (t == typeof(GCheader))
				return 8;
			else if (t == typeof(lua_TValue))
				return 16;
			else if (t == typeof(TString))
				return 16;
			else if (t == typeof(LocVar))
				return 12;
			else if (t == typeof(UpVal))
				return 32;
			else if (t == typeof(CClosure))
				return 40;
			else if (t == typeof(LClosure))
				return 24;
			else if (t == typeof(TKey))
				return 16;
			else if (t == typeof(ConsControl))
				return 40;
			else if (t == typeof(LHS_assign))
				return 32;
			else if (t == typeof(expdesc))
				return 24;
			else if (t == typeof(upvaldesc))
				return 2;
			else if (t == typeof(BlockCnt))
				return 12;
			else if (t == typeof(Zio))
				return 20;
			else if (t == typeof(Mbuffer))
				return 12;
			else if (t == typeof(LoadState))
				return 16;
			else if (t == typeof(MatchState))
				return 272;
			else if (t == typeof(stringtable))
				return 12;
			else if (t == typeof(FilePtr))
				return 4;
			else if (t == typeof(Udata))
				return 24;
			else if (t == typeof(Char))
				return 1;
			else if (t == typeof(UInt16))
				return 2;
			else if (t == typeof(Int16))
				return 2;
			else if (t == typeof(UInt32))
				return 4;
			else if (t == typeof(Int32))
				return 4;
			else if (t == typeof(Single))
				return 4;			
			else if (t == typeof(lua_Number))
				return 8;
			Debug.Assert(false, "Trying to get unknown sized of unmanaged type " + t.ToString());
			return 0;
		}
		
		public static void exit(int exitCode)
		{
			Environment.Exit(exitCode);
		}
		
		public static void abort()
		{
			Environment.Exit(-1); //FIXME:???
		}
		
		public static double floor(double a)
		{
			return Math.Floor(a);
		}
		
		public static double log(double d)
		{
			return Math.Log(d);
		}

		public static double log10(double d)
		{
			return Math.Log10(d);
		}	

		public static StreamProxy _popen(CharPtr command, CharPtr type)
		{
			//FIXME:not implemented
			return null;
		}
		
		public static int _pclose(StreamProxy stream)
		{
			//FIXME:not implemented
			return 0;
		}
		
		public const byte UCHAR_MAX = System.Byte.MaxValue;
		
		//from https://github.com/xanathar/moonsharp/blob/master/src/MoonSharp.Interpreter/Interop/LuaStateInterop/LuaBase_CLib.cs
		public static bool isgraph(char c) { return !Char.IsControl(c) && !Char.IsWhiteSpace(c); }
		public static bool isgraph(int c) { return !Char.IsControl((char)c) && !Char.IsWhiteSpace((char)c); }
		
		public static int system(CharPtr cmd) 
		{
			CharPtr strCmdLine = "/C regenresx " + cmd;
			Process proc = new Process();
			proc.EnableRaisingEvents=false;
			proc.StartInfo.FileName = "CMD.exe";
			proc.StartInfo.Arguments = strCmdLine.ToString();
			proc.Start();
			proc.WaitForExit();
			return proc.ExitCode;
		}
		
		public static int remove(CharPtr filename)
		{
		  	int result = 0;
		  	try 
		  	{
		  		File.Delete(filename.ToString());
		  	} 
		  	catch 
		  	{
		  		result = -1;
		  	}
		  	return result;
		}
		
		public static int rename(CharPtr fromname, CharPtr toname)
		{
			int result = 0;
			try
			{
				File.Move(fromname.ToString(), toname.ToString());
			}
			catch
			{
				result = -1; // todo: this should be a proper error code
			}
		  	return result;
		}
		
		public const int L_tmpnam = 16;
		public static CharPtr tmpnam(CharPtr name) 
		{
			return strcpy(name, Path.GetTempFileName());
		}
		
		private const string number_chars = "0123456789+-eE.";
		public static double strtod(CharPtr s, out CharPtr end)
		{
			end = new CharPtr(s.chars, s.index);
			string str = "";
			while (end[0] == ' ')
				end = end.next();
			while (number_chars.IndexOf(end[0]) >= 0)
			{
				str += end[0];
				end = end.next();
			}

			try
			{
				return Convert.ToDouble(str.ToString());
			}
			catch (OverflowException)
			{
				// this is a hack, fix it - mjf
				if (str[0] == '-')
					return System.Double.NegativeInfinity;
				else
					return System.Double.PositiveInfinity;
			}
			catch
			{
				end = new CharPtr(s.chars, s.index);
				return 0;
			}
		}
		
		public static uint strspn (CharPtr s, CharPtr accept)
		{
			int p;//s
		    int a;//accept
		    uint count = 0;
		    for (p = 0; s[p] != '\0'; ++p)
		    {
		    	for (a = 0; accept[a] != '\0'; ++a)
		        {
		    		if (s[p] == accept[a])
		            {
		                ++count;
		                break;
		            }
		        }
		    	if (accept[a] == '\0')
		        {
		            return count;
		        }
		    }
		    return count;
		}
		
		public const clock_t CLOCKS_PER_SEC = (clock_t)1000;
		public static clock_t clock()
		{
			long ticks = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
			return ticks;
		}
		
		public static int fflush(StreamProxy stream)
		{
			return stream.flush();
		}
		
		public class lconv 
		{
			public CharPtr decimal_point;
			
			public lconv()
			{
				decimal_point = ".";
			}
		}
		public static lconv _lconv = new lconv();
		public static lconv localeconv()
		{
			return _lconv;
		}
		
		public static void WriteLog(string strLog)
		{
		  	string sFileName = "log_" + DateTime.Now.ToString("yyyy-MM-dd") + ".log";
		  	FileMode mode = File.Exists(sFileName) ? FileMode.Append : FileMode.Create;
		  	using (FileStream fs = new FileStream(sFileName, mode, FileAccess.Write))
		  	{
		  		using (StreamWriter sw = new StreamWriter(fs))
		  		{
			  		sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + "   ---   " + strLog);
		  		}
		  	}
		}
		
		public static int abs(int x)
		{
			return Math.Abs(x);
		}
		
		public static double abs(double x)
		{
			return Math.Abs(x);
		}
		
		public static double sin(double x)
		{
			return Math.Sin(x);
		}
		
		public static double sinh(double x)
		{
			return Math.Sinh(x);
		}
		
		public static double cos(double x)
		{
			return Math.Cos(x);
		}
		
		public static double cosh(double x)
		{
			return Math.Cosh(x);
		}
		
		public static double tan(double x)
		{
			return Math.Tan(x);
		}
		
		public static double tanh(double x)
		{
			return Math.Tanh(x);
		}
		
		public static double asin(double x)
		{
			return Math.Asin(x);
		}
		
		public static double acos(double x)
		{
			return Math.Acos(x);
		}
		
		public static double atan(double x)
		{
			return Math.Atan(x);
		}
		
		public static double atan2(double x, double y)
		{
			return Math.Atan2(x, y);
		}
		
		public static double ceil(double x)
		{
			return Math.Ceiling(x);
		}
		
		public static double sqrt(double x)
		{
			return Math.Sqrt(x);
		}
		
		public static double pow(double x, double y)
		{
			return Math.Pow(x, y);
		}
		
		public static double exp(double x)
		{
			return Math.Exp(x);
		}
		
		private const int RAND_MAX = int.MaxValue;
		private static Random rng = new Random();

		public static int rand()
		{
			return rng.Next(RAND_MAX);
		}
		
		public static void srand(uint seed)
		{
			rng = new Random((int)seed);
		}
		
		public static void debug_assert(bool condition)
		{
			Debug.Assert(condition);
		}
		
		public static void debug_assert(bool condition, string message)
		{
			Debug.Assert(condition, message);
		}
		
		public static void debug_writeLine(string line)
		{
			Debug.WriteLine(line);
		}
		
		public static string[] get_args(string[] args)
		{
			List<string> newargs = new List<string>(args);
			newargs.Insert(0, Assembly.GetExecutingAssembly().Location);
			return newargs.ToArray();
		}
		
		public static string[] string_array_plus(string[] args, int n)
		{
			List<string> newargs = new List<string>(args);
			newargs.RemoveRange(0, n);
			return newargs.ToArray();
		}
		
		public static CharPtr object_to_charptr(object b)
		{
			int size = Marshal.SizeOf(b);
			IntPtr ptr = Marshal.AllocHGlobal(size);
			Marshal.StructureToPtr(b, ptr, false);
			byte[] bytes = new byte[size];
			Marshal.Copy(ptr, bytes, 0, size);	
			char[] ch = new char[bytes.Length];
			for (int i = 0; i < bytes.Length; i++)
				ch[i] = (char)bytes[i];
			CharPtr str = ch;			
			return str;
		}
		
		public static object object_to_charptr2(object b)
		{
			if (b.GetType() != typeof(CharPtr))
			{
				using (MemoryStream stream = new MemoryStream())
				{
					BinaryFormatter formatter = new BinaryFormatter();
					formatter.Serialize(stream, b);
					stream.Flush();
					byte[] bytes = stream.GetBuffer();
					char[] chars = new char[bytes.Length];
					for (int i = 0; i < bytes.Length; i++)
						chars[i] = (char)bytes[i];
					b = new CharPtr(chars);
				}
			}
			return b;	
		}
		
		public static int get_object_size(object x)
		{
			return Marshal.SizeOf(x);
		}
		
		public static int get_type_size(Type x)
		{
			return Marshal.SizeOf(x);
		}
		
		public static object array_to_array(object[] objs, Type t)
		{
			int n = objs.Length;
			ArrayList array = new ArrayList();
			for (int i=0; i<n; i++)
				array.Add(objs[i]);
			return array.ToArray(t);
		}
		
		public static object bytes_to_object(byte[] bytes, Type t)
		{
			GCHandle pinnedPacket = GCHandle.Alloc(bytes, GCHandleType.Pinned);
			object b = Marshal.PtrToStructure(pinnedPacket.AddrOfPinnedObject(), t);
			pinnedPacket.Free();
			return b;
		}
	}
}
