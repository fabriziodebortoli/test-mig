using System;
using System.Collections;
using System.Xml;
using System.IO;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.NameSolver;

namespace Microarea.Library.TranslationManager
{
	/// <summary>
	/// Summary description for CPPTranslator.
	/// </summary>
	//================================================================================
	public class GlossaryMerger : SolutionManagerItems
	{
		private Hashtable glossaryList = new Hashtable();
		private ArrayList errors = new ArrayList();
		public GlossaryMerger()
		{
			defaultLookUpType = LookUpFileType.Glossary;
		}

		public override string ToString()
		{
			return "Glossary Merger";
		}

		public override void Run(TranslationManager tManager)
		{
			transManager = tManager;

			OpenLookUpDocument(false);

			GlossaryMergerForm gmf = new GlossaryMergerForm(this);
			gmf.ShowDialog();

			EndRun(true);
		}

		public ArrayList Execute()
		{
			if (xLookUpDoc == null)
				return errors;

			foreach (XmlNode nTerm in xLookUpDoc.SelectNodes("Glossary/Term[@target='']"))
			{
				if (nTerm.Attributes["target"].Value.ToString() != string.Empty)
					continue;

				string term = GetTerm(nTerm.Attributes["source"].Value.ToString());

				if (term != string.Empty)
					nTerm.Attributes["target"].Value = term;
			}

			return errors;
		}

		private string GetTerm(string term)
		{
			ArrayList terms = new ArrayList();

			foreach (XmlDocument xDoc in glossaryList.Values)
			{
				string target = GetTerm(xDoc, term);
				if (target != string.Empty)
				{
					try
					{
						if (!terms.Contains(target))
							terms.Add(target);
					}
					catch
					{
					}
				}
			}

			if (terms.Count == 0)
				return string.Empty;

			if (terms.Count == 1)
				return terms[0].ToString();

			string error = string.Format("Il termine {0} ha le seguenti traduzioni: ", term);
			
			foreach (string t in terms)
				error += t + " ";

			return string.Empty;
		}

		private string GetTerm(XmlDocument xDoc, string term)
		{
			foreach (XmlNode nTerm in xDoc.SelectNodes("Glossary/Term"))
			{
				string source = nTerm.Attributes["source"].Value.ToString();
				if (source.ToLower() == term.ToLower())
				{
					return nTerm.Attributes["target"].Value.ToString();
				}
			}

			return string.Empty;
		}

		public bool AddGlossary(string glossaryFileName)
		{
			try
			{
				XmlDocument xDoc = new XmlDocument();
				xDoc.Load(glossaryFileName);
				XmlNodeList nList = xDoc.SelectNodes("Glossary/Term");
				if (nList == null || nList.Count <= 0)
					return false;

				glossaryList.Add(glossaryFileName, xDoc);
				return true;
			}
			catch
			{
				return false;
			}
		}
	}
}
