using System;
using System.Collections.Generic;
using System.Globalization;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microsoft.Win32;

namespace Microarea.TaskBuilderNet.Data.SQLDataAccess
{
	/// <summary>
	/// SQLRegistryAccess class
	/// Legge le impostazioni dal Registry del PC selezionato per sapere i default di SQL
	/// </summary>
	// ========================================================================
	public class SQLRegistryAccess
	{
		private bool	isPrimaryIstanceOfSql	= false;
		private string	instanceName	= string.Empty;
		private string	serverName		= string.Empty;
		
		// elenco istanze post SQL2005 disponibili sulla macchina
		private Dictionary<string, string> instancesAfter2005Dic = new Dictionary<string, string>();
		// elenco istanze disponibili sulla macchina
		string[] allInstalledInstances = new string[] { }; 

		private Diagnostic diagnostic = new Diagnostic("SQLRegistryAccess");

		// Variabili statiche (percorso chiavi di registry)
		//---------------------------------------------------------------------
		private const string installedInstancesKey = "SOFTWARE\\Microsoft\\Microsoft SQL Server";
		private const string instancesAfter2005Key = "SOFTWARE\\Microsoft\\Microsoft SQL Server\\Instance Names\\SQL";
		private const string simpleMSSqlServerKey = "SOFTWARE\\Microsoft\\MSSQLServer\\MSSQLServer";
		private const string post2005MSSqlServerKey = "Software\\Microsoft\\Microsoft SQL Server\\{0}\\MSSQLServer";

		public bool   IsPrimaryIstanceOfSql { get { return isPrimaryIstanceOfSql; } set { isPrimaryIstanceOfSql = value; }}
		public string IstanceName   		{ get { return instanceName;		}	set { instanceName			= value; }}
		public string ServerName			{ get { return serverName;			}	set { serverName			= value; }}

		public Diagnostic Diagnostic { get { return diagnostic; } }

		//---------------------------------------------------------------------
		public SQLRegistryAccess(string serverName, string serverIstance)
		{
			ServerName = serverName;
			
			isPrimaryIstanceOfSql = (serverIstance == null || serverIstance.Length == 0);
			if (!isPrimaryIstanceOfSql)
				instanceName = serverIstance.ToUpper(CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// GetLoginModeType
		/// data la stringa memorizzata nel registry LoginMode ritorna il tipo di login 
		/// (Windows Authentication oppure MixedMode)
		/// </summary>
		//---------------------------------------------------------------------
		private string GetLoginModeType(string loginMode)
		{
			string typeOfLogin = string.Empty;
			
			switch(loginMode)
			{
				case "1":
					typeOfLogin = "Integrated Security";	
					break;

				case "2": 
				default:
					typeOfLogin = "Mixed Mode";	
					break;
			}

			return typeOfLogin;
		}

		/// <summary>
		/// IsLocalComputer
		/// </summary>
		/// <returns>true se la connessione è su computer locale</returns>
		//---------------------------------------------------------------------
		private bool IsLocalComputer()
		{
			return (String.Compare(System.Net.Dns.GetHostName(), ServerName, StringComparison.InvariantCultureIgnoreCase) == 0);
		}

		///<summary>
		/// Metodo che cerca di individuare leggendo il registry le informazioni relative al LoginMode
		/// di un server SQL.
		/// Prima vengono caricate le istanze installate, poi il nome della chiave per le istanze di versione maggiore/uguale
		/// a SQL 2005
		/// Poi, a seconda della presenza dell'istanza secondaria o meno, vado a recuperare le info nel registry
		///</summary>
		//---------------------------------------------------------------------
		public string LoginMode()
		{
			// carico dal registry l'elenco delle istanze di SQL Server installate ed eventualmente le chiavi di registro
			// specifiche ad esse associato (per le versioni dal SQL2005 in poi)
			LoadInstalledInstances();

			// se il nome istanza e' vuoto procedo a cercare la sua versione
			string typeOfLogin = (string.IsNullOrEmpty(instanceName)) ? CheckPrimaryInstance() : CheckSecondaryInstance();

			return typeOfLogin;
		}

		///<summary>
		/// Lettura nel registry delle istanze di SQL Server installate sulla macchina, nonche' le eventuali
		/// chiavi di registro per identificarle, con la sintassi introdotta da SQL2005 (MSSQL.1/MSSQL10.EXPRESS2008)
		///</summary>
		//---------------------------------------------------------------------
		private void LoadInstalledInstances()
		{
			RegistryKey rkPrimary = null;

			try
			{
				// leggo la chiave "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server"
				// per avere l'elenco delle istanze installate
				rkPrimary =
					(IsLocalComputer())
					? Registry.LocalMachine.OpenSubKey(installedInstancesKey)
					: RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, ServerName).OpenSubKey(installedInstancesKey);

				if (rkPrimary == null)
					return;

				string[] values = rkPrimary.GetValueNames();
				foreach (string val in values)
				{
					// cerco il nome CurrentVersion per leggere la versione di SQL
					if (string.Compare(val, "InstalledInstances", StringComparison.InvariantCultureIgnoreCase) == 0)
					{
						allInstalledInstances = ((string[])(rkPrimary.GetValue("InstalledInstances")));
						break;
					}
				}

				// leggo la chiave "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SQL Server"
				// per avere l'elenco delle istanze e relative chiavi di registro di server da SQL2005 in poi
				rkPrimary =
					(IsLocalComputer())
					? Registry.LocalMachine.OpenSubKey(instancesAfter2005Key)
					: RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, ServerName).OpenSubKey(instancesAfter2005Key);

				if (rkPrimary == null)
					return;

				// memorizzo in un dictionary tutti nomi delle istanze trovati versioni post SQL2005
				string[] instanceValues = rkPrimary.GetValueNames();
				foreach (string val in instanceValues)
					instancesAfter2005Dic.Add(val.ToUpper(CultureInfo.InvariantCulture), rkPrimary.GetValue(val).ToString());

				if (rkPrimary != null)
					rkPrimary.Close();
			}
			catch (System.IO.IOException)
			{
			}
			catch (System.UnauthorizedAccessException)
			{
			}
			catch (System.Security.SecurityException)
			{
			}
			catch (System.NullReferenceException)
			{
			}
			catch (System.ArgumentNullException)
			{
			}
		}

		///<summary>
		/// Ricerca info nel registry per l'istanza primaria di SQL (solitamente nome computer)
		///</summary>
		//---------------------------------------------------------------------
		private string CheckPrimaryInstance()
		{
			string typeOfLogin = string.Empty;

			try
			{
				// leggo la chiave "SOFTWARE\\Microsoft\\MSSQLServer\\MSSQLServer"
				// per avere la chiave semplice di SQL 2000
				RegistryKey rkPrimary =
					(IsLocalComputer())
					? Registry.LocalMachine.OpenSubKey(simpleMSSqlServerKey)
					: RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, ServerName).OpenSubKey(simpleMSSqlServerKey);

				// se trovo la chiave allora leggo subito il LoginMode
				if (rkPrimary != null && rkPrimary.GetValue(Consts.LoginMode) != null)
				{
					string lMode = rkPrimary.GetValue(Consts.LoginMode).ToString();
					rkPrimary.Close();
					typeOfLogin = GetLoginModeType(lMode.ToString());
				}
				
				// se non l'ho trovato allora provo a cercare
				// nell'elenco delle istanze after SQL2005 il nome della relativa chiave del registry
				string key = string.Empty;
				if (instancesAfter2005Dic.TryGetValue("MSSQLSERVER", out key))
				{
					rkPrimary =
						(IsLocalComputer())
						? Registry.LocalMachine.OpenSubKey(string.Format(post2005MSSqlServerKey, key))
						: RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, ServerName).OpenSubKey(string.Format(post2005MSSqlServerKey, key));

					// se trovo la chiave allora leggo subito il LoginMode
					if (rkPrimary != null && rkPrimary.GetValue(Consts.LoginMode) != null)
					{
						string lMode = rkPrimary.GetValue(Consts.LoginMode).ToString();
						rkPrimary.Close();
						typeOfLogin = GetLoginModeType(lMode.ToString());
					}
				}

				if (rkPrimary != null)
					rkPrimary.Close();
			}
			catch (System.IO.IOException)
			{
			}
			catch (System.UnauthorizedAccessException)
			{
			}
			catch (System.Security.SecurityException)
			{
			}
			catch (System.NullReferenceException)
			{
			}
			catch (System.ArgumentNullException)
			{
			}

			return typeOfLogin;
		}

		///<summary>
		/// Ricerca info nel registry per l'istanza secondaria di SQL (solitamente nome computer\istanza)
		///</summary>
		//---------------------------------------------------------------------
		private string CheckSecondaryInstance()
		{
			RegistryKey rkPrimary = null;
			string typeOfLogin = string.Empty;

			try
			{
				// se trovo l'istanza nell'elenco delle istanze post SQL2005, leggo la sua chiave e
				// vado subito a cercare il loginmode
				string key = string.Empty;
				if (instancesAfter2005Dic.TryGetValue(instanceName, out key))
				{
					rkPrimary =
						(IsLocalComputer())
						? Registry.LocalMachine.OpenSubKey(string.Format(post2005MSSqlServerKey, key))
						: RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, ServerName).OpenSubKey(string.Format(post2005MSSqlServerKey, key));

					// se trovo la chiave allora leggo subito il LoginMode
					if (rkPrimary != null && rkPrimary.GetValue(Consts.LoginMode) != null)
					{
						string lMode = rkPrimary.GetValue(Consts.LoginMode).ToString();
						rkPrimary.Close();
						typeOfLogin = GetLoginModeType(lMode.ToString());
					}
				}
				else
				{
					// se non ho trovato l'istanza puo' darsi che si tratti di msde, quindi vado direttamente col suo nome
					rkPrimary =
						(IsLocalComputer())
						? Registry.LocalMachine.OpenSubKey(string.Format(post2005MSSqlServerKey, instanceName))
						: RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, ServerName).OpenSubKey(string.Format(post2005MSSqlServerKey, instanceName));

					// se trovo la chiave allora leggo subito il LoginMode
					if (rkPrimary != null && rkPrimary.GetValue(Consts.LoginMode) != null)
					{
						string lMode = rkPrimary.GetValue(Consts.LoginMode).ToString();
						rkPrimary.Close();
						typeOfLogin = GetLoginModeType(lMode.ToString());
					}
					else
					{
						rkPrimary =
							(IsLocalComputer())
							? Registry.LocalMachine.OpenSubKey(simpleMSSqlServerKey)
							: RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, ServerName).OpenSubKey(simpleMSSqlServerKey);

						// se trovo la chiave allora leggo subito il LoginMode
						if (rkPrimary != null && rkPrimary.GetValue(Consts.LoginMode) != null)
						{
							string lMode = rkPrimary.GetValue(Consts.LoginMode).ToString();
							rkPrimary.Close();
							typeOfLogin = GetLoginModeType(lMode.ToString());
						}
					}
				}

				if (rkPrimary != null)
					rkPrimary.Close();
			}
			catch (System.IO.IOException)
			{
			}
			catch (System.UnauthorizedAccessException)
			{
			}
			catch (System.Security.SecurityException)
			{
			}
			catch (System.NullReferenceException)
			{
			}
			catch (System.ArgumentNullException)
			{
			}

			return typeOfLogin;
		}

		# region Vecchio codice commentato per il browse del registry prima del supporto a SQL2008

		// stringhe generiche
		//---------------------------------------------------------------------
		const string mssqlCurrentVersion = "Software\\Microsoft\\MSSQLServer\\MSSQLServer\\CurrentVersion";

		const string nativeClientCurrentVersion = "SOFTWARE\\Microsoft\\Microsoft SQL Native Client\\CurrentVersion";

		// stringhe pe la versione SQL2000
		//---------------------------------------------------------------------
		const string sqlTypeOfLoginLocalServer = "Software\\Microsoft\\MSSQLServer\\MSSQLServer";

		const string sqlTypeOfLoginIstanceServer = "Software\\Microsoft\\Microsoft SQL Server\\{0}\\MSSQLServer";

		// stringhe per le istanze (solo versione SQL2005)
		//---------------------------------------------------------------------
		const string sql2005Instances = "SOFTWARE\\Microsoft\\Microsoft SQL Server\\Instance Names\\SQL";

		const string sql2005InstancesVersions = "Software\\Microsoft\\Microsoft SQL Server\\{0}\\MSSQLServer\\CurrentVersion";

//		private Hashtable instances = null;
//		private bool is2005Version = false;
		
		/// <summary>
		/// IsSQL2005Installed
		/// Browsa il registry e va a vedere se si tratta della versione di SQL2005
		/// Se così fosse procede a memorizzare in una mappa le istanze presenti MSSQL.n 
		/// (dove andare a cercare il valore del LoginMode successivamente)
		/// </summary>
		/// <returns>true se è la versione del server(+ eventuale istanza) è SQL2005</returns>
		//---------------------------------------------------------------------
/*		private bool IsSQL2005Installed()
		{
			bool is2005 = false;
			string version = string.Empty;
			RegistryKey rkPrimary = null;

			try
			{
				// leggo la chiave "Software\\Microsoft\\MSSQLServer\\MSSQLServer\\CurrentVersion"
				rkPrimary =
					(IsLocalComputer())
					? Registry.LocalMachine.OpenSubKey(mssqlCurrentVersion)
					: RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, ServerName).OpenSubKey(mssqlCurrentVersion);

				// se è diverso da null vuol dire che c'è una versione 8.0
				if (rkPrimary != null)
				{
					string[] values = rkPrimary.GetValueNames();

					foreach (string val in values)
					{
						// cerco il nome CurrentVersion per leggere la versione di SQL
						if (String.Compare(val, "CurrentVersion", true, CultureInfo.InvariantCulture) == 0)
						{
							version = rkPrimary.GetValue(val).ToString();
							is2005 = version.StartsWith("9.00");
							break;
						}
					}
				}
				else
				{
					// se rkPrimary è == null allora vado a cercare la versione per il server 2005
					// leggo la chiave "SOFTWARE\\Microsoft\\Microsoft SQL Native Client\\CurrentVersion"
					rkPrimary =
						(IsLocalComputer())
						? Registry.LocalMachine.OpenSubKey(nativeClientCurrentVersion)
						: RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, ServerName).OpenSubKey(nativeClientCurrentVersion);

					if (rkPrimary != null)
					{
						string[] values = rkPrimary.GetValueNames();

						foreach (string val in values)
						{
							// cerco il nome CurrentVersion per leggere la versione di SQL
							if (String.Compare(val, "Version", true, CultureInfo.InvariantCulture) == 0)
							{
								version = rkPrimary.GetValue(val).ToString();
								// se il valore inizia con 9.00 si tratta di 2005
								is2005 = version.StartsWith("9.00");
								break;
							}
						}
					}
				}

				if (rkPrimary != null)
					rkPrimary.Close();
			}
			catch (System.NullReferenceException)
			{
			}
			catch (System.IO.IOException)
			{
			}
			catch (System.UnauthorizedAccessException)
			{
			}
			catch (System.Security.SecurityException)
			{
			}
			catch (System.ArgumentNullException)
			{
			}

			// carico comunque le istanze presenti nel registry
			version = LoadAndCheckInstances();
			if (version.Length == 0)
				return is2005;

			// caso di istanza SQLExpress (2005) su server SQL2000 (comanda l'istanza 2005)
			if (!is2005 && version.StartsWith("9.00"))
				is2005 = true;

			return is2005;
		}

		/// <summary>
		/// LoginMode
		/// Legge la modalità di Login dell'installazione SQL selezionata
		/// </summary>
		/// <returns>2 for mixed-mode or 1 for integrated</returns>
		//---------------------------------------------------------------------
		public string LoginMode_old()
		{
			// controllo se la versione del server a cui mi sto connettendo è 2000 o 2005
			// per pilotare gli accessi alle chiavi di registro
			is2005Version = IsSQL2005Installed();

			string typeOfLogin = string.Empty;
			StringBuilder loginMode = new StringBuilder();
			RegistryKey rkPrimary = null;

			try
			{
				if (IsLocalComputer())
				{
					// Local Computer + Istanza Primaria
					if (IsPrimaryIstanceOfSql)
					{
						rkPrimary = (is2005Version)
							? Registry.LocalMachine.OpenSubKey
							(string.Format
							(sqlTypeOfLoginIstanceServer,
							instances.Contains("MSSQLSERVER") ? instances["MSSQLSERVER"].ToString() : string.Empty))
							: Registry.LocalMachine.OpenSubKey(sqlTypeOfLoginLocalServer);

						if (rkPrimary != null)
						{
							loginMode.Append(rkPrimary.GetValue(Consts.LoginMode).ToString());
							rkPrimary.Close();
							typeOfLogin = GetLoginModeType(loginMode.ToString());
						}
						else
							return string.Empty;
					}
					else // Local Computer + Istanza secondaria
					{
						rkPrimary = (is2005Version)
							? Registry.LocalMachine.OpenSubKey
							(string.Format
							(sqlTypeOfLoginIstanceServer,
							instances.Contains(instanceName) ? instances[instanceName].ToString() : string.Empty))
							: Registry.LocalMachine.OpenSubKey(string.Format(sqlTypeOfLoginIstanceServer, instanceName));

						if (rkPrimary != null)
						{
							loginMode.Append(rkPrimary.GetValue(Consts.LoginMode).ToString());
							rkPrimary.Close();
							typeOfLogin = GetLoginModeType(loginMode.ToString());
						}
						else
							return string.Empty;
					}
				}
				else
				{
					// Remote Computer + Istanza Primaria
					if (IsPrimaryIstanceOfSql)
					{
						rkPrimary = (is2005Version)
							? RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, ServerName).OpenSubKey
							(string.Format
							(sqlTypeOfLoginIstanceServer,
							instances.Contains("MSSQLSERVER") ? instances["MSSQLSERVER"].ToString() : string.Empty))
							: RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, ServerName).OpenSubKey(sqlTypeOfLoginLocalServer);

						if (rkPrimary != null)
						{
							loginMode.Append(rkPrimary.GetValue(Consts.LoginMode).ToString());
							rkPrimary.Close();
							typeOfLogin = GetLoginModeType(loginMode.ToString());
						}
						else
							return string.Empty;
					}
					else // Remote Computer + Istanza Secondaria
					{
						rkPrimary = (is2005Version)
							? RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, ServerName).
							OpenSubKey(string.Format
							(sqlTypeOfLoginIstanceServer,
							instances.Contains(instanceName) ? instances[instanceName].ToString() : string.Empty))
							: RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, ServerName).
							OpenSubKey(string.Format(sqlTypeOfLoginIstanceServer, instanceName));

						if (rkPrimary != null)
						{
							loginMode.Append(rkPrimary.GetValue(Consts.LoginMode).ToString());
							rkPrimary.Close();
							typeOfLogin = GetLoginModeType(loginMode.ToString());
						}
						else
							return string.Empty;
					}
				}
			}
			catch (System.NullReferenceException excNullRef)
			{
				Diagnostic.Set(DiagnosticType.Error, excNullRef.Message);
				return string.Empty;
			}
			catch (System.IO.IOException excIO)
			{
				Diagnostic.Set(DiagnosticType.Error, excIO.Message);
				return string.Empty;
			}
			catch (System.UnauthorizedAccessException excUnauthorized)
			{
				Diagnostic.Set(DiagnosticType.Error, excUnauthorized.Message);
				return string.Empty;
			}
			catch (System.Security.SecurityException excSecurity)
			{
				Diagnostic.Set(DiagnosticType.Error, excSecurity.Message);
				return string.Empty;
			}
			catch (System.ArgumentNullException excNullArg)
			{
				Diagnostic.Set(DiagnosticType.Error, excNullArg.Message);
				return string.Empty;
			}

			return typeOfLogin;
		}

		/// <summary>
		/// LoadAndCheckInstances
		/// Carica le istanze di sql presenti nel registry e verifica la loro versione
		/// </summary>
		//---------------------------------------------------------------------
		private string LoadAndCheckInstances()
		{
			string version = string.Empty;
			RegistryKey rkPrimary = null;

			try
			{
				// leggo la chiave "SOFTWARE\\Microsoft\\Microsoft SQL Server\\Instance Names\\SQL"
				// per vedere se ho delle istanze installate
				rkPrimary =
					(IsLocalComputer())
					? Registry.LocalMachine.OpenSubKey(sql2005Instances)
					: RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, ServerName).OpenSubKey(sql2005Instances);

				if (rkPrimary == null)
					return version;

				// memorizzo in una mappa tutti nomi delle istanze trovati
				instances = new Hashtable();
				string[] instanceValues = rkPrimary.GetValueNames();
				foreach (string val in instanceValues)
					instances.Add(val.ToUpper(CultureInfo.InvariantCulture), rkPrimary.GetValue(val).ToString());

				// se l'istanza con cui sto tentando di connettermi esiste nel registry controllo la sua versione
				if (instances.Contains(instanceName.ToUpper(CultureInfo.InvariantCulture)))
				{
					rkPrimary = IsLocalComputer()
						? Registry.LocalMachine.OpenSubKey(string.Format(sql2005InstancesVersions, instanceName))
						: RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, ServerName).OpenSubKey(string.Format(sql2005InstancesVersions, instanceName));

					if (rkPrimary != null)
					{
						string[] values = rkPrimary.GetValueNames();

						foreach (string val in values)
						{
							// cerco il nome CurrentVersion per leggere la versione di SQL
							if (string.Compare(val, "CurrentVersion", true, CultureInfo.InvariantCulture) == 0)
							{
								version = rkPrimary.GetValue(val).ToString();
								break;
							}
						}
					}
				}

				if (rkPrimary != null)
					rkPrimary.Close();
			}
			catch (System.IO.IOException)
			{
			}
			catch (System.UnauthorizedAccessException)
			{
			}
			catch (System.Security.SecurityException)
			{
			}
			catch (System.NullReferenceException)
			{
			}
			catch (System.ArgumentNullException)
			{
			}

			return version;
		}
 * */
		# endregion
	}
}