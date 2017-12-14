using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microarea.EasyBuilder.Properties;
using System.IO;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.UI.TBWebFormControl;
using Microarea.TaskBuilderNet.Core.GenericForms;
using System.Xml;
using System.Linq;
using Microarea.TaskBuilderNet.Core.NameSolver;

namespace Microarea.EasyBuilder.UI
{
	//=========================================================================
	internal partial class AddItem : ThemedForm
	{

		public enum ItemType
		{
			Generic_Element= -2,
			Folder = -1,
			Panel = WndObjDescription.WndObjType.Panel,
			Tile_Mini,
			Tile_Standard,
			Tile_Wide,
			View = WndObjDescription.WndObjType.View,
			Frame = WndObjDescription.WndObjType.Frame,
			Href = 70
		}
		private string folder;
		private string optionOpenFileSystem = "open from file system ...";

		//--------------------------------------------------------------------------------
		public string JsonFile { get { return txtFilePath.Text; } }

		//--------------------------------------------------------------------------------
		internal ItemType FormType
		{
			get
			{
				if (comboFormType.Enabled)
					return (ItemType)comboFormType.SelectedItem;
				return ItemType.Href;
			}
		}

		//--------------------------------------------------------------------------------
		public AdditemComboOption PathHref { get; private set; }

		//--------------------------------------------------------------------------------
		public AddItem(string folder, bool isFromDocOutline)
		{
			this.folder = folder;
			InitializeComponent();
			PopulateComboType(isFromDocOutline);
			txtFormName.Focus();
		}

		//--------------------------------------------------------------------------------
		private void PopulateComboType(bool isFromDocOutline)
		{
			comboFormType.Items.Add(ItemType.Generic_Element);
			comboFormType.Items.Add(ItemType.Tile_Mini);
			comboFormType.Items.Add(ItemType.Tile_Standard);
			comboFormType.Items.Add(ItemType.Tile_Wide);

			comboFormType.DropDownStyle = ComboBoxStyle.DropDownList;
			comboFormType.SelectedItem = ItemType.Tile_Standard;

			if (isFromDocOutline)
				return;

			comboFormType.Items.Add(ItemType.Folder);
			comboFormType.Items.Add(ItemType.Panel);
			comboFormType.Items.Add(ItemType.View);
			comboFormType.Items.Add(ItemType.Frame);

			//comboFormType.Items.Add(ItemType.Tab);
			//comboFormType.Items.Add(ItemType.Dialog);
		}


		//--------------------------------------------------------------------------------
		private void PopulateComboHRef()
		{
			List<string> t = ReadFromXml();
			comboBoxHRef.Items.Clear();

			foreach (var item in t)
			{
				AdditemComboOption itemCombo = new AdditemComboOption(item, ItemType.Href);
				comboBoxHRef.Items.Add(itemCombo);
			}
		//	comboBoxHRef.Items.Add(optionOpenFileSystem);
		}

		//--------------------------------------------------------------------------------
		private List<string> ReadFromXml()
		{
			string filePath = BasePathFinder.BasePathFinderInstance.GetStandardPath();
			filePath = Path.Combine(filePath, @"TaskBuilder\Framework\TbGes\JEditorSettings\XMLHRef.xml");
			List<string> paths = new List<string>();
			if(!File.Exists(filePath))
				return paths;
			XmlDocument doc = new XmlDocument();
			doc.Load(filePath);
			XmlElement root = doc.DocumentElement;
			XmlNodeList frameElements = null;

			if (root != null)
				frameElements = root.GetElementsByTagName("Namespace");

			if (frameElements == null) return paths;
			for (int i = 0; i < frameElements.Count; i++)
			{
				string text = frameElements.Item(i).InnerText;
				paths.Add(text);
			}
			return paths;
		}

		//--------------------------------------------------------------------------------
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);
			if (DialogResult == System.Windows.Forms.DialogResult.OK)
			{
				if (txtFormName.Text.Length == 0)
				{
					e.Cancel = true;
				}
				if (HasInvalidChars())
				{
					e.Cancel = true;
					MessageBox.Show(this, Resources.InvalidChars, Resources.EasyBuilder, MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
				if (AlreadyExisting())
				{
					e.Cancel = true;
					MessageBox.Show(this, Resources.FormAlreadyExisting, Resources.EasyBuilder, MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		//--------------------------------------------------------------------------------
		private bool HasInvalidChars()
		{
			List<char> invalid = new List<char>();
			invalid.AddRange(Path.GetInvalidFileNameChars());
			foreach (var ch in txtFormName.Text)
			{
				if (invalid.Contains(ch))
					return true;
			}
			return false;
		}

		//--------------------------------------------------------------------------------
		private bool AlreadyExisting()
		{
			return File.Exists(txtFilePath.Text);
		}

		//--------------------------------------------------------------------------------
		private void txtFormName_TextChanged(object sender, System.EventArgs e)
		{
			namePathAddItem(FormType, false);
		}

		//--------------------------------------------------------------------------------
		public void namePathAddItem(ItemType it = ItemType.Tile_Standard, bool OnLoad = true)
		{

			bool isPanelOrTile = it == ItemType.Panel || it == ItemType.Tile_Standard;
			bool isView = it == ItemType.View;
			bool isFrame = it == ItemType.Frame;
			string startNamePanel = "IDD_";
			string startNameFolder = "UI";

			string extens = it != ItemType.Folder ? NameSolverStrings.TbjsonExtension : string.Empty;
			if (OnLoad)
			{
				if (String.IsNullOrEmpty(txtFormName.Text))
					txtFormName.Text = isPanelOrTile ? startNamePanel : startNameFolder;
				else if (txtFormName.Text.StartsWith(startNamePanel) && !isPanelOrTile && !isView && !isFrame)
					txtFormName.Text = txtFormName.Text.Replace(startNamePanel, startNameFolder);
				else if (txtFormName.Text.StartsWith(startNameFolder) && (isPanelOrTile || isView || isFrame))
					txtFormName.Text = txtFormName.Text.Replace(startNameFolder, startNamePanel);
			}

			txtFilePath.Text = Path.Combine(folder, txtFormName.Text + extens);
		}

		//--------------------------------------------------------------------------------
		private void AddItem_Load(object sender, EventArgs e)
		{
			namePathAddItem(FormType, true);
		}

		//--------------------------------------------------------------------------------
		private void comboFormType_SelectedIndexChanged(object sender, EventArgs e)
		{
			switch (FormType)
			{
				case ItemType.Folder:
				case ItemType.View:
				case ItemType.Frame:
				case ItemType.Panel:
					namePathAddItem(FormType);
					break;
				default:
					namePathAddItem();
					break;
			}
		}

		//--------------------------------------------------------------------------------
		private void radioButtonNew_CheckedChanged(object sender, EventArgs e)
		{
			this.comboFormType.Enabled = this.radioButton1.Checked;
		}

		//--------------------------------------------------------------------------------
		private void radioButtonHRef_CheckedChanged(object sender, EventArgs e)
		{
			this.comboBoxHRef.Enabled = this.radioButton2.Checked;
			if (this.comboBoxHRef.Enabled)
			{
				this.comboFormType.SelectedItem = ItemType.Frame;
				PopulateComboHRef();
			}

		}

		//--------------------------------------------------------------------------------
		private void comboBoxHRef_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (comboBoxHRef.SelectedItem.ToString() == optionOpenFileSystem)
			{
				OpenFileDialog dialog = new OpenFileDialog() { InitialDirectory = Path.GetDirectoryName(this.JsonFile), Filter = "TbJson Files (.tbjson)|*.tbjson" };

				if (dialog.ShowDialog() != DialogResult.OK)
					return;

				string newNodeName = Path.GetFileNameWithoutExtension(dialog.FileName);
				var ns = BasePathFinder.BasePathFinderInstance.GetNamespaceFromPath(dialog.FileName).ToString();
				PathHref = new AdditemComboOption(dialog.FileName, ns, newNodeName, ItemType.Href);
				namePathAddItem(ItemType.Frame);
				return;
			}
			namePathAddItem(ItemType.Frame);
			if(comboBoxHRef.SelectedItem != null)
				PathHref = ((AdditemComboOption)comboBoxHRef.SelectedItem);
		}

		//-----------------------------------------------------------------------------
		private void btnAdd_Click(object sender, EventArgs e)
		{
			if (comboBoxHRef.Enabled && comboBoxHRef.SelectedItem == null) 
			{			
				MessageBox.Show(String.Format(Resources.InvalidChoice, Resources.NoFrameChosen));
				DialogResult = DialogResult.None;
				return;
			}
		}
	}


	//=========================================================================
	/// <remarks/>
	public class AdditemComboOption
	{
		/// <remarks/>
		//-----------------------------------------------------------------------------
		public string FullPath { get; set; }
		/// <remarks/>
		//-----------------------------------------------------------------------------
		public string Namespace { get; set; }
		/// <remarks/>
		//-----------------------------------------------------------------------------
		public string Text { get; set; }
		/// <remarks/>
		//-----------------------------------------------------------------------------
		internal AddItem.ItemType ItemType { get; set; }
		/// <remarks/>
		//-----------------------------------------------------------------------------
		internal AdditemComboOption(string fullPath, string Namespace, string Name, AddItem.ItemType ItemType)
		{
			this.FullPath = fullPath;
			this.Namespace = Namespace;
			this.Text = Name;
			this.ItemType = ItemType;
		}

		/// <remarks/>
		//-----------------------------------------------------------------------------
		internal AdditemComboOption(string Namespace, AddItem.ItemType ItemType)
		{
			this.FullPath = StaticFunctions.GetFileFromJsonFileId(Namespace);
			this.Namespace = Namespace;
			this.Text = Namespace.Split('.').Last();
			this.ItemType = ItemType;
		}

		/// <remarks/>
		//-----------------------------------------------------------------------------
		public override string ToString()
		{
			return Text;
		}
	}
	
}
