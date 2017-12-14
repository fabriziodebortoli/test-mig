using System;

namespace Microarea.TaskBuilderNet.Licence.Activation.Components
{
	/// <summary>
	/// Fornisce servizi per la formattazione di una quantita' da espressa in
	/// base 10 a espressa in base 36 e vicecersa.
	/// </summary>
	//=========================================================================
	public sealed class ProgrNumGen
	{
		private static System.Collections.ArrayList cache;

		//---------------------------------------------------------------------
		private ProgrNumGen()
		{}

		//---------------------------------------------------------------------
		private static void InitCache()
		{
			cache = new System.Collections.ArrayList(
				new string[]{
				"0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C",
				"D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P",
				"Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z"}
			);
		}

		/// <summary>
		/// Ritorna una stringa contenente il valore espresso in base 36
		/// dell'intero decimale preso in ingresso.
		/// </summary>
		//---------------------------------------------------------------------
		public static string GetBase36Progressive(int number)
		{
			if (cache == null)
				InitCache();

			System.Text.StringBuilder strValueBuilder =
				new System.Text.StringBuilder();

			int countBase	= cache.Count;
			int rest		= 0;
			while (number > countBase - 1)
			{
				rest = number % countBase;
				number = number / countBase;
				strValueBuilder.Insert(0, cache[rest]);
			}
			strValueBuilder.Insert(0, cache[number]);

			return strValueBuilder.ToString();
		}

		/// <summary>
		/// Ritorna un intero decimale contenente il valore contenuto nella
		/// stringa in ingresso recante un valore in base 36.
		/// </summary>
		//---------------------------------------------------------------------
		public static int GetBase10Progressive(string strValue)
		{
			if (cache == null)
				InitCache();

			int val = 0;
			int countBase	= cache.Count;
			for (int i = 0; i < strValue.Length; i++)
				val += (int)(cache.IndexOf(strValue[i].ToString()) *
						Math.Pow(countBase, (strValue.Length - 1 - i)));

			return val;
		}
	}
}
