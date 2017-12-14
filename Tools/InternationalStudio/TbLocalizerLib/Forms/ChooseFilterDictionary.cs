using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Forms;
//
using Microarea.Tools.TBLocalizer.CommonUtilities;

namespace Microarea.Tools.TBLocalizer.Forms
{
	/// <summary>
	/// Form to choose the dictionaries to hide.
	/// </summary>
	//=========================================================================
	public class ChooseFilterDictionary : Form
	{
		#region Controls
		private Label LblTitle;
		private Button BtnOk;
		private Button BtnCancel;
		private CheckedListBox LstDictionaries;

		private Container	components = null;
		#endregion

		#region Private members
		/// <remarks>
		/// TODO$: valutare il caso di modificare le variabili
		/// da ArrayList a CaseInsensitiveStringCollection
		/// </remarks>
		private ArrayList choosedDictionaries = new ArrayList();
		private ArrayList commonDictionaries;
		#endregion

		#region Properties
		/// <remarks>
		/// TODO$: valutare il caso di modificare le variabili
		/// da ArrayList a CaseInsensitiveStringCollection
		/// </remarks>
		internal ArrayList ChoosedDictionaries
		{
			get { return choosedDictionaries; }
			set { choosedDictionaries = value; }
		}
		#endregion

		#region Constructors
		//---------------------------------------------------------------------
		public ChooseFilterDictionary(ArrayList commonDictionaries)
		{
			this.commonDictionaries = commonDictionaries;
			InitializeComponent();
		}
		#endregion

		#region Private methods
		//---------------------------------------------------------------------
		private void BtnOk_Click(object sender, System.EventArgs e)
		{
			choosedDictionaries = new ArrayList();

			/// <remarks>
			/// TODO$: valutare il caso di modificare le variabili
			/// da ArrayList a CaseInsensitiveStringCollection.
			/// Nel caso, potrebbe sparire questo orribile ToLower().
			/// </remarks>
			foreach (CultureInfo info in LstDictionaries.CheckedItems)
				choosedDictionaries.Add(info.Name.ToLower());

			DialogResult = DialogResult.OK;
			Close();
		}

		//---------------------------------------------------------------------
		private void ChooseFilterDictionary_Load(object sender, System.EventArgs e)
		{
			if (commonDictionaries.Count == 0)
				return;

			ArrayList dictionaryList = new ArrayList();

			foreach (string shortLang in commonDictionaries)
			{
				if (string.Compare(shortLang, LocalizerTreeNode.BaseLanguage, true, CultureInfo.InvariantCulture) == 0)
					continue;

				try
				{
					dictionaryList.Add(new CultureInfo(shortLang));
				}
				catch (ArgumentException)
				{
					Debug.WriteLine(shortLang + " is not a valid culture!");
				}
			}

			LstDictionaries.Sorted			= true;
			LstDictionaries.DisplayMember	= CultureInfoComboBox.CultureInfoDescription;
			LstDictionaries.ValueMember		= CultureInfoComboBox.CultureInfoCode;
			LstDictionaries.DataSource		= dictionaryList;

			for (int i = 0; i < LstDictionaries.Items.Count; i++)
			{
				/// <remarks>
				/// TODO$: valutare il caso di modificare le variabili
				/// da ArrayList a CaseInsensitiveStringCollection.
				/// Nel caso, potrebbe sparire questo orribile ToLower().
				/// </remarks>
				string cultName = ((CultureInfo)LstDictionaries.Items[i]).Name.ToLower();

				if (choosedDictionaries.Contains(cultName))
					LstDictionaries.SetItemChecked(i, true);				
			}
		}
		#endregion

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		//---------------------------------------------------------------------
		protected override void Dispose( bool disposing )
		{
			if ( disposing )
			{
				if (components != null)
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(ChooseFilterDictionary));
			this.LblTitle = new System.Windows.Forms.Label();
			this.BtnOk = new System.Windows.Forms.Button();
			this.BtnCancel = new System.Windows.Forms.Button();
			this.LstDictionaries = new System.Windows.Forms.CheckedListBox();
			this.SuspendLayout();
			// 
			// LblTitle
			// 
			this.LblTitle.AccessibleDescription = resources.GetString("LblTitle.AccessibleDescription");
			this.LblTitle.AccessibleName = resources.GetString("LblTitle.AccessibleName");
			this.LblTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("LblTitle.Anchor")));
			this.LblTitle.AutoSize = ((bool)(resources.GetObject("LblTitle.AutoSize")));
			this.LblTitle.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("LblTitle.Dock")));
			this.LblTitle.Enabled = ((bool)(resources.GetObject("LblTitle.Enabled")));
			this.LblTitle.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LblTitle.Font = ((System.Drawing.Font)(resources.GetObject("LblTitle.Font")));
			this.LblTitle.Image = ((System.Drawing.Image)(resources.GetObject("LblTitle.Image")));
			this.LblTitle.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblTitle.ImageAlign")));
			this.LblTitle.ImageIndex = ((int)(resources.GetObject("LblTitle.ImageIndex")));
			this.LblTitle.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("LblTitle.ImeMode")));
			this.LblTitle.Location = ((System.Drawing.Point)(resources.GetObject("LblTitle.Location")));
			this.LblTitle.Name = "LblTitle";
			this.LblTitle.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("LblTitle.RightToLeft")));
			this.LblTitle.Size = ((System.Drawing.Size)(resources.GetObject("LblTitle.Size")));
			this.LblTitle.TabIndex = ((int)(resources.GetObject("LblTitle.TabIndex")));
			this.LblTitle.Text = resources.GetString("LblTitle.Text");
			this.LblTitle.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("LblTitle.TextAlign")));
			this.LblTitle.Visible = ((bool)(resources.GetObject("LblTitle.Visible")));
			// 
			// BtnOk
			// 
			this.BtnOk.AccessibleDescription = resources.GetString("BtnOk.AccessibleDescription");
			this.BtnOk.AccessibleName = resources.GetString("BtnOk.AccessibleName");
			this.BtnOk.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("BtnOk.Anchor")));
			this.BtnOk.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BtnOk.BackgroundImage")));
			this.BtnOk.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("BtnOk.Dock")));
			this.BtnOk.Enabled = ((bool)(resources.GetObject("BtnOk.Enabled")));
			this.BtnOk.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("BtnOk.FlatStyle")));
			this.BtnOk.Font = ((System.Drawing.Font)(resources.GetObject("BtnOk.Font")));
			this.BtnOk.Image = ((System.Drawing.Image)(resources.GetObject("BtnOk.Image")));
			this.BtnOk.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnOk.ImageAlign")));
			this.BtnOk.ImageIndex = ((int)(resources.GetObject("BtnOk.ImageIndex")));
			this.BtnOk.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("BtnOk.ImeMode")));
			this.BtnOk.Location = ((System.Drawing.Point)(resources.GetObject("BtnOk.Location")));
			this.BtnOk.Name = "BtnOk";
			this.BtnOk.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("BtnOk.RightToLeft")));
			this.BtnOk.Size = ((System.Drawing.Size)(resources.GetObject("BtnOk.Size")));
			this.BtnOk.TabIndex = ((int)(resources.GetObject("BtnOk.TabIndex")));
			this.BtnOk.Text = resources.GetString("BtnOk.Text");
			this.BtnOk.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnOk.TextAlign")));
			this.BtnOk.Visible = ((bool)(resources.GetObject("BtnOk.Visible")));
			this.BtnOk.Click += new System.EventHandler(this.BtnOk_Click);
			// 
			// BtnCancel
			// 
			this.BtnCancel.AccessibleDescription = resources.GetString("BtnCancel.AccessibleDescription");
			this.BtnCancel.AccessibleName = resources.GetString("BtnCancel.AccessibleName");
			this.BtnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("BtnCancel.Anchor")));
			this.BtnCancel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BtnCancel.BackgroundImage")));
			this.BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.BtnCancel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("BtnCancel.Dock")));
			this.BtnCancel.Enabled = ((bool)(resources.GetObject("BtnCancel.Enabled")));
			this.BtnCancel.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("BtnCancel.FlatStyle")));
			this.BtnCancel.Font = ((System.Drawing.Font)(resources.GetObject("BtnCancel.Font")));
			this.BtnCancel.Image = ((System.Drawing.Image)(resources.GetObject("BtnCancel.Image")));
			this.BtnCancel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnCancel.ImageAlign")));
			this.BtnCancel.ImageIndex = ((int)(resources.GetObject("BtnCancel.ImageIndex")));
			this.BtnCancel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("BtnCancel.ImeMode")));
			this.BtnCancel.Location = ((System.Drawing.Point)(resources.GetObject("BtnCancel.Location")));
			this.BtnCancel.Name = "BtnCancel";
			this.BtnCancel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("BtnCancel.RightToLeft")));
			this.BtnCancel.Size = ((System.Drawing.Size)(resources.GetObject("BtnCancel.Size")));
			this.BtnCancel.TabIndex = ((int)(resources.GetObject("BtnCancel.TabIndex")));
			this.BtnCancel.Text = resources.GetString("BtnCancel.Text");
			this.BtnCancel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnCancel.TextAlign")));
			this.BtnCancel.Visible = ((bool)(resources.GetObject("BtnCancel.Visible")));
			// 
			// LstDictionaries
			// 
			this.LstDictionaries.AccessibleDescription = resources.GetString("LstDictionaries.AccessibleDescription");
			this.LstDictionaries.AccessibleName = resources.GetString("LstDictionaries.AccessibleName");
			this.LstDictionaries.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("LstDictionaries.Anchor")));
			this.LstDictionaries.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("LstDictionaries.BackgroundImage")));
			this.LstDictionaries.CheckOnClick = true;
			this.LstDictionaries.ColumnWidth = ((int)(resources.GetObject("LstDictionaries.ColumnWidth")));
			this.LstDictionaries.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("LstDictionaries.Dock")));
			this.LstDictionaries.Enabled = ((bool)(resources.GetObject("LstDictionaries.Enabled")));
			this.LstDictionaries.Font = ((System.Drawing.Font)(resources.GetObject("LstDictionaries.Font")));
			this.LstDictionaries.HorizontalExtent = ((int)(resources.GetObject("LstDictionaries.HorizontalExtent")));
			this.LstDictionaries.HorizontalScrollbar = ((bool)(resources.GetObject("LstDictionaries.HorizontalScrollbar")));
			this.LstDictionaries.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("LstDictionaries.ImeMode")));
			this.LstDictionaries.IntegralHeight = ((bool)(resources.GetObject("LstDictionaries.IntegralHeight")));
			this.LstDictionaries.Location = ((System.Drawing.Point)(resources.GetObject("LstDictionaries.Location")));
			this.LstDictionaries.Name = "LstDictionaries";
			this.LstDictionaries.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("LstDictionaries.RightToLeft")));
			this.LstDictionaries.ScrollAlwaysVisible = ((bool)(resources.GetObject("LstDictionaries.ScrollAlwaysVisible")));
			this.LstDictionaries.Size = ((System.Drawing.Size)(resources.GetObject("LstDictionaries.Size")));
			this.LstDictionaries.TabIndex = ((int)(resources.GetObject("LstDictionaries.TabIndex")));
			this.LstDictionaries.Visible = ((bool)(resources.GetObject("LstDictionaries.Visible")));
			// 
			// ChooseFilterDictionary
			// 
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.Controls.Add(this.LstDictionaries);
			this.Controls.Add(this.BtnCancel);
			this.Controls.Add(this.BtnOk);
			this.Controls.Add(this.LblTitle);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "ChooseFilterDictionary";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.ShowInTaskbar = false;
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.Load += new System.EventHandler(this.ChooseFilterDictionary_Load);
			this.ResumeLayout(false);

		}
		#endregion
	}
}
