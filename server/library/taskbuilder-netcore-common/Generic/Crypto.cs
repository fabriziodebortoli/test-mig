using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;

namespace Microarea.Common.Generic
{
	/// <summary>
	/// Classe per la crittazione di stringhe
	/// </summary>
	//=========================================================================
	public class Crypto
	{
		private const string defaultCryptKey	= "IlariaAnnaBrunaMarcolinoTroppoCarinoNadia";
		private const string defaultCryptBlock	= "GiustiBalestriCalandriniBauzoneManzoni";

		public const string DesAlgorithmName	= "DES";
        /*
        IMPORATNTE< da .netFramework a .netCORE potrebbe cambiare:
        http://stackoverflow.com/questions/38333722/how-to-use-rijndael-encryption-with-a-net-core-class-library-not-net-framewo
         */

        //---------------------------------------------------------------------
        public static byte[] ToByteArray(string text)
		{
			return ToByteArray(text, 16);
		}

		/// <summary>
		/// Converte una stringa in un array di 8 byte
		/// </summary>
		/// <param name="text">stringa da convertire</param>
		/// <returns>array di byte</returns>
		//---------------------------------------------------------------------
		public static byte[] ToByteArray(string text, int thisSize)
		{
			//Convert Bits to Bytes
			byte[] thisIV = new byte[thisSize];

			if (text.Length < 1)
				return thisIV;
            
			int temp;
			int lastBound = text.Length;
			if (lastBound > thisSize)
				lastBound = thisSize;

			for (temp = 0 ; temp < lastBound ; temp++)
				thisIV[temp] = Convert.ToByte(text[temp]);
            
			return thisIV;
		}

		/// <summary>
		/// Crea un'istanza di un provider di crittazione
		/// </summary>
		/// <param name="algoritmName">l'algoritmo di cripting da utilizzare</param>
		/// <param name="key">chiave di crittazione</param>
		/// <param name="ivBlock">blocco di crittazione</param>
		/// <returns>Algoritmo simmetrico</returns>
		//---------------------------------------------------------------------
		public static SymmetricAlgorithm Init(string algoritmName, byte[] key, byte[] ivBlock)
		{
			SymmetricAlgorithm symmetricAlgorithm = null;

			try
			{
                //DIVERSO!!/*/*/*/*/*/*/*/*/*/
                /*prima era des, Rijndael, pare che rijndael sia come aes se il block è 16 bit, quindi per noi no , tutti i cypt cambieranno.
                symmetricAlgorithm = SymmetricAlgorithm.Create(algoritmName);*/
                symmetricAlgorithm = Aes.Create(/*algoritmName TODO rsweb*/);
              

                if (symmetricAlgorithm == null)
				{
					Debug.WriteLine("Crypto.Init: Errore creazione algoritmo di Rijndael");
					return null;
				}
			}
			catch(Exception err)
			{
				Debug.WriteLine(err.Message);
				return null;
			}

			try
			{
				symmetricAlgorithm.BlockSize =	ivBlock.Length * 8;
				symmetricAlgorithm.KeySize =	key.Length * 8;
			}
			catch (Exception err)
			{
				Debug.WriteLine(err.Message);
				return null;
			}
			return symmetricAlgorithm;
		}

		/// <summary>
		/// Cripta una stringa utilizzando l'algoritmo a chiave simmetrica DES utilizzando le 
		/// cryptkey e cryptblock di default
		/// </summary>
		/// <param name="val">stringa da criptare</param>
		/// <returns>la stringa criptata</returns>
		//---------------------------------------------------------------------
		public static string Encrypt(string val)
		{
			return Encrypt(val, DesAlgorithmName, defaultCryptKey, defaultCryptBlock);
		}

		/// <summary>
		/// Cripta una stringa utilizzando l'algoritmo a chiave simmetrica DES
		/// </summary>
		/// <param name="val">stringa da criptare</param>
		/// <param name="key">chiave di crypt</param>
		/// <param name="ivBlock">blocco di crypt</param>
		/// <returns>la stringa criptata</returns>
		//---------------------------------------------------------------------
		public static string Encrypt(string val, string key, string block)
		{
			return Encrypt(val, DesAlgorithmName, key, block);
		}

		/// <summary>
		/// Cripta una stringa utilizzando l'algoritmo a chiave simmetrica DES
		/// </summary>
		/// <param name="buffer">array di byte da criptare</param>
		/// <param name="key">chiave di crypt</param>
		/// <param name="ivBlock">blocco di crypt</param>
		/// <returns>la stringa criptata</returns>
		//---------------------------------------------------------------------
		public static string Encrypt(byte[] buffer, string key, string block)
		{
			byte[] bkey		= Crypto.ToByteArray(key);
			byte[] ivbBlock	= Crypto.ToByteArray(block);

			return Encrypt(buffer, DesAlgorithmName, bkey, ivbBlock);
		}

		/// <summary>
		/// Cripta una stringa utilizzando un algoritmo a chiave simmetrica
		/// </summary>
		/// <param name="val">stringa da criptare</param>
		/// <param name="algoritmName">nome dell'algoritmo simmetrico prescelto</param>
		/// <param name="key">chiave di crypt</param>
		/// <param name="ivBlock">blocco di crypt</param>
		/// <returns>la stringa criptata</returns>
		//---------------------------------------------------------------------
		public static string Encrypt(string val, string algoritmName, string key, string block)
		{
			byte[] bkey		= Crypto.ToByteArray("1234567887654321");
			byte[] ivbBlock	= Crypto.ToByteArray("8765432112345678");

			return Encrypt(val, algoritmName, bkey, ivbBlock);
		}

		/// <summary>
		/// Cripta una stringa utilizzando un algoritmo a chiave simmetrica
		/// </summary>
		/// <param name="val">stringa da criptare</param>
		/// <param name="algoritmName">nome dell'algoritmo simmetrico prescelto</param>
		/// <param name="key">chiave di crypt</param>
		/// <param name="ivBlock">blocco di crypt</param>
		/// <returns>la stringa criptata</returns>
		//---------------------------------------------------------------------
		public static string Encrypt(string val, string algoritmName, byte[] key, byte[] ivBlock)
		{
			if (val == null || val.Length == 0 )
				return string.Empty;

			SymmetricAlgorithm cryptoProvider = Init(algoritmName, key, ivBlock);
			if (cryptoProvider == null)
				return string.Empty;
            
			MemoryStream ms = new MemoryStream();

			try
			{
				CryptoStream cs = new CryptoStream
					(
					ms, 
					cryptoProvider.CreateEncryptor(key, ivBlock), 
					CryptoStreamMode.Write
					);
				StreamWriter sw = new StreamWriter(cs);
				sw.Write(val);
				sw.Flush();
				cs.FlushFinalBlock();
				ms.Flush();
			}
			catch (Exception err)
			{
				Debug.WriteLine(err.Message);
				return string.Empty;
			}

            //DIVERSO!**-*-*-*-*-*-*-***-*-*-*-*-*--*-*-*-*-
            /*
            byte[] buf = new byte[256];
            buf = ms.GetBuffer();
            return Convert.ToBase64String(buf, 0, (int)ms.Length);
            */

            ArraySegment<byte> buf = new ArraySegment<byte>();
            ms.TryGetBuffer(out buf);
			return Convert.ToBase64String(buf.Array, 0, (int)ms.Length);
		}

		/// <summary>
		/// Cripta una stringa utilizzando un algoritmo a chiave simmetrica
		/// </summary>
		/// <param name="buffer">array di byte da criptare</param>
		/// <param name="algoritmName">nome dell'algoritmo simmetrico prescelto</param>
		/// <param name="key">chiave di crypt</param>
		/// <param name="ivBlock">blocco di crypt</param>
		/// <returns>la stringa criptata</returns>
		//---------------------------------------------------------------------
		public static string Encrypt(byte[] buffer, string algoritmName, byte[] key, byte[] ivBlock)
		{
			byte[] encrypted = EncryptToByteArray(buffer, algoritmName, key, ivBlock);
			return Convert.ToBase64String(encrypted, 0, (int)encrypted.Length);
		}

		/// <summary>
		/// Cripta una stringa utilizzando l'algoritmo a chiave simmetrica DES utilizzando le 
		/// cryptkey e cryptblock di default
		/// </summary>
		/// <param name="buffer">array di byte da criptare</param>
		/// <returns>array di byte criptato</returns>
		//---------------------------------------------------------------------
		public static byte[] EncryptToByteArray(byte[] buffer)
		{
			byte[] bkey = Crypto.ToByteArray(defaultCryptKey);
			byte[] ivbBlock = Crypto.ToByteArray(defaultCryptBlock);

			return EncryptToByteArray(buffer, DesAlgorithmName, bkey, ivbBlock);
		}

		//--------------------------------------------------------------------------------
		public static byte[] EncryptToByteArray(byte[] buffer, string algoritmName, byte[] key, byte[] ivBlock)
		{
			if (buffer == null || buffer.Length == 0 )
				return new byte[0];

			SymmetricAlgorithm cryptoProvider = Init(algoritmName, key, ivBlock);
			MemoryStream ms = new MemoryStream();

			try
			{
				CryptoStream cs = new CryptoStream
					(
					ms, 
					cryptoProvider.CreateEncryptor(key, ivBlock), 
					CryptoStreamMode.Write
					);
				BinaryWriter bw = new BinaryWriter(cs);
				bw.Write(buffer);
				bw.Flush();
				cs.FlushFinalBlock();
				ms.Flush();
			}
			catch (Exception err)
			{
				Debug.WriteLine(err.Message);
				return new byte[0];
			}
			
			byte[] outBytes = new byte[ms.Length];
			ms.Seek(0, SeekOrigin.Begin);
			ms.Read(outBytes, 0, (int)ms.Length);
			return outBytes;
		}

		/// <summary>
		/// Decripta una stringa utilizzando l'algoritmo a chiave simmetrica DES utilizzando le 
		/// cryptkey e cryptblock di default
		/// </summary>
		/// <param name="val">stringa da decriptare</param>
		/// <returns>la stringa decriptata</returns>
		//---------------------------------------------------------------------
		public static string Decrypt(string val)
		{
			byte[] bkey = Crypto.ToByteArray(defaultCryptKey);
			byte[] ivbBlock = Crypto.ToByteArray(defaultCryptBlock);
			
			return Decrypt(val, DesAlgorithmName, bkey, ivbBlock);
		}

		/// <summary>
		/// Decripta una stringa utilizzando l'algoritmo a chiave simmetrica DES
		/// </summary>
		/// <param name="val">stringa da decriptare</param>
		/// <param name="key">chiave di crypt</param>
		/// <param name="ivBlock">blocco di crypt</param>
		/// <returns>la stringa decriptata</returns>
		//---------------------------------------------------------------------
		public static string Decrypt(string val, string key, string block)
		{
			byte[] bkey		= Crypto.ToByteArray(key);
			byte[] ivbBlock	= Crypto.ToByteArray(block);
			
			return Decrypt(val, DesAlgorithmName, bkey, ivbBlock);
		}

		/// <summary>
		/// Decripta un array di byte utilizzando l'algoritmo a chiave simmetrica DES
		/// </summary>
		/// <param name="buffer">array di byte da decriptare</param>
		/// <param name="key">chiave di crypt</param>
		/// <param name="ivBlock">blocco di crypt</param>
		/// <returns>la stringa decriptata</returns>
		//---------------------------------------------------------------------
		public static string Decrypt(byte[] buffer, string key, string block)
		{
			byte[] bkey		= Crypto.ToByteArray(key);
			byte[] ivbBlock	= Crypto.ToByteArray(block);

			return Decrypt(buffer, DesAlgorithmName, bkey, ivbBlock);
		}

		/// <summary>
		/// Decripta una stringa utilizzando un algoritmo a chiave simmetrica
		/// </summary>
		/// <param name="val">stringa da decriptare</param>
		/// <param name="algoritmName">nome dell'algoritmo simmetrico prescelto</param>
		/// <param name="key">chiave di crypt</param>
		/// <param name="ivBlock">blocco di crypt</param>
		/// <returns>la stringa decriptata</returns>
		//---------------------------------------------------------------------
		public static string Decrypt(string val, string algoritmName, string key, string block)
		{
			byte[] bkey		= Crypto.ToByteArray(key);
			byte[] ivbBlock	= Crypto.ToByteArray(block);

			return Decrypt(val, algoritmName, bkey, ivbBlock);
		}

		/// <summary>
		/// Decripta una stringa utilizzando un algoritmo a chiave simmetrica
		/// </summary>
		/// <param name="val">stringa da decriptare</param>
		/// <param name="algoritmName">nome dell'algoritmo simmetrico prescelto</param>
		/// <param name="key">chiave di crypt</param>
		/// <param name="ivBlock">blocco di crypt</param>
		/// <returns>la stringa decriptata</returns>
		//---------------------------------------------------------------------
		public static string Decrypt(string val, string algoritmName, byte[] key, byte[] ivBlock)
		{	
			if (val == null || val.Length == 0 )
				return string.Empty;
			try
			{
				byte[] buffer = Convert.FromBase64String(val);
				return Decrypt(buffer, algoritmName, key, ivBlock);
			}
			catch (Exception err)
			{
				Debug.WriteLine(err.Message);
				return string.Empty;
			}
		}

		/// <summary>
		/// Decripta un array di byte utilizzando un algoritmo a chiave simmetrica
		/// </summary>
		/// <param name="buffer">array di byte da decriptare</param>
		/// <param name="algoritmName">nome dell'algoritmo simmetrico prescelto</param>
		/// <param name="key">chiave di crypt</param>
		/// <param name="ivBlock">blocco di crypt</param>
		/// <returns>la stringa decriptata</returns>
		//---------------------------------------------------------------------
		public static string Decrypt(byte[] buffer, string algoritmName, byte[] key, byte[] ivBlock)
		{	
			if (buffer == null || buffer.Length == 0 )
				return string.Empty;

			SymmetricAlgorithm cryptoProvider = Init(algoritmName, key, ivBlock);
			StreamReader sr;

			try
			{
				cryptoProvider.KeySize = key.Length * 8;
				cryptoProvider.BlockSize = ivBlock.Length * 8;

				MemoryStream ms = new MemoryStream(buffer);
				CryptoStream cs = new CryptoStream
					(
					ms, 
					cryptoProvider.CreateDecryptor(key, ivBlock), 
					CryptoStreamMode.Read
					);
				sr = new StreamReader(cs);
				return sr.ReadToEnd();
			}
			catch (Exception err)
			{
				Debug.WriteLine(err.Message);
				return string.Empty;
			}
		}

		/// <summary>
		/// Decripta un array di byte utilizzando l'algoritmo a chiave simmetrica DES utilizzando le 
		/// cryptkey e cryptblock di default
		/// </summary>
		/// <param name="buffer">array di byte da decriptare</param>
		/// <returns>un array di byte decriptato</returns>
		//--------------------------------------------------------------------------------
		public static string Decrypt(byte[] buffer)
		{
			byte[] bkey = Crypto.ToByteArray(defaultCryptKey);
			byte[] ivbBlock = Crypto.ToByteArray(defaultCryptBlock);

			return Decrypt(buffer, DesAlgorithmName, bkey, ivbBlock);
		}

		/// <summary>
		/// Decripta un array di byte utilizzando l'algoritmo a chiave simmetrica DES utilizzando le 
		/// cryptkey e cryptblock di default
		/// </summary>
		/// <param name="buffer">array di byte da decriptare</param>
		/// <returns>un array di byte decriptato</returns>
		//--------------------------------------------------------------------------------
		public static byte[] DecryptToByteArray(byte[] buffer)
		{
			byte[] bkey = Crypto.ToByteArray(defaultCryptKey);
			byte[] ivbBlock = Crypto.ToByteArray(defaultCryptBlock);

			return DecryptToByteArray(buffer, DesAlgorithmName, bkey, ivbBlock);
		}

		/// <summary>
		/// Decripta un array di byte utilizzando un algoritmo a chiave simmetrica
		/// </summary>
		/// <param name="buffer">array di byte da decriptare</param>
		/// <param name="algoritmName">nome dell'algoritmo simmetrico prescelto</param>
		/// <param name="key">chiave di crypt</param>
		/// <param name="ivBlock">blocco di crypt</param>
		/// <returns>un array di byte decriptato</returns>
		//--------------------------------------------------------------------------------
		public static byte[] DecryptToByteArray(byte[] buffer, string algoritmName, byte[] key, byte[] ivBlock)
		{	
			if (buffer == null || buffer.Length == 0 )
				return new byte[0];
			
			SymmetricAlgorithm cryptoProvider = Init(algoritmName, key, ivBlock);
			try
			{
				cryptoProvider.KeySize = key.Length * 8;
				cryptoProvider.BlockSize = ivBlock.Length * 8;

				MemoryStream ms = new MemoryStream(buffer);
				CryptoStream cs = new CryptoStream
					(
					ms, 
					cryptoProvider.CreateDecryptor(key, ivBlock), 
					CryptoStreamMode.Read
					);
				
				byte[] readBytes = new byte[1024]; 
				int numBytes = 0;

				MemoryStream outMs = new MemoryStream();
				while ((numBytes = cs.Read(readBytes, 0, 1024)) != 0)
					outMs.Write(readBytes, 0, numBytes);
				
				byte[] outBytes = new byte[outMs.Length];
				outMs.Seek(0, SeekOrigin.Begin);
				outMs.Read(outBytes, 0, (int)outMs.Length);
				return outBytes;
			}
			catch (Exception err)
			{
				Debug.WriteLine(err.Message);
				return new byte[0];
			}
		}
	}
}
