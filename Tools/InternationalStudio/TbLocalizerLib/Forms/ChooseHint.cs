using System;
using System.Collections;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections.Generic;

namespace Microarea.Tools.TBLocalizer.Forms
{
	//=========================================================================
	public class ChooseHint : System.Windows.Forms.Form
	{
		private System.ComponentModel.IContainer components = null;
		private Button			BtnInsert;
		private Button			BtnCancel;
		private Button			BtnStop;
		private Label			LblTitle;
		private CheckBox		CkbApplyThis;
		private bool			singleSelection;
		private bool			applyThis;
		private bool			showStop;
		private System.Windows.Forms.DataGrid dataGrid;
		private System.Data.DataView dataView;
		private System.Windows.Forms.DataGridTableStyle dataGridTableStyle;
		private Microarea.Tools.TBLocalizer.Hint hint;
		private Microarea.Tools.TBLocalizer.CommonUtilities.AutoSizeTextColumnStyle dataGridTextBoxColumnRating;
		private Microarea.Tools.TBLocalizer.CommonUtilities.AutoSizeTextColumnStyle dataGridTextBoxColumnTarget;
		private Microarea.Tools.TBLocalizer.CommonUtilities.AutoSizeTextColumnStyle dataGridTextBoxColumnBase;
		private System.Windows.Forms.DataGridBoolColumn dataGridBoolColumnSelected;
		private string			hintAccepted = String.Empty;

		//--------------------------------------------------------------------------------
		public string	HintAccepted	{ get {return hintAccepted;} set {hintAccepted = value;}}
		//--------------------------------------------------------------------------------
		public bool		ApplyThis		{ get {return applyThis;}	 set {applyThis = value;}}
		//--------------------------------------------------------------------------------
		public bool		ShowStop		{ get {return showStop;}	 set {showStop = value;}}


		//---------------------------------------------------------------------
		public ChooseHint(HintItem[] hintList, bool singleSelection, string baseString)
		{
			InitializeComponent();
			this.singleSelection = singleSelection;
			bool ratingHidden = false, baseHidden = false;
			foreach (HintItem h in hintList)
			{
				Hint.HintTableRow row = (Hint.HintTableRow) hint.HintTable.NewRow();
				row.BaseString = h.BaseLanguageString;
				row.TargetString = h.HintString;
				row.Selected = false;
				row.Rating = Math.Round(h.Rating, 2);
				if (h.Rating < 0 && !ratingHidden)
				{
					dataGridTableStyle.GridColumnStyles.Remove(dataGridTextBoxColumnRating);
					ratingHidden = true;
				}
				if (h.BaseLanguageString.Length == 0 && !baseHidden)
				{
					dataGridTableStyle.GridColumnStyles.Remove(dataGridTextBoxColumnBase);
					baseHidden = true;
				}

				hint.HintTable.Rows.Add(row);
			}

			LblTitle.Text = (singleSelection) ? String.Format(Strings.SingleHintChoice, baseString) : String.Format(Strings.MultipleHintsChoice, baseString);
		}

		//---------------------------------------------------------------------
		public ChooseHint(HintItem[] hintList, bool singleSelection, string baseString/*, bool showApplyThis*/, bool showStopButton)
			:
			this (hintList, singleSelection, baseString)
		{
			CkbApplyThis.Visible = BtnStop.Visible = showStopButton;
			ApplyThis			 = CkbApplyThis.Checked;
		}

		//---------------------------------------------------------------------
		Hint.HintTableRow[] CheckedItems
		{
			get
			{
				ArrayList list = new ArrayList();
				foreach (Hint.HintTableRow row in dataView.Table.Rows)
				{
					if (row.Selected)
						list.Add(row);
				}

				return list.ToArray(typeof(Hint.HintTableRow)) as Hint.HintTableRow[];
			}
		}

		//---------------------------------------------------------------------
		private void BtnInsert_Click(object sender, System.EventArgs e)
		{
			Hint.HintTableRow[] checkedItems = CheckedItems;
			
			if (checkedItems.Length == 0) return;
			if (singleSelection && checkedItems.Length > 1)
				return;

			StringBuilder sb = new StringBuilder();

			foreach (Hint.HintTableRow row in checkedItems)
			{
				if (sb.Length > 0)
					sb.Append(' '); 

				sb.Append(row.TargetString); 
					
			}
			
			HintAccepted = sb.ToString();
			DialogResult = DialogResult.OK;
			Close();
		}

		//---------------------------------------------------------------------
		private void BtnCancel_Click(object sender, System.EventArgs e)
		{
			DialogResult = DialogResult.Cancel;

			Close();
		}

		//---------------------------------------------------------------------
		private void BtnStop_Click(object sender, System.EventArgs e)
		{
			DialogResult = DialogResult.Abort;
			Close();
		}

		//---------------------------------------------------------------------
		private void CkbApplyFirst_CheckedChanged(object sender, System.EventArgs e)
		{
			ApplyThis = ((CheckBox)sender).Checked;
		}
		
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
		//--------------------------------------------------------------------------------
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(ChooseHint));
			this.LblTitle = new System.Windows.Forms.Label();
			this.BtnInsert = new System.Windows.Forms.Button();
			this.BtnCancel = new System.Windows.Forms.Button();
			this.CkbApplyThis = new System.Windows.Forms.CheckBox();
			this.BtnStop = new System.Windows.Forms.Button();
			this.dataGrid = new System.Windows.Forms.DataGrid();
			this.dataView = new System.Data.DataView();
			this.hint = new Microarea.Tools.TBLocalizer.Hint();
			this.dataGridTableStyle = new System.Windows.Forms.DataGridTableStyle();
			this.dataGridBoolColumnSelected = new System.Windows.Forms.DataGridBoolColumn();
			this.dataGridTextBoxColumnRating = new Microarea.Tools.TBLocalizer.CommonUtilities.AutoSizeTextColumnStyle();
			this.dataGridTextBoxColumnTarget = new Microarea.Tools.TBLocalizer.CommonUtilities.AutoSizeTextColumnStyle();
			this.dataGridTextBoxColumnBase = new Microarea.Tools.TBLocalizer.CommonUtilities.AutoSizeTextColumnStyle();
			((System.ComponentModel.ISupportInitialize)(this.dataGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dataView)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.hint)).BeginInit();
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
			// BtnInsert
			// 
			this.BtnInsert.AccessibleDescription = resources.GetString("BtnInsert.AccessibleDescription");
			this.BtnInsert.AccessibleName = resources.GetString("BtnInsert.AccessibleName");
			this.BtnInsert.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("BtnInsert.Anchor")));
			this.BtnInsert.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BtnInsert.BackgroundImage")));
			this.BtnInsert.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("BtnInsert.Dock")));
			this.BtnInsert.Enabled = ((bool)(resources.GetObject("BtnInsert.Enabled")));
			this.BtnInsert.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("BtnInsert.FlatStyle")));
			this.BtnInsert.Font = ((System.Drawing.Font)(resources.GetObject("BtnInsert.Font")));
			this.BtnInsert.Image = ((System.Drawing.Image)(resources.GetObject("BtnInsert.Image")));
			this.BtnInsert.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnInsert.ImageAlign")));
			this.BtnInsert.ImageIndex = ((int)(resources.GetObject("BtnInsert.ImageIndex")));
			this.BtnInsert.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("BtnInsert.ImeMode")));
			this.BtnInsert.Location = ((System.Drawing.Point)(resources.GetObject("BtnInsert.Location")));
			this.BtnInsert.Name = "BtnInsert";
			this.BtnInsert.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("BtnInsert.RightToLeft")));
			this.BtnInsert.Size = ((System.Drawing.Size)(resources.GetObject("BtnInsert.Size")));
			this.BtnInsert.TabIndex = ((int)(resources.GetObject("BtnInsert.TabIndex")));
			this.BtnInsert.Text = resources.GetString("BtnInsert.Text");
			this.BtnInsert.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnInsert.TextAlign")));
			this.BtnInsert.Visible = ((bool)(resources.GetObject("BtnInsert.Visible")));
			this.BtnInsert.Click += new System.EventHandler(this.BtnInsert_Click);
			// 
			// BtnCancel
			// 
			this.BtnCancel.AccessibleDescription = resources.GetString("BtnCancel.AccessibleDescription");
			this.BtnCancel.AccessibleName = resources.GetString("BtnCancel.AccessibleName");
			this.BtnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("BtnCancel.Anchor")));
			this.BtnCancel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BtnCancel.BackgroundImage")));
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
			this.BtnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
			// 
			// CkbApplyThis
			// 
			this.CkbApplyThis.AccessibleDescription = resources.GetString("CkbApplyThis.AccessibleDescription");
			this.CkbApplyThis.AccessibleName = resources.GetString("CkbApplyThis.AccessibleName");
			this.CkbApplyThis.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("CkbApplyThis.Anchor")));
			this.CkbApplyThis.Appearance = ((System.Windows.Forms.Appearance)(resources.GetObject("CkbApplyThis.Appearance")));
			this.CkbApplyThis.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("CkbApplyThis.BackgroundImage")));
			this.CkbApplyThis.CheckAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CkbApplyThis.CheckAlign")));
			this.CkbApplyThis.Checked = true;
			this.CkbApplyThis.CheckState = System.Windows.Forms.CheckState.Checked;
			this.CkbApplyThis.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("CkbApplyThis.Dock")));
			this.CkbApplyThis.Enabled = ((bool)(resources.GetObject("CkbApplyThis.Enabled")));
			this.CkbApplyThis.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("CkbApplyThis.FlatStyle")));
			this.CkbApplyThis.Font = ((System.Drawing.Font)(resources.GetObject("CkbApplyThis.Font")));
			this.CkbApplyThis.Image = ((System.Drawing.Image)(resources.GetObject("CkbApplyThis.Image")));
			this.CkbApplyThis.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CkbApplyThis.ImageAlign")));
			this.CkbApplyThis.ImageIndex = ((int)(resources.GetObject("CkbApplyThis.ImageIndex")));
			this.CkbApplyThis.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("CkbApplyThis.ImeMode")));
			this.CkbApplyThis.Location = ((System.Drawing.Point)(resources.GetObject("CkbApplyThis.Location")));
			this.CkbApplyThis.Name = "CkbApplyThis";
			this.CkbApplyThis.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("CkbApplyThis.RightToLeft")));
			this.CkbApplyThis.Size = ((System.Drawing.Size)(resources.GetObject("CkbApplyThis.Size")));
			this.CkbApplyThis.TabIndex = ((int)(resources.GetObject("CkbApplyThis.TabIndex")));
			this.CkbApplyThis.Text = resources.GetString("CkbApplyThis.Text");
			this.CkbApplyThis.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("CkbApplyThis.TextAlign")));
			this.CkbApplyThis.Visible = ((bool)(resources.GetObject("CkbApplyThis.Visible")));
			this.CkbApplyThis.CheckedChanged += new System.EventHandler(this.CkbApplyFirst_CheckedChanged);
			// 
			// BtnStop
			// 
			this.BtnStop.AccessibleDescription = resources.GetString("BtnStop.AccessibleDescription");
			this.BtnStop.AccessibleName = resources.GetString("BtnStop.AccessibleName");
			this.BtnStop.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("BtnStop.Anchor")));
			this.BtnStop.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BtnStop.BackgroundImage")));
			this.BtnStop.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("BtnStop.Dock")));
			this.BtnStop.Enabled = ((bool)(resources.GetObject("BtnStop.Enabled")));
			this.BtnStop.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("BtnStop.FlatStyle")));
			this.BtnStop.Font = ((System.Drawing.Font)(resources.GetObject("BtnStop.Font")));
			this.BtnStop.Image = ((System.Drawing.Image)(resources.GetObject("BtnStop.Image")));
			this.BtnStop.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnStop.ImageAlign")));
			this.BtnStop.ImageIndex = ((int)(resources.GetObject("BtnStop.ImageIndex")));
			this.BtnStop.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("BtnStop.ImeMode")));
			this.BtnStop.Location = ((System.Drawing.Point)(resources.GetObject("BtnStop.Location")));
			this.BtnStop.Name = "BtnStop";
			this.BtnStop.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("BtnStop.RightToLeft")));
			this.BtnStop.Size = ((System.Drawing.Size)(resources.GetObject("BtnStop.Size")));
			this.BtnStop.TabIndex = ((int)(resources.GetObject("BtnStop.TabIndex")));
			this.BtnStop.Text = resources.GetString("BtnStop.Text");
			this.BtnStop.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("BtnStop.TextAlign")));
			this.BtnStop.Visible = ((bool)(resources.GetObject("BtnStop.Visible")));
			this.BtnStop.Click += new System.EventHandler(this.BtnStop_Click);
			// 
			// dataGrid
			// 
			this.dataGrid.AccessibleDescription = resources.GetString("dataGrid.AccessibleDescription");
			this.dataGrid.AccessibleName = resources.GetString("dataGrid.AccessibleName");
			this.dataGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("dataGrid.Anchor")));
			this.dataGrid.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("dataGrid.BackgroundImage")));
			this.dataGrid.CaptionFont = ((System.Drawing.Font)(resources.GetObject("dataGrid.CaptionFont")));
			this.dataGrid.CaptionText = resources.GetString("dataGrid.CaptionText");
			this.dataGrid.DataMember = "";
			this.dataGrid.DataSource = this.dataView;
			this.dataGrid.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("dataGrid.Dock")));
			this.dataGrid.Enabled = ((bool)(resources.GetObject("dataGrid.Enabled")));
			this.dataGrid.Font = ((System.Drawing.Font)(resources.GetObject("dataGrid.Font")));
			this.dataGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.dataGrid.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("dataGrid.ImeMode")));
			this.dataGrid.Location = ((System.Drawing.Point)(resources.GetObject("dataGrid.Location")));
			this.dataGrid.Name = "dataGrid";
			this.dataGrid.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("dataGrid.RightToLeft")));
			this.dataGrid.Size = ((System.Drawing.Size)(resources.GetObject("dataGrid.Size")));
			this.dataGrid.TabIndex = ((int)(resources.GetObject("dataGrid.TabIndex")));
			this.dataGrid.TableStyles.AddRange(new System.Windows.Forms.DataGridTableStyle[] {
																								 this.dataGridTableStyle});
			this.dataGrid.Visible = ((bool)(resources.GetObject("dataGrid.Visible")));
			this.dataGrid.Click += new System.EventHandler(this.dataGrid_Click);
			// 
			// dataView
			// 
			this.dataView.AllowDelete = false;
			this.dataView.AllowNew = false;
			this.dataView.Sort = "Rating desc";
			this.dataView.Table = this.hint.HintTable;
			// 
			// hint
			// 
			this.hint.DataSetName = "Hint";
			this.hint.Locale = new System.Globalization.CultureInfo("en-US");
			// 
			// dataGridTableStyle
			// 
			this.dataGridTableStyle.DataGrid = this.dataGrid;
			this.dataGridTableStyle.GridColumnStyles.AddRange(new System.Windows.Forms.DataGridColumnStyle[] {
																												 this.dataGridBoolColumnSelected,
																												 this.dataGridTextBoxColumnRating,
																												 this.dataGridTextBoxColumnTarget,
																												 this.dataGridTextBoxColumnBase});
			this.dataGridTableStyle.HeaderFont = ((System.Drawing.Font)(resources.GetObject("dataGridTableStyle.HeaderFont")));
			this.dataGridTableStyle.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.dataGridTableStyle.MappingName = "HintTable";
			this.dataGridTableStyle.PreferredColumnWidth = ((int)(resources.GetObject("dataGridTableStyle.PreferredColumnWidth")));
			this.dataGridTableStyle.PreferredRowHeight = ((int)(resources.GetObject("dataGridTableStyle.PreferredRowHeight")));
			this.dataGridTableStyle.RowHeaderWidth = ((int)(resources.GetObject("dataGridTableStyle.RowHeaderWidth")));
			// 
			// dataGridBoolColumnSelected
			// 
			this.dataGridBoolColumnSelected.Alignment = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("dataGridBoolColumnSelected.Alignment")));
			this.dataGridBoolColumnSelected.AllowNull = false;
			this.dataGridBoolColumnSelected.FalseValue = false;
			this.dataGridBoolColumnSelected.HeaderText = resources.GetString("dataGridBoolColumnSelected.HeaderText");
			this.dataGridBoolColumnSelected.MappingName = resources.GetString("dataGridBoolColumnSelected.MappingName");
			this.dataGridBoolColumnSelected.NullText = resources.GetString("dataGridBoolColumnSelected.NullText");
			this.dataGridBoolColumnSelected.NullValue = ((object)(resources.GetObject("dataGridBoolColumnSelected.NullValue")));
			this.dataGridBoolColumnSelected.TrueValue = true;
			this.dataGridBoolColumnSelected.Width = ((int)(resources.GetObject("dataGridBoolColumnSelected.Width")));
			// 
			// dataGridTextBoxColumnRating
			// 
			this.dataGridTextBoxColumnRating.Alignment = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("dataGridTextBoxColumnRating.Alignment")));
			this.dataGridTextBoxColumnRating.Format = "";
			this.dataGridTextBoxColumnRating.FormatInfo = null;
			this.dataGridTextBoxColumnRating.HeaderText = resources.GetString("dataGridTextBoxColumnRating.HeaderText");
			this.dataGridTextBoxColumnRating.MappingName = resources.GetString("dataGridTextBoxColumnRating.MappingName");
			this.dataGridTextBoxColumnRating.NullText = resources.GetString("dataGridTextBoxColumnRating.NullText");
			this.dataGridTextBoxColumnRating.ReadOnly = true;
			this.dataGridTextBoxColumnRating.Width = ((int)(resources.GetObject("dataGridTextBoxColumnRating.Width")));
			// 
			// dataGridTextBoxColumnTarget
			// 
			this.dataGridTextBoxColumnTarget.Alignment = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("dataGridTextBoxColumnTarget.Alignment")));
			this.dataGridTextBoxColumnTarget.Format = "";
			this.dataGridTextBoxColumnTarget.FormatInfo = null;
			this.dataGridTextBoxColumnTarget.HeaderText = resources.GetString("dataGridTextBoxColumnTarget.HeaderText");
			this.dataGridTextBoxColumnTarget.MappingName = resources.GetString("dataGridTextBoxColumnTarget.MappingName");
			this.dataGridTextBoxColumnTarget.NullText = resources.GetString("dataGridTextBoxColumnTarget.NullText");
			this.dataGridTextBoxColumnTarget.ReadOnly = true;
			this.dataGridTextBoxColumnTarget.Width = ((int)(resources.GetObject("dataGridTextBoxColumnTarget.Width")));
			// 
			// dataGridTextBoxColumnBase
			// 
			this.dataGridTextBoxColumnBase.Alignment = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("dataGridTextBoxColumnBase.Alignment")));
			this.dataGridTextBoxColumnBase.Format = "";
			this.dataGridTextBoxColumnBase.FormatInfo = null;
			this.dataGridTextBoxColumnBase.HeaderText = resources.GetString("dataGridTextBoxColumnBase.HeaderText");
			this.dataGridTextBoxColumnBase.MappingName = resources.GetString("dataGridTextBoxColumnBase.MappingName");
			this.dataGridTextBoxColumnBase.NullText = resources.GetString("dataGridTextBoxColumnBase.NullText");
			this.dataGridTextBoxColumnBase.ReadOnly = true;
			this.dataGridTextBoxColumnBase.Width = ((int)(resources.GetObject("dataGridTextBoxColumnBase.Width")));
			// 
			// ChooseHint
			// 
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.Controls.Add(this.dataGrid);
			this.Controls.Add(this.BtnStop);
			this.Controls.Add(this.CkbApplyThis);
			this.Controls.Add(this.BtnCancel);
			this.Controls.Add(this.BtnInsert);
			this.Controls.Add(this.LblTitle);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "ChooseHint";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.ShowInTaskbar = false;
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.Load += new System.EventHandler(this.ChooseHint_Load);
			((System.ComponentModel.ISupportInitialize)(this.dataGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dataView)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.hint)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		//--------------------------------------------------------------------------------
		private void ChooseHint_Load(object sender, System.EventArgs e)
		{
			IList view = ((CurrencyManager) dataGrid.BindingContext[dataView]).List; 
			
			if (view.Count > 0)
			{
				DataRowView drv = view[0] as DataRowView;
				if (drv != null)
					( (Hint.HintTableRow) drv.Row).Selected = true;
			}
		}

		//--------------------------------------------------------------------------------
		private void Select(int row, bool select)
		{
			IList view = ((CurrencyManager) dataGrid.BindingContext[dataView]).List as DataView;
			DataRowView drv = view[row] as DataRowView;
			if (drv == null)
				return;

			if (singleSelection && select)
			{
				Application.DoEvents();
				foreach (DataRowView aDrv in view)
				{
					Hint.HintTableRow aRow = (Hint.HintTableRow)aDrv.Row;
					aRow.Selected = false;
				}
			}

			Hint.HintTableRow currentRow = (Hint.HintTableRow) drv.Row;
			currentRow.Selected = select;

			Application.DoEvents();

		}

		//--------------------------------------------------------------------------------
		private bool IsSelected(int row)
		{
			IList view = ((CurrencyManager) dataGrid.BindingContext[dataView]).List as DataView;
			DataRowView drv = view[row] as DataRowView;
			if (drv != null)
				return ( (Hint.HintTableRow) drv.Row).Selected;
			return false;

		}

		//--------------------------------------------------------------------------------
		private void dataGrid_Click(object sender, System.EventArgs e)
		{
			
			Point pt = dataGrid.PointToClient(MousePosition);
			DataGrid.HitTestInfo hti = dataGrid.HitTest(pt.X, pt.Y);
			if (hti == DataGrid.HitTestInfo.Nowhere || hti.Column != 0 || hti.Row == -1)
				return;
			
			Select(hti.Row, !IsSelected(hti.Row));
			
		}
	}

	//================================================================================
	public class HintItem
	{
		public string BaseLanguageString;
		public string HintString;
		public double Rating;
        public string Context = "";
        public bool Valid = true;
		//--------------------------------------------------------------------------------
		public HintItem(string baseLanguageString, string hint, double rating, string context, bool valid)
		{
			this.BaseLanguageString = baseLanguageString;
			this.HintString = hint;
			this.Rating = rating;
            this.Context = context;
            this.Valid = valid;
		}

		//--------------------------------------------------------------------------------
		public HintItem(string hint)
		{
			this.BaseLanguageString = string.Empty;
			this.HintString = hint;
			this.Rating = -1;
		}

		//--------------------------------------------------------------------------------
		public string RatingString { get { return Math.Round(Rating, 2).ToString("0.00");  }  } 
		//--------------------------------------------------------------------------------
		public ListViewItem Item  
		{
			get
			{
				ListViewItem item = new ListViewItem( new string[] {RatingString, HintString } );
				item.Tag = this;
				return item;
			}
		}

		//--------------------------------------------------------------------------------
		public override bool Equals(object obj)
		{
			HintItem target = (HintItem) obj;
			return BaseLanguageString.Equals(target.BaseLanguageString) &&
				HintString.Equals(target.HintString) &&
				Rating.Equals(target.Rating);
		}

		//--------------------------------------------------------------------------------
		public override int GetHashCode()
		{
			return BaseLanguageString.GetHashCode() + 
				HintString.GetHashCode() +
				Rating.GetHashCode();
		}

		//--------------------------------------------------------------------------------
		public override string ToString()
		{
			return HintString;
		}

		//--------------------------------------------------------------------------------
		public static explicit operator string(HintItem h)
		{
			return h.ToString();
		}


	}
    //--------------------------------------------------------------------------------
    public class HintComparer : IComparer<HintItem>
    {
        string context;

        public HintComparer(string context)
        {
            this.context = context;
        }
        public int Compare(HintItem x, HintItem y)
        {
            int i = x.Rating.CompareTo(y.Rating);
            if (i != 0)
                return i;
            i = x.Valid.CompareTo(y.Valid);
            if (i != 0)
                return -i;
            
            for (int j = 0; j < context.Length; j++)
            {
                char targetch = context[j];
                char xch = (j >= x.Context.Length) ? ' ' : x.Context[j];
                char ych = (j >= y.Context.Length) ? ' ' : y.Context[j];
                if (xch == targetch && ych != targetch)
                {
                    return -1;
                }
                if (ych == targetch && xch != targetch)
                {
                    return 1;
                }
                

            }

            return 0;
        }
    }
	
}
