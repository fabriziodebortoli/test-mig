using System;
using System.Text;
using System.Xml;


namespace Microarea.Library.LogManager.Loggers
{
	/// <summary>
	/// XmlMessageFormatter.
	/// </summary>
	//=========================================================================
	internal class XmlMessageFormatter : TxtMessageFormatter, IDisposable
	{
		private StringBuilder xmlStringBuilder;
		private XmlWriter xmlWriter;

		//---------------------------------------------------------------------
		public XmlMessageFormatter()
		{
			xmlStringBuilder = new StringBuilder();

			InitWriter();
		}

		//---------------------------------------------------------------------
		private void InitWriter()
		{
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.ConformanceLevel = ConformanceLevel.Auto;

			xmlWriter = XmlWriter.Create(xmlStringBuilder, settings);
		}

		#region IMessageFormatter Members

		//---------------------------------------------------------------------
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters")]
		public override IMessageFormatter OpenRoot(string rootName)
		{
			if (rootName == null)
				throw new ArgumentNullException("rootName", "'rootName' cannot be null");

			if (rootName.Length == 0)
				throw new ArgumentException("'rootName' cannot be empty", "rootName");

			xmlWriter.WriteStartElement(rootName);

			return this;
		}

		//---------------------------------------------------------------------
		public override IMessageFormatter CloseRoot()
		{
			xmlWriter.WriteEndElement();

			return this;
		}

		//---------------------------------------------------------------------
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters")]
		public override IMessageFormatter AddElementWithValue(string elementName, object value)
		{
			if (elementName == null)
				throw new ArgumentNullException("elementName", "'elementName' cannot be null");

			if (elementName.Length == 0)
				throw new ArgumentException("'elementName' cannot be empty", "elementName");

			xmlWriter.WriteStartElement(elementName);
			xmlWriter.WriteValue(value);
			xmlWriter.WriteEndElement();

			return this;
		}

		//---------------------------------------------------------------------
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters")]
		public override IMessageFormatter OpenElement(string elementName)
		{
			if (elementName == null)
				throw new ArgumentNullException("elementName", "'elementName' cannot be null");

			if (elementName.Length == 0)
				throw new ArgumentException("'elementName' cannot be empty", "elementName");

			xmlWriter.WriteStartElement(elementName);

			return this;
		}

		//---------------------------------------------------------------------
		public override IMessageFormatter CloseElement()
		{
			xmlWriter.WriteEndElement();

			return this;
		}

		//---------------------------------------------------------------------
		public override void Clear()
		{
			xmlStringBuilder.Remove(0, xmlStringBuilder.Length);
			//TODO: verificare se basta resettare lo string builder
		}

		#endregion

		//---------------------------------------------------------------------
		public override string ToString()
		{
			xmlWriter.Flush();

			string temp = xmlStringBuilder.ToString();

			xmlStringBuilder.Remove(0, xmlStringBuilder.Length);

			InitWriter();

			return temp;
		}

		#region IDisposable Members

		//---------------------------------------------------------------------
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		//---------------------------------------------------------------------
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (xmlWriter != null)
				{
					xmlWriter.Close();
					xmlWriter = null;
				}
			}
		}

		#endregion
	}
}
