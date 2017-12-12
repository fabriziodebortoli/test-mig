using System;
using System.Collections;
using System.Xml;
using System.IO;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.NameSolver;

namespace Microarea.Library.TranslationManager
{
	public class IncludeManager : SolutionManagerItems
	{
		private BasePathFinder pathFinder = new BasePathFinder();

		public IncludeManager()
		{
			defaultLookUpType = LookUpFileType.Structure;
		}

		public override string ToString()
		{
			return "Include Manager";
		}

		public override void Run(TranslationManager tManager)
		{
			TxtReader = new RichTextBox();

			transManager = tManager;
			
			if (!OpenLookUpDocument(false))
				return;

			XmlNode nMain = xLookUpDoc.SelectSingleNode("Application");
			//Corrispondenze crps = new Corrispondenze(nMain);
			string appDir = Path.Combine(transManager.GetApplicationParentPath(), nMain.Attributes["target"].Value.ToString());
			if (!Directory.Exists(appDir))
				return;

			//Leggo i moduli
			foreach (XmlNode nModule in nMain.SelectNodes("Module"))
			{
				string modDir = Path.Combine(appDir, nModule.Attributes["target"].Value.ToString());
				if (!Directory.Exists(modDir))
					return;

				//Leggo le library
				foreach (XmlNode nLibrary in nModule.SelectNodes("Library"))
				{
					string sourcePath = transManager.GetApplicationParentPath() + nMain.Attributes["source"].Value.ToString() + @"\" + nModule.Attributes["source"].Value.ToString() + @"\" + nLibrary.Attributes["sourcefolder"].Value.ToString();
					string destinationPath = Path.Combine(modDir, nLibrary.Attributes["destinationfolder"].Value.ToString());
					
					if (!Directory.Exists(destinationPath))
						return;

					DirectoryInfo sDI = new DirectoryInfo(destinationPath);
					
					//Elaboro i singoli files
					foreach (FileInfo fi in sDI.GetFiles())
					{
						if (fi.Extension.ToLower() == ".h" || fi.Extension.ToLower() == ".cpp" || fi.Extension.ToLower() == ".rc")
						{
							SetProgressMessage(string.Format("Elaborazione in corso: {0}\\{1}\\{2}", nModule.Attributes["target"].Value.ToString(), nLibrary.Attributes["target"].Value.ToString(), fi.Name));

							FileElaboration(fi.FullName, nMain, fi.Extension.ToLower());
						}
					}
				}
			}

			EndRun(false);
		}

		private void FileElaboration(string fileName, XmlNode nApplication, string extension)
		{
			Hashtable hrcInclude = new Hashtable();
			int start, end = 0;
			try
			{
				if (File.Exists(fileName))
					TxtReader.LoadFile(fileName, RichTextBoxStreamType.PlainText);
				else
					return;
			}
			catch
			{
				SetLogError(string.Format("Non riesco a caricare il file: {0}", fileName), ToString());
				return;
			}

			//cablo nel codice il find & replace che dovrei fare a mano sugli include degli rc
			/*if (extension == ".rc")
			{
				if (TxtReader.Text.IndexOf("#include \"articoli") > 0)
				{
					TxtReader.Text = TxtReader.Text.Replace("#include \"articoli", "#include \"items");
					TxtReader.Text = TxtReader.Text.Replace("\"#include \"\"articoli", "\"#include \"\"items");

					File.SetAttributes(fileName, FileAttributes.Normal);
					TxtReader.SaveFile(fileName, RichTextBoxStreamType.PlainText);
				}

				return;
			}*/

			string riga = string.Empty;
			StringReader sr = new StringReader(TxtReader.Text);
			while (true) 
			{
				start = end;
				
				riga = sr.ReadLine();

				if (riga == null)
					break;
				
				end += riga.Length + 1;
				riga = riga.Trim();

				/*if ((extension == ".cpp" && riga.StartsWith("#ifdef")) ||
					(extension == ".h" && riga.ToLower().StartsWith("#include \"beginh.dex\""))) 
				{
					break;
				}*/

				if (!riga.StartsWith("#include"))
					continue;

				string startingParenthesis, endingParenthesis = string.Empty;
				if (riga.IndexOf("<") < 0)
				{
					startingParenthesis = endingParenthesis = "\"";
				}
				else
				{
					startingParenthesis = "<";
					endingParenthesis = ">";
				}

				int sIdx, len = 0;
				sIdx = riga.IndexOf(startingParenthesis) + 1;
				len = riga.IndexOf(endingParenthesis, sIdx) - sIdx;
				string include = riga.Substring(sIdx, len);
				string newInclude = string.Empty;

				char slash;

				if (include.IndexOf("\\") > 0)
					slash = '\\';
				else
					slash = '/';
						
				string mod = string.Empty;
				string lib = string.Empty;
				string file = string.Empty;

				switch (include.Split(slash).Length)
				{
					case 1:
						newInclude = GetConversion(include.Split(slash)[0]);
						break;
					case 2:
						mod = transManager.GetModuleTranslation(include.Split(slash)[0]);
						file = GetConversion(include.Split(slash)[1], include.Split(slash)[0]);
						newInclude = mod + @"\" + file;
						break;
					case 3:
						mod = transManager.GetModuleTranslation(include.Split(slash)[0]);
						lib = transManager.GetLibraryTranslationFolder(include.Split(slash)[0], include.Split(slash)[1]);
						file = GetConversion(include.Split(slash)[2], include.Split(slash)[0] + include.Split(slash)[1], include.Split(slash)[0]);
						newInclude = mod + @"\" + lib + @"\" + file;
						break;
				}

				if (newInclude != string.Empty)
				{
					end += newInclude.Length - include.Length;
					
					string prima = TxtReader.Text.Substring(0, start + sIdx);
					string dopo = TxtReader.Text.Substring(start + sIdx + len);
					TxtReader.Text = prima + newInclude + dopo;
				}

				if (extension == ".rc" && include.EndsWith(".hrc"))
				{
					if (!hrcInclude.ContainsKey(include))
						hrcInclude.Add(include, newInclude);
				}
				/*string prova = TxtReader.Text.Substring(start + sIdx, len);
				if (include != prova)
				{
					MessageBox.Show("Houston, Houston, we have got a problem!!!");
				}*/
			}

			foreach (string hi in hrcInclude.Keys)
			{
				TxtReader.Text = TxtReader.Text.Replace(hi, hrcInclude[hi].ToString());
			}

			File.SetAttributes(fileName, FileAttributes.Normal);
			TxtReader.SaveFile(fileName, RichTextBoxStreamType.PlainText);
		}
	}
}
