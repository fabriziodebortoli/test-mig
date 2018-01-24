using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Xml;

namespace Microarea.Common.NameSolver
{
	/// <summary>
	/// Parsa il file di solution specificato per riempire l'elenco dei 
	/// moduli commerciali che contiene.
	/// </summary>
	public class SolutionInfo
	{
		private	StringCollection	modules			= null;
		private string				solutionFile	= string.Empty;

		//---------------------------------------------------------------------
		public StringCollection	Modules
		{	
			get	
			{
				if (modules != null)
					return modules;

				modules = new StringCollection();				
				Parse();
				return modules;	
			}	
		}

		//---------------------------------------------------------------------
		public SolutionInfo(string solutionFile)
		{
			this.solutionFile = solutionFile;	
		}

		//---------------------------------------------------------------------
		private bool Parse()
		{
			if	( 
					solutionFile == null			|| 
					solutionFile == string.Empty	|| 
					!PathFinder.PathFinderInstance.FileSystemManager.ExistFile(solutionFile)
				)
			{
				Debug.Assert(false);
				return false;
			}

			XmlDocument doc =  new XmlDocument();
            try
            {
                doc = PathFinder.PathFinderInstance.FileSystemManager.LoadXmlDocument(doc, solutionFile);
            }
            catch (XmlException exc)
            {
                Debug.Fail(exc.Message);
                return false;
            }

			XmlNodeList moduleNodes = doc.SelectNodes("//node()/@name");
			if (moduleNodes == null)
			{
				Debug.Assert(false);
				return false;
			}

			foreach (XmlNode moduleNode in moduleNodes)
			{
				modules.Add(moduleNode.Value);
			}

			return true;
		}
	}
}
