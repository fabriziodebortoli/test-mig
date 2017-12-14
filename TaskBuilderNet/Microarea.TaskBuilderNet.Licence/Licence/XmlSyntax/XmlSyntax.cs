
namespace Microarea.TaskBuilderNet.Licence.Licence.XmlSyntax
{
	//=========================================================================
	public class Consts
	{
		public const string ClientNet = "ClientNet";
		public const string UserLicence = "LicenzaUtente";
		public const string UserOfficeLicence = "OfficeLicence";
		public const string IsoAttribute = "iso";
		public const char IsoAttributeSeparator = ',';
		public const string WebFramework = "WebFramework";
		public const string EasyLookLicence = "UnNamedCalLicence";

        public const string DevIDIU = "DVIU";//sviluppo internal use, solo microarea, 10 cal

        public const string DevIdUser = "DVLP";//serial tbs development per utente 9 (non è più il dvlp del mago 3x)
        public const string PersIdUSer = "PERS";//serial tbs personal per utente
        public const string PersIdRiv = "KSTR";//serial tbs personal per rivenditore
        public const string DevIdRiv = "KDVR";//serial tbs development per rivenditore

        // public const string DevPLUS = "DVEB";//sviluppo con eb per gold platinum isv
        //public const string DevPLUSs1 = "DVE1";//sviluppo con eb per silver 1 cal
        //public const string DevPLUSs3 = "DVE3";//sviluppo con eb per silver 3 cal


        public const string DemoID = "DEMO";
		public const string ResellerID = "RNFS";
		public const string DistributorID = "DNFS";//in teoria non più usato


        public const string TestID = "TEST";
        public const string BackUpID = "BCKP";
        public const string StandAloneID = "STDL";

		public const string Extensions = "Extensions";
		public const string MDocPlatform = "MagicDocumentsPlatform";

		#region XML attributes
		public const string AttributeHasCal = "hascal";
		public const string AttributeHasSerial = "hasserial";
        public const string AttributeAvailable = "available";
        public const string AttributeMandatory = "mandatory";
		public const string AttributeNamedCal = "namedcal";
		public const string AttributeCalUse = "caluse";
		public const string AttributeMode = "mode";
		public const string AttributeProducer = "producer";
		public const string AttributeProdID = "prodid";
		public const string AttributeInternalCode = "internalcode";
		public const string AttributePrivateCode = "privatecode";
		public const string AttributeMaxCal = "maxcal";
		public const string AttributeCalType = "caltype";
		public const string AttributeEdition = "edition";
		public const string AttributeMasterCal = "mastercal";
		public const string AttributeName = "name";
		public const string AttributePath = "path";
		public const string AttributeLocalize = "localize";
		public const string AttributeContainer = "container";
		public const string AttributeExpression = "expression";
		public const string AttributOnSelfactivated = "onselfactivated";
		#endregion

		#region XML tags
		public const string TagArticleDependency = "SalesModuleDependency";
		public const string TagApplication = "Application";
		public const string TagContainer = "Container";
		public const string TagConfigurations = "Configurations";
		public const string TagFunctionality = "Functionality";
		public const string TagModule = "Module";
		public const string TagIncludeModulesPath = "IncludeModulesPath";
		public const string TagSalesModule = "SalesModule";
		public const string TagShortNames = "ShortNames";
		public const string TagShortName = "ShortName";
		public const string TagAllow = "Allow";
		public const string TagDeny = "Deny";
		public const string TagProduct = "Product";
		#endregion

	}
}