using System;
using System.Collections;
using System.Globalization;

namespace Microarea.TaskBuilderNet.UI.WinControls.Combo
{
	/// <summary>
	/// LanguageItem.
	/// Elemento i-esimo delle combo delle lingue
	/// </summary>
	// ========================================================================
	public class LanguageItem
	{
		//---------------------------------------------------------------------
		private string languageDescription	= string.Empty;
		private string languageValue		= string.Empty;

		//---------------------------------------------------------------------
		public string LanguageDescription {	get { return languageDescription;} set { languageDescription = value;}}
		public string LanguageValue		  {	get { return languageValue;		 } set { languageValue       = value;}}

		//---------------------------------------------------------------------
		public LanguageItem()
		{
			LanguageDescription = string.Empty;
			LanguageValue		= string.Empty;
		}

		//---------------------------------------------------------------------
		public LanguageItem(string languageText, string languageValue)
		{
			LanguageDescription = languageText;
			LanguageValue		= languageValue;
		}
	}

	# region Sorting Languages
	//============================================================================
	public class LanguagesSort : IComparer
	{
		//---------------------------------------------------------------------------
		int IComparer.Compare(Object lang1, Object lang2)
		{
			return (new CaseInsensitiveComparer(CultureInfo.InvariantCulture)).Compare
				(
				((LanguageItem)lang1).LanguageDescription, 
				((LanguageItem)lang2).LanguageDescription
				);
		}
	}
	# endregion
}
