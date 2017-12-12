using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microarea.TaskBuilderNet.Core.EasyBuilder
{
	//=========================================================================
	[Serializable]
	public class EasyStudioCalSoldOutLicenseException : Exception
	{
		//---------------------------------------------------------------------
		public EasyStudioCalSoldOutLicenseException(string message)
			: this(message, null)
		{ }

		//---------------------------------------------------------------------
		public EasyStudioCalSoldOutLicenseException(
			string message,
			Exception innerException
			)
			: base (message, innerException)
		{}

		//---------------------------------------------------------------------
		protected EasyStudioCalSoldOutLicenseException(
			SerializationInfo info,
			StreamingContext context
			)
			: base (info, context)
		{}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		//---------------------------------------------------------------------
		public override void GetObjectData(
			SerializationInfo info,
			StreamingContext context
			)
		{
			base.GetObjectData (info, context);		
		}
	}
}
