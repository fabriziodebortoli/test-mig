using System;
using System.Globalization;
using System.Text;

namespace Microarea.Library.LogManager.Loggers
{
	//=========================================================================
	internal class TxtMessageFormatter : IMessageFormatter
	{
		private StringBuilder messageBuilder = new StringBuilder();
		private int indent;

		//---------------------------------------------------------------------
		protected virtual string GetPadding()
		{
			StringBuilder indentPaddingBuilder = new StringBuilder();
			for (int i = 0; i < indent; i++)
				indentPaddingBuilder.Append(" ");

			return indentPaddingBuilder.ToString();
		}

		#region IMessageFormatter Members

		//---------------------------------------------------------------------
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters")]
		public virtual IMessageFormatter OpenRoot(string rootName)
		{
			if (rootName == null)
				throw new ArgumentNullException("rootName", "Cannot open a null 'rootName'");

			indent = 0;
			messageBuilder.Append(rootName);
			return this;
		}

		//---------------------------------------------------------------------
		public virtual IMessageFormatter CloseRoot()
		{
			indent = 0;
			messageBuilder.AppendLine();
			return this;
		}

		//---------------------------------------------------------------------
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters")]
		public virtual IMessageFormatter AddElementWithValue(string elementName, object value)
		{
			if (elementName == null)
				throw new ArgumentNullException("elementName", "Cannot add a null 'elementName'");

			if (value == null)
				throw new ArgumentNullException("value", "Cannot add a null 'value'");

			string elementPlaceHolder = elementName.Length == 0 ?
					string.Empty
					:
					String.Concat(elementName, ": ");

			messageBuilder.AppendFormat(
				CultureInfo.InvariantCulture,
				"{0}{1}{2}{3}",
				Environment.NewLine,
				GetPadding(),
				elementPlaceHolder,
				value.ToString()
				);

			return this;
		}

		//---------------------------------------------------------------------
		public virtual IMessageFormatter AddElementWithValue(string elementName, ValueType valueType)
		{
			return AddElementWithValue(elementName, valueType.ToString());
		}

		//---------------------------------------------------------------------
		public virtual IMessageFormatter AddElementWithValue(
			string elementName,
			DateTime dateTime,
			string dateTimeFormat
			)
		{
			string temp = String.Concat(
				"[",
				dateTime.ToString(dateTimeFormat, CultureInfo.InvariantCulture),
				"]"
				);

			return AddElementWithValue(elementName, temp);
		}

		//---------------------------------------------------------------------
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters")]
		public virtual IMessageFormatter OpenElement(string elementName)
		{
			if (elementName == null)
				throw new ArgumentNullException("elementName", "Cannot add a null 'elementName'");

			string elementPlaceHolder = elementName.Length == 0 ?
					string.Empty
					:
					String.Concat(elementName, ": ");

			messageBuilder.AppendFormat(
				CultureInfo.InvariantCulture,
				"{0}{1}{2}",
				Environment.NewLine,
				GetPadding(),
				elementPlaceHolder
				);

			indent += 4;
			return this;
		}

		//---------------------------------------------------------------------
		public virtual IMessageFormatter CloseElement()
		{
			messageBuilder.AppendLine();

			indent -= 4;
			if (indent < 0)
				indent = 0;

			return this;
		}

		//---------------------------------------------------------------------
		public virtual void Clear()
		{
			indent = 0;
			messageBuilder.Remove(0, messageBuilder.Length);
		}

		#endregion

		//---------------------------------------------------------------------
		public override string ToString()
		{
			return messageBuilder.ToString();
		}
	}
}
