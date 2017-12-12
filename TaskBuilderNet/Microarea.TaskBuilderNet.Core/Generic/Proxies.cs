using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Runtime.Serialization;
using System.Web.Services.Protocols;
using System.Xml.Serialization;
using Microarea.TaskBuilderNet.Core.XmlPersister;

namespace Microarea.TaskBuilderNet.Core.Generic
{
	//=========================================================================
	[Serializable]
	public class GlobalSettings : State
	{
		public GlobalSettings(){}	// DO NOT REMOVE : needed for XmlSerialization
		public CommunicationSettings	CommunicationSettings = new CommunicationSettings();
		
		//---------------------------------------------------------------------
		public static GlobalSettings GetGlobalSettings(string globalFilePath)
		{
			return GlobalSettings.GetFromXml(globalFilePath, typeof(GlobalSettings)) as GlobalSettings;
		}
	}

	//=========================================================================
	[Serializable]
	public class CommunicationSettings : State
	{
		public enum CommunicationMode {Sync, Async};
		public CommunicationMode Mode = CommunicationMode.Sync;
		public string RequestDefaultPath = null;

		//---------------------------------------------------------------------
		[XmlIgnore]
		public bool IsSynch 
		{
			get 
			{
				return Mode == CommunicationMode.Sync;
			} 
			set 
			{
				if (value) 
					Mode = CommunicationMode.Sync; 
				else 
					Mode = CommunicationMode.Async;
			}
		}

		//---------------------------------------------------------------------
		public CommunicationSettings(){}	// DO NOT REMOVE : needed for XmlSerialization
	}

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
	public class ProxySettings : State, ICloneable
	{
		// public data members, persisted in XML state file
		public ProxyAddress HttpProxy = new ProxyAddress();
		public ProxyAddress FtpProxy = new ProxyAddress();
		public FirewallCredentialsSettings FirewallCredentialsSettings = new FirewallCredentialsSettings();

		// private members
		private string filePath;

		//---------------------------------------------------------------------
		public ProxySettings(){}	// DO NOT REMOVE : needed for XmlSerialization
		public void SetFilePath(string filePath)
		{
			this.filePath = filePath;
		}

		//---------------------------------------------------------------------
		public bool Save()
		{
			if (this.filePath == null)
				return false;
			return this.SaveToXml(filePath);
		}

		//---------------------------------------------------------------------
		public static ProxySettings GetServerProxySetting(string proxiesFilePath)
		{
			return ProxySettings.GetFromXml(proxiesFilePath, typeof(ProxySettings)) as ProxySettings;
		}

		//---------------------------------------------------------------------
		private static WebProxy CreateProxy(string proxyUrl, int proxyPort)
		{
			if (proxyUrl == null || proxyUrl.Length == 0)
				return null;

			if (proxyPort < 0)
				proxyPort = 80;

			string proxyUriExt = string.Concat(proxyUrl, ":", proxyPort.ToString(CultureInfo.InvariantCulture));
			try
			{
				Uri uri = new Uri(proxyUriExt);
				return new WebProxy(uri, false);
			}
			catch (Exception exc)
			{
				Debug.Fail(exc.Message);
				return null;
			}
		}

		//---------------------------------------------------------------------
		public static bool SetRequestCredentials
			(
			SoapHttpClientProtocol soapClient,
			string proxiesFilePath
			)
		{
			return SetRequestCredentials(soapClient, GetServerProxySetting(proxiesFilePath));
		}

		//---------------------------------------------------------------------
		public static bool SetRequestCredentials
			(
			SoapHttpClientProtocol soapClient,
			ProxySettings proxySettings
			)
		{
			if (soapClient == null || proxySettings == null)
				return false;

			soapClient.Proxy = CreateProxy
				(
				proxySettings.HttpProxy.Server, 
				proxySettings.HttpProxy.Port
				);
		
			if (soapClient.Proxy == null)
				return false;

			if (!proxySettings.FirewallCredentialsSettings.NeedsCredentials)
				return false;

			NetworkCredential nc = null;
			if (
				proxySettings.FirewallCredentialsSettings.Name == null ||
				proxySettings.FirewallCredentialsSettings.Name.Trim().Length == 0 ||
				proxySettings.FirewallCredentialsSettings.Password == null ||
				proxySettings.FirewallCredentialsSettings.Password.Trim().Length == 0
				)
				nc = CredentialCache.DefaultNetworkCredentials;
			else
			{
				nc =
					new NetworkCredential
					(
					proxySettings.FirewallCredentialsSettings.Name,
					Storer.Unstore(proxySettings.FirewallCredentialsSettings.Password),
					proxySettings.FirewallCredentialsSettings.Domain
					);
			}

			CredentialCache myCache = new CredentialCache();
 
			string address = soapClient.Url;
			string mode = "NTLM";
			myCache.Add(new Uri(address), mode, nc);
			soapClient.Proxy.Credentials = myCache.GetCredential(new Uri(address), mode);
			
			return true;
		}

		#region ICloneable Members

		//---------------------------------------------------------------------
		public object Clone()
		{
			ProxySettings ps = new ProxySettings();

			ps.filePath = filePath;

			FirewallCredentialsSettings fcs = new FirewallCredentialsSettings();
			fcs.Domain = FirewallCredentialsSettings.Domain;
			fcs.Name = FirewallCredentialsSettings.Name;
			fcs.Password = FirewallCredentialsSettings.Password;
			fcs.NeedsCredentials = FirewallCredentialsSettings.NeedsCredentials;
			ps.FirewallCredentialsSettings = fcs;

			ProxyAddress pa = new ProxyAddress();
			pa.Server = HttpProxy.Server;
			pa.Port = HttpProxy.Port;
			ps.HttpProxy = pa;

			pa = new ProxyAddress();
			pa.Server = HttpProxy.Server;
			pa.Port = HttpProxy.Port;
			ps.FtpProxy = pa;

			return ps;
		}

		#endregion

		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			ProxySettings ps = obj as ProxySettings;
			if (ps == null)
				return false;

			return String.Compare(this.FirewallCredentialsSettings.Domain, ps.FirewallCredentialsSettings.Domain, StringComparison.InvariantCultureIgnoreCase) == 0 &&
				String.Compare(this.FirewallCredentialsSettings.Name, ps.FirewallCredentialsSettings.Name, StringComparison.InvariantCultureIgnoreCase) == 0 &&
				String.Compare(this.FirewallCredentialsSettings.Password, ps.FirewallCredentialsSettings.Password, StringComparison.InvariantCultureIgnoreCase) == 0 &&
				this.FirewallCredentialsSettings.NeedsCredentials == ps.FirewallCredentialsSettings.NeedsCredentials &&
				String.Compare(this.FtpProxy.Server, ps.FtpProxy.Server, StringComparison.InvariantCultureIgnoreCase) == 0 &&
				String.Compare(this.HttpProxy.Server, ps.HttpProxy.Server, StringComparison.InvariantCultureIgnoreCase) == 0 &&
				this.FtpProxy.Port == ps.FtpProxy.Port &&
				this.HttpProxy.Port == ps.HttpProxy.Port;
		}

		//---------------------------------------------------------------------
		public override int GetHashCode()
		{
			return FirewallCredentialsSettings.GetHashCode() ^ FtpProxy.GetHashCode() ^ HttpProxy.GetHashCode();
		}
	}

	/// <summary>
	/// PackException.
	/// </summary>
	//=========================================================================
	[Serializable]
	public class ProxySettingsException : Exception
	{
		//---------------------------------------------------------------------
		public ProxySettingsException(string message, Exception innerException)
			: base (message, innerException)
		{}

		//---------------------------------------------------------------------
		public ProxySettingsException(string message)
			: base (message, null)
		{}

		// Needed for xml serialization.
		//---------------------------------------------------------------------
		protected ProxySettingsException(
			SerializationInfo info,
			StreamingContext context
			)
			: base (info, context)
		{}
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
