using System;
using System.Collections;
using System.Xml;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;

namespace Microarea.Library.TranslationManager
{
	public class ReportNamesCheck : SolutionManagerItems
	{
		public ReportNamesCheck()
		{
		}

		public override string ToString()
		{
			return "Controllo dei nomi dei report";
		}

		public override void Run(TranslationManager tManager)
		{
			Hashtable reportCount = new Hashtable();
			ArrayList lengths = new ArrayList();
			int idxTerm = 0;
			StreamWriter sw = null;

			transManager = tManager;

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

				string types = nTerm.Attributes["types"].Value.ToString();

				if (types.IndexOf("R") >= 0 && target.Length > 20)
				{
					if (reportCount.ContainsKey(target.Length))
					{
						int count = (int)reportCount[target.Length];
						count ++;
						reportCount[target.Length] = count;
					}
					else
					{
						lengths.Add(target.Length);
						reportCount.Add(target.Length, 1);
					}

					
				}
			}

			if (reportCount.Count > 0)
			{
				lengths.Sort();
//				foreach (int lRef in lengths)
//				{
//					foreach (int l in lengths)
//					{
//						if (l < lRef)
//						{
//							int countL = (int)reportCount[l];
//							int countLREF = (int)reportCount[lRef];
//							reportCount[l] = countL + countLREF;
//						}
//					}
//				}

				if (sw == null)
					sw = File.CreateText(Path.Combine(transManager.GetApplicationInfo().Path, "ReportLenghts.txt"));

				foreach (int l in lengths)
				{
					sw.WriteLine(string.Format("{0}|{1}", l.ToString(), reportCount[l].ToString()));
					sw.Flush();
				}
			}

			EndRun(false);
		}
	}
}
