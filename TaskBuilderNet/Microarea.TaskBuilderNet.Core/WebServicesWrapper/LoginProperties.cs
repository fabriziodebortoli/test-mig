using System;
using System.Runtime.Serialization;

namespace Microarea.WebServices.Core.WebServicesWrapper
{
	//=========================================================================
	[Serializable]
	public class LoginProperties : ISerializable
	{
		string ssoToken;
		string authenticationToken;

		//---------------------------------------------------------------------
		public string AuthenticationToken
		{
			get { return authenticationToken; }
			set { authenticationToken = value; }
		}

		//---------------------------------------------------------------------
		public string SsoToken
		{
			get { return ssoToken; }
			set { ssoToken = value; }
		}

		//---------------------------------------------------------------------
		public LoginProperties()
		{

		}

		//---------------------------------------------------------------------
		protected LoginProperties(SerializationInfo info, StreamingContext context)
		{
			try
			{
				ssoToken = info.GetString("ssoToken");
			}
			catch
			{
				ssoToken = String.Empty;
			}

			try
			{
				authenticationToken = info.GetString("authenticationToken");
			}
			catch
			{
				authenticationToken = String.Empty;
			}
		}

		#region ISerializable Members

		//---------------------------------------------------------------------
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("ssoToken", ssoToken);
			info.AddValue("authenticationToken", authenticationToken);
		}

		#endregion
	}

}
