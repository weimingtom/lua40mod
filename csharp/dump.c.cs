/*
** $Id: dump.c,v 1.30 2000/10/31 16:57:23 lhf Exp $
** save bytecodes to file
** See Copyright Notice in lua.h
*/

using System;

namespace lua40mod
{
	using Number = System.Double; //FIXME:???
	using size_t = System.UInt32; //FIXME:???
	using Instruction = System.UInt32;  //FIXME:???
		
	public partial class Lua
	{
//		#include <stdio.h>
//		#include <stdlib.h>
//		#include <string.h>
//		
//		#include "luac.h"		
		
		private static void DumpVector(object b, int n, int size, StreamProxy D)	{ CharPtr str = Lua.object_to_charptr(b); fwrite(str,size,n,D); }
		private static void DumpBlock(object b, int size, StreamProxy D)	{ CharPtr str = Lua.object_to_charptr(b); fwrite(str,size,1,D); }
		private static void DumpByte(byte c, StreamProxy D) { fputc(c, D); }

		private static void DumpInt(int x, StreamProxy D)
		{
 			DumpBlock(x,Lua.get_object_size(x),D);
		}

		private static void DumpSize(size_t x, StreamProxy D)
		{
 			DumpBlock(x,Lua.get_object_size(x),D);
		}

		private static void DumpNumber(Number x, StreamProxy D)
		{
 			DumpBlock(x,Lua.get_object_size(x),D);
		}

		private static void DumpString(TString s, StreamProxy D)
		{
		 	if (s==null || s.str==null)
		  		DumpSize(0,D);
		 	else
		 	{
		  		size_t size=s.len+1;			/* include trailing '\0' */
		  		DumpSize(size,D);
		  		DumpBlock(s.str,(int)size,D);
		 	}
		}

		private static void DumpCode(Proto tf, StreamProxy D)
		{
			DumpInt(tf.ncode,D);
			DumpVector(tf.code,tf.ncode,Lua.get_object_size(tf.code),D);
		}

		private static void DumpLocals(Proto tf, StreamProxy D)
		{
			int i,n=tf.nlocvars;
			DumpInt(n,D);
			for (i=0; i<n; i++)
			{
				DumpString(tf.locvars[i].varname,D);
				DumpInt(tf.locvars[i].startpc,D);
				DumpInt(tf.locvars[i].endpc,D);
			}
		}

		private static void DumpLines(Proto tf, StreamProxy D)
		{
		 	DumpInt(tf.nlineinfo,D);
		 	DumpVector(tf.lineinfo,tf.nlineinfo,Lua.get_object_size(tf.lineinfo),D);
		}

		//private static void DumpFunction(const Proto* tf, FILE* D);

		private static void DumpConstants(Proto tf, StreamProxy D)
		{
		 	int i,n;
		 	DumpInt(n=tf.nkstr,D);
		 	for (i=0; i<n; i++)
		  		DumpString(tf.kstr[i],D);
		 	DumpInt(tf.nknum,D);
		 	DumpVector(tf.knum,tf.nknum,Lua.get_object_size(tf.knum),D);
		 	DumpInt(n=tf.nkproto,D);
		 	for (i=0; i<n; i++)
		  		DumpFunction(tf.kproto[i],D);
		}

		private static void DumpFunction(Proto tf, StreamProxy D)
		{
			DumpString(tf.source,D);
			DumpInt(tf.lineDefined,D);
			DumpInt(tf.numparams,D);
			DumpByte(tf.is_vararg,D);
			DumpInt(tf.maxstacksize,D);
			DumpLocals(tf,D);
			DumpLines(tf,D);
			DumpConstants(tf,D);
			DumpCode(tf,D);
			if (0!=ferror(D))
			{
				perror("luac: write error");
				exit(1);
			}
		}

		private static void DumpHeader(StreamProxy D)
		{
			DumpByte(ID_CHUNK,D);
			fputs(SIGNATURE,D);
			DumpByte(VERSION,D);
			DumpByte(luaU_endianess(),D);
			DumpByte((byte)Lua.get_type_size(typeof(int)),D);
			DumpByte((byte)Lua.get_type_size(typeof(size_t)),D);
			DumpByte((byte)Lua.get_type_size(typeof(Instruction)),D);
			DumpByte(SIZE_INSTRUCTION,D);
			DumpByte(SIZE_OP,D);
			DumpByte(SIZE_B,D);
			DumpByte((byte)Lua.get_type_size(typeof(Number)),D);
			DumpNumber(TEST_NUMBER,D);
		}

		public static void luaU_dumpchunk(Proto Main, StreamProxy D)
		{
 			DumpHeader(D);
 			DumpFunction(Main,D);
		}
	}
}
