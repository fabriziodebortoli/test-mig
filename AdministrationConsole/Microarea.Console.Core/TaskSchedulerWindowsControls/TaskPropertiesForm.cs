using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.MenuManagerLoader;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Core.CoreTypes;
using Microarea.TaskBuilderNet.Core.TaskschedulerEngine;
using Microarea.TaskBuilderNet.Core.TaskschedulerEngine.TaskSchedulerObjects;

namespace Microarea.Console.Core.TaskSchedulerWindowsControls
{
	/// <summary>
	/// Summary description for TaskPropertiesForm.
	/// </summary>
	//=================================================================================
	public partial class TaskPropertiesForm : System.Windows.Forms.Form
	{
		private WTEScheduledTaskObj task = null;
		private TBSchedulerControl	currentTBSchedulerControl = null;
		private string				commandParameters = String.Empty;
		private bool				validTaskCode = true;

    	//--------------------------------------------------------------------------------------------------------------------------------
		public TaskPropertiesForm(ref WTEScheduledTaskObj aTask, TBSchedulerControl aTBSchedulerControl)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			currentTBSchedulerControl = aTBSchedulerControl;

			if (aTBSchedulerControl != null)
				GeneralPropertiesPageContent.CurrentConnectionString = CurrentConnectionString;

			CommandTypeComboBox.Items.Clear();
			CommandTypeComboBox.Items.Add(TaskSchedulerWindowsControlsStrings.BatchCommandType);			// 0
			CommandTypeComboBox.Items.Add(TaskSchedulerWindowsControlsStrings.ReportCommandType);			// 1
			CommandTypeComboBox.Items.Add(TaskSchedulerWindowsControlsStrings.FunctionCommandType);			// 2
			CommandTypeComboBox.Items.Add(TaskSchedulerWindowsControlsStrings.ExecutableCommandType);		// 3
			CommandTypeComboBox.Items.Add(TaskSchedulerWindowsControlsStrings.MessageCommandType);			// 4
			CommandTypeComboBox.Items.Add(TaskSchedulerWindowsControlsStrings.MailCommandType);				// 5
			CommandTypeComboBox.Items.Add(TaskSchedulerWindowsControlsStrings.WebPageCommandType);			// 6
			if (currentTBSchedulerControl != null && currentTBSchedulerControl.IsSQLServerDB && !aTBSchedulerControl.IsLiteConsole)
			{
				CommandTypeComboBox.Items.Add(TaskSchedulerWindowsControlsStrings.BackupCompanyDBType);		// 7
				CommandTypeComboBox.Items.Add(TaskSchedulerWindowsControlsStrings.RestoreCompanyDBType);	// 8
			}
			if (currentTBSchedulerControl != null && currentTBSchedulerControl.IsXEngineExtensionLicensed)
			{
				CommandTypeComboBox.Items.Add(TaskSchedulerWindowsControlsStrings.DataExportCommandType);		// 9 o 7
				CommandTypeComboBox.Items.Add(TaskSchedulerWindowsControlsStrings.DataImportCommandType);		// 10 o 8
			}
			if (currentTBSchedulerControl != null && currentTBSchedulerControl.IsEasyLookLicensed)
				CommandTypeComboBox.Items.Add(TaskSchedulerWindowsControlsStrings.DelRunnedReportsCommandType);	// 11, 7 o 9

			if (aTask == null || aTask.IsSequence)
			{
				Debug.Fail("TaskPropertiesForm Constructor Warning: invalid task.");
				return;
			}

			task = aTask;
	
			commandParameters = task.XmlParameters;
			
			GeneralPropertiesPageContent.Task = task;
			
			//AuthenticationPageContent.Task = task;
			//if (task == null || task.ToRunOnDemand)
			//	this.TaskTabControl.Controls.Remove(this.AuthenticationPage);
			
			SchedulingPageContent.Task = task;
			TaskMailNotificationsPageContent.Task = task;

			CommandTypeComboBox.SelectedIndex = GetTypeIndex((TaskTypeEnum)task.Type);
			SetApplicationDateCheckBox.Checked = task.SetApplicationDateBeforeRun;
			RunIconizedCheckBox.Checked = task.RunIconized;
			CloseOnEndCheckBox.Checked = task.CloseOnEnd;
			PrintReportCheckBox.Checked = task.PrintReport;
			WaitForProcessHasExitedCheckBox.Checked = task.WaitForProcessHasExited;
			ValidateDataCheckBox.Checked = task.ValidateData;
			UseInternetExplorerRadioButton.Checked = task.OpenWebPageInInternetExplorer;
			UseDefaultBrowserRadioButton.Checked = !task.OpenWebPageInInternetExplorer;
			CreateNewBrowserInstanceCheckBox.Checked = task.CreateNewBrowserInstance;
			CreateNewBrowserInstanceCheckBox.Enabled = UseDefaultBrowserRadioButton.Checked;

			MessageTextBox.Text = (task.SendMail || task.SendMessage) ? task.MessageContent : String.Empty;
			MailTaskRecipientsControl.Text = task.SendMail ? task.Command : String.Empty;

			NormalReportExecutionRadioButton.Checked = !(task.SendReportAsMailAttachment || task.SaveReportAsFile);
			SendMailWithReportAsAttachmentRadioButton.Checked = task.SendReportAsMailAttachment;
			ReportMailRecipientControl.Text = task.SendReportAsMailAttachment ? task.ReportSendingRecipients : String.Empty;
			
			SaveReportAsFileRadioButton.Checked = task.SaveReportAsFile;
			ReportFileNameTextBox.Text = task.SaveReportAsFile ? task.ReportSavingFileName : String.Empty;

			PDFFormatCheckBox.Checked = task.SendReportAsPDFMailAttachment || task.SaveReportAsPDFFile;
			RDEFormatCheckBox.Checked = task.SendReportAsRDEMailAttachment || task.SaveReportAsRDEFile;
			ExcelFormatCheckBox.Checked = task.SendReportAsExcelMailAttachment || task.SaveReportAsExcelFile;

			CompressAttachmentsCheckBox.Checked = task.SendReportAsCompressedMailAttachment;
			ConcatPDFFilesCheckBox.Checked = task.ConcatReportPDFFiles;

			OverwriteCompanyDBBackupCheckBox.Checked = task.OverwriteCompanyDBBackup;
			VerifyCompanyDBBackupCheckBox.Checked = task.VerifyCompanyDBBackup;

			UpdateCommandPropertiesPage();
			
			CommandTextBox.Text = task.Command;
			if (!ValidateCommandText())
			{
				MessageBox.Show(this, String.Format(TaskSchedulerWindowsControlsStrings.InvalidCommandErrMsgFmt, task.Command));
			}

			TaskTabControl.SelectedTab = GeneralPropertiesPage;
			GeneralPropertiesPageContent.SetFocusToCodeTextBox();
		}

		#region TaskPropertiesForm private methods
		
		//--------------------------------------------------------------------------------------------------------------------------------
		private TaskTypeEnum TypeFromIndex(int idx)
		{
			bool isXEngineExtensionLicensed = (currentTBSchedulerControl != null && currentTBSchedulerControl.IsXEngineExtensionLicensed);
			bool isEasyLookLicensed = (currentTBSchedulerControl != null && currentTBSchedulerControl.IsEasyLookLicensed);
			bool isSQLServerDB = (currentTBSchedulerControl != null && currentTBSchedulerControl.IsSQLServerDB);
		
			switch (idx)
			{
				case  0:
					return TaskTypeEnum.Batch;

				case  1:
					return TaskTypeEnum.Report;

				case  2:
					return TaskTypeEnum.Function;

				case  3:
					return TaskTypeEnum.Executable;

				case  4:
					return TaskTypeEnum.Message;

				case  5:
					return TaskTypeEnum.Mail;

				case  6:
					return TaskTypeEnum.WebPage;
				
				case  7:
					if (isSQLServerDB)
						return TaskTypeEnum.BackupCompanyDB;
					else if (isXEngineExtensionLicensed)
						return TaskTypeEnum.DataExport;
					else if (isEasyLookLicensed)
						return TaskTypeEnum.DelRunnedReports;
					break;
				
				case  8:
					if (isSQLServerDB)
						return TaskTypeEnum.RestoreCompanyDB;
					else if (isXEngineExtensionLicensed)
						return TaskTypeEnum.DataImport;
					break;

				case  9:
					if (isXEngineExtensionLicensed && isSQLServerDB)
						return TaskTypeEnum.DataExport;
					else if (isEasyLookLicensed && isSQLServerDB)
						return TaskTypeEnum.DelRunnedReports;
					break;
		
				case  10:
					if (isXEngineExtensionLicensed && isSQLServerDB)
						return TaskTypeEnum.DataImport;
					else if (isXEngineExtensionLicensed && isEasyLookLicensed)
						return TaskTypeEnum.DelRunnedReports;
					break;

				case  11:
					if (isXEngineExtensionLicensed && isEasyLookLicensed && isSQLServerDB)
						return TaskTypeEnum.DelRunnedReports;
					break;

				default:
					break;
			}
			return TaskTypeEnum.Undefined;
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		private TaskTypeEnum GetSelectedTaskType()
		{
			if (CommandTypeComboBox.SelectedIndex == -1)
				return TaskTypeEnum.Undefined;

			return TypeFromIndex(CommandTypeComboBox.SelectedIndex);
		}
	
		//--------------------------------------------------------------------------------------------------------------------------------
		private int GetTypeIndex(TaskTypeEnum type)
		{
			bool isXEngineExtensionLicensed = (currentTBSchedulerControl != null && currentTBSchedulerControl.IsXEngineExtensionLicensed);
			bool isEasyLookLicensed = (currentTBSchedulerControl != null && currentTBSchedulerControl.IsEasyLookLicensed);
			bool isSQLServerDB = (currentTBSchedulerControl != null && currentTBSchedulerControl.IsSQLServerDB);

			switch (type)
			{
				case  TaskTypeEnum.Batch:
					return 0;

				case  TaskTypeEnum.Report:
					return 1;

				case  TaskTypeEnum.Function:
					return 2;

				case  TaskTypeEnum.Executable:
					return 3;

				case  TaskTypeEnum.Message:
					return 4;
				
				case  TaskTypeEnum.Mail:
					return 5;
				
				case  TaskTypeEnum.WebPage:
					return 6;

				case  TaskTypeEnum.BackupCompanyDB:
					if (isSQLServerDB)
						return 7;
					break;

				case  TaskTypeEnum.RestoreCompanyDB:
					if (isSQLServerDB)
						return 8;
					break;

				case  TaskTypeEnum.DataExport:
					if (isXEngineExtensionLicensed)
					{
						if (isSQLServerDB)
							return 9;
						else
							return 7;
					}
					break;

				case  TaskTypeEnum.DataImport:
					if (isXEngineExtensionLicensed)
					{
						if (isSQLServerDB)
							return 10;
						else
							return 8;
					}
					break;

				case  TaskTypeEnum.DelRunnedReports:
					if (isEasyLookLicensed)
					{
						if (isXEngineExtensionLicensed)
						{
							if (isSQLServerDB)
								return 11;
							else
								return 9;
						}
						else
						{
							if (isSQLServerDB)
								return 9;
							else
								return 7;
						}
					}
					break;

				default:
					break;
			}
			return -1;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void SetTaskType(WTEScheduledTaskObj aTask)
		{
			if (aTask == null || aTask.IsSequence)
				return;
		
			bool isXEngineExtensionLicensed = (currentTBSchedulerControl != null && currentTBSchedulerControl.IsXEngineExtensionLicensed);
			bool isEasyLookLicensed = (currentTBSchedulerControl != null && currentTBSchedulerControl.IsEasyLookLicensed);
			bool isSQLServerDB = (currentTBSchedulerControl != null && currentTBSchedulerControl.IsSQLServerDB);

			switch (CommandTypeComboBox.SelectedIndex)
			{
				case  0:
					aTask.RunBatch = true;
					break;

				case  1:
					aTask.RunReport = true;
					break;

				case  2:
					aTask.RunFunction = true;
					break;

				case  3:
					aTask.RunExecutable = true;
					break;

				case  4:
					aTask.SendMessage = true;
					break;

				case  5:
					aTask.SendMail = true;
					break;

				case  6:
					aTask.OpenWebPage = true;
					break;
				
				case  7:
					if (isSQLServerDB)
						aTask.BackupCompanyDB = true;
					else if (isXEngineExtensionLicensed)
						aTask.RunDataExport = true;
					else if (isEasyLookLicensed)
						aTask.DeleteRunnedReports = true;
					break;
				
				case  8:
					if (isSQLServerDB)
						aTask.RestoreCompanyDB = true;
					else if (isXEngineExtensionLicensed)
						aTask.RunDataImport = true;
					break;

				case  9:			
					if (isSQLServerDB && isXEngineExtensionLicensed)
						aTask.RunDataExport = true;
					else if (isEasyLookLicensed && (isSQLServerDB || isXEngineExtensionLicensed))
						aTask.DeleteRunnedReports = true;
					break;
				
				case  10:
					if (isXEngineExtensionLicensed && isSQLServerDB)
						aTask.RunDataImport = true;
					break;

				case  11:
					if (isXEngineExtensionLicensed && isEasyLookLicensed && isSQLServerDB)
						aTask.DeleteRunnedReports = true;
					break;

				default:
					aTask.UndefinedType = true;
					break;
			}
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		private bool ValidateCommandText()
		{
			TaskTypeEnum selectedType = GetSelectedTaskType();

			if (selectedType == TaskTypeEnum.DelRunnedReports || CommandTextBox.Text == null || CommandTextBox.Text == String.Empty)
				return true;

			// Se è già stato valorizzato il comando da eseguire, occorre controllare che tale comando
			// corrisponda effettivamente al tipo selezionato.
			// Devono venire effettuati i controlli su tutti i possibili tipi di comandi schedulabili.

			if (selectedType == TaskTypeEnum.Batch)
			{
				IDocumentInfo docInfo = FindDocumentInfoByCommandText();

				if (docInfo != null && docInfo.IsBatch && docInfo.IsSchedulable)
				{
					CommandParametersButton.Enabled = true;
					return true;
				}

				CommandParametersButton.Enabled = false;
			}
			else if (selectedType == TaskTypeEnum.Report)
			{
				if (FindReportInfoByCommandText())
				{
					CommandParametersButton.Enabled = true;
					return true;
				}
				CommandParametersButton.Enabled = false;
			}
            else if (selectedType == TaskTypeEnum.Function)
            {
                if (FindFunctionInfoByCommandText())
                {
                    CommandParametersButton.Enabled = true;
                    return true;
                }
                CommandParametersButton.Enabled = false;
            }
            else if (selectedType == TaskTypeEnum.DataExport || selectedType == TaskTypeEnum.DataImport)
			{
				IDocumentInfo docInfo = FindDocumentInfoByCommandText();

				if (docInfo != null && docInfo.IsDataEntry)
				{
					CommandParametersButton.Enabled = true; //(selectedType != TaskTypeEnum.DataImport);
					return true;
				}
				CommandParametersButton.Enabled = false;
			}
			else if (selectedType == TaskTypeEnum.WebPage)
			{
				if (WTEScheduledTaskObj.IsValidURI(CommandTextBox.Text))
					return true;

				MessageBox.Show(this, String.Format(TaskSchedulerWindowsControlsStrings.InvalidURIErrMsgFmt, CommandTextBox.Text));
			}
			else if (selectedType == TaskTypeEnum.BackupCompanyDB || selectedType == TaskTypeEnum.RestoreCompanyDB)
			{
				if (WTEScheduledTaskObj.IsValidBackupFilePath(CommandTextBox.Text))
					return true;

				MessageBox.Show(this, String.Format(TaskSchedulerWindowsControlsStrings.InvalidBackupFilenameErrMsgFmt, CommandTextBox.Text));
			}
			else
			{ //@@TODO
				return true;
			}
			TaskTabControl.SelectedTab = CommandPropertiesPage;

			CommandTextBox.Text = String.Empty;
			CommandTextBox.Focus();

			return false;
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		private IDocumentInfo FindDocumentInfoByCommandText()
		{
			if (CommandTextBox.Text == null || CommandTextBox.Text == String.Empty)
				return null;

			if (CurrentMenuLoader == null || CurrentMenuLoader.PathFinder == null)
			{
				Debug.Fail("TaskPropertiesForm.FindDocumentInfoByCommandText Error: invalid menu loader.");
				return null;
			}

			string applicationName = String.Empty;
			string moduleName = String.Empty;
			string commandName = String.Empty;

			NameSpace commandNamespace = new NameSpace(CommandTextBox.Text, NameSpaceObjectType.Document);
			if (!commandNamespace.IsValid())
				return null;
            /*
			ApplicationInfo appInfo = (ApplicationInfo)CurrentMenuLoader.PathFinder.GetApplicationInfoByName(commandNamespace.Application);
			if (appInfo == null)
				return null;
			
			ModuleInfo moduleInfo = (ModuleInfo)appInfo.GetModuleInfoByName(commandNamespace.Module);
			if (moduleInfo == null)
				return null;
			
			ILibraryInfo libInfo = moduleInfo.GetLibraryInfoByPath(commandNamespace.Library);
			if (libInfo == null)
				return null;
			
			return libInfo.GetDocumentInfoByNameSpace(commandNamespace.FullNameSpace);
            */
            return CurrentMenuLoader.PathFinder.GetDocumentInfo(commandNamespace);
		}
		
		
		//--------------------------------------------------------------------------------------------------------------------------------
		private bool FindReportInfoByCommandText()
		{
			if (CommandTextBox.Text == null || CommandTextBox.Text == String.Empty)
				return false;

			if (CurrentMenuLoader == null || CurrentMenuLoader.PathFinder == null)
			{
				Debug.Fail("TaskPropertiesForm.FindReportInfoByCommandText Error: invalid menu loader.");
				return false;
			}

			NameSpace reportNamespace = new NameSpace(CommandTextBox.Text, NameSpaceObjectType.Report);
			if (!reportNamespace.IsValid())
				return false;

			//TODO il terzo parametro è la lingua dell'utente corrente serve per poter trovare alcuni file nella 
			//standard divisi per lingua. Con string.empty cerca nella cartella di default 19/6/2006
			string reportFullFileName = CurrentMenuLoader.PathFinder.GetFilename(reportNamespace, string.Empty);

			return File.Exists(reportFullFileName);
		}

        //--------------------------------------------------------------------------------------------------------------------------------
        private bool FindFunctionInfoByCommandText()
        {
            if (CommandTextBox.Text == null || CommandTextBox.Text == String.Empty)
                return false;

            if (CurrentMenuLoader == null || CurrentMenuLoader.PathFinder == null)
            {
                Debug.Fail("TaskPropertiesForm.FindFunctionInfoByCommandText Error: invalid menu loader.");
                return false;
            }

            Microarea.TaskBuilderNet.Core.CoreTypes.FunctionsList funs = BasePathFinder.BasePathFinderInstance.WebMethods;
            
            this.funInfo = (funs != null ? funs.GetPrototype(CommandTextBox.Text) : null);

            return this.funInfo != null;
        }
		
		//--------------------------------------------------------------------------------------------------------------------------------
		private void UpdateCommandPropertiesPage()
		{
			TaskTypeEnum selectedType = GetSelectedTaskType();

			if (selectedType == TaskTypeEnum.WebPage)
				CommandLabel.Text = TaskSchedulerWindowsControlsStrings.TaskURLLabelText;
			else if (selectedType == TaskTypeEnum.Message || selectedType == TaskTypeEnum.Mail)
				CommandLabel.Text = TaskSchedulerWindowsControlsStrings.TaskRecipientsLabelText;
			else if (selectedType == TaskTypeEnum.BackupCompanyDB || selectedType == TaskTypeEnum.RestoreCompanyDB)
				CommandLabel.Text = TaskSchedulerWindowsControlsStrings.BackupCompanyDBFilenameLabelText;
			else
				CommandLabel.Text = TaskSchedulerWindowsControlsStrings.TaskCommandLabelText;

			CommandLabel.Visible = (selectedType != TaskTypeEnum.DelRunnedReports);

			CommandTextBox.Visible = (selectedType != TaskTypeEnum.Mail && selectedType != TaskTypeEnum.DelRunnedReports);

			bool applicationCommand = (	selectedType == TaskTypeEnum.Batch		|| 
										selectedType == TaskTypeEnum.Report		|| 
										selectedType == TaskTypeEnum.Function	|| 
										selectedType == TaskTypeEnum.DataExport || 
										selectedType == TaskTypeEnum.DataImport);

			SelectMenuCommandBtn.Enabled = SelectMenuCommandBtn.Visible = 
				(CurrentMenuLoader != null && (applicationCommand || selectedType == TaskTypeEnum.Executable));

			SelectCommandButton.Enabled = SelectCommandButton.Visible = (	applicationCommand							 || 
																			selectedType == TaskTypeEnum.Executable		 || 
																			selectedType == TaskTypeEnum.Message		 || 
																			selectedType == TaskTypeEnum.BackupCompanyDB || 
																			selectedType == TaskTypeEnum.RestoreCompanyDB);
		
			MailTaskRecipientsControl.Enabled = MailTaskRecipientsControl.Visible = (selectedType == TaskTypeEnum.Mail);

			CommandParametersButton.Visible = (CurrentMenuLoader != null && applicationCommand); // && selectedType != TaskTypeEnum.DataImport);
			CommandParametersButton.Enabled = CommandParametersButton.Visible && CommandTextBox.Text != null && CommandTextBox.Text != String.Empty;
		
			SetApplicationDateCheckBox.Visible = (	selectedType == TaskTypeEnum.Batch		|| 
													selectedType == TaskTypeEnum.Report		|| 
													selectedType == TaskTypeEnum.Function	|| 
													selectedType == TaskTypeEnum.DataExport || 
													selectedType == TaskTypeEnum.DataImport);

			RunIconizedCheckBox.Visible = (selectedType == TaskTypeEnum.Batch || selectedType == TaskTypeEnum.Report);
			CloseOnEndCheckBox.Visible = (selectedType == TaskTypeEnum.Batch || selectedType == TaskTypeEnum.Report);
			PrintReportCheckBox.Visible = (selectedType == TaskTypeEnum.Report);
			
			WaitForProcessHasExitedCheckBox.Visible = (selectedType == TaskTypeEnum.Executable);
			
			ValidateDataCheckBox.Visible = (selectedType == TaskTypeEnum.DataImport);
			
			UseDefaultBrowserRadioButton.Visible = (selectedType == TaskTypeEnum.WebPage);
			UseInternetExplorerRadioButton.Visible = (selectedType == TaskTypeEnum.WebPage);
			CreateNewBrowserInstanceCheckBox.Visible = (selectedType == TaskTypeEnum.WebPage);

			MessageTextLabel.Visible = (selectedType == TaskTypeEnum.Message || selectedType == TaskTypeEnum.Mail);
			MessageTextBox.Visible = (selectedType == TaskTypeEnum.Message || selectedType == TaskTypeEnum.Mail);

			OverwriteCompanyDBBackupCheckBox.Visible = (selectedType == TaskTypeEnum.BackupCompanyDB);
			VerifyCompanyDBBackupCheckBox.Visible = (selectedType == TaskTypeEnum.BackupCompanyDB);
			
			UpdateReportOptionsState();
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		private void UpdateReportOptionsState()
		{
			TaskTypeEnum selectedType = GetSelectedTaskType();

			bool reportOptionsAvailable =	(selectedType == TaskTypeEnum.Report && 
											currentTBSchedulerControl != null && 
											currentTBSchedulerControl.IsMailConnectorLicensed);
			ReportOptionsGroupBox.Visible = reportOptionsAvailable;
			
			if (!reportOptionsAvailable)
				return;

			bool sendReportAsMail = SendMailWithReportAsAttachmentRadioButton.Checked;

			ReportMailRecipientLabel.Enabled = sendReportAsMail;
			ReportMailRecipientControl.Enabled = sendReportAsMail;
			CompressAttachmentsCheckBox.Enabled = sendReportAsMail;

			bool saveReportAsFile = SaveReportAsFileRadioButton.Checked;

			ReportFileNameLabel.Enabled = saveReportAsFile;
			ReportFileNameTextBox.Enabled = saveReportAsFile;
			BrowseReportFileNameButton.Enabled = saveReportAsFile;

			PDFFormatCheckBox.Enabled = (sendReportAsMail || saveReportAsFile);
			RDEFormatCheckBox.Enabled = (sendReportAsMail || saveReportAsFile);
			ExcelFormatCheckBox.Enabled = (sendReportAsMail || saveReportAsFile);

			if (sendReportAsMail || saveReportAsFile)
			{
				// nel caso in cui nessun radiobutton e' selezionato allora imposto il primo
				if (!PDFFormatCheckBox.Checked && !RDEFormatCheckBox.Checked && !ExcelFormatCheckBox.Checked)
					PDFFormatCheckBox.Checked = true;
			}
			
			CompressAttachmentsCheckBox.Enabled = sendReportAsMail;
			ConcatPDFFilesCheckBox.Enabled = (sendReportAsMail || saveReportAsFile) && PDFFormatCheckBox.Checked;
		}

		#endregion
		
		#region TaskPropertiesForm public properties

		//-------------------------------------------------------------------------------------------
		private string CurrentConnectionString
		{
			get
			{
				return (currentTBSchedulerControl != null) ? currentTBSchedulerControl.ConnectionString : String.Empty;
			}
		}

		//-------------------------------------------------------------------------------------------
		private MenuLoader CurrentMenuLoader
		{
			get
			{
				return (currentTBSchedulerControl != null) ? currentTBSchedulerControl.MenuLoader : null;
			}
		}
		
		#endregion
		
		#region TaskPropertiesForm public methods
		
		//--------------------------------------------------------------------------------------------------------------------------------
		public void RemoveSchedulingPage()
		{
			if (!TaskTabControl.TabPages.Contains(SchedulingPropertiesPage))
				return;

			TaskTabControl.TabPages.Remove(SchedulingPropertiesPage);
			//SchedulingPropertiesPage.Enabled = false;
			TaskTabControl.SelectedTab = GeneralPropertiesPage;
		}
		
		#endregion

		#region TaskPropertiesForm event handlers
	
		//--------------------------------------------------------------------------------------------------------------------------------
		protected override void WndProc (ref Message m)
		{
			if (m.Msg == ExternalAPI.UM_SET_STATUS_BAR_TEXT)
			{
				taskToolStripStatusLabel.Text = Marshal.PtrToStringAuto(m.WParam);
				return;
			}

			if (m.Msg == ExternalAPI.UM_CLEAR_STATUS_BAR)
			{
				taskToolStripStatusLabel.Text = string.Empty;
				return;
			}

			base.WndProc(ref m);
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		protected override void OnLoad(System.EventArgs e)
		{
			// Invoke base class implementation
			base.OnLoad(e);

			if (CurrentConnectionString == null || CurrentConnectionString == String.Empty)
				return;

			if (ScheduledSequencesEngine.IsInSequenceInvolved(CurrentConnectionString, task.Id))
			{
				task.ToRunOnDemand = true;

				if(TaskTabControl.TabPages.Contains(SchedulingPropertiesPage))
				{
					RemoveSchedulingPage();
					MessageBox.Show(this, TaskSchedulerWindowsControlsStrings.CannotChangeTasksSchedulationWarningMsg, TaskSchedulerWindowsControlsStrings.CannotChangeTasksSchedulationCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void CommandTypeComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			UpdateCommandPropertiesPage();

			ValidateCommandText();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
        FunctionPrototype funInfo = null;

        private void SelectCommandButton_Click(object sender, System.EventArgs e)
		{
            funInfo = null;
			TaskTypeEnum selectedType = GetSelectedTaskType();
			if (selectedType == TaskTypeEnum.Batch || selectedType == TaskTypeEnum.Report || selectedType == TaskTypeEnum.Function || selectedType == TaskTypeEnum.DataExport || selectedType == TaskTypeEnum.DataImport)
			{
				if (CurrentMenuLoader == null)
				{
					Debug.Fail("TaskPropertiesForm.SelectCommandButton_Click Error: invalid menu loader.");
					return;
				}
				
				this.Cursor = Cursors.WaitCursor;
				SelectApplicationCommandDlg selAppCmdDlg = new SelectApplicationCommandDlg
																	(
																	CurrentMenuLoader.PathFinder, 
																	CurrentMenuLoader.LoginManager,
																	(currentTBSchedulerControl != null) ? currentTBSchedulerControl.BrandLoader : null,
																	selectedType, 
																	CommandTextBox.Text
																	);

				DialogResult selAppCmdDlgResult = selAppCmdDlg.ShowDialog(this);
				this.Cursor = Cursors.Default;
				if (selAppCmdDlgResult != DialogResult.OK)
					return;

				CommandTextBox.Text = selAppCmdDlg.SelectedCommand;
                if (selectedType == TaskTypeEnum.Function)
                    funInfo = selAppCmdDlg.SelectedFunction;
			}
			else if (selectedType == TaskTypeEnum.Executable)
			{
				OpenFileDialog openExeFileDialog = new OpenFileDialog();

				openExeFileDialog.InitialDirectory = "C:\\" ;
				openExeFileDialog.Filter = TaskSchedulerWindowsControlsStrings.OpenExeFileDialogFilter;
				openExeFileDialog.FilterIndex = 1;
				openExeFileDialog.Multiselect = false;
				openExeFileDialog.RestoreDirectory = true;

				if(openExeFileDialog.ShowDialog(this) != DialogResult.OK)
					return;
					
				CommandTextBox.Text = openExeFileDialog.FileName;
			}
			else if (selectedType == TaskTypeEnum.Message)
			{
				SelectWorkstationDlg selWorkstationsDlg = new SelectWorkstationDlg(CommandTextBox.Text);

				if (selWorkstationsDlg.ShowDialog(this) != DialogResult.OK)
					return;

				CommandTextBox.Text = selWorkstationsDlg.SelectedWorkstations;
			}
			else if (selectedType == TaskTypeEnum.BackupCompanyDB || selectedType == TaskTypeEnum.RestoreCompanyDB)
			{
				OpenFileDialog openBakFileDialog = new OpenFileDialog();

				if (CommandTextBox.Text != String.Empty)
				{
					if (!WTEScheduledTaskObj.IsValidBackupFilePath(CommandTextBox.Text))
						return;

					openBakFileDialog.InitialDirectory = Path.GetDirectoryName(CommandTextBox.Text);
					openBakFileDialog.FileName = Path.GetFileName(CommandTextBox.Text);
				}

/*				if (ScheduledTask.IsValidBackupFilePath(CommandTextBox.Text))
				{
					openBakFileDialog.InitialDirectory = Path.GetFullPath(CommandTextBox.Text);
					openBakFileDialog.FileName = CommandTextBox.Text;
				}
				else
					openBakFileDialog.InitialDirectory = "C:\\";
*/
				openBakFileDialog.FilterIndex = 1;
				openBakFileDialog.Multiselect = false;
				openBakFileDialog.RestoreDirectory = true;
				openBakFileDialog.DefaultExt = "*.bak";
				openBakFileDialog.CheckPathExists = true;
				openBakFileDialog.CheckFileExists = (selectedType == TaskTypeEnum.RestoreCompanyDB);
				openBakFileDialog.Filter = "BAK files (*.bak)|*.bak|All files (*.*)|*.*";

				if (openBakFileDialog.ShowDialog(this) != DialogResult.OK)
					return;
					
				CommandTextBox.Text = openBakFileDialog.FileName;
			}
	
			ValidateCommandText();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		protected override void OnFormClosing (FormClosingEventArgs e)
		{
			if (DialogResult != DialogResult.OK)
			{
				base.OnFormClosing(e);
				return;
			}
			
			if (SendMailWithReportAsAttachmentRadioButton.Checked)
			{
				if (!PDFFormatCheckBox.Checked && !ExcelFormatCheckBox.Checked && !RDEFormatCheckBox.Checked)
				{
					MessageBox.Show(this, TaskSchedulerWindowsControlsStrings.InsertAttachmentFormat);
					PDFFormatCheckBox.Focus();
					e.Cancel = true;
					base.OnFormClosing(e);
					return;
				}
				
				if (string.IsNullOrEmpty(ReportMailRecipientControl.Text))
				{
					MessageBox.Show(this, TaskSchedulerWindowsControlsStrings.InsertEMailAddress);
					ReportMailRecipientControl.Focus();
					e.Cancel = true;
					base.OnFormClosing(e);
					return;
				}

			}

			if (GeneralPropertiesPageContent.CodeText == null || GeneralPropertiesPageContent.CodeText == String.Empty)
			{
				MessageBox.Show(this, TaskSchedulerWindowsControlsStrings.EmptyCodeErrMsg);
				TaskTabControl.SelectedTab = GeneralPropertiesPage;
				GeneralPropertiesPageContent.SetFocusToCodeTextBox();
				e.Cancel = true;
				base.OnFormClosing(e);
				return;
			}

			TaskTypeEnum selectedType = GetSelectedTaskType();

			if
				(
					selectedType != TaskTypeEnum.Mail &&
					selectedType != TaskTypeEnum.DelRunnedReports &&
					(CommandTextBox.Text == null || CommandTextBox.Text == String.Empty))
			{
				MessageBox.Show(this, TaskSchedulerWindowsControlsStrings.EmptyCommandErrMsg);
				TaskTabControl.SelectedTab = CommandPropertiesPage;
				CommandTextBox.Focus();
				e.Cancel = true;
				base.OnFormClosing(e);
				return;
			}

			SetTaskType(task);
			task.Command = CommandTextBox.Text;
			task.XmlParameters = commandParameters;

			if (task.RunBatch || task.RunReport || task.RunFunction || task.RunDataImport || task.RunDataExport)
			{
				task.SetApplicationDateBeforeRun = SetApplicationDateCheckBox.Checked;

				if (task.RunBatch || task.RunReport)
				{
					task.RunIconized = RunIconizedCheckBox.Checked;
					task.CloseOnEnd = CloseOnEndCheckBox.Checked;

					if (task.RunReport)
					{
						task.PrintReport = PrintReportCheckBox.Checked;

						task.SendReportAsPDFMailAttachment = SendMailWithReportAsAttachmentRadioButton.Checked && PDFFormatCheckBox.Checked;
						task.SendReportAsRDEMailAttachment = SendMailWithReportAsAttachmentRadioButton.Checked && RDEFormatCheckBox.Checked;
						task.SendReportAsExcelMailAttachment = SendMailWithReportAsAttachmentRadioButton.Checked && ExcelFormatCheckBox.Checked;
						task.SendReportAsCompressedMailAttachment = SendMailWithReportAsAttachmentRadioButton.Checked && CompressAttachmentsCheckBox.Checked;
						task.ReportSendingRecipients = ReportMailRecipientControl.Text;

						task.SaveReportAsPDFFile = SaveReportAsFileRadioButton.Checked && PDFFormatCheckBox.Checked;
						task.SaveReportAsRDEFile = SaveReportAsFileRadioButton.Checked && RDEFormatCheckBox.Checked;
						task.SaveReportAsExcelFile = SaveReportAsFileRadioButton.Checked && ExcelFormatCheckBox.Checked;
						task.ReportSavingFileName = ReportFileNameTextBox.Text;

						task.ConcatReportPDFFiles = PDFFormatCheckBox.Checked && ConcatPDFFilesCheckBox.Checked;
					}
					if (task.RunDataImport)
					{
						task.ValidateData = ValidateDataCheckBox.Checked;
					}
				}
			}
			else if (task.RunExecutable)
			{
				task.WaitForProcessHasExited = WaitForProcessHasExitedCheckBox.Checked;
			}
			else if (task.OpenWebPage)
			{
				task.OpenWebPageInInternetExplorer = UseInternetExplorerRadioButton.Checked;
				task.CreateNewBrowserInstance = CreateNewBrowserInstanceCheckBox.Checked;
			}
			else if (task.SendMessage)
			{
				task.MessageContent = MessageTextBox.Text;
			}
			else if (task.SendMail)
			{
				task.MessageContent = MessageTextBox.Text;
				task.Command = MailTaskRecipientsControl.Text;
			}
			else if (task.DeleteRunnedReports)
			{
				task.Command = String.Empty;
			}
			else if (task.BackupCompanyDB)
			{
				task.OverwriteCompanyDBBackup = OverwriteCompanyDBBackupCheckBox.Checked;
				task.VerifyCompanyDBBackup = VerifyCompanyDBBackupCheckBox.Checked;
			}

			base.OnFormClosing(e);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void SelectMenuCommandBtn_Click(object sender, System.EventArgs e)
		{
			TaskTypeEnum selectedType = GetSelectedTaskType();

			if (selectedType == TaskTypeEnum.Batch || selectedType == TaskTypeEnum.Report || selectedType == TaskTypeEnum.Function || selectedType == TaskTypeEnum.DataExport || selectedType == TaskTypeEnum.DataImport || selectedType == TaskTypeEnum.Executable)
			{
				if (CurrentMenuLoader == null)
				{
					Debug.Fail("TaskPropertiesForm.SelectMenuCommandBtn_Click Error: invalid menu loader.");
					return;
				}

				SelectMenuCommandDlg selAppCmdDlg = new SelectMenuCommandDlg
								(
									CurrentMenuLoader, 
									selectedType, 
									CommandTextBox.Text
								);

				if (selAppCmdDlg.ShowDialog(this) == DialogResult.OK)
				{
					CommandTextBox.Text = selAppCmdDlg.SelectedCommandItemObject;

					commandParameters = selAppCmdDlg.SelectedCommandArguments;

					CommandTypeComboBox.SelectedIndex = GetTypeIndex(selAppCmdDlg.CommandType);
					
					if (!ValidateCommandText())
					{
						MessageBox.Show(this, String.Format(TaskSchedulerWindowsControlsStrings.InvalidCommandErrMsgFmt, selAppCmdDlg.SelectedCommandItemObject));
					}
				}
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void CommandTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
		{
			ValidateCommandText();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
        private void RequestFunctionArguments()
        {
            if (funInfo == null || funInfo.Parameters.Count == 0)
                return;

            SetFunctionParameters dlg = new SetFunctionParameters(funInfo, commandParameters);
            DialogResult selAppCmdDlgResult = dlg.ShowDialog(this);
            if (selAppCmdDlgResult == System.Windows.Forms.DialogResult.OK)
            {
                commandParameters = dlg.CommandParameters;
            }
        }

		//--------------------------------------------------------------------------------------------------------------------------------
		private void CommandParametersButton_Click(object sender, System.EventArgs e)
		{

            if (task.Code == String.Empty)
            {
                MessageBox.Show(this, "Prima inserire codice task");
                return;
            }


            if (currentTBSchedulerControl == null || currentTBSchedulerControl.UserInteractionDisabled)
				return;

			if (CurrentConnectionString == null || CurrentConnectionString == String.Empty)
			{
				Debug.Fail("TaskPropertiesForm.CommandParametersButton_Click Error: invalid connection string.");
				return;
			}

			if (CurrentMenuLoader == null || CurrentMenuLoader.PathFinder == null)
			{
				Debug.Fail("TaskPropertiesForm.CommandParametersButton_Click Error: invalid menu loader.");
				return;
			}
			
			if (CommandTextBox.Text == null || CommandTextBox.Text == String.Empty)
			{
				MessageBox.Show(this, TaskSchedulerWindowsControlsStrings.NoCommandSelectionWarningMsg);
				return;
			}

            if (GetSelectedTaskType() == TaskTypeEnum.Function)
            {
                if (funInfo == null)
                {
                    MessageBox.Show(this, TaskSchedulerWindowsControlsStrings.NoCommandSelectionWarningMsg);
                    return;
                }

                RequestFunctionArguments();
                return; 
            }

            WTEScheduledTaskObj tmpTask = new WTEScheduledTaskObj(task);
			
			SetTaskType(tmpTask);
			tmpTask.Command = CommandTextBox.Text;
			tmpTask.XmlParameters = commandParameters;
			
			//Imposta l'handle della finestra corrente come "parent" di tutte le operazioni che vengono
			//fatte su eventuali TbLoader istanziati (in questo modo eventuali finestre modali di TB saranno
			//comunque figlie della finestra attuale e non rimarranno nascoste dalla console
			tmpTask.SetWindowHandle(this.Handle);

			IMessageFilter aFilter = null;
			try
			{
				// ATTENZIONE !!!
				// Qui occorre necessariamente catturare possibili eccezioni, predisporre
				// cioè un blocco try-catch- finally, altrimenti continuerebbe a restare
				// disabilitata qualunque interazione da parte del'utente...

				aFilter = currentTBSchedulerControl.DisableUserInteraction();

				bool ok = tmpTask.GetCommandParameters(CurrentMenuLoader.PathFinder, CurrentConnectionString);
				
				currentTBSchedulerControl.RestoreUserInteraction(aFilter);

				aFilter = null;

				if (ok)
				{
					commandParameters = tmpTask.XmlParameters;
					return;
				}

				MessageBox.Show(this, TaskSchedulerWindowsControlsStrings.GetCommandParametersFailedErrMsg);
			}
			catch(ScheduledTaskException exception)
			{
				// ATTENZIONE !!!
				// Qui occorre necessariamente catturare possibili eccezioni, predisporre
				// cioè un blocco try-catch- finally, altrimenti continuerebbe a restare
				// disabilitata qualunque interazione da parte del'utente...
				if (aFilter != null)
					currentTBSchedulerControl.RestoreUserInteraction(aFilter);

				MessageBox.Show(this, exception.ExtendedMessage, TaskSchedulerWindowsControlsStrings.GetCommandParametersFailedErrMsg, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			catch (Exception exception)
			{
				// ATTENZIONE !!!
				// Qui occorre necessariamente catturare possibili eccezioni, predisporre
				// cioè un blocco try-catch- finally, altrimenti continuerebbe a restare
				// disabilitata qualunque interazione da parte del'utente...
				if (aFilter != null)
					currentTBSchedulerControl.RestoreUserInteraction(aFilter);

				MessageBox.Show(this, exception.Message, TaskSchedulerWindowsControlsStrings.GetCommandParametersFailedErrMsg, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		private void SchedulingPageContent_SchedulingModeChanged(object sender, System.EventArgs e)
		{
			//if (task == null || task.ToRunOnDemand)
			//{
			//	if (this.TaskTabControl.Controls.Contains(this.AuthenticationPage))
			//	{
			//		this.TaskTabControl.Controls.Remove(this.AuthenticationPage);
			//		this.TaskTabControl.SelectedTab = SchedulingPropertiesPage;
			//	}
			//}
			//else if (!this.TaskTabControl.Controls.Contains(this.AuthenticationPage))
			//	this.TaskTabControl.Controls.Add(this.AuthenticationPage);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void GeneralPropertiesPageContent_OnValidatedCode(object sender, bool valid)
		{
			validTaskCode = valid;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void TaskTabControl_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if 
				(
				!validTaskCode &&
				GeneralPropertiesPageContent.CodeText != null && 
				GeneralPropertiesPageContent.CodeText != String.Empty
				)
			{
				if (TaskTabControl.SelectedTab == GeneralPropertiesPage)
					MessageBox.Show(this, String.Format(TaskSchedulerWindowsControlsStrings.CodeAlreadyUsedErrMsgFmt, GeneralPropertiesPageContent.CodeText, WTEScheduledTask.TaskCodeUniquePrefixLength));
				else
					TaskTabControl.SelectedTab = GeneralPropertiesPage;

				GeneralPropertiesPageContent.CodeText = String.Empty;
				GeneralPropertiesPageContent.SetFocusToCodeTextBox();
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void NormalReportExecutionRadioButton_CheckedChanged (object sender, EventArgs e)
		{
			UpdateCommandPropertiesPage();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void SendMailWithReportAsAttachmentRadioButton_CheckedChanged(object sender, System.EventArgs e)
		{
			UpdateReportOptionsState();
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		private void SaveReportAsFileRadioButton_CheckedChanged(object sender, System.EventArgs e)
		{
			UpdateReportOptionsState();
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		private void PDFFormatCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			UpdateReportOptionsState();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void BrowseReportFileNameButton_Click(object sender, System.EventArgs e)
		{
			TaskTypeEnum selectedType = GetSelectedTaskType();
			if (selectedType != TaskTypeEnum.Report || !SaveReportAsFileRadioButton.Checked)
				return;
			
			OpenFileDialog openExeFileDialog = new OpenFileDialog();
            openExeFileDialog.Filter = "Excel Files(.xls)|*.xls";
            openExeFileDialog.FilterIndex = 1;
			openExeFileDialog.InitialDirectory = "C:\\" ;
		//	openExeFileDialog.Filter = TaskSchedulerWindowsControlsStrings.OpenAllFilesDialogFilter;
			openExeFileDialog.FilterIndex = 1;
			openExeFileDialog.Multiselect = false;
			openExeFileDialog.RestoreDirectory = true;
			openExeFileDialog.CheckPathExists = true;
			openExeFileDialog.CheckFileExists = false;

			if(openExeFileDialog.ShowDialog(this) != DialogResult.OK)
				return;
				
			ReportFileNameTextBox.Text = openExeFileDialog.FileName;
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		private void BrowserChoiceRadioButton_CheckedChanged(object sender, System.EventArgs e)
		{
			CreateNewBrowserInstanceCheckBox.Enabled = UseDefaultBrowserRadioButton.Checked;
		}

	
		#endregion
	}
}
