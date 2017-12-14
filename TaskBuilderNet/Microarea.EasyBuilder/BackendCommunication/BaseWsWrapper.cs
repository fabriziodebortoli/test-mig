
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.EasyBuilder.BackendCommunication
{
	/// <summary>
	/// BaseWsWrapper.
	/// </summary>
	//=========================================================================
	internal class BaseWsWrapper
	{
		private ProxySettings proxySettings;

		public ProxySettings ProxySettings { get {return proxySettings;}}

		//---------------------------------------------------------------------
		public BaseWsWrapper
			(
			ProxySettings	proxySettings
			)
		{
			this.proxySettings = proxySettings;
		}
	}
}
