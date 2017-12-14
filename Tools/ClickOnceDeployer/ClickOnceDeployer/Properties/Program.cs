using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using HttpNamespaceManager.UI;
using ManifestGenerator;
using Microarea.TaskBuilderNet.Core.SoapCall;
using Microarea.TaskBuilderNet.Interfaces;
using Microsoft.Win32;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.Generic;
using System.Text;

namespace ClickOnceDeployer
{
	//================================================================================
	/// <remarks>Legge i parametri di deploy da linea di comanda ed esegue la corrispondente azione; vedere il file sample.bat per esempio di utilizzo.</remarks>
	class Program
	{
		internal enum Action 
		{
			None,
			Deploy,		//effettuo il deploy a beneficio di clickonce (genero i manifest)
			UpdateDeployment,//aggiorno i manifest esistenti con i dati correnti dell'installazione
            Clean,		//rimuovo un deploy effettuato in precedenza
			RegisterWcf,//registro i namespace a beneficio di wcf
			UnregisterWcf,	//deregistro i namespace a beneficio di wcf
			ViewWcf,	//visualizzo lo stato di registrazione dei namespace per wcf
			UI //visualizza la UIform 
		}
		private const string logfilenmame = "ClickOnceDeployer{0}.log";
		/// <summary>
		/// Nome dell'installazione
		/// </summary>
		static string installationName = "";
		/// <summary>
		/// Percorso dell'installazione
		/// </summary>
		static string installationPath ="";
		/// <summary>
		/// Versione della compilata
		/// </summary>
		static string debugRelease = "Release";
		/// <summary>
		/// Lingua dell'installazione
		/// </summary>
		static string uiCulture = "en";
		/// <summary>
		/// Lingua dell'installazione (regional settings)
		/// </summary>
		static string appCulture = Thread.CurrentThread.CurrentCulture.Name;
		/// <summary>
		/// Cartella radice (c:\installazione\Apps)
		/// </summary>
		static string root = "";
		/// <summary>
		/// Percorso dove si trovano i setup dei prerequisiti
		/// </summary>
		static string prerequisitesPath = "";
		/// <summary>
		/// utente per il quale si devono registrare i namespace WCF
		/// </summary>
		static string user = "";
		/// <summary>
		/// Nome del server che ha le condivisioni di file system (puo` differire dal default in caso di clustering)
		/// </summary>
		static string fileHostName = Dns.GetHostName();
		/// <summary>
		/// Nome del server che ha le condivisioni web (puo` differire dal default in caso di clustering)
		/// </summary>
		static string webHostName = fileHostName;
		/// <summary>
		/// Porta a partire dalla quale si registrano i servizi WCF
		/// </summary>
		static int port = 10000;
		/// <summary>
		/// Porta su cui è in ascolto il server web, serve per compilare il ServerConnection.config.
		/// </summary>
		static int webServicesPort = 80;
        /// <summary>
        /// Se true, inserisce il nome istallazione nel manifest di click once per gestire la multiistanza
        /// </summary>
        static bool multipleInstance = false;
		/// <summary>
		/// se true, cancella tutti i file dalla cartella di deploy e rigenera i manifest da zero, altrimenti li aggiorna
		/// </summary>
		static bool clean = false;
		/// <summary>
		/// Azione da eseguire (deploy o clean)
		/// </summary>
		static Action action = Action.None;

        //--------------------------------------------------------------------------------
        /// <summary>
        /// Entry point del processo
        /// </summary>
        static int Main(string[] args)
        {
            Process p = GetCallingProcess();
            if (p != null)
                AttachConsole(p.Id);

            ReadDefaults();

            string cmdName = null, cmdValue = null;
            bool first = true;
            DateTime dt = DateTime.Now;

            //logfilenmame = String.Format(logfilenmame, dt.ToString("yyyymmdd-hhmmss"));
            //dt.ToString("yyyymmdd-hhmmss");

            if (!Directory.Exists(root))
                return 0;

            string logfolder = Path.Combine(root, "Logs");

            if (!System.IO.Directory.Exists(logfolder))
            { System.IO.Directory.CreateDirectory(logfolder); }

            using (FileStream fs = new FileStream(Path.Combine(logfolder, String.Format(logfilenmame, dt.ToString("yyyyMMdd-hhmmss"))), FileMode.OpenOrCreate))
            using (StreamWriter sw = new StreamWriter(fs))
            {
                Console.SetOut(sw);

                foreach (string arg in args)
                {
                    //il primo argomento e` sempre l'azione da compiere
                    if (first)
                    {
                        first = false;
                        action = ParseAction(arg);
                        if (action == Action.None)
                        {
                            Console.Out.WriteLine("Invalid action: '{0}'", action);
                            return UsageDemoError();
                        }
                        continue;
                    }

                    if (arg.StartsWith("/")) //nome di comando
                    {
                        cmdName = arg;
                        continue;
                    }
                    else //valore di comando
                    {
                        //priva di avere un command value devo aver avuto un command name...
                        if (cmdName == null)
                        {
                            Console.Out.WriteLine("Invalid command: '{0}'", arg);
                            return UsageDemoError();
                        }
                        cmdValue = arg;

                        if (!AssignCommandName(cmdName, cmdValue))
                        {
                            Console.Out.WriteLine("Invalid command: '{0}'", cmdName);
                            return UsageDemoError();
                        }
                        //riazzero la coppia nome-comando
                        cmdName = cmdValue = null;
                    }

                }

                if (action == Action.UI)
                {
                    //se ui chiamo la form
                    ClickOnceDeployerUIForm uif = new ClickOnceDeployerUIForm();
                    //inzializzo le proprietà con i valori di default
                    uif.Root = root;
                    uif.Version = debugRelease;
                    uif.InstallationName = installationName;
                    uif.InstallationPath = installationPath;
                    uif.UICulture = uiCulture;
                    uif.WebServicesPort = webServicesPort.ToString();
                    uif.User = Generator.GetAspNetUser();
                    if (DialogResult.OK != uif.ShowDialog())
                        return 0;

                    //ritorno i valori modificati (e modificabili) dalla form 
                    action = uif.Action;
                    debugRelease = uif.Version;
                    user = uif.User;
                    root = uif.RootBack;
                }
                try
                {
                    switch (action)
                    {
                        case Action.Clean:
                            {
                                using (Generator gen = new Generator(root))
                                    gen.Clean();

                                break;
                            }
                        case Action.Deploy:
                            {
                                using (Generator gen = new Generator(root))
                                {
                                    if (!gen.PerformDeployAction(installationName, fileHostName, webHostName, uiCulture, appCulture, webServicesPort, debugRelease, prerequisitesPath, multipleInstance, clean))
                                        return -1;
                                }
                                break;
                            }
                        case Action.UpdateDeployment:
                            {
                                using (Generator gen = new Generator(root))
                                {
                                    if (!gen.PerformUpdateDeploymentAction(installationName, fileHostName, webHostName, uiCulture, appCulture, webServicesPort, debugRelease, prerequisitesPath, multipleInstance))
                                        return -1;
                                }
                                break;
                            }
                        case Action.RegisterWcf:
                            {
                                WCFServiceRegister.RegisterIstallation(root, port, user, new Logger(Console.Out));
                                break;
                            }
                        case Action.UnregisterWcf:
                            {
                                WCFServiceRegister.UnregisterInstallation(root, port, new Logger(Console.Out));
                                break;
                            }
                        case Action.ViewWcf:
                            {
                                NamespaceMngForm f = new NamespaceMngForm();
                                f.ShowDialog();
                                break;
                            }
                        case Action.UI:
                            {
                                ClickOnceDeployerUIForm uif = new ClickOnceDeployerUIForm();
                                uif.ShowDialog();
                                break;
                            }
                    }
                }
                catch (Exception ex)
                {
                    Console.Out.WriteLine(ex.ToString());
                }
                finally
                {
                    CreateM4ClientConfigFile();
                }
            }
            return 0;
        }

        //----------------------------------------------------------------------------
        private static string WebFrameworkRootUrl
        {
            get
            {
                return string.Format("http://{0}:{1}/{2}", Dns.GetHostName(), InstallationData.ServerConnectionInfo.WebServicesPort, BasePathFinder.BasePathFinderInstance.Installation);
            }
        }


        //--------------------------------------------------------------------------------
        private static void CreateM4ClientConfigFile()
        {
            /*
             {
              "baseUrl": "http://localhost/Development/M4Server",
              "wsBaseUrl": "ws://localhost/Development/M4Server",
              "isDesktop": true
            }
             */
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("{");


            //qui non uso WebFrameworkRootUrl di pathfinder, perchè cablerebbe localhost nell'indirizzo, nel mio caso il file di configurazione verrebbe letto anche da un
            //client clickonce su altre macchine, e quindi localhost non andrebbe bene
            //string pathHttp = new Uri(Path.Combine(PathFinder.BasePathFinderInstance.WebFrameworkRootUrl, NameSolverStrings.M4Server)).ToString();
            string pathHttp = new Uri(Path.Combine(WebFrameworkRootUrl, NameSolverStrings.M4Server)).ToString();
            string pathWs = new Uri(pathHttp.ReplaceNoCase("http://", "ws://")).ToString();

            sb.AppendLine(string.Format("\"baseUrl\": \"{0}\",", pathHttp));
            sb.AppendLine(string.Format("\"wsBaseUrl\": \"{0}\"", pathWs));
            //sb.AppendLine("\"isDesktop\": true");
            sb.AppendLine("}");

            string m4ClientPath = BasePathFinder.BasePathFinderInstance.GetM4ClientPath();
            if (!Directory.Exists(m4ClientPath))
                return;

            string assetsPath = Path.Combine(m4ClientPath, NameSolverStrings.Assets);
            if (!Directory.Exists(assetsPath))
            {
                Directory.CreateDirectory(assetsPath);
            }

            using (FileStream fs = new FileStream(Path.Combine(assetsPath, NameSolverStrings.M4ClientConfigFile), FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(sb.ToString());
                }
            }
        }

        //--------------------------------------------------------------------------------
        private static void ReadDefaults()
		{
		
			string path = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');
			//se lanciato nel suo percorso canonico, e' in grado di recuperare le informazioni che gli servono
			root = Path.GetDirectoryName(path);//salgo di un colpo e becco la apps (root)
			installationName = Path.GetFileName(Path.GetDirectoryName(root)); //salgo ancora e trovo la cartella di installazione
			installationPath = Path.GetDirectoryName(root); //imposto la path dell'installazione
			action = Action.Deploy;
			try
			{
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microarea\Mago4\E51B08A3-8D02-44BE-B3BC-85144A6C7EBA"))
				{
					if (key != null)//provo a vedere se è un valore valido
						uiCulture = System.Globalization.CultureInfo.GetCultureInfo((string)key.GetValue("UICulture")).Name;
				}
			}
			catch
			{
				uiCulture = "en";
			}

			webServicesPort = Generator.ReadPortFromServerConnection(root);
		}

		

		/// <summary>
		/// Trasforma la stringa in enumerativo
		/// </summary>
		private static Action ParseAction (string arg)
		{
			if (string.Compare(arg, "Deploy", true) == 0)
				return Action.Deploy;
			if (string.Compare(arg, "UpdateDeployment", true) == 0)
				return Action.UpdateDeployment;
			if (string.Compare(arg, "Clean", true) == 0)
				return Action.Clean;
			if (string.Compare(arg, "RegisterWcf", true) == 0)
				return Action.RegisterWcf;
			if (string.Compare(arg, "UnregisterWcf", true) == 0)
				return Action.UnregisterWcf;
			if (string.Compare(arg, "ViewWcf", true) == 0)
				return Action.ViewWcf;
			if (string.Compare(arg, "UI", true) == 0)
				return Action.UI;
			return Action.None;
		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Assegna il valore al parametro usando gli argomenti
		/// </summary>
		/// <param name="paramName">Nome del parametro</param>
		/// <param name="paramValue">Valore del parametro</param>
		private static bool AssignCommandName (string paramName, string paramValue)
		{
			switch (paramName.ToLower())
			{
				case "/root": root = paramValue; return true;
				case "/installation": installationName = paramValue; return true;
				case "/version": debugRelease = paramValue; return true;
				case "/uiculture": uiCulture = paramValue; return true;
				case "/appculture": appCulture = paramValue; return true;
				case "/prerequisites": prerequisitesPath = paramValue; return true;
				case "/port": return int.TryParse(paramValue, out port);
				case "/webservicesport": return int.TryParse(paramValue, out webServicesPort);
				case "/user": user = paramValue; return true;
				case "/filehostname": fileHostName = paramValue; return true;
				case "/webhostname": webHostName = paramValue; return true;
                case "/multiinstance": return bool.TryParse(paramValue, out multipleInstance);
				case "/clean": return bool.TryParse(paramValue, out clean);
				default: return false;
			}
		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Vissualizza il messaggio di errore con l'esempio di utilizzo
		/// </summary>
		private static int UsageDemoError ()
		{
			Console.Out.Write("Usage: ClickOnceDeployer <Deploy|Clean> \r\n/root <root path>\r\n/installation <installation Name>\r\n[/version <debug|release>]\r\n");
			Console.Out.Write("Example: ClickOnceDeployer Deploy /root \"c:\\Program Files\\Microarea\\MagoNet\\Apps\" /installation MagoNet /version release");
			return -1;
		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Aggancia la console di output del processo corrente a quella del processo indicato
		/// </summary>
		/// <param name="dwProcessId">ID del processo</param>
		[DllImport("kernel32.dll", SetLastError = true)]
		extern static bool AttachConsole (int dwProcessId);

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Ritorna il processo chiamante (se esiste)
		/// </summary>
		private static Process GetCallingProcess ()
		{
			try
			{
				Process p = Process.GetCurrentProcess();
				int n = 1;
				string name = p.ProcessName;
				while (true)
				{
					PerformanceCounter pc = new PerformanceCounter("Process", "ID Process", name);

					if (pc.RawValue == p.Id)
					{
						PerformanceCounter pc1 = new PerformanceCounter("Process", "Creating Process ID", name);
						return Process.GetProcessById((int)pc1.RawValue);
					}
					name = string.Format("{0}#{1}", p.ProcessName, n++);
				}
			}
			catch
			{
				return null;
			}
		}
	}

	//================================================================================
	class Logger : ILogWriter
	{
		TextWriter writer;
		//--------------------------------------------------------------------------------
		public Logger (TextWriter writer)
		{
			this.writer = writer;
		}

		#region ILogWriter Members

		//--------------------------------------------------------------------------------
		public void WriteLine (string message, params object[] args)
		{
			writer.WriteLine(message, args);
		}

		#endregion
	}
}

