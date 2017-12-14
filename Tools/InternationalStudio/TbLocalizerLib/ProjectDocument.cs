using System;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Microarea.Tools.TBLocalizer.CommonUtilities;

namespace Microarea.Tools.TBLocalizer
{
	//================================================================================
	public class ProjectDocument : DataDocument
	{
		public enum ProjectType { NONE, CS, VC, VCX, TBL, NG }

		//--------------------------------------------------------------------------------
		public ProjectDocument()
		{
		}

		//--------------------------------------------------------------------------------
		internal void ForceModified()
		{
			modified = true;
		}

		//---------------------------------------------------------------------
		internal string SourceFolder
		{
			set
			{
				WriteObject("SourceFolder", CommonFunctions.PhysicalPathToLogicalPath(value), true);
			}
			get
			{
				object[] o = ReadObjects("SourceFolder", typeof(string));
				if (o.Length == 0)
					return null;

				return CommonFunctions.LogicalPathToPhysicalPath(o[0] as string);
			}
		}

		/// (tblprj)Restituisce l'attributo della root, che corrisponde all'estensione del file di progetto vc o cs.
		/// </summary>
		//---------------------------------------------------------------------
		internal ProjectType GetFileType()
		{
			if (currentDocument.DocumentElement == null)
				return ProjectType.NONE;
			XmlNode node = currentDocument.DocumentElement.SelectSingleNode("@" + AllStrings.type);
			if (node == null)
				return ProjectType.NONE;
			return ExtensionToType(node.Value);
		}

		//---------------------------------------------------------------------
		private static ProjectType ExtensionToType(string ext)
		{
			switch (ext)
			{
				case AllStrings.csProjExtension: return ProjectType.CS;
				case AllStrings.vcExtension: return ProjectType.VC;
				case AllStrings.vcxExtension: return ProjectType.VCX;
				case AllStrings.prjExtension: return ProjectType.TBL;
				case AllStrings.tsExtension: return ProjectType.NG;
				default: return ProjectType.NONE;
			}
		}
		//---------------------------------------------------------------------
		private static string TypeToExtension(ProjectType type)
		{
			switch (type)
			{
				case ProjectType.NONE:
					return "";
				case ProjectType.CS:
					return AllStrings.csProjExtension;
				case ProjectType.VC:
					return AllStrings.vcExtension;
				case ProjectType.VCX:
					return AllStrings.vcxExtension;
				case ProjectType.TBL:
					return AllStrings.prjExtension;
				default:
					return "";
			}
		}
		/// <summary>
		/// (all)Crea il file tblprj vuoto, come current document se non esiste 
		/// altrimenti lo carica e restituice il nome del file.
		/// </summary>
		/// <param name="root">cartella nbella quale si trova il file di progetto</param>
		/// <param name="extensionType">estensione del file di progetto originale</param>
		//---------------------------------------------------------------------
		internal string InitializeTblPrj(string sourceFolder, string tblprjFile, ProjectDocument.ProjectType extensionType)
		{
			InitDocument(AllStrings.configuration, TypeToExtension(extensionType), null, FileName, null);
			FileName = tblprjFile;
			SourceFolder = sourceFolder;
			return FileName;
		}

		/// <summary>
		/// (tblprj)Restituisce il nome dell'assembly.
		/// </summary>
		//---------------------------------------------------------------------
		internal string GetAssemblyName()
		{
			XmlNode node = currentDocument.SelectSingleNode
				(
				"//" +
				AllStrings.assemblyTag +
				"/@" +
				AllStrings.name
				);
			if (node == null) return String.Empty;
			return node.Value;
		}

		//---------------------------------------------------------------------
		internal string GetVersion()
		{
			XmlNode node = currentDocument.SelectSingleNode
				(
				"//" +
				AllStrings.versionTag +
				"/@" +
				AllStrings.name
				);
			if (node == null) return "1";
			return node.Value;
		}

		//---------------------------------------------------------------------
		internal string GetFrameworkVersion()
		{
			XmlNode node = currentDocument.SelectSingleNode
				(
				"//" +
				AllStrings.frameworkVersionTag +
				"/@" +
				AllStrings.name
				);
			if (node == null) return "v4.0";
			return node.Value;
		}
		//---------------------------------------------------------------------
		internal string GetOutputPath(string configuration)
		{
			object[] os = ReadObjects(AllStrings.outputPaths, typeof(OutputPath[]));
			if (os.Length == 1)
			{
				OutputPath[] paths = (OutputPath[])os[0];
				foreach (OutputPath op in paths)
					if (string.Compare(op.Configuration, configuration, StringComparison.InvariantCultureIgnoreCase) == 0)
						return op.Path;
			}
			return null;
		}
		/// <summary>
		/// (tblprj)Setta la reference col nome del progetto .
		/// </summary>
		/// <param name="name">nome della refernce</param>
		/// <param name="project">nome del progetto cui si riferisce la reference</param>
		//---------------------------------------------------------------------
		internal void SetReference(string name, string project)
		{
			XmlNode reference = currentDocument.SelectSingleNode
				(
				"//" +
				AllStrings.references +
				"/" +
				AllStrings.reference +
				CommonFunctions.XPathWhereClause(AllStrings.name, name)
				);
			if (reference != null && reference.Attributes[AllStrings.project] != null)
			{
				reference.Attributes[AllStrings.project].Value = project;
				ForceModified();
			}
		}

		/// <summary>
		/// (tblprj)Restituisce tutte le references: nome e progetto associato.
		/// </summary>
		//---------------------------------------------------------------------
		internal ArrayList GetReferences()
		{
			ArrayList list = new ArrayList();
			XmlNodeList listOfReferences = currentDocument.SelectNodes
				(
				"//" +
				AllStrings.references +
				"/" +
				AllStrings.reference
				);
			foreach (XmlElement n in listOfReferences)
			{
				string project = n.GetAttribute(AllStrings.project);
				string name = n.GetAttribute(AllStrings.name);
				list.Add(
					new DictionaryReference
					(
						name,
						project
					)
					);
			}
			return list;
		}

		/// <summary>
		/// (tblprj)Restituisce il nome del progetto relativo a quella reference
		/// </summary>
		/// <param name="name">nome della reference da cercare</param>
		//---------------------------------------------------------------------
		internal string GetProjectReferenced(string name)
		{
			XmlNode reference = currentDocument.SelectSingleNode
				(
				"//" +
				AllStrings.references +
				"/" +
				AllStrings.reference +
				CommonFunctions.XPathWhereClause(AllStrings.name, name)
				);
			if (reference != null && reference.Attributes[AllStrings.project] != null)
				return reference.Attributes[AllStrings.project].Value;
			return String.Empty;

		}

		/// <summary>
		/// (tblprj)Setta le references nel file di progetto accoppiando il nome della 
		/// reference con un progetto della solution con lo stesso nome.Solo se vuoto.
		/// </summary>
		/// <param name="listOfRef">lista di references lette dal file di progetto originale</param>
		/// <param name="projectList">lista dei path dei progetti della solution</param>
		//---------------------------------------------------------------------
		internal bool SetReferencesCouple(ArrayList listOfRef, TreeNodeCollection projectNodes)
		{
			XmlNode references = currentDocument.SelectSingleNode("//" + AllStrings.references);
			//se non esiste neanche il nodo references , lo creo
			if (references == null)
			{
				references = currentDocument.CreateElement(AllStrings.references);
				currentDocument.DocumentElement.AppendChild(references);
			}
			//mi segno una lista delle references contenute nel file tblprj
			XmlNodeList referencesListNow = currentDocument.SelectNodes
				("//" +
				AllStrings.references +
				"/" +
				AllStrings.reference +
				"/@" +
				AllStrings.name);
			foreach (string s in listOfRef)
			{
				//per ogni reference contenuta nella lista ma non presente nel tblprj, la aggiungo
				XmlNode reference = currentDocument.SelectSingleNode
					("//" +
					AllStrings.reference +
					CommonFunctions.XPathWhereClause(AllStrings.name, s));

				if (reference == null)
				{
					reference = currentDocument.CreateElement(AllStrings.reference);
					XmlAttribute attributeN = currentDocument.CreateAttribute(AllStrings.name);
					attributeN.Value = s;
					reference.Attributes.Append(attributeN);
					references.AppendChild(reference);
					ForceModified();

				}
				//setto il nome del progetto, tra queli presenti nella solution
				string correspondingProject = String.Empty;
				foreach (LocalizerTreeNode projectNode in projectNodes)
				{
					ProjectType type;
					ArrayList files = DataDocumentFunctions.GetProjectFiles(projectNode.SourcesPath, out type);
					if (type != ProjectType.CS)
						continue;

					string project = (string)files[0];
					string work = CommonFunctions.GetProjectName(project);
					if (String.Compare(s, work, true) == 0)
					{
						correspondingProject = work;
						break;
					}
				}
				//recupero l'attributo dove dovrebbe esserci scritto ilnome del progetto referenziato
				XmlNode attributeP = currentDocument.SelectSingleNode
					(
					"//" +
					AllStrings.reference +
					CommonFunctions.XPathWhereClause(AllStrings.name, s) +
					"/@" +
					AllStrings.project
					);
				if (attributeP == null)//attributo nuovo
				{
					attributeP = currentDocument.CreateAttribute(AllStrings.project);
					reference.Attributes.Append(attributeP as XmlAttribute);
					ForceModified();
				}
				if (attributeP.Value == String.Empty && correspondingProject != null && correspondingProject != String.Empty)
				{//Dentro l'attributo ci metto il nome del progetto referenziato se esiste
					attributeP.Value = correspondingProject;
					ForceModified();
				}

				if (attributeP.Value != String.Empty && (correspondingProject == null || correspondingProject == String.Empty))
				{
					//In questo caso la reference era stata settata sul relativo progetto forse in un'altra solution, 
					//in questa solution non esiste il corresponding prj, cancello l'informazione del progetto 
					//solo temporaneamnte per questa solution, quindi non flaggo Modified.
					attributeP.Value = String.Empty;
				}
			}
			//Ora controllo quali references segnate nel file sono da eliminare perchè non sono presenti nella lista
			ArrayList toRemove = new ArrayList();
			foreach (XmlNode n in referencesListNow)
			{
				if (n == null) continue;
				string name = n.Value;
				if (name == null || name == String.Empty) continue;
				if (listOfRef.Contains(name)) continue;
				toRemove.Add(name);
			}
			//elimino i nodi da qua, altrimenti non si può modificare la collection dentro un foreach
			foreach (string name in toRemove)
			{
				XmlNode reference = currentDocument.SelectSingleNode
					("//" +
					AllStrings.reference +
					CommonFunctions.XPathWhereClause(AllStrings.name, name));
				//									"[@" +
				//									AllStrings.name +
				//									"='" + 
				//									name + 
				//									"']");
				references.RemoveChild(reference);
				ForceModified();
			}
			return modified;
		}

		/// <summary>
		/// (tblprj)Svuota la reference a quel progetto.
		/// </summary>
		/// <param name="prj">progetto da eliminare come reference</param>
		//---------------------------------------------------------------------
		internal void DeleteReference(string prj)
		{
			XmlNode reference = currentDocument.SelectSingleNode
				(
				"//" +
				AllStrings.references +
				"/" +
				AllStrings.reference +
				CommonFunctions.XPathWhereClause(AllStrings.project, prj)
				);
			if (reference != null)
			{
				reference.Attributes[AllStrings.project].Value = String.Empty;
				//ForceModified();
			}
		}

		/// <summary>
		///	(tblprj)Ritorna tutte le references senza progetto associato.
		/// </summary>
		//---------------------------------------------------------------------
		internal XmlNodeList GetEmptyReferences()
		{
			return currentDocument.SelectNodes
				(
				"//" +
				AllStrings.references +
				"/" +
				AllStrings.reference +
				CommonFunctions.XPathWhereClause(AllStrings.project, String.Empty)
				);
		}

		/// <summary>
		/// (tblprj)Dopo che è stato aggiunto un prj alla solution vado a 
		/// vedere se con esso posso riempire qualche reference vuota.
		/// </summary>
		/// <param name="prj">progetto da associare alla reference</param>
		//---------------------------------------------------------------------
		internal void AddReference(string prj)
		{
			XmlNodeList references = GetEmptyReferences();
			if (references == null) return;
			string projectName = CommonFunctions.GetProjectName(prj);
			foreach (XmlNode reference in references)
			{
				if (reference.Attributes[AllStrings.name] != null &&
					string.Compare(reference.Attributes[AllStrings.name].Value, projectName, true) == 0 &&
					reference.Attributes[AllStrings.project] != null)

					reference.Attributes[AllStrings.project].Value = projectName;
			}
		}

		//--------------------------------------------------------------------------------
		public void Save(StringBuilder errorMessages = null)
		{
			Debug.Assert(FileName != null && FileName != string.Empty);

			if (!modified) return;

			string error = SaveAndReturnError(FileName, true);
			if (errorMessages != null && error != null && error != String.Empty)
			{
				errorMessages.Append(error);
				errorMessages.Append(Environment.NewLine);
			}
		}

		//--------------------------------------------------------------------------------
		public bool IsCsProject()
		{
			return GetFileType() == ProjectType.CS;
		}

		//--------------------------------------------------------------------------------
		public bool IsVcProject()
		{
			return GetFileType() == ProjectType.VC || GetFileType() == ProjectType.VCX;
		}
	}
}
