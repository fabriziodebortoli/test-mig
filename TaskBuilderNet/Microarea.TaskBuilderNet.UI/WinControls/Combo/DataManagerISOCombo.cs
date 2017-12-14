using System;
using System.Collections;

namespace Microarea.TaskBuilderNet.UI.WinControls.Combo
{
	/// <summary>
	/// DataManagerISOCombo.
	/// Mostra la Combo con l'elenco dei codici ISO stato da proporre per il caricamento
	/// dei dati di default ed esempio
	/// E' inizializzata con il valore dell'ISO stato dell'installazione + un codice
	/// internazionale (INTL) per il set di dati internazionali indipendenti dall'iso stato
	/// </summary>
	/// =======================================================================
	public class DataManagerISOCombo : System.Windows.Forms.ComboBox
	{
		public const string InternationalSetDataCode = "INTL";

		//---------------------------------------------------------------------
		public string SelectedCountryValue
		{
			get 
			{
				if (this.SelectedItem == null || !(this.SelectedItem is LanguageItem))
					return string.Empty;

				return ((LanguageItem)this.SelectedItem).LanguageValue; 
			}
		}

		/// <summary>
		/// constructor
		/// </summary>
		//---------------------------------------------------------------------
		public DataManagerISOCombo()
		{
			this.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		}

		//---------------------------------------------------------------------
		protected override void OnDropDown(EventArgs e)
		{
			base.OnDropDown (e);
		}

		/// <summary>
		/// load dei valori ISO stato (quello installato + International)
		/// </summary>
		//---------------------------------------------------------------------
		public void LoadISOCountries(string currentIsoState)
		{
			ClearItems();

			ArrayList countriesList= new ArrayList();

			LanguageItem currCountry = new LanguageItem();
			currCountry.LanguageDescription	= currentIsoState;
			currCountry.LanguageValue		= currentIsoState;
			countriesList.Add(currCountry);

			// gestione della country generica per i dati internazionali (INTL)
			LanguageItem intlCountry = new LanguageItem();
			intlCountry.LanguageDescription = WinControlsStrings.InternationalSetDataDescription;
			intlCountry.LanguageValue		= InternationalSetDataCode;
			countriesList.Add(intlCountry);

			this.DataSource		= countriesList;
			this.DisplayMember	= "LanguageDescription";
			this.ValueMember	= "LanguageValue";
		}

		//---------------------------------------------------------------------
		public void ClearItems()
		{
			this.DataSource = null;
			this.Items.Clear();
		}

		//---------------------------------------------------------------------
		public void SetISOCountry(string countrySelected)
		{
			try
			{
				if (countrySelected == null || countrySelected.Length == 0)
				{
					this.SelectedIndex = -1;
					return;
				}
				this.SelectedIndex = this.FindStringExact(countrySelected, -1);
			}
			catch
			{
				this.SelectedIndex = -1;
			}
		}
	}
}