using System;
using System.Runtime.Serialization;

namespace Microarea.Library.Internet.BitsWrap
{
	public class BitsException : System.Exception
	{

		public BitsException() : base(){}

		public BitsException(string message) : base(message){}

		public BitsException(string message, Exception innerException)
			: base(message, innerException) {}

		protected BitsException(SerializationInfo info, StreamingContext context)
			: base(info, context) {}
	}
}
