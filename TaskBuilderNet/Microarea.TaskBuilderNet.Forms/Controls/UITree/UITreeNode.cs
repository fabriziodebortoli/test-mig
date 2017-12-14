using System;
using System.ComponentModel;
using Telerik.WinControls.UI;

namespace Microarea.TaskBuilderNet.Forms.Controls
{
	//=============================================================================================
	public class UITreeNode : RadTreeNode
	{
		TemplateItemCollection<RadTreeNodeCollection, UITreeNode> internalNodes;

		//---------------------------------------------------------------------------
		public UITreeNode()
		{
			internalNodes = new TemplateItemCollection<RadTreeNodeCollection, UITreeNode>(base.Nodes);
		}

		//---------------------------------------------------------------------------
		public UITreeNode(string text)
			: this()
		{
			base.Text = text;
		}

		//---------------------------------------------------------------------------
		public UITreeNode(string nodeKey, string nodeText)
			: this(nodeText)
		{
			base.Name = nodeKey;
		}

		//---------------------------------------------------------------------------
		public UITreeNode(string nodeKey, string nodeText, int imageIndex)
			: this(nodeKey, nodeText)
		{
			base.ImageIndex = imageIndex;
		}

		//---------------------------------------------------------------------------
		[Browsable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public RadTreeNodeCollection UINodes
		{
			get { return base.Nodes; }
		}

		[Browsable(false)]
		[Obsolete("do not use Nodes, use UINodes instead")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		//-----------------------------------------------------------------------------------------
		public new RadTreeNodeCollection Nodes
		{
			get { return base.Nodes; }
		}
	}
}
