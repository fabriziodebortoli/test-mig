using System;
using System.IO;
using System.Xml;
using System.Windows.Forms;

using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.Library.TranslationManager
{
	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	public abstract class SolutionManagerItems : BaseTranslator
	{
		private XmlDocument xDocMigration = null;

		protected XmlDocument xLookUpDoc = null;
		protected XmlNode nMain = null;

		protected RichTextBox TxtReader = new RichTextBox();
 
		public SolutionManagerItems(){}

		protected bool OpenLookUpDocument(bool create)
		{
			xLookUpDoc = transManager.GetLookUpFile(defaultLookUpType);
			if (xLookUpDoc == null)
			{
				if (create)
				{
					xLookUpDoc = new XmlDocument();
					transManager.SetLookUpFile(defaultLookUpType, xLookUpDoc);
				}
				return false;
			}
			return true;
		}

		protected XmlNode CreaNodoApplication(string applicationName, bool creaTarget)
		{
			return transManager.CreaNodoApplication(defaultLookUpType, applicationName, creaTarget);
		}

		protected XmlNode CreaNodoModule(string moduleName, bool addToMain, bool creaTarget)
		{
			return transManager.CreaNodoModule(defaultLookUpType, nMain, moduleName, addToMain, creaTarget);
		}

		protected XmlNode CreaNodoLibrary(XmlNode nModule, string libName, bool addToModule, LibraryInfo li)
		{
			return transManager.CreaNodoLibrary(defaultLookUpType, nModule, libName, addToModule, li);
		}

		protected XmlNode CreaNodoLookUp(XmlNode nParent, string sourceValue, string targetValue, string modName)
		{
			return transManager.CreaNodoLookUp(defaultLookUpType, nParent, sourceValue, targetValue, modName);
		}
		protected bool ExistNode(string nodeName, XmlNode nParent, string sourceValue, ref XmlNode nReturn)
		{
			return transManager.ExistNode(nodeName, nParent, sourceValue, ref nReturn);
		}
		protected string GetConversion(string sourceValue, XmlNode nParent)
		{
			return transManager.GetConversion(defaultLookUpType, sourceValue, nParent);
		}

		protected string GetConversion(string sourceValue)
		{
			return transManager.GetConversion(defaultLookUpType, sourceValue);
		}

		protected string GetConversion(string sourceValue, string moduleName)
		{
			return transManager.GetConversion(defaultLookUpType, sourceValue, moduleName);
		}

		protected string GetConversion(string sourceValue, string libraryName, string moduleName)
		{
			return transManager.GetConversion(defaultLookUpType, sourceValue, libraryName, moduleName);
		}

		protected bool IsLibrary(XmlNode nModule, string foldername)
		{
			foreach (XmlNode nLibrary in nModule.SelectNodes("Library"))
			{
				if (nLibrary.Attributes["destinationfolder"].Value.ToString().ToLower() == foldername.ToLower())
					return true;
			}
			foreach (XmlNode nLibrary in nModule.SelectNodes("Library"))
			{
				if (nLibrary.Attributes["sourcefolder"].Value.ToString().ToLower() == foldername.ToLower())
					return true;
			}
			return false;
		}

		protected string[] GetFullNamespace(string ns, string appName, string modName, string libName)
		{
			if (ns.Trim() == string.Empty)
				return null;

			string[] res = new string[] {appName, modName, libName, string.Empty};

			string[] tokens = ns.Split('.');

			if (tokens.Length == 1)
				res[3] = ns;
			else
			{
				int idy = 3;
				for (int idx = tokens.Length - 1; idx >= 0 && idy >= 0; idx --)
				{
					res[idy--] = tokens[idx];
				}
			}

			return res;
		}

		protected virtual bool FindTransation(string[] ns)
		{
			XmlNode nApplication = xLookUpDoc.SelectSingleNode("Application");
			if (nApplication.Attributes["source"].Value.ToString().ToLower() != ns[0].ToLower())
			{
				return false;
			}

			foreach (XmlNode nMod in nApplication.SelectNodes("Module"))
			{
				if (nMod.Attributes["source"].Value.ToString().ToLower() == ns[1].ToLower())
				{
					foreach (XmlNode nLib in nMod.SelectNodes("Library"))
					{
						foreach (XmlNode n in nLib.ChildNodes)
						{
							if (n.Attributes["source"].Value.ToString().ToLower() == ns[3].ToLower())
							{
								return true;
							}
						}
					}
				}
			}
			return false;
		}

		#region Funzioni private per Migration_NET
		protected void CreaNodoMig_Net(string ns)
		{
			CreaNodoMig_Net(ns, string.Empty);
		}

		protected void CreaNodoMig_Net(string ns, string subType)
		{
			string mainNode = string.Empty;
			string elNode = string.Empty;

			switch (defaultLookUpType)
			{
				case LookUpFileType.Documents: //Documents
				{
					ns = ns.Replace("Document.", string.Empty);
					mainNode = "Documents";
					elNode = "Document";
					break;
				}
				case LookUpFileType.WebMethods: //Functions
				{
					mainNode = "Functions";
					elNode = "Function";
					break;
				}
				case LookUpFileType.ReferenceObjects: //HotLinks
				{
					mainNode = "HotLinks";
					elNode = "HotLink";
					break;
				}
				case LookUpFileType.Structure: //Application
				{
					mainNode = "Libraries";
					elNode = "Library";
					break;
				}
				case LookUpFileType.Report: //Reports
				{
					mainNode = "Reports";
					elNode = "Report";
					break;
				}
				case LookUpFileType.Activation: //Activation
				{
					mainNode = "Activations";
					elNode = "Activation";
					break;
				}
				case LookUpFileType.Misc: //Utilizzato per ora solo per i Formattatori
				{
					switch (subType)
					{
						case "_NS_FMT":
							mainNode = "Formatters";
							elNode = "Formatter";
							break;
						default:
							return;
					}
					break;
				}
				default:
					return;
			}
			if (xDocMigration == null)
			{
				if (!OpenDocMigration(subType))
					return;
				
				if (defaultLookUpType == LookUpFileType.Activation)
				{
					XmlAttribute aOldApp = xDocMigration.CreateAttribute(string.Empty, "oldAppName", string.Empty);
					XmlAttribute aNewApp = xDocMigration.CreateAttribute(string.Empty, "newAppName", string.Empty);
					aOldApp.Value = aNewApp.Value = transManager.GetApplicationInfo().Name;
					xDocMigration.SelectSingleNode(mainNode).Attributes.Append(aOldApp);
					xDocMigration.SelectSingleNode(mainNode).Attributes.Append(aNewApp);
				}
			}

			bool bFound = false;

			foreach (XmlNode n in xDocMigration.SelectNodes(string.Format("{0}/{1}",mainNode, elNode)))
			{
				string oldName = n.Attributes["oldName"].Value.ToString();
				if (string.Compare(oldName, ns, true) == 0)
				{
					bFound = true;
					break;
				}
			}

			if (!bFound)
			{
				XmlNode n = xDocMigration.CreateNode(XmlNodeType.Element, elNode, string.Empty);
				XmlAttribute aOld = xDocMigration.CreateAttribute(string.Empty, "oldName", string.Empty);
				XmlAttribute aNew = xDocMigration.CreateAttribute(string.Empty, "newName", string.Empty);
				aOld.Value = aNew.Value = ns;
				n.Attributes.Append(aOld);
				n.Attributes.Append(aNew);
				xDocMigration.SelectSingleNode(mainNode).AppendChild(n);
			}
		}

		private bool OpenDocMigration()
		{
			return OpenDocMigration(string.Empty);
		}

		private bool OpenDocMigration(string subType)
		{
			string mainNode = string.Empty;
			string fName = string.Empty;

			switch (defaultLookUpType)
			{
				case LookUpFileType.Documents: //Documents
				{
					mainNode = "Documents";
					fName = "Documents.xml";
					break;
				}
				case LookUpFileType.WebMethods: //Functions
				{
					mainNode = "Functions";
					fName = "Functions.xml";
					break;
				}
				case LookUpFileType.ReferenceObjects: //HotLinks
				{
					mainNode = "HotLinks";
					fName = "HotLinks.xml";
					break;
				}
				case LookUpFileType.Structure: //Application
				{
					mainNode = "Libraries";
					fName = "Application.xml";
					break;
				}
				case LookUpFileType.Report: //Reports
				{
					mainNode = "Reports";
					fName = "Reports.xml";
					break;
				}
				case LookUpFileType.Activation: //Activations
				{
					mainNode = "Activations";
					fName = "Activations.xml";
					break;
				}
				case LookUpFileType.Misc: //Utilizzato per ora solo per i Formattatori
				{
					switch (subType)
					{
						case "_NS_FMT":
							mainNode = "Formatters";
							fName = "Formatters.xml";
							break;
						default:
							return false;
					}
					break;
				}
				default:
					return false;
			}

			IBaseModuleInfo mCore = transManager.GetApplicationInfo().GetModuleInfoByName("Core");
			if (mCore == null)
				return false;

			string migPath = mCore.GetMigrationNetPath();
			if (!Directory.Exists(migPath))
				Directory.CreateDirectory(migPath);

			string fileName = Path.Combine(migPath, fName);
			if (!File.Exists(fileName))
			{
				xDocMigration = new XmlDocument();
				xDocMigration.LoadXml(string.Format("<{0} />", mainNode));
			}
			else
			{
				xDocMigration = new XmlDocument();
				xDocMigration.Load(fileName);
			}

			return true;
		}

		protected bool SaveDocMigration()
		{
			return SaveDocMigration(string.Empty);
		}

		protected bool SaveDocMigration(string subType)
		{
			string fName = string.Empty;

			switch (defaultLookUpType)
			{
				case LookUpFileType.Documents: //Documents
				{
					fName = "Documents.xml";
					break;
				}
				case LookUpFileType.WebMethods: //Functions
				{
					fName = "Functions.xml";
					break;
				}
				case LookUpFileType.ReferenceObjects: //HotLinks
				{
					fName = "HotLinks.xml";
					break;
				}
				case LookUpFileType.Structure: //Application
				{
					fName = "Libraries.xml";
					break;
				}
				case LookUpFileType.Report: //Reports
				{
					fName = "Reports.xml";
					break;
				}
				case LookUpFileType.Activation: //Activations
				{
					fName = "Activations.xml";
					break;
				}
				case LookUpFileType.Misc: //Utilizzato per ora solo per i Formattatori
				{
					switch (subType)
					{
						case "_NS_FMT":
							fName = "Formatters.xml";
							break;
						default:
							return false;
					}
					break;
				}
				default:
					return false;
			}

			if (xDocMigration == null)
				return false;

			IBaseModuleInfo mCore = transManager.GetApplicationInfo().GetModuleInfoByName("Core");
			if (mCore == null)
				return false;

			string migPath = mCore.GetMigrationNetPath();
			string fileName = Path.Combine(migPath, fName);
			
			try
			{
				xDocMigration.Save(fileName);
			}
			catch
			{
				return false;
			}

			return true;
		}
		#endregion
	}
}
