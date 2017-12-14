using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Windows.Forms;

using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;
using Microarea.TaskBuilderNet.Core.CoreTypes;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Core.TaskschedulerEngine;

namespace Microarea.Console.Core.TaskSchedulerWindowsControls
{
	/// <summary>
	/// Summary description for SelectApplicationCommandDlg.
	/// </summary>
	public partial class SelectApplicationCommandDlg : System.Windows.Forms.Form
	{
		private IPathFinder		pathFinder = null;
		private LoginManager	loginManager = null;
		private TaskTypeEnum	commandType = TaskTypeEnum.Batch;

		private IApplicationInfo	currAppInfo = null;
		private IModuleInfo			currModuleInfo = null;
		private ILibraryInfo		currLibraryInfo = null;
		private ArrayList			currReportFiles = null;
		private string				selectedCommand = String.Empty;
        private object              selectedObject = null;
		private NameSpaceObjectType	currNameSpaceObjectType = NameSpaceObjectType.NotValid;

		public SelectApplicationCommandDlg(IPathFinder aPathFinder, LoginManager aLoginManager, IBrandLoader aBrandLoader, TaskTypeEnum aCommandType, string currentCommand)
		{
			InitializeComponent();

			Debug.Assert(aPathFinder != null, "SelectApplicationCommandDlg Constructor Warning: invalid PathFinder.");

			pathFinder = aPathFinder;
			loginManager = aLoginManager;
			if (loginManager == null && pathFinder != null && pathFinder.LoginManagerUrl != null && pathFinder.LoginManagerUrl != String.Empty)
				loginManager = new LoginManager(pathFinder.LoginManagerUrl, InstallationData.ServerConnectionInfo.WebServicesTimeOut);

			if (pathFinder != null)
			{
				this.AppComboBox.PathFinder = pathFinder;
			}

			commandType = aCommandType;

			switch (commandType)
			{
				case TaskTypeEnum.Batch:
					Commandlabel.Text = TaskSchedulerWindowsControlsStrings.SelectBatchCommandLabelText;
					currNameSpaceObjectType = NameSpaceObjectType.Document;
					break;
				case TaskTypeEnum.Function:
					Commandlabel.Text = TaskSchedulerWindowsControlsStrings.SelectFunctionCommandLabelText;
					currNameSpaceObjectType = NameSpaceObjectType.Function;
					break;
				case TaskTypeEnum.Report:
					Commandlabel.Text = TaskSchedulerWindowsControlsStrings.SelectReportCommandLabelText;
					currNameSpaceObjectType = NameSpaceObjectType.Report;
					break;
				case TaskTypeEnum.DataExport:
				case TaskTypeEnum.DataImport:
					Commandlabel.Text = TaskSchedulerWindowsControlsStrings.SelectXmlImportExportCommandLabelText;
					currNameSpaceObjectType = NameSpaceObjectType.Document;
					break;
				default:
					Debug.Fail("SelectApplicationCommandDlg Constructor Error: invalid command type");
					currNameSpaceObjectType = NameSpaceObjectType.NotValid;
					break;
			}

			selectedCommand = currentCommand;
		}

		#region SelectApplicationCommandDlg event handlers

		//--------------------------------------------------------------------------------------------------------------------------------
		protected override void OnLoad(System.EventArgs e)
		{
			// Invoke base class implementation
			base.OnLoad(e);

			currAppInfo = null;
			currModuleInfo = null;

			LibraryComboBox.Enabled = (commandType != TaskTypeEnum.Report);

            AppComboBox.Fill(currNameSpaceObjectType != NameSpaceObjectType.Function);

			if (AppComboBox.Items.Count > 0)
			{
				if (selectedCommand != null && selectedCommand != String.Empty)
				{
					NameSpace commandNamespace = new NameSpace(selectedCommand, currNameSpaceObjectType);

					int i = AppComboBox.FindApplicationIndex(commandNamespace.Application);
					AppComboBox.SelectedIndex = (i >= 0) ? i : 0;

					if (ModuleComboBox.Items.Count > 0)
					{
						i = GetModuleComboBoxIndexFromName(commandNamespace.Module);
						ModuleComboBox.SelectedIndex = (i >= 0) ? i : 0;

						if (commandType != TaskTypeEnum.Report)
						{
							if (LibraryComboBox.Items.Count > 0)
							{
								i = LibraryComboBox.FindStringExact(commandNamespace.Library);
								LibraryComboBox.SelectedIndex = (i >= 0) ? i : 0;

								if (CommandComboBox.Items.Count > 0)
								{
									i = CommandComboBox.FindStringExact(commandNamespace.Command);
									CommandComboBox.SelectedIndex = (i >= 0) ? i : 0;
								}
								else
									CommandComboBox.SelectedIndex = -1;
							}
							else
								LibraryComboBox.SelectedIndex = -1;
						}
						else
						{
							if (CommandComboBox.Items.Count > 0)
							{
								i = CommandComboBox.FindStringExact(commandNamespace.Report);
								CommandComboBox.SelectedIndex = (i >= 0) ? i : 0;
							}
							else
								CommandComboBox.SelectedIndex = -1;
						}
					}
					else
						ModuleComboBox.SelectedIndex = -1;
				}
				else
					AppComboBox.SelectedIndex = 0;
			}
			else
				AppComboBox.SelectedIndex = -1;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void AppComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			ModuleComboBox.Items.Clear();
			currModuleInfo = null;
			
			if (pathFinder != null)
			{
				int selIdx = AppComboBox.SelectedIndex;
				if (selIdx >= 0)
				{
					currAppInfo = AppComboBox.GetApplicationInfoAt(selIdx);
					if (currAppInfo != null && currAppInfo.Modules != null)
					{
						foreach(ModuleInfo moduleInfo in currAppInfo.Modules)
						{
							if (loginManager != null && loginManager.IsActivated(currAppInfo.Name, moduleInfo.Name))
								ModuleComboBox.Items.Add(moduleInfo);
						}
					}
				}
			}
			ModuleComboBox.Enabled = (currAppInfo != null && ModuleComboBox.Items.Count > 0);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void ModuleComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{		
			LibraryComboBox.Items.Clear();
			currLibraryInfo = null;
			
			CommandComboBox.Items.Clear();

			if (commandType == TaskTypeEnum.Report)
			{				
				if(currReportFiles == null)
					currReportFiles = new ArrayList();
				currReportFiles.Clear();
			}

			if (pathFinder != null && currAppInfo != null)
			{
				int selIdx = ModuleComboBox.SelectedIndex;
				if (selIdx >= 0)
				{
					currModuleInfo = (ModuleComboBox.Items[selIdx] != null && (ModuleComboBox.Items[selIdx] is ModuleInfo)) ? (ModuleInfo)ModuleComboBox.Items[selIdx] : null;
					if (commandType != TaskTypeEnum.Report)
					{
						if (currModuleInfo != null && currModuleInfo.Libraries != null)
						{
							foreach(LibraryInfo lib in currModuleInfo.Libraries)
								LibraryComboBox.Items.Add(lib.Name);
						}
						LibraryComboBox.Enabled = (currModuleInfo != null && LibraryComboBox.Items.Count > 0);
						LibraryComboBox.SelectedIndex = (LibraryComboBox.Items.Count > 0) ? 0 : -1;		
						return;
					}
					
					if (currModuleInfo != null && currAppInfo.Path != null && currAppInfo.Path != String.Empty)
					{
						// Vado nelle directory contenenti i report del moduli (standard e custom)
						// e vedo quali sono i file presenti in esse						
						string standardReportpath = currAppInfo.Path + Path.DirectorySeparatorChar + currModuleInfo.Name + Path.DirectorySeparatorChar + NameSolverStrings.Report;
						DirectoryInfo standardReportDirInfo = new DirectoryInfo(standardReportpath);
						if (standardReportDirInfo != null && standardReportDirInfo.Exists)
							currReportFiles.AddRange(standardReportDirInfo.GetFiles("*" + NameSolverStrings.WrmExtension));

						if (currAppInfo.CustomPath != null && currAppInfo.CustomPath != String.Empty)
						{
							try
							{
								string customReportpath = currModuleInfo.GetCustomReportPath();
								if (Directory.Exists(customReportpath))
								{
									DirectoryInfo customReportDirInfo = new DirectoryInfo(customReportpath);
									if (customReportDirInfo != null)
									{
										if (Directory.Exists(customReportpath + Path.DirectorySeparatorChar + NameSolverStrings.AllUsers))
											currReportFiles.AddRange(customReportDirInfo.GetFiles(NameSolverStrings.AllUsers + Path.DirectorySeparatorChar + "*" + NameSolverStrings.WrmExtension));
										if (
											pathFinder.User != null && 
											pathFinder.User != String.Empty && 
											pathFinder.User != NameSolverStrings.AllUsers &&
											Directory.Exists(customReportpath + Path.DirectorySeparatorChar + pathFinder.User)
											)
											currReportFiles.AddRange(customReportDirInfo.GetFiles(pathFinder.User + Path.DirectorySeparatorChar + "*" + NameSolverStrings.WrmExtension));
									}
								}
							}
							catch(SecurityException exception)
							{
								MessageBox.Show(String.Format(TaskSchedulerWindowsControlsStrings.SecurityExceptionErrMsg, exception.Message));								
							}
						}
					}
				}

				if (commandType == TaskTypeEnum.Report && currReportFiles != null)
				{
					foreach (FileInfo fileInfo in currReportFiles)
					{
						string reportName = fileInfo.Name;
						int extensionPos = reportName.LastIndexOf(NameSolverStrings.WrmExtension);
						if (extensionPos > 0)
							reportName = reportName.Substring(0, extensionPos);
						if (CommandComboBox.FindStringExact(reportName) == -1)
							CommandComboBox.Items.Add(reportName);
					}
				}
			}

			CommandComboBox.Enabled = (CommandComboBox.Items.Count > 0);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void LibraryComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			CommandComboBox.Items.Clear();
			
			if (commandType == TaskTypeEnum.Report)
				return;

			if (pathFinder != null && currModuleInfo != null)
			{
				int selIdx = LibraryComboBox.SelectedIndex;
				if (selIdx >= 0)
				{
					string selLibraryName = (string)LibraryComboBox.Items[selIdx];
					currLibraryInfo = currModuleInfo.GetLibraryInfoByName(selLibraryName);

					if (commandType == TaskTypeEnum.Batch || commandType == TaskTypeEnum.DataExport || commandType == TaskTypeEnum.DataImport)
					{
						if (currLibraryInfo != null && currLibraryInfo.Documents != null)
						{
							foreach(DocumentInfo docInfo in currLibraryInfo.Documents)
							{
								if (docInfo.IsBatch)
								{
									if (commandType != TaskTypeEnum.Batch || !docInfo.IsSchedulable)
										continue;
								}
								else
								{
									if (commandType == TaskTypeEnum.Batch)
										continue;

                                    if ((commandType == TaskTypeEnum.DataExport || commandType == TaskTypeEnum.DataImport) && docInfo.IsTransferDisabled)
                                        continue;
                                        
								}
								CommandComboBox.Items.Add(docInfo.NameSpace.Document);
							}
						}
					}
					else if (commandType == TaskTypeEnum.Function)
					{
                        if (currLibraryInfo != null &&
                            currLibraryInfo.ParentModuleInfo != null &&
                            BasePathFinder.BasePathFinderInstance != null   //NB: Proprietà con side effect (alloca l'oggetto on demand): serve nella riga sotto
                            /*&& currLibraryInfo.ParentModuleInfo.WebMethods != null*/)
						{
                            BaseModuleInfo bmi = (currLibraryInfo.ParentModuleInfo as ModuleInfo).GetStaticModuleinfo();
                            foreach (FunctionPrototype functionInfo in bmi.WebMethods)
							{
								CommandComboBox.Items.Add(functionInfo/*.NameSpace.Function*/);
							}
						}

					}
				}
			}

			CommandComboBox.Enabled = (CommandComboBox.Items.Count > 0);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		private void OkBtn_Click(object sender, System.EventArgs e)
		{
			if(currAppInfo == null || currModuleInfo == null || (commandType != TaskTypeEnum.Report && currLibraryInfo == null) || CommandComboBox.Text == null || CommandComboBox.Text == String.Empty)
			{
				this.DialogResult = DialogResult.None;
				return;
			}

            if (commandType == TaskTypeEnum.Report)
                selectedCommand = currAppInfo.Name + "." + currModuleInfo.Name + "." + CommandComboBox.Text;
            else
            {
                selectedCommand = currAppInfo.Name + "." + currModuleInfo.Name + "." + currLibraryInfo.Path + "." + CommandComboBox.Text;
                selectedObject = CommandComboBox.SelectedItem;
            }
		}

		#endregion

		//--------------------------------------------------------------------------------------------------------------------------------
		private int GetModuleComboBoxIndexFromName(string aModuleName)
		{
			if (aModuleName == null || aModuleName == String.Empty || ModuleComboBox.Items.Count == 0)
				return -1; 
			
			for (int i = 0; i < ModuleComboBox.Items.Count; i++)
			{
				if (ModuleComboBox.Items[i] == null || !(ModuleComboBox.Items[i] is ModuleInfo))
					continue;
				if (String.Compare(aModuleName, ((ModuleInfo)ModuleComboBox.Items[i]).Name) == 0)
					return i;
			}
			return -1; 
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public string SelectedCommand { get { return selectedCommand; } }
        public FunctionPrototype SelectedFunction { get { return selectedObject as FunctionPrototype; } }
	}
}
