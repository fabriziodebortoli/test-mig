using System;
using System.Runtime.Serialization;

namespace Microarea.Common.NameSolver
{
	//=========================================================================
	[Serializable]
	public class LoadException : Exception, ISerializable
	{
		//---------------------------------------------------------------------
		public LoadException()
			: this(string.Empty, null)
		{ }

		//---------------------------------------------------------------------
		public LoadException(string message)
			: this(message, null)
		{ }

		//---------------------------------------------------------------------
		public LoadException(string message, Exception innerException)
			: base(message, innerException)
		{ }

		// Needed for xml serialization.
		//---------------------------------------------------------------------
		protected LoadException(
			SerializationInfo info,
			StreamingContext context
			)
			//: base(info, context)    TODO rsweb
			: base(info.ToString())
		{ }

		//---------------------------------------------------------------------
		//[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]          TODO rsweb
		[System.Diagnostics.CodeAnalysis.SuppressMessage(
			"Microsoft.Globalization",
			"CA1303:DoNotPassLiteralsAsLocalizedParameters")
		]
		public override void GetObjectData(
			SerializationInfo info,
			StreamingContext context
			)
		{
			if (Object.ReferenceEquals(info, null))
				throw new ArgumentNullException("info", "'info' cannot be null");

			if (Object.ReferenceEquals(info, null))
				throw new ArgumentNullException("context", "'context' cannot be null");

			base.GetObjectData(info, context);

		}
	}
}
