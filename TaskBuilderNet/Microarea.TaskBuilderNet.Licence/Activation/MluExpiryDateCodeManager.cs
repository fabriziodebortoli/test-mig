using System;
using System.Globalization;
using System.Text;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.TaskBuilderNet.Licence.Activation
{
	//=========================================================================
	/// <summary>
	/// Gestisce un codice formato da:
	/// 2 caratteri casuali
	/// 4 caratteri che esprimono una data come numero di giorni trascorsi dal 1/1/2000
	/// 1 carattere per il check digit calcolato ispirandosi all'algoritmo per l'ISBN-10
	/// </summary>
	public class MluExpiryDateCodeManager
	{
		static readonly DateTime theDate = new DateTime(2000, 1, 1);
		const int randomCodeOffset = 1;
		const string allowedChars = "234679ACDEFGHJKLMNPQRTUVWXYZ";

		DateTime mluExpiryDate;
		bool cancelled;
		int generatedCode;

		public DateTime MluExpiryDate { get { return mluExpiryDate; } }
		public bool Cancelled { get { return cancelled; } }

		//---------------------------------------------------------------------
		public string CryptedMluExpiryDate
		{
			get
			{
				return CryptMluExpiryDate(mluExpiryDate, cancelled);
			}
		}

		//---------------------------------------------------------------------
		public static string CryptMluExpiryDate(DateTime mluExpiryDate, bool cancelled)
		{
			return Crypto.Encrypt(
				mluExpiryDate.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture) +
				cancelled.ToString(CultureInfo.InvariantCulture)
				);
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Genera un codice casuale di 5 cifre numeriche
		/// </summary>
		public int GenerateCode()
		{
			Random rnd = new Random();
			generatedCode = rnd.Next(10000, 99999);

			return generatedCode;
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Genera un codice alfanumerico di 5 caratteri in cui è codificata 
		/// una data e in cui sono codificate anche due cifre inviate dal 
		/// mittente per rendere il processo tempovariante in funzione del
		/// codice generato dal mittente.
		/// Altrimenti, a parità di data, una volta ricevuto un sms con un 
		/// codice valido l'utente potrebbe riproporlo tutte le volte senza
		/// più interpellare il nostro servizio.
		/// </summary>
		public string GenerateCheck(DateTime dateTime, bool cancelled, int senderCode)
		{
			int code = GetCodeFromDateTime(dateTime);

			string strCode =
				senderCode.ToString(CultureInfo.InvariantCulture).ToUpperInvariant().Substring(0, randomCodeOffset) +
				(cancelled ? "1" : "0") +
				code.ToString(CultureInfo.InvariantCulture);

			strCode += CalculateCheckDigit(strCode, 2).ToString(CultureInfo.InvariantCulture);

			return ChangeFace(strCode).ToUpperInvariant();
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Un codice è buono se il check digit è valido e se i primi due
		/// caratteri del codice sono presenti anche nel controcodice
		/// </summary>
		public bool CheckCode(string code)
		{
            if (String.IsNullOrEmpty(code)) return false;

			string strCode = RestoreFace(code.ToUpperInvariant()).ToString(CultureInfo.InvariantCulture);

			string randomCode = strCode.Substring(0, randomCodeOffset);

			if (strCode.Substring(randomCodeOffset, 1) == "1")
			{
				cancelled = true;
			}
			else
			{
				cancelled = false;
			}

			int dateCode = Int32.Parse(strCode.Substring(randomCodeOffset + 1, strCode.Length - randomCodeOffset - 1 - 1));//l'ulteriore -1 considera il check digit
			string checkDigit = strCode.Substring(strCode.Length - 1);

			mluExpiryDate = GetDateTimeFromCode(dateCode);

			return
				(CalculateCheckDigit(strCode, 1) % (strCode.Length + 1)) == 0 &&
				String.Compare(
					randomCode,
					generatedCode.ToString(CultureInfo.InvariantCulture).Substring(0, randomCodeOffset),
					StringComparison.OrdinalIgnoreCase
				) == 0;
		}

		//---------------------------------------------------------------------
		private static int CalculateCheckDigit(string dateCode, int offset)
		{
			//Ispirato al codice di controllo dell'ISBN-10 http://it.wikipedia.org/wiki/ISBN
			int sum = 0;
			int top = dateCode.Length + offset;
			for (int i = 0; i < dateCode.Length; i++)
			{
				sum += Int32.Parse(dateCode[i].ToString()) * (top - i - 1);
			}

			sum = sum % top;

			return top - sum;
		}

		//---------------------------------------------------------------------
		private int GetCodeFromDateTime(DateTime dateTime)
		{
			TimeSpan fromTheDateToDate = dateTime - theDate;

			return Convert.ToInt32(Math.Ceiling(fromTheDateToDate.TotalDays));
		}

		//---------------------------------------------------------------------
		private DateTime GetDateTimeFromCode(int dateCode)
		{
			TimeSpan fromTheDateToDate = new TimeSpan(dateCode, 0, 0, 0);

			return theDate + fromTheDateToDate;
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Cambia il volto ad una stringa di caratteri numerici esprimendola
		/// con i caratteri presenti in allowedChars
		/// </summary>
		private static string ChangeFace(string code)
		{
			int actual = 0;
			if (!Int32.TryParse(code, out actual))
			{
				throw new ArgumentException("Invalid code :" + code);
			}

			if (actual == 0)
			{
				return allowedChars[0].ToString(CultureInfo.InvariantCulture);
			}

			int rest = 0;
			int @base = allowedChars.Length;
			StringBuilder faceBuilder = new StringBuilder();
			while (actual > 0)
			{
				rest = actual % @base;
				actual = actual / @base;

				faceBuilder.Append(allowedChars[rest]);
			}

			return faceBuilder.ToString().Reverse();
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Cambia il volto ad una stringa di caratteri presenti in allowedChars
		/// esprimendola con i caratteri numerici
		/// </summary>
		private static int RestoreFace(string code)
		{
			if (code == null || code.Trim().Length == 0)
				return 0;

			string reversedCode = code.Reverse();

			int actual = 0;
			int @base = allowedChars.Length;
			for (int i = 0; i < reversedCode.Length; i++)
			{
				actual += (int)(Math.Pow(@base, i) * allowedChars.IndexOf(reversedCode[i]));
			}

			return actual;
		}
	}
}
