using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using Microarea.TaskBuilderNet.Core.DiagnosticManager;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Licence.Activation;
using Microarea.TaskBuilderNet.Licence.Licence.Forms;

namespace Microarea.TaskBuilderNet.Licence.Licence
{
	//=========================================================================
	/// <summary>
	/// Gestisce l'interfacciamento con il loginManager, l'activationObject e pathFinder per le funzionalità di attivazione
	/// </summary>
	public class ClientStub
	{
		private LoginManager loginManager;
		public Diagnostic Diagnostic = new Diagnostic("LicensedConfigurationForm");//??
		public ActivationObject ActivationObj;
		private IBasePathFinder pathFinder;

		//---------------------------------------------------------------------
		public ClientStub(IBasePathFinder pathFinder, LoginManager loginManager)
		{
			this.pathFinder = pathFinder;
			this.loginManager = loginManager;
			SetConfigurationStream(loginManager.GetConfigurationStream());
		}

		//---------------------------------------------------------------------
		public ClientStub(IBasePathFinder pathFinder)
			: this(pathFinder, new LoginManager(pathFinder.LoginManagerUrl, InstallationData.ServerConnectionInfo.WebServicesTimeOut))
		{}

		//---------------------------------------------------------------------------
		internal void SetConfigurationStream(byte[] s)
		{
            if (s == null)
                throw new ArgumentNullException("Configuration not available");
      
			BinaryFormatter f = new BinaryFormatter();
			MemoryStream ms = new MemoryStream(s);
			object o = f.Deserialize(ms);
			if (o == null)
				throw new ArgumentNullException("Configuration not deserializable");

			ActivationObj = o as ActivationObject;
			if (ActivationObj == null)
				throw new ArgumentNullException("Configuration not loadable");
		}
		//---------------------------------------------------------------------
		public bool SaveUserInfo(UserInfo ui)
		{
			XmlDocument doc = ui.GetXmlDom();
			return loginManager.SaveUserInfo(doc.OuterXml);
		}

		//---------------------------------------------------------------------
		public UserInfo GetUserInfo()
		{
			return UserInfo.GetFromXmlString(loginManager.GetUserInfo());
		}


		//---------------------------------------------------------------------
		struct ResourceItem
		{
			public string Name;
			public string Value;
		} 

		//---------------------------------------------------------------------
		public IList<ActivityCodeGroup> LoadActivityCode()
		{
			IList<ActivityCodeGroup> AC = new List<ActivityCodeGroup> { };
			try
			{	ResourceManager resources = new ResourceManager("Microarea.TaskBuilderNet.Licence.Licence.ActivityCodes", System.Reflection.Assembly.GetExecutingAssembly());

				List<ResourceItem> items = new List<ResourceItem>();
				using (ResourceReader rr = new ResourceReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.TaskBuilderNet.Licence.Licence.ActivityCodes.resources")))
				{
					IDictionaryEnumerator en = rr.GetEnumerator();
					while (en.MoveNext())
					{
						ResourceItem item = new ResourceItem();
						item.Name = (string)en.Key;
						item.Value = resources.GetString(item.Name); //(string)en.Value;--> così non traduce
						items.Add(item);
					}
				}
				IEnumerable<ResourceItem> list =
						from ResourceItem ss in items
						where ss.Name.IndexOf('_') == -1
						select ss;
				//list contiene i groups
				foreach (ResourceItem acGroup in list)
				{
					//Formato AC123
					string val = acGroup.Name.Substring(2);
					ActivityCodeGroup ACG = new ActivityCodeGroup(val, acGroup.Value);
					IList<ActivityCodeArea> areas = new List<ActivityCodeArea>{};

					//list 1 contiene le areas per ogni group
					IEnumerable<ResourceItem> list1 =
							from ResourceItem ss in items
							where ss.Name.StartsWith(acGroup.Name + "_")
							select ss;

					foreach (ResourceItem a in list1)
					{	//Formato AC123_789
						string val1 = a.Name.Substring(6);
						ACG.Areas.Add(new ActivityCodeArea(val1, resources.GetString(a.Name)));
					}
					
					AC.Add(ACG);
				}
				return AC;
			}
			catch (Exception exc)
			{
				SetWarning("Error loading Activity Codes. " + exc.Message, null, null);
				Debug.WriteLine(exc.ToString());
			}
			return null;
		}

        //---------------------------------------------------------------------
        internal void TryToReloadActivationObject()
        {
            try
            {
                loginManager.ReloadConfiguration();
            }
            catch { }
        }

        //---------------------------------------------------------------------
        public string GetPhasedOutValue(string key)
		{
            //i moduli che non sono previsti in prolite, ma che per vari motivi ( migrazione o vendita) 
            //devono poter essere installati, per vederli si deve avere questo file  o un serial number adeguato.
            string path = Path.Combine(pathFinder.GetCustomPath(), "SMReloader.xml");
            return (!string.IsNullOrEmpty(path) && File.Exists(path)) ? "OK" : null;  

        }

        //---------------------------------------------------------------------
        private bool PreElaborateResult(string res)
        {
            if (res == null)
                return false;
            ReturnValuesReader reader = null;
			ArrayList msgList = new ArrayList();
            try
            {
                reader = new ReturnValuesReader(res);
                foreach (ErrorInfo errorInfo in reader.Errors)
                {
                    int errorCode = errorInfo.Code;
                    int errorId = errorInfo.Id;
                    string details = errorInfo.Details;
                    msgList.Add(new ActivationKeyGeneratorException(errorCode, errorId, details));
                }
            }
                
            catch (Exception exc)
            {
                msgList.Add(new ActivationKeyGeneratorException(-1, -1, exc.Message));
            }
            if (msgList != null && msgList.Count > 0)
            {
                foreach (ActivationKeyGeneratorException exc in msgList)
                {
                    if (exc.Type == ActivationKeyErrors.MessageType.Error)
                        SetError(exc.GenericMessage, exc.Title, exc.SpecificMessage);
                    if (exc.Type == ActivationKeyErrors.MessageType.Info)
                        SetInformation(exc.GenericMessage, exc.Title, exc.SpecificMessage);
                    if (exc.Type == ActivationKeyErrors.MessageType.Warning)
                        SetWarning(exc.GenericMessage, exc.Title, exc.SpecificMessage);
                }

            }
            return(reader.IsAMessage);

        }

		//---------------------------------------------------------------------
		private bool ElaborateResult(string res)
		{
		if (res == null)
			{
				//StopProgress();
				return false;
			}
			ReturnValuesReader reader = null;
			ArrayList msgList = new ArrayList();
			try
			{
				reader = new ReturnValuesReader(res);

				foreach (ErrorInfo errorInfo in reader.Errors)
				{
					int errorCode = errorInfo.Code;
					int errorId = errorInfo.Id;
					string details = errorInfo.Details;
					msgList.Add(new ActivationKeyGeneratorException(errorCode, errorId, details));
				}
				if (reader.IsAMessage)
				{
					ActivationKeyGeneratorException e = new ActivationKeyGeneratorSuccessEvent(reader.BaseIndex);
					msgList.Add(e);
				}

			}
			catch (Exception exc)
			{
				msgList.Add(new ActivationKeyGeneratorException(-1, -1, exc.Message));
			}
		
			if (msgList != null && msgList.Count > 0)
			{
				foreach (ActivationKeyGeneratorException exc in msgList)
				{
					if (exc.Type == ActivationKeyErrors.MessageType.Error)
						SetError(exc.GenericMessage, exc.Title, exc.SpecificMessage);
					if (exc.Type == ActivationKeyErrors.MessageType.Info)
						SetInformation(exc.GenericMessage, exc.Title, exc.SpecificMessage);
					if (exc.Type == ActivationKeyErrors.MessageType.Warning)
						SetWarning(exc.GenericMessage, exc.Title, exc.SpecificMessage);
				}

			}
			if (reader.ShowFormForMluChargeChoice)
			{
				MluManager mm = new MluManager();
                mm.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
                mm.TopMost= true;
				mm.ShowDialog();

				if (mm.Choice == MluManager.MluManagement.Microarea || mm.Choice == MluManager.MluManagement.Reseller)
				{
					bool userChoseMluInChargeToMicroarea = (mm.Choice == MluManager.MluManagement.Microarea);
					try
					{
						loginManager.StoreMLUChoice(userChoseMluInChargeToMicroarea);
					}
					catch
					{
						SetError("StoreMLUChoice failed", null, null);
					}
				}

			}

			return (reader.IsAMessage);
			
		}

		//---------------------------------------------------------------------
		public string TestLoginManager()
		{
			try
			{

				loginManager.IsAlive();
				return null;
			}
			catch (System.Net.WebException exc)
			{
				// se l'errore fosse che non riesco a raggiungere LoginManager, magari è giù
				// (o magari non esiste nell'installazione) e non ci sono quindi vincoli a proseguire
				System.Diagnostics.Debug.WriteLine(exc.Message);
				//ServerStub.LogItem(exc, EventLogEntryType.Error); // commented out: might be normal, so don't log it!
				return ReturnValuesWriter.GetErrorString(ErrorMessage.WebExceptionPinging, exc.Message);
			}

			catch (Exception exc)
			{
				SetError("TestLM:" + exc.Message, null, null);
				if (exc.GetType() == typeof(InvalidOperationException) &&
					string.Compare(exc.Source, "System.Web.Services", true, CultureInfo.InvariantCulture) == 0)
				{
					return ReturnValuesWriter.GetErrorString(ErrorMessage.InvalidOperationExceptionPinging, exc.Message);
				}
				else
				{
					// se l'errore invece fosse di diversa natura probabilmente dovrei impedire nel dubbio
					return ReturnValuesWriter.GetErrorString(ErrorMessage.ExceptionInstantiatingProxy, exc.Message);
				}
			}
		}
		
		//---------------------------------------------------------------------
		public bool Register()
		{
			string retVal = TestLoginManager();
			if (retVal == null)
				retVal = loginManager.Ping();
			return ElaborateResult(retVal);
		}

        //---------------------------------------------------------------------
        public bool PreRegister()
        {
            string retVal = TestLoginManager();
            if (retVal == null)
                retVal = loginManager.PrePing();
            else return ElaborateResult(retVal);
            return PreElaborateResult(retVal);
             
        }

		//---------------------------------------------------------------------
		internal void SetError(string message, string extendedInfoName, string extendedInfoValue)
		{
			SetMessage(message, extendedInfoName, extendedInfoValue, DiagnosticType.Error);
		}

		//---------------------------------------------------------------------
		internal void SetWarning(string message, string extendedInfoName, string extendedInfoValue)
		{
			SetMessage(message, extendedInfoName, extendedInfoValue, DiagnosticType.Warning);
		}
		//---------------------------------------------------------------------
		internal void SetInformation(string message, string extendedInfoName, string extendedInfoValue)
		{
			SetMessage(message, extendedInfoName, extendedInfoValue, DiagnosticType.Information);
		}

		//---------------------------------------------------------------------
		internal void SetMessage(string message, string extendedInfoName, string extendedInfoValue, DiagnosticType type)
		{
			if (Diagnostic == null) return;
			if (message == null || message == String.Empty)
				return;
			ExtendedInfo infos = null;
			if (extendedInfoName != null && extendedInfoName != String.Empty && extendedInfoValue != null && extendedInfoValue != String.Empty)
				infos = new ExtendedInfo(extendedInfoName, extendedInfoValue);
			if (infos == null)
				Diagnostic.SetInformation(message);
			else
				Diagnostic.Set(type, message, infos);
		}

		//---------------------------------------------------------------------
		public ProductInfo GetProductBySerial(SerialNumberInfo serial)
		{
			ProductInfo foundProduct = null;
			string prodId = SerialNumberInfo.GetProdId(serial);
			string prodEd = SerialNumberInfo.GetEditionId(serial);
			//string country = SerialNumberInfo.GetCountry(StbTheSerial.Serial);

			foreach (ProductInfo p in ActivationObj.Products)
                if (
                    (String.Compare(p.ProductId, prodId, true, CultureInfo.InvariantCulture) == 0 || String.Compare(p.GetAcceptDemo(), prodId, true, CultureInfo.InvariantCulture) == 0)
                    &&
                    String.Compare(p.EditionId, prodEd, true, CultureInfo.InvariantCulture) == 0)
                {
                    foundProduct = p;
                    break;
                }
			////nessun prodotto adeguato al serial inserito
			if (foundProduct == null)
			{
				SetError(LicenceStrings.NoProductMatchingWithSerial, null, null);

				return null;
			}

			//cancello se ci sono già dei licensed salvati di altro prodotto master....
			string masterp = ActivationObj.GetMasterSolutionName();
            if (!String.IsNullOrEmpty(masterp) && string.Compare(foundProduct.CompleteName, masterp, true, CultureInfo.InvariantCulture) != 0)
				DeleteLicensed(masterp);

			return foundProduct;
		}


        //la specialversion è quella dove gli viene passato un serial da mettere in un server della solution, 
        //identificato dall'attributo basicServer, solitamente versioni demo-dvlp-nfs
        //altrimenti è una versione normale 
        //e il seriale va messo nel modulo corrispondente controllanso gli short names
        //---------------------------------------------------------------------
        public bool SaveLicenseds(ProductInfo foundProduct, bool specialversion, bool allProducts, params SerialNumberInfo[] serial)
        {
            //prodotto master
            if (serial == null)
                serial = new SerialNumberInfo[] { };
            ArrayList listserials = new ArrayList { };
            foreach (SerialNumberInfo sni in serial)
            {
                if (sni != null)
                    listserials.Add(sni.GetSerialWOSeparator());
            }

            XmlDocument xmldocL = ProductInfo.WriteToLicensed(foundProduct, specialversion, (string[])listserials.ToArray(typeof(string)));
            if (!SaveLicensed(xmldocL.OuterXml, foundProduct.CompleteName))
            {
                return false;
            }

            if (allProducts)//ogni prodotto installato va attivato
            {
                IList<ProductInfo> sc = new List<ProductInfo>();

                foreach (ProductInfo p in ActivationObj.Products)
                    //se ho già attivato il prodotto master non devo attivare altre eventuali solution che sono nella stessa application
                    if (String.Compare(p.Application, foundProduct.Application, true, CultureInfo.InvariantCulture) != 0 &&
                        !sc.Contains(p))
                        sc.Add(p);
                foreach (ProductInfo p in sc)
                {
                    XmlDocument xmldoc = ProductInfo.WriteToLicensed(p, specialversion, null);
                    SaveLicensed(xmldoc.OuterXml, p.CompleteName);
                }
            }
            return true;
        }

        //---------------------------------------------------------------------
        public void DeleteLicensed(string name)
		{
			loginManager.DeleteLicensed(name);
		}
    
        //---------------------------------------------------------------------
        public void DeleteUserInfo()
		{
            loginManager.DeleteUserInfo();
		}

		//---------------------------------------------------------------------
		public bool SaveLicensed(string xml, string name)
		{
			return loginManager.SaveLicensed(xml, name);
		}

        //---------------------------------------------------------------------
        public void ReloadActivation()
        {
            loginManager.ReloadConfiguration();
        }

		//---------------------------------------------------------------------
		public int GetLoggedUsersNumber()
		{
			return loginManager.GetLoggedUsersNumber();
		}

		//---------------------------------------------------------------------
		public ProxySettings GetProxySetting()
		{
			return loginManager.GetProxySettings();

		}

		//---------------------------------------------------------------------
		public void SetProxySetting(ProxySettings proxySettings)
		{
			loginManager.SetProxySettings(proxySettings);
		}

		//---------------------------------------------------------------------
		internal bool ProductActivated ()
		{
			int daysToExpiration;
			ActivationState s = loginManager.GetActivationState(out daysToExpiration);
			//se non sono attivato, dopo l'about  che potrebbe aver portato ad una attivazione, chiuderò la console.
			bool activated = (s != ActivationState.Disabled && s != ActivationState.NoActivated);
            //if (!activated)
            //    SetError(LicenceStrings.ActivationKO, "", "");
			return activated;
		}

		//---------------------------------------------------------------------
		public string GetBrandedProductTitle()
		{
			return loginManager.GetBrandedProductTitle();
		}

		//---------------------------------------------------------------------
		public string GetBrandedProductName()
		{
			return loginManager.GetMasterProductBrandedName();
		}

		//---------------------------------------------------------------------
		public string GetBrandedProducerName()
		{
			return loginManager.GetBrandedProducerName();
		}

	}
}
