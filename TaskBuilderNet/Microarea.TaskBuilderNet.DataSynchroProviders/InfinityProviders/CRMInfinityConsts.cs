
namespace Microarea.TaskBuilderNet.DataSynchroProviders.InfinityProviders
{
	//=========================================================================
	internal class CRMInfinityConnectorConsts
	{
		// attributi dell'XmlDeclaration
		public const string XmlVersion = "1.0";
		public const string XmlEncoding = "iso-8859-1";
		public const string ApplicationIdAttr = "applicationId";

		public const string ActionAttr = "action";
		public const string AddValueAttr = "add";
		public const string DeleteValueAttr = "delete";
		public const string BOPrefixNode = "BO_";

		public const string CRMDateFormat = "yyyy-MM-dd";
	}

	///<summary>
	/// Tag e attributi per il parse dei file con le actions del CRM
	///</summary>
	//=========================================================================
	internal sealed class CRMActionXML
	{
		//-----------------------------------------------------------------
		private CRMActionXML()
		{ }

		//=========================================================================
        internal sealed class Element
		{
			//-----------------------------------------------------------------
			private Element()
			{ }

            public const string Actions = "Actions";
			public const string Action = "Action";
			public const string Fields = "Fields";
			public const string Field = "Field";
			public const string Query = "Query";
			public const string Select = "Select";
			public const string Where = "Where";
            public const string IncrWhere = "IncrementalWhere";
            public const string From = "From";
            public const string IncrFrom = "IncrementalFrom";
            public const string MassiveWhere = "MassiveWhere";
			public const string Subactions = "Subactions";
			public const string Subaction = "Subaction";
		}

		//=========================================================================
        internal sealed class Attribute
		{
			//-----------------------------------------------------------------
			private Attribute()
			{ }

            public const string Name = "name";
            public const string Father = "father";
            public const string Target = "target";
			public const string Source = "source";
			public const string Key = "key";
			public const string Mandatory = "mandatory";
			public const string Table = "table";
			public const string Column = "column";
            public const string IgnoreEmptyValue = "ignoreemptyvalue";
            public const string SkipForDelete = "skipfordelete";
			public const string DeletePrefix = "deleteprefix";
			public const string DeleteKey = "deletekey";
            public const string InternalUse = "internaluse";
			public const string BaseAction = "baseaction";
            public const string IgnoreError = "ignoreerror";
            public const string MasterTable = "mastertable";
        }
	}

	///<summary>
	/// Tag e attributi per il parse dei profili di sincronizzazione
	///</summary>
	//=========================================================================
    internal sealed class CRMInfinitySynchroProfilesXML
	{
		//-----------------------------------------------------------------
		private CRMInfinitySynchroProfilesXML()
		{ }

		//=========================================================================
        internal sealed class Element
		{
			//-----------------------------------------------------------------
			private Element()
			{ }

			public const string SynchroProfiles = "SynchroProfiles";
			public const string SynchroMassiveProfiles = "SynchroMassiveProfiles";
			public const string Documents = "Documents";
			public const string Document = "Document";
			public const string Actions = "Actions";
			public const string Action = "Action";
			public const string SynchroActions = "SynchroActions";
			public const string SynchroAction = "SynchroAction";

			public const string NamespaceMappings = "NamespaceMappings";
			public const string Map = "Map";
		}

		//=========================================================================
        internal sealed class Attribute
		{
			//-----------------------------------------------------------------
			private Attribute()
			{ }

			public const string Name = "name";
			public const string Namespace = "namespace";
			public const string File = "file";
			public const string OnlyMassive = "onlymassive";
            public const string OnlyForDMS = "onlyForDMS";
            public const string iMagoConfigurations = "iMagoConfigurations";
            public const string Type = "type";
			public const string MasterTable = "mastertable";
			public const string PKName = "pkname";
		}
	}

	///<summary>
	/// Tag e attributi per il parse della risposta ritornata dal metodo ExecuteSyncro
	/// del webservice Infinity (da completare)
	///</summary>
	//=========================================================================
    internal sealed class CRMInfinitySynchroResponseXML
	{
		//-----------------------------------------------------------------
		private CRMInfinitySynchroResponseXML()
		{ }

        internal sealed class Element
		{
			//-----------------------------------------------------------------
			private Element()
			{ }

			public const string ExecuteSyncroResult = "ExecuteSyncroResult";
			public const string Process = "Process";
		}

        internal sealed class Attribute
		{
			//-----------------------------------------------------------------
			private Attribute()
			{ }

			public const string Id = "id";
			public const string AtomicLevel = "AtomicLevel";
			public const string GenericResult = "GenericResult";
			public const string Result = "Result";
			public const string ErrorMessage = "ErrorMessage"; 

			public const string ApplicationId = "applicationId";
			public const string Crc = "Crc";
			public const string ErrorCode = "ErrorCode";
			public const string ErrorOwnerCode = "ErrorOwnerCode";
			public const string ErrorTable = "ErrorTable";
			public const string Error = "Error";
		}
	}
}
