using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Core.SoapCall
{
	public class WCFServiceRegister
	{
		//-----------------------------------------------------------------------------
		public static void RegisterIstallation (string installationPath, int startPort, string user, ILogWriter log)
		{
			List<string> nsList = GetNamespaceListFromInstallation(installationPath, log);
			if (nsList == null)
				return;
			RegisterServicesForUser(nsList, startPort, user, log);
		}

		//-----------------------------------------------------------------------------
		private static List<string> GetNamespaceListFromInstallation (string installationPath, ILogWriter log)
		{
			string path = Path.Combine(installationPath, "Standard");
			if (!Directory.Exists(installationPath))
			{
				log.WriteLine(string.Format("Path not found: '{0}'", path));
				return null;
			}

			return GetNamespaceList(path, log);
		}

		//-----------------------------------------------------------------------------
		public static void UnregisterInstallation (string installationPath, int startPort, ILogWriter log)
		{
			List<string> nsList = GetNamespaceListFromInstallation(installationPath, log);
			if (nsList == null)
				return;
			UnregisterServices(nsList, startPort, log);
		}
		//-----------------------------------------------------------------------------
		public static void RegisterServicesForUser (List<string> nsList, int startPort, string user, ILogWriter log)
		{
			try
			{
				log.WriteLine("Registration started...");
				HttpApi nsManager = new HttpApi();
				SecurityDescriptor newSd = new SecurityDescriptor();
				newSd.DACL = new AccessControlList();

				SecurityIdentity sid = SecurityIdentity.SecurityIdentityFromName(user);

				AccessControlEntry ace = new AccessControlEntry(sid);
				ace.AceType = AceType.AccessAllowed;
				ace.Add(AceRights.GenericAll);
				ace.Add(AceRights.GenericExecute);
				ace.Add(AceRights.GenericRead);
				ace.Add(AceRights.GenericWrite);

				newSd.DACL.Add(ace);
				Dictionary<string, SecurityDescriptor> registeredUris = nsManager.QueryHttpNamespaceAcls();
				for (int nPort = startPort; nPort < startPort + 10; nPort++)
				{
					foreach (string service in nsList)
						RegisterUri(nsManager, newSd, registeredUris, new Uri(string.Format("http://localhost:{0}/{1}/", nPort, service)), log);
					RegisterUri(nsManager, newSd, registeredUris, new Uri(string.Format("http://localhost:{0}/disco/", nPort)), log);
				}
			}
			catch (Exception ex)
			{
				log.WriteLine(ex.ToString());
			}
			finally 
			{
				log.WriteLine("Registration ended...");
			}
		}

		//-----------------------------------------------------------------------------
		private static void UnregisterServices (List<string> nsList, int startPort, ILogWriter log)
		{
			try
			{
				log.WriteLine("Deregistration started...");
				HttpApi nsManager = new HttpApi();
				Dictionary<string, SecurityDescriptor> registeredUris = nsManager.QueryHttpNamespaceAcls();
				for (int nPort = startPort; nPort < startPort + 20; nPort++)
				{
					foreach (string service in nsList)
						UnregisterUri(nsManager, registeredUris, new Uri(string.Format("http://localhost:{0}/{1}/", nPort, service)), log);
					UnregisterUri(nsManager, registeredUris, new Uri(string.Format("http://localhost:{0}/disco/", nPort)), log);
				}
			}
			catch (Exception ex)
			{
				log.WriteLine(ex.ToString());
			}
			finally 
			{
				log.WriteLine("Deregistration ended...");				
			}
		}

		//-----------------------------------------------------------------------------
		private static void RegisterUri(
			HttpApi nsManager,
			SecurityDescriptor newSd,
			Dictionary<string, SecurityDescriptor> registeredUris,
			Uri uri,
			ILogWriter log)
		{
			try
			{
				string sUri = AdjustUri(uri);

				log.WriteLine(string.Format("Registering '{0}'", sUri));
				if (registeredUris.ContainsKey(sUri))
					nsManager.RemoveHttpHamespaceAcl(sUri);
				nsManager.SetHttpNamespaceAcl(sUri, newSd);
			}
			catch (Exception ex)
			{
				log.WriteLine(ex.ToString());
			}
		}

		//-----------------------------------------------------------------------------
		private static void UnregisterUri(
			HttpApi nsManager,
			Dictionary<string, SecurityDescriptor> registeredUris,
			Uri uri,
			ILogWriter log)
		{
			try
			{
				string sUri = AdjustUri(uri);

				log.WriteLine(string.Format("Unregistering '{0}'", sUri));
				if (registeredUris.ContainsKey(sUri))
					nsManager.RemoveHttpHamespaceAcl(sUri);
			}
			catch (Exception ex)
			{
				log.WriteLine(ex.ToString());
			}
		}
		//-----------------------------------------------------------------------------
		private static string AdjustUri(Uri uri)
		{
			UriBuilder genericUri = new UriBuilder(uri.ToString());
			genericUri.Host = "+";
			if (!genericUri.Path.EndsWith("/"))
				genericUri.Path += ("/");

			string sUri = genericUri.ToString();
			return sUri;
		}

		//-----------------------------------------------------------------------------
		private static List<string> GetNamespaceList (string path, ILogWriter log)
		{
			List<string> nsList = new List<string>();
			log.WriteLine(string.Format("Retrieving namespace information from path '{0}'...", path));
			foreach (string file in Directory.GetFiles(path, "WebMethods.xml", SearchOption.AllDirectories))
			{
				try
				{
					XmlDocument doc = new XmlDocument();
					doc.Load(file);
					foreach (XmlElement el in doc.SelectNodes("FunctionObjects/Functions/Function"))
					{
						if (el.GetAttribute("WCF") != "true")
							continue;
						string ns = el.GetAttribute("namespace");

						int idx = ns.LastIndexOf('.');
						ns = ns.Substring(0, idx);
						if (nsList.ContainsNoCase(ns))
							continue;

						nsList.Add(ns);
					}

				}
				catch (Exception ex)
				{
					log.WriteLine(ex.ToString());
				}

			}
			return nsList;
		}
	}
}
