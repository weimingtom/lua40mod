/*
** $Id: lmathlib.c,v 1.67.1.1 2007/12/27 13:02:25 roberto Exp $
** Standard mathematical library
** See Copyright Notice in lua.h
*/


namespace lua40mod
{
	using lua_Number = System.Double;

	public partial class Lua
	{
		public const double PI = 3.14159265358979323846;
		public const double RADIANS_PER_DEGREE = PI / 180.0;



		private static int math_abs (lua_State L) {
		  
		  return 1;
		}

		private static int math_sin (lua_State L) {
		  
		  return 1;
		}

		private static int math_sinh (lua_State L) {
		  
		  return 1;
		}

		private static int math_cos (lua_State L) {
		  
		  return 1;
		}

		private static int math_cosh (lua_State L) {
		 
		  return 1;
		}

		private static int math_tan (lua_State L) {
		
		  return 1;
		}

		private static int math_tanh (lua_State L) {
		  
		  return 1;
		}

		private static int math_asin (lua_State L) {
		  
		  return 1;
		}

		private static int math_acos (lua_State L) {
		  
		  return 1;
		}

		private static int math_atan (lua_State L) {
		  
		  return 1;
		}

		private static int math_atan2 (lua_State L) {
		  
		  return 1;
		}

		private static int math_ceil (lua_State L) {
		  
		  return 1;
		}

		private static int math_floor (lua_State L) {
		  
		  return 1;
		}

		private static int math_fmod (lua_State L) {
		  
		  return 1;
		}

		private static int math_modf (lua_State L) {
		  
		  return 2;
		}

		private static int math_sqrt (lua_State L) {
		  
		  return 1;
		}

		private static int math_pow (lua_State L) {
		  
		  return 1;
		}

		private static int math_log (lua_State L) {
		  
		  return 1;
		}

		private static int math_log10 (lua_State L) {
		  
		  return 1;
		}

		private static int math_exp (lua_State L) {
		  
		  return 1;
		}

		private static int math_deg (lua_State L) {
		  
		  return 1;
		}

		private static int math_rad (lua_State L) {
		  
		  return 1;
		}

		private static int math_frexp (lua_State L) {
		  
		  return 2;
		}

		private static int math_ldexp (lua_State L) {
		  return 1;
		}



		private static int math_min (lua_State L) {
		  
		  return 1;
		}


		private static int math_max (lua_State L) {
		  
		  return 1;
		}

		private static int math_random (lua_State L) {
		  return 1;
		}


		private static int math_randomseed (lua_State L) {
		  return 0;
		}


		private readonly static luaL_reg[] mathlib = {
		  new luaL_reg("abs",   math_abs),
		  new luaL_reg("acos",  math_acos),
		  new luaL_reg("asin",  math_asin),
		  new luaL_reg("atan2", math_atan2),
		  new luaL_reg("atan",  math_atan),
		  new luaL_reg("ceil",  math_ceil),
		  new luaL_reg("cosh",   math_cosh),
		  new luaL_reg("cos",   math_cos),
		  new luaL_reg("deg",   math_deg),
		  new luaL_reg("exp",   math_exp),
		  new luaL_reg("floor", math_floor),
		  new luaL_reg("fmod",   math_fmod),
		  new luaL_reg("frexp", math_frexp),
		  new luaL_reg("ldexp", math_ldexp),
		  new luaL_reg("log10", math_log10),
		  new luaL_reg("log",   math_log),
		  new luaL_reg("max",   math_max),
		  new luaL_reg("min",   math_min),
		  new luaL_reg("modf",   math_modf),
		  new luaL_reg("pow",   math_pow),
		  new luaL_reg("rad",   math_rad),
		  new luaL_reg("random",     math_random),
		  new luaL_reg("randomseed", math_randomseed),
		  new luaL_reg("sinh",   math_sinh),
		  new luaL_reg("sin",   math_sin),
		  new luaL_reg("sqrt",  math_sqrt),
		  new luaL_reg("tanh",   math_tanh),
		  new luaL_reg("tan",   math_tan),
		  new luaL_reg(null, null)
		};


		/*
		** Open math library
		*/
		public static int luaopen_math (lua_State L) {
		  return 1;
		}

	}
}
