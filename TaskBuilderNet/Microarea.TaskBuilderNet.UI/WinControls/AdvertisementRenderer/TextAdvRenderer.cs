using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Interfaces;


namespace Microarea.TaskBuilderNet.UI.WinControls.AdvertisementRenderer
{
	/// <summary>
	/// SimpleAdvRenderer.
	/// </summary>
	//=========================================================================
	public class TextAdvRenderer : BaseAdvRenderer
	{
		private System.Windows.Forms.RichTextBox RichTextBox;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		//--------------------------------------------------------------------
		public TextAdvRenderer()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();
		}

		//--------------------------------------------------------------------
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

		#region Component Designer generated code
		//--------------------------------------------------------------------
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.RichTextBox = new System.Windows.Forms.RichTextBox();
			this.SuspendLayout();
			// 
			// RichTextBox
			// 
			this.RichTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.RichTextBox.Cursor = System.Windows.Forms.Cursors.Arrow;
			this.RichTextBox.DetectUrls = false;
			this.RichTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RichTextBox.Location = new System.Drawing.Point(0, 0);
			this.RichTextBox.Name = "RichTextBox";
			this.RichTextBox.Size = new System.Drawing.Size(150, 150);
			this.RichTextBox.TabIndex = 0;
			this.RichTextBox.Text = "";
			this.RichTextBox.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.RichTextBox_LinkClicked);
			// 
			// TextAdvRenderer
			// 
			this.Controls.Add(this.RichTextBox);
			this.Name = "TextAdvRenderer";
			this.ResumeLayout(false);

		}
		#endregion

		//--------------------------------------------------------------------
		public override void RenderAdvertisement(IAdvertisement advertisement)
		{
			if (advertisement == null || advertisement.Body == null)
				return;

			if (advertisement.Body.LocalizationBag != null)
				advertisement.Body.Text = ComposeMessage(advertisement.Body.LocalizationBag, "{0}{5}{5}{1}{5}{2}{5}{3}{5}{5}{4}{5}", Environment.NewLine);

			//ci metto un po di acapo per spaziare dal bordo del controllo contenitore...
			RichTextBox.Text = String.Format("{0}{1}{0}{2}", Environment.NewLine, advertisement.Body.Text, Environment.NewLine, advertisement.Body.Link); 
		}

		//--------------------------------------------------------------------
		public override void RenderAdvertisement(Uri documentUri)
		{
			if (documentUri == null)
				return;

			if (!documentUri.IsFile)
				return;

			string temp = null;
			using (StreamReader sr = new StreamReader(documentUri.ToString(), true))
				temp = sr.ReadToEnd();

			RichTextBox.Text = temp;
		}

		//--------------------------------------------------------------------
		public virtual string ComposeMessage(ILocalizationBag localizationBag, string textMask, string newline)
		{
			if (localizationBag == null || textMask == null || textMask.Length == 0)
				return String.Empty;

			string dearcustomer = string.Format(WinControlsStrings.DearCustomer, newline);
			decimal monthsApprox = new TimeSpan(localizationBag.RenewalPeriodTicks).Days/30;
			decimal months = Math.Round(monthsApprox, 0);

			switch (localizationBag.Key)
			{
				case "ContractBeforeExpiring":
					//nome prodotto, giorni che mancano alla scadenza
					string contract13_1 = String.Format(WinControlsStrings.Contract13_1, localizationBag.ProductName, localizationBag.Days);
					//clicca qui con link, user email, microarea.it, a capo
					string contract13_2 = String.Format(WinControlsStrings.Contract13_2, String.Empty, ConstString.MicroareaSite, newline);
					//formattazione del template textMask coi vari componenti
					return string.Format(
						textMask,
						WinControlsStrings.ContractExpiring,
						String.Concat(dearcustomer, contract13_1),
						contract13_2,
						WinControlsStrings.Ignore,
						WinControlsStrings.Thanks, 
						newline
						);

				case "FreePeriodBeforeExpiring":
					//nome prodotto, giorni che mancano alla scadenza
					string freePeriod11_1 = String.Format(WinControlsStrings.FreePeriod11_1, localizationBag.ProductName, localizationBag.Days);
					//clicca qui con link, user email, microarea.it, a capo
					string freePeriod11_2 = String.Format(WinControlsStrings.FreePeriod11_2, String.Empty, ConstString.MicroareaSite, newline);

					//formattazione del template textMask coi vari componenti
					return string.Format(
						textMask,
						WinControlsStrings.FreePeriodExpiring,
						String.Concat(dearcustomer, freePeriod11_1),
						freePeriod11_2,
						WinControlsStrings.Ignore,
						WinControlsStrings.Thanks, 
						newline);

				case "InstalmentsPeriodBeforeExpiring":
					//nome prodotto, giorni che mancano alla scadenza
					string instalment12_1 = String.Format(WinControlsStrings.Instalment12_1, localizationBag.ProductName, localizationBag.Days);
					//clicca qui con link, user email, microarea.it, a capo
					string instalment12_2 = String.Format(WinControlsStrings.Instalment12_2, String.Empty, ConstString.MicroareaSite, newline, months);

					//formattazione del template textMask coi vari componenti
					return string.Format(
						textMask,
						WinControlsStrings.InstalmentExpiring,
						String.Concat(dearcustomer, instalment12_1),
						instalment12_2,
						WinControlsStrings.Ignore,
						WinControlsStrings.Thanks, 
						newline);

				case "ContractExpiresToday":
					//nome prodotto, giorni che mancano alla scadenza
					string contract33_1 = String.Format(WinControlsStrings.Contract33_1, localizationBag.ProductName);
					//clicca qui con link, user email, microarea.it, a capo
					string contract33_2 = String.Format(WinControlsStrings.Contract33_2, String.Empty, ConstString.MicroareaSite, newline);

					//formattazione del template textMask coi vari componenti
					return string.Format(
						textMask,
						WinControlsStrings.ContractExpiring,
						String.Concat(dearcustomer, contract33_1),
						contract33_2,
						WinControlsStrings.Ignore,
						WinControlsStrings.Thanks, 
						newline);

				case "FreePeriodExpiresToday":
					//nome prodotto, giorni che mancano alla scadenza
					string freePeriod31_1 = String.Format(WinControlsStrings.FreePeriod31_1, localizationBag.ProductName);
					//clicca qui con link, user email, microarea.it, a capo
					string freePeriod31_2 = String.Format(WinControlsStrings.FreePeriod31_2, String.Empty, ConstString.MicroareaSite, newline);

					//formattazione del template textMask coi vari componenti
					return string.Format(
						textMask,
						WinControlsStrings.FreePeriodExpiring,
						String.Concat(dearcustomer, freePeriod31_1),
						freePeriod31_2,
						WinControlsStrings.Ignore,
						WinControlsStrings.Thanks, 
						newline);

				case "InstalmentsExpiresToday":
					//nome prodotto, giorni che mancano alla scadenza
					string instalment32_1 = String.Format(WinControlsStrings.Instalment32_1, localizationBag.ProductName);
					//clicca qui con link, user email, microarea.it, a capo
					string instalment32_2 = String.Format(WinControlsStrings.Instalment32_2, String.Empty, ConstString.MicroareaSite, newline, months);

					//formattazione del template textMask coi vari componenti
					return string.Format(
						textMask,
						WinControlsStrings.InstalmentExpiring,
						String.Concat(dearcustomer, instalment32_1),
						instalment32_2,
						WinControlsStrings.Ignore,
						WinControlsStrings.Thanks, 
						newline);

				case "ContractExpired":
					//nome prodotto, giorni che mancano alla scadenza
					string contract23_1 = String.Format(WinControlsStrings.Contract23_1, localizationBag.ProductName, localizationBag.Days);
					//clicca qui con link, user email, microarea.it, a capo
					string contract23_2 = String.Format(WinControlsStrings.Contract23_2, String.Empty, ConstString.MicroareaSite, newline);
					//formattazione del template textMask coi vari componenti
					return string.Format(
						textMask,
						WinControlsStrings.ContractExpired,
						String.Concat(dearcustomer, contract23_1),
						contract23_2,
						WinControlsStrings.Ignore,
						WinControlsStrings.Thanks, 
						newline);

				case "FreePeriodExpired":
					//nome prodotto, giorni che mancano alla scadenza
					string freePeriod21_1 = String.Format(WinControlsStrings.FreePeriod21_1, localizationBag.ProductName, localizationBag.Days);
					//clicca qui con link, user email, microarea.it, a capo
					string freePeriod21_2 = String.Format(WinControlsStrings.FreePeriod21_2, String.Empty, ConstString.MicroareaSite, newline);
					//formattazione del template textMask coi vari componenti
					return string.Format(
						textMask,
						WinControlsStrings.FreePeriodExpired,
						String.Concat(dearcustomer, freePeriod21_1),
						freePeriod21_2,
						WinControlsStrings.Ignore,
						WinControlsStrings.Thanks, 
						newline);

				case "InstalmentsExpired":
					//nome prodotto, giorni che mancano alla scadenza
					string instalment22_1 = String.Format(WinControlsStrings.Instalment22_1, localizationBag.ProductName, localizationBag.Days);
					//clicca qui con link, user email, microarea.it, a capo
					string instalment22_2 = String.Format(WinControlsStrings.Instalment22_2, String.Empty, ConstString.MicroareaSite, newline, months);

					//formattazione del template textMask coi vari componenti
					return string.Format(
						textMask,
						WinControlsStrings.InstalmentExpired,
						String.Concat(dearcustomer, instalment22_1),
						instalment22_2,
						WinControlsStrings.Ignore,
						WinControlsStrings.Thanks, 
						newline);
			}
			return String.Empty;
		}
		//--------------------------------------------------------------------
		public override void Clear()
		{
			RichTextBox.Clear();
		}

		//--------------------------------------------------------------------
		private void RichTextBox_LinkClicked(object sender, System.Windows.Forms.LinkClickedEventArgs e)
		{
			if (
				e == null ||
				e.LinkText == null ||
				e.LinkText.Trim().Length == 0
				)
				return;

			try
			{
				Process.Start(e.LinkText);
			}
			catch {}
		}
	}
}
