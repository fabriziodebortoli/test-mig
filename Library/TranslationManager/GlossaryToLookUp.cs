using System;
using System.Xml;
using System.IO;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.NameSolver;

namespace Microarea.Library.TranslationManager
{
	public class GlossaryToLookUp : SolutionManagerItems
	{
		private LookUpFileType[] lutList = new LookUpFileType[]{LookUpFileType.ClientDocuments, LookUpFileType.Dbts, LookUpFileType.Documents, LookUpFileType.ReferenceObjects, LookUpFileType.Report, LookUpFileType.Structure, LookUpFileType.Tables, LookUpFileType.WebMethods, LookUpFileType.XTech, LookUpFileType.Activation, LookUpFileType.Misc};
		private int tempStatus = 0; //0 = non definito; 1 = aggiungo _E; 2 = non aggiungo _E
		public GlossaryToLookUp()
		{
			defaultLookUpType = LookUpFileType.Glossary;
		}

		public override string ToString()
		{
			return "Applicazione dei termini del glossario sui files di lookup";
		}

		public override void Run(TranslationManager tManager)
		{
			transManager = tManager;

			SelectFileType sft = new SelectFileType();
			sft.ShowDialog();
			
			foreach (LookUpFileType lType in sft.FileTypeList)
			{
				XmlDocument xDoc = null;
				xDoc = transManager.GetLookUpFile(lType);

				if (xDoc == null)
					continue;

				string xPathQuery = string.Empty;
				if (lType != LookUpFileType.Tables)
					xPathQuery = "//@target";
				else
					xPathQuery = "//@newName";

				XmlNodeList nList = xDoc.SelectNodes(xPathQuery);
				int idxMax = nList.Count;
				int idxProg = 0;
				int idxTerm = 0;

				foreach (XmlNode n in nList)
				{
					idxTerm++;
					idxProg = (int)((idxTerm * 100) / idxMax);
					string msg = string.Format("Elaborazione in corso: tipo file {0}, elemento {1} di {2} ({3}%)", lType.ToString(), idxTerm, idxMax, idxProg);

					SetProgressMessage(msg);
					ConvertNode((XmlAttribute)n);
				}

				if (lType == LookUpFileType.Structure)
				{
					foreach (XmlNode n in xDoc.SelectNodes("//@destinationfolder"))
						ConvertNode((XmlAttribute)n);
				}

				transManager.SaveLookUpFile(lType);
			}

			EndRun(false);
		}

		private void ConvertNode(XmlAttribute nTarget)
		{
			XmlNode nParent = nTarget.OwnerElement;
			string sAttrName = string.Empty;
			if (nTarget.Name == "target")
				sAttrName = "source";
			else if (nTarget.Name == "destinationfolder")
				sAttrName = "sourcefolder";
			else
				sAttrName = "oldName";

			XmlAttribute aSource = nParent.Attributes[sAttrName];

			string newText = transManager.GetGlossaryConversion(aSource.Value.ToString());

			if (newText != string.Empty)
				nTarget.InnerText = newText;
			else
			{
				switch (tempStatus)
				{
					case 0:
						DialogResult dr = MessageBox.Show("La traduzione non è completa! \n Creo una traduzione provvisoria?", "Traduzione incompleta", MessageBoxButtons.YesNo);
						if (dr == DialogResult.Yes)
						{
							tempStatus = 1;
							nTarget.InnerText = aSource.Value.ToString() + "_E";
						}
						else
							tempStatus = 2;
						break;
					case 1:
						nTarget.InnerText = aSource.Value.ToString() + "_E";
						break;
				}
			}
		}
	}
}
