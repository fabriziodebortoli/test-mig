using System;

namespace Microarea.Library.LogManager
{
	/// <summary>
	/// IMessageFormatter.
	/// </summary>
	//=========================================================================
	public interface IMessageFormatter
	{
		//---------------------------------------------------------------------
		IMessageFormatter OpenRoot(string rootName);

		//---------------------------------------------------------------------
		IMessageFormatter CloseRoot();

		//---------------------------------------------------------------------
		IMessageFormatter AddElementWithValue(string elementName, object value);

		//---------------------------------------------------------------------
		IMessageFormatter AddElementWithValue(string elementName, ValueType valueType);

		//---------------------------------------------------------------------
		IMessageFormatter AddElementWithValue(
			string elementName,
			DateTime dateTime,
			string dateTimeFormat
			);

		//---------------------------------------------------------------------
		IMessageFormatter OpenElement(string elementName);

		//---------------------------------------------------------------------
		IMessageFormatter CloseElement();

		//---------------------------------------------------------------------
		void Clear();
	}
}
