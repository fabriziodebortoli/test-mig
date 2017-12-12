using System;
using System.Collections;
using System.Globalization;

namespace Microarea.TaskBuilderNet.Licence.Activation
{
	/// <summary>
	/// Fornisce servizi per la conversione di stringhe.
	/// </summary>
	public class Converter
	{
		/// <summary>
		/// Ritorna una stringa contenente la rappresentazione esadecimale dell'intero in ingresso.
		/// </summary>
		/// <param name="decValue">stringa contenente il valore decimale da convertire.</param>
		/// <returns>stringa contenente il valore esadecimale.</returns>
		//---------------------------------------------------------------------------------------------------
		public static string GetHexadecimalString(int decimalValue)
		{
			string[] conversion =
				{"0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F"};
			string	hexadecimal	= string.Empty;
			int		rest;

			do
			{
				rest			=	decimalValue % 16;
				decimalValue	=	decimalValue / 16;
				hexadecimal		=	conversion[rest] + hexadecimal;
			}
			while (decimalValue > 15);

			hexadecimal = conversion[decimalValue] + hexadecimal;
			return hexadecimal;
		}

		/// <summary>
		/// Ritorna la rappresentazione in stringa esadecimale di un array di byte se la conversione va a
		/// buon fine, stringa vuota altrimenti.
		/// </summary>
		//---------------------------------------------------------------------------------------------------
		public static string GetHexadecimalString(byte[] data)
		{
			string	result	= string.Empty;
			int		temp	= -1;
			
			for (int i = 0; i < data.Length; i++)
			{
				try
				{
					temp = Convert.ToInt32(data[i]);
				}
				catch
				{
					throw new Exception("Data not valid:" + data[i].ToString(CultureInfo.InvariantCulture));
				}

				result += GetHexadecimalString(temp);
			}

			if (result.Length != ( 2 * data.Length ) )
				return "";
			else
				return result;
		}

		/// <summary>
		/// Ritorna la rappresentazione in byte di una stringa secondo il formato UTF-16.
		/// </summary>
		//---------------------------------------------------------------------------------------------------
		public static byte[] GetBytes(string str)
		{
			System.Text.UnicodeEncoding byteConverter = new System.Text.UnicodeEncoding();
			return byteConverter.GetBytes(str);
		}

		/// <summary>
		/// Trasforma la stringa in ingresso contenente un valore esadecimale nel corrispondente
		/// array di byte se la conversione va a buon fine, ritorna un array composto di un solo byte 
		/// posto a 0 altrimenti.
		/// </summary>
		//---------------------------------------------------------------------------------------------------
		public static byte[] GetByteArrayFromHexadecimalString(string hexadecimalString)
		{
			// Traduce da formato esadecimale a formato decimale.
			// N.B.:non mi preoccupo qui se la stringa abbia lunghezza dispari perche' deve
			//      trattare solo activation key che hanno lunghezza di 256 caratteri e ogni
			//      tentativo di gestire stringhe di lunghezza dispari porterebbe inutili complicazioni
			//      nel meccanismo di controllo delle chiavi di attivazione.

			int			dim					= (hexadecimalString.Length / 2);
			string[]	hexadecimalValues	= new string[dim];

			for (int i = 0; i < dim; i++)
				hexadecimalValues[i] = hexadecimalString.Substring((2 * i), 2);

			// Crea l'array di byte.
			bool	ok			= true;
			byte[]	signedData	= new byte[dim];
			byte	temp		= Convert.ToByte(0);
			int		counter		= 0;
			for (int i = 0; i < dim; i++)
			{
				try
				{
					temp = Convert.ToByte(Converter.GetDecimalValue(hexadecimalValues[i]));
				}
				catch
				{
					ok = false;
				}
				if (ok)
				{
					signedData[i] = temp;
					counter++;
				}
				else break;
			}

			if ( counter != (hexadecimalString.Length / 2) )
			{
				byte[] result	= new byte[1];
				result[0]		= Convert.ToByte(0);
				return result;
			}
			else return signedData;
		}

		/// <summary>
		/// Ritorna il valore decimale della stringa che esprime un valore esadecimale in ingresso
		/// se la conversione va a buon fine, -1 altrimenti.
		/// </summary>
		/// <param name="hexaDecimalValue">stringa contenente il valore esadecimale da convertire.</param>
		/// <returns>valore decimale.</returns>
		//---------------------------------------------------------------------------------------------------
		public static int GetDecimalValue(string hexaDecimalValue)
		{
			string[] s =
					{"0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F"};
			ArrayList	conversion		= new ArrayList(s);				
			int			decimalValue	= 0;
			int			temp			= 0;

			for (int i = 0; i < hexaDecimalValue.Length; i++)
			{
				temp = conversion.IndexOf(hexaDecimalValue[i].ToString());
				if ( temp > -1)
					decimalValue	+= temp * ( (int) Math.Pow( 16, ( ( hexaDecimalValue.Length - 1 ) - i ) ) );
				else
				{
					decimalValue = -1;
					break;
				}
			}
			if ( !( decimalValue < 0 ) )
				return decimalValue;
			else return -1;
		}
	}
}
