using System;
//
using Microarea.Library.XmlPersister;

namespace Microarea.Library.CommonDeploymentFunctions.States
{
	/// <summary>
	/// Un'istanza di questa classe rappresenta una fotografia
	/// delle impostazioni dei proxies
	/// </summary>
	/// <remarks>
	/// Tutti i suoi membri sono public r/w perché così facendo si possono
	/// serializzare in formato XML utilizzando XmlSerializer.
	/// </remarks>
	//=========================================================================
	[Serializable]
	public class ProxySettings : State
	{
		public ProxyAddress HttpProxy = new ProxyAddress();
		public ProxyAddress FtpProxy = new ProxyAddress();
		public FirewallCredentialsSettings FirewallCredentialsSettings = new FirewallCredentialsSettings();
	}

	//=========================================================================
	[Serializable]
	public struct ProxyAddress
	{
		public	string	Server;
		public	int		Port;
	}

	//=========================================================================
	[Serializable]
	public struct FirewallCredentialsSettings
	{
		public	bool	NeedsCredentials;
		public	string	Domain;
		public	string	Name;
		public	string	Password;
	}
}
