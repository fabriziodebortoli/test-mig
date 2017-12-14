using System;
using System.Collections;
using System.Windows.Forms;
using System.Xml;

namespace Microarea.Tools.TBLocalizer.Forms
{
	/// <summary>
	/// Summary description for Glossary.
	/// </summary>
	public class Glossary : System.Windows.Forms.Form
	{
		private System.Windows.Forms.ColumnHeader ColumnBase;
		private System.Windows.Forms.ColumnHeader ColumnTarget;
		private System.Windows.Forms.ListView ListGlossary;
		private System.Windows.Forms.ContextMenu ContextMenuList;
		private System.Windows.Forms.MenuItem MiAdd;
		private System.Windows.Forms.MenuItem MiDelete;
		private System.Windows.Forms.MenuItem MiModify;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Button BtnCancel;
		private System.Windows.Forms.Button BtnSave;
		private System.Windows.Forms.Label LblInfo;
		private System.ComponentModel.Container components = null;
		public GlossaryInfo glossaryInfo = null;
		private string languageCode = null;
		private System.Windows.Forms.ColumnHeader ColumnSupport;
		private System.Windows.Forms.GroupBox GBView;
		private System.Windows.Forms.RadioButton RBBase;
		private System.Windows.Forms.RadioButton RBBoth;
		private System.Windows.Forms.RadioButton RBSupport;
		private SupportInfo Supportinfo;
		private bool support = false;

		private int targetindex {get { return support ? 2: 1 ;}}
		//---------------------------------------------------------------------
		public Glossary(string language, SupportInfo supportinfo)
		{
			InitializeComponent();
			languageCode = language;
			Supportinfo = supportinfo;
			PostInitialize();
		}

		//---------------------------------------------------------------------
		public void PostInitialize()
		{
			support = !Supportinfo.IsNull();
			try
			{
				if (!support)
				{
					ListGlossary.Columns.RemoveAt(1);
					//a questo punto la target diventa 1
				}

				if (Supportinfo.SupportView)
				{
					ListGlossary.Columns[0].Width = 0;
					ListGlossary.Columns[1].Width = 350;
					RBSupport.Checked = true;
					RBBoth.Checked = RBBase.Checked = false;
				}
				else if (support)
				{
					ListGlossary.Columns[1].Width = 0;
					ListGlossary.Columns[0].Width = 350;
					RBBase.Checked = true;
					RBBoth.Checked = RBSupport.Checked = false;
				}
				else
				{
					GBView.Visible = false;
				}

					glossaryInfo = GlossaryFunctions.LoadGlossary(languageCode);
				if (glossaryInfo == null)
				{
					LblInfo.Text = String.Format(Strings.ErrorLoadingGlossary,languageCode);
					return;
				}
				XmlNodeList list = glossaryInfo.GlossaryDoc.DocumentElement.ChildNodes;
				string b = "";
				string t = "";
				string s = "";

				foreach (XmlElement n in list)
				{
					b = n.GetAttribute(AllStrings.baseTag);
					t = n.GetAttribute(AllStrings.target);
					if (support)
					{
						s = n.GetAttribute(Supportinfo.SupportLanguage);
						ListGlossary.Items.Add(new ListViewItem(new string[]{b, s, t}));
					}
					else
						ListGlossary.Items.Add(new ListViewItem(new string[]{b, t}));
				}
				string message = String.Format(Strings.GlossaryId, languageCode);
				if (support)
					message = message +" - "+ String.Format(Strings.GlossarySupport, Supportinfo.SupportLanguage);
				LblInfo.Text =  message;
			}
			catch (Exception exc)
			{
				MessageBox.Show(this, String.Format(Strings.Error, exc.Message), Strings.Glossary);
				return;
			}
		}

		//---------------------------------------------------------------------
		private void MiAdd_Click(object sender, System.EventArgs e)
		{
			try
			{
				GlossaryEntry ge = new GlossaryEntry(support);
				if (ge.ShowDialog() == DialogResult.Cancel)
					return;
				string b = ge.Base;
				string t = ge.Target;
				string s = null;
				ListViewItem item = null;
				if (support)
				{
					s = ge.Support;
					item = new ListViewItem(new string[]{b.ToLower(), s, t});
				}
				else
					item = new ListViewItem(new string[]{b.ToLower(), t});
				ListGlossary.Items.Add(item);
				ListGlossary.Items[item.Index].EnsureVisible();
				ListGlossary.Items[item.Index].Selected = true;
				GlossaryFunctions.AddGlossaryEntry(b, t, languageCode, s, Supportinfo.SupportLanguage, false);
			}
		
			catch (Exception exc)
			{
				MessageBox.Show(this, String.Format(Strings.Error, exc.Message), Strings.Glossary);
				return;
			}
		}

		//---------------------------------------------------------------------
		private void MiDelete_Click(object sender, System.EventArgs e)
		{
			try
			{
				ListView.SelectedListViewItemCollection coll = ListGlossary.SelectedItems;
				int index = 0;

				foreach (ListViewItem item in coll)
				{
					index = item.Index;
					ListGlossary.Items.RemoveAt(item.Index);
					GlossaryFunctions.RemoveGlossaryEntryTemp(item.SubItems[0].Text, item.SubItems[targetindex].Text, languageCode);
				
				}
				if (index < ListGlossary.Items.Count)
				{
					ListGlossary.Items[index].EnsureVisible();
					ListGlossary.Items[index].Selected = true;
				}
			}
		
			catch (Exception exc)
			{
				MessageBox.Show(this, String.Format(Strings.Error, exc.Message), Strings.Glossary);
				return;
			}
		}

		//---------------------------------------------------------------------
		private void MiModify_Click(object sender, System.EventArgs e)
		{
			try
			{
				ListView.SelectedListViewItemCollection coll = ListGlossary.SelectedItems;
				if (coll == null || coll.Count < 1) return;
			
				string b = ((ListViewItem)coll[0]).SubItems[0].Text;
				string t = ((ListViewItem)coll[0]).SubItems[targetindex].Text;
				string s = null;
				if (support)
					s = ((ListViewItem)coll[0]).SubItems[1].Text;
				GlossaryEntry ge = new GlossaryEntry(b, t, s, support);
				if (ge.ShowDialog() == DialogResult.Cancel)
					return;

				ListGlossary.Items.RemoveAt(((ListViewItem)coll[0]).Index);
				ListViewItem item = null;
				if (support)
					item = new ListViewItem(new string[]{ge.Base.ToLower(),ge.Support, ge.Target});
				else
					item = new ListViewItem(new string[]{ge.Base.ToLower(), ge.Target});
				ListGlossary.Items.Add(item);
				ListGlossary.Items[item.Index].EnsureVisible();
				ListGlossary.Items[item.Index].Selected = true;
				GlossaryFunctions.RemoveGlossaryEntryTemp(b, t, languageCode);
				GlossaryFunctions.AddGlossaryEntry(ge.Base, ge.Target, languageCode, ge.Support, Supportinfo.SupportLanguage, false);
			}
		
			catch (Exception exc)
			{
				MessageBox.Show(this, String.Format(Strings.Error, exc.Message), Strings.Glossary);
				return;
			}
		}

		private void BtnSave_Click(object sender, System.EventArgs e)
		{
			GlossaryFunctions.SaveGlossary(glossaryInfo, languageCode);
			Close();
		}

		private void BtnCancel_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		//---------------------------------------------------------------------
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(Glossary));
			this.ListGlossary = new System.Windows.Forms.ListView();
			this.ColumnBase = new System.Windows.Forms.ColumnHeader();
			this.ColumnSupport = new System.Windows.Forms.ColumnHeader();
			this.ColumnTarget = new System.Windows.Forms.ColumnHeader();
			this.ContextMenuList = new System.Windows.Forms.ContextMenu();
			this.MiAdd = new System.Windows.Forms.MenuItem();
			this.MiDelete = new System.Windows.Forms.MenuItem();
			this.MiModify = new System.Windows.Forms.MenuItem();
			this.panel1 = new System.Windows.Forms.Panel();
			this.GBView = new System.Windows.Forms.GroupBox();
			this.RBBase = new System.Windows.Forms.RadioButton();
			this.RBBoth = new System.Windows.Forms.RadioButton();
			this.RBSupport = new System.Windows.Forms.RadioButton();
			this.LblInfo = new System.Windows.Forms.Label();
			this.BtnCancel = new System.Windows.Forms.Button();
			this.BtnSave = new System.Windows.Forms.Button();
			this.panel1.SuspendLayout();
			this.GBView.SuspendLayout();
			this.SuspendLayout();
			// 
			// ListGlossary
			// 
			this.ListGlossary.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																						   this.ColumnBase,
																						   this.ColumnSupport,
																						   this.ColumnTarget});
			this.ListGlossary.ContextMenu = this.ContextMenuList;
			this.ListGlossary.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ListGlossary.FullRowSelect = true;
			this.ListGlossary.GridLines = true;
			this.ListGlossary.LabelEdit = true;
			this.ListGlossary.Location = new System.Drawing.Point(0, 0);
			this.ListGlossary.MultiSelect = false;
			this.ListGlossary.Name = "ListGlossary";
			this.ListGlossary.Size = new System.Drawing.Size(920, 318);
			this.ListGlossary.Sorting = System.Windows.Forms.SortOrder.Ascending;
			this.ListGlossary.TabIndex = 0;
			this.ListGlossary.View = System.Windows.Forms.View.Details;
			this.ListGlossary.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.ListGlossary_ColumnClick);
			// 
			// ColumnBase
			// 
			this.ColumnBase.Text = "Base";
			this.ColumnBase.Width = 350;
			// 
			// ColumnSupport
			// 
			this.ColumnSupport.Text = "Support";
			this.ColumnSupport.Width = 350;
			// 
			// ColumnTarget
			// 
			this.ColumnTarget.Text = "Target";
			this.ColumnTarget.Width = 350;
			// 
			// ContextMenuList
			// 
			this.ContextMenuList.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																							this.MiAdd,
																							this.MiDelete,
																							this.MiModify});
			// 
			// MiAdd
			// 
			this.MiAdd.Index = 0;
			this.MiAdd.Text = "Add new...";
			this.MiAdd.Click += new System.EventHandler(this.MiAdd_Click);
			// 
			// MiDelete
			// 
			this.MiDelete.Index = 1;
			this.MiDelete.Text = "Delete";
			this.MiDelete.Click += new System.EventHandler(this.MiDelete_Click);
			// 
			// MiModify
			// 
			this.MiModify.Index = 2;
			this.MiModify.Text = "Modify...";
			this.MiModify.Click += new System.EventHandler(this.MiModify_Click);
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.GBView);
			this.panel1.Controls.Add(this.LblInfo);
			this.panel1.Controls.Add(this.BtnCancel);
			this.panel1.Controls.Add(this.BtnSave);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel1.Location = new System.Drawing.Point(0, 318);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(920, 80);
			this.panel1.TabIndex = 1;
			// 
			// GBView
			// 
			this.GBView.Controls.Add(this.RBBase);
			this.GBView.Controls.Add(this.RBBoth);
			this.GBView.Controls.Add(this.RBSupport);
			this.GBView.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.GBView.Location = new System.Drawing.Point(8, 24);
			this.GBView.Name = "GBView";
			this.GBView.Size = new System.Drawing.Size(384, 48);
			this.GBView.TabIndex = 3;
			this.GBView.TabStop = false;
			this.GBView.Text = "View";
			// 
			// RBBase
			// 
			this.RBBase.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RBBase.Location = new System.Drawing.Point(8, 16);
			this.RBBase.Name = "RBBase";
			this.RBBase.TabIndex = 2;
			this.RBBase.Text = "Base";
			this.RBBase.CheckedChanged += new System.EventHandler(this.radioButton_CheckedChanged);
			// 
			// RBBoth
			// 
			this.RBBoth.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RBBoth.Location = new System.Drawing.Point(272, 16);
			this.RBBoth.Name = "RBBoth";
			this.RBBoth.TabIndex = 1;
			this.RBBoth.Text = "Both";
			this.RBBoth.CheckedChanged += new System.EventHandler(this.radioButton_CheckedChanged);
			// 
			// RBSupport
			// 
			this.RBSupport.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RBSupport.Location = new System.Drawing.Point(140, 16);
			this.RBSupport.Name = "RBSupport";
			this.RBSupport.TabIndex = 0;
			this.RBSupport.Text = "Support";
			this.RBSupport.CheckedChanged += new System.EventHandler(this.radioButton_CheckedChanged);
			// 
			// LblInfo
			// 
			this.LblInfo.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LblInfo.Location = new System.Drawing.Point(8, 8);
			this.LblInfo.Name = "LblInfo";
			this.LblInfo.Size = new System.Drawing.Size(496, 16);
			this.LblInfo.TabIndex = 2;
			// 
			// BtnCancel
			// 
			this.BtnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.BtnCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.BtnCancel.Location = new System.Drawing.Point(832, 32);
			this.BtnCancel.Name = "BtnCancel";
			this.BtnCancel.TabIndex = 1;
			this.BtnCancel.Text = "Cancel";
			this.BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
			// 
			// BtnSave
			// 
			this.BtnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.BtnSave.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.BtnSave.Location = new System.Drawing.Point(744, 32);
			this.BtnSave.Name = "BtnSave";
			this.BtnSave.TabIndex = 0;
			this.BtnSave.Text = "Save";
			this.BtnSave.Click += new System.EventHandler(this.BtnSave_Click);
			// 
			// Glossary
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(7, 16);
			this.ClientSize = new System.Drawing.Size(920, 398);
			this.Controls.Add(this.ListGlossary);
			this.Controls.Add(this.panel1);
			this.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "Glossary";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Glossary";
			this.panel1.ResumeLayout(false);
			this.GBView.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		//---------------------------------------------------------------------
		private void radioButton_CheckedChanged(object sender, System.EventArgs e)
		{
			if (!support) return;
			if (sender == RBBase)
			{
				ListGlossary.Columns[0].Width = 350;
				ListGlossary.Columns[1].Width = 0;
			}
			if (sender == RBSupport)
			{
				ListGlossary.Columns[1].Width = 350;
				ListGlossary.Columns[0].Width = 0;
			}
			if (sender == RBBoth)
			{
				ListGlossary.Columns[1].Width = 350;
				ListGlossary.Columns[0].Width = 350;
			}
		}

		//---------------------------------------------------------------------
		private void ListGlossary_ColumnClick(object sender, System.Windows.Forms.ColumnClickEventArgs e)
		{
			this.ListGlossary.ListViewItemSorter = new ListViewItemComparer(e.Column);

		}
	}

	/// <summary>
	/// Implements the manual sorting of items by columns.
	/// </summary>
	//=========================================================================
	class ListViewItemComparer : IComparer
	{
		private int col;
		public ListViewItemComparer()
		{
			col = 0;
		}
		public ListViewItemComparer(int column)
		{
			col = column;
		}
		public int Compare(object x, object y)
		{
			return String.Compare(((ListViewItem)x).SubItems[col].Text, ((ListViewItem)y).SubItems[col].Text);
		}
	}
}
