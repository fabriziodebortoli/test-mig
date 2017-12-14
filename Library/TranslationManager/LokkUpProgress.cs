using System;
using System.Collections;
using System.Xml;
using System.IO;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.NameSolver;

namespace Microarea.Library.TranslationManager
{
	public class LookUpProgress : SolutionManagerItems
	{
		private LookUpFileType[] lutList = new LookUpFileType[]{LookUpFileType.ClientDocuments, LookUpFileType.Dbts, LookUpFileType.Documents, LookUpFileType.ReferenceObjects, LookUpFileType.Report, LookUpFileType.Structure, LookUpFileType.WebMethods, LookUpFileType.XTech, LookUpFileType.Activation, LookUpFileType.Misc, LookUpFileType.Tables};
		private ArrayList messages = new ArrayList();

		public LookUpProgress()
		{	
		}

		public override string ToString()
		{
			return "Controllo stato della compilazione dei files di lookup";
		}

		public override void Run(TranslationManager tManager)
		{
			transManager = tManager;
			messages.Clear();

			foreach (LookUpFileType lType in lutList)
			{
				int progress = 100;
				int iTarget = 0;
				int iFilled = 0;
				string message = string.Empty;
				XmlDocument xDoc = null;
				xDoc = transManager.GetLookUpFile(lType);

				if (xDoc == null)
				{
					message = string.Format("Non esiste il file di lookup relativo a: {0}\n", LookUpFileName(lType));
					messages.Add(message);
					continue;
				}

				foreach (XmlNode n in xDoc.ChildNodes)
				{
					GetNodeStatus(n, ref iTarget, ref iFilled);
				}

				if (iTarget != 0)
					progress = (int)((iFilled * 100) / iTarget);

				message = string.Format("Sono stati tradotti {0} termini su {1} ({2}%) sul file di lookup relativo a: {3}\n", iFilled.ToString(), iTarget, progress, LookUpFileName(lType));
				messages.Add(message);
			}

			string endMessage = string.Empty;

			foreach (string s in messages)
			{
				endMessage += s;
			}
			
			MessageBox.Show(endMessage);

			EndRun(false);
		}

		private string LookUpFileName(LookUpFileType type)
		{
			switch (type)
			{
				case LookUpFileType.Structure:
					return "Namespace di applicazione";
				case LookUpFileType.Activation:
					return "Moduli/Funzionalita'";
				case LookUpFileType.Dbts:
					return "Dbts";
				case LookUpFileType.Documents:
					return "Documents";
				case LookUpFileType.ClientDocuments:
					return "ClientDocuments";
				case LookUpFileType.ReferenceObjects:
					return "ReferenceObjects";
				case LookUpFileType.Tables:
					return "Tabelle e campi";
				case LookUpFileType.WebMethods:
					return "WebMethods";
				case LookUpFileType.Misc:
					return "Oggetti vari (oggetti grafici, ecc.)";
				case LookUpFileType.XTech:
					return "XTech";
				case LookUpFileType.Report:
					return "Report";
			}

			return string.Empty;
		}

		private void GetNodeStatus(XmlNode nCurrent, ref int iTarget, ref int iFilled)
		{
			string attrName = "target";

			if (nCurrent.Name == "Table" || nCurrent.Name == "Column")
				attrName = "newName";

			try
			{
				string val = nCurrent.Attributes[attrName].Value.ToString();
				iTarget ++;
				if (val != string.Empty)
					iFilled ++;
			}
			catch
			{
			}

			foreach (XmlNode n in nCurrent.ChildNodes)
			{
				GetNodeStatus(n, ref iTarget, ref iFilled);
			}
		}
	}
}
