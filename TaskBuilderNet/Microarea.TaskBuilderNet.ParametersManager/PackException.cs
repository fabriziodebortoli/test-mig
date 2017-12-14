using System;
using System.Runtime.Serialization;

namespace Microarea.TaskBuilderNet.ParametersManager
{
	/// <summary>
	/// PackException.
	/// </summary>
	//=========================================================================
	[Serializable]
	public class PackException : Exception
	{
		//---------------------------------------------------------------------
		public PackException(string message, Exception innerException)
			: base (message, innerException)
		{}

		//---------------------------------------------------------------------
		public PackException(string message)
			: base (message, null)
		{}

		// Needed for xml serialization.
		//---------------------------------------------------------------------
		protected PackException(
			SerializationInfo info,
			StreamingContext context
			)
			: base (info, context)
		{}
	}
}
