namespace lua40mod
{
	public partial class Lua
	{
		public const int EOZ = 0xffff; //-1;			/* end of stream */ //FIXME:changed here

		public class zio : Zio { };

		public static int char2int(char c) { return (int)c; }

		public static int zgetc(zio z)
		{
			if (z.n > 0)
			{
				z.n--;
				int ch = char2int(z.p[0]);
				z.p.inc();
				return ch;
			}
			else {
				z.n = (uint)(((long)z.n - 1) & 0xFFFFFFFFL);
				return luaZ_fill(z);
			}
		}

		public class Mbuffer {
		  public CharPtr buffer = new CharPtr();
		  public uint n;
		  public uint buffsize;
		};

		public static void luaZ_initbuffer(lua_State L, Mbuffer buff)
		{
			buff.buffer = null;
		}

		public static CharPtr luaZ_buffer(Mbuffer buff)	{return buff.buffer;}
		public static uint luaZ_sizebuffer(Mbuffer buff) { return buff.buffsize; }
		public static uint luaZ_bufflen(Mbuffer buff)	{return buff.n;}
		public static void luaZ_resetbuffer(Mbuffer buff) {buff.n = 0;}


		public static void luaZ_resizebuffer(lua_State L, Mbuffer buff, int size)
		{
//			if (buff.buffer == null)
//				buff.buffer = new CharPtr();
//			luaM_reallocvector(L, ref buff.buffer.chars, (int)buff.buffsize, size);
//			buff.buffsize = (uint)buff.buffer.chars.Length;
		}

		public static void luaZ_freebuffer(lua_State L, Mbuffer buff) {luaZ_resizebuffer(L, buff, 0);}



		/* --------- Private Part ------------------ */

		public class Zio {
			public uint n;			/* bytes still unread */
			public CharPtr p;			/* current position in buffer */
			public lua_Reader reader;
			public object data;			/* additional data */
			public lua_State L;			/* Lua state (for reader) */
		};
	}
}
