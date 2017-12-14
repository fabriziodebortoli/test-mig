using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using Microarea.Tools.TBLocalizer.CommonUtilities;
using Microarea.Tools.TBLocalizer.Forms;
using Microarea.Tools.TBLocalizer.Properties;

namespace Microarea.Tools.TBLocalizer
{

	/// <summary>
	/// Genera l'assembly satellite associato ad un determinato dizionario.
	/// </summary>
	//=========================================================================
	public class AssemblyGenerator
	{
		public enum ConfigurationType { CFG_NONE, CFG_DEBUG, CFG_RELEASE, CFG_BOTH }

		private Logger logWriter = null;
		private ConfigurationType configuration = ConfigurationType.CFG_BOTH;
		private ProjectDocument tblPrjWriter;

		//---------------------------------------------------------------------
		public AssemblyGenerator(
			ConfigurationType configuration,
			Logger lw,
			ProjectDocument tblPrj)
		{
			tblPrjWriter = tblPrj;
			logWriter = lw;
			this.configuration = configuration;
		}

		//-----------------------------------------------------------------------------
		ConfigurationType Configuration { get { return configuration; } set { configuration = value; } }

		//---------------------------------------------------------------------
		public static bool CreateDictionaryForResx
			(
			ArrayList files,
			DictionaryTreeNode node,
			LocalizerTreeNode[] references,
			ProjectDocument aTblPrjWriter,
			Logger logWriter,
			AssemblyGenerator.ConfigurationType cfg
			)
		{

			string module = aTblPrjWriter.GetAssemblyName();
			AssemblyGenerator assemblyGenerator = new AssemblyGenerator
				(
				cfg,
				logWriter,
				aTblPrjWriter
				);

			return assemblyGenerator.CreateAssembly(files, node, references, module, cfg);
		}

		//-----------------------------------------------------------------------------
		public static string[] GetConfigurationPaths(ConfigurationType configuration)
		{
			switch (configuration)
			{
				case ConfigurationType.CFG_DEBUG: return new string[] { "Debug" };
				case ConfigurationType.CFG_RELEASE: return new string[] { "Release" };
				case ConfigurationType.CFG_BOTH: return new string[] { "Debug", "Release" };
			}

			return new string[0];

		}



		//-----------------------------------------------------------------------------
		void SetMessage(string message, TypeOfMessage type)
		{
			if (logWriter != null)
				logWriter.WriteLog(message, type);
		}

		//-----------------------------------------------------------------------------
		public bool CreateAssembly(ArrayList files, DictionaryTreeNode node, LocalizerTreeNode[] references, string module, ConfigurationType cfg)
		{
			bool result = true;
			string workingPath = null;

			try
			{
				if (module.Length == 0)
				{
					SetMessage(Strings.EmptyModuleName, TypeOfMessage.error);
					return false;
				}

				workingPath = PrepareSourceFiles(files, node, module);

				string[] configurations = GetConfigurationPaths(cfg);
				foreach (string configuration in configurations)
				{
					result = files.Count == 0 || CreateAssembly
						(
						workingPath,
						GetSatelliteAssemblyPath(node, configuration, true),
						module,
						node.Culture,
						configuration
						) && result;

					CopyReferences(node, references, configuration);
				}

			}
			catch (Exception ex)
			{
				SetMessage(string.Format("Error creating satellite assembly for module {0}: {1}", module, ex.Message), TypeOfMessage.error);
				return false;
			}
			finally
			{
				if (workingPath != null)
					Functions.SafeDeleteFolder(workingPath);
			}
			return result;
		}

		//-----------------------------------------------------------------------------
		private string PrepareSourceFiles(ArrayList files, DictionaryTreeNode node, string module)
		{
			string workingPath = Path.Combine(node.FileSystemPath, AllStrings.temporary);
			string newExtension = string.Format(".{0}{1}", node.Culture, AllStrings.resxExtension);
			try
			{
				XmlDocument projDoc = null;
				XmlElement itemGroup = null;
				if (tblPrjWriter.GetVersion() == "3")
				{
					projDoc = new XmlDocument();
					projDoc.LoadXml(Resources.netcore);

					XmlNode asmNode = projDoc.SelectSingleNode("Project/PropertyGroup/AssemblyName");
					asmNode.AppendChild(projDoc.CreateTextNode(module));

					XmlNode frmwkNode = projDoc.SelectSingleNode("Project/PropertyGroup/TargetFramework");
					frmwkNode.AppendChild(projDoc.CreateTextNode(tblPrjWriter.GetFrameworkVersion()));
					itemGroup = (XmlElement)projDoc.DocumentElement.GetElementsByTagName("ItemGroup")[0];
				}
				foreach (string file in files)
				{
					LocalizerDocument d = LocalizerDocument.GetStandardXmlDocument(file, false);
					string newPath = Path.Combine(workingPath, Path.GetFileName(file));
					newPath = Path.ChangeExtension(newPath, newExtension);
					if (projDoc != null)
					{
						XmlElement resEl = (XmlElement)itemGroup.AppendChild(projDoc.CreateElement("EmbeddedResource"));
						resEl.SetAttribute("Update", Path.GetFileName(newPath));
					}
					CopyReducedFile(CommonFunctions.GetCorrespondingBaseLanguagePath(file), newPath);
					LocalizerDocument.SaveResxFromXml(d, newPath, true, logWriter);
				}

				if (projDoc != null && files.Count > 0)
				{
					projDoc.Save(string.Concat(workingPath, "\\", module, AllStrings.csProjExtension));
				}
			}
			catch (Exception ex)
			{
				if (workingPath != null)
					Functions.SafeDeleteFolder(workingPath);

				throw ex;
			}
			return workingPath;
		}

		//-----------------------------------------------------------------------------
		private void CopyReducedFile(string source, string destination)
		{
			LocalizerDocument d = new LocalizerDocument();
			d.Load(source);
			bool isForm;

			string path = Path.GetDirectoryName(destination);
			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);

			XmlNodeList list = DataDocumentFunctions.GetResxData(d, out isForm);
			XmlNodeList all = DataDocumentFunctions.GetAllResxData(d);
			foreach (XmlNode n in all)
			{
				bool found = false;
				foreach (XmlNode n1 in list)
					if (n1 == n)
					{
						found = true;
						break;
					}
				if (!found)
					n.ParentNode.RemoveChild(n);
			}

			d.Save(destination);
		}

		//-----------------------------------------------------------------------------
		private bool CreateAssembly(string path, string satelliteAssemblyPath, string moduleName, string culture, string configuration)
		{

			if (moduleName == string.Empty)
			{
				Debug.Fail("Empty module name!");
				SetMessage(Strings.EmptyModuleName, TypeOfMessage.error);
				return false;
			}

			string assemblyName = moduleName + ".resources";
			string assemblyFileName = assemblyName + ".dll";

			try
			{
				if (!Directory.Exists(path)) return false;

				string cultureExt = culture + AllStrings.resxExtension;

				if (tblPrjWriter.GetVersion() == "1")
					ProduceAssembly_1_1(path, satelliteAssemblyPath, culture, assemblyFileName);
				else if (tblPrjWriter.GetVersion() == "2")
					SatelliteAssemblyBuilder.ProduceAssembly(path, satelliteAssemblyPath, culture, assemblyFileName, tblPrjWriter.GetFrameworkVersion());
				else if (tblPrjWriter.GetVersion() == "3")
					ProduceAssemblyNetCore(path, satelliteAssemblyPath);
				Application.DoEvents();
			}
			catch (Exception ex)
			{
				SetMessage(ex.Message, TypeOfMessage.error);
				return false;
			}
			finally
			{

			}

			logWriter.WriteLog(string.Format("Generated dictionary file: '{0}'", Path.Combine(satelliteAssemblyPath, assemblyFileName)));
			return true;
		}
		//-----------------------------------------------------------------------------
		internal void ProduceAssemblyNetCore(string path, string outputPath)
		{
			ProcessStartInfo info = new ProcessStartInfo("dotnet.exe", string.Concat("build ", path));
			info.UseShellExecute = false;
			info.RedirectStandardOutput = true;
			info.RedirectStandardError = true;
			info.CreateNoWindow = true;
			Process p = Process.Start(info);
			p.WaitForExit();
			CheckProcessStatus(p);

			foreach (var file in Directory.GetFiles(Path.Combine(path, "Bin"), "*.resources.dll", SearchOption.AllDirectories))
			{
				string dest = Path.Combine(outputPath, Path.GetFileName(file));
				if (File.Exists(dest))
					File.Delete(dest);
				File.Move(file, dest);
			}
		}

		//-----------------------------------------------------------------------------
		public void ProduceAssembly_1_1(string sourcePath, string outputPath, string culture, string assemblyFileName)
		{
			string path = Path.Combine(Application.StartupPath, "Tools\\SatelliteAssemblyBuilder.exe");
			if (!File.Exists(path))
				throw new FileNotFoundException(null, path);
			ProcessStartInfo info = new ProcessStartInfo(path, string.Format("{0} {1} {2} {3}", sourcePath, outputPath, culture, assemblyFileName));
			info.UseShellExecute = false;
			info.RedirectStandardOutput = true;
			info.RedirectStandardError = true;
			info.CreateNoWindow = true;
			Process p = Process.Start(info);
			p.WaitForExit();
			CheckProcessStatus(p);

		}
		//-----------------------------------------------------------------------------
		private void CopyReferences(DictionaryTreeNode node, LocalizerTreeNode[] references, string configuration)
		{
			string culture = node.Culture;
			string satelliteAssemblyPath = GetSatelliteAssemblyPath(node, configuration, true);

			// copio gli assembly referenziati
			foreach (LocalizerTreeNode projectNode in references)
			{
				ArrayList nodes = projectNode.GetTypedChildNodes(NodeType.LANGUAGE, false, node.Culture, true, node.Culture);
				if (nodes.Count != 1)
					continue;
				string originPath = GetSatelliteAssemblyPath((DictionaryTreeNode)nodes[0], configuration, false);
				if (!Directory.Exists(originPath)) continue;
				string[] files = Directory.GetFiles(originPath, "*.resources.dll");
				foreach (string file in files)
				{
					string destination = Path.Combine(satelliteAssemblyPath, Path.GetFileName(file));
					CommonUtilities.Functions.SafeCopyFile(file, destination, true);
				}
			}
		}

		/// <summary>
		/// restituisce il percorso di output dell'assembly satellite
		/// </summary>
		/// <param name="dictionaryPath">il percorso del dizionario (LINGUA COMPRESA!)</param>
		/// <param name="configuration"></param>
		/// <returns></returns>
		//-----------------------------------------------------------------------------
		static string GetSatelliteAssemblyPath(DictionaryTreeNode node, string configuration, bool create)
		{
			ProjectDocument project = DictionaryCreator.MainContext.GetPrjWriter(node);
			string assemblyPath = project.GetOutputPath(configuration);
			if (string.IsNullOrEmpty(assemblyPath))
				throw new ApplicationException("Cannot find assembly output path");
			assemblyPath = Path.GetFullPath(Path.Combine(project.SourceFolder, assemblyPath));

			assemblyPath = Path.Combine(assemblyPath, node.Culture);
			if (create && !Directory.Exists(assemblyPath))
				Directory.CreateDirectory(assemblyPath);

			return assemblyPath;
		}


		//-----------------------------------------------------------------------------
		static bool IsWebModule(string modulePath)
		{
			return File.Exists(Path.Combine(modulePath, "web.config"));
		}

		//-----------------------------------------------------------------------------
		static string GetResourcesPath(string modulePath, string culture, string configuration, bool create)
		{
			string resourcesPath = Path.Combine(modulePath, "Obj" + Path.DirectorySeparatorChar + configuration);

			resourcesPath = Path.Combine(resourcesPath, culture);
			if (create && !Directory.Exists(resourcesPath))
				Directory.CreateDirectory(resourcesPath);

			return resourcesPath;
		}

		//-----------------------------------------------------------------------------
		static string GetResourcesPath(DictionaryTreeNode node, string configuration, bool create)
		{
			string root = CommonFunctions.GetModuleFolder(node);

			return GetResourcesPath(root, node.Culture, configuration, create);
		}

		//-----------------------------------------------------------------------------
		private bool CheckProcessStatus(Process p)
		{
			string error;
			if (!CommonFunctions.CheckProcessStatus(p, out error))
			{
				SetMessage(Environment.NewLine + error, TypeOfMessage.error);
				return false;
			}

			return true;

		}
	}
}