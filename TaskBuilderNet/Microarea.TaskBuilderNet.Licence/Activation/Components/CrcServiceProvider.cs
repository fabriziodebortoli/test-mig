using System;
using System.Globalization;

namespace Microarea.TaskBuilderNet.Licence.Activation.Components
{
	/// <summary>
	/// Fornisce servizi per il calcolo del CRC su di una stringa
	/// lavorando con caratteri stampabili
	/// </summary>
	//=========================================================================
	internal class CrcServiceProvider
	{
		//---------------------------------------------------------------------
		private CrcServiceProvider()
		{}

		/// <summary>
		/// Comupta il CRC della stringa in ingresso e restituisce la
		/// stringa col carattere del CRC in testa.
		/// </summary>
		//---------------------------------------------------------------------
		public static string ComputeCrc(string toBeChecksummed)
		{
			return ComputeCrc(toBeChecksummed, true);
		}

		/// <summary>
		/// Comupta il CRC della stringa in ingresso.
		/// </summary>
		/// <returns>
		/// La stringa passata con il carattere del CRC in testa se atTheBegin
		/// vale true, in coda altrimenti.
		/// </returns>
		//---------------------------------------------------------------------
		public static string ComputeCrc(string toBeChecksummed, bool atTheBegin)
		{
			string checksummed = "";
			if (atTheBegin)
			{
				checksummed = String.Format(
								CultureInfo.InvariantCulture,
								"{0}{1}",
								Crc.ComputeCrc(toBeChecksummed).ToString(),
								toBeChecksummed
								);
			}
			else
			{
				checksummed = String.Format(
								CultureInfo.InvariantCulture,
								"{0}{1}",
								toBeChecksummed,
								Crc.ComputeCrc(toBeChecksummed).ToString()
								);
			}

			return checksummed;
		}

		/// <summary>
		/// Controlla il CRC per la stringa passata come parametro cercando un
		/// CRC di un carattere di lunghezza posto in testa alla stringa.
		/// </summary>
		/// <returns>true se il CRC e' corretto, false altrimenti.</returns>
		//---------------------------------------------------------------------
		public static bool IsCrcCorrect(string toBeControlled)
		{
			return IsCrcCorrect(toBeControlled, 0, 1);
		}

		/// <summary>
		/// Controlla il CRC per la stringa passata come parametro cercando un
		/// CRC di crcLen caratteri di lunghezza posto in crcPos.
		/// </summary>
		/// <returns>/// true se il CRC e' corretto, false altrimenti.</returns>
		//---------------------------------------------------------------------
		public static bool IsCrcCorrect(string toBeControlled, int crcPos, int crcLen)
		{
			string checksum = toBeControlled.Substring(crcPos, crcLen);
			toBeControlled = toBeControlled.Remove(crcPos, crcLen);

			char currCrc = Crc.ComputeCrc(toBeControlled);

			return currCrc.ToString().Equals(checksum);
		}
	}
}
