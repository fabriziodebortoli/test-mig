using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Interfaces;
using System.IO;

namespace Microarea.TaskBuilderNet.TbHermesBL
{
	// rielaborato da classe statica omonima usata in TbSender, dove lo static la rendeva intestabile.
	// TODO sposta in classe base in Microarea.TaskBuilderNet.Core, e deriva (o usa parametri) specializzando (TbSender e TbHermes)
	public class LoginManagerConnector
	{
		LoginManager lm = new LoginManager();
		Dictionary<string, TbSenderDatabaseInfo> companiesInfo;
		private class CmpInfoLocker { }
		private readonly string callGuid;// = "{2695241D-D54A-423C-8A09-ECF83BD36EED}"; // TbHermes

        public const string DEFAULT_GUID="{2695241D-D54A-423C-8A09-ECF83BD36EED}";
 
        //-------------------------------------------------------------------------------
        public LoginManagerConnector()
        {
            this.callGuid = DEFAULT_GUID;
        }
        
        //-------------------------------------------------------------------------------
		public LoginManagerConnector(string callGuid)
		{
			this.callGuid = callGuid;
		}

		//-------------------------------------------------------------------------------
		private Dictionary<string, TbSenderDatabaseInfo> CompaniesInfo
		{
			get
			{
				lock (typeof(CmpInfoLocker))
				{
					if (companiesInfo == null)
					{
						List<TbSenderDatabaseInfo> list = lm.GetCompanyDatabasesInfo(callGuid);
						Dictionary<string, TbSenderDatabaseInfo> dic = new Dictionary<string, TbSenderDatabaseInfo>(StringComparer.InvariantCultureIgnoreCase);
						foreach (TbSenderDatabaseInfo dbInfo in list)
						{
							// TODO controlla .IsEnabled se implementato, e specializza secondo callGuid
							if (false == dbInfo.IsEnabled) // controlla sia un db sottoscritto al servizio PostaLite/Hermes??? sembra non essere mai valorizzato...
								continue;
							if (dic.ContainsKey(dbInfo.Company))
								continue;
							dic[dbInfo.Company] = dbInfo;
						}
						companiesInfo = dic;
					}
					return companiesInfo;
				}
			}
		}

		//-------------------------------------------------------------------------------
		public List<string> GetSubscribedCompaniesNames()
		{
			// TODO fai spirare il dizionario dopo un po'
			return CompaniesInfo.Keys.OrderBy(x => x).ToList();
		}

		//-------------------------------------------------------------------------------
		public List<TbSenderDatabaseInfo> GetSubscribedCompanies()
		{
			// TODO fai spirare il dizionario dopo un po'
			return CompaniesInfo.Values.OrderBy(x => x.Company).ToList();
		}

		//-------------------------------------------------------------------------------
		internal void Restart()
		{
			lock (typeof(CmpInfoLocker))
				companiesInfo = null; // lo rilegge non appena ne ha bisogno
			//companiesInfo = lm.GetCompanyDatabasesInfo(callGuid);
		}

		//-------------------------------------------------------------------------------
		internal TbSenderDatabaseInfo GetCompaniesInfo(string company)
		{
			if (company == null)
				return CompaniesInfo.Values.FirstOrDefault();

			TbSenderDatabaseInfo dbInfo;
			CompaniesInfo.TryGetValue(company, out dbInfo);
			return dbInfo;
		}

		//-------------------------------------------------------------------------------
        //internal void SendBalloon(string message)
        //{
        //    string bodyMessage = message;
        //    List<string> recipients = null;
        //    lm.SendBalloon(callGuid, bodyMessage, Interfaces.MessageType.PostaLite, recipients);
        //}

        //fabio---------------------------------------------------------------------------
        internal void SendBalloonToUser(string message, string user)
		{
            //invio messaggio immediate. Addirittura questo codice ti fa vedere come mandare un link ad un dato specifico del data entry  cliente
            //  string bodyMessage = "<a href=\"TB://Document.ERP.CustomersSuppliers.Documents.Customers?typecustsupp:3211264;custsupp:0001;\">Customer 0001 </a>"/* +
            //  "<br><a href=\"TB://Document.ERP.CustomersSuppliers.Documents.Customers?" + s + "\">Customer</a>"*/;
            string bodyMessage = "<a href=\"TB://Document.OFM.Mail.Documents.MailManagement\">" + message  + "</a>";

            List<string> recipients = new List<string>();
            recipients.Add(user);

            //lm.SendBalloon(lm.AuthenticationToken, bodyMessage, MessageType.Default, recipients);

            lm.AdvancedSendBalloon(
                                lm.AuthenticationToken,         // lavora direttamente con il token unico di tbhermes
                                bodyMessage,
                                DateTime.Now.AddMinutes(15), //AddYears(1),
                                MessageType.Default, // MessageType.Default,
                                new string[] { user },          //viene mandato l'utente a cui va spedito il messaggio.
                                MessageSensation.ResultGreen,
                                false,
                                true,                           //immediato? se False ...
                                500,
                                "OFM_MAIL");                           // ... attendi millisecondi di polling
        }

		////-------------------------------------------------------------------------------
		//internal string GetUserInfoID()
		//{
		//    return lm.GetUserInfoID();
		//}

        //-------------------------------------------------------------------------------
        private List<DmsDatabaseInfo> GetDMSDatabasesInfo(string authenticationToken)
        {
            if (String.IsNullOrWhiteSpace(authenticationToken))
                throw new LoginManagerException(LoginManagerError.NotLogged);

            List<DmsDatabaseInfo> dmsArray = lm.GetDMSDatabasesInfo(authenticationToken);
            return dmsArray;
        }

        //-------------------------------------------------------------------------------
        const string authenticationToken = "{2E8164FA-7A8B-4352-B0DC-479984070507}"; // copiato da EASyncEngine, vedere se specializzare
        internal DmsDatabaseInfo GetDMSDatabasesInfo4Company(int companyId)//string company)
        {
            //if (company == null)
            //    return null;
            List<DmsDatabaseInfo> dmsArray = GetDMSDatabasesInfo(authenticationToken);
            if (dmsArray == null)
                return null;
            return dmsArray.Find(x => x.CompanyId == companyId);
        }

		//---------------------------------------------------------------------
		public TbLoaderClientInterface GetTbLoaderClientInterface(string company, SqlConnectionStringBuilder cb)
		{
			this.createTbAuthToken = null; // non si sa mai, un'eccezione terrebbe un risultato vecchio
			int res = lm.Login(cb.UserID, cb.Password, false, company, "TbHermesAgent", false);
			if (res != 0)
				throw new Exception("Login to Login Manager failed"); // TODO

			//string tbServicesUrl = Microarea.TaskBuilderNet.Core.NameSolver.BasePathFinder.BasePathFinderInstance.TbServicesUrl;
			ITbServices tbService = new TbServices();
			this.createTbAuthToken = lm.AuthenticationToken; // quello cablato prima non va bene, deve essere un utente esistente, e per fortuna la console lo crea
			ITbLoaderClient tb = tbService.CreateTB(
				BasePathFinder.BasePathFinderInstance,
				createTbAuthToken,
				company,
				null,
				DateTime.Today);
			return tb as TbLoaderClientInterface;
		}

		string createTbAuthToken;
		public void LogOff()
		{
			// tbservices.closetb(token)
			// va fatto prima del logoff, serve per rilasciare l'area di memoria usata da tb.
			// non farlo è ok, perché al nuovo login se si è invocato un logoff le area di memoria inutilizzate sono comunque rilasciate,
			// è solo per farlo il prima possibile.
			if (this.createTbAuthToken != null)
			{
				TbServices tbService = new TbServices();
				tbService.CloseTB(this.createTbAuthToken);
			}

			// chiamate login() a cal infinite non riciclano, consumano nuove login... nulla di male visto sono infinite, se
			// non fosse che ogni dieci si crea un nuovo processo tb! occorre quindi dire a login manager fare dispose delle risorse
			// invocando il metodo logoff con l'authentication token utilizzato per la login
			if (lm != null && this.createTbAuthToken != null)
				lm.LogOff(this.createTbAuthToken);
		}

        //---------------------------------------------------------------------
        //-salvataggio di informazioni necessarie a sendtovalstu, verranno criptate e salvate in un file 
        //---------------------------------------------------------------------
        public void GetMyData()
        {
            // le eccezioni sono catturate dal chiamante
		    TbSenderDatabaseInfo aCmp = this.GetSubscribedCompanies().FirstOrDefault();  // mi serve una qualunque company
            string sCustomPath = BasePathFinder.BasePathFinderInstance.GetCustomPath();  // il path e' sicuramente locale
            string myConfigFile = Path.Combine(sCustomPath, "sendtovalstu.cfg");
            Encryptor en = new Encryptor("V4#f#hktFCfXmr6j"); // chiave privata casuale unica per tutto il file
            string _sep_ = "==f";
            string aLine;

            StreamWriter sw = new StreamWriter(myConfigFile);

            // connection string: "Initial Catalog='';User ID='';Password='';"   manca il Data Source (variabile)
            aLine = en.Encrypt(aCmp.ServerName);
            aLine += _sep_ + Convert.ToBase64String(en.IV);
            sw.WriteLine(aLine);
            aLine = en.Encrypt(aCmp.Username);
            aLine += _sep_ + Convert.ToBase64String(en.IV);
            sw.WriteLine(aLine);
            aLine = en.Encrypt(aCmp.Password);
            aLine += _sep_ + Convert.ToBase64String(en.IV);
            sw.WriteLine(aLine);


            sw.Close();

        }
    }
}
