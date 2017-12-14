using System.Resources;

namespace Microarea.Library.SMBaseHandler
{
	/// <summary>
	/// Definisce una risorsa per ogni codice definito nella classe MessagesCode.
	/// </summary>
	/// <remarks>
	/// Mantenere un rapporto di 1 a 1 tra codici e descrizioni.
	/// </remarks>
	//=========================================================================
	public class Strings
	{
		private static ResourceManager resources = new ResourceManager(typeof(Strings));

		public static string Code			{ get { return resources.GetString("Code"); } }
		public static string Detail			{ get { return resources.GetString("Detail"); } }
		public static string GenericError	{ get { return resources.GetString("GenericError"); } }
		// Valore di default: nel caso si introduca un nuovo codice e ci si dimentica
		// di definire la corrispondente risorsa, si avrà un "GenericError".

		#region MessagesCode
		public static string Ok						{ get { return resources.GetString("Ok"); } }
		public static string NotConsistentXml		{ get { return resources.GetString("NotConsistentXml"); } }
		public static string WrongExtension			{ get { return resources.GetString("WrongExtension"); } }
		public static string WrongSyntax			{ get { return resources.GetString("WrongSyntax"); } }
		public static string AuthenticationFailed	{ get { return resources.GetString("AuthenticationFailed"); } }
		public static string NotAuthorizedCdS		{ get { return resources.GetString("NotAuthorizedCdS"); } }
		public static string DatabaseConnectionFailed	{ get { return resources.GetString("DatabaseConnectionFailed"); } }
		public static string NotAuthorizedContents	{ get { return resources.GetString("NotAuthorizedContents"); } }
		public static string CryptingFailed			{ get { return resources.GetString("CryptingFailed"); } }
		public static string ResponseXmlNotValid	{ get { return resources.GetString("ResponseXmlNotValid"); } }
		public static string GenericServerError		{ get { return resources.GetString("GenericServerError"); } }
		public static string TimeError				{ get { return resources.GetString("TimeError"); } }
		public static string NoSyncServerUrl		{ get { return resources.GetString("NoSyncServerUrl"); } }
		public static string SyncServerException	{ get { return resources.GetString("SyncServerException"); } }
		public static string InvalidRequest			{ get { return resources.GetString("InvalidRequest"); } }
		public static string InitializationError	{ get { return resources.GetString("InitializationError"); } }
		public static string LockedLogin			{ get { return resources.GetString("LockedLogin"); } }
		public static string PasswordExpired		{ get { return resources.GetString("PasswordExpired"); } }
		public static string PasswordToChange		{ get { return resources.GetString("PasswordToChange"); } }
		public static string OnlyEnglishSite		{ get { return resources.GetString("OnlyEnglishSite"); } }
		public static string DisabledLogin			{ get { return resources.GetString("DisabledLogin"); } }
		public static string TooMuchLoginErrors		{ get { return resources.GetString("TooMuchLoginErrors"); } }
		public static string ErrorContactingDb		{ get { return resources.GetString("ErrorContactingDb"); } }
		public static string InvalidHasSerial		{ get { return resources.GetString("InvalidHasSerial"); } }
		public static string IncompleteContent		{ get { return resources.GetString("IncompleteContent"); } }
		public static string ProductNotRegisteredAndNoLicence		{ get { return resources.GetString("ProductNotRegisteredAndNoLicence"); } }
		public static string ProductNotRegisteredAndNoStandAlone	{ get { return resources.GetString("ProductNotRegisteredAndNoStandAlone"); } }
		public static string ShortNamesMissing		{ get { return resources.GetString("ShortNamesMissing"); } }
		public static string ShortNameInvalid		{ get { return resources.GetString("ShortNameInvalid"); } }
		public static string ProductNotActive		{ get { return resources.GetString("ProductNotActive"); } }
		public static string ShortNamePrivate		{ get { return resources.GetString("ShortNamePrivate"); } }
		public static string IncludesInvalid		{ get { return resources.GetString("IncludesInvalid"); } }
#endregion
	}
}