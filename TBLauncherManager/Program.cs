using System;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using System.Reflection;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using System.Net;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.Generic;


namespace Microarea.Tools.TBLauncherManager
{
    /*tb o imago protocolli accettati nelle seguenti forme (imago solo con itoken)
    tb://Document.ERP.Contacts.Documents.Contacts
    tb://Document.ERP.Contacts.Documents.Contacts/?contact:F
    tb://Usr-manzoniila1/Development1.0.hf/Document.ERP.Contacts.Documents.Contacts?contact:F 
    tb://Document.ERP.CustomersSuppliers.Documents.Customers?typecustsupp:3211264%3bCustSupp:0001&IToken=AauyeldzsqjwfmwqnEyi$ANOurhYYUOcgiAp!czxS0HvF01CYnH!I!lrohqiyF9NHgQE1KWAVdoxUfd65Foj!n7jwRKYa5KUJQYEFllw!Sg3uV4NK9ayNES0uATAm3C5Wvq8YQt2ajWCo8h!ELQ4GmLP5g2NBvV01by6tt9ghjPG3hpvEDPV1yx2NXRmDu2kys6xI9m53KiX7UFev
    */

    //--------------------------------------------------------------------------------
    static class Program
    {
        private static string protocol = null;
        private static string server = null;

        private static string instance = null;
        private static string nameSpace = null;
        private static string parameters = null;
        private static string itoken = null;

        private static string tbparam = null;
        static string tbparam2;
        public static Uri clickOnceUrl = null;
        private static bool oldMode = false;
        private static bool serverPredefinition = false;



        private const string MyName = "TBLauncherManager";
        private static string logfile = MyName + ".log";



        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        //--------------------------------------------------------------------------------
        static void Main(string[] args)
        {
            try
            {
                //file di log
                string newTemp = BasePathFinder.BasePathFinderInstance.GetAppDataPath(true);
                logfile = Path.Combine(newTemp, MyName + ".log");
                LogToFile(MyName + "TBLauncherManager Avviato", true);

                //utility : in caso venga lanciato senza paramentri, cosa che non dovrebbe capitare se non cliccando sull'eseguibile, si registrano i protocolli tb e imago
                if (args == null || args.Length == 0)
                {
                    LogToFile("No parameter received");
                    RegisterProtocols();
                    return;
                }

                //val è il tb:  o il Imago: completo
                string val = args[0];

                if (String.IsNullOrEmpty(val))
                {
                    LogToFile("empty parameter");
                    return;
                }
                LogToFile("parametro ricevuto: " + val, true);

                Parse(val);
                if (tbparam.IsNullOrWhiteSpace())
                {
                    LogToFile("Impossibile continuare.");
                    return;
                }
                FindProcessToStart();
            }
            catch (Exception exc) { LogToFile("[" + DateTime.Now.ToString() + "]" + " Exception!" + exc.ToString()); }
        }

        //--------------------------------------------------------------------------------
        private static void FindProcessToStart()
        {
            try
            {
                LogToFile("Parsed: " + tbparam, true);
                List<Process> currentp = new List<Process> { };        //processo che gira nella stessa cartella di questo eseguibile in questo momento
                List<Process> correspondingps = new List<Process> { };  //processI che corrispondono ai dati passati nel link come server e istanza

                foreach (Process process in GetRunningProcesses())
                {
                    string processPath = null;
                    string pname = null;
                    try
                    {
                        processPath = process.MainModule.FileName;
                        pname = process.ProcessName;
                    }
                    catch (Exception exc)
                    {
                        LogToFile(" Eccezione reperendo il path del Processo : " + pname + " " + exc.Message);

                        continue;
                    }
                    processPath = processPath.Substring(0, processPath.LastIndexOf('\\'));
                    string currentPath = Process.GetCurrentProcess().MainModule.FileName;
                    currentPath = currentPath.Substring(0, currentPath.LastIndexOf('\\'));

                    if (currentPath == processPath && process.Responding)
                    {
                        LogToFile("Processo trovato : " + process.MainModule.FileName, true);

                        currentp.Add(process);//processo della stessa cartella di questo eseguibile
                        continue;//continueo col prossimo processo questo comunque lo abbiamo preso.
                    }
                    //vuol dire che  mi sono stati passati i valori di server e istanza quindi è cerco l installation info relativo all'installazione cercata.
                    if (serverPredefinition)
                    {
                        string iif = Path.Combine(processPath, "InstallationInfo.xml");//cerco l'installation info file che è presente sul client e che contiene le informazioni relative al server e istanza
                        if (!File.Exists(iif))
                            continue;

                        //carico l installation info
                        XmlDocument doc = new XmlDocument();
                        doc.Load(iif);
                        //se i dati corrispondono è lui il processo richiesto dalle informazioni presenti nel link.
                        XmlNode serverN = doc.SelectSingleNode("//FileSystemServer");
                        if (!serverN.InnerText.CompareNoCase(server)) continue;

                        XmlNode instanceN = doc.SelectSingleNode("//InstallationName");
                        if (instanceN.InnerText.CompareNoCase(instance))
                        {
                            LogToFile("Processo corrispondente trovato : " + processPath, true);
                            correspondingps.Add(process);
                            continue; //caso piu`maghi aperti sulla stessa istanza ma con credenziali diverse che io in questo punto non posso conoscere.
                        }

                    }
                    if (oldMode)//vecchia maniera, senza indicazione di server,protocollo tb, broadcasta. per compatibilità. solo magonet , solo tb
                    {
                        LogToFile("oldMode: ", true);
                        correspondingps.Add(process);
                        continue;
                    }
                }
                //ho trovato dei processi corrispondeti o correnti?
                if (currentp != null && currentp.Count > 0 && (correspondingps.Count == 0 || oldMode))  //se ho trovato solo il orrente lo aggiungo alla lista su cui ciclo, se sono in oldmode lo aggiungo anche se la lista dei crresponnding è popolata, fa broadcast come sempre in 3x
                    correspondingps.AddRange(currentp);
                ////Trovata l'applicazione runnante corretta, mando il messaggio coi param , potrebbero esserci più maghi corretti ma aperti con credeniali diverse, menu manager si occuperà di lanciare il link solo a colui che corrisponde, gli altri daranno errore.
                if (correspondingps.Count > 0)
                {
                    foreach (Process p in correspondingps)
                        SendMessageToMago(p);
                }


                //se  a questo punto non esiste il processo già avviato, dobbiamo avviarlo noi, trovando quello corretto, e passargli i parametri in una process.start
                else
                    FindAndLaunchProcess();
            }
            catch (Exception exc) { LogToFile("[" + DateTime.Now.ToString() + "]" + " Exception!" + exc.ToString()); }
        }

        //---------------------------------------------------------------------
        private static List<Process> GetRunningProcesses()
        {
            List<Process> processes = new List<Process> { };
            AddProcesses(processes, "TBAppManager");

            //se sono in  modalità oldmode cerco anche i mago.net csì è retrocompatibile al 100% interscambiabile tra le varie versioni
            if (oldMode)
                AddProcesses(processes, "Mago.net");
            return processes;
        }

        //---------------------------------------------------------------------
        private static void AddProcesses(List<Process> processes, string procName)
        {
            if (processes == null || procName.IsNullOrWhiteSpace()) return;
            try
            {
                Process[] ppOld = Process.GetProcessesByName(procName);
                if (ppOld.Length > 0)
                    processes.AddRange(ppOld);
            }
            catch (Exception exc)
            {
                LogToFile("Eccezione ricercando i processi " + procName + " attivi: " + exc.Message);
            }
        }
        static bool foundp = false;
        static int passato = 0;
        //---------------------------------------------------------------------
        private static void FindAndLaunchProcess()
        {
            passato++;
            if (passato > 1) return;
            if (InstallationData.IsClickOnceInstallation) //sto runnando dalla standard (Web application) oppure Apps, non uso share di rete
            {
                LogToFile("CLIENT!", true);
                string appRefFile = null;
                try
                {
                    string applicationReferenceContent = null;
                    appRefFile = DownloadManifest(out applicationReferenceContent);
                    if (!applicationReferenceContent.IsNullOrWhiteSpace())
                    {
                        using (var stream = File.OpenWrite(appRefFile))
                        {
                            using (var sw = new StreamWriter(stream, Encoding.Unicode))
                            {
                                sw.WriteLine(applicationReferenceContent);
                            }
                        }
                        if (!appRefFile.IsNullOrWhiteSpace() && File.Exists(appRefFile))
                        {
                            LogToFile("----->" + appRefFile);
                            foundp = true;
                            Process.Start(appRefFile, tbparam2);
                            return;
                        }
                    }
                }
                catch (Exception exc) { LogToFile("FindAndLaunchProcess: Error launching client application " + appRefFile + Environment.NewLine + exc.ToString()); }

            }
            if (!foundp)
            {
                //questo apre se esiste l exe di mago dove sta girando questo exe, ma non sono sicura che sia quello che deve fare,ma è l'ultima spiaggia
                //nel senso che se io nel link ho indicato un server e istanza diverso dal corrente questo non funzionerebbe, ma come faccio ?
                string filePath = Path.Combine(Functions.GetExecutingAssemblyFolderPath(), "TbAppManager.exe");

                if (File.Exists(filePath))//caso server standalone senza clientsetup
                {
                    try
                    {
                        LogToFile("FindAndLaunchProcess: " + filePath, true);
                        foundp = true;
                        Process.Start(filePath, tbparam2);
                    }
                    catch (Exception exc) { LogToFile("FindAndLaunchProcess: Error launching server application  " + filePath + Environment.NewLine + exc.ToString()); }

                }

            }
            if (!foundp)
            {
                //se sono qui non ho trovato nulla ripasso dall'inizio senza indicazione di server e istanza
                server = string.Empty;
                instance = string.Empty;
                serverPredefinition = false;
                FindProcessToStart();
            }
        }

        //---------------------------------------------------------------------
        private static void SendMessageToMago(Process p)
        {
            if (p == null || !p.Responding || tbparam.IsNullOrWhiteSpace()) return;

            LogToFile("SendMessageToMago: " + tbparam, true);

            IntPtr localHGlobal = IntPtr.Zero;
            IntPtr pRemoteBuffer = IntPtr.Zero;
            byte[] bytes = null;
            try
            {
                bytes = Encoding.UTF8.GetBytes(tbparam);
                localHGlobal = Marshal.AllocHGlobal(bytes.Length);

                for (int i = 0; i < bytes.Length; i++)
                    Marshal.WriteByte(localHGlobal, i, bytes[i]);

                IntPtr hwnd = p.MainWindowHandle;
                ShowWindow(hwnd, ShowWindowCommands.Maximize);
                SetForegroundWindow(hwnd);

                pRemoteBuffer = VirtualAllocEx(p.Handle, IntPtr.Zero, bytes.Length, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
                int tot = 0;

                if (!WriteProcessMemory(p.Handle, pRemoteBuffer, localHGlobal, bytes.Length, ref tot))
                {
                    LogToFile("WriteProcessMemory failed!");
                    return;
                }
                SendMessage(hwnd, UM_MAGO_LINKER, (IntPtr)bytes.Length, pRemoteBuffer);
            }

            catch (Exception exc)
            {
                LogToFile("Eccezione eseguendo la SENDMESSAGE a " + p.ProcessName + ". " + exc.ToString());
            }

            finally//libero la memoria, uso send e non post così sono in grado di farlo.
            {
                Marshal.FreeHGlobal(localHGlobal);
                if (pRemoteBuffer != IntPtr.Zero && bytes != null)
                    VirtualFreeEx(p.Handle, pRemoteBuffer, 0, MEM_RELEASE);
            }
        }

        //---------------------------------------------------------------------
        private static void Parse(string val)
        {
            tbparam = null;
            if (val.IsNullOrWhiteSpace()) return;
            if (val.IndexOf("://") < 0)
            {
                LogToFile("Link non valido: " + val);
                return;
            }
            protocol = val.Substring(0, val.IndexOf("://"));
            string work = val.Remove(0, protocol.Length + 3);//ottengo tutto tranne TB:\\ IMAGO:\\

            string[] works = work.Split(new char[] { '?' }, StringSplitOptions.RemoveEmptyEntries);
            if (works.Length > 0)
            {
                //analizzo la prima parte
                string[] works2 = works[0].Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (works2.Length == 1)
                {
                    nameSpace = works2[0];
                    serverPredefinition = false;

                }
                else if (works2.Length == 3)
                {
                    server = works2[0];
                    instance = works2[1];
                    nameSpace = works2[2];

                    //se qualcuna di queste info è vuota o nulla rilacio errore:
                    if (VerifyInfoCompleteness())
                        serverPredefinition = true;
                    else
                    {
                        LogToFile("Parametri non validi, le informazioni relative a server o istanza sono vuote: " + val);
                        return;
                    }
                }
                else
                {
                    LogToFile("Parametri non validi, le informazioni relative a server o istanza non sono formattate correttamente: " + val);
                    return;
                }
            }
            //se c'e'analizzo la seconda parte
            if (works.Length == 2)
            {
                //analizzo la seconda parte dopo il ?
                if (works[1].StartsWith("IToken=", StringComparison.InvariantCultureIgnoreCase))//nessun parametro - solo token
                {
                    itoken = works[1].Remove(0, 7);
                }
                else
                {
                    string[] works2 = works[1].Split(new string[] { "&IToken=" }, StringSplitOptions.RemoveEmptyEntries);
                    if (works2.Length == 1)
                    {
                        parameters = works2[0];
                    }
                    else if (works2.Length == 2)
                    {
                        parameters = works2[0];
                        itoken = works2[1];
                    }
                }
            }

            if (protocol.CompareNoCase("imago") && itoken.IsNullOrWhiteSpace())
            {
                LogToFile("protocollo IMAGO non valido senza IToken: " + val);
                return;
            }

            if (server.IsNullOrWhiteSpace())
            {
                oldMode = protocol.CompareNoCase("TB") && itoken == null;//se non ho parametri di server e istanza ed itoken ed ho protocollo tb potrebbe provenire da un magonet.
                server = BasePathFinder.BasePathFinderInstance.RemoteWebServer;
                instance = BasePathFinder.BasePathFinderInstance.Installation;
            }
            else //o tutti o nessuno, ma dovrei aver già coperto questo caso nei controlli precedenti
                if (instance.IsNullOrWhiteSpace())
            {
                LogToFile("Parametri non validi, mancano informazioni relative a server, istanza: " + val);
                return;
            }
            //link dell'applicazione da chiamare per downloadare i manifest e passare dal processo di clickonce
            clickOnceUrl = new Uri(String.Format("http://{0}:{1}/{2}/Apps/TbAppManager.application", server, InstallationData.ServerConnectionInfo.WebServicesPort, instance));
            string parametersQuery = parameters.IsNullOrWhiteSpace() ? String.Empty : ("?" + parameters);//se ci sono chiavi mette ?chiavi altrimenti stringa vuota
            string oldparam = protocol + "://" + nameSpace + parametersQuery;//creal il link finale con protocollo://namespace e le chiavi ,se presenti
            string iparamP = "Iparam=" + oldparam;//iparam creato esiste sicuramente almeno il namespace
            //se il token imago è presente si attacca al link
            string itokenP = itoken.IsNullOrWhiteSpace() ? String.Empty : "&IToken=" + itoken;

            if (oldMode) tbparam = oldparam;
            else tbparam = iparamP + itokenP;
            tbparam2 = "\"" + tbparam + "\"";
        }

        //---------------------------------------------------------------------
        private static bool VerifyInfoCompleteness()
        {
            return
            !server.IsNullOrWhiteSpace() &&
            !instance.IsNullOrWhiteSpace() &&
            !nameSpace.IsNullOrWhiteSpace();
        }

        //---------------------------------------------------------------------
        private static string DownloadManifest(out string applicationReferenceContent)
        {
            LogToFile("DownloadManifest", true);
            applicationReferenceContent = null;
            try
            {
                // Download the application manifest.
                byte[] applicationManifest = null;
                using (var wc = new WebClient())
                {
                    applicationManifest = wc.DownloadData(clickOnceUrl);
                }

                // Build the .appref-ms contents.

                using (MemoryStream stream = new MemoryStream(applicationManifest))
                {
                    XNamespace asmv1 = "urn:schemas-microsoft-com:asm.v1";
                    XNamespace asmv2 = "urn:schemas-microsoft-com:asm.v2";

                    XDocument doc = XDocument.Load(stream);

                    XElement assembly = doc.Element(asmv1 + "assembly");
                    XElement identity = assembly.Element(asmv1 + "assemblyIdentity");
                    XElement deployment = assembly.Element(asmv2 + "deployment");
                    XElement provider = deployment.Element(asmv2 + "deploymentProvider");

                    XAttribute codebase = provider.Attribute("codebase");
                    XAttribute name = identity.Attribute("name");
                    XAttribute language = identity.Attribute("language");
                    XAttribute publicKeyToken = identity.Attribute("publicKeyToken");
                    XAttribute architecture = identity.Attribute("processorArchitecture");

                    applicationReferenceContent = String.Format(
                      "{0}#{1}, Culture={2}, PublicKeyToken={3}, processorArchitecture={4}",
                      codebase.Value,
                      name.Value,
                      language.Value,
                      publicKeyToken.Value,
                      architecture.Value
                      );
                }

            }
            catch (Exception exc) { LogToFile("Exception Downloading Manifest! " + exc.ToString()); }
            return Path.ChangeExtension(Path.GetTempFileName(), "appref-ms");
        }

        //---------------------------------------------------------------------
        private static void RegisterProtocols()
        {
            string path = TBLauncherManagerPath();
            WriteOnRegistry(path, "TB");
            WriteOnRegistry(path, "IMago");
        }

        //-----------------------------------------------------------------------
        private static void WriteOnRegistry(string path, string protocolName)
        {
            //non verifico l'esistenza perchè la cartella locale dove il programma è messo potrebbe variare in seguito ad aggionamenti.
            RegistryKey subKey1 = null;
            RegistryKey subKey11 = null;
            try
            {
                if (String.IsNullOrWhiteSpace(path)) return;

                subKey1 = Registry.CurrentUser.CreateSubKey(@"Software\Classes\" + protocolName);
                if (subKey1 == null) return;
                subKey1.SetValue("URL Protocol", "");
                subKey11 = subKey1.CreateSubKey(@"Shell\Open\Command");
                if (subKey11 == null) return;
                //null come nome  = (Default)
                subKey11.SetValue(null, path);
                LogToFile("Written registry keys in HKEY_CURRENT_USER\\Software\\Classes\\" + protocolName);
            }
            catch (Exception exc)
            {
                LogToFile("Error writing registry key in: HKEY_CURRENT_USER\\Software\\Classes\\" + protocolName + ": " + exc.ToString());
            }
            finally
            {
                //chiudo le chiavi
                if (subKey11 != null) subKey11.Close();
                if (subKey1 != null) subKey1.Close();
            }

            //runasadministrator
            RegistryKey k = null;
            try
            {
                string exepath = GetExePath();
                if (exepath == null) return;
                k = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers");
                if (k == null) return;

                k.SetValue(exepath, "~ RUNASADMIN");
                LogToFile("Written registry keys in HKEY_CURRENT_USER\\Software\\Microsoft\\Windows NT\\CurrentVersion\\AppCompatFlags\\Layers");
            }
            catch (Exception exc)
            {
                LogToFile("Error writing registry key in: HKEY_CURRENT_USER\\Software\\Microsoft\\Windows NT\\CurrentVersion\\AppCompatFlags\\Layers" + ": " + exc.ToString());
            }
            finally
            {
                //chiudo le chiavi
                if (k != null) k.Close();
            }
        }

        //---------------------------------------------------------------------------------
        private static void LogToFile(string message, bool debug = false)
        {// come si potrebbe compattare?
            if (debug)
            {
#if DEBUG
                try
                {
                    using (StreamWriter sr = new StreamWriter(logfile, true, System.Text.Encoding.UTF8))
                    {
                        sr.WriteLine("[" + DateTime.Now.ToString() + "] " + message);
                    }
                }
                catch {/*eccezione scrivendo il file di log...*/ }

#endif
            }
            else
            {
                try
                {
                    using (StreamWriter sr = new StreamWriter(logfile, true, System.Text.Encoding.UTF8))
                    {
                        sr.WriteLine("[" + DateTime.Now.ToString() + "] " + message);
                    }
                }
                catch {/*eccezione scrivendo il file di log...*/ }
            }
        }

        //---------------------------------------------------------------------------------
        private static string GetExePath()
        {
            string path = new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath; //converte da url a path di file system
            string exepath = Path.Combine(Path.GetDirectoryName(path), MyName + ".exe");
            if (!File.Exists(exepath)) return null;
            return exepath;
        }

        //---------------------------------------------------------------------------------
        private static string TBLauncherManagerPath()
        {
            string mask = "\"{0}\" \"%1\"";
            string exepath = GetExePath();
            if (exepath == null) return null;
            return String.Format(mask, exepath);
        }


        #region INTERFACCIA C++

        //--------------------------------------------------------------------------------
        //Serve per portare in foreground la finestra del programma lanciato
        internal enum ShowWindowCommands : int
        {
            Hide = 0,
            Normal = 1,
            ShowMinimized = 2,
            Maximize = 3,
            ShowMaximized = 3,
            ShowNoActivate = 4,
            Show = 5,
            Minimize = 6,
            ShowMinNoActive = 7,
            ShowNA = 8,
            Restore = 9,
            ShowDefault = 10,
            ForceMinimize = 11
        }

        [DllImport("user32.dll")]
        public static extern UInt32 SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShowWindow(IntPtr hWnd, ShowWindowCommands nCmdShow);

        //--------------------------------------------------------------------------------
        public const int UM_MAGO_LINKER = WM_USER + 951;
        public const int WM_USER = 0x0400;
        public const int MEM_COMMIT = 0x1000;
        public const int MEM_RESERVE = 0x2000;
        public const int PAGE_READWRITE = 0x04;
        public const int MEM_RELEASE = 0x8000;

        //--------------------------------------------------------------------------------
        [DllImport("kernel32.dll")]
        public static extern bool VirtualFreeEx(
            IntPtr hProcess,
            IntPtr lpAddress,
            int dwSize,
            int dwFreeType
            );

        //--------------------------------------------------------------------------------
        [DllImport("kernel32.dll")]
        public static extern bool WriteProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            IntPtr lpBuffer,
            int nSize,
            ref int lpNumberOfBytesWritten
            );

        //--------------------------------------------------------------------------------
        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hwnd, uint Msg, IntPtr wParam, IntPtr lParam);

        //--------------------------------------------------------------------------------
        [DllImport("kernel32.dll")]
        public static extern IntPtr VirtualAllocEx(
            IntPtr hProcess,
            IntPtr lpAddress,
            int dwSize,
            int flAllocationType,
            int flProtect);


    }
    #endregion
}
#region COMMENTI E NOTE
//-----------------------------------------------------------------------------
// 
//-----------------------------------------------------------------------------

//valutare broadcast vecchia maniera se non arrivano paraemntri oltre al ns in modo da essere retrocompatibili/
//estrapolo i dati se presenti di server porta e istanza:
// prendo la stringa e cerco ://
//
//quello prima è il protocollo, quello dopo è il resto.
//il resto lo splitto per ?
//quello dopo è la parte di parametri con chiavi quello prima è il resto
//prendo il resto e lo splitto per /
//l'ultimo è il ns e potrebbe esserci solo questo
//quello prima sono server,porta, istanza e sono opzionali

//la parte di parametri potrebbe contentere un itoken quindi splitto per ITOKEN
//la parte prima è la parte di parametri chiave e la seconda è il token di infinity che dovrebbe esserci solo sed obbligatoriamente se siamo in protocollo imago, se lo trovo con protocollo tb non mi incazzo e lo controllerò.

// a mago poi deve arrivare una roba del genere come parametri
/*
tb://Document.ERP.CustomersSuppliers.Documents.Customers?typecustsupp:3211264%3bCustSupp:0001&IToken=AauyeldzsqjwfmwqnEyi$ANOurhYYUOcgiAp!czxS0HvF01CYnH!I!lrohqiyF9NHgQE1KWAVdoxUfd65Foj!n7jwRKYa5KUJQYEFllw!Sg3uV4NK9ayNES0uATAm3C5Wvq8YQt2ajWCo8h!ELQ4GmLP5g2NBvV01by6tt9ghjPG3hpvEDPV1yx2NXRmDu2kys6xI9m53KiX7UFev
*/
//che verrà trasformato in args: 
/* Iparam=tb://Document.ERP.CustomersSuppliers.Documents.Customers 
   Itoken=AauyeldzsqjwfmwqnEyi$ANOurhYYUOcgiAp!czxS0HvF01CYnH!I!lrohqiyF9NHgQE1KWAVdoxUfd65Foj!n7jwRKYa5KUJQYEFllw!Sg3uV4NK9ayNES0uATAm3C5Wvq8YQt2ajWCo8h!ELQ4GmLP5g2NBvV01by6tt9ghjPG3hpvEDPV1yx2NXRmDu2kys6xI9m53KiX7UFev
*/
//se mago viene lanciato da processo aperto si lancia messaggio UM_tblink
//se mago viene lanciato come processo nuovo gli si passano già i parametri nella modalità in cui se li aspetta
//valuta se unificare ora non so se si può
//il controlllo che imago deve avere il token lo si fa qua? o lo si delega a mago? io lo farei qua che se arriva un imago senza token si butta via direttamente.
//quinid se scrivi un tb il link viene sempre aperto e lanci un imago viene fatto il controllo token infinity
/*
modalità per capire istanza e blabla
C:\Program Files (x86)\Microarea\Mago4\Apps\Publish\InstallationInfo.xml

<? xml version = "1.0" ?>

-< InstallationInfo xmlns:xsd = "http://www.w3.org/2001/XMLSchema" xmlns: xsi = "http://www.w3.org/2001/XMLSchema-instance" >


< FileSystemServer > usr - manzoniila1 </ FileSystemServer >


< WebServer > usr - manzoniila1 </ WebServer >


< InstallationName > Mago4 </ InstallationName >


</ InstallationInfo

trovi le app
trovi il percorso
trovi sto file installationinfo
verifichi se corrisponde coi paramentri arrivati
se si lanci se no passi
se non ne trovi
fai il downloadmanfiest come ora*/


//string[] myArgs = { };
//    int idx = val.IndexOf("IToken", StringComparison.CurrentCultureIgnoreCase);
//    if (idx > 0)
//    {
//        val = val.Replace("IToken", "Itoken");
//        string[] par = val.Split('?');

//        if (par.Length > 0)
//        {
//            string[] querypar = par[1].Split('&');
//            if (querypar.Length == 1)
//            {
//                par[0] = "Iparam=" + par[0];
//                par[1] = querypar[0];
//                myArgs = par;
//            }
//            else
//            {
//                par[0] = "Iparam=" + par[0] + "?" + querypar[0];
//                par[1] = querypar[1];
//                myArgs = par;
//            }
//        }
//    }
//    else
//        val = "Iparam=" + val;

#endregion