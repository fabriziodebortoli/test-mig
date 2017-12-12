using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.Applications;

namespace Microarea.TaskBuilderNet.UI.CodeEditor
{
	//================================================================================
	class CodeNodeComparer : IComparer
	{
		//--------------------------------------------------------------------------------
		public int Compare(object x, object y)
		{
			if (x is ContainerNode)
				return 0;

			CodeNode source = x as CodeNode;
			CodeNode target = y as CodeNode;
			if (source == null || target == null)
				return 0;

			return source.Text.CompareTo(target.Text);
		}
	}

	enum CodeNodeType { Container, Keyword, PhysicalField, LogicalField, InternalFunction, ExternalFunction }
	//================================================================================
	internal class CodeNode : TreeNode, IComparable
	{
		protected string description;
		//--------------------------------------------------------------------------------
		public virtual bool Visible { get { return true; } }

		//--------------------------------------------------------------------------------
		public CodeNode(CodeNode parent, string text, string description, int imageIndex)
			: base(text, imageIndex, imageIndex)
		{
			this.description = description;
		}

		//--------------------------------------------------------------------------------
		public override string ToString()
		{
			return Text;
		}
		//--------------------------------------------------------------------------------
		internal CodeNode GetChildByName(string text)
		{
			foreach (CodeNode cn in Nodes)
				if (string.Compare(cn.Text, text, StringComparison.InvariantCultureIgnoreCase) == 0)
					return cn;
			return null;
		}

		//--------------------------------------------------------------------------------
		public virtual string Script
		{
			get
			{
				return "";
			}
		}

		//--------------------------------------------------------------------------------
		public virtual int CaretBackwardOffset
		{
			get { return 0; }
		}

		//--------------------------------------------------------------------------------
		public string Description { get { return description; } }

		//--------------------------------------------------------------------------------
		public int CompareTo(object obj)
		{
			CodeNode codeNode = obj as CodeNode;
			return this.Text.CompareTo(codeNode.Text);
		}
	}

	//================================================================================
	class ContainerNode : CodeNode
	{
		//--------------------------------------------------------------------------------
		public ContainerNode(CodeNode parent, string text)
			: base(parent, text, "", (int)ImageIconIndex.Container)
		{

		}

		//--------------------------------------------------------------------------------
		public override bool Visible
		{
			get
			{
				return Nodes.Count == 0
					? false
					: base.Visible;
			}
		}

	}
	//================================================================================
	class FunctionCodeNode : CodeNode
	{
		//--------------------------------------------------------------------------------
		public FunctionCodeNode(CodeNode parent, string text, string description, int imageIndex)
			: base(parent, text, description, imageIndex)
		{
		}

		//--------------------------------------------------------------------------------
		public override string Script
		{
			get
			{
				return string.Format(
					"{0}.{1}.{2}.{3} (  )",
					Parent.Parent.Parent.Text,
					Parent.Parent.Text,
					Parent.Text,
					Text);
			}
		}

		//--------------------------------------------------------------------------------
		public override int CaretBackwardOffset
		{
			get { return 2; }
		}


	}

	//================================================================================
	class WoormFunctionCodeNode : CodeNode
	{
		//--------------------------------------------------------------------------------
		public WoormFunctionCodeNode(CodeNode parent, string text, string description, int imageIndex)
			: base(parent, text, description, imageIndex)
		{
		}

		//--------------------------------------------------------------------------------
		public override string Script
		{
			get
			{
				return string.Format("{0} (  )", Text);
			}
		}

		//--------------------------------------------------------------------------------
		public override int CaretBackwardOffset
		{
			get { return 2; }
		}


	}
	//================================================================================
	class VariableNode : CodeNode
	{
		//--------------------------------------------------------------------------------
		public VariableNode(CodeNode parent, string text, string description, int imageIndex)
			: base(parent, text, description, imageIndex)
		{
		}
		//--------------------------------------------------------------------------------
		public override string Script
		{
			get
			{
				return Text;
			}
		}
	}
	//================================================================================
	class KeywordNode : CodeNode
	{
		//--------------------------------------------------------------------------------
		public KeywordNode(CodeNode parent, string text, int imageIndex)
			: base(parent, text, "", imageIndex)
		{
		}
		//--------------------------------------------------------------------------------
		public override string Script
		{
			get
			{
				return Text;
			}
		}
	}
	//================================================================================
	class EnumTagNode : CodeNode
	{
		EnumTag tag;
		//--------------------------------------------------------------------------------
		public EnumTagNode(CodeNode parent, EnumTag tag, string text, string description, int imageIndex)
			: base(parent, text, description, imageIndex)
		{
			this.tag = tag;
		}

		//--------------------------------------------------------------------------------
		public override string Script
		{
			get
			{
				return Description;
			}
		}
	}

	//================================================================================
	class EnumItemNode : CodeNode
	{
		EnumItem item;
		//--------------------------------------------------------------------------------
		public EnumItemNode(CodeNode parent, EnumItem item, string text, string description, int imageIndex)
			: base(parent, text, description, imageIndex)
		{
			this.item = item;
		}

		//--------------------------------------------------------------------------------
		public override string Script
		{
			get
			{
				return item.FullValue;
			}
		}

	}
	//================================================================================
	class TableNode : CodeNode
	{
		//--------------------------------------------------------------------------------
		public TableNode(CodeNode parent, string text, string description, int imageIndex)
			: base(parent, text, description, imageIndex)
		{
		}
		//--------------------------------------------------------------------------------
		public override string Script
		{
			get
			{
				return Text;
			}
		}
	}

	//================================================================================
	class ColumnNode : CodeNode
	{
		//--------------------------------------------------------------------------------
		public ColumnNode(CodeNode parent, string text, string description, int imageIndex)
			: base(parent, text, description, imageIndex)
		{
		}
		//--------------------------------------------------------------------------------
		public override string Script
		{
			get
			{
				return string.Format("{0}.{1}", Parent.Text, Text);
			}
		}
	}
}
