
namespace Microarea.TaskBuilderNet.DataSynchroProviders.InfiniteCRM
{
	//=========================================================================
	internal class InfiniteCRMConsts
	{
		public const string Operations = "Operations";
		public const string Username = "username";
		public const string Password = "password";
		public const string ParallelExecution = "parallelexecution";
		
		public const string Operation = "Operation";
		public const string Get = "Get";
		public const string Set = "Set";
		public const string Delete = "Delete";
		public const string ID = "ID";
        public const string ERPKey = "ERPKey";

		public const string OwnerUserID = "OwnerUserID";
		public const string FirstUpdate = "FirstUpdate";
		public const string FirstUpdateUserID = "FirstUpdateUserID";
		public const string LastUpdate = "LastUpdate";
		public const string LastUpdateUserID = "LastUpdateUserID";

		public const string BankAccount = "BankAccount";

		public const string InfiniteCRMDateFormat = "yyyy-MM-dd";


	}

	///<summary>
	/// Tag e attributi per il parse dei file con le entita' del CRM
	///</summary>
	//=========================================================================
	internal sealed class CRMEntityXML
	{
		//-----------------------------------------------------------------
		private CRMEntityXML()
		{ }

		//=========================================================================
		internal sealed class Element
		{
			//-----------------------------------------------------------------
			private Element()
			{ }

			public const string Entity = "Entity";
			public const string Transcoding = "Transcoding";
			public const string Fields = "Fields";
			public const string Field = "Field";
			public const string Query = "Query";
			public const string Select = "Select";
			public const string From = "From";
			public const string Where = "Where";
			public const string MassiveWhere = "MassiveWhere";
			public const string Delete = "Delete";
			public const string SubEntities = "SubEntities";
			public const string SubEntity = "SubEntity";
		}

		//=========================================================================
		internal sealed class Attribute
		{
			//-----------------------------------------------------------------
			private Attribute()
			{ }

			public const string Name = "name";
			public const string Table = "table";
			public const string Field = "field";
			public const string ExternalField = "externalfield";
			public const string Target = "target";
			public const string Entity = "entity";
			public const string Key = "key";
			public const string Mandatory = "mandatory";
			public const string ParentField = "parentfield";
			public const string InternalUse = "internaluse";
			public const string Append = "append";
			public const string Value = "value";
		}
	}

	///<summary>
	/// Tag e attributi per il parse dei profili di sincronizzazione
	///</summary>
	//=========================================================================
	internal sealed class InfiniteCRMSynchroProfilesXML
	{
		//-----------------------------------------------------------------
		private InfiniteCRMSynchroProfilesXML()
		{ }

		//=========================================================================
		internal sealed class Element
		{
			//-----------------------------------------------------------------
			private Element()
			{ }

			public const string SynchroProfiles = "SynchroProfiles";
			public const string Documents = "Documents";
			public const string Document = "Document";
			public const string ICRMEntity = "ICRMEntity";
		}

		//=========================================================================
		internal sealed class Attribute
		{
			//-----------------------------------------------------------------
			private Attribute()
			{ }

			public const string Name = "name";
			public const string Namespace = "namespace";
			public const string Direction = "direction";
			public const string Actions = "actions";
		}
	}

	///<summary>
	/// Tag e attributi per il parse della risposta ritornata dal metodo Execute del webservice Pat
	///</summary>
	//=========================================================================
	internal sealed class InfiniteCRMSynchroResponseXML
	{
		//-----------------------------------------------------------------
		private InfiniteCRMSynchroResponseXML()
		{ }

		internal sealed class Element
		{
			//-----------------------------------------------------------------
			private Element()
			{ }

			public const string Results = "Results";
			public const string Result = "Result";
			public const string Exception = "Exception";
			public const string Message = "Message";
			public const string StackTrace = "StackTrace";
		}

		internal sealed class Attribute
		{
			//-----------------------------------------------------------------
			private Attribute()
			{ }

			public const string Code = "code";
			public const string Operation = "operation";
			public const string Desc = "desc";
			public const string Id = "id";
		}
	}
}
