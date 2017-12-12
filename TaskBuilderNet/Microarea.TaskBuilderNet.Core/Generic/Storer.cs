/*
Classe dedicata alla crittografia molto blanda della pwd indicata nelle credenziali del proxy.
Nessun merito personale, avevo fretta e l'ho copiata e modificata solo in modo minimale da
http://groups.google.com/groups?hl=it&lr=&ie=UTF-8&threadm=rscfhus6sro3khqj9npldnfm64vslb4mkk%404ax.com&rnum=26&prev=/groups%3Fas_q%3Dstrings%2520Rijndael%26ie%3DUTF-8%26as_ugroup%3Dmicrosoft.public.dotnet*%26lr%3D%26num%3D50%26hl%3Dit
Tra le modifiche effettuate, ho aggiunto un Base64 encoding alla stringa criptata in modo
da poterla scrivere in un messaggio soap senza sollevare eccezioni.
Ho chiamato la classe in tale modo e rinominato i metodi per sviare un po'.
In pratica cripta delle stringhe restituendo delle stringhe reversibili.
Usa un algoritmo a chiave simmetrica (RijndaelManaged).
NOTA: non mettere nei commenti xml info che chiariscano la natura dell'oggetto.
*/
using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Microarea.TaskBuilderNet.Core.Generic
{
	public class Storer
	{
		private static byte[] key;
		private static byte[] initializationVector;

		//---------------------------------------------------------------------
		static Storer()
		{
			key						= StringToByteArray("12345678");
			// must be at least 8 (change keysize for more bits)

			initializationVector	= StringToByteArray("1234567890123456");
			// must be at least 16
		}
	
		//---------------------------------------------------------------------
		public static string Store(string toStore)
		{
			string result = string.Empty;
			SymmetricAlgorithm sa = SymmetricAlgorithm.Create();
			MemoryStream ms = new MemoryStream();
			CryptoStream encStream = 
				new CryptoStream(ms, sa.CreateEncryptor(key, initializationVector), CryptoStreamMode.Write);

			byte[] buffer = StringToByteArray(toStore);
			encStream.Write(buffer, 0, toStore.Length);
			encStream.FlushFinalBlock();
			encStream.Flush();
			encStream.Close();
			result = MemoryStreamToString(ms);
			ms.Close();

			// aggiungo un Base64 encoding per poterlo mettere su xml e passarlo via soap
			result = Convert.ToBase64String(StringToByteArray(result));

			return result;
		}         
    
		//---------------------------------------------------------------------
		public static string Unstore(string toUnstore)
		{
			string result = string.Empty;
			try
			{
				// prima tolgo il Base64 encoding
				toUnstore = ByteArrayToString(Convert.FromBase64String(toUnstore));

				// poi applico il crypting
				SymmetricAlgorithm sa = SymmetricAlgorithm.Create();
				MemoryStream ms = new MemoryStream();
				CryptoStream encStream = 
					new CryptoStream(ms, sa.CreateDecryptor(key, initializationVector), CryptoStreamMode.Write);

				byte[] buffer = StringToByteArray(toUnstore);
				encStream.Write(buffer, 0, toUnstore.Length);
				encStream.FlushFinalBlock();
				encStream.Flush();
				encStream.Close();
				result = MemoryStreamToString(ms);
				ms.Close();
			}
			catch (Exception exc)
			{
				Debug.WriteLine(exc.Message);
				return string.Empty;
			}
			return result;
		}

		#region Helper methods
		//---------------------------------------------------------------------
		private static byte[] StringToByteArray(string s)
		{
			byte[] result = new byte[s.Length];
			for (int i=0; i<s.Length; i++)
				result[i] = (byte)s[i];
			return result;
		}

		//---------------------------------------------------------------------
		private static string MemoryStreamToString(MemoryStream source)
		{
			string result = string.Empty;
			result = ByteArrayToString(source.ToArray());
			return result;
		}

		//---------------------------------------------------------------------
		private static string ByteArrayToString(byte[] source)
		{
			string result = string.Empty;
			StringBuilder resultBuilder = new StringBuilder(source.Length);
			foreach (byte b in source)
				resultBuilder.Append((char)b);
			result = resultBuilder.ToString();
			return result;
		}
		#endregion

		//---------------------------------------------------------------------
	}
}
