using Microarea.Framework.TBApplicationWrapper;

namespace Microarea.EasyBuilder.MVC
{
	//=========================================================================
	/// <summary>
	/// DocumentController fantoccio da utilizzare in tutti quei casi in cui
	/// il controller vero e proprio non sia disponibile.
	/// </summary>
	public class DummyDocumentController : DocumentController
	{
		//---------------------------------------------------------------------
		/// <summary>
		/// Internal use
		/// </summary>
		public override MDocument Document
		{
			get
			{
				return null;
			}
		}
	}
}
