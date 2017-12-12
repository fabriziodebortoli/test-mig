using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microarea.EasyBuilder.Properties;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Core.GenericForms;
using System.Text.RegularExpressions;

namespace Microarea.EasyBuilder.UI
{
	//=============================================================================
	/// <remarks/>
	internal partial class HotLinkWrapperEditor : ThemedForm
	{
		//-----------------------------------------------------------------------------
		private Func<string, bool> isNameValid;
		private string		hklNameSpace;
		private string		hklName = string.Empty;

		public NameSpace HotLinkNamespace { get { return new NameSpace(hklNameSpace); } }
		public string	HotLinkName { get { return hklName; } }

		//-----------------------------------------------------------------------------
		/// <remarks/>
		public HotLinkWrapperEditor(Func<string, bool> isNameValid, string nameSpace)
		{
			InitializeComponent();

			this.isNameValid = isNameValid;
			this.hklNameSpace = nameSpace;

			images.Images.Add(Resources.Repository);
			images.Images.Add(Resources.Application32x32);
			images.Images.Add(Resources.Module32x32);
			images.Images.Add(Resources.MiniHotLink);
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			UseWaitCursor = true;
			Update();
			List<string> appTitles = new List<string>();
			List<string> modTitles = new List<string>();
			List<string> titles = new List<string>();
			List<string> moduleNamespaces = new List<string>();
			CUtility.GetHotLinks(appTitles, modTitles, titles, moduleNamespaces);
			if (titles.Count == 0)
				return;
			TreeNode selection = null;
			TreeNode root = new TreeNode(Resources.HotLinksTemplates, 0, 0);
			treeViewHotLinks.Nodes.Add(root);

			for (int i = 0; i < titles.Count; i++)
			{
				string app = appTitles[i];
				string mod = modTitles[i];
				string title = titles[i];
				string nameSpace = moduleNamespaces[i] ;
				TreeNode appNode = GetNode(root.Nodes, app, 1);
				TreeNode modNode = GetNode(appNode.Nodes, mod, 2);
				TreeNode hklNode = GetNode(modNode.Nodes, title, 3);
				hklNode.Tag = nameSpace;
				if (nameSpace == hklNameSpace)
					selection = hklNode;
			}
			root.Expand();

			treeViewHotLinks.Scrollable = true;
			labelHotLinks.Text = Resources.RegisteredHotLinks;
			labelHotLinks.ForeColor = labelName.ForeColor;
			UseWaitCursor = false;
			if (selection != null)
			{
				treeViewHotLinks.SelectedNode = selection;
				treeViewHotLinks.Update();
			}
		}

		//-----------------------------------------------------------------------------
		private TreeNode GetNode(TreeNodeCollection treeNodeCollection, string text, int imageindex)
		{
			foreach (TreeNode node in treeNodeCollection)
				if (node.Text == text)
					return node;
			TreeNode n = new TreeNode(text, imageindex, imageindex);
			treeNodeCollection.Add(n);
			return n;
		}

		//-----------------------------------------------------------------------------
		private void BtnOk_Click(object sender, EventArgs e)
		{
			TreeNode obj = treeViewHotLinks.SelectedNode;
			if (obj == null || obj.Tag == null)
				return;
			hklNameSpace = (string)obj.Tag;
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			Regex rgx = new Regex(@"[^a-zA-Z0-9]+");//tutto ciò che non è lettera o numero non è valido
			Match match = rgx.Match(textBoxName.Text);
			if (match.Success)
			{
				sb.Append(string.Format(Resources.InvalidObjectName, match.Value));
				sb.Append(Environment.NewLine);
			}
			if (!string.IsNullOrEmpty(sb.ToString()))
			{
				MessageBox.Show(sb.ToString());
				return;
			}
			hklName = textBoxName.Text;

			DialogResult = DialogResult.OK;
			this.Close();
		}

		//-----------------------------------------------------------------------------
		private void BtnCancel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			this.Close();
		}

		//-----------------------------------------------------------------------------
		private string GetUniqueName(string seedName)
		{
			string hklName = seedName;
			int nr = 0;
			while (!isNameValid(hklName))
			{
				hklName = string.Format("{0}{1}", hklName, ++nr);
			}
			return hklName;
		}

		//-----------------------------------------------------------------------------
		private void treeViewHotLinks_AfterSelect(object sender, TreeViewEventArgs e)
		{
			TreeNode obj = treeViewHotLinks.SelectedNode;
			if (obj == null || obj.Tag == null)
			{
				btnOk.Enabled = false;
				return;
			}
			NameSpace ns = new NameSpace((string)obj.Tag);

			textBoxName.Text = GetUniqueName(ns.Leaf);
			btnOk.Enabled = true;
		}
	}
}
