namespace lua40mod
{
	using TValue = Lua.Value;
	
	public partial class Lua
	{
//		public static Node gnode(Table t, int i)	{return t.node[i];}
//		public static TKey_nk gkey(Node n)			{ return n.i_key.nk; }
		public static TValue gval(Node n)			{
//			return n.i_val;
			return null;
		}
		public static Node gnext(Node n)			{
//			return n.i_key.nk.next;
			return null;
		}
		
		public static void gnext_set(Node n, Node v) { 
//			n.i_key.nk.next = v; 
		}

		public static TValue key2tval(Node n) { 
//			return n.i_key.tvk; 
			return null;
		}
	}
}
