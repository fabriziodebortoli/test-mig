using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Microarea.TaskBuilderNet.Core.Applications;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.UI.WinControls;

namespace Microarea.Tools.TBLocalizer.CommonUtilities
{
	//================================================================================
	/// <summary>
	/// Classe custom per gestire un pathfinder ad uso e consumo di localizer con installazione variabile
	/// </summary>
	class LocalizerPathFinder : PathFinder
	{
		string filePath;

		//--------------------------------------------------------------------------------
		public LocalizerPathFinder ()
			: base("dummy", "dummy")
		{
		}

		//--------------------------------------------------------------------------------
		protected override bool Init ()
		{
			return true; //inibisco la init del costruttore per chiamare la mia custom in seguito
		}

		//--------------------------------------------------------------------------------
		public bool InitPathFinder (string installation, string filePath)
		{
			this.filePath = filePath;
			return base.Init(Dns.GetHostName(), installation);
		}

		//--------------------------------------------------------------------------------
		protected override bool CalculatePathsInsideInstallation ()
		{
			//prima provo a calcolare i percorsi relativi al file di solution
			if (CalculatePathsInsideInstallation(filePath))
				return true; 
			
			//poi uso quelli relativi al file eseguibile (logica standard)
			return base.CalculatePathsInsideInstallation();
		}
	}

	/// <summary>
	/// Summary description for CommonFunctions.
	/// </summary>
	//================================================================================
	public class Functions
	{
		private static SolutionCache currentSolutionCache = null;

		//--------------------------------------------------------------------------------
		public static SolutionCache CurrentSolutionCache { get { return currentSolutionCache; } set { currentSolutionCache = value; } }

		private static Hashtable questionWindows = new Hashtable();
		private static Hashtable sessions = new Hashtable();
		//--------------------------------------------------------------------------------
		public static void ResetSession (string installation)
		{
			sessions[installation] = null;
		}
		//--------------------------------------------------------------------------------
		public static TbReportSession GetSession(string installation, string filePath)
		{
			TbReportSession session = sessions[installation] as TbReportSession;
			if (session == null)
			{
				try
				{
					UserInfo ui = new UserInfo();
					// le informazioni legate a user e company non mi servono perché 
					// non devo gestire i dati custom (d'altronde non le ho)

					LocalizerPathFinder pf = new LocalizerPathFinder();
					pf.InitPathFinder(installation, filePath);					
					ui.PathFinder = pf;
					BasePathFinder.BasePathFinderInstance = pf;
					session = new TbReportSession(ui);
					sessions[installation] = session;
					session.LoadSessionInfo(null, false);
				}
				catch (Exception ex)
				{
					System.Diagnostics.Debug.Fail(ex.Message);
					throw;
				}
			}

			return session;
		}

		public delegate void CalculateChildNodesDelegate (LocalizerTreeNode aNode, bool recursive);
		public static CalculateChildNodesDelegate CalculateChildNodesFunction;
		//--------------------------------------------------------------------------------
		public static void CalculateChildNodes (LocalizerTreeNode aNode, bool recursive)
		{
			if (CalculateChildNodesFunction == null) return;
			CalculateChildNodesFunction(aNode, recursive);
		}

		public delegate string GetSourcesPathDelegate (LocalizerTreeNode aNode);
		public static GetSourcesPathDelegate GetSourcesPathFunction;
		//--------------------------------------------------------------------------------
		public static string GetSourcesPath (LocalizerTreeNode aNode)
		{
			if (GetSourcesPathFunction == null) return null;
			return GetSourcesPathFunction(aNode);
		}

		public delegate string GetWordInfoStringDelegate (LocalizerTreeNode nodeToCount, bool verbose);
		public static GetWordInfoStringDelegate GetWordInfoStringFunction;
		//--------------------------------------------------------------------------------
		public static string GetWordInfoString (LocalizerTreeNode nodeToCount, bool verbose)
		{
			if (GetWordInfoStringFunction == null) return "";
			return GetWordInfoStringFunction(nodeToCount, verbose);
		}

        public delegate string[] GetFiltersDelegate();
        public static GetFiltersDelegate GetFiltersFunction;
        //--------------------------------------------------------------------------------
        public static string[] GetFilters()
        {
            if (GetFiltersFunction == null) return new string[]{};
            return GetFiltersFunction();
        }

        public delegate StringCollection GetAvailableFiltersDelegate();
        public static GetAvailableFiltersDelegate AvailableFiltersFunction;
        //--------------------------------------------------------------------------------
        public static StringCollection AvailableFilters()
        {
            if (AvailableFiltersFunction == null) return new StringCollection();
            return AvailableFiltersFunction();
        }

        public delegate bool IsUsingFiltersDelegate();
        public static IsUsingFiltersDelegate IsUsingFiltersFunction;
        //--------------------------------------------------------------------------------
        public static bool IsUsingFilters()
        {
            if (IsUsingFiltersFunction == null) return false;
            return IsUsingFiltersFunction();
        }

        //---------------------------------------------------------------------
		public static string CalculateRelativePath (string path1, string path2, bool trimSlash)
		{
			string relPath;

			path1 = path1.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
			path2 = path2.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

			if (path1.ToLower().IndexOf(path2.ToLower()) == 0)
				relPath = path1.Substring(path2.Length);
			else if (path2.ToLower().IndexOf(path1.ToLower()) == 0)
				relPath = path2.Substring(path1.Length);
			else
				relPath = string.Empty;

			if (trimSlash)
				relPath = relPath.Trim(Path.DirectorySeparatorChar).Trim(Path.AltDirectorySeparatorChar);
			return relPath;
		}

		//---------------------------------------------------------------------
		public static bool SameTreeNodes (LocalizerTreeNode first, LocalizerTreeNode second)
		{
			if (first == null || first.Tag == null) return false;
			if (second == null || second.Tag == null) return false;

			return first.Tag.IsEqual(second.Tag);
		}

		//---------------------------------------------------------------------
		public static LocalizerTreeNode GetTypedParentNode (TreeNode aNode, NodeType aType)
		{
			if (aNode == null) return null;

			if (((LocalizerTreeNode)aNode).Type == aType)
				return (LocalizerTreeNode)aNode;

			return GetTypedParentNode(aNode.Parent, aType);
		}

		//---------------------------------------------------------------------
		public static ArrayList GetNodesByLevel (TreeNodeCollection nodes, int level)
		{
			ArrayList list = new ArrayList();

			if (level == 1)
			{
				list.AddRange(nodes);
				return list;
			}

			foreach (TreeNode n in nodes)
				list.AddRange(GetNodesByLevel(n.Nodes, level - 1));

			return list;
		}

		//---------------------------------------------------------------------
		public static bool IsReadOnlyFile (string path)
		{
			if (!File.Exists(path))
				return false;

			FileInfo fInfo = new FileInfo(path);
			return (fInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly;
		}

		//---------------------------------------------------------------------
		public static void SetReadOnlyFile (string path)
		{
			if (!File.Exists(path))
				return;

			FileInfo fInfo = new FileInfo(path);
			File.SetAttributes(path, fInfo.Attributes | FileAttributes.ReadOnly);
		}

		//---------------------------------------------------------------------
		public static void SafeDeleteFile (string path)
		{
			string errorMessage;
			SafeDeleteFile(path, out errorMessage);
		}
		//---------------------------------------------------------------------
		public static bool SafeDeleteFile (string path, out string errorMessage)
		{
			errorMessage = String.Empty;
			try
			{
				if (File.Exists(path))
				{
					FileInfo fInfo = new FileInfo(path);
					fInfo.Attributes = FileAttributes.Normal;

					File.Delete(path);
				}
				return true;
			}
			catch (Exception exc)
			{
				errorMessage = exc.Message;
				System.Diagnostics.Debug.WriteLine(String.Format("Errore in cancellazione del file {0}. Messaggio: {1}", path, exc.Message));
				return false;
			}
		}

		//---------------------------------------------------------------------
		public static bool SafeCopyFolder (string sourceFolderName, string destFolderName, bool overwriteExisting)
		{
			if (
				!Directory.Exists(sourceFolderName) ||
				(!overwriteExisting && Directory.Exists(destFolderName))
				)
				return false;

			if (overwriteExisting)
				SafeDeleteFolder(destFolderName);

			Directory.CreateDirectory(destFolderName);

			foreach (string folder in Directory.GetDirectories(sourceFolderName))
				if (!SafeCopyFolder(folder, Path.Combine(destFolderName, Path.GetFileName(folder)), overwriteExisting))
					return false;

			foreach (string file in Directory.GetFiles(sourceFolderName))
				if (!SafeCopyFile(file, Path.Combine(destFolderName, Path.GetFileName(file)), overwriteExisting))
					return false;

			return true;
		}

		//---------------------------------------------------------------------
		public static bool SafeCopyFile (string sourceFileName, string destFileName, bool overwriteExisting)
		{
			if (
				!File.Exists(sourceFileName) ||
				(!overwriteExisting && File.Exists(destFileName))
				)
				return false;

			if (string.Compare(sourceFileName, destFileName, StringComparison.InvariantCultureIgnoreCase) == 0)
				return true;

			if (overwriteExisting)
				SafeDeleteFile(destFileName);

			string path = Path.GetDirectoryName(destFileName);
			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);

			File.Copy(sourceFileName, destFileName);
			File.SetAttributes(destFileName, FileAttributes.Normal);
			return true;
		}

		//---------------------------------------------------------------------
		public static void SafeDeleteFolder (string path)
		{
			SafeDeleteFolder(path, false);
		}

		//---------------------------------------------------------------------
		public static void SafeDeleteFolder (string path, bool onlyContents)
		{
			if (Directory.Exists(path))
			{
				foreach (string dir in Directory.GetDirectories(path, "*.*"))
					SafeDeleteFolder(dir);

				foreach (string file in Directory.GetFiles(path, "*.*"))
					SafeDeleteFile(file);

				if (!onlyContents)
					Directory.Delete(path, true);
			}
		}

		//---------------------------------------------------------------------
		public static void RemoveUselessFolders (string folder, string[] uselessFiles)
		{
			if (Directory.GetDirectories(folder).Length != 0)
				return;

			if (uselessFiles == null)
				uselessFiles = new string[0];

			string[] files = Directory.GetFiles(folder);
			if (files.Length > uselessFiles.Length)
				return;

			int matchingFiles = 0;
			if (files.Length != 0)
			{
				foreach (string file in uselessFiles)
				{
					string fullPath;
					if (Path.GetDirectoryName(file) == string.Empty)
						fullPath = Path.Combine(folder, file);
					else
						fullPath = file;

					foreach (string existingFile in files)
						if (string.Compare(existingFile, fullPath, true) == 0)
						{
							matchingFiles++;
							break;
						}
				}
			}

			if (matchingFiles != files.Length) return;

			SafeDeleteFolder(folder);
			RemoveUselessFolders(Path.GetDirectoryName(folder), uselessFiles);
		}

		//--------------------------------------------------------------------------------
		public static DialogResult RepeatableMessage (IWin32Window owner, string message, params object[] args)
		{
			return RepeatableMessage(owner, false, message, args);
		}

		//--------------------------------------------------------------------------------
		public static DialogResult RepeatableMessage (IWin32Window owner, bool isQuestion, string message, params object[] args)
		{
			AskWindow window = questionWindows[message] as AskWindow;
			if (window == null)
			{
				window = new AskWindow();
				window.IsQuestion = isQuestion;
				window.Caption = CommonStrings.WarningCaption;
				questionWindows[message] = window;
			}

			window.Message = string.Format(message, args);
			return window.ShowDialog(owner);
		}

		//--------------------------------------------------------------------------------
		public static string GetCultureDescription (string culture)
		{
			try
			{
				System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo(culture);
				return ci.DisplayName;
			}
			catch
			{
				return string.Empty;
			}
		}

		//---------------------------------------------------------------------
		public static void OrderNodes (TreeNodeCollection nodes)
		{
			ArrayList list = new ArrayList();
			foreach (TreeNode n in nodes)
				list.Add(n);

			nodes.Clear();

			list.Sort(new TreeNodeComparer());

			for (int i = 0; i < list.Count; i++)
			{
				nodes.Add(list[i] as TreeNode);
			}
		}

		//---------------------------------------------------------------------
		public static void RemoveNodeAndEmptyAncestors (XmlElement el)
		{
			XmlElement candidateNode = el, parentNode = el;
			while (parentNode.ParentNode as XmlElement != null)
			{
				candidateNode = parentNode;
				parentNode = parentNode.ParentNode as XmlElement;

				if (parentNode.ChildNodes.Count > 1)
					break;
			}

			if (parentNode != null)
				parentNode.RemoveChild(candidateNode);
		}

		//---------------------------------------------------------------------
		public static string ExtractMessages (Exception ex)
		{
			StringBuilder sb = new StringBuilder();
			while (ex != null)
			{
				if (sb.Length > 0)
					sb.Append("\r\n");

				sb.Append(ex.Message);
				ex = ex.InnerException;
			}
			return sb.ToString();
		}

		//-----------------------------------------------------------------------------
		public static NodeType GetNodeTypeFromPath (string path)
		{
			if (path.EndsWith(".tblsln", StringComparison.InvariantCultureIgnoreCase))
				return NodeType.SOLUTION;

			if (path.EndsWith(".tblprj", StringComparison.InvariantCultureIgnoreCase))
				return NodeType.PROJECT;

			return NodeType.LANGUAGE;
		}

		//-----------------------------------------------------------------------------
		public static void RecursiveAddFiles (string folder, List<string> folderFiles)
		{
			folderFiles.AddRange(Directory.GetFiles(folder));
			foreach (string innerFolder in Directory.GetDirectories(folder))
				RecursiveAddFiles(innerFolder, folderFiles);
		}
	}

}
