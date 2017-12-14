using System;

namespace Microarea.Library.LogManager.Loggers
{
	/// <summary>
	/// HtmlMessageFormatter.
	/// </summary>
	//=========================================================================
	public class HtmlMessageFormatter : IMessageFormatter
	{
		//---------------------------------------------------------------------
		public HtmlMessageFormatter()
		{}

		#region IMessageFormatter Members

		//---------------------------------------------------------------------
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters")]
		public IMessageFormatter OpenRoot(string rootName)
		{
			throw new NotImplementedException("The method or operation is not implemented.");
		}

		//---------------------------------------------------------------------
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters")]
		public IMessageFormatter CloseRoot()
		{
			throw new NotImplementedException("The method or operation is not implemented.");
		}

		//---------------------------------------------------------------------
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters")]
		public IMessageFormatter AddElementWithValue(string elementName, object value)
		{
			throw new NotImplementedException("The method or operation is not implemented.");
		}

		//---------------------------------------------------------------------
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters")]
		public IMessageFormatter AddElementWithValue(string elementName, ValueType valueType)
		{
			throw new NotImplementedException("The method or operation is not implemented.");
		}

		//---------------------------------------------------------------------
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters")]
		public IMessageFormatter AddElementWithValue(string elementName, DateTime dateTime, string dateTimeFormat)
		{
			throw new NotImplementedException("The method or operation is not implemented.");
		}

		//---------------------------------------------------------------------
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters")]
		public IMessageFormatter OpenElement(string elementName)
		{
			throw new NotImplementedException("The method or operation is not implemented.");
		}

		//---------------------------------------------------------------------
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters")]
		public IMessageFormatter CloseElement()
		{
			throw new NotImplementedException("The method or operation is not implemented.");
		}

		//---------------------------------------------------------------------
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters")]
		public void Clear()
		{
			throw new NotImplementedException("The method or operation is not implemented.");
		}

		#endregion
	}
}
