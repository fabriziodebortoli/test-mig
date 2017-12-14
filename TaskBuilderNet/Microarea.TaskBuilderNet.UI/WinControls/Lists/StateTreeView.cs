using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.UI.WinControls.Lists
{
	//============================================================================
	public partial class DrawStateIconTreeView : System.Windows.Forms.TreeView, IStateTreeNodesOwner
	{
		private StateTreeNodeCollection nodes = null;
        private bool showStateImages = false;
        private bool showImages = false;

		public DrawStateIconTreeView()
		{
			InitializeComponent();

			nodes = new StateTreeNodeCollection(this);

			nodesImageList.ColorDepth		= ColorDepth.Depth32Bit;
			nodesImageList.ImageSize		= new Size(16, 16);
			nodesImageList.TransparentColor	= Color.Magenta;

			stateImageList.ColorDepth		= ColorDepth.Depth32Bit;
			stateImageList.ImageSize		= new Size(16, 16);
			stateImageList.TransparentColor	= Color.Magenta;

			AddImageFromCurrentAssemblyToNodesImageList("DummyImage.bmp");			// 0
			AddImageFromCurrentAssemblyToNodesImageList("DefaultCollapsedNode.bmp");// 1
			AddImageFromCurrentAssemblyToNodesImageList("DefaultExpandedNode.bmp");	// 2

			// Devo comunque inserire nella lista delle immagini relative allo
			// stato dell'elemento una prima immagine (quella, cioè, con indice
			// uguale a 0) che non verrà mai usata: infatti, l'indice individuato
			// dall'attributo StateImageIndex di un oggetto di classe StateTreeNode
			// deve essere sempre > 0 perchè usato per settare un certo bit
			// di tvi.state.
			AddBitmapFromCurrentAssemblyToStatesImageList("DummyState.bmp");	// 0
			AddBitmapFromCurrentAssemblyToStatesImageList("DummyState.bmp");	// 1
	
			this.StateTreeNodeToolTip = new System.Windows.Forms.ToolTip();
		}
		
		#region DrawStateIconTreeView protected overridden methods

		//---------------------------------------------------------------------------
		protected override void OnBeforeExpand(TreeViewCancelEventArgs e)
		{
			// Invoke base class implementation
			base.OnBeforeExpand(e);

			if (e.Cancel || e.Node == null || !(e.Node is StateTreeNode))
				return;

			StateTreeNode currentnode = (StateTreeNode)e.Node;
			if (currentnode.ImageIndex == GetDefaultCollapsedNodeImageIndex)
				currentnode.ImageIndex = GetDefaultExpandedNodeImageIndex;
		}
		
		//---------------------------------------------------------------------------
		protected override void OnBeforeCollapse(TreeViewCancelEventArgs e)
		{
			// Invoke base class implementation
			base.OnBeforeCollapse(e);

			if (e.Cancel || e.Node == null || !(e.Node is StateTreeNode))
				return;

			StateTreeNode currentnode = (StateTreeNode)e.Node;
			if (currentnode.ImageIndex == GetDefaultExpandedNodeImageIndex)
				currentnode.ImageIndex = GetDefaultCollapsedNodeImageIndex;
		}
		
		//---------------------------------------------------------------------------
		protected override void OnClick(EventArgs e) 
		{
			// Invoke base class implementation
			base.OnClick(e);
			
			//25 Maggio - Nadia
			//questo genera l'impossibilità di usare la OnBeforeSelect (in quanto verrebbe chiamata almeno due volte)
			//per il momento con Carlotta decidiamo di disabilitare questa parte di codice

			/*Point ptMouse = Control.MousePosition; // coordinates of the mouse cursor relative to the upper-left corner of the screen.
			StateTreeNode aNodeToSel = (StateTreeNode)GetNodeAt(PointToClient(ptMouse));
			if (aNodeToSel != null)
				SelectedNode = aNodeToSel;
				*/
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e)
		{
			// Invoke base class implementation
			base.OnMouseMove(e);

			Point ptMouse = this.PointToClient(Control.MousePosition);
			StateTreeNode aNode = (StateTreeNode)this.GetNodeAt(ptMouse);

			string currentToolTipText = String.Empty;
			if (aNode != null && aNode.Bounds.Contains(ptMouse) && aNode.IsSelected)
				currentToolTipText = aNode.ToolTipText;

			StateTreeNodeToolTip.SetToolTip(this, currentToolTipText);
		}

		#endregion

		#region DrawStateIconTreeView private methods

		//---------------------------------------------------------------------------
		private int AddBitmapFromCurrentAssemblyToNodesImageList(string resourceName)
		{
			return 	AddImageToNodesImageList("Microarea.TaskBuilderNet.UI.WinControls.Lists.Images." + resourceName);
		}

		//---------------------------------------------------------------------------
		private int AddImageToNodesImageList(string resourceName)
		{
			return 	AddImageToImageList(nodesImageList, resourceName);
		}
		
		//---------------------------------------------------------------------------
		private int AddImageFromCurrentAssemblyToNodesImageList(string resourceName)
		{
			return 	AddImageToImageList(nodesImageList, "Microarea.TaskBuilderNet.UI.WinControls.Lists.Images." + resourceName);
		}
		
		//---------------------------------------------------------------------------
		private int AddBitmapFromCurrentAssemblyToStatesImageList(string resourceName)
		{
			return 	AddImageToStateImageList("Microarea.TaskBuilderNet.UI.WinControls.Lists.Images." + resourceName);
		}

		//---------------------------------------------------------------------------
		private int AddImageToStateImageList(string resourceName)
		{
			return 	AddImageToImageList(stateImageList, resourceName);
		}
		
		//---------------------------------------------------------------------------
		private static int AddImageToImageList(System.Windows.Forms.ImageList imgList, string resourceName)
		{
			if (imgList == null || resourceName == null || resourceName == String.Empty)
				return -1;
				
			Stream imageStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
			if (imageStream == null)
				return -1;

			Image image = Image.FromStream(imageStream);
			if (image == null)
				return -1;
				
			imgList.Images.Add(image);
				
			return imgList.Images.Count-1;
		}

		#endregion

		#region DrawStateIconTreeView public methods

		//---------------------------------------------------------------------------
		public int AddImageToNodesImageList(Image imageToAdd)
		{
			if (imageToAdd == null || nodesImageList == null)
				return -1;
				
			nodesImageList.Images.Add(imageToAdd);
				
			return nodesImageList.Images.Count-1;
		}

		//---------------------------------------------------------------------------
		public int AddImageToStateImageList(Image imageToAdd)
		{
			if (imageToAdd == null || stateImageList == null)
				return -1;
				
			stateImageList.Images.Add(imageToAdd);
				
			return stateImageList.Images.Count-1;
		}
	
		//--------------------------------------------------------------------------------------------------------------------------------
		public void SetNodeAt(int index, string aNodeText)
		{
			SetNodeAt(index, new StateTreeNode(aNodeText));
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void SetNodeAt(int index, StateTreeNode node)
		{
			if (index < 0 || index >= NodesCount)
				return;

			base.Nodes[index] = node;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public StateTreeNode GetNodeAt(int index)
		{
			if (index < 0 || index >= NodesCount)
			{
				Debug.Fail("Error in DrawStateIconTreeView.GetNodeAt");
				return null;
			}

			return (StateTreeNode)base.Nodes[index];
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public int InsertNodeAt(int index, string aNodeText)
		{
			if (index < 0)
				return -1;

			return InsertNodeAt(index, new StateTreeNode(aNodeText));
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public int InsertNodeAt(int index, StateTreeNode node)
		{
			base.Nodes.Insert(index, node);

			node.RecursiveUpdateStateImages();
			
			return index;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public int AddNode(StateTreeNode node)
		{
			int index = base.Nodes.Add(node);

			if (index >= 0)
				node.RecursiveUpdateStateImages();

			return index;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void RemoveNode(StateTreeNode node)
		{
			if (node == null || !this.Nodes.Contains(node))
				return;

			StateTreeNode siblingNode = node.PrevNode;
			if (siblingNode == null)
				siblingNode = node.NextNode;
			
			base.Nodes.Remove(node);

			if (siblingNode != null)
			{
				siblingNode.UpdateImageIndexFromSiblings();
				siblingNode.UpdateStateImageIndexFromSiblings();
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void RemoveNodeAt(int index)
		{
			if (index < 0 || index >= this.Nodes.Count)
				return;

			StateTreeNode siblingNode = this.Nodes[index].PrevNode;
			if (siblingNode == null)
				siblingNode = this.Nodes[index].NextNode;
			
			base.Nodes.RemoveAt(index);

			if (siblingNode != null)
			{
				siblingNode.UpdateImageIndexFromSiblings();
				siblingNode.UpdateStateImageIndexFromSiblings();
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void Clear()
		{
			base.Nodes.Clear();
		}

		#endregion

		#region DrawStateIconTreeView public properties

		//--------------------------------------------------------------------------------------------------------------------------------
		public IEnumerator Enumerator {	get { return base.Nodes.GetEnumerator(); } }

		//--------------------------------------------------------------------------------------------------------------------------------
		public new StateTreeNodeCollection Nodes { get { return nodes; } }

		//--------------------------------------------------------------------------------------------------------------------------------
		public int NodesCount { get { return base.Nodes.Count; } }

		//--------------------------------------------------------------------------------------------------------------------------------
		public new System.Windows.Forms.ImageList ImageList 
		{
			get { return nodesImageList; } 
		}

		//---------------------------------------------------------------------------
		public bool ShowImages 
		{
			get { return showImages; }
			set
			{
				showImages = value;

				base.ImageList = showImages ? nodesImageList : null; 
			}
		}

        //---------------------------------------------------------------------------
        public bool ShowStateImages
        {
            get { return showStateImages; }
            set
            {
                if (this.Disposing || this.IsDisposed)
                    return;

                showStateImages = value;

                this.StateImageList = showStateImages ? stateImageList : null;
            }
        }

		//--------------------------------------------------------------------------------------------------------------------------------
		public new StateTreeNode SelectedNode { get { return (StateTreeNode)base.SelectedNode; } set { base.SelectedNode = (TreeNode)value;}}
	
		//------------------------------------------------------------------------
		public static int GetDummyImageIndex { get { return 0; } }
		//------------------------------------------------------------------------
		public static int GetDefaultCollapsedNodeImageIndex { get { return 1; } }
		//------------------------------------------------------------------------
		public static int GetDefaultExpandedNodeImageIndex { get { return 2; } }
		//------------------------------------------------------------------------
		public static int GetDummyStateImageIndex { get { return 1; } }

		#endregion
	}
	
	//============================================================================
	public class StateTreeNode : TreeNode, IStateTreeNodesOwner
	{
		private StateTreeNodeCollection	nodes			= null;
		private ContextMenu 			contextMenu		= null;

		//--------------------------------------------------------------------------------------------------------------------------------
		public StateTreeNode(string aNodeText, bool setDefaultImagesFlag)
		{
			Text = aNodeText;
			
			nodes = new StateTreeNodeCollection(this);

			contextMenu	= new ContextMenu();

			if (setDefaultImagesFlag)
				SetDefaultImages();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public StateTreeNode(string aNodeText, int aImageIndex, int aStateImageIndex) : this(aNodeText, false)
		{
			ImageIndex = aImageIndex;

			StateImageIndex = aStateImageIndex;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public StateTreeNode(string aNodeText) : this(aNodeText, -1, -1)
		{
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public StateTreeNode() : this(String.Empty)
		{
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		internal void RecursiveUpdateStateImages()
		{
			UpdateSiblingsImageIndexes();
			UpdateSiblingsStateImageIndexes();

			if (this.Nodes == null || this.Nodes.Count == 0)
				return;

			foreach(StateTreeNode childNode in this.Nodes)
				childNode.RecursiveUpdateStateImages();
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		public void SetDefaultImages()
		{
			this.ImageIndex = DrawStateIconTreeView.GetDefaultCollapsedNodeImageIndex;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void SetNodeAt(int index, string aNodeText)
		{
			SetNodeAt(index, new StateTreeNode(aNodeText));
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void SetNodeAt(int index, StateTreeNode node)
		{
			if (index < 0 || index >= NodesCount)
				return;

			base.Nodes[index] = node;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public StateTreeNode GetNodeAt(int index)
		{
			if (index < 0 || index >= NodesCount)
			{
				Debug.Fail("Error in StateTreeNode.GetNodeAt");
				return null;
			}

			return (StateTreeNode)base.Nodes[index];
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public int InsertNodeAt(int index, StateTreeNode node)
		{
			base.Nodes.Insert(index, node);
			
			RecursiveUpdateStateImages();

			return index;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public int AddNode(StateTreeNode node)
		{
			int index = base.Nodes.Add(node);

			if (index >= 0)
				RecursiveUpdateStateImages();

			return index;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void RemoveNode(StateTreeNode node)
		{
			if (node == null || !this.Nodes.Contains(node))
				return;

			StateTreeNode siblingNode = node.PrevNode;
			if (siblingNode == null)
				siblingNode = node.NextNode;

			base.Nodes.Remove(node);

			if (siblingNode != null)
			{
				siblingNode.UpdateImageIndexFromSiblings();
				siblingNode.UpdateStateImageIndexFromSiblings();
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void RemoveNodeAt(int index)
		{
			if (index < 0 || index >= this.Nodes.Count)
				return;

			StateTreeNode siblingNode = this.Nodes[index].PrevNode;
			if (siblingNode == null)
				siblingNode = this.Nodes[index].NextNode;
			
			base.Nodes.RemoveAt(index);
			
			if (siblingNode != null)
			{
				siblingNode.UpdateImageIndexFromSiblings();
				siblingNode.UpdateStateImageIndexFromSiblings();
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void Clear()
		{
			base.Nodes.Clear();
		}
      
		//--------------------------------------------------------------------------------------------------------------------------------
		public void SetUndefinedStateImage()
		{
			StateImageIndex = DrawStateIconTreeView.GetDummyStateImageIndex; 
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void UpdateSiblingsImageIndexes()
		{
			if (ImageIndex >= DrawStateIconTreeView.GetDummyImageIndex)
			{
				StateTreeNode prevNode = this.PrevNode;
				if (prevNode != null && prevNode.ImageIndex < DrawStateIconTreeView.GetDummyImageIndex)
					prevNode.ImageIndex = DrawStateIconTreeView.GetDummyImageIndex;
					
				StateTreeNode nextNode = this.NextNode;
				if (nextNode != null && nextNode.ImageIndex < DrawStateIconTreeView.GetDummyImageIndex)
					nextNode.ImageIndex = DrawStateIconTreeView.GetDummyImageIndex;
			}
			else
				UpdateImageIndexFromSiblings();
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		public void UpdateImageIndexFromSiblings()
		{
			StateTreeNode prevNode = this.PrevNode;
			if (prevNode != null && prevNode.ImageIndex >= DrawStateIconTreeView.GetDummyImageIndex)
				if (ImageIndex < DrawStateIconTreeView.GetDummyImageIndex)
					ImageIndex = DrawStateIconTreeView.GetDummyImageIndex;
			else
			{
				StateTreeNode nextNode = this.NextNode;
				if (nextNode != null && nextNode.ImageIndex >= DrawStateIconTreeView.GetDummyImageIndex)
					if (ImageIndex < DrawStateIconTreeView.GetDummyImageIndex)
						ImageIndex = DrawStateIconTreeView.GetDummyImageIndex;
			}
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		public void UpdateSiblingsStateImageIndexes()
		{
			if (StateImageIndex >= DrawStateIconTreeView.GetDummyStateImageIndex)
			{
				StateTreeNode prevNode = this.PrevNode;
				if (prevNode != null && prevNode.StateImageIndex < DrawStateIconTreeView.GetDummyStateImageIndex)
					prevNode.StateImageIndex = DrawStateIconTreeView.GetDummyStateImageIndex;
					
				StateTreeNode nextNode = this.NextNode;
				if (nextNode != null && nextNode.StateImageIndex < DrawStateIconTreeView.GetDummyStateImageIndex)
					nextNode.StateImageIndex = DrawStateIconTreeView.GetDummyStateImageIndex;
			}
			else
				UpdateStateImageIndexFromSiblings();
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		public void UpdateStateImageIndexFromSiblings()
		{
			StateTreeNode prevNode = this.PrevNode;
			if (prevNode != null && prevNode.StateImageIndex >= DrawStateIconTreeView.GetDummyStateImageIndex)
				if (StateImageIndex < DrawStateIconTreeView.GetDummyStateImageIndex)
					StateImageIndex = DrawStateIconTreeView.GetDummyStateImageIndex;
				else
				{
					StateTreeNode nextNode = this.NextNode;
					if (nextNode != null && nextNode.StateImageIndex >= DrawStateIconTreeView.GetDummyStateImageIndex)
						if (StateImageIndex < DrawStateIconTreeView.GetDummyStateImageIndex)
							StateImageIndex = DrawStateIconTreeView.GetDummyStateImageIndex;
				}
		}
		
		//--------------------------------------------------------------------------------------------------------------------------------
		public IEnumerator Enumerator
		{
			get { return base.Nodes.GetEnumerator(); }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		new public StateTreeNodeCollection Nodes
		{
			get { return nodes; }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public int NodesCount
		{
			get { return base.Nodes.Count; }
		}

		/// <summary>
		/// The item's image index
		/// </summary>
		//--------------------------------------------------------------------------------------------------------------------------------
		public new int ImageIndex
		{
			get { return base.ImageIndex; }
			set 
			{
				base.ImageIndex = (value > DrawStateIconTreeView.GetDummyImageIndex) ? value : DrawStateIconTreeView.GetDummyImageIndex;
				
				base.SelectedImageIndex = base.ImageIndex;
			
				UpdateSiblingsImageIndexes();
			}
		}

		//---------------------------------------------------------------------
		public bool HasContextMenu
		{
			get { return contextMenu != null; }
		}

		//---------------------------------------------------------------------
		public ContextMenu NodeContextMenu
		{
			get { return contextMenu; }
			set { contextMenu = value; }
		}

       //--------------------------------------------------------------------------------------------------------------------------------
        public new StateTreeNode Parent
        {
            get
            {
                try
                {
                    return base.Parent as StateTreeNode;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
        //--------------------------------------------------------------------------------------------------------------------------------
        public new StateTreeNode PrevNode
        {
            get
            {
                try
                {
                    return base.PrevNode as StateTreeNode;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
        //--------------------------------------------------------------------------------------------------------------------------------
		public new StateTreeNode NextNode 
		{
			get 
			{ 
				try
				{
                    return base.NextNode as StateTreeNode;
				}
				catch(Exception)
				{
					return null;
				}
			} 
		}
	}

	/// <summary>
	/// </summary>
	//============================================================================
	public class StateTreeNodeCollection : IList, ICollection, IEnumerable
	{
		private IStateTreeNodesOwner owner = null;

		//--------------------------------------------------------------------------------------------------------------------------------
		public StateTreeNodeCollection(object aOwner)
		{
			if (aOwner != null && (aOwner is IStateTreeNodesOwner))
				owner = (IStateTreeNodesOwner)aOwner;
		}
        
		#region IList implemented members...

		//--------------------------------------------------------------------------------------------------------------------------------
		object IList.this[int index] 
		{
			get { return this[index]; }
			set { this[index] = (StateTreeNode)value; }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool IList.Contains(object item)
		{
			throw new NotSupportedException();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		int IList.Add(object item)
		{
			return Add((StateTreeNode)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool IList.IsFixedSize 
		{
			get { return false; }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		int IList.IndexOf(object item)
		{
			throw new NotSupportedException();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.Insert(int index, object item)
		{
			Insert(index, (StateTreeNode)item);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.Remove(object item)
		{
			if (item == null)
				return;
			
			if (item is StateTreeNode)
			{
				RemoveNode((StateTreeNode)item);
				return;
			}
			Debug.Fail("Error in StateTreeNodeCollection.Remove");
			throw new NotSupportedException();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		void IList.RemoveAt(int index)
		{
			RemoveAt(index);
		}

        #endregion

		#region ICollection implemented members...

		//--------------------------------------------------------------------------------------------------------------------------------
		void ICollection.CopyTo(Array array, int index) 
		{
			for (IEnumerator e = GetEnumerator(); e.MoveNext();)
				array.SetValue(e.Current, index++);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		bool ICollection.IsSynchronized 
		{
			get { return false; }
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		object ICollection.SyncRoot 
		{
			get { return this; }
		}

        #endregion

		//--------------------------------------------------------------------------------------------------------------------------------
		public int Count 
		{
			get 
			{
				return (owner != null) ? owner.NodesCount : 0;
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public bool IsReadOnly 
		{
			get { return false; }
		}
			
		//--------------------------------------------------------------------------------------------------------------------------------
		public StateTreeNode this[int index]
		{
			get 
			{
				return (owner != null) ? owner.GetNodeAt(index) : null;
			}
			set
			{
				if (owner != null)
					owner.SetNodeAt(index, value);
			}
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public IEnumerator GetEnumerator() 
		{
			return (owner != null) ? owner.Enumerator : null;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public bool Contains(object item)
		{
			for (int i = 0; i < this.Count; i++)
			{
				if (this[i].Equals(item))
					return true;
			}
			return false;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public int IndexOf(object item)
		{
			for (int i = 0; i < this.Count; i++)
			{
				if (this[i].Equals(item))
					return i;
			}
			return -1;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void RemoveNode(StateTreeNode node)
		{
			if (owner != null)
				owner.RemoveNode(node);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void Insert(int index, StateTreeNode node)
		{
			if (owner != null)
				owner.InsertNodeAt(index, node);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public int Add(StateTreeNode item)
		{
			return (owner != null) ? owner.InsertNodeAt(Count, item) : -1;
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void AddRange(StateTreeNode[] items)
		{
			for(IEnumerator e = items.GetEnumerator(); e.MoveNext();)
				Add((StateTreeNode)e.Current);
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void Clear()
		{
			if (owner != null)
				owner.Clear();
		}

		//--------------------------------------------------------------------------------------------------------------------------------
		public void RemoveAt(int index)
		{
			if (owner != null)
				owner.RemoveNodeAt(index);
		}
	}
	
	//============================================================================
	public interface IStateTreeNodesOwner
	{
		StateTreeNode	GetNodeAt(int index);
		void			SetNodeAt(int index, StateTreeNode item);
		int				InsertNodeAt(int index, StateTreeNode item);
		void			RemoveNodeAt(int index);
		void			RemoveNode(StateTreeNode node);
		void			Clear();

		int NodesCount { get; }
		IEnumerator Enumerator { get; }
	}
}