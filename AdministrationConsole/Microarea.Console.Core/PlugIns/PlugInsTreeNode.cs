using System;
using System.Windows.Forms;

namespace Microarea.Console.Core.PlugIns
{
	///<summary>
	/// PlugInTreeNode
	/// Classe che gestisce tutti i nodi visualizzati nel tree della MicroareaConsole
	///</summary>
    //============================================================================
	public class PlugInTreeNode : TreeNode
    {
        #region DataMember Privati
		private string	assemblyName	= String.Empty;
		private Type	assemblyType	= null;
		private string	type			= String.Empty;
		private string	id				= String.Empty;
		private string	companyId		= String.Empty;
		private string  roleId			= String.Empty;
		private string  provider        = String.Empty;
		private bool    isValid         = true;
		private bool	useEasyAttachment = false;
        private bool    readOnly          = false;
		#endregion

		/// <summary>
		/// Id
		/// Rappresenta l'Id del nodo (es, se è un nodo Azienda rappresenta il
		/// CompanyId, se è un nodo Utente rappresenta la LoginId, etc.)
		/// </summary>
		//---------------------------------------------------------------------
		public string Id { get { return id; } set { id = value; } }

		/// <summary>
		/// CompanyId
		/// Per i nodi che hanno una dipendenza dal CompanyId, contiene l'Id della company (es. CompanyUser)
		/// </summary>
		//---------------------------------------------------------------------
		public string CompanyId { get { return companyId; } set { companyId = value; } }

		/// <summary>
		/// Provider
		/// Per i nodi Company, il provider (SQLServer o ORACLE) utilizzato dal database di quella company
		/// </summary>
		//---------------------------------------------------------------------
		public string Provider { get { return provider; } set { provider = value; } }

		/// <summary>
		/// IsValid
		/// Per i nodi Company, utilizzato dalla migrazione (Mago 1.2 -> Mago 2.0) poi a disposizione per altri usi
		/// </summary>
		//---------------------------------------------------------------------
		public bool IsValid { get { return isValid; } set { isValid = value; } }

		/// <summary>
		/// RoleId
		/// Per i nodi che hanno una dipendenza dal RoleId, contiene l'Id del ruolo da cui il nodo dipende
		/// </summary>
		//---------------------------------------------------------------------
		public string RoleId { get { return this.roleId; } set { this.roleId = value; } }

		/// <summary>
		/// AssemblyName
		/// Nome dell'assembly a cui il TreeNode appartiene (SysAdmin piuttosto che ApplicationDBAmin)
		/// </summary>
		//---------------------------------------------------------------------
		public string AssemblyName { get { return assemblyName; } set { assemblyName = value; } }

		/// <summary>
		/// AssemblyType
		/// Tipo dell'assembly a cui il TreeNode appartiene (Microarea.SysAdmin)
		/// </summary>
		//---------------------------------------------------------------------
		public Type AssemblyType { get { return assemblyType; } set { assemblyType = value; } }

		/// <summary>
		/// Type
		/// Tipologia dei Nodi. In particolare il SysAdmin utilizza i seguenti:
		/// ContenitoreUtenti,ContenitoreProvider,ContenitoreRuoliAzienda,ContenitoreUtentiAzienda,
		/// ContenitoreUtentiRuolo,ContenitoreAziende,
		/// Ruolo,Azienda,UtenteAzienda,UtenteRuoloAzienda,Utente,Provider
		/// </summary>
		//---------------------------------------------------------------------
		public string Type { get { return type; } set { type = value; } }

		/// <summary>
		/// UseEasyAttachment
		/// Vale per i nodi CompanyId, ovvero se sulla company e' stato specificato che usa 
		/// il modulo EasyAttachment
		/// </summary>
		//---------------------------------------------------------------------
		public bool UseEasyAttachment { get { return useEasyAttachment; } set { useEasyAttachment = value; } }
        public bool ReadOnly          { get { return readOnly; } set { readOnly = value; } }
		//---------------------------------------------------------------------------
		public new PlugInTreeNode Parent { get { return (PlugInTreeNode)base.Parent as PlugInTreeNode; } }
		//---------------------------------------------------------------------------
		public new PlugInTreeNode PrevNode { get { return (PlugInTreeNode)base.PrevNode as PlugInTreeNode; } }
		//---------------------------------------------------------------------------
		public new PlugInTreeNode NextNode { get { return (PlugInTreeNode)base.NextNode as PlugInTreeNode; } }

		//---------------------------------------------------------------------------
		public int NodesCount { get { return base.Nodes.Count; } }

        #region Costruttori
        //---------------------------------------------------------------------------
		public PlugInTreeNode(string aNodeText, string aNodeId, int aImageIndex, int aStateImageIndex)
		{
			Text = aNodeText;
			id = aNodeId;
			
			if (aImageIndex != -1)
				ImageIndex = aImageIndex;

			if (aStateImageIndex != -1)
				StateImageIndex = aStateImageIndex;
		}

		//---------------------------------------------------------------------------
		public PlugInTreeNode(string aNodeText, string aNodeId) : this(aNodeText, aNodeId, -1, -1)
		{
		}

		//---------------------------------------------------------------------------
		public PlugInTreeNode(string aNodeText) : this(aNodeText, String.Empty, -1, -1)
		{
		}

		//---------------------------------------------------------------------------
		public PlugInTreeNode() : this(String.Empty)
		{
        }
        #endregion

		//---------------------------------------------------------------------------
        public static string GetUsersDefaultText { get { return PlugInsTreeViewStrings.UsersDefaultText; } }

        //---------------------------------------------------------------------------
        public static string GetCompaniesDefaultText { get { return PlugInsTreeViewStrings.CompaniesDefaultText; } }
      
		//---------------------------------------------------------------------------
		public bool IsDescendantOf(PlugInTreeNode aNode)
		{
			if (aNode == null || aNode == this || aNode.Nodes == null || aNode.Nodes.Count == 0)
				return false;

            PlugInTreeNode tmpParentNode = this.Parent;
			while(tmpParentNode != null)
			{ 
				if (tmpParentNode == aNode)
					return true;
				tmpParentNode = tmpParentNode.Parent;
			}
			return false;
		}

		//---------------------------------------------------------------------------
		public void UpdateSiblingsStateImageIndexes()
		{
			if (StateImageIndex >= GetDummyStateImageIndex)
			{
                PlugInTreeNode prevNode = this.PrevNode;
				if (prevNode != null && prevNode.StateImageIndex < GetDummyStateImageIndex)
					prevNode.StateImageIndex = GetDummyStateImageIndex;

                PlugInTreeNode nextNode = this.NextNode;
				if (nextNode != null && nextNode.StateImageIndex < GetDummyStateImageIndex)
					nextNode.StateImageIndex = GetDummyStateImageIndex;
			}
			else
				UpdateStateImageIndexFromSiblings();
		}
		
		//---------------------------------------------------------------------------
		public void UpdateStateImageIndexFromSiblings()
		{
            PlugInTreeNode prevNode = this.PrevNode;
			if (prevNode != null && prevNode.StateImageIndex >= GetDummyStateImageIndex)
				if (StateImageIndex < GetDummyStateImageIndex)
					StateImageIndex = GetDummyStateImageIndex;
			else
			{
                PlugInTreeNode nextNode = this.NextNode;
				if (nextNode != null && nextNode.StateImageIndex >= GetDummyStateImageIndex)
					if (StateImageIndex < GetDummyStateImageIndex)
						StateImageIndex = GetDummyStateImageIndex;
			}
		}

		# region Set Image
		//---------------------------------------------------------------------------
		public void SetUndefinedStateImage()
		{
			StateImageIndex = GetDummyStateImageIndex;
		}

		//---------------------------------------------------------------------------
		public void SetSearchStateImage()
		{
			StateImageIndex = GetSearchStateImageIndex;
		}

		//---------------------------------------------------------------------------
		public void SetKeyStateImage()
		{
			StateImageIndex = GetKeyStateImageIndex;
		}

		//---------------------------------------------------------------------------
		public void SetLockStateImage()
		{
			StateImageIndex = GetLockStateImageIndex;
		}

		//---------------------------------------------------------------------------
		public void SetCheckStateImage()
		{
			StateImageIndex = GetCheckStateImageIndex;
		}

		//---------------------------------------------------------------------------
		public void SetUncheckStateImage()
		{
			StateImageIndex = GetUncheckStateImageIndex;
		}

		//---------------------------------------------------------------------------
		public void SetGreenSemaphoreStateImage()
		{
			StateImageIndex = GetGreenSemaphoreStateImageIndex;
		}

		//---------------------------------------------------------------------------
		public void SetRedSemaphoreStateImage()
		{
			StateImageIndex = GetRedSemaphoreStateImageIndex;
		}

		//---------------------------------------------------------------------------
		public void SetGreenFlagStateImage()
		{
			StateImageIndex = GetGreenFlagStateImageIndex;
		}

		//---------------------------------------------------------------------------
		public void SetRedFlagStateImage()
		{
			StateImageIndex = GetRedFlagStateImageIndex;
		}

		//---------------------------------------------------------------------------
		public void SetArrivalFlagStateImage()
		{
			StateImageIndex = GetArrivalFlagStateImageIndex;
		}

		//---------------------------------------------------------------------------
		public void SetInformationStateImage()
		{
			StateImageIndex = GetInformationStateImageIndex;
		}
		# endregion

		# region Metodi per gestire le immagini e le icone da associare ai nodi
		//---------------------------------------------------------------------------
		public static int GetDefaultImageIndex { get { return PlugInsTreeView.GetDefaultImageIndex; } }
		//---------------------------------------------------------------------------
		public static int GetCompanyDefaultImageIndex { get { return PlugInsTreeView.GetCompanyDefaultImageIndex; } }
		//---------------------------------------------------------------------------
		public static int GetCompaniesDefaultImageIndex { get { return PlugInsTreeView.GetCompaniesDefaultImageIndex; } }
		//---------------------------------------------------------------------------
		public static int GetLoginsDefaultImageIndex { get { return PlugInsTreeView.GetLoginsDefaultImageIndex; } }
		//---------------------------------------------------------------------------
		public static int GetUserDefaultImageIndex { get { return PlugInsTreeView.GetUserDefaultImageIndex; } }
		//---------------------------------------------------------------------------
		public static int GetUsersGroupDefaultImageIndex { get { return PlugInsTreeView.GetUsersGroupDefaultImageIndex; } }
		//---------------------------------------------------------------------------
		public static int GetUsersDefaultImageIndex { get { return PlugInsTreeView.GetUsersDefaultImageIndex; } }
		//---------------------------------------------------------------------------
		public static int GetProfilesDefaultImageIndex { get { return PlugInsTreeView.GetProfilesDefaultImageIndex; } }
		//---------------------------------------------------------------------------
		public static int GetProfileDefaultImageIndex { get { return PlugInsTreeView.GetProfileDefaultImageIndex; } }
		//---------------------------------------------------------------------------
		public static int GetRoleDefaultImageIndex { get { return PlugInsTreeView.GetRoleDefaultImageIndex; } }
		//---------------------------------------------------------------------------
		public static int GetRolesDefaultImageIndex { get { return PlugInsTreeView.GetRolesDefaultImageIndex; } }
		//---------------------------------------------------------------------------
		public static int GetDatabaseDefaultImageIndex { get { return PlugInsTreeView.GetDatabaseDefaultImageIndex; } }
		//---------------------------------------------------------------------------
		public static int GetDatabaseBackupDefaultImageIndex { get { return PlugInsTreeView.GetDatabaseBackupDefaultImageIndex; } }
		//---------------------------------------------------------------------------
		public static int GetDatabaseManagementDefaultImageIndex { get { return PlugInsTreeView.GetDatabaseManagementDefaultImageIndex; } }
		//---------------------------------------------------------------------------
		public static int GetSqlServerDefaultImageIndex { get { return PlugInsTreeView.GetSqlServerDefaultImageIndex; } }
		//---------------------------------------------------------------------------
		public static int GetSqlServerGroupDefaultImageIndex { get { return PlugInsTreeView.GetSqlServerGroupDefaultImageIndex; } }
		//---------------------------------------------------------------------------
		public static int GetSqlUserDefaultImageIndex { get { return PlugInsTreeView.GetSqlUserDefaultImageIndex; } }
		//---------------------------------------------------------------------------
		public static int GetTableDefaultImageIndex { get { return PlugInsTreeView.GetTableDefaultImageIndex; } }
		//---------------------------------------------------------------------------
		public static int GetViewDefaultImageIndex { get { return PlugInsTreeView.GetViewDefaultImageIndex; } }
		//---------------------------------------------------------------------------
		public static int GetStoredProcedureDefaultImageIndex { get { return PlugInsTreeView.GetStoredProcedureDefaultImageIndex; } }
		//---------------------------------------------------------------------------
		public static int GetToolsDefaultImageIndex { get { return PlugInsTreeView.GetToolsDefaultImageIndex; } }
		//---------------------------------------------------------------------------
		public static int GetConfigSettingsDefaultImageIndex { get { return PlugInsTreeView.GetConfigSettingsDefaultImageIndex; } }
		//---------------------------------------------------------------------------
		public static int GetApplicationImageIndex { get { return PlugInsTreeView.GetApplicationImageIndex; } }
		//---------------------------------------------------------------------------
		public static int GetModuleImageIndex { get { return PlugInsTreeView.GetModuleImageIndex; } }
		//---------------------------------------------------------------------------
		public static int GetMessagesImageIndex { get { return PlugInsTreeView.GetMessagesImageIndex; } }
		//---------------------------------------------------------------------------
		public static int GetTableUncheckedImageIndex { get { return PlugInsTreeView.GetTableUncheckedImageIndex; } }
		//---------------------------------------------------------------------------
		public static int GetViewUncheckedImageIndex { get { return PlugInsTreeView.GetViewUncheckedImageIndex; } }
		//---------------------------------------------------------------------------
		public static int GetStoredProcedureUncheckedImageIndex { get { return PlugInsTreeView.GetStoredProcedureUncheckedImageIndex; } }
		//---------------------------------------------------------------------------
		public static int GetColumnDefaultImageIndex { get { return PlugInsTreeView.GetColumnDefaultImageIndex; } }
		//---------------------------------------------------------------------------
		public static int GetInformationImageIndex { get { return PlugInsTreeView.GetInformationImageIndex; } }
		//------------------------------------------------------------------------
		public static int GetDummyStateImageIndex { get { return PlugInsTreeView.GetDummyStateImageIndex; } }
		//------------------------------------------------------------------------
		public static int GetSearchStateImageIndex { get { return PlugInsTreeView.GetSearchStateImageIndex; } }
		//------------------------------------------------------------------------
		public static int GetKeyStateImageIndex { get { return PlugInsTreeView.GetKeyStateImageIndex; } }
		//------------------------------------------------------------------------
		public static int GetLockStateImageIndex { get { return PlugInsTreeView.GetLockStateImageIndex; } }
		//------------------------------------------------------------------------
		public static int GetCheckStateImageIndex { get { return PlugInsTreeView.GetCheckStateImageIndex; } }
		//------------------------------------------------------------------------
		public static int GetUncheckStateImageIndex { get { return PlugInsTreeView.GetUncheckStateImageIndex; } }
		//------------------------------------------------------------------------
		public static int GetGreenSemaphoreStateImageIndex { get { return PlugInsTreeView.GetGreenSemaphoreStateImageIndex; } }
		//------------------------------------------------------------------------
		public static int GetRedSemaphoreStateImageIndex { get { return PlugInsTreeView.GetRedSemaphoreStateImageIndex; } }
		//------------------------------------------------------------------------
		public static int GetGreenFlagStateImageIndex { get { return PlugInsTreeView.GetGreenFlagStateImageIndex; } }
		//------------------------------------------------------------------------
		public static int GetRedFlagStateImageIndex { get { return PlugInsTreeView.GetRedFlagStateImageIndex; } }
		//------------------------------------------------------------------------
		public static int GetArrivalFlagStateImageIndex { get { return PlugInsTreeView.GetArrivalFlagStateImageIndex; } }
		//------------------------------------------------------------------------
		public static int GetCompaniesToMigrateImageIndex { get { return PlugInsTreeView.GetCompaniesToMigrateStateImageIndex; } }
		//------------------------------------------------------------------------
		public static int GetInformationStateImageIndex { get { return PlugInsTreeView.GetInformationStateImageIndex; } }
		//------------------------------------------------------------------------
		public static int GetGreenLampStateImageIndex { get { return PlugInsTreeView.GetGreenLampStateImageIndex; } }
		//------------------------------------------------------------------------
		public static int GetRedLampStateImageIndex { get { return PlugInsTreeView.GetRedLampStateImageIndex; } }
		//------------------------------------------------------------------------
		public static int GetResultGreenStateImageIndex { get { return PlugInsTreeView.GetResultGreenStateImageIndex; } }
		//------------------------------------------------------------------------
		public static int GetResultRedStateImageIndex { get { return PlugInsTreeView.GetResultRedStateImageIndex; } }
		//------------------------------------------------------------------------
		public static int GetErrorStateImageIndex { get { return PlugInsTreeView.GetErrorStateImageIndex; } }
		//------------------------------------------------------------------------
		public static int GetWarningStateImageIndex { get { return PlugInsTreeView.GeWarningStateImageIndex; } }
		//------------------------------------------------------------------------
		public static int GetEasyAttachmentStateImageIndex { get { return PlugInsTreeView.GetEasyAttachmentStateImageIndex; } }
        //------------------------------------------------------------------------
        public static int GetTBSenderStateImageIndex { get { return PlugInsTreeView.GetTBSenderStateImageIndex; } }

		//------------------------------------------------------------------------
		public static int GetDataSynchroStateImageIndex { get { return PlugInsTreeView.GetDataSynchroStateImageIndex; } }
		#endregion
	}
}