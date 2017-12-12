using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Microarea.TaskBuilderNet.Licence.Activation.Components
{
	/// <summary>
	/// Fornisce i servizi di criptazione e decriptazione dei dati
	/// basandosi su un algoritmo a chiave privata noto come "Codice di Vigenere".
	/// </summary>
	//=======================================================================================================
	internal class SNCryptoServiceProvider
	{
		// Matrice che serve ad estrarre i valori per le operazioni di criptazione e decriptazione.
		// Raccoglie tutti i caratteri ammessi per la generazione dei serial number.
		private static string	matrix	= GetValue(new byte[]{	66, 178, 177, 139, 186, 36, 214, 177, 225, 5, 245, 205, 125, 117,
											  189, 239, 198, 86, 201, 3, 84, 133, 218, 186, 107, 69, 129, 169,
											  254, 227, 161, 189, 178, 28, 16, 149, 80, 82, 154, 216	});
							
		// Chiave privata in base alla quale avviene la criptazione e la decriptazione.	
		private static string	key		= GetValue(new byte[]{81, 72, 159, 60, 188, 136, 47, 20, 223, 195, 112, 178,
											  171, 7, 112, 242});

		#region Private Methods

		//---------------------------------------------------------------------------------------------------
		private static string GetValue(byte[] param)
		{
			byte[] gen1 = new byte[]{	12, 25, 255, 15, 69, 125, 33, 201, 26, 95,
										167, 215, 25, 105, 29, 165, 38, 101, 200, 100,
										1, 55, 155, 19};
			byte[] gen2 =  new byte[]{	132, 235, 255, 15, 69, 125, 33, 201	};

			System.Text.ASCIIEncoding byteConverter = new System.Text.ASCIIEncoding();
			TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();

			MemoryStream outputFrontEndStream = new MemoryStream();
				
			CryptoStream stream =
				new CryptoStream
				(outputFrontEndStream, tdes.CreateDecryptor(gen1, gen2),CryptoStreamMode.Write);

			stream.Write(param, 0, param.Length);
			stream.Close();

			return byteConverter.GetString(outputFrontEndStream.ToArray());
		}

		// Inizializza la chiave generando una stringa lunga come la stringa da criptare ripetendo
		// il valore della chiave.
		// Es.:	key = "TEST";
		//		lunghezza della stringa da criptare = 30;
		// allora key diventa:	key = "TESTTESTTESTTESTTESTTESTTESTTE".
		//---------------------------------------------------------------------------------------------------
		private static string InitializeKey(string plainSN)
		{
			int length = plainSN.Length;
			StringBuilder workingKeyBuilder = new StringBuilder();
			while(workingKeyBuilder.Length < length)
				workingKeyBuilder.Append(key);

			return workingKeyBuilder.Remove(length, workingKeyBuilder.Length - length).ToString();
		}

		// Prepara la stringa per essere criptata e decriptata: mischia i caratteri del
		// Serial Number secondo una logica prefissata.
		//---------------------------------------------------------------------------------------------------
		private static string PrepareSN(string workingString)
		{
			string[] workingSN	= new string[workingString.Length];
			int index			= 0;
			int length			= workingString.Length;

			for (int i = 0; i < length; i++)
			{
				index = i * 4;
				while (index > (length - 1))
					index = index - length + 1;

				workingSN[index] = workingString[i].ToString();
			}

			StringBuilder workingStringBuilder = new StringBuilder();

			for (int i = 0; i < length; i++)
				workingStringBuilder.Append(workingSN[i]);

			return workingStringBuilder.ToString().ToUpperInvariant();
		}

		#endregion

		#region Public Methods

		//---------------------------------------------------------------------------------------------------
		public static string CryptSN(string plainSN)
		{
			// Prepara la stringa per essere criptata ed inizializza la chiave.
			plainSN				= PrepareSN(plainSN);
			string workingKey	= InitializeKey(plainSN);
			
			// Cripta il serial number.
			StringBuilder	encryptedSNBuilder = new StringBuilder(16);
			int normalizedIndex	= 0;
			for (int i = 0; i < plainSN.Length; i++)
			{
				normalizedIndex = matrix.IndexOf(plainSN[i]) + matrix.IndexOf(workingKey[i].ToString());

				if (normalizedIndex > (matrix.Length - 1))
					normalizedIndex -= matrix.Length;

				encryptedSNBuilder.Append(matrix[normalizedIndex]);
			}			

			return encryptedSNBuilder.ToString();
		}

		//---------------------------------------------------------------------------------------------------
		public static string DecryptSN(string encryptedSN)
		{
			string workingKey = InitializeKey(encryptedSN);			

			// Decripta il serial number.
			StringBuilder plainSNBuilder = new StringBuilder(16);
			int		normalizedIndex	= 0;
			for (int i = 0; i < encryptedSN.Length; i++)
			{
				normalizedIndex = matrix.IndexOf(encryptedSN[i]) - matrix.IndexOf(workingKey[i].ToString());

				if (normalizedIndex < 0)
					normalizedIndex += matrix.Length;

				plainSNBuilder.Append(matrix[normalizedIndex]);
			}

			return PrepareSN(plainSNBuilder.ToString());
		}

		#endregion
	}
}
