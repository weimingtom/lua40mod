namespace lua40mod
{
	public partial class Lua
	{
		public static int pcRel(InstructionPtr pc, Proto p)
		{
			debug_assert(pc.codes == p.code);
			return pc.pc - 1;
		}
		public static int getline(Proto f, int pc) { return (f.lineinfo != null) ? f.lineinfo[pc] : 0; }
		public static void resethookcount(lua_State L) { L.hookcount = L.basehookcount; }
	}
}