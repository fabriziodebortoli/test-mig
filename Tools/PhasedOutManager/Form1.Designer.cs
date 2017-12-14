namespace ProgressiveEncrypter
{
	partial class Form1
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
			this.BtnDecrypt = new System.Windows.Forms.Button();
			this.BtnCrypt = new System.Windows.Forms.Button();
			this.TxtValue = new System.Windows.Forms.TextBox();
			this.TxtResult = new System.Windows.Forms.TextBox();
			this.BtnDammiValore = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.TxtName = new System.Windows.Forms.TextBox();
			this.TxtDB = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.TxtEdition = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.richTextBox1 = new System.Windows.Forms.RichTextBox();
			this.button1 = new System.Windows.Forms.Button();
			this.BtnDecod = new System.Windows.Forms.Button();
			this.TxtDecod = new System.Windows.Forms.TextBox();
			this.button2 = new System.Windows.Forms.Button();
			this.label6 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// BtnDecrypt
			// 
			this.BtnDecrypt.Location = new System.Drawing.Point(235, 200);
			this.BtnDecrypt.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.BtnDecrypt.Name = "BtnDecrypt";
			this.BtnDecrypt.Size = new System.Drawing.Size(207, 25);
			this.BtnDecrypt.TabIndex = 0;
			this.BtnDecrypt.Text = "Dammi progressivo da valore";
			this.BtnDecrypt.UseVisualStyleBackColor = true;
			this.BtnDecrypt.Click += new System.EventHandler(this.button1_Click);
			// 
			// BtnCrypt
			// 
			this.BtnCrypt.Location = new System.Drawing.Point(235, 157);
			this.BtnCrypt.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.BtnCrypt.Name = "BtnCrypt";
			this.BtnCrypt.Size = new System.Drawing.Size(207, 25);
			this.BtnCrypt.TabIndex = 1;
			this.BtnCrypt.Text = "Dammi valore da progressivo";
			this.BtnCrypt.UseVisualStyleBackColor = true;
			this.BtnCrypt.Click += new System.EventHandler(this.button2_Click);
			// 
			// TxtValue
			// 
			this.TxtValue.Location = new System.Drawing.Point(21, 31);
			this.TxtValue.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.TxtValue.Name = "TxtValue";
			this.TxtValue.Size = new System.Drawing.Size(201, 22);
			this.TxtValue.TabIndex = 2;
			this.TxtValue.TextChanged += new System.EventHandler(this.TxtValue_TextChanged);
			// 
			// TxtResult
			// 
			this.TxtResult.Location = new System.Drawing.Point(21, 74);
			this.TxtResult.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.TxtResult.Name = "TxtResult";
			this.TxtResult.ReadOnly = true;
			this.TxtResult.Size = new System.Drawing.Size(201, 22);
			this.TxtResult.TabIndex = 3;
			// 
			// BtnDammiValore
			// 
			this.BtnDammiValore.AccessibleRole = System.Windows.Forms.AccessibleRole.TitleBar;
			this.BtnDammiValore.Location = new System.Drawing.Point(235, 31);
			this.BtnDammiValore.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.BtnDammiValore.Name = "BtnDammiValore";
			this.BtnDammiValore.Size = new System.Drawing.Size(207, 25);
			this.BtnDammiValore.TabIndex = 1;
			this.BtnDammiValore.Text = "Processa e Crea Mail (2.x)";
			this.BtnDammiValore.UseVisualStyleBackColor = true;
			this.BtnDammiValore.Click += new System.EventHandler(this.BtnDammiValore_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(19, 59);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(62, 14);
			this.label1.TabIndex = 4;
			this.label1.Text = "Risultato";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(18, 15);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(203, 14);
			this.label2.TabIndex = 5;
			this.label2.Text = "Inserisci seriale (o altro valore)";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(21, 104);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(43, 14);
			this.label3.TabIndex = 7;
			this.label3.Text = "Name";
			// 
			// TxtName
			// 
			this.TxtName.Location = new System.Drawing.Point(21, 117);
			this.TxtName.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.TxtName.Name = "TxtName";
			this.TxtName.ReadOnly = true;
			this.TxtName.Size = new System.Drawing.Size(201, 22);
			this.TxtName.TabIndex = 6;
			// 
			// TxtDB
			// 
			this.TxtDB.Location = new System.Drawing.Point(21, 160);
			this.TxtDB.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.TxtDB.Name = "TxtDB";
			this.TxtDB.ReadOnly = true;
			this.TxtDB.Size = new System.Drawing.Size(201, 22);
			this.TxtDB.TabIndex = 6;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(19, 146);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(24, 14);
			this.label4.TabIndex = 7;
			this.label4.Text = "Db";
			// 
			// TxtEdition
			// 
			this.TxtEdition.Location = new System.Drawing.Point(21, 203);
			this.TxtEdition.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.TxtEdition.Name = "TxtEdition";
			this.TxtEdition.ReadOnly = true;
			this.TxtEdition.Size = new System.Drawing.Size(201, 22);
			this.TxtEdition.TabIndex = 6;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(19, 188);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(50, 14);
			this.label5.TabIndex = 7;
			this.label5.Text = "Edition";
			// 
			// richTextBox1
			// 
			this.richTextBox1.Location = new System.Drawing.Point(21, 275);
			this.richTextBox1.Name = "richTextBox1";
			this.richTextBox1.Size = new System.Drawing.Size(421, 373);
			this.richTextBox1.TabIndex = 8;
			this.richTextBox1.Text = "";
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(235, 243);
			this.button1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(207, 25);
			this.button1.TabIndex = 0;
			this.button1.Text = "Pulisci tutto";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click_1);
			// 
			// BtnDecod
			// 
			this.BtnDecod.Location = new System.Drawing.Point(235, 115);
			this.BtnDecod.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.BtnDecod.Name = "BtnDecod";
			this.BtnDecod.Size = new System.Drawing.Size(207, 25);
			this.BtnDecod.TabIndex = 0;
			this.BtnDecod.Text = "Decodifica seriale";
			this.BtnDecod.UseVisualStyleBackColor = true;
			this.BtnDecod.Click += new System.EventHandler(this.BtnDecod_Click);
			// 
			// TxtDecod
			// 
			this.TxtDecod.Location = new System.Drawing.Point(21, 246);
			this.TxtDecod.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.TxtDecod.Name = "TxtDecod";
			this.TxtDecod.ReadOnly = true;
			this.TxtDecod.Size = new System.Drawing.Size(201, 22);
			this.TxtDecod.TabIndex = 2;
			// 
			// button2
			// 
			this.button2.AccessibleRole = System.Windows.Forms.AccessibleRole.TitleBar;
			this.button2.Location = new System.Drawing.Point(235, 73);
			this.button2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(207, 25);
			this.button2.TabIndex = 1;
			this.button2.Text = "Processa e Crea File (3.x)";
			this.button2.UseVisualStyleBackColor = true;
			this.button2.Click += new System.EventHandler(this.button2_Click_1);
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(21, 230);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(72, 14);
			this.label6.TabIndex = 7;
			this.label6.Text = "PlainValue";
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 14F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Lavender;
			this.ClientSize = new System.Drawing.Size(455, 660);
			this.Controls.Add(this.TxtEdition);
			this.Controls.Add(this.TxtDB);
			this.Controls.Add(this.TxtName);
			this.Controls.Add(this.TxtResult);
			this.Controls.Add(this.TxtValue);
			this.Controls.Add(this.richTextBox1);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.TxtDecod);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.BtnDammiValore);
			this.Controls.Add(this.BtnCrypt);
			this.Controls.Add(this.BtnDecod);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.BtnDecrypt);
			this.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.MaximumSize = new System.Drawing.Size(461, 692);
			this.MinimumSize = new System.Drawing.Size(461, 692);
			this.Name = "Form1";
			this.Text = "Phased-out Manager 20091116";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button BtnDecrypt;
		private System.Windows.Forms.Button BtnCrypt;
		private System.Windows.Forms.TextBox TxtValue;
		private System.Windows.Forms.TextBox TxtResult;
		private System.Windows.Forms.Button BtnDammiValore;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox TxtName;
		private System.Windows.Forms.TextBox TxtDB;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox TxtEdition;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.RichTextBox richTextBox1;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button BtnDecod;
		private System.Windows.Forms.TextBox TxtDecod;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Label label6;
	}
}

