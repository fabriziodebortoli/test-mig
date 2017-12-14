using System;

namespace Microarea.Library.SMHandlerDbInterface
{
	/// <summary>
	/// Fornisce servizi per criptare e decriptare stringhe
	/// mediante un algoritmo a chiave simmetrica noto come
	/// "Codice di Vigenere".
	/// </summary>
	public class PasswordCryptoServiceProvider
	{
		// Stringa che serve ad estrarre i valori per le operazioni di criptazione e decriptazione.
		private const string	matrix	=
			"0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz`~!@#$%^&*()-_=+{}[]:;',./?><\\\"";
							
		// Chiave privata in base alla quale viene la criptazione e la decriptazione.	
		private const string	key		= "IEhMSP1PM6PCCgEG";

		#region Private Methods

		// Inizializza la chiave generando una stringa lunga come la stringa da criptare ripetendo
		// il valore della chiave.
		// Es.:	key = "TEST";
		//		lunghezza della stringa da criptare = 30;
		// allora key diventa:	key = "TESTTESTTESTTESTTESTTESTTESTTE".
		//---------------------------------------------------------------------------------------------------
		private static string InitializeKey(string plainValue)
		{
			string workingKey = string.Empty;
			while(workingKey.Length < plainValue.Length)
				workingKey += key;

			return workingKey.Substring(0, plainValue.Length);
		}

		#endregion

		#region Public Methods

		//---------------------------------------------------------------------------------------------------
		public static string Encrypt(string plainValue)
		{
			// Inizializza la chiave.
			string workingKey	= InitializeKey(plainValue);
			
			// Cripta.
			string	encryptedValue	= string.Empty;
			int normalizedIndex		= 0;
			int plainValueItemIndex = -1;
			for (int i = 0; i < plainValue.Length; i++)
			{
				plainValueItemIndex = matrix.IndexOf(plainValue[i]);
				if (plainValueItemIndex == -1)
					throw new SystemException(	"Character not valid in \"plainValue\" parameter: " +
						plainValue[i]	);

				normalizedIndex = plainValueItemIndex + matrix.IndexOf(workingKey[i].ToString());

				if (normalizedIndex > (matrix.Length - 1))
					normalizedIndex -= matrix.Length;

				encryptedValue += matrix[normalizedIndex];	
			}			

			return encryptedValue;
		}
		#endregion
	}
}

