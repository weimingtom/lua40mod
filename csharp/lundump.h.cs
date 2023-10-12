namespace lua40mod
{
	public partial class Lua
	{
		/* for header of binary files -- this is Lua 5.1 */
		public const int LUAC_VERSION		= 0x51;

		/* for header of binary files -- this is the official format */
		public const int LUAC_FORMAT		= 0;

		/* size of header of binary files */
		public const int LUAC_HEADERSIZE		= 12;
		
		
public const byte VERSION = 0x40;		/* last format change was in 4.0 */
public const byte VERSION0 = 0x40;		/* last major  change was in 4.0 */
public const byte ID_CHUNK = 27;		/* binary files start with ESC... */
public const string SIGNATURE = "Lua";		/* ...followed by this signature */

public const double	TEST_NUMBER	= 3.14159265358979323846E8;
	}
}
