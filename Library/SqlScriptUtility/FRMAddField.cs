using System;
using System.Drawing;
using System.Collections;
using System.Windows.Forms;

namespace Microarea.Library.SqlScriptUtility
{
	/// <summary>
	/// Summary description for FieldDialog.
	/// </summary>
	public class FieldDialog : System.Windows.Forms.Form
	{
		private System.Windows.Forms.TextBox ENTNomeField;
		private System.Windows.Forms.ComboBox CMBTipo;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private string tableName = string.Empty;
		public TableColumn MyTableColumn;
		public delegate string UpdateDefaultConstraint(string nTabella, string nColonna);
		public UpdateDefaultConstraint OnUpdateDefaultConstraint;
		public delegate bool CheckNomeCampo(string nomeCampo);
		public CheckNomeCampo OnCheckNomeCampo;
		private NumberBox ENTLen;
		private System.Windows.Forms.CheckBox CHKNull;
		private System.Windows.Forms.TextBox ENTDefault;
		private System.Windows.Forms.TextBox ENTConstraint;
		private System.Windows.Forms.Button CMDAnnulla;
		private System.Windows.Forms.Button CDMOk;
		private string oldName = string.Empty;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public FieldDialog()
		{
			MyTableColumn = new TableColumn();
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		public FieldDialog(TableColumn tColumn, string tabella)
		{
			tableName = tabella;
			MyTableColumn = tColumn;
			
			if (MyTableColumn == null)
				MyTableColumn = new TableColumn();

			InitializeComponent();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
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
			this.ENTNomeField = new System.Windows.Forms.TextBox();
			this.CMBTipo = new System.Windows.Forms.ComboBox();
			this.ENTLen = new NumberBox();
			this.CHKNull = new System.Windows.Forms.CheckBox();
			this.ENTDefault = new System.Windows.Forms.TextBox();
			this.ENTConstraint = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.CMDAnnulla = new System.Windows.Forms.Button();
			this.CDMOk = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// ENTNomeField
			// 
			this.ENTNomeField.Location = new System.Drawing.Point(8, 32);
			this.ENTNomeField.Name = "ENTNomeField";
			this.ENTNomeField.Size = new System.Drawing.Size(168, 20);
			this.ENTNomeField.TabIndex = 0;
			this.ENTNomeField.Text = "";
			// 
			// CMBTipo
			// 
			this.CMBTipo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.CMBTipo.Items.AddRange(new object[] {
														 "smallint",
														 "int",
														 "float",
														 "char",
														 "varchar",
														 "uniqueidentifier",
														 "text",
														 "datetime"});
			this.CMBTipo.Location = new System.Drawing.Point(8, 80);
			this.CMBTipo.Name = "CMBTipo";
			this.CMBTipo.Size = new System.Drawing.Size(96, 21);
			this.CMBTipo.TabIndex = 1;
			this.CMBTipo.SelectedIndexChanged += new System.EventHandler(this.CMBTipo_SelectedIndexChanged);
			// 
			// ENTLen
			// 
			this.ENTLen.Enabled = false;
			this.ENTLen.Location = new System.Drawing.Point(112, 80);
			this.ENTLen.Name = "ENTLen";
			this.ENTLen.Size = new System.Drawing.Size(48, 20);
			this.ENTLen.TabIndex = 2;
			this.ENTLen.Text = "";
			// 
			// CHKNull
			// 
			this.CHKNull.Location = new System.Drawing.Point(176, 80);
			this.CHKNull.Name = "CHKNull";
			this.CHKNull.Size = new System.Drawing.Size(96, 16);
			this.CHKNull.TabIndex = 3;
			this.CHKNull.Text = "Ammette null";
			// 
			// ENTDefault
			// 
			this.ENTDefault.Location = new System.Drawing.Point(8, 128);
			this.ENTDefault.Name = "ENTDefault";
			this.ENTDefault.Size = new System.Drawing.Size(216, 20);
			this.ENTDefault.TabIndex = 4;
			this.ENTDefault.Text = "";
			this.ENTDefault.TextChanged += new System.EventHandler(this.ENTDefault_TextChanged);
			// 
			// ENTConstraint
			// 
			this.ENTConstraint.Location = new System.Drawing.Point(232, 128);
			this.ENTConstraint.Name = "ENTConstraint";
			this.ENTConstraint.Size = new System.Drawing.Size(304, 20);
			this.ENTConstraint.TabIndex = 5;
			this.ENTConstraint.Text = "";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(8, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(34, 16);
			this.label1.TabIndex = 6;
			this.label1.Text = "Nome";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(8, 64);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(26, 16);
			this.label2.TabIndex = 7;
			this.label2.Text = "Tipo";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(112, 64);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(23, 16);
			this.label3.TabIndex = 8;
			this.label3.Text = "Len";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(8, 112);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(40, 16);
			this.label4.TabIndex = 9;
			this.label4.Text = "Default";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(232, 112);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(107, 16);
			this.label5.TabIndex = 10;
			this.label5.Text = "Constraint di Default";
			// 
			// CMDAnnulla
			// 
			this.CMDAnnulla.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CMDAnnulla.Location = new System.Drawing.Point(376, 160);
			this.CMDAnnulla.Name = "CMDAnnulla";
			this.CMDAnnulla.Size = new System.Drawing.Size(80, 32);
			this.CMDAnnulla.TabIndex = 11;
			this.CMDAnnulla.Text = "Annulla";
			this.CMDAnnulla.Click += new System.EventHandler(this.CMDAnnulla_Click);
			// 
			// CDMOk
			// 
			this.CDMOk.Location = new System.Drawing.Point(464, 160);
			this.CDMOk.Name = "CDMOk";
			this.CDMOk.Size = new System.Drawing.Size(72, 32);
			this.CDMOk.TabIndex = 12;
			this.CDMOk.Text = "OK";
			this.CDMOk.Click += new System.EventHandler(this.CMDOk_Click);
			// 
			// FieldDialog
			// 
			this.AcceptButton = this.CDMOk;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.CMDAnnulla;
			this.ClientSize = new System.Drawing.Size(546, 208);
			this.ControlBox = false;
			this.Controls.Add(this.CDMOk);
			this.Controls.Add(this.CMDAnnulla);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.ENTConstraint);
			this.Controls.Add(this.ENTDefault);
			this.Controls.Add(this.CHKNull);
			this.Controls.Add(this.ENTLen);
			this.Controls.Add(this.CMBTipo);
			this.Controls.Add(this.ENTNomeField);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "FieldDialog";
			this.Text = "Inserimento/Modifica campo";
			this.Load += new System.EventHandler(this.FieldDialog_Load);
			this.ResumeLayout(false);

		}
		#endregion

		private void FieldDialog_Load(object sender, System.EventArgs e)
		{
			ENTNomeField.Text = MyTableColumn.m_NomeColonnaEsteso;
			oldName = MyTableColumn.m_NomeColonnaEsteso;
			ENTLen.Text = MyTableColumn.m_LenColonna.ToString();
			CHKNull.Checked = MyTableColumn.isNullable;
			ENTDefault.Text = MyTableColumn.m_Default;
			ENTConstraint.Text = MyTableColumn.m_DefaultConstraintName;

			int selInd = CMBTipo.Items.IndexOf(MyTableColumn.m_TipoColonna);
			CMBTipo.SelectedIndex = selInd;
			TestType();
		}

		private void CMBTipo_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			TestType();
		}

		private void TestType()
		{
			if (CMBTipo.SelectedIndex < 0)
				return;
			switch (CMBTipo.Items[CMBTipo.SelectedIndex].ToString())
			{
				case "char":
					ENTLen.Enabled = true;
					break;
				case "varchar":
					ENTLen.Enabled = true;
					break;
				default:
					ENTLen.Text = string.Empty;
					ENTLen.Enabled = false;
					break;
			}
		}

		private void CMDAnnulla_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			Close();
		}

		private void CMDOk_Click(object sender, System.EventArgs e)
		{
			if (!TestControls())
				return;

			MyTableColumn.m_NomeColonnaEsteso = ENTNomeField.Text;
			if (ENTLen.Text != string.Empty)
				MyTableColumn.m_LenColonna = int.Parse(ENTLen.Text);
			MyTableColumn.isNullable = CHKNull.Checked;
			MyTableColumn.m_Default = ENTDefault.Text;
			MyTableColumn.m_DefaultConstraintName = ENTConstraint.Text;
			MyTableColumn.m_TipoColonna = CMBTipo.Text;

			this.DialogResult = DialogResult.OK;
			Close();
		}

		private void ENTDefault_TextChanged(object sender, System.EventArgs e)
		{
			if (ENTConstraint.Text != string.Empty)
				return;

			if (OnUpdateDefaultConstraint != null)
				ENTConstraint.Text = OnUpdateDefaultConstraint(tableName, ENTNomeField.Text);
			else
				ENTConstraint.Text = string.Empty;
		}

		private bool TestControls()
		{
			if (ENTNomeField.Text == string.Empty)
			{
				MessageBox.Show("Il nome del campo non può essere vuoto");
				ENTNomeField.Focus();
				return false;
			}

			//inserire test di esistenza del nome del campo
			if (ENTNomeField.Text != oldName)
				if (OnCheckNomeCampo != null)
					if (!OnCheckNomeCampo(ENTNomeField.Text))
					{
						MessageBox.Show("Il nome del campo è già presente!");
						ENTNomeField.Focus();
						return false;
					}

			if (CMBTipo.SelectedIndex < 0)
			{
				MessageBox.Show("E' necessario inserire un tipo!");
				CMBTipo.Focus();
				return false;
			}

			switch (CMBTipo.Items[CMBTipo.SelectedIndex].ToString())
			{
				case "char":
					if (ENTLen.Text == "0")
					{
						MessageBox.Show("Lunghezza del campo non valida!");
						ENTLen.Focus();
						return false;
					}
					break;
				case "varchar":
					if (ENTLen.Text == "0")
					{
						MessageBox.Show("Lunghezza del campo non valida!");
						ENTLen.Focus();
						return false;
					}
					break;
			}

			if (ENTDefault.Text != string.Empty && ENTConstraint.Text == string.Empty)
			{
				MessageBox.Show("E' necessario inserire un constraint di default!");
				ENTConstraint.Focus();
				return false;
			}

			return true;
		}
	}
}
