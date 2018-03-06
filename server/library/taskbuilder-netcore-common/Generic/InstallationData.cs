using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;

//TODO RSWEB using Microarea.TaskBuilderNet.Core.WebServicesWrapper;   //unused -> Country + CheckDBSize

using Microarea.Common.NameSolver;
using TaskBuilderNetCore.Interfaces;
using static Microarea.Common.Generic.InstallationInfo;
using System.Diagnostics;

namespace Microarea.Common.Generic
{
    /// <summary>
    /// classe che contiene tutte le informazioni relative all'installazione; sono 
    /// statiche (globali) e caricate ondemand
    /// </summary>
    public class InstallationData
	{
		private static DateTime installationDate = DateTime.MinValue;
		private static IBrandLoader brandLoader = null;
		private static IServerConnectionInfo serverConnectionInfo = null;
		private static bool installationPathCalculated = false;
		private static bool isClickOnceInstallation = false;
		private static string fileSystemServerName = null;
		private static string webServerName = null;
		private static string installationName = null;
		private static List<CultureInfo> cultures = null;
		
		//----------------------------------------------------------------------------
		public static bool IsClickOnceInstallation
		{
			get
			{
				if (!installationPathCalculated)
					CalculateInstallationInfo();
				return InstallationData.isClickOnceInstallation;
			}
		}

		//----------------------------------------------------------------------------
		public static string FileSystemServerName
		{
			get
			{
				if (!installationPathCalculated)
					CalculateInstallationInfo();
				return InstallationData.fileSystemServerName;
			}
		}

		//----------------------------------------------------------------------------
		public static string WebServerName
		{
			get
			{
				if (!installationPathCalculated)
					CalculateInstallationInfo();
				return InstallationData.webServerName;
			}
		}

		//----------------------------------------------------------------------------
		public static string InstallationName
		{
			get
			{
				if (!installationPathCalculated)
					CalculateInstallationInfo();
				return InstallationData.installationName;
			}
		}

		/// <summary>
		/// Oggetto contenente le informazioni relative al file ServerConnection.config
		/// </summary>
		//----------------------------------------------------------------------------
		public static IServerConnectionInfo ServerConnectionInfo
		{
			get
			{
				if (serverConnectionInfo != null)
					return serverConnectionInfo;

				lock (typeof(InstallationData))
				{
					if (serverConnectionInfo != null)
						return serverConnectionInfo;

					serverConnectionInfo = new ServerConnectionInfo();

					if (PathFinder.PathFinderInstance.ExistFile(PathFinder.PathFinderInstance.ServerConnectionFile) &&
						!serverConnectionInfo.Parse(PathFinder.PathFinderInstance.ServerConnectionFile))
						throw new Exception(string.Format(Messages.ErrorReadingFile, PathFinder.PathFinderInstance.ServerConnectionFile));

					return serverConnectionInfo;
				}
			}
		}

		//----------------------------------------------------------------------------
		public static DateTime InstallationDate
		{
			get
			{
				if (installationDate == DateTime.MinValue)
				{
					lock (typeof(InstallationData))
					{
						if (installationDate == DateTime.MinValue)
						{
							string file = PathFinder.PathFinderInstance.GetInstallationVersionPath();
							InstallationVersion info = InstallationVersion.LoadFromOrCreate(file);

							installationDate = info.IDate;
						}

					}
				}
				return installationDate;
			}
		}
		//----------------------------------------------------------------------------
		public static DateTime CacheDate
		{
			get
			{
				string file = PathFinder.PathFinderInstance.GetInstallationVersionPath();
				InstallationVersion info = InstallationVersion.LoadFromOrCreate(file);

				return info.CDate;
			}
		}
		//----------------------------------------------------------------------------
		public static IBrandLoader BrandLoader
		{
			get
			{
				if (brandLoader == null)
				{
					lock (typeof(InstallationData))
					{
						if (brandLoader == null)
						{
							//Matteo/Luca (e anche un po' Ilaria): trick per far apparire subito la splash.
							//In questo modo non dobbiamo chiedere la country a LoginManager.
							//Ovviamente se la splash dipende dalla country questa soluzione non è accettabiile.
							//Attualmente (7/6/11) il brand non dipende dalla country da un bel pezzo, 
							//quando si porrà la necessità di avere il brand variabile con la country 
							//(come coi cinesi) allora bisognerà lavorare su questa parte
							//La country non è disponibilie quando ancora non è stato generato l'userinfo.config (prima attivazione)
							
                            //ilaria cancello tutto
						    brandLoader = new BrandLoader();
						}
					}
				}
				return brandLoader;
			}
		}

		//----------------------------------------------------------------------------
		public static void Clear()
		{
			serverConnectionInfo = null;
			brandLoader = null;
			//country = null;
			installationDate = DateTime.MinValue;
		}

		//---------------------------------------------------------------------
		private static void CalculateInstallationInfo()
		{
			try
			{
				//se l'installazione e' tuttora vuota (caso di EasyLook in remoto), provo a leggere dal file di configurazione su file system 
				if (InstallationInfo.Exists)
				{
					InstallationInfo info = InstallationInfo.Load();
					installationName = info.InstallationName;
					fileSystemServerName = info.FileSystemServer;
					webServerName = info.WebServer;
					isClickOnceInstallation = false;

                    //Se il file InstallationInfo.xml che ho caricato ha le info che mi servono allora ok,
                    //altrimenti proseguo e le calcolo come se non avessi caricato il file InstallationInfo.xml
                    if (!String.IsNullOrWhiteSpace(installationName))
                    {
                        return;
                    }
				}

				//prima verifico se sono un'applicazione ClickOnce, in tal caso pesco le informazioni dall'infrastruttura di ClickOnce
				//if (isClickOnceInstallation = ClickOnceDeploy.GetServerInstallationInfo(out fileSystemServerName, out installationName))
				//{
				//	webServerName = fileSystemServerName;
				//	return;
				//}

				//altrimenti devo trovarmi o nella Standard, o nella Apps del server, il nome installazione
				//e' la cartella immediatamente sopra Apps o Standard, e il nome del server e' quello su cui sto girando
				string pattern = string.Format("\\\\(?<inst>([^\\\\])+)((\\\\{0}\\\\)|(\\\\{1}\\\\))", "Standard", "Apps");
				Assembly asm = Assembly.GetEntryAssembly();
                //Assembly.GetExecutingAssembly();      todo rsweb
                string basePath = asm.Location;
				Match m = Regex.Match(basePath, pattern, RegexOptions.IgnoreCase);
				if (!m.Success)
				{
					m = Regex.Match(/*AppDomain.CurrentDomain.BaseDirectory TODO rsweb*/"" , pattern, RegexOptions.IgnoreCase);
					if (!m.Success)
						return;
				}
				installationName = m.Groups["inst"].Value;
				webServerName = fileSystemServerName = Dns.GetHostName();
			}
			finally
			{
				installationPathCalculated = true;
			}
		}

		//---------------------------------------------------------------------
		public static CultureInfo[] InternalGetInstalledDictionaries(string path)
		{
			//la prima volta che mi vengono chiesti i dizionari installati vado a leggere su file system e popolo l'array
			if (cultures == null)
			{
				cultures = new List<CultureInfo>();
				cultures.Add(new CultureInfo(string.Empty));    //lingua nativa
 				try
                {
					if (PathFinder.PathFinderInstance.ExistPath(path))
					{
						foreach (string folder in Directory.GetDirectories(path))
						{
							string culture = Path.GetFileName(folder);
							try { cultures.Add(new CultureInfo(culture)); }
							catch { }
						}
					}
				} 
                catch(Exception ex)
                {
					//qui non dovrebbe passare
					Debug.Fail(ex.Message);
                }
                
			}

			return cultures.ToArray();
		}
        

        //---------------------------------------------------------------------
        public static CultureInfo[] GetInstalledDictionaries()
		{
			string path = PathFinder.PathFinderInstance.GetStandardDictionaryPath("framework", "tbloader");//Functions.GetAssemblyPath(Assembly.GetEntryAssembly());
			return InternalGetInstalledDictionaries(path);
		}

        //----------------------------------------------------------------------------
/*
        private static string country = null;

        public static string Country
        {
            get
            {
                if (country == null)
                {
                    lock (typeof(InstallationData))
                    {
                        if (country == null)
                        {
                            LoginManager tempLoginManager = new LoginManager();
                            country = tempLoginManager.GetCountry();
                        }
                    }
                }
                return country;
            }
        }

        //---------------------------------------------------------------------

        public static bool CheckDBSize
        {
            get
            {
                LoginManager tempLoginManager = new LoginManager();
                return tempLoginManager.VerifyDBSize();
            }
        }
*/
    }

}
