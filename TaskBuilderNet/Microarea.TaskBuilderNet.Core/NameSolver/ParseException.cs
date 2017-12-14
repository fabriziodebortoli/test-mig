using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microarea.TaskBuilderNet.Core.NameSolver
{
	//=========================================================================
	[Serializable]
	public class ParseException : Exception, ISerializable
	{
		//---------------------------------------------------------------------
		public ParseException()
			: this(string.Empty, null)
		{ }

		//---------------------------------------------------------------------
		public ParseException(string message)
			: this(message, null)
		{ }

		//---------------------------------------------------------------------
		public ParseException(string message, Exception innerException)
			: base(message, innerException)
		{ }

		// Needed for xml serialization.
		//---------------------------------------------------------------------
		protected ParseException(
			SerializationInfo info,
			StreamingContext context
			)
			: base(info, context)
		{}

		//---------------------------------------------------------------------
		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
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
