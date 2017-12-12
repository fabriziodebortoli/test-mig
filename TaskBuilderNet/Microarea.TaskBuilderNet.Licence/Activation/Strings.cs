
namespace Microarea.TaskBuilderNet.Licence.Activation
{
	/// <summary>
	/// Raccoglie le stringhe usate nel file wce, quindi tag e attribute.
	/// Non necessita di localizzazione.
	/// </summary>
	//=========================================================================
	public class WceStrings
	{
		internal static string[] UserInfoTagsVatNr = new string[]{Element.Company, Element.VatNumber, Element.Country};
		internal static string[] UserInfoTagsCodFisc = new string[]{Element.Company, Element.CodFisc, Element.Country};
        internal static string MluDateFormat = "dd-MM-yyyy";

		//=====================================================================
		public class Attribute
		{
			public static string ActivationKey	= "activationkey";
			public static string ActiveSince	= "activesince";
			public static string ActiveTo		= "activeto";
			public static string HasSerial		= "hasserial";
			public static string Id				= "id";
			public static string Installation	= "installation";
			public static string Key			= "key";
			public static string Name			= "name";
			public  const string InternalCode	= "internalcode";
			public const string BasicServer = "basicserver";
			public const string Demo = "defaultdemo";
			public const string Obsolete = "obsolete";
			
			public  const string Producer		= "producer";
			public static string Product		= "product";
			public static string Size			= "size";
			public static string UserId			= "userid";
			public  const string Value			= "value";
			public const string ActivationID = "activationid";
			public  const string Release		= "release";
			public  const string ActivationVersion	= "activationversion";
            public const string Family = "family";
            public const string CompleteName = "completename";
            
			public const string ProductId = "prodid";
			public const string EditionId = "editionid";
			public  const string ProducerKey	= "pk";
		}

		//=====================================================================
		public class Element
		{
			public static string ActivationKeys		= "ActivationKeys";
			public static string ActivationKey		= "ActivationKey";
			public static string CalFile			= "CalFile";
			public static string CalFiles			= "CalFiles";
			public static string Configuration		= "Configuration";
			public static string Errors				= "Errors";
			public static string Error				= "Error";
			public static string Export				= "Export";
			public static string LicensedFile		= "LicensedFile";
			public static string LicensedFiles		= "LicensedFiles";
			public static string Producer			= "Producer";
			public static string ProducerInfo		= "ProducerInfo";	
			public static string Product			= "Product";
			public static string ReturnValues		= "ReturnValues";
			public static string SalesModule		= "SalesModule";
			public static string SalesModules		= "SalesModules";
			public static string SalesModuleInfo	= "SalesModuleInfo";
			public static string Serial				= "Serial";
			public static string UserInfoFile		= "UserInfoFile";
			public static string UserId				= "UserId";
			public static string ActivationInfo		= "ActivationInfo";
			public static string ShowFormForMluChargeChoice = "ShowFormForMluChargeChoice";
			// TODO
			/*Vedere se si possono eliminare queste ed utilizzare l'oggetto UserInfo*/
			public static string UserInfo			= "UserInfo";
			public static string Company			= "Company";
			public static string VatNumber			= "VatNumber";
			public static string Country			= "Country";
			public static string CodFisc			= "CodFisc";
			public static string ProductHashCode	= "ProductHashCode";

			public static string ProducerKey		= "ProducerKey";
		}

		
	}
}
