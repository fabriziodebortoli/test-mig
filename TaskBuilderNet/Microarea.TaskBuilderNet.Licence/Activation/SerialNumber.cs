using System;
using System.Collections;
using System.Globalization;
using System.Text;
using Microarea.TaskBuilderNet.Licence.Activation.Components;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Licence.Activation
{
	#region Class SerialNumber

	//=========================================================================
	[Serializable]
    public enum CalTypeEnum { None, Master, MasterNew, Server, ServerNew, Auto, TPGate, TPGateNew, AutoFunctional, WmsMobile, Null, AutoDemo, AutoTbs, tbf }//default none

	[Serializable]
	public enum CalUseEnum { Unnamed, Named, Function, tbf }//default unnamed

    [Serializable]
    public enum ModuleModeEnum { Default, Back, NoDemo, PhasedOut, NDB, NDB_PhasedOut, DVLPPlus }//default Default

	/// <summary>
	/// Incapsula le funzionalitá di un serial number.
	/// </summary>
	/// <remarks>
	/// Con l'uscita della versione 2.0 di Mago.Net e` cambiata la codifica del
	/// serial number e di conseguenza anche questa classe per gestire i
	/// cambiamenti e mantenere compatibilita` con il pregresso e` stata
	/// modificata.
	/// In particolare si e` deciso di leggere il progressivo come fosse una
	/// cifra in base 36 e non piu` in base 10.
	/// Per evitare conflitti tra  numeri di serie con vecchia e nuova codifica
	/// e` stato guardato il numero di serie col progressivo piu` alto, ci si e`
	/// posti al di sopra di tale valore in maniera ragionevole e si e` stabilito un
	/// valore per il contatore del numero progressivo da cui tutti i nuovi
	/// numeri di serie sarebbero partiti.
	/// In soldoni cio` vuol dire che al momento della transizione il
	/// progressivo piu` alto era a 4600 circa, quindi si e` scelto 5000 come
	/// soglia tra vecchia e nuova codifica.
	/// 5000 in base 36 e` espresso come 233280, per cui al momento della
	/// transizione sono stati spostati tutti i progressivi a 233281.
	/// Il risultato di tutto questo discorso e` che questa classe e` in grado
	/// di garantire un comportamento coerente per tutti i progressivi minori
	/// di 5000 e maggiori di 233281, mentre il comportamento non e` garantito
	/// essere coerente con tutti i valori intermedi (anche perche` i valori
	/// intermedi non dovrebbero esistere).
	/// I valori progressivi tra 5000 e 99999 danno luogo a numeri di serie con
	/// vecchia o nuova codifica a seconda del costruttore che si invoca, i valori
	/// tra 100000 e 233280 danno eccezione perche` di sei caratteri mentre il
	/// massimo con la vecchia codifica progressivo ammesso e` di 5 caratteri.
	/// Quanto appena esposto vale per le nazioni per cui esistono serial
	/// number con vecchia e nuova codifica.
	/// </remarks>
	//=========================================================================
	public class SerialNumber : IComparable, IComparable<SerialNumber>, IEquatable<SerialNumber>
	{
		// Lista di countries per cui mantenere la divisione tra vecchia e
		// nuova codifica dei serial number.
		// Le nazioni non elencate qui avranno solo serial number con
		// nuova codifica.
		private	const	string		oldSerialNumberFormatCountries = "BG;CH;CS;GR;HU;IT;PL;RO;US";

		//CAL management
		const string CalPlaceHolderStandard			= "PL";
		const string CalPlaceHolderConcurrent		= "FL";
		public const string WebAccessCalPlaceHolder	= "WS";
		const string CalPlaceHolderMagicDocs		= "MD";
		const string CalPlaceHolderMagicDocOffice	= "MO";
		const string CalPlaceHolderEducational		= "ED";
		const string UnlimitedPlaceHolder			= "IL";
		const string EasyLookCalPlaceHolder			= "C";
		const string EasyLookDBCalPlaceHolder		= "E";
		const string PureOfficePlaceHolder			= "OFFI";
       internal const string WMSMobilePlaceHolder = "WMRC";
       internal const string ManufacturingMobilePlaceHolder = "MANC";
		public const string OraclePlaceHolder		= "OR";
		const string MSDBPlaceHolder				= "MS";	
		const string MSDBSTPlaceHolder				= "ST";
		const string MSDBENPlaceHolder				= "EN";	
        //getione sql 2012
        const string SQL12EmbPlaceHolder= "S2";	
        const string SQL12WgStPlaceHolder= "W2";
        const string SQL12StStPlaceHolder = "U2";
        const string SQL14EmbPlaceHolder = "S4";
        const string SQL14StStPlaceHolder = "U4";


        public const int UnlimitedCalNumber		= Int32.MaxValue;

		// Lunghezza dei campi nella stringa che rappresenta un serial number in chiaro.
		public	const	int			SerialNumberLength	= 16;
		private	const	int			progrNumLength		= 5;
		private	const	int			prodCodLength		= 2;
		private	const	int			editionLength		= 1;
		private	const	int			moduleLength		= 4;
		private	const	int			dbLength			= 1;
		private	const	int			opSysLength			= 1;
		private	const	int			countryLength		= 2;

		private	const	int			countryPos			= 9;
		private	const	int			progrNumPos			= 11;

		// Posizione e lunghezza del crc nel serial number.
		private	const	int			crcPos = 0;
		private	const	int			crcLen = 1;

		// Massima quantita' ammessa per il progressivo.
		private	const	int			maxProgrNum = 60466175;// = 36^5 - 1

		// Soglia oltre la quale si generano numeri di serie con la codifica per la 2.0.
		private	const	int			oldNewThreshold = 233280;// = 5000 in base 36.

		// Mappa traduzione coppia database-sistema operativo.
		private	static	Hashtable	db_osMap;

		private	string				product;
		private	Edition				edition;
		private	string				module;
		private	string				namedcal;
		private string concurrentcal;
		private	string				rawData;
		private	DatabaseVersion		database;
		private Microarea.TaskBuilderNet.Interfaces.OperatingSystem operatingSystem;
		private	string				country;
		private	int					progressiveNumber;
		private	string				plainValue;
		private	string				encryptedValue;
		private	bool				isWebCal = false;
		private	ProductCode			productCode;

		// Chiave che memorizza il produttore che si "accaparra" il serial number
		// Serve solo a gestire MagicDocument Platform con le relative CAL
		private string				producerKey;

		// Classe del serial number: serve per gestire la politica di licensing
		// dalla 2.9/3.0 in avanti.
		private DBNetworkType dbNetworkType;

		#region Public Properties

		public		string		Product				{ get { return	product; } }
		public		Edition		Edition				{ get { return	edition; } }
		public		string		Module				{ get {	return	module; } }
		public string NamedCAL { get { return namedcal; } }
		public string ConcurrentCAL { get { return concurrentcal; } }
		public		string		RawData				{ get { return	rawData; } }
		public		DatabaseVersion		Database			{ get { return	database; } }
		public Microarea.TaskBuilderNet.Interfaces.OperatingSystem OperatingSystem		{ get { return	operatingSystem; } }
		public		string		Country				{ get { return	country; } }
		public		int			ProgressiveNumber	{ get { return	progressiveNumber; } }
		public		string		CRCValue			{ get { return	IsNewPattern ? plainValue.Substring(crcPos, crcLen) : ""; } }
		public		bool		IsWebCal			{ get { return	isWebCal; } }

		/// <summary>
		/// Valore dell'istanza attuale di serial number.
		/// </summary>
		public		string		Value		{ get { return encryptedValue; } }

		/// <summary>
		/// Valore dell'istanza attuale di serial number espressa in chiaro.
		/// </summary>
		public		string		PlainValue	{ get { return plainValue; } }

		/// <summary>
		/// Ritorna il codice prodotto di Microarea da cui e' stato generato
		/// il serial number rappresentato dall'istanza di questa classe.
		/// </summary>
		public	ProductCode	ProductCode		{ get { return productCode; } }

		/// <summary>
		/// Ritorna true se la corrente istanza della classe ha il
		/// valore corretto per il CRC o se è di un vecchio tipo, 
		/// false altrimenti.
		/// </summary>
		public	bool	HasCorrectCrc		{ get { return !IsNewPattern || SerialNumber.IsCrcCorrect(encryptedValue); } }

		/// <summary>
		/// Ritorna true se l'istanza attuale e' espressa secondo la codifica
		/// uscita con la 2.0 di Mago.Net, false altrimenti.
		/// </summary>
		public	bool	IsNewPattern		{ get { return ((!WantsOldFormat(country)) || (progressiveNumber > oldNewThreshold)); } }

		/// <summary>
		/// Gestisce il 'ProducerKey' per MagicDocument Platform.
		/// </summary>
		public string	ProducerKey
		{
			get
			{
				return producerKey;
			}
			set
			{
				if (value == null)
					value = string.Empty;

				producerKey = value;
			}
		}

		/// <summary>
		/// Ritorna la classe che descrive il licensing applicabile al sales module.
		/// </summary>
		/// <remarks>
		/// Descrive la nuova politica di licensing dei moduli del prodotto a partire
		/// dalla coppia di versioni 2.9/3.0 in avanti.
		/// Si divide (al 16/07/2007) in <code>SmallNetworks</code> (per versioni del
		/// prodotto che girano su MSDE/SqlExpress 2005 con database fino a 2GB) e
		/// <code>LargeNetwork</code> (versione di database libera tra tutte quelle
		/// supportate e nessun limite di crescita del database).
		/// </remarks>
		public DBNetworkType DBNetworkType { get { return dbNetworkType; } }

		#endregion

		#region Constructors

		/// <summary>
		/// Istanzia un oggetto di tipo SerialNumber non valorizzando le
		/// proprietá "Module" e "CAL".
		/// L'informazione riguardo tali campi, che deve comunque essere
		/// passata al costruttore (parametro "mod_cal"), viene memorizzata
		/// in un campo interno ed esposta mediante la proprietá "RawData"
		/// a cui non é associata alcuna semantica ma memorizza
		/// esclusivamente i quattro caratteri della parte di serial number.
		/// </summary>
		/// <exception cref="SerialNumberFormatException">
		/// Lanciata se uno qualunque dei parametri non ha la lunghezza
		/// corretta o se 'progressivenum' e' maggiore di 36^5.
		/// </exception>
		/// <param name="prod">Codice identificativo del prodotto.</param>
		/// <param name="ed">Edition.</param>
		/// <param name="mod_cal">Nome del modulo o numero di postazioni di
		/// client access license.</param>
		/// <param name="db">Database supportato.</param>
		/// <param name="opSys">Sistema operativo supportato.</param>
		/// <param name="countryname">Country in cui é stato licenziato
		/// il prodotto.</param>
		/// <param name="progressivenum">Numero progressivo per il modulo.</param>
		//---------------------------------------------------------------------
		public SerialNumber(
			string prod,
			string ed,
			string mod_cal,
			string db,
			string opSys,
			string countryname,
			int progressivenum
			)
			: this(prod, ed, mod_cal, db, opSys, countryname, progressivenum, CalTypeEnum.Null)
		{}
		
		/// <summary>
		/// Istanzia un oggetto di tipo SerialNumber valorizzando tutti i
		/// campi di cui é composto e in particolare il campo 'Module' o
		/// il campo 'CAL' a seconda del valore del flag 'isModuleWithCAL'.
		/// </summary>
		/// <exception cref="SerialNumberFormatException">
		/// Lanciata se uno qualunque dei parametri non ha la lunghezza
		/// corretta o se 'progressivenum' e' maggiore di 36^5.
		/// </exception>
		/// <param name="prod">Codice identificativo del prodotto.</param>
		/// <param name="ed">Edition.</param>
		/// <param name="mod_cal">Nome del modulo o numero di postazioni
		/// di client access license.</param>
		/// <param name="db">Database supportato.</param>
		/// <param name="opSys">Sistema operativo supportato.</param>
		/// <param name="countryname">Country in cui é stato licenziato
		/// il prodotto.</param>
		/// <param name="progressivenum">Numero progressivo per il modulo.</param>
		//---------------------------------------------------------------------
		public SerialNumber(
			string prod,
			string ed,
			string mod_cal,
			string db,
			string opSys,
			string countryname,
			int progressivenum,
			CalTypeEnum caltype
			)
		{
			if (db_osMap == null)
				InitDbOsMap();

			producerKey = string.Empty;

			if (prod == null || prod.Length != prodCodLength)
				throw new SerialNumberFormatException("Invalid lenght of " + prod);
			else product = prod.ToUpperInvariant();

			if (ed == null || ed.Length != editionLength)
				throw new SerialNumberFormatException("Invalid lenght of " + ed);
			else edition = GetEditionFromString(ed);

			if (mod_cal == null || mod_cal.Length != moduleLength)
				throw new SerialNumberFormatException("Invalid lenght of " + mod_cal);
			else rawData = mod_cal.ToUpperInvariant();

			if (db == null || db.Length != dbLength)
				throw new SerialNumberFormatException("Invalid lenght of " + db);
			else database = GetDatabaseVersionFromString(db);

			if (opSys == null || opSys.Length != opSysLength)
				throw new SerialNumberFormatException("Invalid lenght of " + opSys);
			else operatingSystem = GetOperatingSystemFromString(opSys);

			if (countryname == null || countryname.Length != countryLength)
				throw new SerialNumberFormatException("Invalid lenght of " + countryname);
			else country = countryname.ToUpperInvariant();

			if (maxProgrNum < progressivenum)
				throw new SerialNumberFormatException("Progressive number too big");
			else progressiveNumber = progressivenum;

			if (caltype.Equals(CalTypeEnum.Null))
				InitializeModuleCALProperty();
			else
				InitializeModuleCALProperty(rawData, caltype);

			this.dbNetworkType = InitDBNetworkType(this.database);

			if (WantsOldFormat(country) && (progressiveNumber <= oldNewThreshold))
			{
				StringBuilder pnBuilder = new StringBuilder(progressiveNumber.ToString(CultureInfo.InvariantCulture));
				if (pnBuilder.Length > progrNumLength)
					throw new SerialNumberFormatException("Progressive number too big");
				while (pnBuilder.Length < progrNumLength)
					pnBuilder.Insert(0, "0");

				plainValue		=	String.Format(
					CultureInfo.InvariantCulture,
					"{0}{1}{2}{3}{4}{5}{6}",
					product,
					ed,
					rawData,
					db,
					opSys,
					country,
					pnBuilder.ToString()
					).ToUpperInvariant();
			}
			else
			{
				plainValue		=	String.Format(
					CultureInfo.InvariantCulture,
					"{0}{1}{2}{3}{4}{5}",
					product,
					ed,
					rawData,
					GetDbOsValue(db, opSys),
					country,
					GetFormattedProgressiveNumber(progressiveNumber)
					).ToUpperInvariant();
				plainValue		=	CrcServiceProvider.ComputeCrc(plainValue);
			}
			

			encryptedValue	=	Crypt(plainValue);

			InitializeProductCode();
		}

		/// <summary>
		/// Istanzia un oggetto di tipo SerialNumber non valorizzando le
		/// proprietá "Module" e "CAL".
		/// L'informazione riguardo tali campi viene memorizzata in un campo
		/// interno ed esposta mediante la proprietá "RawData" a cui non é
		/// associata alcuna semantica ma memorizza esclusivamente i quattro
		/// caratteri della parte di serial number.
		/// </summary>
		//---------------------------------------------------------------------
		public SerialNumber(string serialnumber)
			: this(serialnumber, CalTypeEnum.Null)
		{}

		/// <summary>
		/// Istanzia un oggetto di tipo Serial Number a partire da una stringa
		/// in ingresso contenente il valore del serial number criptato.
		/// </summary>
		/// <param name="isModuleWithCAL">
		/// true se "serialnumber" é un modulo che esprime anche CAL, false
		/// altrimenti.
		/// </param>
		//---------------------------------------------------------------------
		public SerialNumber(string serialnumber, CalTypeEnum caltype)
		{
			producerKey = string.Empty;

			InitializeInnerProperties(serialnumber);

			this.dbNetworkType = InitDBNetworkType(this.database);

			if (caltype.Equals(CalTypeEnum.Null))
				InitializeModuleCALProperty();
			else
				InitializeModuleCALProperty(rawData, caltype);

			InitializeProductCode();
		}

		#endregion

		#region Private Methods

		//---------------------------------------------------------------------
		private static bool WantsOldFormat(string country)
		{
			if (country == null)
				throw new ArgumentNullException("country", "null is not allowed.");
			if (country.Length != 2)
				throw new ArgumentException("Invalid 'country'", "country");

			return (oldSerialNumberFormatCountries.IndexOf(country.ToUpperInvariant()) != -1);
		}

		//---------------------------------------------------------------------
		public static bool IsMeaningLess(string module, bool powerProducer)
		{
			return (
				String.Compare(module, PureOfficePlaceHolder, true, CultureInfo.InvariantCulture) == 0 ||
				IsDbPlaceHolder(module, powerProducer) || 
                IsSql2012PlaceHolder(module, powerProducer)
				);
		}

		//---------------------------------------------------------------------
		public static bool IsIntegrativeSerial(string module, bool powerProducer)
		{
			return (string.Compare(module,  Licence.XmlSyntax.Consts.TestID, true, CultureInfo.InvariantCulture) == 0 ||
					 string.Compare(module, Licence.XmlSyntax.Consts.BackUpID, true, CultureInfo.InvariantCulture) == 0 ||
					 string.Compare(module, Licence.XmlSyntax.Consts.StandAloneID, true, CultureInfo.InvariantCulture) == 0);
		}

		
		//---------------------------------------------------------------------
        internal static bool IsDbPlaceHolder(string module, bool powerProducer)
		{
			if (module == null || module.Length!= 2 || !powerProducer)
				return false;

			return (IsDbPlaceHolder(module) || IsSql2012PlaceHolder(module));
		}

        //---------------------------------------------------------------------
        internal static bool IsDbPlaceHolderOLD(string module, bool powerProducer)
		{
			if (module == null || module.Length!= 2 || !powerProducer)
				return false;

			return (IsDbPlaceHolder(module) );
		}
        //---------------------------------------------------------------------
        internal static bool IsSql2012PlaceHolder(string module, bool powerProducer)
        {
            if (module == null || module.Length != 2 || !powerProducer)
                return false;

            return (IsSql2012PlaceHolder(module));
        }


        //---------------------------------------------------------------------
		public static bool IsDbPlaceHolder(string module)
		{          
			return  (string.Compare(module, OraclePlaceHolder, true, CultureInfo.InvariantCulture) == 0  ||
					 string.Compare(module, MSDBPlaceHolder,   true, CultureInfo.InvariantCulture) == 0	 ||
					 string.Compare(module, MSDBSTPlaceHolder, true, CultureInfo.InvariantCulture) == 0  ||
					 string.Compare(module, MSDBENPlaceHolder, true, CultureInfo.InvariantCulture) == 0);
		}

        //---------------------------------------------------------------------
        public static bool IsSql2012PlaceHolder(string module)
        {
            return ( string.Compare(module, SQL12EmbPlaceHolder, true, CultureInfo.InvariantCulture) == 0 ||
                     string.Compare(module, SQL14EmbPlaceHolder, true, CultureInfo.InvariantCulture) == 0 ||
                     string.Compare(module, SQL14StStPlaceHolder, true, CultureInfo.InvariantCulture) == 0 ||
                     string.Compare(module, SQL12WgStPlaceHolder, true, CultureInfo.InvariantCulture) == 0 ||
                     string.Compare(module, SQL12StStPlaceHolder, true, CultureInfo.InvariantCulture) == 0);
        }

		//---------------------------------------------------------------------
		private static void InitDbOsMap()
		{
			if (db_osMap == null)
			{
				db_osMap = new Hashtable();

				db_osMap.Add("SW", "A");
				db_osMap.Add("OW", "B");
				db_osMap.Add("MW", "C");
				db_osMap.Add("AW", "D");
				db_osMap.Add("NW", "E");
			}
		}

		//---------------------------------------------------------------------
		private string GetDbOsValue(string db, string os)
		{
			object val = db_osMap[String.Format(CultureInfo.InvariantCulture, "{0}{1}", db.ToUpperInvariant(), os.ToUpperInvariant())];

			return (val == null) ? db : val.ToString();
		}

		//---------------------------------------------------------------------
		private object[] GetDbOsKey(string val)
		{
			object[] aVals = new object[2];
			// Default values.
			aVals[0] = DatabaseVersion.Undefined;//database
			aVals[1] = Microarea.TaskBuilderNet.Interfaces.OperatingSystem.Undefined;//operating system

			foreach (string s in db_osMap.Keys)
			{
				if (String.Compare(db_osMap[s].ToString(), val, true, CultureInfo.InvariantCulture) == 0)
				{
					aVals[0] = GetDatabaseVersionFromString(s[0].ToString());//database
					aVals[1] = GetOperatingSystemFromString(s[1].ToString());//operating system
					break;
				}
			}

			return aVals;
		}


		//---------------------------------------------------------------------
		private static string GetFormattedProgressiveNumber(int number)
		{
			StringBuilder numbBuilder =
				new StringBuilder(ProgrNumGen.GetBase36Progressive(number));

			while (numbBuilder.Length < progrNumLength)
				numbBuilder.Insert(0, "0");

			return numbBuilder.ToString();
		}

		//---------------------------------------------------------------------
		private void InitializeInnerProperties(string serialnumber)
		{
			if (serialnumber == null || serialnumber.Length != SerialNumberLength)
				throw new ArgumentException("Invalid lenght of parameter \"serialnumber\"");

			if (db_osMap == null)
				InitDbOsMap();

			encryptedValue		= serialnumber.ToUpperInvariant();
			plainValue			= Decrypt(encryptedValue);
			progressiveNumber	=
				ProgrNumGen.GetBase10Progressive(plainValue.Substring(progrNumPos, progrNumLength));

			if (
				WantsOldFormat(plainValue.Substring(countryPos, countryLength)) &&
				(progressiveNumber <= oldNewThreshold)
				)
				InitWithOldFormat();
			else
				InitWithNewFormat();
		}

		//---------------------------------------------------------------------
		private void InitWithNewFormat()
		{
			product				= plainValue.Substring(1, prodCodLength);
			edition				= GetEditionFromString(plainValue.Substring(3, editionLength));
			rawData				= plainValue.Substring(4, moduleLength);
			country				= plainValue.Substring(countryPos, countryLength);
			// ProgressiveNumber value is already set by the caller of this method.

			object[] aVals	= GetDbOsKey(plainValue.Substring(8, dbLength));
			database		= ((DatabaseVersion)aVals[0]);
			operatingSystem	= (Microarea.TaskBuilderNet.Interfaces.OperatingSystem)aVals[1];
		}

		//---------------------------------------------------------------------
		private void InitWithOldFormat()
		{
			product				= plainValue.Substring(0, prodCodLength);
			edition				= GetEditionFromString(plainValue.Substring(2, editionLength));
			rawData				= plainValue.Substring(3, moduleLength);
			database			= GetDatabaseVersionFromString(plainValue.Substring(7, dbLength));
			operatingSystem		= GetOperatingSystemFromString(plainValue.Substring(8, opSysLength));
			country				= plainValue.Substring(countryPos, countryLength);

			// Re-imposta il progressiveNumber sovrascrivendo l'impostazione
			// del metodo chiamante perche' deve essere gestito con la vecchia
			// codifica e quindi interpreta la stringa come fosse gia' in formato
			// decimale.
			try
			{
				progressiveNumber	= Int32.Parse(plainValue.Substring(progrNumPos, progrNumLength), CultureInfo.InvariantCulture);
			}
			catch
			{
				throw new SerialNumberFormatException("Invalid progressive number");
			}
		}

		//---------------------------------------------------------------------
		private void InitializeProductCode()
		{
			string productCodeEdition = GetStringFromEdition(this.edition);
			if (productCodeEdition == null || productCodeEdition.Length == 0)
				return;

			string productCodeDatabase = GetStringFromDatabaseVersion(this.database, this);
			if (productCodeDatabase == null || productCodeDatabase.Length == 0)
				return;

			string productOperatingSystem = GetStringFromOperatingSystem(this.operatingSystem);
			if (productOperatingSystem == null || productOperatingSystem.Length == 0)
				return;

			switch(this.Edition)
			{
				case Edition.Enterprise:    productCodeEdition = "ENT"; break;
				case Edition.Standard:      productCodeEdition = "STD"; break;
				case Edition.Professional:  productCodeEdition = "PRO"; break;
                case Edition.EditionA:      productCodeEdition = "EDA"; break;
                case Edition.EditionB:      productCodeEdition = "EDB"; break;
                case Edition.EditionC:      productCodeEdition = "EDC"; break;
            }

            switch (this.Database)
			{
				case DatabaseVersion.All:
				{
					if (String.Compare(productCodeDatabase, "N", true, CultureInfo.InvariantCulture) == 0)
						productCodeDatabase = "NDB";
					else
						productCodeDatabase = "ALL";
					break;
				}
				case DatabaseVersion.Ndb: productCodeDatabase = "NDB";break;
				case DatabaseVersion.SqlServer2000: productCodeDatabase = "SQL";break;
				case DatabaseVersion.Oracle: productCodeDatabase = "ORA";break;
				case DatabaseVersion.MSDE: productCodeDatabase = "MSD";break;
//				default : throw new SerialNumberFormatException("Database not recognized.");
			}

			switch(this.operatingSystem)
			{
				case Microarea.TaskBuilderNet.Interfaces.OperatingSystem.Windows: productOperatingSystem = "W";break;
//				default : throw new SerialNumberFormatException("Operating system not recognized.");
			}

			productCode = new ProductCode(
				"SFT",
				this.Product,
				productCodeEdition,
				this.RawData,
				productCodeDatabase,
				productOperatingSystem
				);
		}

		//---------------------------------------------------------------------
		private void InitializeModuleCALProperty()
		{
			module	 = "";
			namedcal = "";
		}

		/// <summary>
		/// Valorizza il numero di cal per il serial in questione 
		/// in base ai vari codici prodotto esistenti.
        /// Questo metodo è il frutto di anni di modifiche ed aggiunte alle politiche commerciali 
        /// e comincio a pensare che abbia imparato ad automodificarsi a mia insaputa.
        /// Una qualsiasi modifica a questo codice potrebbe comportare side-effects impensabili.
        ///  Lasciate ogni speranza o voi che entrate!
		/// </summary>
		//---------------------------------------------------------------------
		private void InitializeModuleCALProperty(string parameter, CalTypeEnum caltype)
		{
			if (parameter == null || parameter.Length != 4 /*|| caltype == CalTypeEnum.None*/)
			{
				module = namedcal = string.Empty;	
				return;
			}

			string	temp		 = parameter.ToUpperInvariant();
			int		calnumber	 = -1;
			bool concurrent = false;
			
			try
			{
				if (caltype == CalTypeEnum.None)
				return;

				bool	unlimited	 = false;
				bool	threeCalChar = false;
				bool	twoCalChar	 = false;

				string firstOne	= temp.Substring(0, 1);
				string firstTwo	= temp.Substring(0, 2);
				twoCalChar		= char.IsDigit(temp, 2) && char.IsDigit(temp, 3);
				threeCalChar	= char.IsDigit(temp, 1) && twoCalChar;

				string number	= (threeCalChar) ? temp.Substring(1) : temp.Substring(2);
				calnumber		= twoCalChar ? Convert.ToInt32(number, CultureInfo.InvariantCulture) : -1;
				concurrent = String.Compare(firstTwo, CalPlaceHolderConcurrent, true, CultureInfo.InvariantCulture) == 0;

                //wms mobile
                //Nel caso di modulo wms mobile che ha le sue cal (WMRC) e il modulo server (WMRF)
                if (caltype == CalTypeEnum.WmsMobile)
                {
                    if (IsSql2012PlaceHolder(firstTwo, true))
                    {
                        module = firstTwo;
                        calnumber = -1;
                    }
                    if (calnumber == -1 && String.Compare(temp, WMSMobilePlaceHolder, true, CultureInfo.InvariantCulture) == 0)
                    //valorizzo sia cal (1 di default) e modulo (shortname)
                    {
                        module = temp;
                        calnumber = 1;
                    }
                   
                }
                //manufacturing mobile
                //Nel caso di modulo wms mobile che ha le sue cal (WMRC) e il modulo server (WMRF)
                if (caltype == CalTypeEnum.WmsMobile)
                {
                    if (IsSql2012PlaceHolder(firstTwo, true))
                    {
                        module = firstTwo;
                        calnumber = -1;
                    }
                    if (calnumber == -1 && String.Compare(temp,ManufacturingMobilePlaceHolder, true, CultureInfo.InvariantCulture) == 0)
                    //valorizzo sia cal (1 di default) e modulo (shortname)
                    {
                        module = temp;
                        calnumber = 1;
                    }
                }


				//auto
				//Nel caso di moduli auto ci sono dei serial number 
				//che valgono comunque 1 altri che contano cal 
				//a seconda del numero indicato 
				if (caltype == CalTypeEnum.Auto )
				{
					if (calnumber == -1)
					{
						//valorizzo sia cal (1 di default) e modulo (shortname)
						module		= temp;
						calnumber	= 1;
					}
					else
					{
						//se cal valorizzata allora valorizzo il modulo(shortname)
						module	= threeCalChar ? firstOne : firstTwo;
					}
				}
                    //autodemo
				//Nel caso di moduli autodemo ci sono dei serial number 
				//che valgono demo, sono dmxx dove xx è il numero di cal
                //i moduli Demo permettono di avere il prodotto in demo e cioè di poter selezionare tutti i moduli inserendo un solo serial /
				if (caltype == CalTypeEnum.AutoDemo)
				{
                    if (firstTwo == "DM" && calnumber>-1)
						module		= "DEMO";

					else//come modalità auto
					{
                        if (calnumber == -1)
                        {
                            //valorizzo sia cal (1 di default) e modulo (shortname)
                            module = temp;
                            calnumber = 1;
                        }
                        else
                        {
                            //se cal valorizzata allora valorizzo il modulo(shortname)
                            module = threeCalChar ? firstOne : firstTwo;
                        }
					}
				}
				else if (caltype == CalTypeEnum.AutoFunctional || caltype == CalTypeEnum.AutoTbs)
				{
                    bool sql2012 = IsSql2012PlaceHolder(firstTwo, true);

					concurrent = true;
					if (String.Compare(firstTwo, WebAccessCalPlaceHolder, true, CultureInfo.InvariantCulture)	 == 0)
					{
						//cal web di terze parti	
						bool illimitedTermination = String.Compare(number, UnlimitedPlaceHolder, true, CultureInfo.InvariantCulture) == 0;
						unlimited = illimitedTermination ;
						if (unlimited)
							calnumber = UnlimitedCalNumber;
						module		= "";
						isWebCal	= (calnumber != -1);
					}
                        
                    else if (IsDbPlaceHolder(firstTwo, true) || sql2012)
					{
						module		= "";
						calnumber	= 0;
					}

                    if (IsIntegrativeSerial(temp, true))
                    {
                        module = temp;
                        calnumber = 0;
                    }
						//deve fare come auto, ma non per en-ms ws-
					else if (calnumber == -1)
					{
						//valorizzo sia cal (1 di default) e modulo (shortname)
						module		= temp;
						calnumber	= 1;
					}
					else
					{
						//se cal valorizzata allora valorizzo il modulo(shortname)
						module	= threeCalChar ? firstOne : firstTwo;
                        if (sql2012) module = firstTwo;// lo so, fa schifo, ma serve così perchè i seriali di sql 2012 son del tipo U201 U205 o S201 o W201 etc etc
					}
				}
				else if (caltype == CalTypeEnum.Master || caltype == CalTypeEnum.MasterNew)
				{
					if (calnumber != -1)
					{
						//se è uno dei tipo elencati qua sotto ed ho già valorizzato le cal svuoto il modulo
						//inoltre se è di tipo WSxx la cal è web
						if (String.Compare(firstTwo, CalPlaceHolderStandard, true, CultureInfo.InvariantCulture) == 0		||
							String.Compare(firstTwo, CalPlaceHolderConcurrent, true, CultureInfo.InvariantCulture) == 0 ||
							String.Compare(firstTwo, CalPlaceHolderEducational, true, CultureInfo.InvariantCulture) == 0)		
							module	= "";

						if (String.Compare(firstTwo, WebAccessCalPlaceHolder, true, CultureInfo.InvariantCulture)	 == 0)
							isWebCal	= true;
						//se è un serial marcatore di cal di db....MS01, OR01, ST01, EN01, e sql2012 (U2xx, W2xx, S2xx)
                        if ((SerialNumber.IsDbPlaceHolder(firstTwo) || SerialNumber.IsSql2012PlaceHolder(firstTwo)) && calnumber != -1)
						{
                            calnumber = -1;
							//seriali di cal emb di db non devon valere cal e come short name devono avere solo i primi due caratteri.
							module = temp = firstTwo;
						}
						
					}

					if (caltype == CalTypeEnum.MasterNew)
					{
						if (String.Compare(firstOne, EasyLookCalPlaceHolder, true, CultureInfo.InvariantCulture)	 == 0)
						{
							isWebCal = true;
						}
					}
					
				}
				else if (caltype == CalTypeEnum.TPGateNew || caltype == CalTypeEnum.ServerNew)
				{
					//se sono cal , rimaste da versioni vecchie, non deve considerarle.
					if (twoCalChar
						&&
						(String.Compare(firstTwo, WebAccessCalPlaceHolder,   true, CultureInfo.InvariantCulture) == 0 ||
						String.Compare(firstTwo, CalPlaceHolderMagicDocs, true, CultureInfo.InvariantCulture) == 0 ))
						module = String.Empty;
					else
						module	= temp;
					calnumber = UnlimitedCalNumber;
					isWebCal = true;
					
				}
				else if (caltype == CalTypeEnum.Server || caltype == CalTypeEnum.TPGate)
				{
					//NON ESISTONO PIÙ
					//FORSE INVECE SI PER LA NUOVA MAGICDOCSPLATFORM
					//IL MIO CODICE È COME IL MAIALE: NON SI BUTTA VIA NIENTE! ila.
					//la versione a cal illimitate prevista solo per modalità a cal separate, cioè server e master e TPGate
					bool illimitedTermination = String.Compare(number, UnlimitedPlaceHolder, true, CultureInfo.InvariantCulture) == 0;
					unlimited = illimitedTermination && (caltype == CalTypeEnum.Server || caltype == CalTypeEnum.Master || caltype == CalTypeEnum.TPGate);
					if (unlimited)
						calnumber = UnlimitedCalNumber;

					//se è del tipo Cxxx o Exxx (EasyLook) o MDxx o MOxx (MagicDocs) 
					//allora è cal-web 
					//con numero di cal specificate dalle ultimedue o tre cifre
				
					if (
						(caltype == CalTypeEnum.Server && 
						(String.Compare(firstOne, EasyLookCalPlaceHolder,		true, CultureInfo.InvariantCulture)	 == 0	|| 
						String.Compare(firstOne, EasyLookDBCalPlaceHolder,		true, CultureInfo.InvariantCulture)	 == 0	||
						String.Compare(firstTwo, CalPlaceHolderMagicDocOffice,  true, CultureInfo.InvariantCulture)	 == 0	||
						String.Compare(firstTwo, CalPlaceHolderMagicDocs,		true, CultureInfo.InvariantCulture)	 == 0))	||
						(caltype == CalTypeEnum.TPGate && String.Compare(firstTwo, WebAccessCalPlaceHolder, true, CultureInfo.InvariantCulture)	 == 0)
						)
					{
						module		= "";
						isWebCal	= (calnumber != -1);
					}
				}				
			}
			catch
			{
				calnumber	= -1;
			}
			finally
			{
				if (calnumber	== -1)
					module	= temp;
				//valorizzo la cal come stringa
				if (concurrent)
				{
					concurrentcal = (calnumber == -1) ? "" : calnumber.ToString(CultureInfo.InvariantCulture);
					namedcal = "";
				}
				else
				{
					namedcal = (calnumber == -1) ? "" : calnumber.ToString(CultureInfo.InvariantCulture);
					concurrentcal = "";
				}
			}
		}

		#endregion

		#region Protected Methods

		//---------------------------------------------------------------------
		protected static string Crypt(string plainSerialNumber)
		{
			return SNCryptoServiceProvider.CryptSN(plainSerialNumber);
		}

		//---------------------------------------------------------------------
		protected static string Decrypt(string cryptedSerialNumber)
		{
			return SNCryptoServiceProvider.DecryptSN(cryptedSerialNumber);
		}

		//---------------------------------------------------------------------
		protected static DBNetworkType InitDBNetworkType(DatabaseVersion database)
		{
			switch (database)
			{
				case DatabaseVersion.MSDE:
					return DBNetworkType.Small;
				case DatabaseVersion.All:
				case DatabaseVersion.Oracle:
				case DatabaseVersion.Ndb:
				case DatabaseVersion.SqlServer2000:
					return DBNetworkType.Large;
				default:
					return DBNetworkType.Undefined;
			}
		}

		#endregion

		#region Public Methods

		///---------------------------------------------------------------------
		public static int GetCalNumber(string module)
		{
			if (module == null || module.Length != 4)
				throw new Exception("invalid module");
			module = module.ToUpperInvariant();
			if (
				module.StartsWith(SerialNumber.CalPlaceHolderStandard) ||
				module.StartsWith(SerialNumber.CalPlaceHolderConcurrent) || 
				module.StartsWith(SerialNumber.WebAccessCalPlaceHolder) || 
				module.StartsWith(SerialNumber.CalPlaceHolderEducational) || 
				module.StartsWith(SerialNumber.CalPlaceHolderMagicDocs) || 
				module.StartsWith(SerialNumber.CalPlaceHolderMagicDocOffice)
				)
				return Int32.Parse(module.Substring(2), CultureInfo.InvariantCulture);
			if (
				module.StartsWith(SerialNumber.EasyLookCalPlaceHolder) || 
				module.StartsWith(SerialNumber.EasyLookDBCalPlaceHolder)
				)
				return Int32.Parse(module.Substring(1), CultureInfo.InvariantCulture);
			return -1;
		}

		//---------------------------------------------------------------------
		public static string GetCalShortName(int calnumber)
		{
			return String.Format
				(CultureInfo.InvariantCulture, 
				"{0}{1}", 
				SerialNumber.CalPlaceHolderStandard, 
				calnumber.ToString("00", CultureInfo.InvariantCulture)
				);
		}

        //---------------------------------------------------------------------
        public static bool IsPLCalNamedShortName(SerialNumber sn)
        {
            string rawData = sn.rawData.ToUpperInvariant();
            return (rawData.StartsWith(SerialNumber.CalPlaceHolderStandard)
                    &&
                   (sn.Module == null || sn.Module.Length == 0));
        }

		//---------------------------------------------------------------------
		public static bool IsCalNamedShortName(SerialNumber sn)
		{
			string rawData = sn.rawData.ToUpperInvariant();
			return (
				rawData.StartsWith(SerialNumber.CalPlaceHolderStandard) || 
				rawData.StartsWith(SerialNumber.WebAccessCalPlaceHolder) ||
				rawData.StartsWith(SerialNumber.CalPlaceHolderEducational)
				)
				&&  
				(sn.Module == null || sn.Module.Length == 0) ;
		}
		
		//---------------------------------------------------------------------
		public static bool IsCalConcurrentShortName(SerialNumber sn)
		{
			string rawData = sn.rawData.ToUpperInvariant();
			return (rawData.StartsWith(SerialNumber.CalPlaceHolderConcurrent))
					&&
					(sn.Module == null || sn.Module.Length == 0);
		}

		/*//---------------------------------------------------------------------
		public static bool IsCalUnNamedShortName(SerialNumber sn)
		{
			string rawData = sn.rawData.ToUpperInvariant();
			return 
				(rawData.StartsWith(SerialNumber.CalPlaceHolderMagicDocs) ||
				rawData.StartsWith(SerialNumber.CalPlaceHolderMagicDocOffice)) &&  
				(sn.Module == null || sn.Module.Length == 0) ;
		}*/

		
		//---------------------------------------------------------------------
		public static bool IsCrcCorrect(string toBeChecked)
		{
			if (toBeChecked == null)
				throw new ArgumentNullException("toBeChecked", "'toBeChecked' cannot be null.");
			if (toBeChecked.Length != SerialNumberLength)
				throw new ArgumentException("Invalid length", "toBeChecked");

			string plain = Decrypt(toBeChecked);
			string progrNum = plain.Substring(progrNumPos, progrNumLength);

			// if it is a number read it as it was a 10-based number.
			// if the 10-based number is greater than oldNewThreshold
			// then it is a new pattern serial number otherwise it is
			// an old pattern serial number.
			bool isCrcCorrect = true;
			int progressiveNumber = ProgrNumGen.GetBase10Progressive(progrNum);
			
			if (progressiveNumber > oldNewThreshold)
				isCrcCorrect = CrcServiceProvider.IsCrcCorrect(
					SNCryptoServiceProvider.DecryptSN(toBeChecked),
					crcPos,
					crcLen
					);
			else
				isCrcCorrect = true;


			return isCrcCorrect;
		}

		//---------------------------------------------------------------------
		public override string ToString()
		{
			return this.Value;
		}


		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			return Equals(obj as SerialNumber);
		}

		//----------------------- Overloading degli operatori -----------------
		public static bool operator == (SerialNumber sn1, SerialNumber sn2) 
		{
			if (Object.ReferenceEquals(null, sn1) &&  Object.ReferenceEquals(null, sn2))
				return true;
			if (Object.ReferenceEquals(null, sn1) ||  Object.ReferenceEquals(null, sn2))
				return false;
			return sn1.Equals(sn2);
		}

		//---------------------------------------------------------------------
		public static bool operator != (SerialNumber sn1, SerialNumber sn2) 
		{
			if (Object.ReferenceEquals(null, sn1) &&  Object.ReferenceEquals(null, sn2))
				return false;
			if (Object.ReferenceEquals(null, sn1) ||  Object.ReferenceEquals(null, sn2))
				return true;
			return !sn1.Equals(sn2);
		}

		//---------------------------------------------------------------------
		public bool IsTheSameType(SerialNumber serial)
		{
			if (serial == null) return false;
			return
				(
				string.Compare(this.Country, serial.Country, true, CultureInfo.InvariantCulture) == 0	&&
				(VeryfyDb(serial)/*this.Database == serial.Database || this.IsDbFree()*/)  	&&
				this.Edition == serial.Edition											                &&
				this.OperatingSystem == serial.OperatingSystem							                &&
				string.Compare(this.Product, serial.Product, true, CultureInfo.InvariantCulture) == 0
				);
		}

		//---------------------------------------------------------------------
		public bool VeryfyDb(SerialNumber serial)
		{
			return this.DBNetworkType == serial.DBNetworkType || this.IsDbFree();
		}

		//---------------------------------------------------------------------
		public bool IsDbFree()
		{
			return (this.database == DatabaseVersion.All || this.database == DatabaseVersion.Ndb);
		}

		//---------------------------------------------------------------------
		public override int GetHashCode()
		{
			 return this.Value.GetHashCode();
		}

		/// <summary>
		/// Ritorna il tipo di sistema operativo associato al serial number.
		/// </summary>
		/// <param name="opSys"></param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		public static Microarea.TaskBuilderNet.Interfaces.OperatingSystem GetOperatingSystemFromString(string opSys)
		{
			return (String.Compare(opSys, "w", true, CultureInfo.InvariantCulture) == 0) ?
                Microarea.TaskBuilderNet.Interfaces.OperatingSystem.Windows
				:
                Microarea.TaskBuilderNet.Interfaces.OperatingSystem.Undefined;
		}

		/// <summary>
		/// Ritorna il tipo di databaseassociato al serial in oggetto.
		/// </summary>
		/// <param name="db">stringa che indica il db</param>
		//---------------------------------------------------------------------
		public static DatabaseVersion GetDatabaseVersionFromString(string db)
		{
//			if (db == null)
//				return DatabaseVersion.Undefined;
//			switch (db.ToLower().Trim())
//			{
//				case "m":
//					return DatabaseVersion.MSDE;
//				case "s":
//				case "d":
//					return DatabaseVersion.SqlServer2000;
//				case "o":
//					return DatabaseVersion.Oracle;
//				case "a":
//					return DatabaseVersion.All;
//				default:
//					return DatabaseVersion.Undefined;
//			}
			// Cambiato per consentire l'offuscazione
			if (db == null)
				return DatabaseVersion.Undefined;

			if (db.ToLower(CultureInfo.InvariantCulture).Trim() == "m")
				return DatabaseVersion.MSDE;
			if (db.ToLower(CultureInfo.InvariantCulture).Trim() == "s")
				return DatabaseVersion.SqlServer2000;
			if (db.ToLower(CultureInfo.InvariantCulture).Trim() == "d")
				return DatabaseVersion.SqlServer2000;
			if (db.ToLower(CultureInfo.InvariantCulture).Trim() == "o")
				return DatabaseVersion.Oracle;
			if (db.ToLower(CultureInfo.InvariantCulture).Trim() == "a")
				return DatabaseVersion.All;
			if (db.ToLower(CultureInfo.InvariantCulture).Trim() == "n")
				return DatabaseVersion.Ndb;

			return DatabaseVersion.Undefined;
		}

		

		/// <summary>
		/// Ritorna una stringa di un carattere contenente la codifica della
		/// <code>DatabaseVersion</code>.
		/// </summary>
		//---------------------------------------------------------------------
		[Obsolete(
			"Use 'GetStringFromDatabaseVersion(DatabaseVersion, SerialNumber)' " +
			"because the result of a call to this method may be inconsistent.",
			 true)]
		public static string GetStringFromDatabaseVersion(DatabaseVersion db)
		{
			switch (db)
			{
				case DatabaseVersion.MSDE:
					return "M";
				case DatabaseVersion.SqlServer2000:
					return "S";
				case DatabaseVersion.Oracle:
					return "O";
				case DatabaseVersion.All:
					return "A";
				case DatabaseVersion.Ndb:
					return "N";
				default:
					return "";
			}
		}

		/// <summary>
		/// Ritorna una stringa di un carattere contenente la codifica della
		/// <code>DatabaseVersion</code> e del tipo di <code>SerialNumber</code>.
		/// </summary>
		//---------------------------------------------------------------------
		public static string GetStringFromDatabaseVersion(
			DatabaseVersion db,
			SerialNumber sn
			)
		{
			switch (db)
			{
				case DatabaseVersion.MSDE:
					return "M";
				case DatabaseVersion.SqlServer2000:
					return "S";
				case DatabaseVersion.Oracle:
					return "O";
				case DatabaseVersion.Ndb:
					return "N";
				case DatabaseVersion.All:
				{
					if (!sn.IsNewPattern)
						return "A";

					if (sn.PlainValue.Substring(8, 1) == "E")
						return "N";
					else
						return "A";
				}
				default:
					return "";
			}
		}

		//---------------------------------------------------------------------
		public static string GetStringFromOperatingSystem(Microarea.TaskBuilderNet.Interfaces.OperatingSystem opSys)
		{
			switch(opSys)
			{
				case Microarea.TaskBuilderNet.Interfaces.OperatingSystem.Windows: return "W";
				default: return "";
			}
		}

        /// <summary>
        /// Ritorna il tipo di edition al serial in oggetto.
        /// </summary>
        /// <param name="db">stringa che indica il db</param>
        //---------------------------------------------------------------------
        public static Edition GetEditionFromString(string edition)
        {
            //			if (edition == null)
            //				return Microarea.Library.Activation.Edition.Undefined;
            //			switch (edition.ToLower().Trim())
            //			{
            //				case "s":
            //					return Microarea.Library.Activation.Edition.Standard;
            //				case "p":
            //					return Microarea.Library.Activation.Edition.Professional;
            //				case "e":
            //					return Microarea.Library.Activation.Edition.Enterprise;
            //				default:
            //					return Microarea.Library.Activation.Edition.Undefined;
            //			}

            // Cambiato per permettere l'offuscamento.
            if (edition == null)
                return Edition.Undefined;

            if (String.Compare(edition, "s", true, CultureInfo.InvariantCulture) == 0)
                return Edition.Standard;
            if (String.Compare(edition, "p", true, CultureInfo.InvariantCulture) == 0)
                return Edition.Professional;
            if (String.Compare(edition, "e", true, CultureInfo.InvariantCulture) == 0)
                return Edition.Enterprise;

            if (String.Compare(edition, "a", true, CultureInfo.InvariantCulture) == 0)
                return Edition.EditionA;
            if (String.Compare(edition, "b", true, CultureInfo.InvariantCulture) == 0)
                return Edition.EditionB;
            if (String.Compare(edition, "c", true, CultureInfo.InvariantCulture) == 0)
                return Edition.EditionC;

            return Edition.Undefined;
        }

        /// <summary>
        /// Ritorna una stringa di un carattere contenente la codifica della
        /// <code>Edition</code>.
        /// </summary>
        //---------------------------------------------------------------------
        public static string GetStringFromEdition(Edition edition)
		{
			switch (edition)
			{
				case Edition.Standard:
					return "S";
				case Edition.Professional:
					return "P";
				case Edition.Enterprise:
					return "E";
                case Edition.EditionA:
                    return "A";
                case Edition.EditionB:
                    return "B";
                case Edition.EditionC:
                    return "C";
                default:
					return "";
			}
		}

		/// <summary>
		/// Ritorna il char che identifica il database a partire dalla strina di tre caratteri.
		/// </summary>
		/// <param name="db"></param>
		/// <returns></returns>
		//---------------------------------------------------------------------
		public static string GetDBChar(string db)
		{
			if (db == null || db.Length != 3)
				throw new ArgumentException("Invalid db value");
			return db.Substring(0,1);
		}

		/// <summary>
		/// Ritorna il carattere che esprime l'accoppiata database/sistema operativo.
		/// </summary>
		//---------------------------------------------------------------------
		public static char Encode (string db, string opSys)
		{
			if (db == null || db.Length != 1)
				throw new ArgumentException("Invalid db value");
			if (opSys == null || opSys.Length != 1)
				throw new ArgumentException("Invalid opSys value");

			if (db_osMap == null)
				InitDbOsMap();
			
			string s = db + opSys;
			object obj = db_osMap[s];
			if (obj == null)
				throw new Exception("Unable to encode db and opSys");
			return obj.ToString()[0];
		}

		/// <summary>
		/// Ritorna la stringa di due caratteri in cui il primo esprime il
		/// database e il secondo il sistema operativo per il codice
		/// identificato dal char in ingresso.
		/// </summary>
		//---------------------------------------------------------------------
		public static string Decode (char db_opSys)
		{
			if (db_osMap == null)
				InitDbOsMap();
			foreach (DictionaryEntry de in db_osMap)
			{
				if (String.Compare(de.Value.ToString(), db_opSys.ToString(), true, CultureInfo.InvariantCulture) == 0)
					return de.Key.ToString();
			}
			throw new Exception("Unable to decode db and db_opSys");
			
		}

		#endregion

		#region IComparable Members

		//---------------------------------------------------------------------
		public int CompareTo(object obj)
		{
			return CompareTo(obj as SerialNumber);
		}

		#endregion

		#region IComparable<SerialNumber> Members

		//---------------------------------------------------------------------
		public int CompareTo(SerialNumber other)
		{
			if (other == null)
				return 1;

			if (Object.ReferenceEquals(this, other))
				return 0;

			return String.Compare(PlainValue, other.PlainValue, StringComparison.InvariantCultureIgnoreCase);
		}

		#endregion

		#region IEquatable<SerialNumber> Members

		//---------------------------------------------------------------------
		public bool Equals(SerialNumber other)
		{
			return CompareTo(other) == 0;
		}

		#endregion

		//---------------------------------------------------------------------
		public static implicit operator string(SerialNumber sn)
		{
			if (sn == null)
				throw new ArgumentNullException("sn");

			return sn.Value;
		}

		//---------------------------------------------------------------------
		public static implicit operator SerialNumber(string s)
		{
			if (s == null)
				throw new ArgumentNullException("s");

			if (s.Trim().Length != SerialNumber.SerialNumberLength)
				throw new ArgumentException("Invalid lenght for serial number", "s");

			return new SerialNumber(s);
		}

        //---------------------------------------------------------------------
        internal static bool IsWMSMobile(string shortName, bool powerProd)
        {
            return 
                (shortName != null 
                && shortName.Length == 4 
                && powerProd 
                && 
                (string.Compare(shortName, WMSMobilePlaceHolder, true, CultureInfo.InvariantCulture) == 0 || 
                 string.Compare(shortName, ManufacturingMobilePlaceHolder, true, CultureInfo.InvariantCulture) == 0)
                );
             
        }
    }

	#endregion

	#region Class SerialNumberFormatException

	/// <summary>
	/// Esprime il fatto che il formato del serial number non é corretto.
	/// </summary>
	//=========================================================================
	public class SerialNumberFormatException : Exception
	{
		//---------------------------------------------------------------------
		public SerialNumberFormatException()
		{}

		//---------------------------------------------------------------------
		public SerialNumberFormatException(string errorMessage) : base(errorMessage)
		{}
	}

	#endregion
}
