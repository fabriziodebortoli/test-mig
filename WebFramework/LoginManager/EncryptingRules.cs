using System.Collections;

namespace Microarea.WebServices.LoginManager
{
	/// <summary>
	/// contatori associati
	/// </summary>
	//---------------------------------------------------------------------------
	internal enum CounterType
	{
		Standard, 
		//Standard2, 
		Standard3, 
		Custom,
		AppData, 
		Cookie, 
		InternetTemp, 
		Registry, 
		Database, 
		Custom2, 
		LoginManager,
		LockManager,
		TbService
	}

	/// <summary>
	/// Algoritmi di crypting supportati
	/// </summary>
	//---------------------------------------------------------------------------
	internal enum EncryptingAlgorithm
	{
		NotDefined, 
		Rc2, 
		Des,
		TripleDes, 
		Rijndael
	}

	/// <summary>
	/// rule structure
	/// </summary>
	//=========================================================================
	internal class EncryptingRule
	{
		//---------------------------------------------------------------------------
		private int					ruleID			= 0;
		private EncryptingAlgorithm algorithm		= EncryptingAlgorithm.NotDefined;
		private string				algorithmName	= string.Empty;
		private byte[]				key				= null;
		private byte[]				block			= null;
		private int					keySize			= 8;
		private int					blockSize		= 8;

		// properties
		//---------------------------------------------------------------------------
		internal int					RuleID			{ get { return ruleID; } }
		internal EncryptingAlgorithm	Algorithm		{ get { return algorithm; } }
		internal string					AlgorithmName	{ get { return algorithmName; } }
		internal byte[]					Key				{ get { return key; } }
		internal byte[]					Block			{ get { return block; } }
		internal int					KeySize			{ get { return keySize; } }
		internal int					BlockSize		{ get { return blockSize; } }

		/// <summary>
		/// construction
		/// </summary>
		/// <param name="counterType">type of counter </param>
		/// <param name="algorithm"></param>
		/// <param name="key"></param>
		/// <param name="block"></param>
		//---------------------------------------------------------------------------
		internal EncryptingRule (int ruleID, EncryptingAlgorithm algorithm, byte[] k, byte[] b)
		{
			this.algorithm	= algorithm;
			this.ruleID		= ruleID;

			// Algorithm names are case sensitive!
			switch (algorithm)
			{
				case EncryptingAlgorithm.Des:
					this.algorithmName	= "DES";
					break;
				case EncryptingAlgorithm.Rc2:
					keySize				= 16;
					this.algorithmName	= "RC2";
					break;
				case EncryptingAlgorithm.Rijndael:
					blockSize			= 16;
					keySize				= 16;
					this.algorithmName	= "Rijndael";
					break;
				case EncryptingAlgorithm.TripleDes:
					keySize				= 16;
					this.algorithmName	= "TripleDES";
					break;
			}
			
			InitBytes (k, b);
		}
		/// <summary>
		/// completes the object construction 
		/// </summary>
		//---------------------------------------------------------------------------
		private void InitBytes (byte[] k, byte[] b)
		{
			this.key	= new byte[keySize]; 

			int lenght = k.Length;
			if (lenght > keySize)
				lenght = keySize;

			for (int i = 0 ; i < lenght ; i++)
				this.key[i] = k[i];

			this.block	= new byte[blockSize];

			lenght = b.Length;
			if (lenght > blockSize)
				lenght = blockSize;

			for (int i = 0 ; i < lenght ; i++)
				this.block[i] = b[i];
		}
	}

	//=========================================================================
	internal class CountersEncryptRules
	{		
		/// <summary>
		/// tabella di associazione
		/// </summary>
		//---------------------------------------------------------------------------
		private ArrayList rules = new ArrayList();

		/// <summary>
		/// Encrypting rules definition. 
		/// </summary>
		//---------------------------------------------------------------------------
		internal CountersEncryptRules()
		{
			rules.Add (new EncryptingRule((int)CounterType.Standard,		EncryptingAlgorithm.Rc2,		new byte[] { 77, 105, 98, 106, 49, 35, 98, 105 },  new byte[] { 33, 67, 98, 100, 55, 54, 72, 105 } ));
			//rules.Add (new EncryptingRule((int)CounterType.Standard2,		EncryptingAlgorithm.Rijndael,	new byte[] { 33, 67, 98, 106, 55, 54, 72, 105 },  new byte[] { 93, 103, 104, 56, 57, 48, 52, 53 } ));
			rules.Add (new EncryptingRule((int)CounterType.Standard3,		EncryptingAlgorithm.Des,		new byte[] { 66, 93, 98, 105, 55, 42, 53, 105 },  new byte[] { 88, 56, 82, 106, 49, 98, 108, 23 } ));
			rules.Add (new EncryptingRule((int)CounterType.Custom,			EncryptingAlgorithm.TripleDes,	new byte[] { 55, 78, 102, 110, 55, 68, 98, 58 },  new byte[] { 66, 93, 98, 105, 49, 42, 53, 105 } ));
			rules.Add (new EncryptingRule((int)CounterType.AppData,			EncryptingAlgorithm.Rijndael,	new byte[] { 44, 105, 98, 37, 55, 54, 98, 112 },  new byte[] { 100, 45, 34, 106, 67, 54, 77, 55} ));
			rules.Add (new EncryptingRule((int)CounterType.Cookie,			EncryptingAlgorithm.Des,		new byte[] { 88, 56, 86, 106, 49, 98, 108, 23 },  new byte[] { 92, 100, 102, 55, 75, 98, 54, 96 } ));
			rules.Add (new EncryptingRule((int)CounterType.InternetTemp,	EncryptingAlgorithm.Rijndael,	new byte[] { 99, 105, 98, 124, 55, 102, 98, 98 },  new byte[] { 33, 67, 98, 106, 55, 54, 72, 105  } ));
			rules.Add (new EncryptingRule((int)CounterType.Registry,		EncryptingAlgorithm.Rc2,		new byte[] { 100, 45, 34, 106, 55, 54, 77, 55 },  new byte[] { 44, 105, 98, 37, 55, 54, 98, 112 } ));
			rules.Add (new EncryptingRule((int)CounterType.Database,		EncryptingAlgorithm.TripleDes,	new byte[] { 104, 96, 98, 45, 55, 54, 98, 33 },  new byte[] { 77, 105, 98, 106, 49, 35, 98, 105 } ));
			rules.Add (new EncryptingRule((int)CounterType.Custom2,			EncryptingAlgorithm.Rc2,		new byte[] { 101, 47, 64, 126, 54, 57, 87, 105 },  new byte[] { 144, 125, 58, 97, 57, 24, 99, 12 } ));
			rules.Add (new EncryptingRule((int)CounterType.LoginManager,	EncryptingAlgorithm.TripleDes,	new byte[] { 19, 106, 8, 45, 45, 4, 8, 133 },  new byte[] { 7, 15, 108, 126, 47, 25, 78, 155 } ));
			rules.Add (new EncryptingRule((int)CounterType.LockManager,		EncryptingAlgorithm.Des,		new byte[] { 89, 107, 108, 42, 15, 94, 68, 13 },  new byte[] { 57, 115, 18, 16, 46, 125, 77, 15 } ));
			rules.Add (new EncryptingRule((int)CounterType.TbService,		EncryptingAlgorithm.Rijndael,		new byte[] { 189, 207, 208, 242, 215, 194, 168, 213 },  new byte[] { 157, 11, 86, 96, 106, 15, 76, 34 } ));

		}

		/// <summary>
		/// </summary>
		//---------------------------------------------------------------------------
		internal EncryptingRule GetRule (CounterType type)
		{
			foreach (EncryptingRule rule in rules)
				if (rule.RuleID == (int) type)
					return rule;
			
			return null;
		}
	}
}
