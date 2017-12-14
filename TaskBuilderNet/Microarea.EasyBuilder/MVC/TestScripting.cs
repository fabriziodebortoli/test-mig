

#if DEBUG
namespace Microarea.EasyBuilder.MVC
{
	/// <summary>
	/// 
	/// </summary>
	//================================================================================
	class TestScripting
	{
		private int a;
		private int b;

		//-----------------------------------------------------------------------------
		public TestScripting(int a, int b)
		{
			// TODO: Complete member initialization
			this.a = a;
			this.b = b;
		}

		//-----------------------------------------------------------------------------
		public InnerObject GetInnerObject(int c)
		{
			return new InnerObject(a, b, c);
		}
	}

	/// <summary>
	/// 
	/// </summary>
	//================================================================================
	public class InnerObject
	{
		private int a;
		private int b;
		private int c;

		/// <summary>
		/// 
		/// </summary>
		//-----------------------------------------------------------------------------
		public InnerObject(int a, int b, int c)
		{
			// TODO: Complete member initialization
			this.a = a;
			this.b = b;
			this.c = c;
		}

		/// <summary>
		/// 
		/// </summary>
		//-----------------------------------------------------------------------------
		public override string ToString()
		{
			return (a + b + c).ToString();
		}
	}
}
#endif