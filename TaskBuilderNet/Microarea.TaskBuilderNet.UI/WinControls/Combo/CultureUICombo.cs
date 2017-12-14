using System;
using System.Collections;
using System.Globalization;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.TaskBuilderNet.UI.WinControls.Combo
{
	/// <summary>
	/// CultureUICombo.
	/// Mostra la Combo con l'elenco delle lingue installate
	/// </summary>
	/// =======================================================================
	public class CultureUICombo : System.Windows.Forms.ComboBox
	{
		# region Properties
		
		//---------------------------------------------------------------------
		public string SelectedLanguageValue
		{
			get
			{
				if (this.SelectedItem == null || !(this.SelectedItem is LanguageItem))
					return String.Empty;

				return ((LanguageItem)this.SelectedItem).LanguageValue;
			}
		}
		# endregion

		# region Constructor
		//---------------------------------------------------------------------
		public CultureUICombo()
		{
			this.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		}
		# endregion

		# region LoadLanguages (caricamento delle lingue installate + overload)
		/// <summary>
		/// load delle lingue installate (compresa la Invariant Language (ovvero stringa vuota))
		/// </summary>
		//---------------------------------------------------------------------
		public void LoadLanguagesUI()
		{
			LoadLanguagesUI(false);
		}
		
		/// <summary>
		/// Carico le lingue installate (con o senza la Invariant Language)
		/// </summary>
		/// <param name="withoutInvariantLanguage">true: non carico la lingua vuota; false: metto anche la Invariant Language</param>
		//---------------------------------------------------------------------
		public void LoadLanguagesUI(bool withoutInvariantLanguage)
		{
			ArrayList languageValues		= new ArrayList();
			ArrayList languageDescriptions	= new ArrayList();
			ArrayList languagesPreferredList= new ArrayList();

			// aggiungo la lingua vuota (nessuna selezione) solo se mi viene specificato
			if (!withoutInvariantLanguage)
				languagesPreferredList.Add(new LanguageItem(string.Empty, string.Empty));

			GetInstalledDictionaries(out languageValues, out languageDescriptions);

			ClearItems();

			for (int i = 0; i < languageDescriptions.Count; i++)
			{
				LanguageItem language		 = new LanguageItem();
				language.LanguageDescription = languageDescriptions[i].ToString();
				language.LanguageValue		 = languageValues[i].ToString();
				languagesPreferredList.Add(language);
			}

			this.DataSource		= languagesPreferredList;
			this.DisplayMember	= "LanguageDescription";
			this.ValueMember	= "LanguageValue";
		}
		# endregion

		# region SetUILanguage (inizializzazione lingua selezionata)
		//---------------------------------------------------------------------
		public void SetUILanguage(string languageSelected)
		{
			try
			{
				if (languageSelected == null)
				{
					this.SelectedIndex = -1;
					return;
				}
			
				if (languageSelected.Length == 0)
				{
					this.SelectedIndex = 0;
					return;
				}

				CultureInfo uiLanguageName = new CultureInfo(languageSelected);
				this.SelectedIndex = this.FindStringExact(uiLanguageName.DisplayName, -1);
			}
			catch
			{
				this.SelectedIndex = -1;
			}
		}
		# endregion

		# region ClearItems
		///<summary>
		/// Svuotamento degli elementi dalla combo
		///</summary>
		//---------------------------------------------------------------------
		public void ClearItems()
		{
			this.DataSource = null;
			this.Items.Clear();
		}
		# endregion

		# region OnDropDown
		//---------------------------------------------------------------------
		protected override void OnDropDown(EventArgs e)
		{
			base.OnDropDown(e);
		}
		# endregion

		//---------------------------------------------------------------------
		private static void GetInstalledDictionaries(out ArrayList cultures, out ArrayList descriptions)
		{
			cultures = new ArrayList();
			descriptions = new ArrayList();

			//Aggiungo l'inglese che non esiste come dizionario
			CultureInfo ciEN = new CultureInfo("en");
			cultures.Add(ciEN.Name);
			descriptions.Add(ciEN.DisplayName);

			CultureInfo[] dictionaries = InstallationData.GetInstalledDictionaries();
			foreach (CultureInfo ci in dictionaries)
			{
				if (ci.Name == string.Empty)
					continue;

				cultures.Add(ci.Name);
				descriptions.Add(ci.DisplayName);
			}
		}
	}
}