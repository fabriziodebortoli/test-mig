using System;
using System.Diagnostics;
using System.Security.Cryptography;


namespace Microarea.Library.SMBaseHandler
{
	//=========================================================================
	public class Algorithm
	{
		//---------------------------------------------------------------------
		public static byte[] ToByteArray(string text)
		{
			//Convert Bits to Bytes
			int thisSize = 8;
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

		//---------------------------------------------------------------------
		public static SymmetricAlgorithm Init(string algoritmName, byte[] key, byte[] ivBlock)
		{
			SymmetricAlgorithm symmetricAlgorithm = null;
			try
			{
				symmetricAlgorithm = SymmetricAlgorithm.Create(algoritmName);
				if(symmetricAlgorithm == null)
				{
					Debug.Fail("Algorithm.Init: Error creating algorithm of Rijndael");
					return null;
				}
			}
			catch(Exception err)
			{
				Debug.Fail(err.Message);
				return null;
			}

			try
			{
				symmetricAlgorithm.BlockSize =	ivBlock.Length * 8;
				symmetricAlgorithm.KeySize =	key.Length * 8;
			}
			catch (Exception err)
			{
				Debug.Fail(err.Message);
				return null;
			}
			return symmetricAlgorithm;
		}
	}
}
