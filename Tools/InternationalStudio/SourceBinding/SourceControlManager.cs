using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Microarea.Tools.TBLocalizer.CommonUtilities;

namespace Microarea.Tools.TBLocalizer.SourceBinding
{
	//--------------------------------------------------------------------------------
	public enum Action
	{
		None,
		SynchToSourceSafe,
		CheckIn, CheckOutAndIn, AddToSourceSafe, RemoveFromSourceSafe,
		SynchToLocal,
		GetLatest, UndoCheckout, RemoveLocal,
		CheckOut
	}

	//================================================================================
	public class SourceControlManager : IDisposable
	{
		private static PromptForCheckOut promptDialog = new PromptForCheckOut();
		private string currentSolutionFileName = string.Empty;
		private IWin32Window owner = null;
		private bool enabled = false;
		private string compareExecutablePath = "";
		private ILogger logWriter;

		public event EventHandler CompareExecutablePathChanged;
		public event EventHandler OnSourceTreeRefreshNeeded;
		
		//------------------------------------------------------------------------------------
		public string CompareExecutablePath 
		{ 
			get { return compareExecutablePath;} 
			set
			{ 
				compareExecutablePath = value; 
				if (CompareExecutablePathChanged != null)
					CompareExecutablePathChanged(this, EventArgs.Empty);
			}
		}

		//--------------------------------------------------------------------------------
		public SourceControlManager(string solutionPath, IWin32Window owner, string compareExecutablePath, ILogger logWriter)
		{
			this.currentSolutionFileName = solutionPath;
			this.owner = owner;
			this.compareExecutablePath = compareExecutablePath;
			this.logWriter = logWriter;
		}

		//--------------------------------------------------------------------------------
		public void PerformSourceTreeRefreshNeeded(object sender, EventArgs args)
		{
			if (OnSourceTreeRefreshNeeded != null)
				OnSourceTreeRefreshNeeded(sender, args);
		}

		//--------------------------------------------------------------------------------
		public bool Enabled 
		{
			get
			{ 
				return enabled;
			} 
			set 
			{
				enabled = value && (GetBindingInfo().GetSourceControlDatabase() != null); 
			}
		}

		//--------------------------------------------------------------------------------
		private ProjectBindingInfo GetBindingInfo()
		{
			string path = CommonFunctions.GetSourceSafeFilePath(currentSolutionFileName);
			ProjectBindingInfo bindingInfo = BindingInfoCache.GetBindingInfo(path);
			return bindingInfo;
		}
		
		//-------------------------------------------------------------------------------
		private void SaveLocalBindingInfo()
		{ 
			string path = CommonFunctions.GetSourceSafeFilePath(currentSolutionFileName);
			ProjectBindingInfo bindingInfo = BindingInfoCache.GetBindingInfo(path);
			bindingInfo.SaveLocalInfos(path);
		}

		//-------------------------------------------------------------------------------
		private bool VerifyBindings()
		{
			if (!Enabled) return false;

			if (!VerifySolutionPath()) return false;
			
			ProjectBindingInfo info = GetBindingInfo();
			if (info.VerifyBindings(ProjectBindingInfo.VerifyMode.All))
				return true;
			ArrayList messages = new ArrayList();
			messages.Add(info.LastError);
			Summary s = new Summary(messages, Strings.ErrorsOccurred, string.Empty, false);
			s.ShowDialog(owner);
			return false;
		}

		//--------------------------------------------------------------------------------
		public bool AdjustLocalBindings(TreeNode node)
		{
			if (!VerifySolutionPath()) return false;
			
			if (!TestValidSyncNode(node)) return false;

			ProjectBindingInfo info = GetBindingInfo();
			ArrayList invalidSourceSafePaths, localPaths, invalidLocalPaths, sourceSafePaths;
				
			try
			{
				if (!info.VerifyBindings (
					ProjectBindingInfo.VerifyMode.OnlyLocal,
					node,
					out invalidSourceSafePaths,
					out localPaths,
					out invalidLocalPaths,
					out sourceSafePaths
					)
					)
				{
					foreach (string folder in invalidLocalPaths)
						Directory.CreateDirectory(folder);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(owner, ex.Message);
				return false;
			}
			return true;

		}
		
		//--------------------------------------------------------------------------------
		private bool VerifySolutionPath()
		{
			if (!File.Exists(currentSolutionFileName))
			{
				MessageBox.Show(owner, Strings.InvalidSolutionPath);
				return false;
			}

			return true;
		}

		//--------------------------------------------------------------------------------
		public bool TestDatabaseConnection()
		{
			if (GetBindingInfo().GetSourceControlDatabase() != null)
				return true;

			switch (MessageBox.Show
				(
				owner,
				"Do you want to set Team System connection?", 
				Application.ProductName,
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question,
				MessageBoxDefaultButton.Button1
				))
			{
				case DialogResult.No:
					return false;
				case DialogResult.Yes:
					return SetTFSDatabaseConnection();
			}
			return false;
		}
		
		//--------------------------------------------------------------------------------
		public bool SetTFSDatabaseConnection()
		{
			DatabaseBindingInfo binding = GetBindingInfo().DatabaseBindingInfo;
			TFSSourceBinding form = new TFSSourceBinding(binding.TFSBinding);
			if (form.ShowDialog(owner) == DialogResult.OK)
			{
				binding.TFSBinding = form.CurrentBinding;
				SaveLocalBindingInfo();
				enabled = true;
			}
			return enabled;
		}

		//--------------------------------------------------------------------------------
		public SourceControlStatus GetSourceControlStatus(string filename, NodeType type)
		{
			if (!Enabled)
				return SourceControlStatus.NotBound;

			ISourceControlDatabase db;
			string sourceSafePath;
			ISourceControlItem item = GetSourceControlItem(filename, type, out db, out sourceSafePath, false);
			if (item == null)
				return SourceControlStatus.NotBound;

			if (item.IsCheckedOutToMe)
				return SourceControlStatus.CheckedOut;

			return SourceControlStatus.NotCheckedOut;
		}
		//--------------------------------------------------------------------------------
		public bool TryToCheckOut(string filename, NodeType type)
		{	
			try
			{
				if (!Enabled) return false;

				ISourceControlDatabase db;
				string sourceSafePath;
				ISourceControlItem item = GetSourceControlItem(filename, type, out db, out sourceSafePath, true);
				if (item == null)
				{
					logWriter.WriteLog(string.Format("Cannot find source control item for file: '{0}'", filename), TypeOfMessage.error);
					return false;
				}

				if (promptDialog.ShowDialog(owner, string.Format(Strings.CheckOutPrompt, filename, sourceSafePath)) != DialogResult.Yes)
					return false;
		
				if (	item != null &&
					item.IsDifferent(filename) &&
					Functions.RepeatableMessage
					(
					owner, 
					true,
					Strings.CheckOutDifferent, 
					sourceSafePath
					) != DialogResult.Yes
					)
					return false;
			
			
				if (db.CheckOutFile(sourceSafePath, filename))
					return true;

				Functions.RepeatableMessage(owner, Strings.CheckOutError, filename, db.LastError);
				logWriter.WriteLog(string.Format(Strings.CheckOutError, filename, db.LastError), TypeOfMessage.error);
				
				return false;
			}
			catch (Exception ex)
			{
				Functions.RepeatableMessage(owner, Strings.CheckOutError, filename, ex.Message);
				logWriter.WriteLog(string.Format(Strings.CheckOutError, filename, ex.Message), TypeOfMessage.error);
				return false;
			}
		}

		//--------------------------------------------------------------------------------
		public bool TryToRemoveFromSourceControl(string filename, NodeType type)
		{
			try
			{
				if (!Enabled) return false;

				ISourceControlDatabase db;
				string sourceSafePath;
				ISourceControlItem item = GetSourceControlItem(filename, type, out db, out sourceSafePath, true);
				if (item == null)
				{
					logWriter.WriteLog(string.Format("Cannot find source control item for file: '{0}'", filename), TypeOfMessage.error);
					return false;
				}

				if (promptDialog.ShowDialog(owner, string.Format(Strings.CheckOutPrompt, filename, sourceSafePath)) != DialogResult.Yes)
					return false;

				if (db.RemoveFile(sourceSafePath))
					return true;

				Functions.RepeatableMessage(owner, Strings.CheckOutError, filename, db.LastError);
				logWriter.WriteLog(string.Format(Strings.CheckOutError, filename, db.LastError), TypeOfMessage.error);

				return false;
			}
			catch (Exception ex)
			{
				Functions.RepeatableMessage(owner, Strings.CheckOutError, filename, ex.Message);
				logWriter.WriteLog(string.Format(Strings.CheckOutError, filename, ex.Message), TypeOfMessage.error);
				return false;
			}
		}
		//--------------------------------------------------------------------------------
		private ISourceControlItem GetSourceControlItem
			(
			string filename,
			NodeType type,
			out ISourceControlDatabase db,
			out string sourceControlPath,
			bool message
			)
		{
			ISourceControlItem item = null;
			sourceControlPath = null;
			ProjectBindingInfo bindingInfo = GetBindingInfo();

			db = bindingInfo.GetSourceControlDatabase();
			if (db == null)
			{
				Enabled = false;
				if (message)
					Functions.RepeatableMessage(owner, Strings.InvalidDatabase);
				logWriter.WriteLog(Strings.InvalidDatabase, TypeOfMessage.error);

				return null;
			}

			sourceControlPath = bindingInfo.GetSourceControlPath(filename, type);
			if (string.IsNullOrEmpty(sourceControlPath))
			{
				if (message)
					Functions.RepeatableMessage(owner, Strings.InvalidBindingInfo, filename);
				logWriter.WriteLog(string.Format(Strings.InvalidBindingInfo, filename), TypeOfMessage.error);
				return null;
			}

			if ((item = db.GetItem(sourceControlPath)) == null)
			{
				if (db is TFSDBWrapper)
				{
					if (File.Exists(filename))
						item = ((TFSDBWrapper)db).AddItem(sourceControlPath);
				}
				else
				{
					if (message)
						Functions.RepeatableMessage(owner, Strings.InvalidBindingInfo, filename);
					logWriter.WriteLog(string.Format(Strings.InvalidBindingInfo, filename), TypeOfMessage.error);
					return null;
				}
			}
			return item;
		}
		
		//--------------------------------------------------------------------------------
		public bool TestValidSyncNode(TreeNode node)
		{
			NodeType type = ((NodeTag)node.Tag).GetNodeType();
				
			if (type != NodeType.SOLUTION && 
				type != NodeType.PROJECT &&
				type != NodeType.LANGUAGE &&
				CommonUtilities.Functions.GetTypedParentNode(node, NodeType.LANGUAGE) == null
				)
			{
				MessageBox.Show(owner, Strings.InvalidSelection); 
				return false;
			}
			return true;
		}

		
		//--------------------------------------------------------------------------------
		public void UndoCheckOut(string[] files)
		{
			if (!Enabled) return;

			List<ISourceControlItem> items = GetSourceControlItems(files, true);
			SourceControlSummary form = new SourceControlSummary(items.ToArray(), Action.UndoCheckout, logWriter);
			form.ShowDialog(owner);
		}

		//--------------------------------------------------------------------------------
		public void CheckOut(string[] files)
		{
			if (!Enabled) return;

			List<ISourceControlItem> items = GetSourceControlItems(files, false);
			SourceControlSummary form = new SourceControlSummary(items.ToArray(), Action.CheckOut, logWriter);
			form.ShowDialog(owner);
		}

		//--------------------------------------------------------------------------------
		public void CheckIn(string[] files)
		{
			if (!Enabled) return;

			List<ISourceControlItem> items = GetSourceControlItems(files, true);
			SourceControlSummary form = new SourceControlSummary(items.ToArray(), Action.CheckIn, logWriter);
			form.ShowDialog(owner);
		}

		//--------------------------------------------------------------------------------
		private List<ISourceControlItem> GetSourceControlItems(string[] files, bool isToBeOut)
		{
			List<ISourceControlItem> items = new List<ISourceControlItem>();
			foreach (string filename in files)
			{
				ISourceControlDatabase db;
				string sourceSafePath;

				NodeType type = Functions.GetNodeTypeFromPath(filename);
				ISourceControlItem item = GetSourceControlItem(filename, type, out db, out sourceSafePath, true);

				if (item == null 
					|| (isToBeOut && !item.IsCheckedOutToMe) 
					|| (!isToBeOut && item.IsCheckedOutToMe))
					continue;

				items.Add(item);
			}
			return items;
		}

		//--------------------------------------------------------------------------------
		public void GetLatestVersion(LocalizerTreeNode node)
		{
			if (!Enabled) return;
				
			ISourceControlDatabase db;
			string sourceSafePath;
		
			switch (node.Type)
			{
				case NodeType.SOLUTION:
				case NodeType.PROJECT:
					{
						SafeGetLatestVersion(node, out db, out sourceSafePath);
						
						foreach (LocalizerTreeNode child in node.Nodes)
							GetLatestVersion(child);
						break;
					}
				case NodeType.LANGUAGE:
				case NodeType.LASTCHILD:
					{
						SafeGetLatestVersion(node, out db, out sourceSafePath);
						break;
					}
			}
		}

		//--------------------------------------------------------------------------------
		private void SafeGetLatestVersion(LocalizerTreeNode node, out ISourceControlDatabase db, out string sourceSafePath)
		{
			ISourceControlItem item = GetSourceControlItem(node.FileSystemPath, node.Type, out db, out sourceSafePath, true);
			if (item != null)
			{
				item.GetLatestVersion(item.LocalPath);
				logWriter.WriteLog(string.Format("Getting '{0}'...", item.Path), TypeOfMessage.info);
			}
		}


		//--------------------------------------------------------------------------------
		public void WriteReadyMessage()
		{
			logWriter.WriteLog(Strings.Ready);
		}

		#region IDisposable Members

		//--------------------------------------------------------------------------------
		public void Dispose()
		{
			BindingInfoCache.ResetBindingInfo(CommonFunctions.GetSourceSafeFilePath(currentSolutionFileName));
		}

		#endregion
	}
}
