using System;
using System.Collections;
using System.Xml;
using System.IO;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.NameSolver;

namespace Microarea.Library.TranslationManager
{
	public class SolutionMaker : SolutionManagerItems
	{
		private BasePathFinder pathFinder = new BasePathFinder();

		public SolutionMaker()
		{
			defaultLookUpType = LookUpFileType.Structure;
		}

		public override string ToString()
		{
			return "Solution Maker";
		}

		public override void Run(TranslationManager tManager)
		{
			TxtReader = new RichTextBox();

			transManager = tManager;
			
			if (!OpenLookUpDocument(false))
			{
				EndRun(false);
				return;
			}

			XmlNode nMain = xLookUpDoc.SelectSingleNode("Application");

			string oldAppName = nMain.Attributes["source"].Value.ToString();
			string newAppName = transManager.GetApplicationTranslation(oldAppName);
			//Corrispondenze crps = new Corrispondenze(nMain);
			string oldappDir = Path.Combine(transManager.GetApplicationParentPath(), oldAppName);
			string newAppDir = Path.Combine(transManager.GetApplicationParentPath(), newAppName);
			
			if (!Directory.Exists(newAppDir))
			{
				EndRun(false);
				return;
			}

			string oldSlnFileName = Path.Combine(oldappDir, string.Format("{0}.sln", oldAppName));
			if (!File.Exists(oldSlnFileName))
			{
				EndRun(false);
				return;
			}
			
			string newSlnFileName = Path.Combine(newAppDir, string.Format("{0}.sln", newAppName));

			try
			{
				File.Copy(oldSlnFileName, newSlnFileName);
				File.SetAttributes(newSlnFileName, FileAttributes.Normal);
			}
			catch (Exception ex)
			{
				SetLogError("Errore: " + ex.Message, ToString());
				EndRun(false);
				return;
			}

			TxtReader.LoadFile(newSlnFileName, RichTextBoxStreamType.PlainText);

			FileElaboration();

			TxtReader.SaveFile(newSlnFileName, RichTextBoxStreamType.PlainText);

			EndRun(false);
		}

		private void FileElaboration()
		{
			int start, end = 0;
			
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

				if (!riga.StartsWith("Project("))
					continue;

				string startingParenthesis = " = \"";
				string endingParenthesis = "\", ";

				int sIdx, len = 0;
				sIdx = riga.IndexOf(startingParenthesis) + 4;
				len = riga.IndexOf(endingParenthesis, sIdx) - sIdx;
				string modName = riga.Substring(sIdx, len);
				string newModName = transManager.GetModuleTranslation(modName);

				if (newModName != string.Empty)
				{
					end += newModName.Length - modName.Length;
					
					string prima = TxtReader.Text.Substring(0, start + sIdx);
					string dopo = TxtReader.Text.Substring(start + sIdx + len);
					TxtReader.Text = prima + newModName + dopo;
				}
				int tmpStart = start + newModName.Length - modName.Length;

				startingParenthesis = ", \"";
				endingParenthesis = "\\";

				sIdx = len = 0;
				sIdx = riga.IndexOf(startingParenthesis) + 3;
				len = riga.IndexOf(endingParenthesis, sIdx) - sIdx;
				modName = riga.Substring(sIdx, len);
				if (string.Compare(modName, "MNPchStub", true) == 0)
					newModName = "ERPPchStub";
				else
					newModName = transManager.GetModuleTranslation(modName);

				if (newModName != string.Empty)
				{
					end += newModName.Length - modName.Length;
					
					string prima = TxtReader.Text.Substring(0, tmpStart + sIdx);
					string dopo = TxtReader.Text.Substring(tmpStart + sIdx + len);
					TxtReader.Text = prima + newModName + dopo;
				}
				tmpStart += newModName.Length - modName.Length;

				startingParenthesis = "\\";
				if (riga.IndexOf(".etp") < 0)
					endingParenthesis = ".vcproj";
				else
					endingParenthesis = ".etp";

				sIdx = len = 0;
				sIdx = riga.IndexOf(startingParenthesis) + 1;
				len = riga.IndexOf(endingParenthesis, sIdx) - sIdx;
				modName = riga.Substring(sIdx, len);
				if (string.Compare(modName, "MNPchStub", true) == 0)
					newModName = "ERPPchStub";
				else
					newModName = transManager.GetModuleTranslation(modName);

				if (newModName != string.Empty)
				{
					end += newModName.Length - modName.Length;
					
					string prima = TxtReader.Text.Substring(0, tmpStart + sIdx);
					string dopo = TxtReader.Text.Substring(tmpStart + sIdx + len);
					TxtReader.Text = prima + newModName + dopo;
				}
			}
		}
	}
}
