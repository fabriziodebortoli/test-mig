using System;
using System.Resources;
using System.Reflection;

namespace Microarea.Library.WorkFlowWindowsControls
{
	/// <summary>
	/// Summary description for WorkFlowWinCtrlStrings.
	/// </summary>
	// ========================================================================
	public class WorkFlowControlsString
	{
		private static ResourceManager rm = new ResourceManager("Microarea.Library.WorkFlowWindowsControls.Strings", Assembly.GetExecutingAssembly());

		public static string ConnectionErrorMsgFmt			{ get { return rm.GetString("ConnectionErrorMsgFmt"); }}
		public static string ConfirmWorkFlowDeletionMsg		{ get { return rm.GetString("ConfirmWorkFlowDeletionMsg"); }}
		public static string ConfirmWorkFlowDeletionCaption { get { return rm.GetString("ConfirmWorkFlowDeletionCaption"); }}
		public static string ConfirmWorkFlowActivityDeletionMsg { get { return rm.GetString("ConfirmWorkFlowActivityDeletionMsg"); }}
		public static string ConfirmWorkFlowActivityDeletionCaption { get { return rm.GetString("ConfirmWorkFlowActivityDeletionCaption"); }}
		public static string InsertActivityFailedErrorMsgFmt { get { return rm.GetString("InsertActivityFailedErrorMsgFmt"); }}
		
		
	}

	// ========================================================================
	public class WorkFlowTemplatesString
	{
		private static ResourceManager rm = new ResourceManager("Microarea.Library.WorkFlowWindowsControls.Strings", Assembly.GetExecutingAssembly());

		public static string WorkFlowTemplateGridCaption		  { get { return rm.GetString("WorkFlowTemplateGridCaption"); }}
		public static string NewWorkFlowTemplateCaption			  { get { return rm.GetString("NewWorkFlowTemplateCaption"); }}
		public static string NewWorkFlowTemplateTitle			  { get { return rm.GetString("NewWorkFlowTemplateTitle"); }}
		public static string ViewWorkFlowTemplateTitle            { get { return rm.GetString("ViewWorkFlowTemplateTitle"); }}
		public static string ViewWorkFlowTemplateCaption          { get { return rm.GetString("ViewWorkFlowTemplateCaption"); }}
		public static string DataGridTemplateNameColumnHeaderText { get { return rm.GetString("DataGridTemplateNameColumnHeaderText"); }}
		public static string DataGridTemplateDescColumnHeaderText { get { return rm.GetString("DataGridTemplateDescColumnHeaderText"); }}
		public static string EmptyTemplateNameError				  { get { return rm.GetString("EmptyTemplateNameError"); }}
		public static string NameAlreadyUsedErrMsgFmt               { get { return rm.GetString("NameAlreadyUsedErrMsgFmt"); }}
		public static string ConfirmWorkFlowTemplateDeletionCaption	{ get { return rm.GetString("ConfirmWorkFlowTemplateDeletionCaption"); }}
		public static string ConfirmWorkFlowTemplateDeletionMsg		{ get { return rm.GetString("ConfirmWorkFlowTemplateDeletionMsg"); }}

	}

	// ========================================================================
	public class WorkFlowTemplateActionsString
	{
		private static ResourceManager rm = new ResourceManager("Microarea.Library.WorkFlowWindowsControls.Strings", Assembly.GetExecutingAssembly());

		public static string NoneConfiguredActionsError			{ get { return rm.GetString("NoneConfiguredActionsError"); }}

	}

	// ========================================================================
	public class WorkFlowTemplateStatesString
	{
		private static ResourceManager rm = new ResourceManager("Microarea.Library.WorkFlowWindowsControls.Strings", Assembly.GetExecutingAssembly());

		public static string NoneConfiguredStatesError			{ get { return rm.GetString("NoneConfiguredStatesError"); }}

	}

	// ========================================================================
	public class WorkFlowActionsString
	{
		private static ResourceManager rm = new ResourceManager("Microarea.Library.WorkFlowWindowsControls.Strings", Assembly.GetExecutingAssembly());

		public static string WorkFlowActionGridCaption { get { return rm.GetString("WorkFlowActionGridCaption"); }}
		public static string DataGridActivityNameColumnHeaderText { get { return rm.GetString("DataGridActivityNameColumnHeaderText"); }}
		public static string DataGridActivityDescColumnHeaderText { get { return rm.GetString("DataGridActivityDescColumnHeaderText"); }}
	}

	// ========================================================================
	public class WorkFlowStatesString
	{
		private static ResourceManager rm = new ResourceManager("Microarea.Library.WorkFlowWindowsControls.Strings", Assembly.GetExecutingAssembly());

		public static string WorkFlowStateGridCaption { get { return rm.GetString("WorkFlowActionGridCaption"); }}
		public static string DataGridStateNameColumnHeaderText { get { return rm.GetString("DataGridStateNameColumnHeaderText"); }}
		public static string DataGridStateDescColumnHeaderText { get { return rm.GetString("DataGridStateDescColumnHeaderText"); }}
	}

	// ========================================================================
	public class WorkFlowUsersString
	{
		private static ResourceManager rm = new ResourceManager("Microarea.Library.WorkFlowWindowsControls.Strings", Assembly.GetExecutingAssembly());

		public static string WorkFlowUserGridCaption { get { return rm.GetString("WorkFlowUserGridCaption"); }}
		public static string DataGridWorkFlowMemberColumnHeaderText { get { return rm.GetString("DataGridWorkFlowMemberColumnHeaderText"); }}
		public static string DataGridUserNameColumnHeaderText { get { return rm.GetString("DataGridUserNameColumnHeaderText"); }}
		public static string DataGridUserEMailColumnHeaderText { get { return rm.GetString("DataGridUserEMailColumnHeaderText"); }}
	}

	// ========================================================================
	public class WorkFlowTransitionsString
	{
		private static ResourceManager rm = new ResourceManager("Microarea.Library.WorkFlowWindowsControls.Strings", Assembly.GetExecutingAssembly());

		public static string DataGridFromActivityColumnHeaderText { get { return rm.GetString("DataGridFromActivityColumnHeaderText"); }}
		public static string DataGridToActivityColumnHeaderText   { get { return rm.GetString("DataGridToActivityColumnHeaderText"); }}
		public static string DataGridToStateColumnHeaderText      { get { return rm.GetString("DataGridToStateColumnHeaderText"); }}
		public static string DataGridOwnerColumnHeaderText { get { return rm.GetString("DataGridOwnerColumnHeaderText"); }}
	}

	// ========================================================================
	public class WorkFlowString
	{
		private static ResourceManager rm = new ResourceManager("Microarea.Library.WorkFlowWindowsControls.Strings", Assembly.GetExecutingAssembly());

		public static string WorkFlowGridCaption					{ get { return rm.GetString("WorkFlowGridCaption"); }}
		public static string NewWorkFlowCaption						{ get { return rm.GetString("NewWorkFlowCaption"); }}
		public static string NewWorkFlowTitle						{ get { return rm.GetString("NewWorkFlowTitle"); }}
		public static string ViewWorkFlowTitle						{ get { return rm.GetString("ViewWorkFlowTitle"); }}
		public static string ViewWorkFlowCaption					{ get { return rm.GetString("ViewWorkFlowCaption"); }}
		public static string DataGridWorkFlowNameColumnHeaderText	{ get { return rm.GetString("DataGridWorkFlowNameColumnHeaderText"); }}
		public static string DataGridWorkFlowDescColumnHeaderText	{ get { return rm.GetString("DataGridWorkFlowDescColumnHeaderText"); }}
		public static string EmptyWorkFlowNameError					{ get { return rm.GetString("EmptyWorkFlowNameError"); }}
		public static string WorkFlowNameAlreadyUsedErrMsgFmt       { get { return rm.GetString("WorkFlowNameAlreadyUsedErrMsgFmt"); }}
		
	}

	// ========================================================================
	public class ContextMenusString
	{
		private static ResourceManager rm = new ResourceManager("Microarea.Library.WorkFlowWindowsControls.Strings", Assembly.GetExecutingAssembly());

		public static string Add { get { return rm.GetString("Add"); }}
		public static string View { get { return rm.GetString("View"); }}
		public static string Delete { get { return rm.GetString("Delete"); }}
		public static string Clone { get { return rm.GetString("Clone"); }}
	}

}
