using System;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Data.DatabaseLayer
{
	///<summary>
	/// Helper per il ritorno di info relative agli iso stati
	///</summary>
	//================================================================================
	public class IsoHelper
	{
		///<summary>
		/// Per sapere se un isostato prevede l'utilizzo del character set Unicode
		///</summary>
		//---------------------------------------------------------------------
		public static bool IsoExpectUnicodeCharSet(string isoState)
		{
			return
				IsChineseLicence(isoState) || IsJapaneseLicence(isoState) || IsHungarianLicence(isoState) ||
				IsPolishLicence(isoState) || IsSerbianLicence(isoState) || IsBulgarianLicence(isoState) ||
				IsTurkishLicence(isoState) || IsRomanianLicence(isoState) || IsSlovenianLicence(isoState) ||
				IsCroatianLicence(isoState) || IsGreekLicence(isoState);
		}

		/// <summary>
		/// ISOSTATO = IT
		/// </summary>
		//---------------------------------------------------------------------
		public static bool IsItalianLicence(string isoState)
		{
			return (string.Compare(isoState, NameSolverStrings.IsoStateItalian, StringComparison.InvariantCultureIgnoreCase) == 0);
		}

		/// <summary>
		/// ISOSTATO = CN (Cina)
		/// </summary>
		//---------------------------------------------------------------------
		public static bool IsChineseLicence(string isoState)
		{
			return (string.Compare(isoState, NameSolverStrings.IsoStateChinese, StringComparison.InvariantCultureIgnoreCase) == 0);
		}

		/// <summary>
		/// ISOSTATO = JP (Giappone)
		/// </summary>
		//---------------------------------------------------------------------
		public static bool IsJapaneseLicence(string isoState)
		{
			return (string.Compare(isoState, NameSolverStrings.IsoStateJapanese, StringComparison.InvariantCultureIgnoreCase) == 0);
		}

		/// <summary>
		/// ISOSTATO = HU (Ungheria)
		/// </summary>
		//---------------------------------------------------------------------
		public static bool IsHungarianLicence(string isoState)
		{
			return (string.Compare(isoState, NameSolverStrings.IsoStateHungarian, StringComparison.InvariantCultureIgnoreCase) == 0);
		}

		/// <summary>
		/// ISOSTATO = PL (Polonia)
		/// </summary>
		//---------------------------------------------------------------------
		public static bool IsPolishLicence(string isoState)
		{
			return (string.Compare(isoState, NameSolverStrings.IsoStatePolish, StringComparison.InvariantCultureIgnoreCase) == 0);
		}

		/// <summary>
		/// ISOSTATO = CS (Serbia e Montenegro)
		/// </summary>
		//---------------------------------------------------------------------
		public static bool IsSerbianLicence(string isoState)
		{
			return (string.Compare(isoState, NameSolverStrings.IsoStateSerbian, StringComparison.InvariantCultureIgnoreCase) == 0);
		}

		/// <summary>
		/// ISOSTATO = BG (Bulgaria)
		/// </summary>
		//---------------------------------------------------------------------
		public static bool IsBulgarianLicence(string isoState)
		{
			return (string.Compare(isoState, NameSolverStrings.IsoStateBulgarian, StringComparison.InvariantCultureIgnoreCase) == 0);
		}

		/// <summary>
		/// ISOSTATO = TR (Turchia)
		/// </summary>
		//---------------------------------------------------------------------
		public static bool IsTurkishLicence(string isoState)
		{
			return (string.Compare(isoState, NameSolverStrings.IsoStateTurkish, StringComparison.InvariantCultureIgnoreCase) == 0);
		}

		/// <summary>
		/// ISOSTATO = RO (Romania)
		/// </summary>
		//---------------------------------------------------------------------
		public static bool IsRomanianLicence(string isoState)
		{
			return (string.Compare(isoState, NameSolverStrings.IsoStateRomanian, StringComparison.InvariantCultureIgnoreCase) == 0);
		}

		/// <summary>
		/// ISOSTATO = SI (Slovenia)
		/// </summary>
		//---------------------------------------------------------------------
		public static bool IsSlovenianLicence(string isoState)
		{
			return (string.Compare(isoState, NameSolverStrings.IsoStateSlovenian, StringComparison.InvariantCultureIgnoreCase) == 0);
		}

		/// <summary>
		/// ISOSTATO = HR (Croazia)
		/// </summary>
		//---------------------------------------------------------------------
		public static bool IsCroatianLicence(string isoState)
		{
			return (string.Compare(isoState, NameSolverStrings.IsoStateCroatian, StringComparison.InvariantCultureIgnoreCase) == 0);
		}

		/// <summary>
		/// ISOSTATO = GR (Grecia)
		/// </summary>
		//---------------------------------------------------------------------
		public static bool IsGreekLicence(string isoState)
		{
			return (string.Compare(isoState, NameSolverStrings.IsoStateGreek, StringComparison.InvariantCultureIgnoreCase) == 0);
		}
	}
}
