using System.Xml;

namespace Microarea.Tools.TBLocalizer
{
	public class IniDocument : DataDocument
	{
		public IniDocument()
		{
			
		}

		/// <summary>
		/// (tbl)Modifica tbl a seguito di rinomina solution o per cancellazione di solution.
		/// </summary>
		/// <param name="source">nome prima della rinominazione</param>
		/// <param name="dest">nuovo nome</param>
		//---------------------------------------------------------------------
		internal  void INIModifier(string source, string dest)
		{
			XmlNode oldNode = currentDocument.SelectSingleNode 
				(
				"//" + 
				AllStrings.solution + 
				CommonFunctions.XPathWhereClause (AllStrings.path, source) +
				"/@" + 
				AllStrings.path 
				
				);
			if (oldNode == null) return;
			//rimozione
			if (dest == null)	RemoveSolution(source);	
				//sostituzione			
			else				oldNode.Value = dest;
			modified = true;
			SaveAndShowError("DataDocument - INIModifier", true);
		}

		/// <summary>
		/// (tbl)Restituisce l'elenco dei path delle solution del file tbl.
		/// </summary>
		//---------------------------------------------------------------------
		internal  XmlNodeList GetAllSolutionPaths()
		{
			return currentDocument.SelectNodes
				(
				"//" + 
				AllStrings.solution + 
				"/@" + 
				AllStrings.path
				);
		}

		/// <summary>
		///  (tbl)Se il file tbl contiene già la solution la rimuove e fa in modo di averne solo 4 nell'elenco.
		/// </summary>
		/// <param name="tblslnPath">path della solution inserita</param>
		//---------------------------------------------------------------------
		private void IniMaintenance( string tblslnPath)
		{
			XmlNodeList nodes = GetAllSolutionPaths();
			bool removed = false;
			//default di max 4 solution memorizzate
			foreach (XmlNode n in nodes)
			{
				if (n.Value.ToLower() == tblslnPath.ToLower())	
				{
					RemoveSolution(n.Value);
					removed = true;
					break;
				}
			}
			if (nodes.Count >= 4 && !removed)
				RemoveOldestSolution();
		}

		/// <summary>
		/// (tbl)Rimuove la solution dal file.
		/// </summary>
		/// <param name="slnPath">path della solution da rimuovere</param>
		//---------------------------------------------------------------------
		public void RemoveSolution(string slnPath)
		{
			XmlNode nodeToRemove = currentDocument.SelectSingleNode 
				(
				"//" + 
				AllStrings.solution + 
				CommonFunctions.XPathWhereClause(AllStrings.path, slnPath)
				);			
			if (nodeToRemove != null)
				currentDocument.DocumentElement.RemoveChild(nodeToRemove);
		}
		
		/// <summary>
		/// (tbl)Cancella la solution più vecchia(ultimo elemento)dal file tbl per far spazio a quella nuova.
		/// </summary>
		//---------------------------------------------------------------------
		private void RemoveOldestSolution()
		{
			currentDocument.DocumentElement.RemoveChild(currentDocument.DocumentElement.LastChild);
		}

		/// <summary>
		/// (tbl)Aggiunge le solutions salvate al file tbl.
		/// </summary>
		/// <param name="tblslnPath">path del file di solution da aggiungere</param>
		//---------------------------------------------------------------------
		internal void WriteINI(string tblslnPath)
		{
			if (currentDocument == null)
				currentDocument.Load(AllStrings.INI);
			IniMaintenance(tblslnPath);
						
			XmlNode node		= currentDocument.DocumentElement;
			XmlElement elem		= currentDocument.CreateElement(AllStrings.solution);
			XmlAttribute path	= currentDocument.CreateAttribute(AllStrings.path);
			path.Value			= tblslnPath;
			elem.Attributes.SetNamedItem(path);
			node.PrependChild(elem) ;
			modified = true;
			SaveAndShowError("DataDocument - WriteINI", true);
		}
	}
}
