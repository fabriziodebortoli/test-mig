using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Microarea.Console.Core.SecurityLibrary;
using Microarea.TaskBuilderNet.Core.MenuManagerLoader;
using Microarea.TaskBuilderNet.Core.SecurityLayer;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Plugin.SecurityAdmin.WizardForms
{

	public enum WizardOperationType
	{
		Protect 		= 0,
		Unprotect		= 1,
		DeleteGrants	= 2,
		SetGrants		= 3
	}

	public enum WizardParametersType
	{
		AllObjects						= 0,
		SelectObjectsType				= 1,
		AllUsersAndRoles				= 2,
		AllUsers						= 3,
		AllRoles						= 4,		
		SelectedUsersAndRoles			= 5,
		SelectedUserOrRoleByTree		= 6,
		ApplyGrantsAllObjects			= 7,
		ApplyGrantsOnlyProtectedObjects = 8
	}
	//=========================================================================
	public class WizardParameters
	{
		
		private ShowObjectsTree			showObjectsTree			= null;

		private WizardOperationType		operationType;
		private WizardParametersType	objectSelectionType;
		private WizardParametersType	usersOrRolesSelectionType;
		private WizardParametersType	applyType;

		private ArrayList	obejctTypeArrayList	= null;
		private ArrayList	usersArrayList		= null;
		private ArrayList	rolesArrayList		= null;
		private DataTable	grantsDataTable		= null;
		
		public ShowObjectsTree		ShowObjectsTreeForm		{ get { return showObjectsTree; }		set {showObjectsTree = value;} }
		
		public WizardOperationType	OperationType				{ get { return operationType; }				set {operationType = value;} }
		public WizardParametersType ObjectSelectionType			{ get { return objectSelectionType; }		set {objectSelectionType = value;} }
		public WizardParametersType UsersOrRolesSelectionType	{ get { return usersOrRolesSelectionType; } set {usersOrRolesSelectionType = value;} }
		public WizardParametersType ApplyType					{ get { return applyType; }					set {applyType = value;} }
	
		public ArrayList			ObejctTypeArrayList	{ get { return obejctTypeArrayList; }	set {obejctTypeArrayList = value;} }
		public ArrayList			UsersArrayList		{ get { return usersArrayList; }		set {usersArrayList = value;} }
		public ArrayList			RolesArrayList		{ get { return rolesArrayList; }		set {rolesArrayList = value;} }
		public DataTable			GrantsDataTable		{ get { return grantsDataTable; }		set {grantsDataTable = value;} }
		
		//---------------------------------------------------------------------
		public WizardParameters()
		{
			obejctTypeArrayList = new ArrayList();
			usersArrayList		= new ArrayList();
			rolesArrayList		= new ArrayList();
            grantsDataTable = new DataTable(GrantsString.Grants);

			grantsDataTable.Columns.Add(securityGrants.Grant, Type.GetType("System.String") );
			grantsDataTable.Columns.Add(securityGrants.Inherit, Type.GetType("System.Int32") );
			grantsDataTable.Columns.Add(securityGrants.Role, Type.GetType("System.Int32") );
			grantsDataTable.Columns.Add(securityGrants.User, Type.GetType("System.Int32") );
			grantsDataTable.Columns.Add(securityGrants.Total, Type.GetType("System.Int32") );
			grantsDataTable.Columns.Add(securityGrants.Assign, Type.GetType("System.Int32") );
			grantsDataTable.Columns.Add(securityGrants.OldValue, Type.GetType("System.Int32") );
			grantsDataTable.Columns.Add(securityGrants.GrantMask, Type.GetType("System.Int32") );

		}

        //---------------------------------------------------------------------
        private bool IsControl(int objectTypeId, int type)
        {
            
            int codeType = CommonObjectTreeFunction.GetObjectTypeId((int)SecurityType.Control, showObjectsTree.Connection);

            if (objectTypeId != 7 && objectTypeId != 3 && objectTypeId != 4 && objectTypeId != 5 && objectTypeId != 10 && type == codeType)
                return true;

            return false;
        }

        //---------------------------------------------------------------------
        public void SetGroupGrant(MenuXmlNode aMenuXmlNode, int objectId, int objectTypeId)
		{
			if (showObjectsTree == null || showObjectsTree.Connection == null)
				return;

			if (ObejctTypeArrayList == null || ObejctTypeArrayList.Count == 0)
				return;

			if (objectTypeId == -1)
                objectTypeId = CommonObjectTreeFunction.GetObjectTypeId(aMenuXmlNode, showObjectsTree.Connection);
			if (objectTypeId == -1)
				return;

            foreach (ListViewItem objectType in ObejctTypeArrayList)
			{
				if (objectType.Tag != null)
				{
					if (objectTypeId == (int)objectType.Tag)
					{
						if (showObjectsTree == null || showObjectsTree.Connection == null)
							return;
			
						if (OperationType == WizardOperationType.Protect) 
							showObjectsTree.SetCommandProtection(aMenuXmlNode, true);

						if (OperationType == WizardOperationType.Unprotect) 
							showObjectsTree.SetCommandProtection(aMenuXmlNode, false);

						if (OperationType == WizardOperationType.DeleteGrants) 
							DeleteGrants(aMenuXmlNode);

						if (OperationType == WizardOperationType.SetGrants) 
						{
							if (ApplyType == WizardParametersType.ApplyGrantsAllObjects)
								showObjectsTree.ApplyEasyGrantsOperationToCurrentMenuXmlNode(AllObjectsOperationType.RapidGrants, GrantsDataTable, true, usersArrayList, rolesArrayList, aMenuXmlNode);
							else
								showObjectsTree.ApplyEasyGrantsOperationToCurrentMenuXmlNode(AllObjectsOperationType.RapidGrants, GrantsDataTable, false, usersArrayList, rolesArrayList, aMenuXmlNode);
						}
					}
				}
			}
		}
		//---------------------------------------------------------------------
		private void DeleteGrants(MenuXmlNode aMenuXmlNode)
		{
			if (aMenuXmlNode == null || showObjectsTree == null || showObjectsTree.Connection == null)
				return;

			foreach(ListViewItem roleItem in RolesArrayList)
				if (roleItem.Tag != null)
					CommonObjectTreeFunction.DeleteGrants(aMenuXmlNode, showObjectsTree.CompanyId, true, (int)roleItem.Tag, showObjectsTree.Connection);

			foreach(ListViewItem userItem in UsersArrayList)
				if (userItem.Tag != null)
					CommonObjectTreeFunction.DeleteGrants(aMenuXmlNode, showObjectsTree.CompanyId, false, (int)userItem.Tag, showObjectsTree.Connection);
		}

		//---------------------------------------------------------------------
	}
	//=========================================================================
	public class WizardStringMaker 
	{
		//---------------------------------------------------------------------
        public WizardStringMaker()
		{
			
		}
		//---------------------------------------------------------------------
		public static string GetWelcomeOperationDescription(WizardOperationType operationType)
		{
			switch (operationType)
			{
				case WizardOperationType.Protect:
					return Strings.ProtectDescription;

				case WizardOperationType.Unprotect:
					return Strings.UnProtectDescription;

				case WizardOperationType.DeleteGrants:
					return Strings.CancelGrantsDescription;

				case WizardOperationType.SetGrants:
					return Strings.UnProtectDescription;

				default:
					return String.Empty;
			}
		}
		//---------------------------------------------------------------------
		public static string GetSelectObjectOperationTitle(WizardOperationType operationType)
		{
			switch (operationType)
			{
				case WizardOperationType.Protect:
					return Strings.SelectObjectForProtectionTitle;

				case WizardOperationType.Unprotect:
					return Strings.SelectObjectForUnProtectionTitle;

				case WizardOperationType.DeleteGrants:
					return Strings.SelectObjectForCancelGrantTitle;

				case WizardOperationType.SetGrants:
					return Strings.SelectObjectFoSetGrantsTitle;

				default:
					return String.Empty;
			}
		}
		//---------------------------------------------------------------------
		public static string GetSelectObjectOperationDescription(WizardOperationType operationType)
		{
			switch (operationType)
			{
				case WizardOperationType.Protect:
					return Strings.SelectObjectsForProtectionDescription;

				case WizardOperationType.Unprotect:
					return Strings.SelectObjectsForUnProtectionDescription;
					
				case WizardOperationType.DeleteGrants:
					return Strings.SelectObjectsForCancelGrantsDescription;
					
				case WizardOperationType.SetGrants:
					return Strings.SelectObjectsForSetGrantsDescription;
				
				default:
					return String.Empty;
			}

		}
		//---------------------------------------------------------------------
		public static string GetSummaryOperationTitle(WizardOperationType operationType)
		{
			switch (operationType)
			{
				case WizardOperationType.Protect:
					return Strings.SummaryProtections;

				case WizardOperationType.Unprotect:
					return Strings.SummaryUnProtections;

				case WizardOperationType.DeleteGrants:
					return Strings.SummaryCancelGrants;

				case WizardOperationType.SetGrants:
					return Strings.SummarySetGrants;

				default:
					return String.Empty;
			}
		}
		//---------------------------------------------------------------------
		public static string GetSelectRoleAndUsersTitle(WizardOperationType operationType)
		{
			
			switch (operationType)
			{
				case WizardOperationType.DeleteGrants:
					return Strings.SelectRolesAndUsersForCancelTitle;

				case WizardOperationType.SetGrants:
					return Strings.SelectRolesAndUsersForSetGrantsTitle;

				default:
					return String.Empty;
			}
		}
		//---------------------------------------------------------------------
		public static string GetSelectRoleAndUsersDescription(WizardOperationType operationType)
		{
			
			switch (operationType)
			{
				case WizardOperationType.DeleteGrants:
					return Strings.SelectRolesAndUsersForCancelDescription;

				case WizardOperationType.SetGrants:
					return Strings.SelectRolesAndUsersForSetGrantsDescription;

				default:
					return String.Empty;
			}

		}
		//---------------------------------------------------------------------
		public static string GetSummaryTitleDescription(WizardOperationType operationType)
		{
			
			switch (operationType)
			{
				case WizardOperationType.DeleteGrants:
					return Strings.SummaryCancelGrants;
		
				case WizardOperationType.SetGrants:
					return Strings.SummarySetGrants;

				case WizardOperationType.Unprotect:
					return Strings.SummaryUnProtections;
		
				case WizardOperationType.Protect:
					return Strings.SummaryProtections;
				default:
					return String.Empty;
			}
		}
		//---------------------------------------------------------------------
		public static string  GetGrantsValueDescription(int index)
		{
			switch(index)
			{
				case 0:
					return Strings.NotExist;
				case 2:
                    return Strings.Deny;
				case 3:
                    return Strings.Inheritance;
				case 4:
                    return Strings.Allow;
				default:
					return "";
			}
		}
		//---------------------------------------------------------------------
		public static Stream GetImageByOperationType(WizardOperationType operationType)
		{
			string fileName = "";
			Assembly securityAssembly = Assembly.GetExecutingAssembly();

			switch(operationType)
			{
				case WizardOperationType.DeleteGrants:
					fileName = ".img.DeleteGrants.bmp";
					break;
				case WizardOperationType.Protect:
					fileName = ".img.SetProtection.bmp";
					break;
				case WizardOperationType.SetGrants:
					fileName = ".img.SetGrants.bmp";
					break;
				case WizardOperationType.Unprotect:
					fileName = ".img.DisableProtection.bmp";
					break;
				default:
					fileName = ".img.SetGrants.gif";
					break;
			}
			
			return securityAssembly.GetManifestResourceStream(SecurityConstString.SecurityAdminPlugInNamespace + fileName);
			
		}
	}
	//=========================================================================

}
