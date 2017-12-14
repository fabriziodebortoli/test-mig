using System;

namespace Microarea.Library.LogManager.Loggers
{
	//=========================================================================
	public class MessageFormatterFactory : IMessageFormatterFactory
	{
		#region IMessageFormatterFactory Members

		//---------------------------------------------------------------------
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000", Justification = "XmlMessageFormatter will be disposed by the calling object")]
		public IMessageFormatter CreateMessageFormatter(MessageFormatterType messageFormatterType)
		{
			switch (messageFormatterType)
			{
				case MessageFormatterType.Text:
					return new TxtMessageFormatter();
				case MessageFormatterType.Xml:
					return new XmlMessageFormatter();
				case MessageFormatterType.Html:
					return new HtmlMessageFormatter();
				default:
					throw new ArgumentException("Unrecognized 'messageFormatterType'", "messageFormatterType");
			}
		}

		#endregion
	}
}
