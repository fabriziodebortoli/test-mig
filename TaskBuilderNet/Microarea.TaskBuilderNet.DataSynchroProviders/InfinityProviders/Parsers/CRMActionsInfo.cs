using System.Collections.Generic;

namespace Microarea.TaskBuilderNet.DataSynchroProviders.InfinityProviders.Parsers
{
	#region Classi di memorizzazione info lette dai file xml delle azioni (utilizzato dal parser)
	///<summary>
	/// Nodo di tipo Field
	///</summary>
	//================================================================================
    internal class CRMField
	{
		public string Target { get; set; }
		public string Source { get; set; }
		public bool Key { get; set; }
		public bool Mandatory { get; set; }
		public string DeletePrefix  { get; set; }
		public string DeleteKey { get; set; }
        public bool InternalUse { get; set; }

		//--------------------------------------------------------------------------------
		public CRMField()
		{
		}

		///<summary>
		/// Copy-constructor
		///</summary>
		//--------------------------------------------------------------------------------
		public CRMField(CRMField field)
		{
			Target = field.Target;
			Source = field.Source;
			Key = field.Key;
			Mandatory = field.Mandatory;
			DeletePrefix = field.DeletePrefix;
			DeleteKey = field.DeleteKey;
			InternalUse = field.InternalUse;
		}
	}

	///<summary>
	/// Nodo di tipo Action/Subaction
	///</summary>
	//================================================================================
    internal class CRMAction
	{
        public string ActionName { get; set; }
        public string FatherActionName { get; set; }
        public bool BaseAction { get; set; }
        public string MasterTable { get; set; }

        public List<CRMField> Fields = new List<CRMField>(); // ordinati per nr di sequenza crescente
		public string Select { get; set; }
        public string From { get; set; }
        public string IncrFrom { get; set; }
        public string Where { get; set; }
        public string MassiveWhere { get; set; }
        public string IncrWhere { get; set; }
        public bool IgnoreEmptyValue { get; set; }
        public bool IgnoreError { get; set; }
        public bool SkipForDelete { get; set; }

        public List<CRMAction> Subactions = new List<CRMAction>(); //Subactions
        public List<string> SubactionsParams = new List<string>(); // params per subactions

		// lista di azioni aggiunte dalle personalizzazioni che vanno in append all'azione che le contiene
		public List<CRMAction> AppendActions = new List<CRMAction>(); 

		//--------------------------------------------------------------------------------
        public CRMAction()
        {
            IgnoreEmptyValue = false;
            SkipForDelete = false;
        }

		///<summary>
		/// Copy-constructor
		///</summary>
		//--------------------------------------------------------------------------------
		public CRMAction(CRMAction action)
		{
			ActionName = action.ActionName;
			BaseAction = action.BaseAction;

			foreach (CRMField field in action.Fields)
				Fields.Add(field);

			Select = action.Select;
			From = action.From;
			Where = action.Where;
			MassiveWhere = action.MassiveWhere;
			IgnoreEmptyValue = action.IgnoreEmptyValue;
			IgnoreError = action.IgnoreError;
			SkipForDelete = action.SkipForDelete;

			foreach (CRMAction subA in action.Subactions)
				Subactions.Add(new CRMAction(subA));

			foreach (string param in action.SubactionsParams)
				SubactionsParams.Add(param);

			foreach (CRMAction appendA in action.AppendActions)
				AppendActions.Add(new CRMAction(appendA));
		}
        
	}

	///<summary>
	/// Classi per il caricamento in memoria delle informazioni lette dai file che 
	/// descrivono le varie Actions del CRM
	///</summary>
	//================================================================================
    internal class CRMActionInfo
	{
        public List<CRMAction> Actions { get; set; }

		//--------------------------------------------------------------------------------
		public CRMActionInfo()
		{
            Actions = new List<CRMAction>(); 
		}

		///<summary>
		/// Copy-constructor
		///</summary>
		//--------------------------------------------------------------------------------
		public CRMActionInfo(CRMActionInfo crmInfo)
		{
			Actions = new List<CRMAction>();

			foreach (CRMAction action in crmInfo.Actions)
				Actions.Add(new CRMAction(action));
		}
	}
	# endregion
}
