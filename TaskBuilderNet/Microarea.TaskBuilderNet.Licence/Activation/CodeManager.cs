using System;
using System.Collections.Generic;
using System.IO;

namespace Microarea.TaskBuilderNet.Licence.Activation
{
	//=========================================================================
	public class CodeManager : ICodeManager
	{
		private const int minThreshold = 1000000;
		private const int maxThreshold = 9999999;
		private const int minThreshold1 = 10000;
		private const int maxThreshold1 = 99999;
		private const int maxMillSecs = 999;

		private Random rnd;
		private int check;
		private bool isGeneratedCheckGood;

		//---------------------------------------------------------------------
		public virtual bool IsGeneratedCheckGood
		{
			get { return isGeneratedCheckGood; }
		}

		//---------------------------------------------------------------------
		public CodeManager()
			:
			this(Path.GetFileNameWithoutExtension(Path.GetRandomFileName()))
		{}

		//---------------------------------------------------------------------
		public CodeManager(string seed)
		{
			if (seed == null)
				throw new ArgumentNullException("seed");

			if (seed.Trim().Length == 0)
				throw new ArgumentException("'seed' is empty");

			rnd = new Random(GetSeedFromString(seed));
			check = -1;
		}

		//---------------------------------------------------------------------
		public CodeManager(ICollection<string> seeds)
		{
			if (seeds == null)
				throw new ArgumentNullException("seeds");
			if (seeds.Count == 0)
				throw new ArgumentException("seeds is empty");

			List<string> current = new List<string>(seeds);
			current.Sort(StringComparer.InvariantCultureIgnoreCase);

			rnd = new Random(GetSeedFromString(current[0]));
			check = -1;
		}

		//---------------------------------------------------------------------
		private int GetSeedFromString(string seed)
		{
			seed = seed.ToUpperInvariant();

			UInt64 v4 = Convert.ToUInt64(seed[0]);
			for (int i = 1; i < seed.Length; i++)
				v4 = v4 * 16 + Convert.ToUInt64(seed[i]);

			uint h0 = (uint)((v4 & 0xFFFF000000000000) >> 48);
			uint h1 = (uint)((v4 & 0xFFFF00000000) >> 32);
			uint h2 = (uint)((v4 & 0xFFFF0000) >> 16);
			uint h3 = (uint)(v4 & 0xFFFF);
			return (int)(h0 ^ h1 ^ h2 ^ h3);
		}

		#region ICodeManager Members

		//---------------------------------------------------------------------
		public virtual int GenerateCode()
		{
			int maxRepetitions = DateTime.Now.Millisecond;
			int code = 0;

			//Butto via una serie di codici casuale.
			for (int i = 0; i < maxRepetitions; i++)
				rnd.Next(minThreshold, maxThreshold);

			code = rnd.Next(minThreshold1, maxThreshold1);
			//Il codice di controllo è quello che segue code nella serie.
			check = rnd.Next(minThreshold1, maxThreshold1);

			return code;
		}

		//---------------------------------------------------------------------
		public virtual bool CheckCode(int code)
		{
			return (code == check);
		}

		//---------------------------------------------------------------------
		public virtual int GenerateCheck(int code)
		{
			//Per generare il codice di controllo è necessario che io ripercorra
			//tutta la serie di codici generata fino che non trovo code.
			//Il codice di controllo è il codice che segue code nella serie.
			//Il ciclo ha come limite di ripetizioni maxMillSecs a causa di come
			//è calcolato code nel metodo GenerateCode().
			//Questo è necessario perchè, se Random è inizializzato con il
			//medesimo seed allora prima o poi, percorrendo tutta la serie, troverò code.
			//Altrimenti è possibile non trovare mai code e quindi è possibile che
			//non si esca mai dal ciclo while.
			int check = 0;
			int maxLimit = maxMillSecs + 2;//Aggiungo due perchè nel calcolo del metodo GenetareCode chiamo rnd.Next 2 volte in più dei millisecondi letti per generare il codice e il contro codice.
			int i = 0;
			do
			{
				check = rnd.Next(minThreshold1, maxThreshold1);
			} while ((check != code) && ((++i) <= maxLimit));

			//Il check enerato è buono se e solo se sono uscito dal while perchè
			//check == code.
			//Se invece sono uscito dal while perchè ho superato maxLimit
			//significa che il check generato non è buono.
			//Un check non buono non permetterà di concludere con successo
			//la validazione via SMS sul client.
			isGeneratedCheckGood = check == code;

			return rnd.Next(minThreshold1, maxThreshold1);
		}

		#endregion
	}
}
