using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;

using Microarea.TaskBuilderNet.Interfaces;

using Microarea.TaskBuilderNet.Core.CoreTypes;
using Microarea.TaskBuilderNet.Core.XmlPersister;

namespace Microarea.TaskBuilderNet.Core.Generic
{

	//=========================================================================
	[Serializable]
	public class Attachment : State
	{
		private string id;
		private byte[] content;

		//---------------------------------------------------------------------
		public string Id
		{
			get { return id; }
			set { id = value; }
		}

		//---------------------------------------------------------------------
		[XmlIgnore]
		public byte[] Content
		{
			get { return content; }
			set { content = value; }
		}

		//---------------------------------------------------------------------
		[XmlElement(DataType="string", ElementName="Content", IsNullable=true)]
		public string Base64Content
		{
			get
			{
				if (content == null)
					return string.Empty;

				return Convert.ToBase64String(content);
			}
			set
			{
				if (value == null || value.Trim().Length == 0)
				{
					content = new byte[0];
					return;
				}
				content = Convert.FromBase64String(value);
			}
		}

		//---------------------------------------------------------------------
		public Attachment() { }

		//---------------------------------------------------------------------
		public Attachment(string id, byte[] content)
		{
			this.id = id;
			this.content = content;
		}
	}

	//=========================================================================
	[Serializable]
	public class Parameters: State
	{
		public const int		ProtocolVersion		= 6;

		[XmlAttribute (AttributeName = "protocolversion")]
		public int				CurrentProtocolVersion= 0;
		public int				ActivationVersion	= 0;
		public string			InstallationName = string.Empty;
		public StringCollection MacAddresses;
		public string			TraceRoute = string.Empty;	
		public string			MachineName = string.Empty;
		public XmlNode			Wce;
		public string			ActivationKey = string.Empty;
		public int				Mode;
		public string			ServiceUrl = string.Empty;
		public string			Country = string.Empty;
		public string			OperatingSystemVersion = string.Empty;
		public int				ActivationPeriod = 0;
		public int				WarningPeriod = 0;
		public string			Error = string.Empty;
		public string			UserId = string.Empty;
		public string			ActivationId = string.Empty;
		public ContractDataBag	ContractData;
		public bool ShowFormForMluChargeChoice;
		public bool UserChoseMluInChargeToMicroarea;
		public SerialNumberType SerialNumberType;
		public List<Attachment> Attachments = new List<Attachment>();
        public List<CompanyDataBag> UsedCompanies = new List<CompanyDataBag>();
        public List<LBag> LoginInfos = new List<LBag>();
        public List<ClientData> LoggedClientData = new List<ClientData>();
        public bool FeUsed = false;
        public StringCollection Customizations;

        //---------------------------------------------------------------------
        public Parameters(
			int					activationVersion,
			string				installationName,
			StringCollection	macAddresses,
			string				traceRoute,
			string				machineName,
			XmlNode				wce,
			string				activationKey,
			int					mode,
			string				serviceUrl,
			string				country,
			int					activationPeriod,
			int					warningPeriod,
			string				error,
			string				userid,
			string				activationId,
			ContractDataBag		contractData,
			SerialNumberType	serialNumberType
			)
		{
			ActivationVersion	= activationVersion;
			CurrentProtocolVersion= ProtocolVersion;
			InstallationName	= installationName;
			MacAddresses		= macAddresses;
			TraceRoute			= traceRoute;	
			MachineName			= machineName;
			Wce					= wce;
			ActivationKey		= activationKey;
			Mode				= mode;
			ServiceUrl			= serviceUrl;
			Country				= country;
			ActivationPeriod	= activationPeriod;
			WarningPeriod		= warningPeriod;
			Error				= error;	
			UserId				= userid;
			ActivationId		= activationId;
			ContractData		= contractData;
			try
			{
				OperatingSystemVersion = Environment.OSVersion.ToString();
			}
			catch
			{
				OperatingSystemVersion = string.Empty;
			}
			SerialNumberType	= serialNumberType;
		}

		//---------------------------------------------------------------------
		public Parameters(Parameters parameters)
		{
			ActivationVersion	= parameters.ActivationVersion;
			CurrentProtocolVersion = parameters.CurrentProtocolVersion;
			InstallationName	= parameters.InstallationName;
			MacAddresses		= parameters.MacAddresses;
			TraceRoute			= parameters.TraceRoute;	
			MachineName			= parameters.MachineName;
			Wce					= parameters.Wce;
			ActivationKey		= parameters.ActivationKey;
			Mode				= parameters.Mode;
			ServiceUrl			= parameters.ServiceUrl;
			Country				= parameters.Country;
			OperatingSystemVersion = parameters.OperatingSystemVersion;
			ActivationPeriod	= parameters.ActivationPeriod;
			WarningPeriod		= parameters.WarningPeriod;
			Error				= parameters.Error;
			UserId				= parameters.UserId;
			ActivationId		= parameters.ActivationId;
			ContractData		= parameters.ContractData;
            Customizations = parameters.Customizations;
            ShowFormForMluChargeChoice = parameters.ShowFormForMluChargeChoice;
			UserChoseMluInChargeToMicroarea = parameters.UserChoseMluInChargeToMicroarea;
			SerialNumberType	= parameters.SerialNumberType;
			UsedCompanies.AddRange(parameters.UsedCompanies);
            LoginInfos.AddRange(parameters.LoginInfos);
            LoggedClientData.AddRange(parameters.LoggedClientData);
		}

		//---------------------------------------------------------------------
		public Parameters() { }

		//---------------------------------------------------------------------
		[XmlIgnore]
		public bool HasErrors {get {return (Error != null && Error.Trim().Length > 0);}}


        //---------------------------------------------------------------------
        public static Parameters GetParametersForError(string error)
		{
			Parameters err = new Parameters();
			err.Error = error;
			return err;
		}

		//---------------------------------------------------------------------
		public override int GetHashCode()
		{
			return base.GetHashCode ();
		}

		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			return Equals(obj, true);
		}

		//---------------------------------------------------------------------
		public bool Equals(object obj, bool checkActivationKey)
		{
			if (obj == null || !(obj is Parameters))
				return false;

			Parameters currentParam = (Parameters) obj;
			
			for(int i = 0; i < currentParam.MacAddresses.Count; i ++)
				if (string.Compare(currentParam.MacAddresses[i], this.MacAddresses[i], true, CultureInfo.InvariantCulture) != 0)
					return false;

			if (checkActivationKey && String.Compare(currentParam.ActivationKey, this.ActivationKey, false, CultureInfo.InvariantCulture) != 0)
					return false;
			
			return (
				currentParam.ActivationVersion	==	this.ActivationVersion							&&
				String.Compare(currentParam.InstallationName,	this.InstallationName, false,	CultureInfo.InvariantCulture)	== 0	&&
				String.Compare(currentParam.MachineName,		this.MachineName, false,		CultureInfo.InvariantCulture)	== 0	&&
				String.Compare(currentParam.TraceRoute,			this.TraceRoute, false,			CultureInfo.InvariantCulture)	== 0	&&
				//String.Compare(currentParam.Wce.OuterXml,		this.Wce.OuterXml, false,		CultureInfo.InvariantCulture)	== 0	&&
				String.Compare(currentParam.ServiceUrl,			this.ServiceUrl, false,			CultureInfo.InvariantCulture)	== 0	&&
				String.Compare(currentParam.Country,			this.Country, false,			CultureInfo.InvariantCulture)	== 0);
		}
	}

	//=========================================================================
	[Serializable]
	public class Advertisement : ISerializable, IAdvertisement	
	{
		private bool		hideDisclaimer = false;
		private DateTime	expiryDate	= DateTime.MinValue;
		private DateTime creationDate	= DateTime.Now;
		private MessageType type		= MessageType.None;
        private string      tag         = null;
		private int			severity	= 0;
		private string		id			= String.Empty;
		private IAdvertisementBody body  = new AdvertisementBody();
		private List<String> recipients = new List<string>();
        private bool expireWithRestart = false;
        private bool historicize = true; //come era prima di aggiungere questa property, così il vecchio rimane uguale
        private bool immediate = false;// messaggio prioritario 
        private MessageSensation sensation =  MessageSensation.Information;
        private int autoClosingTime = 0;//zero vuol dire che deve chiuderlo l'utente

        /// <summary>
        ///I messaggi immediate che vengono mostrati nel balloon in autotico portano questa proprietà che indica quanti 
        ///millisecondi il balloon deve rimanere aperto prima di chiudersi da solo. (se si utilizzano i pulsanti di 
        ///navigazione il balloon poi resterà aperto fino all chiusura manuale da parte dell'utente). 
        ///se zero (default) vuol dire che  il balloon non si chiuederà da solo, ma solo ad opera dell'utente.
        /// </summary>
        public int AutoClosingTime { get { return autoClosingTime; } set { autoClosingTime = value; } }
        /// <summary>
        /// id univoco del messaggio
        /// </summary>
		public string		ID				{ get {return id;}				set {id				= value;}}
        /// <summary>
        ///Il disclaimer che spiega come modificare le impostazioni di ricezione dei messaggi in arrivo da microarea 
        ///viene mostrato solo se questa property è a false e comunque mai se il tipo è postalite 
        ///(perchè sono messaggi interni all'applicazione, non avrebbe senso disabilitarli da sito) (default = false)
        /// </summary>
        public bool HideDisclaimer
        {
            get
            {
                return
                    (this.type == MessageType.PostaLite || this.Immediate) ? true : hideDisclaimer;
            }
            set { hideDisclaimer = value; }
        }

        /// <summary>
        ///i messaggi con questa proprietà a true vengono cancellati definitivamente al restart di login manager (default = false).
        /// </summary>
        public bool ExpireWithRestart { get { return expireWithRestart; } set { expireWithRestart = value;} }
        /// <summary>
        ///i messaggi se storicizzabili una volta letti vengono mantenuti fra i messaggi letti fino alla loro scadenza, 
        ///altrimenti se non storicizzabili vengono cancellati subito dopo la lettura. (default = true)
        /// </summary>
        public bool Historicize { get { return historicize; } set { historicize = value;} }

        /// <summary>
        /// I messaggi immediate possono essere reperiti da un metodo apposito ed essere mostrati senza dover cliccare sulla bustina  (default = false)
        /// </summary>
        public bool Immediate { get { return immediate; } set { immediate = value;} }
        /// <summary>
        /// Data di scadenza del messaggio, oltre la quale il messaggio sarà cancellato e non più visibile neanche fra i messaggi già letti
        /// </summary>
        public DateTime		ExpiryDate		{ get {return  expiryDate;}		set {expiryDate		= value;}}
        /// <summary>
        /// tipologia di messaggio, comanda l'immagine che viene mostrata nel balloon e se è di tipo none il messagggio viene ignorato
        /// </summary>
        public MessageType Type { get { return type; } set { type = value; } }
        /// <summary>
        /// stringa customizzabile per la quale è possibile riconoscere gli Advertisement ( utilizzabile come tipazione custom)
        /// </summary>
        public string Tag { get { return tag; } set { tag = value; } }
		/// <summary>
		/// Grado di importanza del messaggio, attualmente non usato.
		/// </summary>
        public int			Severity		{ get {return severity;}		set {severity		= value;}}
        /// <summary>
        /// lista di stringhe con i nomi degli user (username) che riceveranno il messaggio, se null il messaggio è per tutti
        /// </summary>
        public List<String> Recipients		{ get { return recipients; } }
       
        /// <summary>
        /// corpo del messaggio
        /// </summary>
		[XmlIgnore]
		public IAdvertisementBody Body { get { return body; } set { body = value; } }
		public AdvertisementBody BodyTyped { get { return Body as AdvertisementBody; } set { Body = value; } }//mi sa che serve per serializzazione
        /// <summary>
        /// data di creazione del messaggio che viene impostata automaticamente a datetime.now e che viene utilizzata per mostrare i messaggi ordinati
        /// </summary>
        public DateTime CreationDate { get { return creationDate; } set { creationDate = value; } }
        /// <summary>
        /// Enumerativo che comanda l apiccola icona che specifica ulteriormente il tipo di messaggio, a prima vista si capisce se è errore, warning, successo,...
        /// </summary>
        public MessageSensation Sensation { get { return sensation; } set { sensation = value; } }


		private const string hideDisclaimerName	= "HideDisclaimer";
        private const string expireWithRestartName = "ExpireWithRestart";
		private const string expiryDateName		= "ExpiryDate";
		private const string typeName			= "Type";
		private const string severityName		= "Severity";
		private const string idName				= "ID";
		private const string bodyName			= "Body";
		private const string creationDateName	= "CreationDate";
        private const string recipientsName		= "Recipients";
        private const string immediateName = "Immediate";
        private const string storifyToExpiringName = "Historicize";
        private const string sensationName = "Sensation";
        private const string autoClosingTimeName = "AutoClosingTime";

		//---------------------------------------------------------------------
		public Advertisement(){} // DO NOT REMOVE! Needed for serialization

		//---------------------------------------------------------------------
		protected Advertisement(SerializationInfo info, StreamingContext context)
		{
			try
			{this.hideDisclaimer	= info.GetBoolean(hideDisclaimerName);}
			catch (SerializationException)
			{this.hideDisclaimer = false;}
            try
            { this.expireWithRestart = info.GetBoolean(expireWithRestartName); }
            catch (SerializationException)
            { this.expireWithRestart = false; }
			try
			{this.expiryDate		= info.GetDateTime(expiryDateName);}
			catch (SerializationException)
			{this.expiryDate = DateTime.Now.Add(new TimeSpan(24,0,0));}
			try
			{this.type			= (MessageType)info.GetValue(typeName, typeof(MessageType));}
			catch (SerializationException)
			{this.type = MessageType.None;}
			try
			{this.severity		= info.GetInt32(severityName);}
			catch (SerializationException)
			{this.severity = 0;}
			try
			{this.id			= info.GetString(idName);}
			catch (SerializationException)
			{this.id = string.Empty;}
			try
			{this.body			= (AdvertisementBody)info.GetValue(bodyName, typeof(AdvertisementBody));}
			catch (SerializationException)
			{this.body = new AdvertisementBody();}
			try
			{this.creationDate		= info.GetDateTime(creationDateName);}
			catch (SerializationException)
			{this.creationDate = DateTime.Now;}
            try
            { this.recipients = (List<string>)info.GetValue(recipientsName, typeof(List<string>)); }
            catch (SerializationException)
            { this.recipients = new List<string>(); }

            try
			{this.historicize	= info.GetBoolean(storifyToExpiringName);}
			catch (SerializationException)
            { this.historicize = true; }

            try
            { this.immediate = info.GetBoolean(immediateName); }
            catch (SerializationException)
            { this.immediate = false; }

            try
			{this.sensation			= (MessageSensation)info.GetValue(sensationName, typeof(MessageSensation));}
            catch (SerializationException)
            { this.sensation = MessageSensation.Information; }

            try
            { this.autoClosingTime = info.GetInt32(autoClosingTimeName); }
            catch (SerializationException)
            { this.autoClosingTime = 0; }

            //if (this.type == MessageType.PostaLite) this.hideDisclaimer = true;

		}

		//---------------------------------------------------------------------
        public Advertisement(string message, string link, string html, bool hideDisclaimer, DateTime expiration, MessageType type, int severity, string id, bool storify = true, bool immediate = false,  MessageSensation sensation = MessageSensation.Information)
		{
            this.hideDisclaimer = hideDisclaimer;
			this.expiryDate		= expiration;
			this.type			= type;
           // if (this.type == MessageType.PostaLite) this.hideDisclaimer = true;//i messaggi di tipo postalite sono utilizzati all'interno dell'applicazione quindi non è possibile disabilitarli da sito
			this.severity		= severity;
			this.id				= id;
			this.Body			= new AdvertisementBody(message, link, html);
            this.historicize  = storify;
            this.immediate = immediate;
            this.sensation = sensation;
		}

		//---------------------------------------------------------------------
		public bool Expired
		{
			get { return DateTime.Now > expiryDate; }
		}

		//---------------------------------------------------------------------
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue(hideDisclaimerName, hideDisclaimer);
            info.AddValue(expireWithRestartName, expireWithRestart);
			info.AddValue(expiryDateName, expiryDate);
			info.AddValue(typeName, type);
			info.AddValue(severityName, severity);
			info.AddValue(idName, id);
			info.AddValue(bodyName, body);
			info.AddValue(creationDateName, creationDate);
			info.AddValue(recipientsName, recipients);
            info.AddValue(immediateName, immediate);
            info.AddValue(storifyToExpiringName, historicize);
            info.AddValue(autoClosingTimeName, autoClosingTime);
		}
	
		//=========================================================================
		[Serializable]
		public class AdvertisementBody : ISerializable, IAdvertisementBody
		{
			private	string		text	= String.Empty;
			private string		link	= String.Empty;
			private string		html	= String.Empty;
			private ILocalizationBag localizationBag;

			public	string		Text	{ get {return text;} set {text = value;}}
			public	string		Link	{ get {return link;} set {link = value;}}
			public	string		Html	{ get {return html;} set {html = value;}}
			[XmlIgnore]
			public	ILocalizationBag		LocalizationBag	{ get {return localizationBag;}set {localizationBag = value;}}
			public LocalizationBag LocalizationBagTyped { get { return LocalizationBag as LocalizationBag; } set { LocalizationBag = value; } }

			//---------------------------------------------------------------------
			public AdvertisementBody(){} // DO NOT REMOVE! Needed for serialization

			//---------------------------------------------------------------------
			public AdvertisementBody(string text, string link, string html)
			{
				this.text = text;
				this.link = link;
				this.html = html;
			}

			//---------------------------------------------------------------------
			public AdvertisementBody(LocalizationBag localizationBag)
			{
				this.localizationBag = localizationBag;
			}

			//---------------------------------------------------------------------
			protected AdvertisementBody(SerializationInfo info, StreamingContext context)
			{
				try
				{this.text = info.GetString(textName);}
				catch (SerializationException)
				{this.text = string.Empty;}

				try
				{this.link = info.GetString(linkName);}
				catch (SerializationException)
				{this.link = string.Empty;}

				try
				{this.html = info.GetString(htmlName);}
				catch (SerializationException)
				{this.html = string.Empty;}

				try
				{this.localizationBag		= info.GetValue(localizationBagName, typeof(LocalizationBag)) as LocalizationBag;}
				catch (SerializationException)
				{this.localizationBag = new LocalizationBag();}
			}

			//---------------------------------------------------------------------
			public void GetObjectData(SerializationInfo info, StreamingContext context)
			{
				info.AddValue(textName, text);
				info.AddValue(linkName, link);
				info.AddValue(htmlName, html);
				info.AddValue(localizationBagName, localizationBag);
			}

			private const string textName	= "Text";
			private const string linkName	= "Link";
			private const string htmlName	= "Html";
			private const string localizationBagName= "LocalizationBag";
		}
	}

    //=========================================================================
    public enum ContractData
	{
		None = 0,
		FreePeriodExpiration = 1,
		InstalmentExpiration = 2,
		ContractExpiration = 3
	}


    //=========================================================================
    [Serializable]
    public class LBag//LoginInfo relative al db di sistema
    {
        [XmlIgnore]
        public string Login;
        public string L { get { return Login; } set { Login = value; } }
        [XmlIgnore]
        public bool Disabled;
        public int D { get { return Disabled ? 1 : 0; } set { Disabled = (value==1); } }
        [XmlIgnore]
        public bool WebSiteAdmin;
        public int A { get { return WebSiteAdmin ? 1 : 0; } set { WebSiteAdmin = (value == 1); } }


        //---------------------------------------------------------------------
        public LBag()// DO NOT REMOVE! Needed for serialization
        {
        }

                //---------------------------------------------------------------------
public LBag(string login, bool disabled, bool admin)
        {
            Login = login;
            Disabled = disabled;
            WebSiteAdmin = admin;
        }
    }

    //=========================================================================
    [Serializable]
    public class ClientData
    {
        public string Name;
        public string CPU;
        public string Resolutions;
        public string RamType;
       

        //---------------------------------------------------------------------
        public ClientData()// DO NOT REMOVE! Needed for serialization
        {
        }

        //---------------------------------------------------------------------
        public ClientData(string name, string cpu, string resolution, string ramtype)
        {
            Name = name;
            CPU = cpu;
            Resolutions = resolution;
            RamType = ramtype;
       
        }
    }


	//=========================================================================
    [Serializable]
    public class CompanyDataBag
    {
        public string Name ;
        public string Code ;
        public DateTime Date;
        public string Username;
        public string CompanyName;
        public string DbName;


         //---------------------------------------------------------------------
        public CompanyDataBag()// DO NOT REMOVE! Needed for serialization
        {
        }

        //---------------------------------------------------------------------
		public CompanyDataBag(string name, string code, DateTime date, string company, string db, string user)
        {
            Name = name.ToUpperInvariant();
            Code = code.ToUpperInvariant();
            Date = date;
            CompanyName = company;
            DbName = db;
            Username = user;
        }

        //---------------------------------------------------------------------
        public override bool Equals(object obj)
        {
            CompanyDataBag o = obj as CompanyDataBag;
            if (o == null) return false;
            return (
            string.Compare(this.Code, o.Code, true, CultureInfo.InvariantCulture) == 0 &&
            string.Compare(this.Name, o.Name, true, CultureInfo.InvariantCulture) == 0 && 
            string.Compare(this.CompanyName, o.CompanyName, true, CultureInfo.InvariantCulture) == 0 &&
            string.Compare(this.DbName, o.DbName, true, CultureInfo.InvariantCulture) == 0
            );
        }

        //---------------------------------------------------------------------
        public override int GetHashCode()
        {
            return (Name + Code + CompanyName + DbName).GetHashCode();
        }



    }

	//=========================================================================
	public struct ContractDataBag
	{
		public const int ToBeShowedOffset = 100;

		//---------------------------------------------------------------------
		static ContractDataBag()
		{
		}

		private ContractData contractData;
		private TimeSpan paymentRenewalPeriod;
		private DateTime paymentRenewalDate;
		private bool toBeShowed;

		//---------------------------------------------------------------------
		public ContractDataBag(
			ContractData contractData,
			TimeSpan paymentRenewalPeriod,
			DateTime paymentRenewalDate,
			bool toBeShowed
			)
		{
			this.contractData = contractData;
			this.paymentRenewalPeriod = paymentRenewalPeriod;

            if (paymentRenewalDate < DateTimeFunctions.MinValue)
                paymentRenewalDate = DateTimeFunctions.MinValue;
          
            if (paymentRenewalDate > DateTimeFunctions.MaxValue)
                paymentRenewalDate = DateTimeFunctions.MaxValue;

			this.paymentRenewalDate = paymentRenewalDate;
			this.toBeShowed = toBeShowed;
		}

		//---------------------------------------------------------------------
		public ContractData ContractData
		{
			get { return contractData; }
			set { contractData = value; }
		}

		//---------------------------------------------------------------------
		[XmlIgnore]
		public TimeSpan PaymentRenewalPeriod
		{
			get { return paymentRenewalPeriod; }
			set { paymentRenewalPeriod = value; }
		}

		//---------------------------------------------------------------------
		[XmlElement("PaymentRenewalPeriod")]//Trick to serialize a time span in xml.
		public string XmlPaymentRenewalPeriod
		{
			get { return paymentRenewalPeriod.ToString(); }
			set { paymentRenewalPeriod = TimeSpan.Parse(value); }
		}

		//---------------------------------------------------------------------
		public bool ToBeShowed
		{
			get { return toBeShowed; }
			set { toBeShowed = value; }
		}

		//---------------------------------------------------------------------
		public DateTime PaymentRenewalDate
		{
			get
			{
                if (paymentRenewalDate < DateTimeFunctions.MinValue)
                    paymentRenewalDate = DateTimeFunctions.MinValue;

                if (paymentRenewalDate > DateTimeFunctions.MaxValue)
                    paymentRenewalDate = DateTimeFunctions.MaxValue;

				return paymentRenewalDate;
			}
			set
			{
                if (value < DateTimeFunctions.MinValue)
                    value = DateTimeFunctions.MinValue;

                if (value > DateTimeFunctions.MaxValue)
                    value = DateTimeFunctions.MaxValue;

				paymentRenewalDate = value;
			}
		}
	}

	//=========================================================================
	[Serializable]
	public class LocalizationBag : ISerializable, ILocalizationBag
	{
		private string key = string.Empty;
		private string productName = string.Empty;
		private string userEmail = string.Empty;
		private	string days = string.Empty;
		private long renewalPeriodTicks;

		//---------------------------------------------------------------------
		public string Key
		{
			get
			{
				return key;
			}
			set
			{
				key = value;
			}
		}
		//---------------------------------------------------------------------
		public string ProductName
		{
			get
			{
				return productName;
			}
			set
			{
				productName = value;
			}
		}
		//---------------------------------------------------------------------
		public string UserEmail
		{
			get
			{
				return userEmail;
			}
			set
			{
				userEmail = value;
			}
		}

		//---------------------------------------------------------------------
		public string Days
		{
			get
			{
				return days;
			}
			set
			{
				days = value;
			}
		}

		//---------------------------------------------------------------------
		public long RenewalPeriodTicks
		{
			get
			{
				return renewalPeriodTicks;
			}
			set
			{
				renewalPeriodTicks = value;
			}
		}

		//---------------------------------------------------------------------
		public LocalizationBag() {}  // DO NOT REMOVE! Needed for serialization

		//---------------------------------------------------------------------
		public LocalizationBag(string key, string productName, string userEmail, string days, long renewalPeriodTicks)
		{
			this.key		= key;
			this.productName= productName;
			this.userEmail	= userEmail;
			this.days		= days;
			this.renewalPeriodTicks = renewalPeriodTicks;
		}

		//---------------------------------------------------------------------
		protected LocalizationBag(SerializationInfo info, StreamingContext context)
		{
			try
			{this.userEmail = info.GetString(userEmailName);}
			catch (SerializationException)
			{this.userEmail = string.Empty;}

			try
			{this.productName = info.GetString(productNameName);}
			catch (SerializationException)
			{this.productName = string.Empty;}

			try
			{this.key = info.GetString(keyName);}
			catch (SerializationException)
			{this.key = string.Empty;}

			try
			{this.days = info.GetString(daysName);}
			catch (SerializationException)
			{this.days = string.Empty;}
			try
			{this.renewalPeriodTicks = info.GetInt64(renewalPeriodTicksName);}
			catch (SerializationException)
			{this.renewalPeriodTicks = new TimeSpan(90,0,0,0).Ticks;}//default tre mesi
		}

		#region ISerializable Members

		//---------------------------------------------------------------------
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue(userEmailName, userEmail);
			info.AddValue(productNameName, productName);
			info.AddValue(keyName, key);
			info.AddValue(daysName, days);
			info.AddValue(renewalPeriodTicksName, renewalPeriodTicks);
		}

		private const string userEmailName	= "UserEmail";
		private const string productNameName= "ProductName";
		private const string keyName		= "Key";
		private const string daysName		= "Days";
		private const string renewalPeriodTicksName = "RenewalPeriodTicks";

		#endregion
	}

	//=========================================================================
	[Serializable]
	public class SerialNumberBag : State
	{
		public const int ProtocolVersion = Parameters.ProtocolVersion;

		private const string LoginTag = "Login";
		private const string PasswordTag = "Password";
		private const string SolutionFileNameTag = "SolutionFileNAme";
		private const string DummyCharsTag = "DummyChars";
		private const string CountryTag = "Country";
		private const string OrderedQtyTag = "OrderedQty";
		private const string GeneratedSerialNumbersTag = "GeneratedSerialNumbers";

		private string login;
		private string password;
		private string solutionFileName;
		private string dummyChars;
		private string country;
		private int orderedQty;
		private string[] generatedSerialNumbers;

		//---------------------------------------------------------------------
		public string Login
		{
			get { return login; }
			set { login = value; }
		}

		//---------------------------------------------------------------------
		public string Password
		{
			get { return password; }
			set { password = value; }
		}

		//---------------------------------------------------------------------
		public string SolutionFileName
		{
			get { return solutionFileName; }
			set { solutionFileName = value; }
		}

		//---------------------------------------------------------------------
		public string DummyChars
		{
			get { return dummyChars; }
			set { dummyChars = value; }
		}

		//---------------------------------------------------------------------
		public string Country
		{
			get { return country; }
			set { country = value; }
		}

		//---------------------------------------------------------------------
		public int OrderedQty
		{
			get { return orderedQty; }
			set { orderedQty = value; }
		}

		//---------------------------------------------------------------------
		public string[] GeneratedSerialNumbers
		{
			get { return generatedSerialNumbers; }
			set { generatedSerialNumbers = value; }
		}

		//---------------------------------------------------------------------
		public SerialNumberBag() { }  // DO NOT REMOVE! Needed for serialization

		//---------------------------------------------------------------------
		public SerialNumberBag(
			string login,
			string password,
			string solutionFileName,
			string dummyChars,
			string country,
			int orderedQty
			)
		{
			this.login = login;
			this.password = password;
			this.solutionFileName = solutionFileName;
			this.dummyChars = dummyChars;
			this.country = country;
			this.orderedQty = orderedQty;
		}


		//---------------------------------------------------------------------
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue(LoginTag, Login);
			info.AddValue(PasswordTag, password);
			info.AddValue(SolutionFileNameTag, solutionFileName);
			info.AddValue(DummyCharsTag, dummyChars);
			info.AddValue(CountryTag, country);
			info.AddValue(OrderedQtyTag, orderedQty);
			info.AddValue(GeneratedSerialNumbersTag, generatedSerialNumbers);
		}
	}

	#region ModuleNameInfo
	//=========================================================================
	[Serializable]
	public class ModuleNameInfo
	{
		public string	Name;
		public string	LocalizedName;
		public int		CAL;
		public ModuleNameInfo()
		{
			
		}
		public ModuleNameInfo(string name, string localizedName, int cal)
		{
			Name = name;
			LocalizedName = localizedName;
			CAL = cal;
		}
	}

	//=========================================================================
	[Serializable]
	public class CrypterBag : State
	{
		public const int ProtocolVersion = Parameters.ProtocolVersion;

		public string Login;
		public string Password;
		public string Product;
		public string ForcedProduct;
		public string Messages;
		public List<Attachment> Attachments = new List<Attachment>();

		//---------------------------------------------------------------------
		public CrypterBag() {}  // DO NOT REMOVE! Needed for serialization

		//---------------------------------------------------------------------
		public CrypterBag(string login, string password, string product, string forcedProducer)
		{
			this.Login = login;
			this.Password = password;
			this.Product = product;
			this.ForcedProduct = forcedProducer;
		}


		//---------------------------------------------------------------------
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue(LoginName, Login);
			info.AddValue(PasswordNameName, Password);
			info.AddValue(ProductName, Product);
			info.AddValue(ForcedProducerName, ForcedProduct);
			info.AddValue(MessagesName, Messages);
		}	
		private const string LoginName = "Login";
		private const string PasswordNameName = "Password";
		private const string ProductName = "Product";
		private const string ForcedProducerName = "ForcedProducer";
		private const string MessagesName = "Messages";
	}
	#endregion
}
