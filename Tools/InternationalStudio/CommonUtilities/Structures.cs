using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace Microarea.Tools.TBLocalizer.CommonUtilities
{
	/// <summary>Tipologie di messaggi</summary>
	public enum TypeOfMessage { info, warning, error, state };
	
	public interface ILogger
	{
		void WriteLog(string message);
		void WriteLog(string message, TypeOfMessage typeOfMessage);
	}

	/// <summary>Immagini utilizzate nel tree per diversificare i nodi</summary>
	public enum Images
	{
		PROJECT		= 0,  MINILOGO		 = 1,
		FILE		= 2,  DIALOG		 = 3, STRINGTABLE	= 4,  MENU			= 5, 
		SELECTED	= 6,  SOURCE		 = 7, ENUMS			= 8,  XML			= 9,
		REPORT		=10,  RESXSTRING	 =11, RESXFORM		= 12, REFERENCES	= 13, 
		REFERENCE	=14,  REFERENCEEMPTY =15, REFERENCESOPEN= 16, FOLDEROPENED	= 17, 
		FOLDERCLOSED=18,  LANGUAGE		 =19, RECYCLE		= 20, OLDSTRINGS	= 21,
		DBSCRIPT	=22
	};
	
	//================================================================================
	public enum SourceControlStatus
	{
		NotBound,
		CheckedOut,
		NotCheckedOut
	}

	/// <summary>Elenca i tipi di nodo che possono essere nel treeView, viene inserito nel tag. </summary>
	public enum	NodeType
	{
		SOLUTION, PROJECT, LANGUAGE, RESOURCE, LASTCHILD, REFERENCEBLOCK, REFERENCE, NULL
	};

	//=========================================================================
	public class LocalizerTreeView : TreeView
	{
		private const int WM_PAINT = 0x000F;
		
		//---------------------------------------------------------------------
		protected override void WndProc(ref Message m)
		{
            try
            {
                base.WndProc(ref m);
                if (m.Msg == WM_PAINT)
                    PaintDetails();
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.ToString());
            }
		}
		
		//---------------------------------------------------------------------
		public void PaintDetails()
		{
            using (Graphics g = CreateGraphics())
            {
                foreach (LocalizerTreeNode n in Nodes)
                    PaintDetails(n, g, true);
            }
		}

		//--------------------------------------------------------------------------------
		private void PaintDetails(LocalizerTreeNode node, Graphics g, bool recursive)
		{
			if (node.IsVisible && !node.IsBaseLanguageNode)
			{
				string text = GetString(node);
				if (text.Length > 0)
				{
					Rectangle r = new Rectangle(node.Bounds.Right, node.Bounds.Top, Convert.ToInt32(g.VisibleClipBounds.Width) - 20, node.Bounds.Height);

					Brush b = System.Text.RegularExpressions.Regex.IsMatch(text, "100(\\D000)?\\%") ? Brushes.Blue : Brushes.Red;
					
					g.DrawString(text, this.Font, b, r);
				}
			}

			if (recursive)
				foreach (LocalizerTreeNode child in node.Nodes)
					PaintDetails(child, g, true);
		}

		//--------------------------------------------------------------------------------
		private string GetString(LocalizerTreeNode node)
		{
			switch (node.Type)
			{
				case NodeType.SOLUTION :
				case NodeType.REFERENCE:
				case NodeType.REFERENCEBLOCK:
				case NodeType.NULL:
					return "";
				default : return node.Tag.Details;
			}
		}
	}

	//=========================================================================
	public class LocalizerTreeNode : TreeNode
	{
		Hashtable wordTable = null;

		public static string BaseLanguage = "en";
		public static bool StopUpdating = false;
		public static bool StopSCCRefresh = false;

		//--------------------------------------------------------------------------------
		public bool IsBaseLanguageNode
		{
			get
			{
				LocalizerTreeNode langNode = GetTypedParentNode(NodeType.LANGUAGE);
				if (langNode == null)
					return false;
				return string.Compare(langNode.Name, LocalizerTreeNode.BaseLanguage, true) == 0;
			}
		}
		public LocalizerTreeNode[] ReferencedNodes { get { return (LocalizerTreeNode[])Tag.ReferencedNodes.ToArray(typeof(LocalizerTreeNode)); } }
		//---------------------------------------------------------------------
		public new LocalizerTreeView TreeView { get { return (LocalizerTreeView)base.TreeView; } }
		//---------------------------------------------------------------------
		public Hashtable WordTable { get { return wordTable; } set { wordTable = value; } }
		//---------------------------------------------------------------------
		public new string Name { get { return Tag.NodeName; } }
		//---------------------------------------------------------------------
		public NodeType Type { get { return Tag.GetNodeType(); } }
		//---------------------------------------------------------------------
		public new NodeTag Tag { get { return (NodeTag)base.Tag; } set { base.Tag = value; RefreshText(); } }
		//---------------------------------------------------------------------
		public string FileSystemPath { get { return Tag.ToString(); } }
		//---------------------------------------------------------------------
		public string SourcesPath { get { return Functions.GetSourcesPath(this); } }

		//---------------------------------------------------------------------
		public virtual SourceControlStatus SourceControlStatus
		{
			get
			{
				return SourceControlStatus.NotBound;
			}
		}

		//---------------------------------------------------------------------
		public new string FullPath
		{
			get
			{
				if (Parent == null)
					return this.Name;
				else
					return Path.Combine(((LocalizerTreeNode)Parent).FullPath, this.Name);
			}
		}

		//---------------------------------------------------------------------
		public LocalizerTreeNode(string nodePath, string nodeName, NodeType nodeType)
			: this(new NodeTag(nodePath, nodeName, nodeType))
		{
		}

		//---------------------------------------------------------------------
		public LocalizerTreeNode(NodeTag tag)
		{
			this.Tag = tag;
		}

		//---------------------------------------------------------------------
		public void RefreshText()
		{
			string text = Tag.GetDescription();
			if (Text != text)
				Text = text;
		}

		//---------------------------------------------------------------------
		public void AddReferencedNode(LocalizerTreeNode aNode)
		{
			if (!Tag.ReferencedNodes.Contains(aNode))
				Tag.ReferencedNodes.Add(aNode);
		}

		//--------------------------------------------------------------------------------
		public void CalculateChildNodes(bool recursive)
		{
			Functions.CalculateChildNodes(this, recursive);
		}

		//---------------------------------------------------------------------
		public string[] GetNodeFiles()
		{
			List<string> files = new List<string>();

			AddNodeFiles(files);
			return files.ToArray();
		}
		//---------------------------------------------------------------------
		private void AddNodeFiles(List<string> files)
		{
			string nodePath = FileSystemPath;
			if (!string.IsNullOrEmpty(nodePath))
			{
				switch (Type)
				{
					case NodeType.SOLUTION:
					case NodeType.PROJECT:
					case NodeType.LASTCHILD:
						if (!files.Contains(nodePath))
							files.Add(nodePath);
						break;
					case NodeType.LANGUAGE:
						{
							List<string> folderFiles = new List<string>();
							Functions.RecursiveAddFiles(nodePath, folderFiles);
							foreach (string folderFile in folderFiles)
								if (!files.Contains(folderFile))
									files.Add(folderFile);

						}
						break;
				}

			}
			foreach (LocalizerTreeNode child in Nodes)
				child.AddNodeFiles(files);
		}

		//---------------------------------------------------------------------
		public LocalizerTreeNode GetTypedParentNode(NodeType aType)
		{
			if (this.Type == aType)
				return this;

			LocalizerTreeNode parent = this.Parent as LocalizerTreeNode;
			if (parent == null)
				return null;
			return parent.GetTypedParentNode(aType);
		}

		//---------------------------------------------------------------------
		public ArrayList GetTypedChildNodes(NodeType aType, bool deep)
		{
			return this.GetTypedChildNodes(aType, deep, null, false);
		}

		//---------------------------------------------------------------------
		public ArrayList GetTypedChildNodes(NodeType aType, bool deep, string nameFilter, bool ignoreCase)
		{
			return GetTypedChildNodes(aType, deep, nameFilter, ignoreCase, null);
		}

		//---------------------------------------------------------------------
		public ArrayList GetTypedChildNodes(NodeType aType, bool deep, string nameFilter, bool ignoreCase, string cultureFilter)
		{
			ArrayList list = new ArrayList();

			GetTypedChildNodes(aType, deep, nameFilter, ignoreCase, cultureFilter, list);

			return list;
		}

		//---------------------------------------------------------------------
		protected virtual void GetTypedChildNodes(NodeType aType, bool deep, string nameFilter, bool ignoreCase, string cultureFilter, ArrayList list)
		{
			if (
				this.Type == aType &&
				(nameFilter == null || string.Compare(this.Name, nameFilter, true) == 0)
				)
			{
				list.Add(this);
				if (!deep) return;
			}

			foreach (LocalizerTreeNode n in this.Nodes)
				n.GetTypedChildNodes(aType, deep, nameFilter, ignoreCase, cultureFilter, list);
		}

		//---------------------------------------------------------------------
		public void RefreshNodeAndAncestors(bool showTranslationProgress)
		{
			CleanWordTable(false);
			UpdateDetails(showTranslationProgress, false);
			LocalizerTreeNode parent = Parent as LocalizerTreeNode;
			if (parent != null)
				parent.RefreshNodeAndAncestors(showTranslationProgress);
			TreeView.Invalidate();
		}

		//---------------------------------------------------------------------
		public void CleanWordTable(bool recursive)
		{
			WordTable = null;
			if (!recursive) return;
			foreach (LocalizerTreeNode n in Nodes)
				n.CleanWordTable(recursive);

		}

		//---------------------------------------------------------------------
		private delegate Rectangle GetBoundsFunction();
		public Rectangle GetBounds()
		{
			if (TreeView.InvokeRequired)
				return (Rectangle)TreeView.Invoke(new GetBoundsFunction(GetBounds));
			return Bounds;
		}

		private delegate void InvalidateNodeFunction();
		private void InvalidateNode()
		{
			if (TreeView == null)
				return;

			if (TreeView.InvokeRequired)
			{
				TreeView.BeginInvoke(new InvalidateNodeFunction(InvalidateNode));
				return;
			}
			Rectangle r = Bounds;
			r.Width = TreeView.Width - r.Left;
			TreeView.Invalidate(r);
		}
		//---------------------------------------------------------------------
		public void UpdateDetails(bool showTranslationProgress, bool recursive)
		{
			if (StopUpdating) return;
			try
			{
				switch (Type)
				{
					case NodeType.NULL:
					case NodeType.REFERENCE:
					case NodeType.REFERENCEBLOCK:
					case NodeType.SOLUTION:
						break;
					case NodeType.PROJECT:
						{
							ArrayList cultureNodes = GetTypedChildNodes(NodeType.LANGUAGE, false);
							if (cultureNodes.Count == 2)
							{
								foreach (LocalizerTreeNode node in cultureNodes)
									if (!node.IsBaseLanguageNode)
										Tag.Details = showTranslationProgress ? Functions.GetWordInfoString(node, false) : "";
							}
							else
							{
								Tag.Details = "";
							}
							InvalidateNode();
							break;
						}
					case NodeType.RESOURCE:
						{
							if (
									Functions.IsUsingFilters() &&
									Functions.AvailableFilters().Contains(Name)
								)
							{
								bool found = false;
								foreach (string filter in Functions.GetFilters())
									if (string.Compare(filter, Name, true) == 0)
									{
										found = true;
										break;
									}
								if (!found)
								{
									Tag.Details = "";
									return;
								}
							}

							Tag.Details = showTranslationProgress ? Functions.GetWordInfoString(this, false) : "";
							InvalidateNode();
							break;
						}
					default:
						{
							Tag.Details = showTranslationProgress ? Functions.GetWordInfoString(this, false) : "";
							InvalidateNode();
							break;
						}
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(string.Format("ERROR updating node {0}: {1}", FullPath, ex.Message));
			}
			if (recursive)
				foreach (LocalizerTreeNode child in Nodes)
					child.UpdateDetails(showTranslationProgress, recursive);
		}


		delegate void RefreshStateImageFunction(int index);
		//--------------------------------------------------------------------------------
		public int RefreshSourceControlStatus(bool recursive)
		{
			try
			{
				if (StopSCCRefresh) return -1;

				int index = -1;
				if (Type != NodeType.LASTCHILD)
				{
					if (recursive)
						foreach (LocalizerTreeNode child in Nodes)
						{
							index = Math.Max(index, child.RefreshSourceControlStatus(true));
						}
				}
				else
				{
					switch (SourceControlStatus)
					{
						case SourceControlStatus.CheckedOut:
							index = 1;
							break;
						case SourceControlStatus.NotCheckedOut:
							index = 0;
							break;
						default:
							index = -1;
							break;
					}
				}

				if (StateImageIndex != index)
				{
					if (TreeView.InvokeRequired)
					{
						RefreshStateImageFunction p = delegate { StateImageIndex = index; };
						TreeView.BeginInvoke(p, new object[] { index });
					}
					else
					{
						StateImageIndex = index;
					}
				}

				return index;
			}
			catch
			{
				return -1;
			}
		}

	}
	/// <summary>
	///Struct che contiene le informazioni sui nodi del tree ed è inserita come tag del nodo.
	/// </summary>
	//=========================================================================
	public class NodeTag 
	{
		public string		nodePath;
		private NodeType	nodeType;
		// o per la gestione delle stringhe not valid			
		public ArrayList	ReferencedNodes;	//utilizzato nella gestione delle stringhe non valide
		public string		NodeName;			
		public string		Details;


		/// <summary>
		/// Costruttore per il tag del TreeNode.
		/// </summary>
		/// <param name="nodePath">path completo del nodo su fileSystem</param>
		/// <param name="nodeType">tipo di nodo(enumerativo)</param>
		/// <param name="referencesPath">lista dei path dei dizionari dei progetti referenziati(c#)</param>
		//---------------------------------------------------------------------
		public NodeTag(string nodePath, string nodeName, NodeType nodeType) 
		{
			this.nodePath		= nodePath;
			this.NodeName		= nodeName;
			if (this.nodePath == null) this.nodePath = String.Empty;
			this.nodeType		= nodeType;
			this.Details		= "";
			this.ReferencedNodes= new ArrayList();
		}

		/// <summary>
		///  Override the ToString method.
		/// </summary>
		//---------------------------------------------------------------------
		public override string ToString()
		{
			return nodePath;   
		}

		//---------------------------------------------------------------------
		public override int GetHashCode()
		{
			return nodePath.GetHashCode() + NodeName.GetHashCode() + nodeType.GetHashCode();
		}

		//---------------------------------------------------------------------
		public string GetDescription()
		{
			if (nodeType == NodeType.SOLUTION)
				return string.Format("'{0}'{1}", NodeName, Details);	

			return NodeName;
		}

		//---------------------------------------------------------------------
		public bool IsEqual(NodeTag aTag)
		{
			return	this.NodeName == aTag.NodeName &&
				this.nodeType == aTag.nodeType;
		}

		/// <summary>
		/// Restituisce il tipo di nodo(enumerativo).
		/// </summary>
		//---------------------------------------------------------------------
		public  NodeType GetNodeType()
		{
			return nodeType;
		}
	}

	/// <summary>
	/// Modalità di comparazione per i nodi del tree
	/// </summary>
	//=========================================================================
	public class TreeNodeComparer : IComparer
	{
		//--------------------------------------------------------------------------------
		public int Compare(object x,object y)
		{
			LocalizerTreeNode nX = x as LocalizerTreeNode;
			LocalizerTreeNode nY = y as LocalizerTreeNode;

			if (nX == null || nY == null) 
				throw new NullReferenceException();

			return string.Compare(nX.Name, nY.Name);
		}
	}
	
	//=========================================================================
	public struct DictionaryPositionInfo
	{
		public string Directory;
		public string Company;

		//---------------------------------------------------------------------
		public DictionaryPositionInfo(string directory, string company)
		{
			this.Directory = directory;
			this.Company = company;
		}	
	}

	//=========================================================================
	public class LocalizerXmlNodeList : XmlNodeList
	{
		private ArrayList innerList = new ArrayList();
		//--------------------------------------------------------------------------------
		public override int Count
		{
			get
			{
				return innerList.Count;
			}
		}

		//--------------------------------------------------------------------------------
		public override IEnumerator GetEnumerator()
		{
			return innerList.GetEnumerator();
		}

		//--------------------------------------------------------------------------------
		public override XmlNode Item(int index)
		{
			return innerList[index] as XmlNode;
		}

		//--------------------------------------------------------------------------------
		public void Add(XmlNode n)
		{
			innerList.Add(n);
		}

		//--------------------------------------------------------------------------------
		public void AddRange(XmlNodeList list)
		{
			foreach (XmlNode n in list)
				innerList.Add(n);
		}
	}

	//=========================================================================
	public class CaseInsensitiveStringCollection : ArrayList
	{
		public override bool Contains(object item)
		{
			string sItem = item as string;
			if (sItem == null)
				return false;

			foreach (string s in this)
				if (string.Compare(s, sItem, true) == 0)
					return true;
			return false;
		}
	}


}
