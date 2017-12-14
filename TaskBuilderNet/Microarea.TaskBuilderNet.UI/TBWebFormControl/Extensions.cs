using System.Reflection;
using System.Text.RegularExpressions;
using System.Web.UI;

namespace Microarea.TaskBuilderNet.UI.TBWebFormControl
{
	/// <summary>
	/// Summary description for Extensions
	/// </summary>
	static class Extensions
	{
    	//--------------------------------------------------------------------------------------
		public static void AddTbStyleAttribute(this HtmlTextWriter writer, string key, string value)
		{
			writer.IncrementHtmlTextWriterStyleCountField();
			writer.AddStyleAttribute(key, value);
		}

		//--------------------------------------------------------------------------------------
		public static void AddTbStyleAttribute(this HtmlTextWriter writer, HtmlTextWriterStyle key, string value)
		{
			writer.IncrementHtmlTextWriterStyleCountField();
			writer.AddStyleAttribute(key, value);
		}

		/// Increments the HtmlTextWriter's _styleCount field. Used to bypass a bug in the framework 
        /// where IndexOutOfRangeException is thrown on the 21st, 41st, 81st, etc call to AddStyleAttribute.
		//--------------------------------------------------------------------------------------
		internal static void IncrementHtmlTextWriterStyleCountField(this HtmlTextWriter writer)
		{
			// as an optimization, get the FieldInfo lazily and cache it for subsequent hits, since the 
            // lookup is the major piece of work. a double check locking technique is used to be thread 
            // safe but only use locking if the FieldInfo hasn't been cached.
			if (m_htmlTextWriter_styleCountInfo == null)
			{
				lock (m_htmlTextWriter_styleCountLockObj)
				{
					// check again for null in case two threads entered the above if block and the first 
                    // one exited the critical section
					if (m_htmlTextWriter_styleCountInfo == null)
						m_htmlTextWriter_styleCountInfo = typeof(HtmlTextWriter).GetField("_styleCount", BindingFlags.Instance | BindingFlags.NonPublic);
				}
			}
			int currValue = (int)m_htmlTextWriter_styleCountInfo.GetValue(writer);
			if (currValue > 0 && (currValue % 20) == 0)
				m_htmlTextWriter_styleCountInfo.SetValue(writer, currValue + 1);

		}

		static FieldInfo m_htmlTextWriter_styleCountInfo;
		static object m_htmlTextWriter_styleCountLockObj = new object();

		//------------------------------------------------------------------------------
		public static string TrimAmpersand(this string text)
		{
			if (string.IsNullOrEmpty(text))
				return "";
			string pattern = @"(?:&(?!&))";
			return Regex.Replace(text, pattern, "");
		}
		//------------------------------------------------------------------------------
		public static string TrimTab(this string text)
		{
			if (string.IsNullOrEmpty(text))
				return "";
			return text.Replace("\t", " ");
		}
    }
}