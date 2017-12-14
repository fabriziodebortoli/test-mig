using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

using Microarea.TaskBuilderNet.Licence.Activation;
using Microarea.TaskBuilderNet.Licence.Licence.XmlSyntax;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Licence.Licence
{
	/// <summary>
	/// Permette di usare il serialnumber digitato nelle textboxes come oggetto
	/// </summary>
	//=========================================================================
	[Serializable]
    public class SerialNumberInfo : IComparable<SerialNumberInfo>
	{
		private		string	group1		= String.Empty;
		private		string	group2		= String.Empty;
		private		string	group3		= String.Empty;
		private		string	group4		= String.Empty;
		public		static string	Separator	= "-";
		private		static int		GroupLen	= 4;
	
		public bool IsComplete { get {return 
										 group1 != null && group1.Length == GroupLen && group1 != String.Empty &&
										 group2 != null && group2.Length == GroupLen && group2 != String.Empty &&
										 group3 != null && group3.Length == GroupLen && group3 != String.Empty &&
										 group4 != null && group4.Length == GroupLen && group4 != String.Empty ;
									 }}
		public string PK;
		
		//---------------------------------------------------------------------
		public SerialNumberInfo(string group1, string group2, string group3, string group4)
		{
			if (group1 == null || group1.Length != GroupLen) 
				this.group1 = String.Empty;
			else	
				this.group1 = group1;
			if (group2 == null || group2.Length != GroupLen) 
				this.group2 = String.Empty;
			else	
				this.group2 = group2;
			if (group3 == null || group3.Length != GroupLen) 
				this.group3 = String.Empty;
			else	
				this.group3 = group3;
			if (group4 == null || group4.Length != GroupLen) 
				this.group4 = String.Empty;
			else	
				this.group4 = group4;
			
		}

		//---------------------------------------------------------------------
		public SerialNumberInfo(string sn)
		{
			string g1 = String.Empty;
			string g2 = String.Empty;
			string g3 = String.Empty;
			string g4 = String.Empty;

			if (sn != null && sn != string.Empty)
			{
				string noSeparator = sn.Replace(Separator, String.Empty);
				try
				{
					if (noSeparator == null || noSeparator.Length < 16)
						return;
					g1 = noSeparator.Substring(0,4);
					g2 = noSeparator.Substring(4,4);
					g3 = noSeparator.Substring(8,4);
					g4 = noSeparator.Substring(12,4);

				}
				catch (Exception)
				{
					Debug.Fail(String.Format(CultureInfo.InvariantCulture, LicenceStrings.IncorrectSerial, sn));
					return;
				}
			}
			
			this.group1 = g1;
			this.group2 = g2;
			this.group3 = g3;
			this.group4 = g4;
		}

        //---------------------------------------------------------------------
        public int CompareTo(SerialNumberInfo obj)
        {
            return GetSerialWOSeparator().CompareTo(obj.GetSerialWOSeparator());
        }

		//---------------------------------------------------------------------
		public string GetSerialWSeparator()
		{	
			if (IsComplete)
				return String.Concat(group1, Separator, group2, Separator, group3, Separator, group4);
			else return String.Empty;
		}

		//---------------------------------------------------------------------
		public string GetSerialWOSeparator()
		{	
			return String.Concat(group1, group2, group3, group4);
		}

		//---------------------------------------------------------------------
		public string[] GetSerialAsArray()
		{	
			return new string[]{group1, group2, group3, group4};
		}

		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is SerialNumberInfo))
				return false;

			SerialNumberInfo comp = obj as SerialNumberInfo;
			//paragone case insensitive perchè devono essere tutti maiuscoli
			return (string.Compare(this.GetSerialWOSeparator(), comp.GetSerialWOSeparator(), false, CultureInfo.InvariantCulture) == 0);
		}
		
		//---------------------------------------------------------------------
		public override int GetHashCode()
		{
			return (group1 + "," + group2 + "," + group3 + "," + group4).GetHashCode();
		}

        //---------------------------------------------------------------------
        public static bool IsPLCalNamedShortName(SerialNumberInfo serial)
		{
            try
            {
                SerialNumber sn = new SerialNumber(serial.GetSerialWOSeparator());
                return SerialNumber.IsPLCalNamedShortName(sn);
            }
            catch (Exception)
            {
              return false;
            }
         
		}

        
		/// <summary>
		/// Ritorna il tipo di databaseassociato al serial in oggetto.
		/// </summary>
		/// <param name="serialsList">Serial-Number</param>
		//---------------------------------------------------------------------
		public static DatabaseVersion GetDatabaseVersion(SerialNumberInfo serial)
		{
			DatabaseVersion db = DatabaseVersion.Undefined;
			try
			{
				SerialNumber sn = new SerialNumber(serial.GetSerialWOSeparator());
				db = sn.Database;
			}
			catch (Exception)
			{
				db = DatabaseVersion.Undefined;
			}

			//DatabaseVersion e = SerialNumber.GetDatabaseVersionFromString(db);
			return db;

		}

		/// <summary>
		/// Ritorna il tipo di edition al serial in oggetto.
		/// </summary>
		/// <param name="serialsList">Serial-Number</param>
		//---------------------------------------------------------------------
		public static Edition GetEdition(SerialNumberInfo serial)
		{
			Edition e = Edition.Undefined;
			try
			{
				SerialNumber sn = new SerialNumber(serial.GetSerialWOSeparator());
				e = sn.Edition;
			}
			catch (Exception)
			{
				 e = Edition.Undefined;
			}

			//Edition e = SerialNumber.GetEditionFromString(edition);
			return e;

		}
		/// <summary>
		/// Ritorna il tipo di edition al serial in oggetto.
		/// </summary>
		/// <param name="serialsList">Serial-Number</param>
		//---------------------------------------------------------------------
		public static string GetEditionId(SerialNumberInfo serial)
		{
			string e = string.Empty;
			try
			{
				SerialNumber sn = new SerialNumber(serial.GetSerialWOSeparator());
				e = sn.ProductCode.Edition;
			}
			catch
			{
			}
			return e;

		}
		//---------------------------------------------------------------------
		public static string GetCountry(SerialNumberInfo serial)
		{
			string e = string.Empty;
			try
			{
				SerialNumber sn = new SerialNumber(serial.GetSerialWOSeparator());
				e = sn.Country;
			}
			catch
			{
			}
			return e;

		}
		
		//---------------------------------------------------------------------
		public static int GetProgressive(SerialNumberInfo serial)
		{
			try
			{
				SerialNumber sn = new SerialNumber(serial.GetSerialWOSeparator());
				return  sn.ProgressiveNumber;
			}
			catch (Exception)
			{
				return 0;
			}
		}

		//---------------------------------------------------------------------
		public static DBNetworkType GetDBNetworkType(SerialNumberInfo serial)
		{
			DBNetworkType netType = DBNetworkType.Undefined;
			try
			{
				SerialNumber sn = new SerialNumber(serial.GetSerialWOSeparator());
				netType = sn.DBNetworkType;
			}
			catch (Exception)
			{
				netType = DBNetworkType.Undefined;
			}

			return netType;
		}

		/// <summary>
        /// Ritorna ilprodID del serial in oggetto.
        /// </summary>
        /// <param name="serialsList">Serial-Number</param>
        //---------------------------------------------------------------------
        public static string GetProdId(SerialNumberInfo serial)
        {
            try
            {
                SerialNumber sn = new SerialNumber(serial.GetSerialWOSeparator());
                return sn.ProductCode.Product;
            }
            catch
            {
                return String.Empty;
            }

        }

		/// <summary>
		/// Ritorna il numero di cal associato alla lista di serial in oggetto.
		/// </summary>
		/// <param name="serialsList">Lista di Serial-Number</param>
		/// <param name="isModuleWithCal">Specifica se si tratta di un modulo con cal</param>
		/// <param name="errorMessage">Eventuale messaggio di errore se se ne sono verificati</param>
		//---------------------------------------------------------------------
		internal static int GetCalNumberFromSerials(string name, IList<SerialNumberInfo> serialsList, CalTypeEnum caltype, out string errorMessage, bool webCal, bool concurrent, bool namedcal, bool wmsMobile, bool manufacturingMobile = false)
		{
			int result = 0;
			errorMessage = null;
			if (serialsList != null)
                foreach (SerialNumberInfo serial in serialsList)
                    result += GetCalNumberFromSerial(name, serial, caltype, ref errorMessage, webCal, concurrent, namedcal, wmsMobile, manufacturingMobile);
                
			return result;
		}

		private static string MagicDocProModuleName = "ERP-Pro.MagicDocuments" ;
		private static string MagicDocStdModuleName = "ERP-Std.MagicDocuments" ;
		private static string EasyLookProModuleName = "ERP-Pro.EasyLook" ;
		private static string Office				= "OFFI";
		private static string MagicDoc				= "M";
		private static string MagicDocOffice		= "MO";
		private static string EasyLook				= "C";
		private static string EasyLookDB			= "E";


		//---------------------------------------------------------------------
		private static bool VerifyNameAndCAL(string name, SerialNumberInfo serial, CalTypeEnum caltype)
		{
			SerialNumber sn = new SerialNumber(serial.GetSerialWOSeparator(), caltype);
			bool isMagicDocs = (
						String.Compare(MagicDocProModuleName, name, true, CultureInfo.InvariantCulture) == 0	|| 
						String.Compare(MagicDocStdModuleName, name, true, CultureInfo.InvariantCulture) == 0
						);	
			bool isEasylook = String.Compare(EasyLookProModuleName, name, true, CultureInfo.InvariantCulture) == 0;
			bool ok =  !
						(isMagicDocs &&	
						(!sn.RawData.ToUpperInvariant().StartsWith(MagicDoc) && 
						!sn.RawData.ToUpperInvariant().StartsWith(Office))
						||
						(isEasylook	&& 
						!sn.RawData.ToUpperInvariant().StartsWith(EasyLookDB) && 
						!sn.RawData.ToUpperInvariant().StartsWith(EasyLook)
						)
					) ;

				return ok;
		}
		//---------------------------------------------------------------------
		public static bool CountAsOfficeLicence(string name, SerialNumberInfo serial, CalTypeEnum caltype)
		{
			SerialNumber sn = new SerialNumber(serial.GetSerialWOSeparator(), caltype);
			return  (
						(
							(
							String.Compare(MagicDocProModuleName, name, true, CultureInfo.InvariantCulture) == 0	|| 
							String.Compare(MagicDocStdModuleName, name, true, CultureInfo.InvariantCulture) == 0
							)															&& 
							sn.RawData.ToUpperInvariant().StartsWith(MagicDocOffice)
						)																||
				
						sn.RawData.ToUpperInvariant().StartsWith(Office)
				
					) ;

				
		}

		//---------------------------------------------------------------------
		public static bool IsValidMDOfficeLicence(string name, SerialNumberInfo serial, CalTypeEnum caltype)
		{
			SerialNumber sn = new SerialNumber(serial.GetSerialWOSeparator(), caltype);
			if (sn.RawData.ToUpperInvariant().StartsWith(Office))
			{
				bool ok = (String.Compare(MagicDocProModuleName, name, true, CultureInfo.InvariantCulture) == 0	|| 
							String.Compare(MagicDocStdModuleName, name, true, CultureInfo.InvariantCulture) == 0);
				return ok;
			}
			return true;
				
		}
		//---------------------------------------------------------------------
		public static bool IsTPCal(SerialNumberInfo serial, CalTypeEnum caltype)
		{
			SerialNumber sn = new SerialNumber(serial.GetSerialWOSeparator(), caltype);
			return (sn.RawData.ToUpperInvariant().StartsWith(SerialNumber.WebAccessCalPlaceHolder));
				
		}


        //---------------------------------------------------------------------
        private static int GetCalNumberFromSerial(string name, SerialNumberInfo serial, CalTypeEnum caltype, ref string errorMessage, bool webCal, bool concurrent, bool namedcal, bool wmsMobile)
        {
            return GetCalNumberFromSerial(name, serial, caltype, ref errorMessage, webCal, concurrent, namedcal, wmsMobile, false);
        }

		/// <summary>
		/// Ritorna il numero di cal associate al serial in oggetto.
		/// </summary>
		/// <param name="serial">Serial-Number da esaminare</param>
		/// <param name="isModuleWithCal">Specifica se si tratta di un modulo con cal</param>
		/// <param name="errorMessage">Eventuale messaggio di errore se se ne sono verificati</param>
		//---------------------------------------------------------------------
        private static int GetCalNumberFromSerial(string name, SerialNumberInfo serial, CalTypeEnum caltype, ref string errorMessage, bool webCal, bool concurrent, bool namedcal, bool wmsMobile, bool manufacturingMobile)
		{
			try
			{
				SerialNumber sn = new SerialNumber(serial.GetSerialWOSeparator(), caltype);
				
				if (!VerifyNameAndCAL(name, serial, caltype)) 
					return 0;

                int val = 0;
				if (String.Compare(sn.Module, Consts.DevIdUser, true, CultureInfo.InvariantCulture) == 0 || // development  e personal di tbs
                    String.Compare(sn.Module, Consts.DevIdRiv, true, CultureInfo.InvariantCulture) == 0||
                    String.Compare(sn.Module, Consts.PersIdRiv, true, CultureInfo.InvariantCulture) == 0||
                    String.Compare(sn.Module, Consts.PersIdUSer, true, CultureInfo.InvariantCulture) == 0)
                    val = 1;
                if (String.Compare(sn.Module, Consts.DevIDIU, true, CultureInfo.InvariantCulture) == 0)//development uso interno
                    val = 999;
                if (String.Compare(sn.Module, Consts.DemoID, true, CultureInfo.InvariantCulture) == 0)//demo
                    val = 3;

                if (String.Compare(sn.Module, Consts.ResellerID, true, CultureInfo.InvariantCulture) == 0)//reseller
                    val = (sn.Edition == Edition.Standard) ? 1 : 3;

				if (String.Compare(sn.Module, Consts.DistributorID, true, CultureInfo.InvariantCulture) == 0)//distributor
                    val = (sn.Edition == Edition.Standard) ? 1 : 10;

                if (val > 0 && (caltype == CalTypeEnum.Master || caltype == CalTypeEnum.tbf || caltype == CalTypeEnum.AutoFunctional || caltype == CalTypeEnum.AutoTbs || caltype == CalTypeEnum.MasterNew) && !wmsMobile && !manufacturingMobile)
                        return val;

                //gestione wmsmobile ACDC
                if (!wmsMobile && String.Compare(sn.Module, SerialNumber.WMSMobilePlaceHolder, true, CultureInfo.InvariantCulture) == 0)//wmsmobile
                    return 0;
                if (wmsMobile && String.Compare(sn.Module, SerialNumber.WMSMobilePlaceHolder, true, CultureInfo.InvariantCulture) == 0)//wmsmobile
                    return 1;

                //gestione manufmobile ACDC
                if (!manufacturingMobile && String.Compare(sn.Module, SerialNumber.ManufacturingMobilePlaceHolder, true, CultureInfo.InvariantCulture) == 0)//manufmobile
                    return 0;
                if (manufacturingMobile && String.Compare(sn.Module, SerialNumber.ManufacturingMobilePlaceHolder, true, CultureInfo.InvariantCulture) == 0)//manufmobile
                    return 1;


				//è un SerialNumber di CAL?concurrent o named
				bool CALSN = ((sn.ConcurrentCAL != null && sn.ConcurrentCAL != "") ||(sn.NamedCAL != null && sn.NamedCAL != ""));
					
				//se è master con CAL...
				if (caltype == CalTypeEnum.Master && CALSN)
				{
					//...devo verificare che il modulo mastercal named non prenda 
					//le cal di tipo mastercalUnnamed...
					//però i nuovi package che contengono il server di easylook 
					//devono poter accettare i seriali di cal di EL
					//quindi masternew non ricade in questo caso.
					//--deve però accettare le cal concurrent 
					if (namedcal && !SerialNumber.IsCalNamedShortName(sn))
					{
                        if (!(SerialNumber.IsDbPlaceHolder(sn.Module) || SerialNumber.IsSql2012PlaceHolder(sn.Module))
                            && !SerialNumber.IsCalConcurrentShortName(sn))
							errorMessage += String.Format(CultureInfo.InvariantCulture, LicenceStrings.IncorrectSerial, serial.GetSerialWSeparator());	
						return 0;
					}
					//...e viceversa----non esistono più i seriali di cal di md e ml
					/*else if (!namedcal && !SerialNumber.IsCalUnNamedShortName(sn) && !SerialNumber.IsCalConcurrentShortName(sn))
					{
						errorMessage += String.Format(CultureInfo.InvariantCulture, LicenceStrings.IncorrectSerial, serial.GetSerialWSeparator());
						return 0;
					}	*/
				}

                ////se è autofunctional con CAL...i moduli autofunctional per loro prerogativa comunque valgono una cal, ma dovrebbero fare una verifica dello short name per evitare di conteggiare uno anche dei serial number qualsiasi (esempio se metto un cfin dentro un cful mi conta uno!!!)
                //if (caltype == CalTypeEnum.AutoFunctional && CALSN)
                //{
                    
                //}

				if (CALSN) 
				{
					if (!(webCal ^ sn.IsWebCal))
					{
						if (concurrent)
							return String.IsNullOrEmpty(sn.ConcurrentCAL) ? 0 : Int32.Parse(sn.ConcurrentCAL, CultureInfo.InvariantCulture);
						return String.IsNullOrEmpty(sn.NamedCAL)? 0 : Int32.Parse(sn.NamedCAL, CultureInfo.InvariantCulture);
					}
				}
				
			}
			catch (Exception exc)
			{
				errorMessage += String.Format(CultureInfo.InvariantCulture, LicenceStrings.IncorrectSerial, serial.GetSerialWSeparator());
				errorMessage += " ";
				errorMessage += exc.Message;
				errorMessage += " ";
			}
			
			
			return 0;
		}

		/// <summary>
		/// Ritorna il numero di cal associate al serial in oggetto.
		/// </summary>
		/// <param name="serial">Serial-Number da esaminare</param>
		/// <param name="isModuleWithCal">Specifica se si tratta di un modulo con cal</param>
		//---------------------------------------------------------------------
		public static int GetCalNumberFromSerial(string name,SerialNumberInfo serial, CalTypeEnum caltype, bool namedcal)
		{
			int cal = GetCalNumberFromSerial(name,serial, caltype, false,false, namedcal, false);
            int concurrentcal = GetCalNumberFromSerial(name, serial, caltype, false, true, false, false);
            int webcal = GetCalNumberFromSerial(name, serial, caltype, true, false, namedcal, false);
            int wmscal = GetCalNumberFromSerial(name, serial, caltype, false, false, false, true);
			return cal + webcal + concurrentcal + wmscal;
		}

		//---------------------------------------------------------------------
        public static int GetCalNumberFromSerial(string name, SerialNumberInfo serial, CalTypeEnum caltype, bool webCal, bool concurrent, bool namedcal, bool wmsmobilecal)
		{
			string errorMessage = null;
            int cal = GetCalNumberFromSerial(name, serial, caltype, ref errorMessage, webCal, concurrent, namedcal, wmsmobilecal);
			if (errorMessage != null)
				Debug.WriteLine(errorMessage);
			
			return cal ;
		}

        //---------------------------------------------------------------------
        public bool IsIntegrative(CalTypeEnum caltype)
        {
            return IsBackUp(caltype) || IsTest(caltype) || IsStandAlone(caltype);
        }

        //---------------------------------------------------------------------
        public bool IsBackUp(CalTypeEnum caltype)
        {
            string mod = GetModuleShortNameFromSerial(this, caltype);
            return String.Compare(mod, Consts.BackUpID, true, CultureInfo.InvariantCulture) == 0;
        }

        //---------------------------------------------------------------------
        public bool IsTest(CalTypeEnum caltype)
        {
            string mod = GetModuleShortNameFromSerial(this, caltype);
            return String.Compare(mod, Consts.TestID, true, CultureInfo.InvariantCulture) == 0;
        }

        //---------------------------------------------------------------------
        public bool IsStandAlone(CalTypeEnum caltype)
        {
            string mod = GetModuleShortNameFromSerial(this, caltype);
            return String.Compare(mod, Consts.StandAloneID, true, CultureInfo.InvariantCulture) == 0;
        }

        //---------------------------------------------------------------------
        public bool IsSpecial(CalTypeEnum caltype)
        {
            return IsDeveloper(caltype) || IsNFS(caltype) || IsDemo(caltype);
        }

		//---------------------------------------------------------------------
		public bool IsNFS(CalTypeEnum caltype)
		{
			return IsReseller(caltype) || IsDistributor(caltype);
		}

		//---------------------------------------------------------------------
		public bool IsDeveloper(CalTypeEnum caltype)
		{
			string mod = GetModuleShortNameFromSerial(this, caltype);
			return
                String.Compare(mod, Consts.DevIdRiv, true, CultureInfo.InvariantCulture) == 0 ||
                String.Compare(mod, Consts.DevIdUser, true, CultureInfo.InvariantCulture) == 0 ||
                String.Compare(mod, Consts.PersIdUSer, true, CultureInfo.InvariantCulture) == 0 ||
                String.Compare(mod, Consts.PersIdRiv, true, CultureInfo.InvariantCulture) == 0 ||
                String.Compare(mod, Consts.DevIDIU, true, CultureInfo.InvariantCulture) == 0;
		}

        //---------------------------------------------------------------------
        public bool IsDeveloperPlus(CalTypeEnum caltype)
        {
            string mod = GetModuleShortNameFromSerial(this, caltype);
            return String.Compare(mod, Consts.DevIdRiv, true, CultureInfo.InvariantCulture) == 0 ||
                String.Compare(mod, Consts.DevIdUser, true, CultureInfo.InvariantCulture) == 0 ||
                String.Compare(mod, Consts.PersIdUSer, true, CultureInfo.InvariantCulture) == 0 ||
                String.Compare(mod, Consts.PersIdRiv, true, CultureInfo.InvariantCulture) == 0;
        }

        //---------------------------------------------------------------------
        public bool IsDeveloperPlusK(CalTypeEnum caltype)
        {
            string mod = GetModuleShortNameFromSerial(this, caltype);
            return String.Compare(mod, Consts.DevIdRiv, true, CultureInfo.InvariantCulture) == 0 ||
                   String.Compare(mod, Consts.PersIdRiv, true, CultureInfo.InvariantCulture) == 0;
        } 
        //---------------------------------------------------------------------
        public bool IsDeveloperPlusUser(CalTypeEnum caltype)
        {
            string mod = GetModuleShortNameFromSerial(this, caltype);
            return String.Compare(mod, Consts.DevIdUser, true, CultureInfo.InvariantCulture) == 0 ||
                   String.Compare(mod, Consts.PersIdUSer, true, CultureInfo.InvariantCulture) == 0;
        }

        //---------------------------------------------------------------------
        public bool IsDeveloperIU(CalTypeEnum caltype)
		{
			string mod = GetModuleShortNameFromSerial(this, caltype);
			return String.Compare(mod, Consts.DevIDIU, true, CultureInfo.InvariantCulture) == 0;
		}

		//---------------------------------------------------------------------
		public bool IsDemo(CalTypeEnum caltype)
		{
			string mod = GetModuleShortNameFromSerial(this, caltype);
			return String.Compare(mod, Consts.DemoID, true, CultureInfo.InvariantCulture) == 0;
		}

		//---------------------------------------------------------------------
		public bool IsReseller(CalTypeEnum caltype)
		{
			string mod = GetModuleShortNameFromSerial(this, caltype);
			return String.Compare(mod, Consts.ResellerID, true, CultureInfo.InvariantCulture) == 0;
		}

		//---------------------------------------------------------------------
		public bool IsDistributor(CalTypeEnum caltype)
		{
			string mod = GetModuleShortNameFromSerial(this, caltype);
			return String.Compare(mod, Consts.DistributorID, true, CultureInfo.InvariantCulture) == 0;
		}

		/// <summary>
		/// Ritorna il nome breve per il Serial-Number in oggetto.
		/// </summary>
		/// <param name="serial">Serial-Number da esaminare</param>
		/// <param name="isModuleWithCal">Specifica se si tratta di un modulo con cal</param>
		/// <param name="errorMessage">Eventuale messaggio di errore se se ne sono verificati</param>
		//---------------------------------------------------------------------
		public static string GetModuleShortNameFromSerial(SerialNumberInfo serial, CalTypeEnum caltype)
		{
			try
			{
				SerialNumber sn = new SerialNumber(serial.GetSerialWOSeparator(), caltype);
				return sn.Module;
			}
			catch (Exception exc)
			{
				string errorMessage = String.Format(CultureInfo.InvariantCulture, LicenceStrings.IncorrectSerial, serial.GetSerialWSeparator());
				errorMessage += " ";
				errorMessage += exc.Message;
				Debug.WriteLine(errorMessage);
			}
			return String.Empty;
		}

        //---------------------------------------------------------------------
        internal void Analize2012Status(CalTypeEnum calType, bool powerprod, out bool placeHolderOLD, out bool placeHolder2012, out bool calPLsql)
        {
            placeHolderOLD = false;
            placeHolder2012 = false;
            calPLsql = false;
            string mod = GetModuleShortNameFromSerial(this, calType);

            placeHolderOLD = SerialNumber.IsDbPlaceHolderOLD(mod, powerprod);
            placeHolder2012 = SerialNumber.IsSql2012PlaceHolder(mod, powerprod);
            calPLsql = IsPLCalNamedShortName(this);

            if (calPLsql)
            {
                DatabaseVersion dbnt = GetDatabaseVersion(this);
                calPLsql = (dbnt == DatabaseVersion.SqlServer2000);
            }

        }



    }
    //=========================================================================
    public class MyListSorter : IComparer<SerialNumberInfo>
    {
        public int Compare(SerialNumberInfo obj1, SerialNumberInfo obj2)
        {
            return obj2.CompareTo(obj1);
           
        }
    }

	#region SerialsModuleInfo
	//=========================================================================
	public class SerialsModuleInfo
	{
		//lista di SerialNumberInfo
		public IList<SerialNumberInfo> SerialList = new List<SerialNumberInfo>();
		public string		SalesModuleName;
		public string		SalesModuleLocalizedName;
		public CalTypeEnum	CalType;
		public CalUseEnum	CalUse;
		public ArrayList	PKList = new ArrayList();

		//---------------------------------------------------------------------
		public SerialsModuleInfo(string name, CalUseEnum caluse, string localizedName, IList<SerialNumberInfo> list, CalTypeEnum calType, IList pkList)
		{
			if (name != null || name != String.Empty)
			{
				SalesModuleLocalizedName = localizedName;
				SalesModuleName = name;
				if (list != null)
					SerialList  = list;
				CalType = calType;
				CalUse = caluse;
				if(pkList != null)
				PKList = new ArrayList(pkList);
			}
		}

		//---------------------------------------------------------------------
		public SerialsModuleInfo(string name)
		{
			if (name != null || name != String.Empty)
				SalesModuleName = name;
		}

//		//---------------------------------------------------------------------
//		public void AddSerial(SerialNumberInfo serial)
//		{
//			if (!SerialList.Contains(serial))
//			{
//				SerialList.Add(serial);
//			}
//		}

//		//---------------------------------------------------------------------
//		public void RemoveSerial(SerialNumberInfo serial)
//		{
//			if (SerialList.Contains(serial))
//				SerialList.Remove(serial);
//		}

		//---------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is SerialsModuleInfo))
				return false;

			SerialsModuleInfo comp = obj as SerialsModuleInfo;

			return String.Compare(comp.SalesModuleName, SalesModuleName, true, CultureInfo.InvariantCulture) == 0;
			
		}

		//---------------------------------------------------------------------
		public override int GetHashCode()
		{
			return SalesModuleName.GetHashCode();
		}
	}
	#endregion
}
