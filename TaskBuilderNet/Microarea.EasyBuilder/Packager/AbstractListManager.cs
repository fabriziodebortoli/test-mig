using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Microarea.EasyBuilder.Properties;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.EasyBuilder.Packager
{
	internal abstract class AbstractListManager : ICustomListManager
	{
		private readonly object lockTicket = new object();
		
		private ICustomList customList = new CustomList();
		private string customListFullPath;
		private bool isEnabled = true;
		private string application;
		private string module;
		private string myEasyBuilderAppFolder;

		//---------------------------------------------------------------------
		public AbstractListManager(string application, string module, string myEasyBuilderAppFolder)
		{
			this.application = application;
			this.module = module;

			this.myEasyBuilderAppFolder = myEasyBuilderAppFolder;
		}

		//---------------------------------------------------------------------
		public ICustomList CustomList { get { return customList; } }

		//---------------------------------------------------------------------
		public string CustomListFullPath { get { return customListFullPath; } set { customListFullPath = value; } }

		//---------------------------------------------------------------------
		public bool IsEnabled { get { return isEnabled; } set { isEnabled = value; } }

		//---------------------------------------------------------------------
		public string MyEasyBuilderAppFolder { get { return myEasyBuilderAppFolder; } }

		//-----------------------------------------------------------------------------
		public bool AddReadOnlyServerDocumentPartToCustomList(string filePath, bool save = true)
		{
			lock (lockTicket)
			{
				ICustomListItem cItem = CustomList.FindItem(filePath);
				bool changed = false;
				if (cItem == null)
				{
					cItem = new CustomListItem(filePath, false, String.Empty, CalculateItemSource(filePath), string.Empty);
					cItem.IsReadOnlyServerDocumentPart = true;

					CustomList.Add(cItem);
					changed = true;
				}

				//il bool changed serve per evitare di salvare s enon ci sono state modifiche anche se è stato specificato di salvare.
				if (save && changed)
					SaveCustomList();

				return true;
			}
		}

		//-----------------------------------------------------------------------------
		public bool AddToCustomList(
			string filePath,
			bool save = true,
			bool isActiveDocument = false,
			string publishedUser = "", 
			string documentNamespace = ""
			)
		{
			lock (lockTicket)
			{
				ICustomListItem cItem = CustomList.FindItem(filePath);
				if (cItem != null)
				{
					cItem.IsActiveDocument = isActiveDocument;
					cItem.PublishedUser = publishedUser;
				}
				else
				{
					cItem = new CustomListItem(filePath, isActiveDocument, publishedUser, CalculateItemSource(filePath), documentNamespace);
					CustomList.Add(cItem);
				}

				if (isActiveDocument)
					SetActiveDocument(filePath);

				if (save)
					SaveCustomList();

				return true;
			}
		}

		//---------------------------------------------------------------------
		public void RemoveFromCustomListAndFromFileSystem(string filePath)
		{
			if (filePath.IsNullOrEmpty())
				return;

			CustomList.Remove(filePath);

			try
			{
				if (File.Exists(filePath))
					File.Delete(filePath);
				//se è una dll, elimino anche i file cs o vb se ci sono
				if (filePath.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase))
				{
					foreach (var ext in NewCustomizationInfos.AllSupportedExtensions)
					{
						string filePathToDelete = Path.ChangeExtension(filePath, ext);
						if (File.Exists(filePathToDelete))
							File.Delete(filePathToDelete);
					}
				}

				//Se la cartella rimanente non contiene file ne` sottocartelle ed e` all'interno del file system
				//della mia applicazione allora la posso rimuovere.
				DirectoryInfo di = new DirectoryInfo(Path.GetDirectoryName(filePath));
				if (IsInMyEasyBuilderAppFolder(di.FullName) && di.GetFiles().Length == 0 && di.GetDirectories().Length == 0)
					di.Delete(true);
			}
			catch (Exception exc)
			{
				string message = string.Format(Resources.ErrorDeletingItem, filePath, exc.Message);
				BaseCustomizationContext.CustomizationContextInstance.Diagnostic.SetError(message);
			}
		}

		//-----------------------------------------------------------------------------
		public Stream GetStreamFromCustomList(ICustomList list)
		{
			try
			{
				//Genero il file xml della custom list
				XmlDocument xDoc = new XmlDocument();
				XmlElement root = xDoc.CreateElement(GetRootXmlTag());
				root.SetAttribute(EasyBuilderAppListXML.Attribute.Enabled, IsEnabled.ToString());
				xDoc.AppendChild(root);

				foreach (CustomListItem current in list)
				{
					XmlElement el = xDoc.CreateElement(GetItemXmlTag());
					el.SetAttribute(EasyBuilderAppListXML.Attribute.RelativePath, PurgePath(current.FilePath));

					if (current.IsActiveDocument)
						el.SetAttribute(EasyBuilderAppListXML.Attribute.IsActiveDocument, current.IsActiveDocument.ToString());

					if (!current.PublishedUser.IsNullOrEmpty())
						el.SetAttribute(EasyBuilderAppListXML.Attribute.PublishedUser, current.PublishedUser);

					if (current.IsReadOnlyServerDocumentPart)
						el.SetAttribute(EasyBuilderAppListXML.Attribute.IsReadOnlyServerDocumentPart, current.IsReadOnlyServerDocumentPart.ToString());

					el.SetAttribute(EasyBuilderAppListXML.Attribute.ItemSource, current.ItemSource.ToString());

					if (!current.DocumentNamespace.IsNullOrEmpty())
						el.SetAttribute(EasyBuilderAppListXML.Attribute.DocumentNamespace, current.DocumentNamespace);

					root.AppendChild(el);
				}

				Stream s = new MemoryStream();
				xDoc.Save(s);
				return s;
			}
			catch (Exception e)
			{
				BaseCustomizationContext.CustomizationContextInstance.Diagnostic.SetError(string.Format(Resources.SaveCustomListError, e.Message));
			}

			return null;
		}

		//-----------------------------------------------------------------------------
		public void LoadCustomList()
		{
			if (String.IsNullOrWhiteSpace(CustomListFullPath) || !File.Exists(CustomListFullPath))
				return;

			using (Stream input = File.OpenRead(CustomListFullPath))
			{
				LoadCustomList(input);
			}
		}

		//-----------------------------------------------------------------------------
		public void LoadCustomList(Stream stream)
		{
			CustomList.Clear();

			if (stream == null)
				return;

			try
			{
				XmlDocument xDoc = new XmlDocument();
				xDoc.Load(stream);
				XmlNode root = xDoc.SelectSingleNode(string.Format("//{0}", GetRootXmlTag()));
				if (root == null)
					return;

				if (root.Attributes[EasyBuilderAppListXML.Attribute.Enabled] != null)
				{
					string enable = root.Attributes[EasyBuilderAppListXML.Attribute.Enabled].Value;
					bool isEnabled = false;
					bool.TryParse(enable, out isEnabled);
					IsEnabled = isEnabled;
				}

				XmlNodeList list = xDoc.SelectNodes(string.Format(
						"//{0}/{1}",
						GetRootXmlTag(),
						GetItemXmlTag()
						));

				CustomListItem aItem = null;
				XmlAttribute bufferAttr = null;
				foreach (XmlNode current in list)
				{
					bufferAttr = current.Attributes[EasyBuilderAppListXML.Attribute.RelativePath];
					if (bufferAttr == null)
						continue;

					bufferAttr = current.Attributes[EasyBuilderAppListXML.Attribute.ItemSource];
					string itemSourceStr = null;
					if (bufferAttr != null)
						itemSourceStr = bufferAttr.Value;

					//Parso qui ItemSource per poter essere in grado dopo di invocare la RestorePath.
					//L'oggetto CustomListItem è in grado da solo di calcolare ItemSource in pase al file path passatogli.
					ItemSource itemSource = GetDefaultValueForItemSource();
					Enum.TryParse<ItemSource>(itemSourceStr, out itemSource);

					string relativePathAttributeValue = current.Attributes[EasyBuilderAppListXML.Attribute.RelativePath].Value;

					//Nella correzione dell'anomalia 19021 devo, per retrocompatibilità, badare al fatto che ci siano percorsi
					//assoluti nella custom list e, se ci sono, devo purge-arli e calcolare il giusto valore perItemSource.
					PurgePathIfAbsolute(ref relativePathAttributeValue, ref itemSource);

					string file = RestorePath(relativePathAttributeValue, itemSource);

					bufferAttr = current.Attributes[EasyBuilderAppListXML.Attribute.IsActiveDocument];
					string isActive = bufferAttr == null
						? "false"
						: bufferAttr.Value;

					bool isActiveDoc = false;
					bool.TryParse(isActive, out isActiveDoc);

					bufferAttr = current.Attributes[EasyBuilderAppListXML.Attribute.PublishedUser];
					string publishedUser = bufferAttr == null
					? string.Empty
					: bufferAttr.Value;

					bufferAttr = current.Attributes[EasyBuilderAppListXML.Attribute.DocumentNamespace];
					string documentNamespace = bufferAttr == null
					? string.Empty
					: bufferAttr.Value;

					aItem = new CustomListItem(file, isActiveDoc, publishedUser, itemSource, documentNamespace);
					CustomList.Add(aItem);

					bufferAttr = current.Attributes[EasyBuilderAppListXML.Attribute.IsReadOnlyServerDocumentPart];
					string isReadOnlyServerDocumentPartStr = bufferAttr == null
						? "false"
						: bufferAttr.Value;

					bool isReadOnlyServerDocumentPart = false;
					bool.TryParse(isReadOnlyServerDocumentPartStr, out isReadOnlyServerDocumentPart);

					aItem.IsReadOnlyServerDocumentPart = isReadOnlyServerDocumentPart;
				}
			}
			catch (Exception e)
			{
				BaseCustomizationContext.CustomizationContextInstance.Diagnostic.SetError(string.Format(Resources.LoadCustomListError, e.Message));
			}

			//Carico anche gli active documents
			foreach (CustomListItem item in CustomList)
			{
				if (!item.IsActiveDocument)
					continue;

				string custNs = BaseCustomizationContext.CustomizationContextInstance.GetPseudoNamespaceFromFullPath(item.FilePath, item.PublishedUser);
				if (custNs.IsNullOrEmpty())
					continue;

				if (!BaseCustomizationContext.CustomizationContextInstance.ActiveDocuments.ContainsNoCase(custNs))
					BaseCustomizationContext.CustomizationContextInstance.ActiveDocuments.Add(custNs);
			}
		}

		//-----------------------------------------------------------------------------
		public void SaveCustomList()
		{
			try
			{
				//Se il file su disco non esiste e sono vuoto allora non salvo nulla.
				if (!File.Exists(CustomListFullPath) && CustomList.Count == 0)
					return;

				using (StreamWriter sw = new StreamWriter(CustomListFullPath))
				using (Stream aStream = GetStreamFromCustomList(CustomList))
				{
					aStream.Seek(0, SeekOrigin.Begin);
					using (StreamReader sr = new StreamReader(aStream))
						sw.Write(sr.ReadToEnd());
				}
			}

			catch (Exception e)
			{
				BaseCustomizationContext.CustomizationContextInstance.Diagnostic.SetError(string.Format(Resources.SaveCustomListError, e.Message));
			}
		}

		/// <summary>
		/// Purges from the current custom list all entries not existing on file system.
		/// </summary>
		//--------------------------------------------------------------------------------
		public void PurgeCustomListFromNonExistingFiles()
		{
			ICustomListItem item = null;
			for (int i = 0; i < CustomList.Count; i++)
			{
				item = CustomList[i];
				if (item == null)
					continue;

				if (File.Exists(item.FilePath))
					continue;

				CustomList.Remove(item);
			}
		}

		/// <summary>
		/// Rinomina i path comprendenti nomi azienda all'interno del file di custom list con i nuovi nomi
		/// scelti dall'utente
		/// </summary>
		/// <param name="renamedCompanies"></param>
		//-----------------------------------------------------------------------------
		public void RenameCompanyPathsInCustomList(IList<IRenamedCompany> renamedCompanies)
		{
			for (int i = 0; i < CustomList.Count; i++)
			{
				foreach (RenamedCompany item in renamedCompanies)
				{
					//Se il nome non è cambiato non faccio niente
					if (item.OldCompanyName.CompareNoCase(item.NewCompanyName))
						continue;

					//Sostituisco nei file memorizzati le aziende vecchie con le nuove
					CustomList[i].FilePath = CustomList[i].FilePath.ReplaceNoCase
						(
						Path.Combine(NameSolverStrings.Custom, NameSolverStrings.Companies, item.OldCompanyName),
						Path.Combine(NameSolverStrings.Custom, NameSolverStrings.Companies, item.NewCompanyName)
						);
				}
			}
		}

		//---------------------------------------------------------------------
		public bool ContainsControllerDll(INameSpace controllerNamespace)
		{
			string purgedPath = null;
			foreach (CustomListItem item in CustomList)
			{
				if (Path.GetExtension(item.FilePath) != NameSolverStrings.DllExtension)
					continue;

				purgedPath = PurgePath(item.FilePath);
				if (
					GetApplicationNameFromPurgedPath(purgedPath) == controllerNamespace.Application &&
					GetModuleNameFromPurgedPath(purgedPath) == controllerNamespace.Module &&
					GetDocumentNameFromPurgedPath(purgedPath) == controllerNamespace.Document &&
					Path.GetFileNameWithoutExtension(purgedPath) == controllerNamespace.Leaf
					)
					return true;
			};

			return false;
		}

		//Ritorna true se la customlist contiene un altro controller che agisce sullo stesso documento su
		//cui agisce il controller passato coem parametro
		//---------------------------------------------------------------------
		public bool ContainsOtherControllerDll(INameSpace controllerNamespace)
		{
			if (controllerNamespace == null || controllerNamespace == NameSpace.Empty)
				return false;

			string purgedPath = null;
			foreach (CustomListItem item in CustomList)
			{
				if (Path.GetExtension(item.FilePath) != NameSolverStrings.DllExtension)
					continue;

				purgedPath = PurgePath(item.FilePath);
				if (
					GetApplicationNameFromPurgedPath(purgedPath) == controllerNamespace.Application &&
					GetModuleNameFromPurgedPath(purgedPath) == controllerNamespace.Module &&
					GetDocumentNameFromPurgedPath(purgedPath) == controllerNamespace.Document
					)
					return true;
			};

			return false;
		}

		//-----------------------------------------------------------------------------
		public string[] GetAllReadOnlyServerDocPart(string fullPath)
		{
			if (String.IsNullOrWhiteSpace(fullPath))
				return new string[] { };

			//Le dll che costituiscono le parti read-only di un server doc
			//sono tutte quelle dll nella cartella del documento tranne quella indicata da fullPath
			string folderPath = Path.GetDirectoryName(fullPath);
			List<string> readOnlyServerDocPartPaths = new List<string>();

			string extension = Path.GetExtension(fullPath);
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fullPath);

			string currentItemPath = null;
			foreach (var item in CustomList)
			{
				if (!item.IsReadOnlyServerDocumentPart)
					continue;

				currentItemPath = item.FilePath;
				if (
					extension.CompareNoCase(Path.GetExtension(currentItemPath)) &&
					folderPath.CompareNoCase(Path.GetDirectoryName(currentItemPath)) &&
					!fileNameWithoutExtension.CompareNoCase(Path.GetFileNameWithoutExtension(currentItemPath))//skip-o il file che mi arriva come parametro perche` verra` eliminato dopo.
					)
					readOnlyServerDocPartPaths.Add(currentItemPath);
			}

			return readOnlyServerDocPartPaths.ToArray();
		}

		//---------------------------------------------------------------------
		private void SetActiveDocument(string purgedPath)
		{
			if (purgedPath.IsNullOrEmpty())
				return;

			CustomList.SetActiveDocument(purgedPath);
		}

		//Se relativePath e` invece un path assoluto allora lo monda della parte di percorso fino a Custom o Standard comrpesa.
		//Es:	se relativePath = "C:\Program Files (x86)\Microarea\MagoNet\Standard\Applications\ERP\CostAccounting\Report"
		//		=> relativePath verra` calcolato come Applications\ERP\CostAccounting\Report
		//
		//Ritorna true se ha effettivamente operato modifiche su relativePath, false altrimenti.
		//Se ritorna true, allora valorizza in maniera consna anche itemSource
		//-----------------------------------------------------------------------------
		internal static bool PurgePathIfAbsolute(ref string relativePath, ref ItemSource itemSource)
		{
			if (String.IsNullOrWhiteSpace(relativePath) || !Path.IsPathRooted(relativePath))
				return false;

			string[] tokens = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

			StringBuilder relPathBuilder = new StringBuilder();
			string Custom = NameSolverStrings.Custom;
			string Standard = NameSolverStrings.Standard;
			bool customFound = false;
			bool standardFound = false;
			foreach (var token in tokens)
			{
				if (!customFound && !standardFound && String.Compare(token, Custom, StringComparison.OrdinalIgnoreCase) == 0)
				{
					customFound = true;
					continue;//skip-o il token Custom
				}
				if (!customFound && !standardFound && String.Compare(token, Standard, StringComparison.OrdinalIgnoreCase) == 0)
				{
					standardFound = true;
					continue;//skip-o il token Standard
				}
				if (customFound || standardFound)
				{
					relPathBuilder.Append(token).Append(Path.DirectorySeparatorChar);
				}
			}

			if (customFound)
				itemSource = ItemSource.Custom;
			if (standardFound)
				itemSource = ItemSource.Standard;

			relativePath = relPathBuilder.ToString(0, relPathBuilder.Length - 1);//Elimino l' ultimo /

			return customFound || standardFound;
		}

		//-----------------------------------------------------------------------------
		private bool IsInMyEasyBuilderAppFolder(string path)
		{
			if (String.IsNullOrWhiteSpace(path))
				return false;

			return path.IndexOfNoCase(MyEasyBuilderAppFolder) > -1;
		}

		//-----------------------------------------------------------------------------
		internal static ItemSource CalculateItemSource(string filePath)
		{
			if (String.IsNullOrWhiteSpace(filePath) || filePath.IndexOfNoCase(NameSolverStrings.Standard) > -1)
				return ItemSource.Standard;

			return ItemSource.Custom;
		}

		protected abstract ItemSource GetDefaultValueForItemSource();
		protected abstract string PurgePath(string fullPath);
		protected abstract string RestorePath(string relativePath, ItemSource itemSource);
		protected abstract string GetApplicationNameFromPurgedPath(string path);
		protected abstract string GetModuleNameFromPurgedPath(string path);
		protected abstract string GetDocumentNameFromPurgedPath(string path);
		protected abstract string GetRootXmlTag();
		protected abstract string GetItemXmlTag();
	}
}
