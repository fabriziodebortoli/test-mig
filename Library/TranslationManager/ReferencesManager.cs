using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Diagnostics;

using Microarea.TaskBuilderNet.Core.NameSolver;

namespace Microarea.Library.TranslationManager
{
	/// <summary>
	/// Summary description for ReferencesManager.
	/// </summary>
	public partial class ReferencesManager : System.Windows.Forms.Form
	{
		private BasePathFinder pathFinder = new BasePathFinder();
		private XmlDocument xDoc = new XmlDocument();
		private Hashtable setVariables = new Hashtable();
		private Hashtable vcprojs = new Hashtable();
		private string applicationName = string.Empty;
		private BaseApplicationInfo wApplication = null;

		public ReferencesManager()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		public ReferencesManager(string appName)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			applicationName = appName;
		}

	

		private void ReferencesManager_Load(object sender, System.EventArgs e)
		{
			Process myProcess = new Process();
            
			string commandLine = @"C:\WINDOWS\system32\cmd.exe";
			string commandArgs = @"/C set > c:\tmp.txt";

			myProcess.StartInfo.FileName = commandLine; 
			myProcess.StartInfo.Arguments = commandArgs;
			myProcess.StartInfo.CreateNoWindow = true;
			myProcess.StartInfo.UseShellExecute = false;
			myProcess.Start();
			myProcess.WaitForExit();

			TxtReader.LoadFile(@"c:\tmp.txt", RichTextBoxStreamType.PlainText);
			
			string riga = string.Empty;
			StringReader sr = new StringReader(TxtReader.Text);
			while (true) 
			{
				riga = sr.ReadLine();
				if (riga == null) 
				{
					break;
				}
				
				if (riga.ToLower().IndexOf("microarea") > 0)
					setVariables.Add(riga.Split('=')[0], riga.Split('=')[1]);
			}

			File.Delete(@"c:\tmp.txt");

			pathFinder.Init();

			int MNIndex = 0;

			foreach (BaseApplicationInfo ai in pathFinder.ApplicationInfos)
			{
				CMBApplicazione.Items.Add(ai);
				if (ai.Name == "MagoNet")
					MNIndex = CMBApplicazione.Items.IndexOf(ai);

				if (ai.Name == applicationName)
					wApplication = ai;

				FindVcprojs(new DirectoryInfo(ai.Path));
				if (wApplication != null)
				{
					ConvertiApplicazione();
					MessageBox.Show("Elaborazione terminata!");
					wApplication = null;
					Close();
				}
			}

			if (CMBApplicazione.Items.Count > 0)
				CMBApplicazione.SelectedIndex = MNIndex;
		}

		private void ConvertiApplicazione()
		{
			foreach (BaseModuleInfo mi in wApplication.Modules)
			{
				foreach (LibraryInfo li in mi.Libraries)
				{
					string appPath = wApplication.Path;
					string modPath = mi.Path;
					string libPath = li.SourceFolder.ToLower().Replace("\\bin", string.Empty);
					string vcprojName = string.Format("{0}.vcproj", li.Name);
					string vcprojPath = Path.Combine(appPath, Path.Combine(modPath, Path.Combine(libPath, vcprojName)));
					try
					{
						if (File.Exists(vcprojPath))
							xDoc.Load(vcprojPath);
						else
						{
							//Devo chiedere il file da aprire
						}
						foreach (XmlNode nConfiguration in xDoc.SelectNodes("VisualStudioProject/Configurations/Configuration"))
						{
							ConfigurationNode cn = new ConfigurationNode(nConfiguration);

							if (cn == null)
								return;

							string additionalDependencies = string.Empty;
							string additionalLibraryDirectories = string.Empty;

							foreach (XmlNode n in cn.nConf.SelectNodes("Tool"))
							{
								if (n.Attributes["Name"].Value.ToString() == "VCLinkerTool")
								{
									additionalDependencies = n.Attributes["AdditionalDependencies"].Value.ToString();
									additionalLibraryDirectories = n.Attributes["AdditionalLibraryDirectories"].Value.ToString();
									break;
								}
							}

							foreach (string s in additionalDependencies.Split(' '))
							{
								if (s != null && s != string.Empty)
								{
									string lib = s;
									string oFile = string.Empty;
									foreach (string d in additionalLibraryDirectories.Split(';'))
									{
										if (d != null && d != string.Empty)
											if (d.ToLower().IndexOf(s.ToLower().Split('.')[0]) > 0)
											{
												oFile = d;
												break;
											}
									}
									LSTLibs.Items.Add(new ReferenceItem(ReferenceItem.ReferenceType.Lib, lib, oFile));
								}
							}

							foreach (XmlNode n in xDoc.SelectNodes("VisualStudioProject/References/ProjectReference"))
							{
								ReferenceItem ri = new ReferenceItem(ReferenceItem.ReferenceType.Reference, n.Attributes["Name"].Value.ToString(), n.Attributes["ReferencedProjectIdentifier"].Value.ToString());
								LSTReferences.Items.Add(ri);
								LSTReferences.SelectedItem = ri;
								RefToLib(ri);
							}

							break;
						}

						Salva();
					}
					catch
					{
					}
				}
			}
		}

		private void FindVcprojs(DirectoryInfo di)
		{
			bool found = false;
			foreach (FileInfo fi in di.GetFiles("*.vcproj"))
			{
				try
				{
					found = true;
					XmlDocument xVcproj = new XmlDocument();
					xVcproj.Load(fi.FullName);
				
					string name = string.Empty;
					XmlNode nVSP = xVcproj.SelectSingleNode("VisualStudioProject");
					if (nVSP != null)
					{
						XmlAttribute attr = nVSP.Attributes["Name"];
						if (attr != null)
						{
							name = attr.Value.ToString();
							if (!vcprojs.ContainsKey(name))
								vcprojs.Add(name, fi.FullName);
						}
					}
				}
				catch
				{}
			}
			if (found)
				return;

			foreach (DirectoryInfo cdi in di.GetDirectories())
			{
				FindVcprojs(cdi);
			}
		}

		private void CMBApplicazione_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			CMBModulo.Items.Clear();

			BaseApplicationInfo ai = (BaseApplicationInfo)CMBApplicazione.SelectedItem;

			if (ai == null)
				return;

			foreach (BaseModuleInfo mi in ai.Modules)
			{
				CMBModulo.Items.Add(mi);
			}

			if (CMBModulo.Items.Count > 0)
				CMBModulo.SelectedIndex = 0;
		}

		private void CMBLibrary_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			CMDConfiguration.Items.Clear();

			BaseApplicationInfo ai = (BaseApplicationInfo)CMBApplicazione.SelectedItem;

			if (ai == null)
				return;

			BaseModuleInfo mi = (BaseModuleInfo)CMBModulo.SelectedItem;

			if (mi == null)
				return;

			LibraryInfo li = (LibraryInfo)CMBLibrary.SelectedItem;

			if (li == null)
				return;

			string appPath = ai.Path;
			string modPath = mi.Path;
			string libPath = li.SourceFolder.ToLower().Replace("\\bin", string.Empty);
			string vcprojName = string.Format("{0}.vcproj", li.Name);
			string vcprojPath = Path.Combine(appPath, Path.Combine(modPath, Path.Combine(libPath, vcprojName)));
			try
			{
				if (File.Exists(vcprojPath))
					xDoc.Load(vcprojPath);
				else
				{
					//Devo chiedere il file da aprire
				}
				foreach (XmlNode nConfiguration in xDoc.SelectNodes("VisualStudioProject/Configurations/Configuration"))
				{
					CMDConfiguration.Items.Add(new ConfigurationNode(nConfiguration));
				}

				if (CMDConfiguration.Items.Count > 0)
					CMDConfiguration.SelectedIndex = 0;
			}
			catch
			{
			}
		}

		private void CMBModulo_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			CMBLibrary.Items.Clear();

			BaseModuleInfo mi = (BaseModuleInfo)CMBModulo.SelectedItem;

			if (mi == null)
				return;

			foreach (LibraryInfo li in mi.Libraries)
			{
				CMBLibrary.Items.Add(li);
			}

			if (CMBLibrary.Items.Count > 0)
				CMBLibrary.SelectedIndex = 0;
		}

		private void CMDConfiguration_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			LSTLibs.Items.Clear();

			ConfigurationNode cn = (ConfigurationNode)CMDConfiguration.SelectedItem;

			if (cn == null)
				return;

			string additionalDependencies = string.Empty;
			string additionalLibraryDirectories = string.Empty;

			foreach (XmlNode n in cn.nConf.SelectNodes("Tool"))
			{
				if (n.Attributes["Name"].Value.ToString() == "VCLinkerTool")
				{
					additionalDependencies = n.Attributes["AdditionalDependencies"].Value.ToString();
					additionalLibraryDirectories = n.Attributes["AdditionalLibraryDirectories"].Value.ToString();
					break;
				}
			}

			foreach (string s in additionalDependencies.Split(' '))
			{
				if (s != null && s != string.Empty)
				{
					string lib = s;
					string oFile = string.Empty;
					foreach (string d in additionalLibraryDirectories.Split(';'))
					{
						if (d != null && d != string.Empty)
							if (d.ToLower().IndexOf(s.ToLower().Split('.')[0]) > 0)
							{
								oFile = d;
								break;
							}
					}
					LSTLibs.Items.Add(new ReferenceItem(ReferenceItem.ReferenceType.Lib, lib, oFile));
				}
			}

			foreach (XmlNode n in xDoc.SelectNodes("VisualStudioProject/References/ProjectReference"))
			{
				LSTReferences.Items.Add(new ReferenceItem(ReferenceItem.ReferenceType.Reference, n.Attributes["Name"].Value.ToString(), n.Attributes["ReferencedProjectIdentifier"].Value.ToString()));
			}
		}

		private void CMDLibToRef_Click(object sender, System.EventArgs e)
		{
			ReferenceItem ri = (ReferenceItem) LSTLibs.SelectedItem;
			
			if (ri == null || ri.rType != ReferenceItem.ReferenceType.Lib)
				return;

			LibToRef(ri);
		}

		private void LibToRef(ReferenceItem ri)
		{
			if (ri.guid == string.Empty)
			{
				string nomeVcproj = ri.directory.Replace("$(OutDir)", ri.lib + ".vcproj");
				nomeVcproj = nomeVcproj.Replace(".lib", string.Empty);
				int sIdx, len = 0;
				sIdx = nomeVcproj.IndexOf("$(") + 2;
				len = nomeVcproj.IndexOf(")") - sIdx;
				string sysVar = nomeVcproj.Substring(sIdx, len);
			
				if (setVariables.ContainsKey(sysVar))
				{
					nomeVcproj = nomeVcproj.Replace(string.Format("$({0})", sysVar), setVariables[sysVar].ToString());
				}

				if (!File.Exists(nomeVcproj))
				{
					MessageBox.Show(string.Format("Non trovo il file {0}!\n E' necessario spostare la reference manualmente!", nomeVcproj));
					return;
				}

				XmlDocument xVcproj = new XmlDocument();
				xVcproj.Load(nomeVcproj);
				ri.lib = xVcproj.SelectSingleNode("VisualStudioProject").Attributes["Name"].Value.ToString();
				ri.guid = xVcproj.SelectSingleNode("VisualStudioProject").Attributes["ProjectGUID"].Value.ToString();
			}
			else
				ri.lib = ri.lib.Replace(".lib", string.Empty);
			
			ri.rType = ReferenceItem.ReferenceType.Reference;
			LSTLibs.Items.Remove(ri);
			LSTReferences.Items.Add(ri);
		}

		private void CMDRefToLib_Click(object sender, System.EventArgs e)
		{
			ReferenceItem ri = (ReferenceItem) LSTReferences.SelectedItem;
			
			if (ri == null || ri.rType != ReferenceItem.ReferenceType.Reference)
				return;

			RefToLib(ri);
		}

		private void RefToLib(ReferenceItem ri)
		{
			if (ri.directory == string.Empty)
			{
				if (!vcprojs.ContainsKey(ri.lib))
				{
					MessageBox.Show(string.Format("Non trovo la libreria {0}!\n E' necessario spostare la reference manualmente!", ri.lib));
					return;
				}

				XmlDocument xVcproj = new XmlDocument();
				xVcproj.Load(vcprojs[ri.lib].ToString());
				ri.directory = vcprojs[ri.lib].ToString().Substring(0, vcprojs[ri.lib].ToString().LastIndexOf(@"\") + 1);

				int sVarLen = -1;
				string sVarValue = string.Empty;
				foreach (string sVar in setVariables.Values)
				{
					if (ri.directory.ToLower().IndexOf(sVar.ToLower()) >= 0)
					{
						if (sVarLen < sVar.Length)
						{
							sVarLen = sVar.Length;
							sVarValue = sVar;
						}
					}
				}

				string sVarKey = string.Empty;

				foreach (string sKey in setVariables.Keys)
				{
					if (setVariables[sKey].ToString() == sVarValue)
					{
						sVarKey = sKey;
						break;
					}
				}

				ri.directory = "$(" + sVarKey + ")" + ri.directory.Substring(sVarLen) + "$(OutDir)";
			}

			ri.lib += ".lib";

			ri.rType = ReferenceItem.ReferenceType.Lib;
			LSTReferences.Items.Remove(ri);
			LSTLibs.Items.Add(ri);
		}

		private void CMDSalva_Click(object sender, System.EventArgs e)
		{
			Salva();
		}

		private void Salva()
		{
			CMDConfiguration.Items.Clear();

			BaseApplicationInfo ai = (BaseApplicationInfo)CMBApplicazione.SelectedItem;

			if (ai == null)
				return;

			BaseModuleInfo mi = (BaseModuleInfo)CMBModulo.SelectedItem;

			if (mi == null)
				return;

			LibraryInfo li = (LibraryInfo)CMBLibrary.SelectedItem;

			if (li == null)
				return;

			string appPath = ai.Path;
			string modPath = mi.Path;
			string libPath = li.SourceFolder.ToLower().Replace("\\bin", string.Empty);
			string vcprojName = string.Format("{0}.vcproj", li.Name);
			string vcprojPath = Path.Combine(appPath, Path.Combine(modPath, Path.Combine(libPath, vcprojName)));
			try
			{
				if (File.Exists(vcprojPath))
					xDoc.Load(vcprojPath);
				else
				{
					//Devo chiedere il file da aprire
				}
				string additionalDependencies = string.Empty;
				string additionalLibraryDirectories = string.Empty;

				foreach (ReferenceItem ri in LSTLibs.Items)
				{
					if (ri.rType != ReferenceItem.ReferenceType.Lib)
						continue;

					additionalDependencies += ri.lib + " ";
					additionalLibraryDirectories += ri.directory + ";";
				}
				
				foreach (XmlNode nConfiguration in xDoc.SelectNodes("VisualStudioProject/Configurations/Configuration"))
				{
					foreach (XmlNode nTool in nConfiguration.SelectNodes("Tool"))
					{
						if (nTool.Attributes["Name"].Value.ToString() == "VCLinkerTool")
						{
							nTool.Attributes["AdditionalDependencies"].Value = additionalDependencies;
							nTool.Attributes["AdditionalLibraryDirectories"].Value = additionalLibraryDirectories;
							break;
						}
					}
				}
				XmlNode nReferences = xDoc.SelectSingleNode("VisualStudioProject/References");
				nReferences.RemoveAll();
				foreach (ReferenceItem ri in LSTReferences.Items)
				{
					if (ri.rType != ReferenceItem.ReferenceType.Reference)
						continue;

					XmlNode nProjectReference = xDoc.CreateNode(XmlNodeType.Element, "ProjectReference", string.Empty);
					XmlAttribute aReferencedProjectIdentifier = xDoc.CreateAttribute(string.Empty, "ReferencedProjectIdentifier", string.Empty);
					aReferencedProjectIdentifier.Value = ri.guid;
					XmlAttribute aName = xDoc.CreateAttribute(string.Empty, "Name", string.Empty);
					aName.Value = ri.lib;
					nProjectReference.Attributes.Append(aReferencedProjectIdentifier);
					nProjectReference.Attributes.Append(aName);
					nReferences.AppendChild(nProjectReference);
				}
				File.SetAttributes(vcprojPath, FileAttributes.Normal);
				xDoc.Save(vcprojPath);
			}
			catch
			{
			}
		}
			
	}

	public class ConfigurationNode
	{
		public XmlNode nConf = null;

		public ConfigurationNode(XmlNode n)
		{
			nConf = n;
		}

		//------------------------------------------------------------------------------
		public override string ToString()
		{
			return nConf.Attributes["Name"].Value.ToString();
		}
	}

	public class ReferenceItem
	{
		public enum ReferenceType {Lib, Reference};
		public ReferenceType rType = ReferenceType.Lib;
		public string lib = string.Empty;
		public string directory = string.Empty;
		public string guid = string.Empty;

		public ReferenceItem(ReferenceType type, string Reference, string par)
		{
			rType = type;
			lib = Reference;
			if (type == ReferenceType.Lib)
				directory = par;
			else
				guid = par;
		}

		//------------------------------------------------------------------------------
		public override string ToString()
		{
			return lib;
		}
	}
}
