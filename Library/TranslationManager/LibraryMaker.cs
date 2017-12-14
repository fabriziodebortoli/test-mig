using System;
using System.Collections;
using System.Xml;
using System.IO;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.NameSolver;

namespace Microarea.Library.TranslationManager
{
	public class LibraryMaker : SolutionManagerItems
	{
		public LibraryMaker()
		{
			defaultLookUpType = LookUpFileType.Structure;
		}

		public override string ToString()
		{
			return "Library Maker";
		}

		public override void Run(TranslationManager tManager)
		{
			TxtReader = new RichTextBox();

			transManager = tManager;

			if (!OpenLookUpDocument(false))
				return;

			XmlNode nMain = xLookUpDoc.SelectSingleNode("Application");

			string appDir = Path.Combine(transManager.GetApplicationParentPath(), nMain.Attributes["target"].Value.ToString());
			if (!Directory.Exists(appDir))
				return;

			//Leggo i moduli
			foreach (XmlNode nModule in nMain.SelectNodes("Module"))
			{
				string modDir = Path.Combine(appDir, nModule.Attributes["target"].Value.ToString());
				if (!Directory.Exists(modDir))
					return;

				//Leggo le library
				foreach (XmlNode nLibrary in nModule.SelectNodes("Library"))
				{
					string oldLibName = nLibrary.Attributes["source"].Value.ToString();
					string newLibName = nLibrary.Attributes["target"].Value.ToString();

					string sourcePath = transManager.GetApplicationParentPath() + nMain.Attributes["source"].Value.ToString() + @"\" + nModule.Attributes["source"].Value.ToString() + @"\" + nLibrary.Attributes["sourcefolder"].Value.ToString();
					string destinationPath = Path.Combine(modDir, nLibrary.Attributes["destinationfolder"].Value.ToString());
					
					if (!Directory.Exists(destinationPath))
						return;

					//Rinomino il vcproj di libreria e i files di sistema
					string newVcprojFileName = Path.Combine(destinationPath, newLibName + ".vcproj");
					
					SetProgressMessage(string.Format("Elaborazione in corso: {0}.vcproj", newLibName));

					if (!File.Exists(newVcprojFileName))
					{
						File.Move(Path.Combine(destinationPath, oldLibName + ".vcproj"), newVcprojFileName);
						File.SetAttributes(newVcprojFileName, FileAttributes.Normal);
					}
					
					File.SetAttributes(newVcprojFileName, FileAttributes.Normal);
					
					string oldLibraryCPP = Path.Combine(destinationPath, oldLibName + ".cpp");
					string newLibraryCPP = Path.Combine(destinationPath, newLibName + ".cpp");
					
					string oldLibraryRC = Path.Combine(destinationPath, oldLibName + ".rc");
					string newLibraryRC = Path.Combine(destinationPath, newLibName + ".rc");
					
					string oldLibraryInterface = Path.Combine(destinationPath, oldLibName + "Interface.cpp");
					string newLibraryInterface = Path.Combine(destinationPath, newLibName + "Interface.cpp");

					XmlDocument dVcproj = new XmlDocument();
					dVcproj.Load(newVcprojFileName);

					foreach (XmlNode n in dVcproj.SelectNodes("VisualStudioProject/References/ProjectReference"))
					{
						string lName = n.Attributes["Name"].Value.ToString();

						n.Attributes["Name"].Value = transManager.GetLibraryNameTranslation(lName);
					}

					XmlNode nList = dVcproj.CreateNode(XmlNodeType.Element, "Lista", string.Empty);
					foreach (XmlNode n in dVcproj.SelectNodes("VisualStudioProject/Files/Filter/File"))
					{
						nList.AppendChild(n.Clone());
					}

					foreach (XmlNode n in dVcproj.SelectNodes("VisualStudioProject/Files/Filter/Filter/File"))
					{
						nList.AppendChild(n.Clone());
					}

					foreach (XmlNode n in dVcproj.SelectNodes("VisualStudioProject/Files/File"))
					{
						nList.AppendChild(n.Clone());
					}

					XmlNode nFiles = dVcproj.SelectSingleNode("VisualStudioProject/Files");
					nFiles.RemoveAll();

					XmlNode nSourcesFilter = dVcproj.CreateNode(XmlNodeType.Element, "Filter", string.Empty);
					XmlAttribute aName = dVcproj.CreateAttribute(string.Empty, "Name", string.Empty);
					aName.Value = "Source Files";
					XmlAttribute aFilter = dVcproj.CreateAttribute(string.Empty, "Filter", string.Empty);
					aFilter.Value = "cpp;c;cxx;def;odl;idl;hpj;bat;asm";
					nSourcesFilter.Attributes.Append(aName);
					nSourcesFilter.Attributes.Append(aFilter);

					XmlNode nHeadersFilter = dVcproj.CreateNode(XmlNodeType.Element, "Filter", string.Empty);
					aName = dVcproj.CreateAttribute(string.Empty, "Name", string.Empty);
					aName.Value = "Header Files";
					aFilter = dVcproj.CreateAttribute(string.Empty, "Filter", string.Empty);
					aFilter.Value = "h;hpp;hxx;hm;inl;inc";
					nHeadersFilter.Attributes.Append(aName);
					nHeadersFilter.Attributes.Append(aFilter);

					XmlNode nResourcesFilter = dVcproj.CreateNode(XmlNodeType.Element, "Filter", string.Empty);
					aName = dVcproj.CreateAttribute(string.Empty, "Name", string.Empty);
					aName.Value = "Resource Files";
					aFilter = dVcproj.CreateAttribute(string.Empty, "Filter", string.Empty);
					aFilter.Value = "rc;ico;cur;dlg;rc2;rct;bin;rgs";
					nResourcesFilter.Attributes.Append(aName);
					nResourcesFilter.Attributes.Append(aFilter);

					XmlNode nResourcesHeadersFilter = dVcproj.CreateNode(XmlNodeType.Element, "Filter", string.Empty);
					aName = dVcproj.CreateAttribute(string.Empty, "Name", string.Empty);
					aName.Value = "Headers (.hrc)";
					aFilter = dVcproj.CreateAttribute(string.Empty, "Filter", string.Empty);
					aFilter.Value = "hrc";
					nResourcesHeadersFilter.Attributes.Append(aName);
					nResourcesHeadersFilter.Attributes.Append(aFilter);

					XmlNode nImagesFilter = dVcproj.CreateNode(XmlNodeType.Element, "Filter", string.Empty);
					aName = dVcproj.CreateAttribute(string.Empty, "Name", string.Empty);
					aName.Value = "Images";
					aFilter = dVcproj.CreateAttribute(string.Empty, "Filter", string.Empty);
					aFilter.Value = "bmp;gif;jpg;jpeg;jpe";
					nImagesFilter.Attributes.Append(aName);
					nImagesFilter.Attributes.Append(aFilter);

					nFiles.AppendChild(nSourcesFilter);
					nFiles.AppendChild(nHeadersFilter);
					nResourcesFilter.AppendChild(nResourcesHeadersFilter);
					nResourcesFilter.AppendChild(nImagesFilter);
					nFiles.AppendChild(nResourcesFilter);

					foreach (XmlNode nFile in nList.SelectNodes("File"))
					{
						AddFile(nFile, nSourcesFilter, nHeadersFilter, nResourcesFilter, nResourcesHeadersFilter, nImagesFilter, nFiles, nLibrary);
					}

					XmlNode nVSP = dVcproj.SelectSingleNode("VisualStudioProject");

					nVSP.Attributes["Name"].Value = newLibName;

					foreach (XmlNode nTool in dVcproj.SelectNodes("VisualStudioProject/Configurations/Configuration/Tool[@Name='VCCLCompilerTool']"))
					{
						try
						{
							string sAddIncDir = nTool.Attributes["AdditionalIncludeDirectories"].Value.ToString();
							sAddIncDir = ReplaceNoCase(sAddIncDir, nMain.Attributes["source"].Value.ToString(), nMain.Attributes["target"].Value.ToString());
							nTool.Attributes["AdditionalIncludeDirectories"].Value = sAddIncDir;
						}
						catch (Exception)
						{
						}

						try
						{
							string sPreProcDef = nTool.Attributes["PreprocessorDefinitions"].Value.ToString();
							sPreProcDef = ReplaceNoCase(sPreProcDef, "_" + oldLibName + "EXT", "_" + newLibName.ToUpper() + "EXT");
							nTool.Attributes["PreprocessorDefinitions"].Value = sPreProcDef;
						}
						catch (Exception)
						{
						}
					}

					foreach (XmlNode nTool in dVcproj.SelectNodes("VisualStudioProject/Configurations/Configuration/Tool[@Name='VCLinkerTool']"))
					{
						try
						{
							string sOutputFile = nTool.Attributes["OutputFile"].Value.ToString();
							sOutputFile = ReplaceNoCase(sOutputFile, oldLibName + ".dll", newLibName + ".dll");
							nTool.Attributes["OutputFile"].Value = sOutputFile;
						}
						catch (Exception)
						{
						}

						try
						{
							string sImportLibrary = nTool.Attributes["ImportLibrary"].Value.ToString();
							sImportLibrary = ReplaceNoCase(sImportLibrary, oldLibName + ".lib", newLibName + ".lib");
							nTool.Attributes["ImportLibrary"].Value = sImportLibrary;
						}
						catch (Exception)
						{
						}
					}

					foreach (XmlNode nTool in dVcproj.SelectNodes("//Tool[@Name='VCResourceCompilerTool']"))
					{
						try
						{
							string sAddIncDir = nTool.Attributes["AdditionalIncludeDirectories"].Value.ToString();
							sAddIncDir = ReplaceNoCase(sAddIncDir, nMain.Attributes["source"].Value.ToString(), nMain.Attributes["target"].Value.ToString());
							nTool.Attributes["AdditionalIncludeDirectories"].Value = sAddIncDir;
						}
						catch (Exception)
						{
						}
					}

					dVcproj.Save(newVcprojFileName);

					if (File.Exists(newLibraryRC))
						FindAndReplaceInFile(newLibraryRC, oldLibName, newLibName);

					if (File.Exists(newLibraryInterface))
						FindAndReplaceInFile(newLibraryInterface, oldLibName, newLibName);
					
					if (File.Exists(Path.Combine(destinationPath, "beginh.dex")))
						FindAndReplaceInFile(Path.Combine(destinationPath, "beginh.dex"), oldLibName, newLibName);

					if (File.Exists(newLibraryCPP))
					{
						FindAndReplaceInFile(newLibraryCPP, oldLibName + ".DLL", newLibName + ".DLL");
						FindAndReplaceInFile(newLibraryCPP, oldLibName + "DLL", newLibName + "DLL");
					}
				}
			}
			
			EndRun(false);
		}

		private string ReplaceNoCase(string sFull, string sSource, string sTarget)
		{
			int startDir = sFull.ToLower().IndexOf(sSource.ToLower());
			if (startDir < 0)
				return sFull;

			string inizio = sFull.Substring(0, startDir);
			string fine = sFull.Substring(startDir + sSource.Length);

			return inizio + sTarget + fine;
		}

		private void AddFile(XmlNode nFile, XmlNode nSourcesFilter, XmlNode nHeadersFilter, XmlNode nResourcesFilter, XmlNode nResourcesHeadersFilter, XmlNode nImagesFilter, XmlNode nFiles, XmlNode nLibrary)
		{
			string fileName = nFile.Attributes["RelativePath"].Value.ToString();
			if (fileName.StartsWith(".\\"))
				fileName = fileName.Replace(".\\", string.Empty);

			switch (fileName.Split('.')[fileName.Split('.').Length-1])
			{
				case "cpp":
				case "CPP":
					nFile.Attributes["RelativePath"].Value = GetConversion(fileName, nLibrary).Replace(".CPP", ".cpp");
					nSourcesFilter.AppendChild(nFile);
					break;
				case "h":
				case "H":
					nFile.Attributes["RelativePath"].Value = GetConversion(fileName, nLibrary).Replace(".H", ".h");
					nHeadersFilter.AppendChild(nFile);
					break;
				case "rc":
				case "RC":
					nFile.Attributes["RelativePath"].Value = GetConversion(fileName, nLibrary);
					nResourcesFilter.AppendChild(nFile);
					break;
				case "hrc":
				case "HRC":
					nFile.Attributes["RelativePath"].Value = GetConversion(fileName, nLibrary);
					nResourcesHeadersFilter.AppendChild(nFile);
					break;
				case "gif":
				case "GIF":
				case "jpg":
				case "JPG":
				case "jpeg":
				case "JPEG":
				case "jpe":
				case "JPE":
					nImagesFilter.AppendChild(nFile);
					break;
				case "bmp":
				case "BMP":
					nFile.Attributes["RelativePath"].Value = @".\res\" + GetConversion(fileName.Substring(fileName.IndexOf(@"\") + 1), nLibrary);
					nImagesFilter.AppendChild(nFile);
					break;
				default:
					nFiles.AppendChild(nFile);
					break;
			}
		}

		private void FindAndReplaceInFile(string fileName, string source, string destination)
		{
			try
			{
				TxtReader.LoadFile(fileName, RichTextBoxStreamType.PlainText);
				TxtReader.Text = TxtReader.Text.Replace(source, destination);
				File.SetAttributes(fileName, FileAttributes.Normal);
				TxtReader.SaveFile(fileName, RichTextBoxStreamType.PlainText);
			}
			catch (Exception ex)
			{
				SetLogError("Errore: " + ex.Message, ToString());
			}
		}

		private void FindAndReplaceNoCaseInFile(string fileName, string source, string destination)
		{
			try
			{
				TxtReader.LoadFile(fileName, RichTextBoxStreamType.PlainText);
				TxtReader.Text = ReplaceNoCase(TxtReader.Text, source, destination);
				File.SetAttributes(fileName, FileAttributes.Normal);
				TxtReader.SaveFile(fileName, RichTextBoxStreamType.PlainText);
			}
			catch (Exception ex)
			{
				SetLogError("Errore: " + ex.Message, ToString());
			}
		}
	}
}
