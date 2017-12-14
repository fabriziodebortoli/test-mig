using System;
using System.Collections;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using Microarea.Tools.TBLocalizer.CommonUtilities;
using Microarea.Tools.TBLocalizer.Forms;

namespace Microarea.Tools.TBLocalizer
{
	/// <summary>
	/// Summary description for TextSearcher.
	/// </summary>
	//================================================================================
	public class TextSearcher
	{
		string		searchingTag;
		bool		onlyTranslated;
		string		toSearch;
		string		languageCode;
		string		supportCode;
		bool		matchWord;
		bool		matchCase;
		bool		useRegex;
		bool		applyFilter;
		string[]	filters;

		StatusBar	sBar;
		
		int count = 0;

		public LocalizerTreeNode	Node;
		public ArrayList FinderInfos;
		public Finder Finder;
		
		//--------------------------------------------------------------------------------
		public TextSearcher(
			Finder finder,
			LocalizerTreeNode node,
			string searchingTag,
			bool onlyTranslated,
			string toSearch,
			string languageCode,
			string supportCode,
			bool matchWord,
			bool matchCase,
			bool useRegex,
			bool applyFilter,
			string[] filters,
			StatusBar sBar)
		{
			this.Finder			= finder;
			this.Node			= node;
			this.searchingTag	= searchingTag;
			this.onlyTranslated = onlyTranslated;
			this.toSearch		= toSearch;
			this.languageCode	= languageCode;
			this.supportCode	= supportCode;
			this.matchWord		= matchWord;
			this.matchCase		= matchCase;
			this.useRegex		= useRegex;
			this.filters		= filters;
			this.applyFilter	= applyFilter;
			this.sBar			= sBar;
		}

		//---------------------------------------------------------------------
		public void SearchInTreeNode ()
		{
			FinderInfos = new ArrayList();
			try
			{
				SearchDictionaryNodes(Node, Finder.SearchingPlace.ValueMember, languageCode);				
			}
			catch (ThreadAbortException)
			{
				MessageBox.Show(sBar.FindForm(), Strings.ProcedureAborted);
			}
			catch(Exception ex)
			{
				MessageBox.Show(sBar.FindForm(), ex.Message);			
			}
		}

		//--------------------------------------------------------------------------------
		private void SearchInNode(DictionaryTreeNode treeNode)
		{
			XmlElement xmlNode = treeNode.GetResourceNode();
			if (xmlNode == null)
				return;

			ArrayList listOfTranslation = new ArrayList();
			
			try
			{
				sBar.Text = string.Format(Strings.SearchingMessage, treeNode.FullPath);

				treeNode.MergeWithSupport(supportCode);

				XmlNodeList list = treeNode.GetStringNodes(xmlNode);
				if (list == null)
					return;
			
				foreach (XmlNode n in list)
				{	
					if (!DictionaryCreator.MainContext.Working) return;

					XmlElement el = n as XmlElement;
					if (el == null) return;
					
					string targetString = el.GetAttribute(searchingTag);
					if (!CommonFunctions.StringContains(targetString, toSearch, matchCase, matchWord, useRegex))
						continue;
					
					string t = el.GetAttribute(AllStrings.target);
					if (onlyTranslated && (t == null || t == String.Empty))
						continue;
					
					string b = el.GetAttribute(AllStrings.baseTag);
					string s = el.GetAttribute(AllStrings.support);
					string name = el.GetAttribute(AllStrings.name);

					listOfTranslation.Add(new TranslationInfo(b, t, s, name));
					count++;
				}
			}
			finally
			{
				if (listOfTranslation.Count > 0)
					FinderInfos.Add(new FinderInfo(xmlNode, treeNode, listOfTranslation));	
			}
		}

		

		//---------------------------------------------------------------------
		private void SearchDictionaryNodes(LocalizerTreeNode node, string languageCode)
		{
			if (node == null) return;			
	
			if (!DictionaryCreator.MainContext.Working) return;
			
			sBar.Text = string.Format(Strings.SearchingNode, count, node.Name);

			NodeType type = node.Type;
			if (type == NodeType.LASTCHILD)
			{
				SearchInNode((DictionaryTreeNode) node);
				return;
			}
			else if ( type == NodeType.LANGUAGE )
			{
				if (string.Compare(node.Name, languageCode, true) != 0)
					return;
			}
            else if (type == NodeType.RESOURCE && applyFilter && ((LocalizerTreeNode)node.Parent).Type == NodeType.LANGUAGE)
			{
				
				string nodeName = node.Name;
				bool found = false;
				foreach (string filter in filters)
				{
					if (string.Compare(filter, nodeName, true) == 0)
					{
						found = true;
						break;
					}
				}

				if (!found) return;
			}

			TreeNodeCollection nodes = node.Nodes;
			foreach(LocalizerTreeNode n in nodes)
				SearchDictionaryNodes(n, languageCode);
		}

		//---------------------------------------------------------------------
		private void SearchDictionaryNodes(LocalizerTreeNode node, NodeType selectedType, string languageCode)
		{ 
			if (node == null) return;

			if (!DictionaryCreator.MainContext.Working) return;

			LocalizerTreeNode n = node.GetTypedParentNode(selectedType);
			if (n == null) n = node;
			SearchDictionaryNodes(n, languageCode);
		}
	}
}

