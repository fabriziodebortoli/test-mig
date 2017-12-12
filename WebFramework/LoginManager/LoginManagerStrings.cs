using System.Resources;

namespace Microarea.WebServices.LoginManager
{
	/// <summary>
	/// Summary description for LoginManagerStrings.
	/// </summary>
	public class LoginManagerStrings
	{
		private static ResourceManager rm = new ResourceManager (typeof(LoginManagerStrings));
		
		public static string ErrReadingLoginsArticles		{ get { return rm.GetString ("ErrReadingLoginsArticles"); }}
		public static string ErrLicenzaUtente				{ get { return rm.GetString ("ErrLicenzaUtente"); }}
		public static string ErrReadingAcivationFile		{ get { return rm.GetString ("ErrReadingAcivationFile"); }}
		public static string ErrInitAcivationMng			{ get { return rm.GetString ("ErrInitAcivationMng"); }}
		public static string ErrNoUserConfigured			{ get { return rm.GetString ("ErrNoUserConfigured"); }}
		public static string ErrDBConnectionFailed			{ get { return rm.GetString ("ErrDBConnectionFailed"); }}
		public static string ErrPathFinderUninitializated	{ get { return rm.GetString ("ErrPathFinderUninitializated"); }}
		public static string ErrServerKeyEmpty				{ get { return rm.GetString ("ErrServerKeyEmpty"); }}
		public static string InitSuccess					{ get { return rm.GetString ("InitSuccess"); }}
		public static string SysDBConnectionStringEmpty		{ get { return rm.GetString ("SysDBConnectionStringEmpty"); }}
		public static string Stop							{ get { return rm.GetString ("Stop"); }}
		public static string ResourcesTimeout				{ get { return rm.GetString ("ResourcesTimeout"); }}
		public static string FunctionError					{ get { return rm.GetString ("FunctionError"); }}
		public static string Initializing					{ get { return rm.GetString ("Initializing"); }}
		public static string Ending							{ get { return rm.GetString ("Ending"); }}
		public static string FunctionalityNotPresent		{ get { return rm.GetString ("FunctionalityNotPresent"); }}
		public static string UserNotAuthenticated			{ get { return rm.GetString ("UserNotAuthenticated"); }}
		public static string ErrAssignUserToArticle			{ get { return rm.GetString ("ErrAssignUserToArticle"); }}
		public static string CompanyUnknown					{ get { return rm.GetString ("CompanyUnknown"); }}
		public static string UserUnknown					{ get { return rm.GetString ("UserUnknown"); }}
		public static string CalWithoutAuthentication		{ get { return rm.GetString ("CalWithoutAuthentication"); }}
		public static string ErrCalLicenzaUtente			{ get { return rm.GetString ("ErrCalLicenzaUtente"); }}
		public static string UnregisteredProduct			{ get { return rm.GetString ("UnregisteredProduct"); }}
		public static string InvalidDemoName				{ get { return rm.GetString ("InvalidDemoName"); }}
        public static string Case { get { return rm.GetString("Case"); } }
        public static string ValidationStatus { get { return rm.GetString("ValidationStatus"); } }

		public static string SysDBInfo { get { return rm.GetString("SysDBInfo"); } }
		public static string SMSOK { get { return rm.GetString("SMSOK"); } }
		public static string SmsCodeError { get { return rm.GetString("SmsCodeError"); } }
		public static string LblInsertAndClick { get { return rm.GetString("LblInsertAndClick"); } }
		public static string LblSendSMS1 { get { return rm.GetString("LblSendSMS1"); } }
        public static string SendSmsInfo1 { get { return rm.GetString("SendSmsInfo1"); } }
        public static string ServiceAvaiability { get { return rm.GetString("ServiceAvaiability"); } }

        public static string LblSMSNotSupported { get { return rm.GetString("LblSMSNotSupported"); } }
        public static string LblSMSNotReceived1 { get { return rm.GetString("LblSMSNotReceived1"); } }
       
        public static string LblSMSNotReceived2 { get { return rm.GetString("LblSMSNotReceived2"); } }
        public static string Notes { get { return rm.GetString("Notes"); } }
        public static string SMSInfoVatNumber1 { get { return rm.GetString("SMSInfoVatNumber1"); } }
        public static string SMSInfoVatNumber2 { get { return rm.GetString("SMSInfoVatNumber2"); } }
		public static string ValidationOK { get { return rm.GetString("ValidationOK"); } }
		public static string ValidationOKInfo { get { return rm.GetString("ValidationOKInfo"); } }
		
		public static string PingFailed1First { get { return rm.GetString("PingFailed1First"); } }
		public static string PingFailed2 { get { return rm.GetString("PingFailed2"); } }
		public static string PingFailed3 { get { return rm.GetString("PingFailed3"); } }
		public static string PingFailed4 { get { return rm.GetString("PingFailed4"); } }
		public static string PingFailed5 { get { return rm.GetString("PingFailed5"); } }
		public static string PingFailed6 { get { return rm.GetString("PingFailed6"); } }
		public static string PingFailed7 { get { return rm.GetString("PingFailed7"); } }
		public static string LblSMSNotNeeded { get { return rm.GetString("LblSMSNotNeeded"); } }
		public static string PingFailed1Second { get { return rm.GetString("PingFailed1Second"); } }
		public static string PingFailed1Last { get { return rm.GetString("PingFailed1Last"); } }
		public static string PingFailed8 { get { return rm.GetString("PingFailed8"); } }
		public static string PingFailed9 { get { return rm.GetString("PingFailed9"); } }

		public static string SMSPageTitle { get { return rm.GetString("SMSPageTitle"); } }
		public static string SMSPageBtnRegisterText { get { return rm.GetString("SMSPageBtnRegisterText"); } }
        public static string LblRemark1 { get { return rm.GetString("LblRemark1"); } }
        public static string LblRemark2 { get { return rm.GetString("LblRemark2"); } }
        
		public static string LblInfo3 { get { return rm.GetString("LblInfo3"); } }
		public static string LblError1 { get { return rm.GetString("LblError1"); } }
		public static string LblError2 { get { return rm.GetString("LblError2"); } }
        public static string Example { get { return rm.GetString("Example"); } }
        public static string LblReceivedError1 { get { return rm.GetString("LblReceivedError1"); } }
        public static string LblReceivedError2 { get { return rm.GetString("LblReceivedError2"); } }
        public static string NoMessage { get { return rm.GetString("NoMessage"); } }
        public static string LblSMSNotReceived3Title { get { return rm.GetString("LblSMSNotReceived3Title"); } }
        public static string LblSMSNotReceived3 { get { return rm.GetString("LblSMSNotReceived3"); } }

		public static string EASyncInitSuccess { get { return rm.GetString("EASyncInitSuccess"); } }
		public static string EASyncInitError { get { return rm.GetString("EASyncInitError"); } }
        public static string DSyncInitError { get { return rm.GetString("DSyncInitError"); } }
        public static string DSyncInitSuccess { get { return rm.GetString("DSyncInitSuccess"); } }
	//infinity
        public static string NoValidUserLogged { get { return rm.GetString("NoValidUserLogged"); } }
        public static string NoValidUserLoggedNow { get { return rm.GetString("NoValidUserLoggedNow"); } }
        public static string InfinityUserNotLogged { get { return rm.GetString("InfinityUserNotLogged"); } }
        public static string TokenExpired { get { return rm.GetString("TokenExpired"); } }
        





        //Campi di tabelle
        public static string UseAuditing					= "UseAuditing";
		public static string UseSecurity					= "UseSecurity";
        public static string UseRowSecurity                 = "UseRowSecurity";
		public static string UseDataSynchro					= "UseDataSynchro";

		public static string Company						= "Company";
		public static string CompanyID						= "CompanyID";
		public static string ProviderID						= "ProviderID";
		public static string Provider						= "Provider";
		public static string Description					= "Description";
		public static string Login							= "Login";
		public static string LoginID						= "LoginID";
        public static string Disabled = "Disabled";
		public static string WindowsAuthentication			= "WindowsAuthentication";
		public static string CompanyDBName					= "CompanyDBName";
		public static string CompanyDBServer				= "CompanyDBServer";
		public static string Admin							= "Admin";
		public static string DBUser							= "DBUser";
		public static string DBPassword						= "DBPassword";
		public static string DBWindowsAuthentication		= "DBWindowsAuthentication";
		public static string DBDefaultUser					= "DBDefaultUser";
		public static string DBDefaultPassword				= "DBDefaultPassword";
		public static string CompanyDBAuthenticationWindows	= "CompanyDBAuthenticationWindows";
		public static string CompanyDBWindowsAuthentication	= "CompanyDBWindowsAuthentication";
		public static string Password						= "Password";
		public static string UseConstParameter				= "UseConstParameter";
		public static string StripTrailingSpaces			= "StripTrailingSpaces";
		public static string UseKeyedUpdate					= "UseKeyedUpdate";
		public static string SQLSetPosSupported				= "SQLSetPosSupported";
		public static string CursorType						= "CursorType";
		public static string UseTransaction					= "UseTransaction";
		public static string UseUnicode						= "UseUnicode";
		public static string Updating						= "Updating";
		public static string IsValid						= "IsValid";
		public static string WebAccess						= "WebAccess";
		public static string SmartClientAccess				= "SmartClientAccess";
		public static string Guest							= "Guest";
        public static string Port                           = "Port";
		public static string ConcurrentLogin				= "ConcurrentAccess";
		public static string PrivateAreaWebSiteAccess		= "PrivateAreaWebSiteAccess";
		public static string EMail							= "Email";
		public static string EBDeveloper					= "EBDeveloper";
		//   public static string BalloonBlockedType             = "BalloonBlockedType";
	}
}
