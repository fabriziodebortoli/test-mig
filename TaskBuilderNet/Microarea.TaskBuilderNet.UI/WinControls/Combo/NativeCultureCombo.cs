using System;
using System.Drawing;

namespace Microarea.TaskBuilderNet.UI.WinControls.Combo
{
	/// <summary>
	/// UserControl che ingloba la Combo per i regionalSettings più una 
	/// checkBox per vedere la lista con ogni item tradotto nella sua lingua.
	/// </summary>
	//=========================================================================
	public partial class NativeCultureCombo : System.Windows.Forms.UserControl
	{
		private bool withoutInvariantLanguage = false;
		private int InterfaceID = 0;
		public delegate void SelectionChangeCommitted(object sender, EventArgs e);

		/// <summary>
		/// Evento che scatta quando viene commitata la selezione della combo.
		/// </summary>
		public event SelectionChangeCommitted OnSelectionChangeCommitted;
		
		/// <summary>
		/// Culture selezionata
		/// </summary>
		//---------------------------------------------------------------------
		public string ApplicationLanguage 
		{
			get
			{
				return CmbCultureApplication.ApplicationLanguage;
			}
			set 
			{
				CmbCultureApplication.ApplicationLanguage = value;
			}
		}

		//---------------------------------------------------------------------
		public NativeCultureCombo()
		{
			InitializeComponent();
			CmbCultureApplication.SelectionChangeCommitted +=new EventHandler(CmbCultureApplication_SelectionChangeCommitted);
		}

		//---------------------------------------------------------------------
		private void CkbNativeName_CheckedChanged(object sender, System.EventArgs e)
		{
			LoadLanguages(withoutInvariantLanguage);
		}

		/// <summary>
		/// Carica la lista di culture includendo anche un item vuoto.
		/// </summary>
		//---------------------------------------------------------------------
		public void LoadLanguages()
		{
			LoadLanguages(false);
		}

		/// <summary>
		/// Carica la lista di culture includendo, se si desidera, un item vuoto.
		/// </summary>
		//---------------------------------------------------------------------
		public void LoadLanguages(bool withoutInvariantLanguage)
		{
			this.withoutInvariantLanguage = withoutInvariantLanguage;
			CmbCultureApplication.LoadLanguages(withoutInvariantLanguage, CkbNativeName.Checked);
		}

		/// <summary>
		/// Elimina la Label, sposta in alto la combo e sposta la CheckBox sotto la combo.
		/// </summary>
		//---------------------------------------------------------------------
		public void ControlLocationVerticalNoLabel()
		{
			if (InterfaceID == 0)
			{
				InterfaceID = 1;
				LblRegionalSettings.Visible = false;
				CmbCultureApplication.Location = LblRegionalSettings.Location;
				CkbNativeName.Location = new Point(0, CmbCultureApplication.Size.Height + CmbCultureApplication.Location.Y+ 3);
			}
		}

		/// <summary>
		/// Assegna un testo differente alla label che compare sopra la combobox
		/// Il default e' Regional Settings
		/// </summary>
		//---------------------------------------------------------------------
		public void SetComboBoxText(string text)
		{
			if (InterfaceID == 0)
			{
				InterfaceID = 1;
				LblRegionalSettings.Text = text;
			}
		}

		/// <summary>
		/// Riporta i controlli alla posizione normale se erano stati spostati.
		/// </summary>
		//---------------------------------------------------------------------
		public void ControlLocationNormal()
		{
			if (InterfaceID != 0)
			{
				InterfaceID = 0;
				this.CkbNativeName.Location = new System.Drawing.Point(184, 8);
				this.CmbCultureApplication.Location = new System.Drawing.Point(0, 24);
				this.LblRegionalSettings.Location = new System.Drawing.Point(0, 8);
				LblRegionalSettings.Visible = true;
			}
		}

		/// <summary>
		/// Consente di impostare esternamente la lunghezza della combobox
		/// </summary>
		//---------------------------------------------------------------------
		public void ControlSetComboBoxWidth(int width)
		{
			this.CmbCultureApplication.Width = width;
		}

//		//---------------------------------------------------------------------
//		/// <summary>
//		/// Permette di modificare il testo della CheckBox
//		/// </summary>
//		[Localizable(true)]
//		public string CheckBoxText
//		{
//			get
//			{
//				return CkbNativeName.Text;
//			}
//			set
//			{
//				CkbNativeName.Text = value;
//			}
//				
//		}

		//---------------------------------------------------------------------
		private void CmbCultureApplication_SelectionChangeCommitted(object sender, EventArgs e)
		{
			if (this.OnSelectionChangeCommitted != null)
				this.OnSelectionChangeCommitted(sender, e);
		}

	}
}
