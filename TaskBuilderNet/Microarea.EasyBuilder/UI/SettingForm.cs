using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Microarea.EasyBuilder.BackendCommunication;
using Microarea.EasyBuilder.Packager;
using Microarea.EasyBuilder.Properties;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.GenericForms;
using Microarea.TaskBuilderNet.Core.NameSolver;

namespace Microarea.EasyBuilder.UI
{
	//================================================================================
	/// <summary>Internal use</summary>
	public partial class SettingForm : ThemedForm
	{
		Credentials cred;
		bool referencesAdded;
		TreeNode referencesRoot = new TreeNode(Resources.CommonAssemblies, 0, 0);
		bool referencesChanged;
		private Sources sources;

		//--------------------------------------------------------------------------------
		internal bool ReferencesChanged
		{
			get { return referencesChanged; }
		}

		//--------------------------------------------------------------------------------
		/// <summary>Internal use</summary>
		public SettingForm(Sources sources)
		{
			this.sources = sources;
			InitializeComponent();
			treeViewAssemblies.ExpandAll();

			if (BaseCustomizationContext.CustomizationContextInstance.ShouldStandardizationsBeAvailable())
			{
				cred = new Credentials();
				cred.TopLevel = false;
				cred.Dock = DockStyle.Fill;
				cred.FormBorderStyle = FormBorderStyle.None;
				cred.BackColor = Color.White;
				cred.HideButtons = true;
				TabPageAccount.Controls.Add(cred);
			}
			else
            {
				this.tabControlOptions.TabPages.Remove(TabPageAccount);
				this.tabControlOptions.TabPages.Remove(TabPageBackendUrl);
			}

			//nascosto questo controllo perchè, nel menù EasyStudio, aprendo la settingform, risulterebbe vuota
			//  if(sources == null)	this.tabControlOptions.TabPages.Remove(TabPageCommon);

			//Si possono aggiungere o rimuovere assembly solo se non ci sono più documenti in editing di EasyStudio, al massimo uno.
			bool thereAreMultipleOpenDocuments = CUtility.GetAllOpenDocumentNumberEditMode() > 1;
			TabPageCommon.Enabled = !thereAreMultipleOpenDocuments;
			addRemoveAssemblyDisabled.Visible = thereAreMultipleOpenDocuments;


		}
		//--------------------------------------------------------------------------------
		/// <remarks/>
		protected override void OnHelpRequested(HelpEventArgs hevent)
		{
			base.OnHelpRequested(hevent);
			FormEditor.ShowHelp();
			hevent.Handled = true;
		}
		//--------------------------------------------------------------------------------
		/// <remarks/>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			cbShowHiddenFields.Checked = true; // Settings.Default.ShowHiddelFields;
			
			//cbUseMsBuild.Visible = EBLicenseManager.GenerateCsproj;
			if (BaseCustomizationContext.CustomizationContextInstance.ShouldStandardizationsBeAvailable())
			{
				bool changed = false;
				if (String.IsNullOrWhiteSpace(Settings.Default.Microarea_EasyBuilder_CrypterRef1_Crypter30))
				{
					Settings.Default.Microarea_EasyBuilder_CrypterRef1_Crypter30 = "http://www.microarea.it/Crypter30/Crypter30.asmx";
					changed = true;
				}

				if (String.IsNullOrWhiteSpace(Settings.Default.Microarea_EasyBuilder_SNGeneratorRef_SNGenerator30))
				{
					Settings.Default.Microarea_EasyBuilder_SNGeneratorRef_SNGenerator30 = "http://www.microarea.it/sngenerator/sngenerator.asmx";
					changed = true;
				}

				TxtCrypterUrl.Text = Settings.Default.Microarea_EasyBuilder_CrypterRef1_Crypter30;
				TxtSNGeneratorUrl.Text = Settings.Default.Microarea_EasyBuilder_SNGeneratorRef_SNGenerator30;

				if (changed)
					BaseCustomizationContext.CustomizationContextInstance.SaveSettings();

				cred.Show();
			}
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			if (DialogResult == DialogResult.OK)
			{
				bool changed = false;

				
				if (cbShowHiddenFields.Checked != Settings.Default.ShowHiddelFields)
				{
					Settings.Default.ShowHiddelFields = cbShowHiddenFields.Checked;
					changed = true;
				}

                if (cbUseMsBuild.Checked != Settings.Default.UseMsBuild)
                {
                    Settings.Default.UseMsBuild = cbUseMsBuild.Checked;
                    changed = true;
                }

                bool credentialsChanged = false;
				if (cred != null && cred.Username != Settings.Default.Username)
				{
					credentialsChanged = true;
				}

				if (cred != null && cred.Password != Crypto.Decrypt(Settings.Default.Password))
				{
					credentialsChanged = true;
				}

				if (credentialsChanged)
				{
					if (cred != null && !CrypterWrapper.TestCredentials(cred.Username, cred.Password))
					{
						MessageBox.Show(Resources.WrongCredentials);
						e.Cancel = true;
						return;
					}
					if (cred != null)
					{
						Settings.Default.Username = cred.Username;
						Settings.Default.Password = Crypto.Encrypt(cred.Password);
					}
					changed = true;
				}

				if (TxtCrypterUrl.Text != Settings.Default.Microarea_EasyBuilder_CrypterRef1_Crypter30)
				{
					Settings.Default.Microarea_EasyBuilder_CrypterRef1_Crypter30 = TxtCrypterUrl.Text;
					changed = true;
				}

				if (TxtSNGeneratorUrl.Text != Settings.Default.Microarea_EasyBuilder_SNGeneratorRef_SNGenerator30)
				{
					Settings.Default.Microarea_EasyBuilder_SNGeneratorRef_SNGenerator30 = TxtSNGeneratorUrl.Text;
					changed = true;
				}

				if (changed)
					BaseCustomizationContext.CustomizationContextInstance.SaveSettings();

				try
				{
					string referencesPath = PathFinderWrapper.GetEasyStudioReferenceAssembliesPath();
					if (Directory.Exists(referencesPath))
					{
						//prima cancello da file system quelli che ho rimosso
						foreach (string file in Directory.GetFiles(referencesPath, "*.dll"))
						{
							string name = Path.GetFileName(file);
							if (!ExistAssembly(name))
							{
                                if (BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp != null)
                                    BaseCustomizationContext
                                        .CustomizationContextInstance
                                        .CurrentEasyBuilderApp
                                        .EasyBuilderAppFileListManager
                                        .RemoveFromCustomListAndFromFileSystem(file);
                                else
                                    File.Delete(file);
								referencesChanged = true;
							}
						}
					}

					if (referencesRoot.Nodes.Count > 0 && !Directory.Exists(referencesPath))
						Directory.CreateDirectory(referencesPath);

					foreach (TreeNode t in referencesRoot.Nodes)
					{
						string file = t.Tag as string;
						if (!string.IsNullOrEmpty(file) && !file.StartsWith(referencesPath, StringComparison.InvariantCultureIgnoreCase))
						{
							string name = Path.GetFileName(file);
							string destFile = Path.Combine(referencesPath, name);
							File.Copy(file, destFile, true);
							BaseCustomizationContext.CustomizationContextInstance.AddToCurrentCustomizationList(destFile);
							referencesChanged = true;
						}
					}

					if (referencesChanged && sources != null)
					{
						sources.RefreshReferencedAssemblies(true);
						sources.OnCodeChanged();
					}
				}
				catch (Exception ex)
				{
					MessageBox.Show(this, ex.Message);
					e.Cancel = true;
				}
			}
			base.OnFormClosing(e);

		}

		//--------------------------------------------------------------------------------
		private void TabPageCommon_Layout(object sender, LayoutEventArgs e)
		{
			if (referencesAdded)
				return;

			referencesAdded = true;
			treeViewAssemblies.Nodes.Add(referencesRoot);
			string referencesPath = PathFinderWrapper.GetEasyStudioReferenceAssembliesPath();
            if (!PathFinderWrapper.ExistFolder(referencesPath))
				return;
			
			foreach (string file in PathFinderWrapper.GetFiles(referencesPath, "*.dll"))
				AddAssembly(file, false);
						
		}

		//--------------------------------------------------------------------------------
		private void AddAssembly(string file, bool check)
		{
			string name = Path.GetFileName(file);
			if (check && ExistAssembly(name))
				throw new ApplicationException(string.Format(Resources.AssemblyAlreadyExisting, name));
			
			//controllo se è un assembly valido
			AssemblyName an = AssemblyName.GetAssemblyName(file);
			
			TreeNode n = new TreeNode(name, 1, 1);
			n.Tag = file;
			n.Name = name;
			referencesRoot.Nodes.Add(n);
		}

		//--------------------------------------------------------------------------------
		private bool ExistAssembly(string name)
		{
			foreach (TreeNode child in referencesRoot.Nodes)
				if (string.Compare(child.Name, name, StringComparison.InvariantCultureIgnoreCase) == 0)
					return true;
			return false;
		}

		//--------------------------------------------------------------------------------
		private void tsbAdd_Click(object sender, EventArgs e)
		{
			AddNode();
		}

		//--------------------------------------------------------------------------------
		private void tsbDelete_Click(object sender, EventArgs e)
		{
			RemoveNode();
		}

		//--------------------------------------------------------------------------------
		private void AddNode()
		{
			using (OpenFileDialog ofd = new OpenFileDialog())
			{
				ofd.Filter = "Assembly (*.dll)|*.dll";
				if (DialogResult.OK == ofd.ShowDialog(this))
				{
					try
					{
						AddAssembly(ofd.FileName, true);
					}
					catch (Exception ex)
					{
						MessageBox.Show(this, ex.Message);
					}
				}
			}
			GC.Collect();
			GC.WaitForPendingFinalizers();//per evitare un problema di rilascio dell'oggetto COM quando ormai il thread è morto (DisconnectedContext)
		}

		//--------------------------------------------------------------------------------
		private void RemoveNode()
		{
			TreeNode n = treeViewAssemblies.SelectedNode;
			if (n != null && n.Tag != null)
				n.Remove();
		}

		//--------------------------------------------------------------------------------
		private void cmsCommonAssemblies_Opening(object sender, System.ComponentModel.CancelEventArgs e)
		{
			Point p = MousePosition;
			Point clientPoint = treeViewAssemblies.PointToClient(p);
			treeViewAssemblies.SelectedNode = treeViewAssemblies.GetNodeAt(clientPoint);

			tsmiAddAssembly.Visible = true;
			tsmiRemoveAssembly.Visible = treeViewAssemblies.SelectedNode != null && treeViewAssemblies.SelectedNode != referencesRoot;
		}

		//--------------------------------------------------------------------------------
		private void tsmiAddAssembly_Click(object sender, EventArgs e)
		{
			AddNode();
		}

		//--------------------------------------------------------------------------------
		private void tsmiRemoveAssembly_Click(object sender, EventArgs e)
		{
			RemoveNode();
		}

		//--------------------------------------------------------------------------------
		private void treeViewAssemblies_AfterSelect(object sender, TreeViewEventArgs e)
		{
			TreeNode n = treeViewAssemblies.SelectedNode;
			tsbDelete.Enabled = n != null && n.Tag != null;
		}
	}
}
