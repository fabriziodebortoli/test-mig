using System;
using System.Diagnostics;
using System.Windows.Forms;

using Microarea.TaskBuilderNet.Core.MenuManagerLoader;
using Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls;
using Microarea.TaskBuilderNet.Core.TaskschedulerEngine;

namespace Microarea.Console.Core.TaskSchedulerWindowsControls
{
	/// <summary>
	/// Summary description for SelectMenuCommandDlg.
	/// </summary>
	public partial class SelectMenuCommandDlg : MenuManagerDialog
	{
		private TaskTypeEnum commandType = TaskTypeEnum.Batch;
		private string selectedCommandItemObject = String.Empty;
		private string selectedCommandArguments= String.Empty;
		
		#region SelectMenuCommandDlg constructors

		//--------------------------------------------------------------------------------------------------------------------------------
		public SelectMenuCommandDlg(MenuLoader aMenuLoader, TaskTypeEnum aCommandType, string currentCommand) : base(aMenuLoader)
		{
			InitializeComponent();

			//Ingrandisco la finestra (qui e non in designer per evitare problemi nel posizionamento
			//dei control
			this.Size = new System.Drawing.Size(900, 600);

			Debug.Assert(menuLoader != null && menuLoader.MenuInfo != null, "SelectMenuCommandDlg Constructor Warning: invalid menu loader");

			commandType = aCommandType;
			
			selectedCommandItemObject = currentCommand;
			selectedCommandArguments = String.Empty;
		}

		#endregion
		
		//--------------------------------------------------------------------------------------------------------------------------------
		public TaskTypeEnum CommandType { get { return commandType; } }
		//--------------------------------------------------------------------------------------------------------------------------------
		public string SelectedCommandItemObject { get { return selectedCommandItemObject; } }
		//--------------------------------------------------------------------------------------------------------------------------------
		public string SelectedCommandArguments { get { return selectedCommandArguments; } }
		
		//--------------------------------------------------------------------------------------------------------------------------------
		private bool CheckValidMenuCommandSelection()
		{
			
			if (selectedCommandNode == null)
			{
				MessageBox.Show(TaskSchedulerWindowsControlsStrings.NoMenuCommandSelectionErrMsg);
				return false;
			}
			if 
				(
				!selectedCommandNode.IsRunDocument &&
				!selectedCommandNode.IsRunBatch &&
				!selectedCommandNode.IsRunFunction &&
				!selectedCommandNode.IsRunReport &&
				!selectedCommandNode.IsRunExecutable
				)
			{
				MessageBox.Show(TaskSchedulerWindowsControlsStrings.MenuCommandTypeInvalidErrMsg);
				return false;
			}
			if 
				(
				(commandType == TaskTypeEnum.Batch && !selectedCommandNode.IsRunBatch) ||
				(commandType == TaskTypeEnum.Function && !selectedCommandNode.IsRunFunction) ||
				(commandType == TaskTypeEnum.Report && !selectedCommandNode.IsRunReport) ||
				(commandType == TaskTypeEnum.Executable && !selectedCommandNode.IsRunExecutable) ||
				((commandType == TaskTypeEnum.DataExport || commandType == TaskTypeEnum.DataImport) && !selectedCommandNode.IsRunDocument)
				)
			{
				if (MessageBox.Show(TaskSchedulerWindowsControlsStrings.DifferentCommandTypeWarningMsg, TaskSchedulerWindowsControlsStrings.DifferentCommandTypeWarningcaption, MessageBoxButtons.YesNo) == DialogResult.No)
					return false;
				
				if (selectedCommandNode.IsRunBatch)
					commandType = TaskTypeEnum.Batch;
				else if (selectedCommandNode.IsRunFunction)
					commandType = TaskTypeEnum.Function;
				else if (selectedCommandNode.IsRunReport)
					commandType = TaskTypeEnum.Report;
				else if (selectedCommandNode.IsRunExecutable)
					commandType = TaskTypeEnum.Executable;
				else if (selectedCommandNode.IsRunDocument)
					commandType = TaskTypeEnum.DataExport;
			}
			return true;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		protected override void OnMenuShowed()
		{
			// Invoke base class implementation
			base.OnMenuShowed();

			if (menuLoader != null && menuLoader.MenuInfo != null)
			{
				MenuManagerWinCtrl.MenuXmlParser = menuLoader.AppsMenuXmlParser;
				
				if (selectedCommandItemObject != null && selectedCommandItemObject != String.Empty)
				{
					switch(commandType)
					{
						case TaskTypeEnum.Batch:
							MenuManagerWinCtrl.SelectBatchNodeFromItemObject(selectedCommandItemObject);
							break;
						case TaskTypeEnum.Function:
							MenuManagerWinCtrl.SelectFunctionNodeFromItemObject(selectedCommandItemObject);
							break;
						case TaskTypeEnum.Report:
							MenuManagerWinCtrl.SelectReportNodeFromItemObject(selectedCommandItemObject);
							break;
						case TaskTypeEnum.Executable:
							MenuManagerWinCtrl.SelectExeNodeFromItemObject(selectedCommandItemObject);
							break;
						case TaskTypeEnum.DataImport:
						case TaskTypeEnum.DataExport:
							MenuManagerWinCtrl.SelectDocumentNodeFromItemObject(selectedCommandItemObject);
							break;
						default:
							Debug.Fail("Error in SelectMenuCommandDlg.OnMenuShowed");
							break;
					}
				}
			}
		}

		//---------------------------------------------------------------------------------------------
		protected override void OnSelectedCommandChanged(MenuMngCtrlEventArgs e)
		{
			selectedCommandItemObject = (e != null) ? e.ItemObject : String.Empty;
			selectedCommandArguments = (e != null) ? e.Arguments : String.Empty;
		}
		
		//---------------------------------------------------------------------------------------------
		protected override bool OnOk()
		{
			if (!base.OnOk())
				return false;

			return CheckValidMenuCommandSelection();
		}
	}
}
