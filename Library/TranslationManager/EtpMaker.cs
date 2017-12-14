using System;
using System.IO;
using System.Xml;
using System.Collections;
using Microarea.TaskBuilderNet.Core.NameSolver;

namespace Microarea.Library.TranslationManager
{
	public class EtpMaker : SolutionManagerItems
	{
		private XmlDocument xEtp = new XmlDocument();
		private XmlNode nCurrentMod = null;
		private ArrayList acceptedFileType = new ArrayList();

		public EtpMaker()
		{
			acceptedFileType.Add(".h");
			acceptedFileType.Add(".cpp");
			acceptedFileType.Add(".rc");
			acceptedFileType.Add(".hrc");
			acceptedFileType.Add(".config");
			acceptedFileType.Add(".wrm");
			acceptedFileType.Add(".xml");
			acceptedFileType.Add(".sql");
			acceptedFileType.Add(".menu");
			acceptedFileType.Add(".gif");

			defaultLookUpType = LookUpFileType.Structure;
		}

		public override void Run(TranslationManager tManager)
		{
			transManager = tManager;

			if (!OpenLookUpDocument(false))
				return;

			XmlNode nMain = xLookUpDoc.SelectSingleNode("Application");
			string oldAppDir = Path.Combine(transManager.GetApplicationParentPath(), nMain.Attributes["source"].Value.ToString());
			string newAppDir = Path.Combine(transManager.GetApplicationParentPath(), nMain.Attributes["target"].Value.ToString());
			if (!Directory.Exists(newAppDir))
				return;
				

			//Leggo i moduli
			foreach (XmlNode nModule in nMain.SelectNodes("Module"))
			{
				SetProgressMessage(string.Format("Elaborazione in corso: {0}.etp", nModule.Attributes["target"].Value.ToString()));
				

				nCurrentMod = nModule;
				string oldModDir = Path.Combine(oldAppDir, nModule.Attributes["source"].Value.ToString());
				string newModDir = Path.Combine(newAppDir, nModule.Attributes["target"].Value.ToString());
				if (!Directory.Exists(newModDir))
					continue;

				string EtpFileName = Path.Combine(newModDir, nModule.Attributes["target"].Value.ToString() + ".etp");

				File.Copy(Path.Combine(oldAppDir, "ETPTemplate.etp"), EtpFileName, true);
				File.SetAttributes(EtpFileName, FileAttributes.Normal);
				xEtp.Load(EtpFileName);

				XmlNode nProjectExplorer = xEtp.SelectSingleNode("EFPROJECT/GENERAL/Views/ProjectExplorer");
				XmlNode nReferences = xEtp.SelectSingleNode("EFPROJECT/GENERAL/References");

				DirectoryInfo di = new DirectoryInfo(newModDir);
				foreach (DirectoryInfo cdi in di.GetDirectories())
				{
					if (!IsLibrary(nCurrentMod, cdi.Name))
					{
						AddDir(nProjectExplorer, nReferences, cdi, newModDir);
						//FolderCopy(cdi.FullName, Path.Combine(newModDir, cdi.Name));
					}
				}

				foreach (FileInfo fi in di.GetFiles())
				{
					if (CheckFileType(fi.Extension.ToLower()))
					{
						string relativePath = fi.FullName.Replace(newModDir + @"\", string.Empty);
						AddFile(nProjectExplorer, nReferences, relativePath);
					}
				}

				foreach (XmlNode nLibrary in nModule.SelectNodes("Library"))
				{
					XmlNode nFileV = xEtp.CreateNode(XmlNodeType.Element, "File", string.Empty);
					string folder = nLibrary.Attributes["destinationfolder"].Value.ToString();
					string vcproj = nLibrary.Attributes["target"].Value.ToString() + ".vcproj";
					nFileV.InnerText = Path.Combine(folder, vcproj);
					nProjectExplorer.AppendChild(nFileV);
					
					XmlNode nReference = xEtp.CreateNode(XmlNodeType.Element, "Reference", string.Empty);
					nReferences.AppendChild(nReference);

					XmlNode nFileR = xEtp.CreateNode(XmlNodeType.Element, "FILE", string.Empty);
					nFileR.InnerText = Path.Combine(folder, vcproj);
					XmlNode nGUIDPROJECTID = xEtp.CreateNode(XmlNodeType.Element, "GUIDPROJECTID", string.Empty);
					XmlDocument xVcproj = new XmlDocument();
					xVcproj.Load(Path.Combine(newModDir, Path.Combine(folder, vcproj)));
					string guid = xVcproj.SelectSingleNode("VisualStudioProject").Attributes["ProjectGUID"].Value.ToString();
					nGUIDPROJECTID.InnerText = guid;
					nReference.AppendChild(nFileR);
					nReference.AppendChild(nGUIDPROJECTID);
				}

				xEtp.Save(EtpFileName);
			}

			EndRun(false);
		}

		private void AddDir(XmlNode nParentFolder, XmlNode nReferences, DirectoryInfo di, string newModDir)
		{
			//TODO: Aggiungere un controllo di esistenza della cartella
			XmlNode nFolder = xEtp.CreateNode(XmlNodeType.Element, "Folder", string.Empty);
			XmlNode nFOLDERNAME = xEtp.CreateNode(XmlNodeType.Element, "FOLDERNAME", string.Empty);
			nFOLDERNAME.InnerText = di.Name;
			nFolder.AppendChild(nFOLDERNAME);
			nParentFolder.AppendChild(nFolder);
			
			foreach (DirectoryInfo cdi in di.GetDirectories())
			{
				AddDir(nFolder, nReferences, cdi, newModDir);
			}

			foreach (FileInfo fi in di.GetFiles())
			{
				if (CheckFileType(fi.Extension.ToLower()))
				{
					string relativePath = fi.FullName.Replace(newModDir + @"\", string.Empty);
					AddFile(nFolder, nReferences, relativePath);
				}
			}
		}

		private void AddFile(XmlNode nParentFolder, XmlNode nReferences, string relativePath)
		{
			XmlNode nFileV = xEtp.CreateNode(XmlNodeType.Element, "File", string.Empty);
			nFileV.InnerText = relativePath;
			nParentFolder.AppendChild(nFileV);

			XmlNode nReference = xEtp.CreateNode(XmlNodeType.Element, "Reference", string.Empty);
			nReferences.AppendChild(nReference);

			XmlNode nFileR = xEtp.CreateNode(XmlNodeType.Element, "FILE", string.Empty);
			nFileR.InnerText = relativePath;
			nReference.AppendChild(nFileR);
		}

		private bool CheckFileType(string fileType)
		{
			return acceptedFileType.Contains(fileType);
		}

		public override string ToString()
		{
			return "Creazione dell'ETP";
		}

	}
}
