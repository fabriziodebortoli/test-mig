namespace Microarea.TaskBuilderNet.CRCCalculator
{
    partial class Form_CRCCalc
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
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.textBox_filePath = new System.Windows.Forms.TextBox();
            this.button_browse = new System.Windows.Forms.Button();
            this.textBox_crc = new System.Windows.Forms.TextBox();
            this.button_calc = new System.Windows.Forms.Button();
            this.textBox_string = new System.Windows.Forms.TextBox();
            this.label_filePath = new System.Windows.Forms.Label();
            this.label_text = new System.Windows.Forms.Label();
            this.label_crc = new System.Windows.Forms.Label();
            this.label_source = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // textBox_filePath
            // 
            this.textBox_filePath.Location = new System.Drawing.Point(35, 38);
            this.textBox_filePath.Name = "textBox_filePath";
            this.textBox_filePath.Size = new System.Drawing.Size(365, 20);
            this.textBox_filePath.TabIndex = 0;
            this.textBox_filePath.TextChanged += new System.EventHandler(this.TextBox_filePath_TextChanged);
            // 
            // button_browse
            // 
            this.button_browse.Location = new System.Drawing.Point(418, 35);
            this.button_browse.Name = "button_browse";
            this.button_browse.Size = new System.Drawing.Size(31, 23);
            this.button_browse.TabIndex = 1;
            this.button_browse.Text = "...";
            this.button_browse.UseVisualStyleBackColor = true;
            this.button_browse.Click += new System.EventHandler(this.Button_browse_Click);
            // 
            // textBox_crc
            // 
            this.textBox_crc.Location = new System.Drawing.Point(35, 205);
            this.textBox_crc.Name = "textBox_crc";
            this.textBox_crc.Size = new System.Drawing.Size(365, 20);
            this.textBox_crc.TabIndex = 2;
            // 
            // button_calc
            // 
            this.button_calc.Location = new System.Drawing.Point(35, 128);
            this.button_calc.Name = "button_calc";
            this.button_calc.Size = new System.Drawing.Size(75, 23);
            this.button_calc.TabIndex = 3;
            this.button_calc.Text = "Calculate";
            this.button_calc.UseVisualStyleBackColor = true;
            this.button_calc.Click += new System.EventHandler(this.Button_calc_Click);
            // 
            // textBox_string
            // 
            this.textBox_string.Location = new System.Drawing.Point(35, 94);
            this.textBox_string.Name = "textBox_string";
            this.textBox_string.Size = new System.Drawing.Size(365, 20);
            this.textBox_string.TabIndex = 4;
            this.textBox_string.TextChanged += new System.EventHandler(this.textBox_string_TextChanged);
            // 
            // label_filePath
            // 
            this.label_filePath.AutoSize = true;
            this.label_filePath.Location = new System.Drawing.Point(35, 19);
            this.label_filePath.Name = "label_filePath";
            this.label_filePath.Size = new System.Drawing.Size(48, 13);
            this.label_filePath.TabIndex = 5;
            this.label_filePath.Text = "File Path";
            // 
            // label_text
            // 
            this.label_text.AutoSize = true;
            this.label_text.Location = new System.Drawing.Point(35, 71);
            this.label_text.Name = "label_text";
            this.label_text.Size = new System.Drawing.Size(28, 13);
            this.label_text.TabIndex = 6;
            this.label_text.Text = "Text";
            // 
            // label_crc
            // 
            this.label_crc.AutoSize = true;
            this.label_crc.Location = new System.Drawing.Point(35, 186);
            this.label_crc.Name = "label_crc";
            this.label_crc.Size = new System.Drawing.Size(82, 13);
            this.label_crc.TabIndex = 7;
            this.label_crc.Text = "Calculated CRC";
            // 
            // label_source
            // 
            this.label_source.AutoSize = true;
            this.label_source.Location = new System.Drawing.Point(143, 186);
            this.label_source.Name = "label_source";
            this.label_source.Size = new System.Drawing.Size(0, 13);
            this.label_source.TabIndex = 8;
            // 
            // Form_CRCCalc
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(529, 341);
            this.Controls.Add(this.label_source);
            this.Controls.Add(this.label_crc);
            this.Controls.Add(this.label_text);
            this.Controls.Add(this.label_filePath);
            this.Controls.Add(this.textBox_string);
            this.Controls.Add(this.button_calc);
            this.Controls.Add(this.textBox_crc);
            this.Controls.Add(this.button_browse);
            this.Controls.Add(this.textBox_filePath);
            this.Name = "Form_CRCCalc";
            this.Text = "CRC-32 Calculator";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.TextBox textBox_filePath;
        private System.Windows.Forms.Button button_browse;
        private System.Windows.Forms.TextBox textBox_crc;
        private System.Windows.Forms.Button button_calc;
        private System.Windows.Forms.TextBox textBox_string;
        private System.Windows.Forms.Label label_filePath;
        private System.Windows.Forms.Label label_text;
        private System.Windows.Forms.Label label_crc;
        private System.Windows.Forms.Label label_source;
    }
}

