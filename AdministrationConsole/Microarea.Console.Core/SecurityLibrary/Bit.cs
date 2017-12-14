
namespace Microarea.Console.Core.SecurityLibrary
{
	/// <summary>
	/// Summary description for Bit.
	/// </summary>
	
	//=========================================================================
	public class Bit
	{
		public Bit()
		{
		}
		//---------------------------------------------------------------------
		static public int SetUno(int val, int mask)
		{
			return val | mask;
		}
		//---------------------------------------------------------------------
		static public int SetZero(int val, int mask)
		{
			return val & ~ mask;
		}
		//---------------------------------------------------------------------
		static public int SetUno2(int val, int pos)
		{
			return val | (1 << pos);
		}
		//---------------------------------------------------------------------
		static public int SetZero2(int val, int pos)
		{
			return val & ~(1 << pos);
		}
		//---------------------------------------------------------------------
		static public int GetBitByPos(int val, int pos)
		{
			return val & (1 << pos);
		}
		//---------------------------------------------------------------------
		static public int GetBitByMask(int val, int mask)
		{
			return val & mask;
		}
		//---------------------------------------------------------------------
		static public bool IsBitByPosFlagged(int val, int pos)
		{
			return GetBitByPos(val,pos) != 0 ;
		}
		//---------------------------------------------------------------------
	}
	//=========================================================================
}
