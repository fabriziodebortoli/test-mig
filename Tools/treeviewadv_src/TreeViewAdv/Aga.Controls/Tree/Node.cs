using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using System.Drawing;

namespace Aga.Controls.Tree
{
	public class Node
	{
        //4112
        private String _key;
        private String _nodeImageKey;
		private Image _icon;
        private String _toolTipText;
        private bool _checkBox;

		#region NodeCollection

		private class NodeCollection : Collection<Node>
		{
			private Node _owner;

			public NodeCollection(Node owner)
			{
				_owner = owner;
			}

			protected override void ClearItems()
			{
				while (this.Count != 0)
					this.RemoveAt(this.Count - 1);
			}

			protected override void InsertItem(int index, Node item)
			{
				if (item == null)
					throw new ArgumentNullException("item");

				if (item.Parent != _owner)
				{
					if (item.Parent != null)
						item.Parent.Nodes.Remove(item);
					item._parent = _owner;
					base.InsertItem(index, item);

					TreeModel model = _owner.FindModel();
					if (model != null)
						model.OnNodeInserted(_owner, index, item);
				}
			}

			protected override void RemoveItem(int index)
			{
				Node item = this[index];
				item._parent = null;
				base.RemoveItem(index);

				TreeModel model = _owner.FindModel();
				if (model != null)
					model.OnNodeRemoved(_owner, index, item);
			}

			protected override void SetItem(int index, Node item)
			{
				if (item == null)
					throw new ArgumentNullException("item");

				RemoveAt(index);
				InsertItem(index, item);
			}
		}

		#endregion

		#region Properties

        public String Key
        {
            get
            {
                return _key;
            }
        }

        public String NodeImageKey
        {
            get
            {
                return _nodeImageKey;
            }

            set
            {
                _nodeImageKey = value;
            }
        }

		//MagoWeb
		public Image Icon
		{
			set
			{
				_icon = value;
			}
			get
			{
				return _icon;
			}
		}

		//MagoWeb
		public Bitmap Bitmap
		{
			get
			{
				return Icon as Bitmap;
			}
		}

        //4112 - manage ToolTip
        public String ToolTipText
        {
            get { return _toolTipText; }
            set { _toolTipText = value; }
        }

        //M. 5831
        public bool CheckBox
        {
            get { return _checkBox; }
            set { _checkBox = value; }
        }

		private TreeModel _model;
		internal TreeModel Model
		{
			get { return _model; }
			set { _model = value; }
		}

		private NodeCollection _nodes;
		public Collection<Node> Nodes
		{
			get { return _nodes; }
		}

		private Node _parent;
		public Node Parent
		{
			get { return _parent; }
			set 
			{
				if (value != _parent)
				{
					if (_parent != null)
						_parent.Nodes.Remove(this);

					if (value != null)
						value.Nodes.Add(this);
				}
			}
		}

		public int Index
		{
			get
			{
				if (_parent != null)
					return _parent.Nodes.IndexOf(this);
				else
					return -1;
			}
		}

		public Node PreviousNode
		{
			get
			{
				int index = Index;
				if (index > 0)
					return _parent.Nodes[index - 1];
				else
					return null;
			}
		}

		public Node NextNode
		{
			get
			{
				int index = Index;
				if (index >= 0 && index < _parent.Nodes.Count - 1)
					return _parent.Nodes[index + 1];
				else
					return null;
			}
		}

		private string _text;
		public virtual string Text
		{
			get { return _text; }
			set 
			{
				if (_text != value)
				{
					_text = value;
					NotifyModel();
				}
			}
		}

		private CheckState _checkState;
		public virtual CheckState CheckState
		{
			get { return _checkState; }
			set 
			{
				if (_checkState != value)
				{
					_checkState = value;
                    //Prog. 5831
                    NotifyModel("CheckState");
				}
			}
		}

		public bool IsChecked
		{
			get 
			{ 
				return CheckState != CheckState.Unchecked;
			}
			set 
			{
				if (value)
					CheckState = CheckState.Checked;
				else
					CheckState = CheckState.Unchecked;
			}
		}

		public virtual bool IsLeaf
		{
			get
			{
				return false;
			}
		}

		#endregion

        //4112
		public Node()
			: this(string.Empty, string.Empty, string.Empty, string.Empty)
		{
		}

        //4112
        public Node(string text, string key, string keyImage)
        {
            _text = text;
            _key = key;
            _nodeImageKey = keyImage;
            _nodes = new NodeCollection(this);
        }

        //4112
        public Node(string text, string key, string keyImage, string textToolTip)
            : this(text, key, keyImage)
        {
            _toolTipText = textToolTip;
        }

        //M. 5831 
        public Node(string text, string key, string keyImage, string textToolTip, bool checkBox)
            : this(text, key, keyImage)
        {
            _toolTipText    = textToolTip;
            _checkBox       = checkBox;
        }

		private TreeModel FindModel()
		{
            Node node = this;
			while (node != null)
			{
				if (node.Model != null)
					return node.Model;
                node = node.Parent;
			}
			return null;
		}

        //Prog. 5831
		protected void NotifyModel()
		{
            NotifyModel(string.Empty);
		}

        //Prog. 5831
        protected void NotifyModel(string nodeChangedProperty)
        {
            TreeModel model = FindModel();
            if (model != null && Parent != null)
            {
                TreePath path = model.GetPath(Parent);
                if (path != null)
                {
                    TreeModelEventArgs args = CreateTreeModelEventArgs(path, Index, nodeChangedProperty);
                    model.OnNodesChanged(args);
                }
            }
        }

        //Prog. 5831
        protected TreeModelEventArgs CreateTreeModelEventArgs(TreePath path, int nodeIdx, string nodeChangedProperty) 
        {
            int[] indices = new int[] { nodeIdx };
            object[] children = new object[] { this };

            return new TreeModelEventArgs(path, indices, children, nodeChangedProperty);
        }
	}
}
