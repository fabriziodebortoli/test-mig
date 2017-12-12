using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Licence.Activation;

namespace ProgressiveEncrypter
{
	public partial class Form1 : Form
	{
		public string mailText = @"Salve,
Deve seguire le seguenti istruzioni:

1. chiudere WebUpdaterAdmin;
2. aprire con NOTEPAD il file C:\Program Files\Microarea\WebUpdater Services\Services\WebUpdaterAdmin\Bin\Release\WebUpdaterAdmin.exe.config
(il path è relativo al percorso dove ha installato i Services, se ha il sistema operativo in italiano per esempio sarà Programmi invece di Program Files....);
3. sostituire, se esiste, la riga {3} con la seguente:
{0}
(cambia il value);
se la riga da sostituire non esiste eseguire solo l'aggiunta di quella nuova.

NB: questa modifica è valida solo per il serial number {2} 
4. Salvare e chiudere il file;
5. riavviare WebUpdaterAdmin;
6. inserire nel modulo {1} il seriale in suo possesso; 
7. in fondo alla lista di moduli compariranno quelli prima assenti.

Rimango a disposizione per ulteriori chiarimenti e porgo cordiali saluti,
Microarea S.p.A.";

		string rigaMask = "<add key=\"{0}\" value=\"{1}\" />";
		string ERROR = "ERROR";
		public Hashtable codeTable = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
		public Hashtable progressiveTable = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
		int Progressivo = -1;
		//---------------------------------------------------------------------
		public Form1()
		{
			InitializeComponent();
			//tabella shortname -> nome modulo, con placeholder per edition e db
			codeTable.Add("CFIN", "ERP-{1}.Financials@{0}");
			codeTable.Add("CFUL", "ERP-{1}.Full@{0}");

			codeTable.Add("SEVR", "ERP-{1}.Server@{0}");

			codeTable.Add("EDU1", "EducationalPackage1@{0}");
			codeTable.Add("EDU2", "EducationalPackage2@{0}");

			codeTable.Add("ADPK", "ERP-{1}.Advanced Package@{0}");
			codeTable.Add("COMM", "ERP-{1}.Commercial Package@{0}");
			codeTable.Add("PACK", "ERP-{1}.Small Business Package@{0}");
			codeTable.Add("TRPK", "ERP-{1}.Trade Package@{0}");

			codeTable.Add("PCKF", "ERP-{1}.Small Business Package.Overload@{0}");
			codeTable.Add("TRKF", "ERP-{1}.Trade Package.Overload@{0}");
            codeTable.Add("ADKF", "ERP-{1}.Advanced Package.Overload@{0}");
			codeTable.Add("COMF", "ERP-{1}.Commercial Package.Overload@{0}");

			//tabella nome modulo completa -> progressimo criptato massimo entro il quele saranno visibili i moduli
			progressiveTable.Add("ERP-Ent.Financials@NDB", "MjMzMjk3");
			progressiveTable.Add("ERP-Ent.Full@NDB", "MjMzMzcx");
			progressiveTable.Add("ERP-Pro.Advanced Package@MSD", "MjM1OTcz");
			progressiveTable.Add("ERP-Pro.Advanced Package@NDB", "MjMzMjgx");
			progressiveTable.Add("ERP-Pro.Advanced Package@ORA", "MjMzMjk5");
			progressiveTable.Add("ERP-Pro.Advanced Package@SQL", "MjMzOTYy");
			progressiveTable.Add("ERP-Pro.Commercial Package@MSD", "MjMzMjg1");
			progressiveTable.Add("ERP-Pro.Commercial Package@ORA", "MjMzMjgx");
			progressiveTable.Add("ERP-Pro.Commercial Package@SQL", "MjMzMjg3");
			progressiveTable.Add("EducationalPackage1@MSD", "MjMzMjk0");
			progressiveTable.Add("EducationalPackage2@MSD", "MjMzMjg3");
			progressiveTable.Add("ERP-Pro.Small Business Package@MSD", "MjM1MDQ1");
			progressiveTable.Add("ERP-Pro.Small Business Package@NDB", "MjMzMjgx");
			progressiveTable.Add("ERP-Pro.Small Business Package@ORA", "MjMzMjgz");
			progressiveTable.Add("ERP-Pro.Small Business Package@SQL", "MjMzNDUx");
			progressiveTable.Add("ERP-Pro.Server@MSD", "MjMzOTE3");
			progressiveTable.Add("ERP-Pro.Server@NDB", "MjMzMjgx");
			progressiveTable.Add("ERP-Pro.Server@ORA", "MjMzMzE2");
			progressiveTable.Add("ERP-Pro.Server@SQL", "MjMzNDM4");
			progressiveTable.Add("ERP-Pro.Trade Package@MSD", "MjQzNzY0");
			progressiveTable.Add("ERP-Pro.Trade Package@NDB", "MjMzMjgx");
			progressiveTable.Add("ERP-Pro.Trade Package@ORA", "MjMzMzE2");
			progressiveTable.Add("ERP-Pro.Trade Package@SQL", "MjMzOTE3");
			progressiveTable.Add("ERP-Std.Small Business Package@MSD", "MjMzNTAy");
			progressiveTable.Add("ERP-Std.Commercial Package@MSD", "MjMzMjg4");
			progressiveTable.Add("ERP-Std.Server@MSD", "MjMzNDM1");

			progressiveTable.Add("ERP-Pro.Small Business Package.Overload@NDB", "MjMzMjgw");
			progressiveTable.Add("ERP-Pro.Small Business Package.Overload@MSD", "MjMzMjgw");
			progressiveTable.Add("ERP-Pro.Advanced Package.Overload@NDB", "MjMzMjgw");
			progressiveTable.Add("ERP-Pro.Advanced Package.Overload@MSD", "MjMzMjgw");
			progressiveTable.Add("ERP-Pro.Trade Package.Overload@NDB", "MjMzMjgw");
			progressiveTable.Add("ERP-Pro.Trade Package.Overload@MSD", "MjMzMjgw");
			progressiveTable.Add("ERP-Pro.Commercial Package.Overload@NDB", "MjMzMjgw");
			progressiveTable.Add("ERP-Pro.Commercial Package.Overload@MSD", "MjMzMjgw");

			progressiveTable.Add("ERP-Std.Small Business Package.Overload@MSD", "MjMzMjgw");
			progressiveTable.Add("ERP-Std.Commercial Package.Overload@MSD", "MjMzMjgw");

		}

		//---------------------------------------------------------------------
		private void button2_Click(object sender, EventArgs e)
		{
			TxtResult.Text = Crypt();
		}

		//---------------------------------------------------------------------
		private void button1_Click(object sender, EventArgs e)
		{
			TxtResult.Text = Decrypt();
		}

		//---------------------------------------------------------------------
		private string Decrypt()
		{
			return Decrypt(TxtValue.Text);
		}
		//---------------------------------------------------------------------
		private string Decrypt(string source)
		{
			if (source.Length == 16)
			{
				string db = null;
				SerialNumber n = new SerialNumber(source);
				if (n.ProductCode == null)
				{
					db = n.Database.ToString();
					TxtEdition.Text = n.Edition.ToString();
				}
				else
				{
					db = n.ProductCode.Database.ToString();
					TxtEdition.Text = n.ProductCode.Edition.ToString();
				}
				if (String.Compare(n.Database.ToString(), "All", true, System.Globalization.CultureInfo.InvariantCulture) == 0)
					db = "NDB";
				TxtDB.Text = db;
				TxtName.Text = n.RawData;
				Progressivo = n.ProgressiveNumber;
				TxtDecod.Text = n.PlainValue;
				return Progressivo.ToString();
			}
			else
			{
				Progressivo = UnparseProgressive(source);
				if (Progressivo == -1) 
					return ERROR;
				return Progressivo.ToString();
			}
		}

		//---------------------------------------------------------------------
		private string Crypt()
		{
			return Crypt(TxtValue.Text);
		}
		//---------------------------------------------------------------------
		private string Crypt(string source)
		{
			if (source.Length == 16)
				return ParseProgressive(source);
			else
			{
				try { return ParseProgressive(int.Parse(source)); }
				catch { return ERROR; }
			}
		}

		//---------------------------------------------------------------------
		private int UnparseProgressive(string val)
		{
			try
			{
				byte[] source = Convert.FromBase64String(val);
				string result = string.Empty;
				StringBuilder resultBuilder = new StringBuilder(source.Length);
				foreach (byte b in source)
					resultBuilder.Append((char)b);
				return int.Parse(resultBuilder.ToString());
			}
			catch { return -1; }
		}

		//---------------------------------------------------------------------
		private string ParseProgressive(string serial)
		{
			SerialNumber n = new SerialNumber(serial);
			

			return ParseProgressive(n.ProgressiveNumber);
		}

		//---------------------------------------------------------------------
		private string ParseProgressive(int c)
		{
			string val = c.ToString();
			byte[] result = new byte[val.Length];
			for (int i = 0; i < val.Length; i++)
				result[i] = (byte)val[i];

			return Convert.ToBase64String(result);
		}

		//---------------------------------------------------------------------
		private void BtnDammiValore_Click(object sender, EventArgs e)
		{
			string d = Decrypt();
			TxtResult.Text = Crypt(d);
			string key = codeTable[TxtName.Text] as string;
			if (key == null)
			{
				TxtResult.Text = ERROR;
				return;
			}

			
			string moduloAndDb = string.Format(key, TxtDB.Text, TxtEdition.Text);
			string progCrypted = progressiveTable[moduloAndDb] as string;
			int prog = UnparseProgressive(progCrypted);
			if (Progressivo <= prog)
			{
				TxtResult.Text = "Nothing to do";
				return;
			}

			string modulo = moduloAndDb.Substring(0, key.LastIndexOf('@'));
			string riga = string.Format(rigaMask, moduloAndDb, TxtResult.Text);
			string rigavecchia = string.Format(rigaMask, moduloAndDb, progCrypted);
			mailText = string.Format(mailText, riga, modulo, TxtValue.Text, rigavecchia);
			richTextBox1.Text = mailText;
		}

		//---------------------------------------------------------------------
		private void TxtValue_TextChanged(object sender, EventArgs e)
		{
			TxtValue.Text = TxtValue.Text.Replace("-", "");
			TxtValue.Text = TxtValue.Text.Replace(" ", ""); 
		}

		//---------------------------------------------------------------------
		private void button1_Click_1(object sender, EventArgs e)
		{
			TxtDB.Clear();
			TxtEdition.Clear();
			TxtName.Clear();
			TxtValue.Clear();
			TxtResult.Clear();
			richTextBox1.Clear();
			TxtDecod.Clear();
		}

		//---------------------------------------------------------------------
		private void BtnDecod_Click(object sender, EventArgs e)
		{
			TxtDB.Clear();
			TxtEdition.Clear();
			TxtName.Clear();
			
			TxtResult.Clear();
			richTextBox1.Clear();
			
			TxtResult.Text = Decrypt();
			
		}
		
		public string fileText = "<SMReloader>{0}</SMReloader>";

		//---------------------------------------------------------------------
		private void button2_Click_1(object sender, EventArgs e)
		{
			string source = this.Decrypt();
			this.TxtResult.Text = this.Crypt(source);
			string format = this.codeTable[this.TxtName.Text] as string;
			if (format == null)
			{
				this.TxtResult.Text = this.ERROR;
			}
			else
			{
				string str3 = string.Format(format, this.TxtDB.Text, this.TxtEdition.Text);
				string val = this.progressiveTable[str3] as string;
				int num = this.UnparseProgressive(val);
				if (this.Progressivo <= num)
					this.TxtResult.Text = "Nothing to do";
				else
				{
					string localpath = System.Reflection.Assembly.GetExecutingAssembly().Location;
					string folder = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(localpath), "SMReloader");
					if (!System.IO.Directory.Exists(folder))
						System.IO.Directory.CreateDirectory(folder);
					string str7 = string.Format(this.rigaMask, str3, this.TxtResult.Text);
					this.fileText = string.Format(this.fileText, str7);
					System.IO.StreamWriter sw = new System.IO.StreamWriter(System.IO.Path.Combine(folder, "SMReloader.xml"), false);
					sw.Write(fileText);
					sw.Close();

					this.richTextBox1.Text = ("Il file SMReloader.xml deve essere copiato nella cartella <InstallationName>\\Custom . Qui trovi il file : file:/" + folder.Replace(" ", "%20"));
				}
			}

		}


	}
}