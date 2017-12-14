using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microarea.EasyBuilder.CodeCompletion
{
	/// <summary>
	/// This interface allows to provide more information for scripts such as using statements, etc.
	/// </summary>
	//=============================================================================================
	public interface ICSharpScriptProvider
    {
		/// <summary>
		/// Internal Use
		/// </summary>
		string GetUsing();

		/// <summary>
		/// Internal Use
		/// </summary>
		string GetVars();
    }
}
