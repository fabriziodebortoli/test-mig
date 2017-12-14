using System;
using System.Collections;
using System.Xml;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.NameSolver;

namespace Microarea.Library.TranslationManager
{
	public class GlossaryCheck : SolutionManagerItems
	{
		private Hashtable terms = new Hashtable();
		private Hashtable repetitions = new Hashtable();
		private ArrayList targetWithBlank = new ArrayList();
		private ArrayList targetWithVat = new ArrayList();
		private ArrayList valueTooLong = new ArrayList();
		private ArrayList typeArray = new ArrayList();

		public GlossaryCheck()
		{	
		}

		public override string ToString()
		{
			return "Controllo del glossario";
		}

		public override void Run(TranslationManager tManager)
		{
			SelectFileType sft = new SelectFileType();
			sft.ShowDialog();

			typeArray = sft.FileTypeList;

			int idxTerm = 0;
			int filledTerm = 0;
			int progress = 0;
			StreamWriter sw = null;

			transManager = tManager;

			terms.Clear();
			repetitions.Clear();
			targetWithVat.Clear();
			targetWithBlank.Clear();
			valueTooLong.Clear();

			XmlDocument xDoc = new XmlDocument();

			if (!File.Exists(Path.Combine(transManager.GetApplicationInfo().Path, "MagoNet-ITtoENGlossary.xml")))
			{
				MessageBox.Show("Non esiste un glossario per l'applicazione selezionata!");
				EndRun(false);
				return;
			}

			xDoc.Load(Path.Combine(transManager.GetApplicationInfo().Path, "MagoNet-ITtoENGlossary.xml"));

			XmlNodeList nList = xDoc.SelectNodes("Glossary/Term");
			int idxMax = nList.Count;
			int idxProg = 0;

			foreach (XmlNode nTerm in nList)
			{
				idxTerm++;
				idxProg = (int)((idxTerm * 100) / idxMax);
				string msg = string.Format("Elaborazione in corso: elemento {0} di {1} ({2}%)", idxTerm, idxMax, idxProg);
				SetProgressMessage(msg);
				string target = nTerm.Attributes["target"].Value.ToString();

				if (target.IndexOf(" ") >= 0)
					targetWithBlank.Add(nTerm);

				if (target.ToLower().IndexOf("tax") >= 0)
					targetWithVat.Add(nTerm);

				string types = nTerm.Attributes["types"].Value.ToString();

				if ((types.IndexOf("C") >= 0 || types.IndexOf("D") >= 0 || types.IndexOf("R") >= 0 || types.IndexOf("T") >= 0) && target.Length > 28)
				{
					valueTooLong.Add(nTerm);
				}

				/*if (types.IndexOf("R") >= 0 && target.Length > 20)
				{
					valueTooLong.Add(nTerm);
				}*/

				bool bAllowDuplicate = false;
				try
				{
					bAllowDuplicate = bool.Parse(nTerm.Attributes["allowDuplicate"].Value.ToString());
				}
				catch
				{
				}

				if (target != string.Empty)
				{
					filledTerm++;
					if (terms.Contains(target))
					{
						//if (types != "N")
						//{
						if (!repetitions.Contains(((XmlNode)terms[target]).OuterXml) && !bAllowDuplicate)
							repetitions.Add(((XmlNode)terms[target]).OuterXml, (XmlNode)terms[target]);

						if (!bAllowDuplicate)
							repetitions.Add(nTerm.OuterXml, nTerm);
						//}
					}
					else
						terms.Add(target, nTerm);
				}
				else if (CheckElementType(types))
				{
					string module = nTerm.Attributes["module"].Value.ToString();
					string source = nTerm.Attributes["source"].Value.ToString();
					string debMessage = string.Format("{0}|{1}|{2}", module, source, types);
					if (sw == null)
						sw = File.CreateText(Path.Combine(transManager.GetApplicationInfo().Path, "Missings.txt"));
					
					sw.WriteLine(debMessage);
					sw.Flush();
				}
			}

			progress = (int)((filledTerm * 100) / idxTerm);

			if (repetitions.Count > 0)
			{
				string message = string.Format("Trovate {0} ripetizioni!\nVuoi verificarne la correttezza?", repetitions.Count.ToString());
				DialogResult dr = MessageBox.Show(message, "Ripetizioni", MessageBoxButtons.YesNo);
				if (dr == DialogResult.Yes)
				{
					RepetitionManager rm = new RepetitionManager(transManager.GetApplicationInfo().Path, repetitions);
					rm .ShowDialog();
				}
			}
			if (targetWithBlank.Count > 0 || valueTooLong.Count > 0)
			{
				Repetitions r = new Repetitions(transManager.GetApplicationInfo().Path, repetitions, targetWithBlank, valueTooLong, targetWithVat);
				r.ShowDialog();
			}

			MessageBox.Show(string.Format("Sono stati tradotti {0} termini su {1} ({2}%)", filledTerm.ToString(), idxTerm, progress));

			if (sw != null)
				sw.Close();

			EndRun(false);
		}

		private bool CheckElementType(string type)
		{
			foreach (LookUpFileType luft in typeArray)
			{
				if (type.IndexOf(transManager.GetElementType(luft)) >= 0)
					return true;
			}
			return false;
		}
	}
}
