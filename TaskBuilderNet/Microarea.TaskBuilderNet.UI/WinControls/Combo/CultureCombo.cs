using System;
using System.Collections;
using System.Globalization;

namespace Microarea.TaskBuilderNet.UI.WinControls.Combo
{
	/// <summary>
	/// CultureCombo.
	/// Lingue della Culture 
	/// </summary>
	// ========================================================================
	public class CultureCombo : System.Windows.Forms.ComboBox
	{
		private bool showNative = false;

		//---------------------------------------------------------------------
		public CultureCombo()
		{
			this.Text = null;
			this.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		}

		//---------------------------------------------------------------------
		protected override void OnDropDown(EventArgs e)
		{
			base.OnDropDown (e);
		}

		//---------------------------------------------------------------------
		public string ApplicationLanguage
		{
			get 
			{
				if (this.SelectedItem == null || !(this.SelectedItem is LanguageItem))
					return String.Empty;

				return ((LanguageItem)this.SelectedItem).LanguageValue; 
			}
			set
			{
				try
				{
					if (value == null || value.Length == 0)
					{
						this.SelectedIndex = -1;
						return;
					}

					CultureInfo uiLanguageName = new CultureInfo(value);
					if (showNative)
						this.SelectedIndex = this.FindStringExact(uiLanguageName.NativeName, -1);
					else
						this.SelectedIndex = this.FindStringExact(uiLanguageName.DisplayName, -1);
				}
				catch
				{
					this.SelectedIndex = -1;
				}
			}
		}

		/// <summary>
		/// load delle lingue  (compresa la invariant language (ovvero stringa vuota))
		/// </summary>
		//---------------------------------------------------------------------
		public void LoadLanguages()
		{
			LoadLanguages(false, false);
		}

		//---------------------------------------------------------------------
		public void LoadNativeLanguages()
		{
			LoadLanguages(false, true);
		}

		//---------------------------------------------------------------------
		public void LoadLanguages(bool withoutInvariantLanguage, bool nativeName)
		{
			showNative = nativeName;
			string toremember = this.ApplicationLanguage;
			ClearItems();

			ArrayList languagesApplicationList = new ArrayList();

			//aggiungo la lingua vuota (nessuna selezione) solo se mi viene specificato
			if (!withoutInvariantLanguage)
				languagesApplicationList.Add(new LanguageItem(string.Empty, string.Empty));

			CultureInfo[] cults = CultureInfo.GetCultures(CultureTypes.SpecificCultures);

			for (int i = 0; i < cults.Length; i++)
			{
				LanguageItem currentLanguage = new LanguageItem();
				if (showNative)
					currentLanguage.LanguageDescription = cults[i].NativeName;
				else
					currentLanguage.LanguageDescription = cults[i].DisplayName;
				currentLanguage.LanguageValue = cults[i].Name;
				languagesApplicationList.Add(currentLanguage);
			}

			IComparer comparer = new LanguagesSort();
			languagesApplicationList.Sort(comparer);

			this.DisplayMember	= "LanguageDescription";
			this.ValueMember	= "LanguageValue";
			this.DataSource		= languagesApplicationList;
			
			this.ApplicationLanguage = toremember;
		}

		//---------------------------------------------------------------------
		public void ClearItems()
		{
			this.DataSource = null;
			this.Items.Clear();
		}
	}
}
