using System;
using System.Drawing;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Licence.Activation;

namespace Microarea.TaskBuilderNet.Licence.Licence.Forms
{	
	/// <summary>
	/// Quattro textbox per inserire i 4 campi del serial-number.
	/// </summary>
	//=========================================================================
	public class SerialTextBoxes : System.Windows.Forms.UserControl
	{
		private System.ComponentModel.Container components = null;
		private TextBoxNoKeyUp TxtSerial1;
		private TextBoxNoKeyUp TxtSerial2;
		private TextBoxNoKeyUp TxtSerial3;
		private TextBoxNoKeyUp TxtSerial4;
		private System.Windows.Forms.ContextMenu CMOptions;
		private System.Windows.Forms.MenuItem MiClear;
		private System.Windows.Forms.MenuItem MiCopy;
		private System.Windows.Forms.MenuItem MiPaste;

		public bool IsValid		= true;
		public bool Complete	= false;

		/// <summary>Scatta alla modifica di una delle TextBox del controllo</summary>
		public event ModificationManager Modified;
		/// <summary>Scatta alla modifica di una delle TextBox del controllo</summary>
		public event ModificationManager Completed;
        /// <summary>Scatta in caso di eccezione in una delle TextBox del controllo</summary>
        public event ExceptionManager ExceptionRaised;
		//---------------------------------------------------------------------
		public SerialTextBoxes()
		{
			InitializeComponent();
		}


		//---------------------------------------------------------------------
		private void TxtSerial_TextChanged(object sender, System.EventArgs e)
		{
			TxtSerial1.ForeColor = Color.Black;
			TxtSerial2.ForeColor = Color.Black;
			TxtSerial3.ForeColor = Color.Black;
			TxtSerial4.ForeColor = Color.Black;

			string textBoxName =((TextBox)sender).Name ;
			string text = ((TextBox)sender).Text;
			//Funzionalità per incollare serial number, 
			//solo se su prima casella e solo se stringa 
			//è serialNumber valido, accettato anche con separatore.
			string cleanText = text;
			foreach (char c in text)
			{
				if (CharRefused(c))
					cleanText = cleanText.Replace(c.ToString(), String.Empty);
			}
			if (cleanText.Length == 16 && textBoxName == "TxtSerial1")
			{
				SafeGui.ControlText(TxtSerial1, cleanText.Substring(0, 4));
				SafeGui.ControlText(TxtSerial2, cleanText.Substring(4, 4));
				SafeGui.ControlText(TxtSerial3, cleanText.Substring(8, 4));
				SafeGui.ControlText(TxtSerial4, cleanText.Substring(12, 4));
			}

			else if (cleanText.Length == 4)
				switch (textBoxName)
				{
					case "TxtSerial1":
						TxtSerial2.Focus();
						TxtSerial2.Select(0,4);
						break;
					case "TxtSerial2":
						TxtSerial3.Focus();
						TxtSerial3.Select(0,4);
						break;
					case "TxtSerial3":
						TxtSerial4.Focus();
						TxtSerial4.Select(0,4);
						break;
					case "TxtSerial4":
						
						break;
				}

			if (TxtSerial1.Text.Length > 4)
				Clear();

			Complete = (
				TxtSerial1.Text.Length == 4 &&
				TxtSerial2.Text.Length == 4 &&
				TxtSerial3.Text.Length == 4 &&
				TxtSerial4.Text.Length == 4 
				);
			
			if (Complete)
			{
				if (!CanInstanceSN(Serial.GetSerialWOSeparator()) || !SerialNumber.IsCrcCorrect(Serial.GetSerialWOSeparator()))
				{
					TxtSerial1.ForeColor = 
					TxtSerial2.ForeColor = 
					TxtSerial3.ForeColor = 
					TxtSerial4.ForeColor = Color.Red;
					IsValid = false;
				}
				else 
				{
					TxtSerial1.ForeColor = 
					TxtSerial2.ForeColor =
					TxtSerial3.ForeColor =
					TxtSerial4.ForeColor = Color.Black;
					IsValid = true;
				}
			}				
			if (Modified != null)
					Modified(this, EventArgs.Empty);

			if (Complete && Completed != null)
					Completed(this, EventArgs.Empty);

		}

		//---------------------------------------------------------------------
		private bool CanInstanceSN(string sn)
		{
			try
			{
				SerialNumber s = new SerialNumber(sn);
			}
			catch (System.Security.Cryptography.CryptographicException exc)
			{
                if (ExceptionRaised != null)
                    ExceptionRaised(this, new ExceptionEventArgs(exc));
				return false;
			}
            catch 
			{
				return false;
			}

			return true;
		}

		//---------------------------------------------------------------------
		private void TxtSerial_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			if (CharRefused(e.KeyChar))
			//Accetto solo lettere e numeri.
			//if (!char.IsLetterOrDigit(e.KeyChar) && !char.IsControl(e.KeyChar)) 
			{
				e.Handled = true; 
				return;
			}
		}

		//---------------------------------------------------------------------
		private bool CharRefused(char c)
		{
			return (!char.IsLetterOrDigit(c) && !char.IsControl(c));
		}

		//---------------------------------------------------------------------
		public void Fill(SerialNumberInfo serial)
		{
			if (serial == null)
				return;
			string[] serialArray = serial.GetSerialAsArray();
			SafeGui.ControlText(TxtSerial1, serialArray[0]);
			SafeGui.ControlText(TxtSerial2, serialArray[1]);
			SafeGui.ControlText(TxtSerial3, serialArray[2]);
			SafeGui.ControlText(TxtSerial4, serialArray[3]);
		}

		//---------------------------------------------------------------------
		public void Clear()
		{
			SafeGui.ControlText(TxtSerial1, String.Empty);
			SafeGui.ControlText(TxtSerial2, String.Empty);
			SafeGui.ControlText(TxtSerial3, String.Empty);
			SafeGui.ControlText(TxtSerial4, String.Empty);
		}

		//---------------------------------------------------------------------
		public void Fill(string serial)
		{
			SerialNumberInfo sn = new SerialNumberInfo(serial);
			Fill(sn);			
		}

		//---------------------------------------------------------------------
		public SerialNumberInfo Serial
		{
			get
			{
				SerialNumberInfo sn = new SerialNumberInfo(TxtSerial1.Text, TxtSerial2.Text, TxtSerial3.Text, TxtSerial4.Text);	
				return sn;
			}
		}

		//---------------------------------------------------------------------
		public string SerialString
		{
			get {return Serial.GetSerialWSeparator();}
		}

		//---------------------------------------------------------------------
		public void Activate()
		{
			TxtSerial1.Focus();
			TxtSerial1.Select(0,0);
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

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SerialTextBoxes));
			this.TxtSerial1 = new Microarea.TaskBuilderNet.Licence.Licence.Forms.TextBoxNoKeyUp();
			this.CMOptions = new System.Windows.Forms.ContextMenu();
			this.MiClear = new System.Windows.Forms.MenuItem();
			this.MiCopy = new System.Windows.Forms.MenuItem();
			this.MiPaste = new System.Windows.Forms.MenuItem();
			this.TxtSerial2 = new Microarea.TaskBuilderNet.Licence.Licence.Forms.TextBoxNoKeyUp();
			this.TxtSerial3 = new Microarea.TaskBuilderNet.Licence.Licence.Forms.TextBoxNoKeyUp();
			this.TxtSerial4 = new Microarea.TaskBuilderNet.Licence.Licence.Forms.TextBoxNoKeyUp();
			this.SuspendLayout();
			// 
			// TxtSerial1
			// 
			this.TxtSerial1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.TxtSerial1.ContextMenu = this.CMOptions;
			resources.ApplyResources(this.TxtSerial1, "TxtSerial1");
			this.TxtSerial1.Name = "TxtSerial1";
			this.TxtSerial1.TextChanged += new System.EventHandler(this.TxtSerial_TextChanged);
			this.TxtSerial1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtSerial_KeyPress);
			// 
			// CMOptions
			// 
			this.CMOptions.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.MiClear,
            this.MiCopy,
            this.MiPaste});
			// 
			// MiClear
			// 
			this.MiClear.Index = 0;
			resources.ApplyResources(this.MiClear, "MiClear");
			this.MiClear.Click += new System.EventHandler(this.MiClear_Click);
			// 
			// MiCopy
			// 
			this.MiCopy.Index = 1;
			resources.ApplyResources(this.MiCopy, "MiCopy");
			this.MiCopy.Click += new System.EventHandler(this.MiCopy_Click);
			// 
			// MiPaste
			// 
			this.MiPaste.Index = 2;
			resources.ApplyResources(this.MiPaste, "MiPaste");
			this.MiPaste.Click += new System.EventHandler(this.MiPaste_Click);
			// 
			// TxtSerial2
			// 
			this.TxtSerial2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.TxtSerial2.ContextMenu = this.CMOptions;
			resources.ApplyResources(this.TxtSerial2, "TxtSerial2");
			this.TxtSerial2.Name = "TxtSerial2";
			this.TxtSerial2.TextChanged += new System.EventHandler(this.TxtSerial_TextChanged);
			this.TxtSerial2.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtSerial_KeyPress);
			// 
			// TxtSerial3
			// 
			this.TxtSerial3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.TxtSerial3.ContextMenu = this.CMOptions;
			resources.ApplyResources(this.TxtSerial3, "TxtSerial3");
			this.TxtSerial3.Name = "TxtSerial3";
			this.TxtSerial3.TextChanged += new System.EventHandler(this.TxtSerial_TextChanged);
			this.TxtSerial3.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtSerial_KeyPress);
			// 
			// TxtSerial4
			// 
			this.TxtSerial4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.TxtSerial4.ContextMenu = this.CMOptions;
			resources.ApplyResources(this.TxtSerial4, "TxtSerial4");
			this.TxtSerial4.Name = "TxtSerial4";
			this.TxtSerial4.TextChanged += new System.EventHandler(this.TxtSerial_TextChanged);
			this.TxtSerial4.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtSerial_KeyPress);
			// 
			// SerialTextBoxes
			// 
			this.BackColor = System.Drawing.SystemColors.Window;
			this.Controls.Add(this.TxtSerial4);
			this.Controls.Add(this.TxtSerial3);
			this.Controls.Add(this.TxtSerial2);
			this.Controls.Add(this.TxtSerial1);
			this.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.Name = "SerialTextBoxes";
			resources.ApplyResources(this, "$this");
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		//---------------------------------------------------------------------
		private void MiClear_Click(object sender, System.EventArgs e)
		{
			Clear();
			TxtSerial1.Select(0,1);
		}

		//---------------------------------------------------------------------
		private void MiCopy_Click(object sender, System.EventArgs e)
		{
			if (SerialString != null && SerialString.Length > 0)
				Clipboard.SetDataObject(SerialString, true);
		}

		//---------------------------------------------------------------------
		private void MiPaste_Click(object sender, System.EventArgs e)
		{
			IDataObject o = Clipboard.GetDataObject();
			if(o.GetDataPresent(DataFormats.Text)) 
			{
				string s = (String)o.GetData(DataFormats.Text);
				if (s != null && s.Length > 0)
				{
					Fill(s);
					TxtSerial4.Select(4,1);
				}
			}
		}

		
		
	}
	//Gestisce il change delle textBox
	public delegate void ModificationManager(object sender, EventArgs e);
    //Gestisce eccezioni delle textBox
    public delegate void ExceptionManager(object sender, ExceptionEventArgs e);

    //=========================================================================
    public class TextBoxNoKeyUp : TextBox
	{
		private const int WM_KEYUP = 0x101; 

		//---------------------------------------------------------------------
		protected override void WndProc(ref System.Windows.Forms.Message m) 
		{ 
			if(m.Msg == WM_KEYUP) 
			{ 
				return; //ignore the keyup 
			} 

			base.WndProc(ref m); 
		} 
	}
  
 
}
