using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Microarea.TaskBuilderNet.Core.CoreTypes;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.XmlPersister;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Licence.Activation
{
	/// <summary>
	/// Un'istanza di questa classe rappresenta una fotografia
	/// dello stato delle informazioni utente.
	/// Tutti i suoi membri sono public r/w perché così facendo si possono
	/// serializzare in formato XML utilizzando XmlSerializer.
	/// </summary>
	//=========================================================================
	[Serializable]
	public class UserInfo : State
	{
		public string	Name;
		public string	Company;
		[XmlElement(ElementName = "FiscalCode")]
		public string	CodFisc; 		
		public string	VatNumber;		// P.Iva , massimo 16 caratteri
		public string	Address;
		public string	ZipCode;		// Codice postale, massimo 8 caratteri
		public string	City;			// Città
		public string	CityCode;		// Sigla di provincia
		public string	Country ;		// Codice ISO Nazione
		public string	Phone;
		public string	Fax;
		public string	Email;
		public string	DealerEmail;
        public string PECEmail;
		public ActivityCode ActivityCode;//codice attività di PAI
		public UserId[]	UserIdInfos;
		[XmlElement (ElementName="InternalAgreement")]
		public bool	ConsensoInterno = false;
		[XmlElement (ElementName="ExtensiveAgreement")]
		public bool	ConsensoEsteso	= false;
        public bool UsersListAgreement = false;

		public string UserIdHashCode;
        public string ProductHashCode ;
		public static string[] InternalCodes = new string[]{NameSolverStrings.Microarea, "0110G081"};

		ArrayList missingPersonalData = new ArrayList();
		public enum PersonalData { Name, Company, CodFisc, VatNumber, Address, ZipCode, City, Country, Phone, Fax, Email, ActivityCode, InternalAgreement }

		//---------------------------------------------------------------------
        [XmlIgnore]
        public ArrayList MissingPersonalData { get { return missingPersonalData; } }

		//---------------------------------------------------------------------
		public static UserInfo GetFromXml(string userInfoFile, out string message)
		{
			message = null;
			return UserInfo.GetFromXml(userInfoFile, typeof(UserInfo), out message) as UserInfo;
		}

        //---------------------------------------------------------------------
        private static byte[] StringToByteArray(string s)
        {
            byte[] result = new byte[s.Length];
            for (int i = 0; i < s.Length; i++)
                result[i] = (byte)s[i];
            return result;
        }

        //---------------------------------------------------------------------
        private static string ByteArrayToString(byte[] source)
        {
            string result = string.Empty;
            StringBuilder resultBuilder = new StringBuilder(source.Length);
            foreach (byte b in source)
                resultBuilder.Append((char)b);
            result = resultBuilder.ToString();
            return result;
        }

		//---------------------------------------------------------------------
		public XmlDocument GetXmlDom()
		{
			XmlDocument doc = new XmlDocument();
			string val;
			try
			{
				GetXmlString(out val);
				doc.LoadXml(val);
			}
			catch { }
			return doc;
		}

		[Obsolete ("Use 'UserInfo GetFromXml(string userInfoFile, out string message)'")]
		//---------------------------------------------------------------------
		public static UserInfo GetFromXml(string userInfoFile)
		{
			string message = null;
			return UserInfo.GetFromXml(userInfoFile, typeof(UserInfo), out message) as UserInfo;
		}

		//---------------------------------------------------------------------
		public bool IsComplete()
		{
			return (PersonalDataComplete() && AgreementComplete());
		}
		//---------------------------------------------------------------------
		public bool PersonalDataComplete()
		{
			GetMissingDataList();
			return (missingPersonalData == null || missingPersonalData.Count == 0);
		}

		//---------------------------------------------------------------------
		public bool AgreementComplete()
		{
			return ConsensoInterno;
		}

       

		//---------------------------------------------------------------------
		public void GetMissingDataList()
		{
			missingPersonalData.Clear();
			if (Name == null || Name.Length == 0)
				missingPersonalData.Add(PersonalData.Name);
			if (Company == null || Company.Length == 0)
				missingPersonalData.Add(PersonalData.Company);
			if (VatNumber == null || VatNumber.Length == 0)
				missingPersonalData.Add(PersonalData.VatNumber);
			if (Address == null || Address.Length== 0)
				missingPersonalData.Add(PersonalData.Address);
			if (ZipCode == null || ZipCode.Length == 0)
				missingPersonalData.Add(PersonalData.ZipCode);
			if (City == null || City.Length == 0)
				missingPersonalData.Add(PersonalData.City);
			if (Phone == null || Phone.Length == 0)
				missingPersonalData.Add(PersonalData.Phone);
			if (Fax == null || Fax.Length == 0)
				missingPersonalData.Add(PersonalData.Fax);
			if (Email == null || Email.Length == 0)
				missingPersonalData.Add(PersonalData.Email);
			if (Country == null	|| Country.Length == 0)
				missingPersonalData.Add(PersonalData.Country);
			if (!ConsensoInterno)
				missingPersonalData.Add(PersonalData.InternalAgreement);
			if (ActivityCode == null || ActivityCode.Area == null || ActivityCode.Group == null || ActivityCode.Area.Length == 0|| ActivityCode.Group.Length == 0)
				missingPersonalData.Add(PersonalData.ActivityCode);
			//per la country italia sono obbligatori sia cod fisc sia P.I.
			else if(String.Compare(Country, "IT", true, CultureInfo.InvariantCulture) == 0 && (CodFisc == null	|| CodFisc.Length == 0))
				missingPersonalData.Add(PersonalData.CodFisc);
		}

		//---------------------------------------------------------------------
		public static UserInfo GetFromDom(XmlDocument userInfoDom)
		{
			return UserInfo.GetFromXmlString(userInfoDom.InnerXml);
		}

		//---------------------------------------------------------------------
		public static UserInfo GetFromXmlString(string xmlString)
		{
			return UserInfo.GetFromXmlString(xmlString, typeof(UserInfo)) as UserInfo;
		}

		//---------------------------------------------------------------------
		public void SetUserIdInfos(UserId[] infos)
		{
			//sovrascrivo sempre con cio`che mi torna indietro dal nostro ws
			UserIdInfos = infos;	
		}

		//---------------------------------------------------------------------
		public string GetUserId()
		{
			if (UserIdInfos == null || UserIdInfos.Length == 0)
				return string.Empty;
			foreach (UserId userId in UserIdInfos)
			{
				foreach (string s in InternalCodes)
				{
					if (
						(String.Compare(userId.InternalCode, s, true, CultureInfo.InvariantCulture) == 0) ||
						(String.Compare(userId.Producer, s, true, CultureInfo.InvariantCulture) == 0)
						)
						return userId.Value;
				}
			}
			return String.Empty;
		}
		//---------------------------------------------------------------------
		public string GetUserId(string internalCode)
		{
			if (UserIdInfos == null || UserIdInfos.Length == 0)
				return string.Empty;
			foreach (UserId userId in UserIdInfos)
				if (String.Compare(userId.InternalCode, internalCode, true, CultureInfo.InvariantCulture) == 0)
					return userId.Value;
			return String.Empty;
		}

		/// <summary>
		/// Specifica se tra i due stati ci sono diversità che vanno ad influire sulla chiave di attivazione
		/// </summary>
		//---------------------------------------------------------------------
		public static bool IsToReactivate(State userInfo1, State userInfo2, int activationVersion)
		{
			UserInfo user1 = userInfo1 as UserInfo;
			UserInfo user2 = userInfo2 as UserInfo;
			if (user1 == null || user2 == null)
				return true; 
			//Considero case-insensitive e trimmati, come fa anche la verifica della chiave, 
			//per eliminare un po di messaggi inutili, in caso di modifiche del genere.
			string country1		= user1.Country;
			string country2		= user2.Country;
			string codFisc1	= user1.CodFisc;
			string codFisc2	= user2.CodFisc;
			string company1		= user1.Company;
			string company2		= user2.Company;

			//utilizzo per verificare se la stringa può essere considerata uguale, lo stesso metodo usato da Activation
			ISignParamsPreparer signParametersPreparer =
				new SignParamsPrepFactory().GetSignParamsPreparer(activationVersion);
			if (country1 != null && country2 != null)
			{
				signParametersPreparer.PrepareXmlConfigFile(ref country1);
				signParametersPreparer.PrepareXmlConfigFile(ref country2);
				country1	= Components.SNFormatter.CleanSN(country1.ToUpperInvariant());
				country2	= Components.SNFormatter.CleanSN(country2.ToUpperInvariant());
			}
			if (codFisc1 != null && codFisc2 != null)
			{
				signParametersPreparer.PrepareXmlConfigFile(ref codFisc1);
				signParametersPreparer.PrepareXmlConfigFile(ref codFisc2);
				codFisc1	= Components.SNFormatter.CleanSN(codFisc1.ToUpperInvariant());
				codFisc2	= Components.SNFormatter.CleanSN(codFisc2.ToUpperInvariant());
			}
			if (company1 != null && company2 != null)
			{
				signParametersPreparer.PrepareXmlConfigFile(ref company1);
				signParametersPreparer.PrepareXmlConfigFile(ref company2);
				company1	= Components.SNFormatter.CleanSN(company1.ToUpperInvariant());
				company2	= Components.SNFormatter.CleanSN(company2.ToUpperInvariant());
			}
			
			//paragone caseSensitive, perchè ho messo la stringa a toUpper, 
			//come fa il procedimento di generazione della chiave
			bool b =  (!(
				String.Compare(country1,	country2,	false, CultureInfo.InvariantCulture) == 0 &&
				String.Compare(codFisc1,	codFisc2, false, CultureInfo.InvariantCulture) == 0 &&
				String.Compare(company1,	company2,	false, CultureInfo.InvariantCulture) == 0
				));
			if (!b)
				return false;
			return true;
		}

		//---------------------------------------------------------------------
		public override int GetHashCode()
		{
			int hashCode = 0;
			if (CodFisc != null && CodFisc.Trim().Length > 0)
				hashCode = CodFisc.GetHashCode();

			if (VatNumber != null && VatNumber.Trim().Length > 0)
				hashCode += VatNumber.GetHashCode();

			if (Company != null && Company.Trim().Length > 0)
				hashCode += Company.GetHashCode();

			if (Country != null && Country.Trim().Length > 0)
				hashCode += Country.GetHashCode();

			return hashCode;
		}

		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is UserInfo))
				return false;

			UserInfo ui2 = obj as UserInfo;

			return
			AreActivityCodesEqual(ActivityCode, ui2.ActivityCode) &&
			AreUserInfoEqual(this.Address, ui2.Address) &&
			AreUserInfoEqual(this.City, ui2.City) &&
			AreUserInfoEqual(this.CityCode, ui2.CityCode) &&
			AreUserInfoEqual(this.CodFisc, ui2.CodFisc) &&
			AreUserInfoEqual(this.Company, ui2.Company) &&
			AreUserInfoEqual(this.Country, ui2.Country) &&
			AreUserInfoEqual(this.DealerEmail, ui2.DealerEmail) &&
            AreUserInfoEqual(this.PECEmail, ui2.PECEmail) &&
			AreUserInfoEqual(this.Email, ui2.Email) &&
			AreUserInfoEqual(this.Fax, ui2.Fax) &&
			AreUserInfoEqual(this.Name, ui2.Name) &&
			AreUserInfoEqual(this.Phone, ui2.Phone) &&
            AreUserInfoEqual(this.ProductHashCode, ui2.ProductHashCode) &&
			AreUserInfoEqual(this.VatNumber, ui2.VatNumber) &&
			AreUserInfoEqual(this.ZipCode, ui2.ZipCode) &&
			AreUserIdInfosEqual(ui2.UserIdInfos) &&
			this.ConsensoEsteso == ui2.ConsensoEsteso &&
			this.ConsensoInterno == ui2.ConsensoInterno;
		}

		//---------------------------------------------------------------------
		private bool AreActivityCodesEqual(ActivityCode a, ActivityCode b)
		{
			bool activitiCodeEquals = false;

			if (a == null)
				activitiCodeEquals = (b == null);
			else
				activitiCodeEquals = a.Equals(b);
			return activitiCodeEquals;
		}

		//---------------------------------------------------------------------
		private bool AreUserInfoEqual(string a, string b)
		{//osono identiche o sono entrambe empty e/o null
			return
		(String.Compare(a, b, false, CultureInfo.InvariantCulture) == 0) ||
		(String.IsNullOrEmpty(a) && String.IsNullOrEmpty(b));
		}

		//---------------------------------------------------------------------
		private bool AreUserIdInfosEqual(UserId[] uids)
		{
			if (uids == null && this.UserIdInfos == null)
				return true;

			if ((this.UserIdInfos == null && uids != null) ||
				(this.UserIdInfos != null && uids == null))
				return false;

			if (uids.Length != this.UserIdInfos.Length)
				return false;
			bool found = false;
			foreach (UserId id in this.UserIdInfos)
			{
				foreach (UserId id2 in uids)
				{
					if (id.Equals(id2))
					{
						found = true;
						break;
					}
				}
				if (!found) return false;
			}
			return true;
		}

        
        //---------------------------------------------------------------------
        public DateTime GetMluDate(out bool cancelled)
        {
            cancelled = false;
            DateTime mludate = DateTimeFunctions.MaxValue;
            if (String.IsNullOrWhiteSpace(ProductHashCode)) 
            //in caso di valore vuoto ( perchè non ancora impostato  o perchè qualcuno lo ha cancellato) restituisco comunque una data che fa funzionare il programma con qualsiasi dataApplicazione.
                return mludate;

            string dateString = Crypto.Decrypt(ProductHashCode);
            //cosa fare se il producthashcode è in formato non valido e non si riesce a decriptare
            //per es: modificato a mano dall'utente, o anche potrebbe capitare che abbiamo cambiato il contenuto della stringa in versioni successive
            //se lo ripulissi poi non avrei modo di bloccarlo in caso avesse manomesso il codice, quindi per scelta lo lascio errato, l'utente sarà bloccato
            //in caso fosse un problema di versioni successive che hanno cambiato il crypting allora la situazione si risolve con i ping successivi che correggono 
            //o con un intervento manuale di cancellazione del producthashcode dal file delle useringo ( dopo aver stoppato iis, perchè il codice viene persistito ad ogni end applicazione)

            if (String.IsNullOrWhiteSpace(dateString))
            {
                throw new Exception(" - Error 13322 - Error evaluating operation date: " + ProductHashCode);
            }

            //dentro datestring c'è sia la data sia  il valore bool che indica se MLU disdettato o no, devo quindi parsarmi la stringa
            cancelled = (dateString.EndsWith(bool.TrueString, StringComparison.InvariantCultureIgnoreCase));

            if (!cancelled)//SEMPRE OK SE NON DISDETTATO!
                return new DateTime(DateTimeFunctions.MaxYear, DateTimeFunctions.MaxMonth, DateTimeFunctions.MaxDay-1);//non è max date, ma è una data massima in modo che valga sempre come valida e sia da me riconosciuta come diversa da datetime.maxvalue, se usassi max date andrebbe tutte le volte a rileggere il file

            //se disdettato devo leggere la data separandola dal 'True' con essa concatenato
            int trueLen = bool.TrueString.Length;
            if (dateString.Length > trueLen)//non può essere lungo come o meno della stringa 'true' , perchè deve contenere anche la data!
            {
                dateString = dateString.Remove(dateString.Length - trueLen, trueLen);//rimuovo il 'true' finale e provo a riconoscere la data
                if (DateTime.TryParseExact(dateString, WceStrings.MluDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out mludate))
                    return mludate;

            }
            throw new Exception(" - Error 13322 - Error evaluating operation date: " + ProductHashCode);
        }
    }

	//=========================================================================
	[Serializable]
	public class UserId
	{
		public const string  internalcode	= WceStrings.Attribute.InternalCode;
		public const string  producer		= WceStrings.Attribute.Producer;
		public const string  val			= WceStrings.Attribute.Value;
		public const string activationid = WceStrings.Attribute.ActivationID;


		[XmlAttribute (AttributeName = internalcode)]
		public string InternalCode = null;

		[XmlAttribute (AttributeName = producer)]
		public string Producer	 = null;

		[XmlAttribute (AttributeName = val)]
		public string Value		= null;

		[XmlAttribute(AttributeName = activationid)]
		public string ActivationID = null;

		//---------------------------------------------------------------------
		public UserId()
		{
			
		}

		//---------------------------------------------------------------------
		public UserId(string producer, string internalCode, string aValue)
			: this(producer, internalCode, aValue, "")
		{
			
		}

		//---------------------------------------------------------------------
		public UserId(string producer, string internalCode, string aValue, string activationId)
		{
			Value			= aValue;
			InternalCode	= internalCode;
			Producer		= producer;
			ActivationID	= activationId;
		}

		//---------------------------------------------------------------------
		public override int GetHashCode()
		{
			return this.Value.GetHashCode ();
		}


		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is UserId))
				return false;
			UserId ui2 = obj as UserId;
			return
			String.Compare(this.Value, ui2.Value, true, CultureInfo.InvariantCulture) == 0 &&
			String.Compare(this.Producer, ui2.Producer, true, CultureInfo.InvariantCulture) == 0 &&
			String.Compare(this.InternalCode, ui2.InternalCode, true, CultureInfo.InvariantCulture) == 0;
		}

	}

	//=========================================================================
	[Serializable]
	public class ActivityCodeGroup
	{
		private string group = null;
		private string description = null;
		public IList<ActivityCodeArea> Areas = new List<ActivityCodeArea> { };
		
		public string Description  {get { return description; } set { description = value; }}

		public string Group  {get { return group; } set { group = value; }}

		//---------------------------------------------------------------------
		public ActivityCodeGroup()
		{
			
		}

		//---------------------------------------------------------------------
		public ActivityCodeGroup(string group, string description)
		{
			this.group = group;
			this.description = description;
		}

		//---------------------------------------------------------------------
		public override string ToString()
		{
			return description;
		}

	}

	//=========================================================================
	[Serializable]
	public class ActivityCodeArea
	{
		private string description = null;
		private string area	 = null;
		public string Description  {get { return description; } set { description = value; }}

		public string Area  {get { return area; } set { area = value; }}

		//---------------------------------------------------------------------
		public ActivityCodeArea()
		{
		}

		//---------------------------------------------------------------------
		public ActivityCodeArea(string area, string description)
		{
			this.description = description;
			this.area  = area;
		}

		//---------------------------------------------------------------------
		public override string ToString()
		{
			return description;
		}
	}

	//=========================================================================
	[Serializable]
	public class ActivityCode
	{

		[XmlAttribute (AttributeName = "group")]
		public string Group = null;
		[XmlAttribute (AttributeName = "area")]
		public string Area	 = null;


		//---------------------------------------------------------------------
		public ActivityCode()
		{
			
		}

		//---------------------------------------------------------------------
		public ActivityCode(string group, string area)
		{
			Group = group;
			Area  = area;
		}

		//---------------------------------------------------------------------
		public override int GetHashCode()
		{
			return this.Group.GetHashCode() + this.Area.GetHashCode();
		}


		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is ActivityCode))
				return false;
			ActivityCode ac = obj as ActivityCode;
			return
			String.Compare(this.Group, ac.Group, true, CultureInfo.InvariantCulture) == 0 &&
			String.Compare(this.Area, ac.Area, true, CultureInfo.InvariantCulture) == 0;
		}

	}
}
