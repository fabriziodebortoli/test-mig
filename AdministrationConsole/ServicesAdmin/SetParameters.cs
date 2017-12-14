using System;
using System.Collections;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using System.Xml;

using Microarea.Console.Core.PlugIns;
using Microarea.TaskBuilderNet.Core.DiagnosticUI;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Console.Plugin.ServicesAdmin
{
	//================================================================================
	public partial class SetParameters : PlugInsForm
	{
		private SystemParametersTreeNode node = null;
		private SectionInfo section = null;
		private PathFinder pathFinder = null;

		private CustomEditListViewItem listViewItem;

		//Disabilita pulsante Cancella della Console---------------------------
		public event System.EventHandler OnDisableDeleteToolBarButton;
		public delegate void RefreshSectionsArrayEventHandler(object sender, SystemParametersTreeNode node);
		public event RefreshSectionsArrayEventHandler OnRefreshSectionsArray;

		//---------------------------------------------------------------------
		public SetParameters
			(
			SystemParametersTreeNode selectedSettingNode,
			SectionInfo aSectionInfo,
			PathFinder aPathFinder,
			bool allSettings
			)
		{
			node = selectedSettingNode;
			section = aSectionInfo;
			pathFinder = aPathFinder;

			InitializeComponent();

			AddObjectForEditing();

			LoadNoHiddenSettings(allSettings);
		}

		#region Inizializzazione
		//--------------------------------------------------------------------------------
		private void AddObjectForEditing()
		{
			editListView.ChkBox.Size = new Size(0, 0);
			editListView.ChkBox.Location = new Point(0, 0);

			editListView.Controls.Add(editListView.ChkBox);
			editListView.ChkBox.Font = new Font("Verdana", 10F, FontStyle.Regular, GraphicsUnit.Point, ((System.Byte)(0)));
			editListView.ChkBox.CheckedChanged += new EventHandler(this.chkBox_CheckedChanged);
			editListView.ChkBox.CheckAlign = ContentAlignment.MiddleLeft;
			editListView.ChkBox.Hide();
			editListView.ChkBox.Text = "";

			editListView.Controls.Add(editListView.EditBox);
			editListView.EditBox.Size = new Size(0, 0);
			editListView.EditBox.Location = new Point(0, 0);
			editListView.EditBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.EditOver);
			editListView.EditBox.Font = new Font("Verdana", 10F, FontStyle.Regular, GraphicsUnit.Point, ((System.Byte)(0)));
			editListView.EditBox.TextChanged += new EventHandler(this.editBox_TextChanged);
			editListView.EditBox.KeyPress += new KeyPressEventHandler(editBox_KeyPress);
			editListView.EditBox.AutoSize = false;
			editListView.EditBox.BorderStyle = BorderStyle.FixedSingle;
			editListView.EditBox.Hide();
		}

		//---------------------------------------------------------------------
		private void editBox_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			if (string.Compare(listViewItem.Type, ConstString.integer, true, CultureInfo.InvariantCulture) == 0)
			{
				if (e.KeyChar == 8)
					return;

				if (!Char.IsNumber(e.KeyChar) && e.KeyChar != '-')
					e.Handled = true;
			}
		}

		//---------------------------------------------------------------------
		private void chkBox_CheckedChanged(object sender, System.EventArgs e)
		{
			if (State != StateEnums.Editing)
				State = StateEnums.Editing;
		}

		//---------------------------------------------------------------------
		private void editBox_TextChanged(object sender, System.EventArgs e)
		{
			if (State != StateEnums.Editing)
				State = StateEnums.Editing;
		}

		//---------------------------------------------------------------------
		private void LoadNoHiddenSettings(bool allSettings)
		{
			foreach (SettingItem aSettingItem in section.Settings)
			{
				//Controllo che non sia un setting idden= true
				if (aSettingItem.Hidden && !allSettings)
					continue;

				if (aSettingItem.SourceFileType != SourceOfSettingsConfig.Standard)
					if (IsStandardHiddenSetting(aSettingItem, section.Settings) || !ExistStandardItem(aSettingItem, section.Settings))
						continue;

				AddElementToListView(section.GetSettingItemByName(aSettingItem.Name));
			}
		}

		//---------------------------------------------------------------------
		private bool ExistStandardItem(SettingItem aSettingItem, ArrayList settings)
		{
			foreach (SettingItem item in section.Settings)
			{
				if (String.Compare(aSettingItem.Name, item.Name) == 0 && 
					item.SourceFileType == SourceOfSettingsConfig.Standard)
					return true;
			}

			return false;
		}

		//---------------------------------------------------------------------
		private bool IsStandardHiddenSetting(SettingItem aSettingItem, ArrayList settings)
		{
			foreach (SettingItem item in section.Settings)
			{
				if (String.Compare(aSettingItem.Name, item.Name) == 0 
					&& item.SourceFileType == SourceOfSettingsConfig.Standard)
					if (item.Hidden)
						return true;
			}

			return false;
		}

		//---------------------------------------------------------------------
		private void AddElementToListView(SettingItem aSettingItem)
		{
			//devo controllare di non averlo inserito
			foreach (CustomEditListViewItem item in this.editListView.Items)
			{
				if (string.Compare(item.Name, aSettingItem.Name, true, CultureInfo.InvariantCulture) == 0)
					return;
			}

			CustomEditListViewItem listViewItem = new CustomEditListViewItem();
			listViewItem.Text = aSettingItem.Name;
			listViewItem.Localize = aSettingItem.Localize;
			listViewItem.Name = aSettingItem.Name;
			listViewItem.Type = aSettingItem.Type;
			listViewItem.Hidden = aSettingItem.Hidden;
			listViewItem.ParameterValue = aSettingItem.Values[0].ToString();
			listViewItem.ImageIndex = PlugInTreeNode.GetLoginsDefaultImageIndex;
			listViewItem.ImageIndex = PlugInTreeNode.GetLoginsDefaultImageIndex;
			if (aSettingItem.Name.Length > 7)
			{
				if (String.Compare(aSettingItem.Name.ToUpper(CultureInfo.InvariantCulture).Substring(0, 8), "PASSWORD") == 0)
				{
					string pw = "";
					for (int i = 0; i < aSettingItem.Values[0].ToString().Trim().Length; i++)
					{
						pw = pw + "*";
					}
					listViewItem.SubItems.Add(pw);
					listViewItem.IsPassword = true;
				}
				else
					listViewItem.SubItems.Add(aSettingItem.Values[0].ToString());
			}
			else
				listViewItem.SubItems.Add(aSettingItem.Values[0].ToString());

			editListView.Items.Add(listViewItem);

			if (aSettingItem.SourceFileType != SourceOfSettingsConfig.Standard)
				listViewItem.BackColor = Color.LightBlue;

			if (aSettingItem.UserSetting)
				listViewItem.BackColor = Color.LemonChiffon;
		}
		#endregion

		#region Eventi sulla Lista
		//--------------------------------------------------------------------------------
		private void EditOver(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			if (e.KeyChar == 13)
			{
				listViewItem.SubItems[1].Text = editListView.EditBox.Text;
				editListView.EditBox.Hide();
			}

			if (e.KeyChar == 27)
				editListView.EditBox.Hide();
		}

		//---------------------------------------------------------------------
		private void Edit()
		{
			int widthCustomCell = 0;
			int nStart = 0;
			int spos = 0;
			int epos = editListView.Columns[0].Width;

			for (int i = 0; i < editListView.Columns.Count; i++)
			{
				if (nStart > spos && nStart < epos)
					break;

				spos = epos;
				epos += editListView.Columns[i].Width;
			}

			string subItemText = listViewItem.SubItems[1].Text;
			widthCustomCell = editListView.Columns[1].Width;

			if (listViewItem.Type == ConstString.boolType)
			{
				Rectangle r = new Rectangle(spos, listViewItem.Bounds.Y, widthCustomCell, listViewItem.Bounds.Bottom);
				editListView.ChkBox.Size = new Size(widthCustomCell, listViewItem.Bounds.Bottom - listViewItem.Bounds.Top);
				editListView.ChkBox.Location = new Point(editListView.Columns[0].Width, listViewItem.Bounds.Y);
				editListView.ChkBox.Show();

				if (subItemText == "1")
					editListView.ChkBox.Checked = true;
				else
					editListView.ChkBox.Checked = false;
				editListView.ChkBox.Focus();
				Application.DoEvents();
				editListView.ChkBox.ImageAlign = ContentAlignment.MiddleLeft;
			}
			else
			{
				Rectangle r = new Rectangle(spos, listViewItem.Bounds.Y, widthCustomCell, listViewItem.Bounds.Bottom);
				editListView.EditBox.Size = new Size(widthCustomCell, listViewItem.Bounds.Height);
				editListView.EditBox.Location = new Point(editListView.Columns[0].Width, listViewItem.Bounds.Y);
				editListView.EditBox.Show();

				editListView.EditBox.Enabled = true;
				editListView.EditBox.Text = subItemText;

				editListView.EditBox.SelectAll();
				Application.DoEvents();
				editListView.EditBox.Focus();
				Application.DoEvents();
			}
		}

		//---------------------------------------------------------------------
		private void editListView_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			ListView list = (ListView)sender;

			if (list.SelectedItems == null || list.SelectedItems.Count == 0)
			{
				SetNewValueInListView();
				return;
			}

			listViewItem = (CustomEditListViewItem)list.SelectedItems[0];
			EditRow(listViewItem);
		}

		//---------------------------------------------------------------------
		private void EditRow(CustomEditListViewItem listItem)
		{
			//lblParametersName.Text = listItem.Text + " :";
			lblParameterDescription.Text = listItem.Localize;

			if (!listItem.IsPassword)
				Edit();
			else
			{
				//chiedo conferma della password
				ChangePassword changePassword = new ChangePassword();
				changePassword.OnChangePasswordEvent += new ChangePassword.ChangePasswordEventHandler(OnChangePassword);
				changePassword.OldPassword = listItem.ParameterValue;
				changePassword.Focus();
				changePassword.ShowDialog();
			}
		}

		//---------------------------------------------------------------------
		private void OnChangePassword(object sender, string newPassword)
		{
			listViewItem.ParameterValue = newPassword;
			string pw = "";
			for (int i = 0; i < newPassword.Length; i++)
				pw = pw + "*";
			listViewItem.SubItems[1].Text = pw;
		}
		#endregion

		//--------------------------------------------------------------------------------
		public void SaveCustomSettings()
		{
			//innanzi tutto devo far scomparire se c'è ancora il controllo di editing
			//e mettere il valore giusto
			SetNewValueInListView();

			//Mi creo una section di appoggio dove metterò tutti i valori da salvare
			SectionInfo sectionToSave = new SectionInfo(node.SectionInfoNode.Name, node.SectionInfoNode.Localize);
			sectionToSave.AllowNewSettings = node.SectionInfoNode.AllowNewSettings;
			sectionToSave.Hidden = node.SectionInfoNode.Hidden;
			sectionToSave.Release = node.SectionInfoNode.Release;
			//Setting Item con il valore da Salvare
			SettingItem settingItemToCompare = null;
			SectionInfo actualSection = null;

			//Devo ricaricarmi i setting per quella sezione perchè qualcuno potrebbe avermeli modificati sotto il sedere
			ModuleInfo moduleInfo = (ModuleInfo)pathFinder.GetModuleInfoByName(node.ApplicationName, node.Module.Name);

			SettingsConfigInfo aSettingsConfigInfo = new SettingsConfigInfo(node.FileName, moduleInfo);
			aSettingsConfigInfo.Parse(); // lo parso

			if (aSettingsConfigInfo.Sections == null)
				return;

			actualSection = aSettingsConfigInfo.GetSectionInfoByName(node.SectionInfoNode.Name);

			//A questo punto ho in canna la situazione reale dei settings e la devo andare a confrontare con quella
			//che avevo in canna io quando ho iniziato a lavorare sui settings di quella sezione
			if (section.isModify(actualSection))
			{
				DialogResult res = DiagnosticViewer.ShowQuestion("I parametri sono stati modificati dall'Amministratore. Procedere con il salvataggio ugualmente?", "Errore");
				if (res == DialogResult.No || res == DialogResult.Cancel)
					return;
			}

			ArrayList arraySettings = null;
			SettingItem settingBySourceType = null;

			foreach (CustomEditListViewItem item in editListView.Items)
			{
				//PerOgni elemento lo devo controntare con quelli della standard
				arraySettings = actualSection.GetSettingsItemByName(item.Name);
				if (arraySettings == null)
					continue;

				//Ora becco quello della standard 
				settingBySourceType = SectionInfo.GetSettingBySourceType(SourceOfSettingsConfig.Standard, arraySettings);
				if (settingBySourceType == null)
					continue;

				settingItemToCompare = settingBySourceType;
				//COnfronto l'elemento della standard con quello a video
				if (string.Compare(settingBySourceType.Values[0].ToString(), item.SubItems[1].Text, true, CultureInfo.InvariantCulture) != 0)
				{
					//Il valore è diverso da quello della standard quindi lo aggiungo alla lista dei settings da sanvare
					//con suorce ALLCOMPANIES/ALLUSERS 
					SettingItem setting = new SettingItem(item.Name, item.Type);
					setting.Hidden = item.Hidden;
					setting.Localize = item.Localize;
					setting.Release = item.Release;
					setting.SourceFileType = SourceOfSettingsConfig.AllCompaniesAllUsers;
					setting.UserSetting = item.AllowAddNewParameter;
					setting.Values.Add((item.IsPassword) ? item.ParameterValue : item.SubItems[1].Text);
					//Lo aggiungo
					sectionToSave.AddSetting(setting);
					settingItemToCompare = setting;
					if (item.AllowAddNewParameter)
						item.BackColor = Color.LemonChiffon;
					if (item.AllowAddNewParameter)
						item.BackColor = Color.LightBlue;
				}
				else
				{
					item.BackColor = SystemColors.Window;
					//Controllo se esiste nella session CUSTOM ALLCOMPANIES/ALLUSERS se si lo tolgo
					settingBySourceType = SectionInfo.GetSettingBySourceType(SourceOfSettingsConfig.AllCompaniesAllUsers, arraySettings);
					if (settingBySourceType == null)
						continue;
					if (String.Compare(settingBySourceType.Values[0].ToString(), item.SubItems[1].Text, true, CultureInfo.InvariantCulture) != 0)
					{
						//Vuol dire che me l'hanno rimessa come prima quindi la devo eliminare dal file nella CUSTOM
						DeleteParameter(settingBySourceType.Name, item);
					}
				}

				//Cerco quello nella ALLCOMPANIES/NOME_USER
				settingItemToCompare = CompareSettingItemValue(SourceOfSettingsConfig.AllCompaniesSpecificiUser,
					actualSection,
					item.Name,
					settingItemToCompare,
					sectionToSave);

				//Confronto con quello nella NomeCOMPANY/ALLUSERS
				settingItemToCompare = CompareSettingItemValue(SourceOfSettingsConfig.SpecificiCompanyAllUsers,
					actualSection,
					item.Name,
					settingItemToCompare,
					sectionToSave);

				//Confronto con quello nella NomeCOMPANY/USERS
				settingItemToCompare = CompareSettingItemValue(SourceOfSettingsConfig.SpecificiCompanySpecificiUser,
					actualSection,
					item.Name,
					settingItemToCompare,
					sectionToSave);

				//Ora devo cercare i setting aggiunti a mano dall'utente; possono essere solo nella  ALLCOMPANIES/ALLUSERS
				// quindi scorro la lista a video e cerco tutti quelli che sono AllowAddNewParameter = true
				SettingItem userSettingItem = null;
				if (sectionToSave.AllowNewSettings)
				{
					foreach (CustomEditListViewItem itemCustom in editListView.Items)
					{
						if (item.AllowAddNewParameter)
							userSettingItem = AddSettingToSection(itemCustom, SourceOfSettingsConfig.AllCompaniesAllUsers, sectionToSave);
					}
				}
			}

			//DEvo looppare sui setting hidden perche potrebbero aver fatto qualche mastrusso
			//a mano nella custom
			foreach (SettingItem item in actualSection.Settings)
			{
				if (!item.Hidden)
					continue;
				//PerOgni elemento lo devo controntare con quelli della standard
				arraySettings = actualSection.GetSettingsItemByName(item.Name);
				//Controllo se esiste nella session CUSTOM ALLCOMPANIES/ALLUSERS se si lo tolgo
				settingBySourceType = SectionInfo.GetSettingBySourceType(SourceOfSettingsConfig.AllCompaniesAllUsers, arraySettings);
				if (settingBySourceType == null)
					continue;

				sectionToSave.AddSetting(settingBySourceType);
			}

			//Cerco i Setting da mettere nella ALLCOMPANIES/ALLUSER
			ArrayList settingsBySourceType = sectionToSave.GetSettingsBySourceType(SourceOfSettingsConfig.AllCompaniesAllUsers);
			if (settingsBySourceType != null)
				SaveSettingsBySourceType(SourceOfSettingsConfig.AllCompaniesAllUsers, moduleInfo, settingsBySourceType);
			//Cerco i setting nella ALLCOMPANIES/USER
			settingsBySourceType = sectionToSave.GetSettingsBySourceType(SourceOfSettingsConfig.AllCompaniesSpecificiUser);
			if (settingsBySourceType != null && settingsBySourceType.Count != 0)
				SaveSettingsBySourceType(SourceOfSettingsConfig.AllCompaniesSpecificiUser, moduleInfo, settingsBySourceType);
			//Cerco i setting nella COMPANIES/ALLUSER
			settingsBySourceType = sectionToSave.GetSettingsBySourceType(SourceOfSettingsConfig.SpecificiCompanyAllUsers);
			if (settingsBySourceType != null && settingsBySourceType.Count != 0)
				SaveSettingsBySourceType(SourceOfSettingsConfig.SpecificiCompanyAllUsers, moduleInfo, settingsBySourceType);
			//Cerco i setting nella COMPANIES/USER
			settingsBySourceType = sectionToSave.GetSettingsBySourceType(SourceOfSettingsConfig.SpecificiCompanySpecificiUser);
			if (settingsBySourceType != null && settingsBySourceType.Count != 0)
				SaveSettingsBySourceType(SourceOfSettingsConfig.SpecificiCompanySpecificiUser, moduleInfo, settingsBySourceType);

			State = StateEnums.None;
			if (OnRefreshSectionsArray != null)
				OnRefreshSectionsArray(this, node);
		}

		//---------------------------------------------------------------------
		private SettingItem CompareSettingItemValue
			(
			SourceOfSettingsConfig sourceType,
			SectionInfo sectionInfo,
			string settingName,
			SettingItem aSettingItem,
			SectionInfo sectionToSave
			)
		{
			ArrayList arraySettingByName = sectionInfo.GetSettingsItemByName(settingName);
			SettingItem settingItemBySource = SectionInfo.GetSettingBySourceType(sourceType, arraySettingByName);
			if (settingItemBySource != null)
			{
				//COnfronto se sono uguali con quello precedente
				if (string.Compare(aSettingItem.Values[0].ToString(), settingItemBySource.Values[0].ToString(), true, CultureInfo.InvariantCulture) != 0)
					aSettingItem = AddSettingToSection(settingItemBySource, sourceType, sectionToSave);
			}
			return aSettingItem;
		}

		//---------------------------------------------------------------------
		private SettingItem AddSettingToSection(CustomEditListViewItem aCustomEditListViewItem, SourceOfSettingsConfig source, SectionInfo section)
		{
			SettingItem setting = new SettingItem(aCustomEditListViewItem.Name, aCustomEditListViewItem.Type);
			setting.Hidden = aCustomEditListViewItem.Hidden;
			setting.Localize = aCustomEditListViewItem.Localize;
			setting.Release = aCustomEditListViewItem.Release;
			setting.SourceFileType = source;
			setting.UserSetting = aCustomEditListViewItem.AllowAddNewParameter;
			setting.Values.Add(aCustomEditListViewItem.SubItems[1]);
			//Lo aggiungo
			section.AddSetting(setting);
			return setting;
		}

		//----------------------------------------------------------------------
		private SettingItem AddSettingToSection(SettingItem aSetting, SourceOfSettingsConfig source, SectionInfo section)
		{
			SettingItem setting = new SettingItem(aSetting.Name, aSetting.Type);
			setting.Hidden = aSetting.Hidden;
			setting.Localize = aSetting.Localize;
			setting.Release = aSetting.Release;
			setting.SourceFileType = source;
			setting.UserSetting = aSetting.UserSetting;
			setting.Values.Add(aSetting.Values[0]);
			//Lo aggiungo
			section.AddSetting(setting);
			return setting;
		}

		//---------------------------------------------------------------------
		private void SaveSettingsBySourceType(SourceOfSettingsConfig source, ModuleInfo aModuleInfo, ArrayList aSettings)
		{
			XmlDocument xmlDocument = null;
			SettingsConfigInfo aSettingsConfigInfo = new SettingsConfigInfo(node.FileName, aModuleInfo);

			string path = GetSettingsFilePathBySourceType(source, aModuleInfo);
			if (string.IsNullOrWhiteSpace(path))
				return;

			if (aSettingsConfigInfo == null)
				return;

			if (!File.Exists(path))
			{
				string directoryPath = GetSettingsPathBySourceType(source, aModuleInfo);
				xmlDocument = aSettingsConfigInfo.CreateDocument();
				if (!Directory.Exists(aModuleInfo.GetCustomAllCompaniesAllUsersSettingsPath()))
					Directory.CreateDirectory(aModuleInfo.GetCustomAllCompaniesAllUsersSettingsPath());
			}
			else
			{
				xmlDocument = new XmlDocument();
				xmlDocument.Load(path);
				xmlDocument = aSettingsConfigInfo.DeleteSection(xmlDocument, node.SectionInfoNode.Name);
			}

			SectionInfo sectionInfo = new SectionInfo(node.SectionInfoNode.Name, node.SectionInfoNode.Localize);

			foreach (SettingItem item in aSettings)
				sectionInfo.Settings.Add(item);

			xmlDocument = aSettingsConfigInfo.UnparseSection(xmlDocument, sectionInfo);

			xmlDocument.Save(path);
		}

		//----------------------------------------------------------------------
		private string GetSettingsPathBySourceType(SourceOfSettingsConfig source, ModuleInfo aModuleInfo)
		{
			switch (source)
			{
				case SourceOfSettingsConfig.AllCompaniesAllUsers:
					return aModuleInfo.GetCustomAllCompaniesAllUsersSettingsPath();

				case SourceOfSettingsConfig.AllCompaniesSpecificiUser:
					return aModuleInfo.GetCustomAllCompaniesUserSettingsPath();

				case SourceOfSettingsConfig.SpecificiCompanyAllUsers:
					return aModuleInfo.GetCustomCompanyAllUserSettingsPath();

				case SourceOfSettingsConfig.SpecificiCompanySpecificiUser:
					return aModuleInfo.GetCustomCompanyUserSettingsPath();
			}

			return string.Empty;
		}

		//---------------------------------------------------------------------
		private string GetSettingsFilePathBySourceType(SourceOfSettingsConfig source, ModuleInfo aModuleInfo)
		{
			switch (source)
			{
				case SourceOfSettingsConfig.AllCompaniesAllUsers:
					return aModuleInfo.GetCustomAllCompaniesAllUsersSettingsFullFilename(node.FileName);

				case SourceOfSettingsConfig.AllCompaniesSpecificiUser:
					return aModuleInfo.GetCustomAllCompaniesUserSettingsFullFilename(node.FileName);

				case SourceOfSettingsConfig.SpecificiCompanyAllUsers:
					return aModuleInfo.GetCustomCompanyAllUserSettingsPathFullFilename(node.FileName);

				case SourceOfSettingsConfig.SpecificiCompanySpecificiUser:
					return aModuleInfo.GetCustomCompanyUserSettingsPathFullFilename(node.FileName);
			}
			return string.Empty;
		}

		//---------------------------------------------------------------------
		public void DeleteParameter(string settingName, CustomEditListViewItem customEditListViewItem)
		{
			if (customEditListViewItem.AllowAddNewParameter)
			{
				int index = editListView.SelectedIndices[0];
				editListView.Items[index].Remove();
				editListView.Refresh();
			}

			ModuleInfo moduleInfo = (ModuleInfo)pathFinder.GetModuleInfoByName(node.ApplicationName, node.Module.Name);
			string path = moduleInfo.GetCustomAllCompaniesAllUsersSettingsFullFilename(node.FileName);
			if (!File.Exists(path)) 
				return;
			
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(path);
			
			try
			{
				string xpath = string.Format
				(
				"/{0}/{1}[@name='" + node.SectionInfoNode.Name + "']",
				"ParameterSettings",
				SettingsConfigXML.Element.Section
				);
				
				XmlNode xmlNodeSection = xmlDocument.SelectSingleNode(xpath);
				if (xmlNodeSection == null) 
					return;
				
				xpath = string.Format
					(
					"/{0}/{1}/{2}[@name='" + settingName + "']",
					"ParameterSettings",
					SettingsConfigXML.Element.Section,
					SettingsConfigXML.Element.Setting
					);

				XmlNode xmlNodeSetting = xmlDocument.SelectSingleNode(xpath);
				if (xmlNodeSetting == null) 
					return;
				xmlNodeSection.RemoveChild(xmlNodeSetting);
				
				xpath = string.Format("/{0}", "ParameterSettings");
				XmlNode xmlNodeParam = xmlDocument.SelectSingleNode(xpath);
				if (xmlNodeParam == null) 
					return;

				if (xmlNodeSection.ChildNodes.Count == 0)
					xmlNodeParam.RemoveChild(xmlNodeSection);
				
				File.Delete(path);
				
				xmlDocument.Save(path);
			}
			catch (XmlException)
			{
			}

			if (OnDisableDeleteToolBarButton != null)
			{
				object sender = new object();
				EventArgs e = new EventArgs();
				OnDisableDeleteToolBarButton(sender, e);
			}
		}

		//---------------------------------------------------------------------
		private void SetParameters_Resize(object sender, System.EventArgs e)
		{
			if (this.editListView.Columns.Count > 0)
			{
				for (int i = 0; i < this.editListView.Columns.Count; i++)
					this.editListView.Columns[i].Width = -2;
			}
		}

		//---------------------------------------------------------------------
		private void SetNewValueInListView()
		{
			if (editListView.ChkBox.Visible)
			{
				if (editListView.ChkBox.Checked)
					listViewItem.SubItems[1].Text = "1";
				else
					listViewItem.SubItems[1].Text = "0";
				editListView.ChkBox.Hide();
			}

			if (editListView.EditBox.Visible)
			{
				listViewItem.SubItems[1].Text = editListView.EditBox.Text;
				editListView.EditBox.Hide();
			}
		}

		//--------------------------------------------------------------------
		private void editListView_DoubleClick(object sender, System.EventArgs e)
		{
			ListView list = (ListView)sender;

			if (list.SelectedItems == null || list.SelectedItems.Count == 0)
			{
				SetNewValueInListView();
				return;
			}

			listViewItem = (CustomEditListViewItem)list.SelectedItems[0];
			EditRow(listViewItem);
		}

		//---------------------------------------------------------------------
		/*		private void btnAddParameter_Click(object sender, System.EventArgs e)
				{
					if (txtParameterName.Text.Length == 0)
					{
						DiagnosticViewer.ShowWarning(Strings.InsertNewName, ConstString.servicePlugIn);
						return;
					}

					foreach (CustomEditListViewItem listViewItem in editListView.Items)
					{
						if (listViewItem.Name == txtParameterName.Text.Trim())
						{
							DiagnosticViewer.ShowWarning(Strings.ExistParameterName, ConstString.servicePlugIn);
							return;
						}
					}

					if (txtParameterLocalize.Text.Length == 0)
					{
						DiagnosticViewer.ShowWarning(Strings.InsertNewDescription, ConstString.servicePlugIn);
						return;
					}

					foreach (CustomEditListViewItem listViewItem in editListView.Items)
					{
						if (listViewItem.Localize == this.txtParameterLocalize.Text)
						{
							DiagnosticViewer.ShowWarning(Strings.ExistParameterDescription, ConstString.servicePlugIn);
							return;
						}
					}

					if (txtParameterValue.Text.Length == 0)
					{
						DiagnosticViewer.ShowWarning(Strings.InsertNewValue, ConstString.servicePlugIn);
						return;
					}

					//Aggiungo il valore alla ListView
					CustomEditListViewItem listViewItemNew = new CustomEditListViewItem();
					listViewItemNew.Text = txtParameterLocalize.Text;
					listViewItemNew.Name = txtParameterName.Text.Trim();
					listViewItemNew.Localize = txtParameterLocalize.Text;
					listViewItemNew.Type = "string";
					listViewItemNew.AllowAddNewParameter = true;
					listViewItemNew.SubItems.Add(txtParameterValue.Text);
					editListView.Items.Add(listViewItemNew);
					editListView.Refresh();

					txtParameterName.Text = "";
					txtParameterLocalize.Text = "";
					txtParameterValue.Text = "";
 
				}*/
	}
}
