using System.Collections;
using System.Windows.Forms;

namespace Microarea.Tools.TBLocalizer.CommonUtilities
{
	/// <summary>
	/// Summary description for LocalizerCursor.
	/// </summary>
	public class LocalizerCursor
	{
		public static Stack cursorStack = new Stack();
		public static Cursor Current
		{
			get
			{
				return Cursor.Current;
			}

			set
			{
				if (value == Cursors.Default)
				{
					object o = cursorStack.Pop();
					if (o == null)
						o = value;
					Cursor.Current = (Cursor) o;
				}
				else
				{
					cursorStack.Push(Cursor.Current);
					Cursor.Current = value;
				}
			}
		}
	}
}
