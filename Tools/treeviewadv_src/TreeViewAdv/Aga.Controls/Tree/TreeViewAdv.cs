using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Security.Permissions;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Collections;
using System.Drawing.Design;
using Aga.Controls.Tree.NodeControls;
using System.Runtime.InteropServices;
using System.Threading;

namespace Aga.Controls.Tree
{
	public partial class TreeViewAdv : Control
	{
		#region Inner Classes

		private struct NodeControlInfo
		{
			public static readonly NodeControlInfo Empty = new NodeControlInfo();

			private NodeControl _control;
			public NodeControl Control
			{
				get { return _control; }
			}

			private Rectangle _bounds;
			public Rectangle Bounds
			{
				get { return _bounds; }
			}

			public NodeControlInfo(NodeControl control, Rectangle bounds)
			{
				_control = control;
				_bounds = bounds;
			}
		}

		private class ExpandedNode
		{
			private object _tag;
			public object Tag
			{
				get { return _tag; }
				set { _tag = value; }
			}

			private Collection<ExpandedNode> _children = new Collection<ExpandedNode>();
			public Collection<ExpandedNode> Children
			{
				get { return _children; }
				set { _children = value; }
			}
		}

        public class ToolTipProvider : IToolTipProvider
        {
            public string GetToolTip(TreeNodeAdv node)
            {
                //4112 - manage ToolTip
                return ((node.Tag as Node).ToolTipText);
            }
        }

		#endregion

		private const int LeftMargin = 7;
		private const int ItemDragSensivity = 4;
		private readonly int _columnHeaderHeight;
		private const int DividerWidth = 9; 

		private int _offsetX;
		private int _firstVisibleRow;
		private ReadOnlyCollection<TreeNodeAdv> _readonlySelection;
		private Pen _linePen;
		private Pen _markPen;
		private bool _dragMode;
		private bool _suspendUpdate;
		private bool _structureUpdating;
		private bool _needFullUpdate;
		private bool _fireSelectionEvent;
		private NodePlusMinus _plusMinus;
		private Control _currentEditor;
		private EditableControl _currentEditorOwner;
		private ToolTip _toolTip;
		//4112 + 5071
		private string _currentTextEdited = String.Empty;
		private Boolean _canDirectlyEditing = false;
		private string _defaultTextForEditing = String.Empty;
        private NodeCheckBox _nodeCheckBox = null;
        private NodeStateIcon _nodeStateIcon = null;
        private NodeTextBox _nodeTextBox = null;
        private Boolean _checkBoxNode = false;
        private Boolean _stateIconNode = false;
        private Hashtable _imagesCollection = null;
        private Boolean _dragAndDropOnSameLevel = false;
        private ContextMenu _ctxMenu = null;
        private int _idxContextMenuItemClicked = -1;
		private string _textContextMenuItemClicked = string.Empty;
        private Boolean _ContextMenuItemClickResponse = true;
        private Boolean _allowDragOver = false;
        private int _SelectOnlyOnLevel = -1;
        private Boolean _dblClickWithExpandCollapse = true;
        private String _CaptionBoxConfirm = String.Empty;
        private String _TextBoxConfirm = String.Empty;
        //TreeView con Stack
        private Stack<Node> _OldNodes = null;
        private Node _CurrentNode = null;
        private Node _NewNode = null;
        private Boolean _CancelDragDrop = false;
        private Boolean _CancelDragOver = false;
        private String _NewParentKey = String.Empty;
        private CustomColors customColors;
        public event EventHandler<WndProcEventArgs> OnWndProc;
		public event EventHandler<WndProcEventArgs> OnAfterWndProc;

        //Prog 5831
        private Node _lastChangedNode = null;
        private bool _isEditable = true;
        

		public int TopMargin = 0;

        //4112
        public Boolean CancelDragDrop
        {
            set
            {
                _CancelDragDrop = value;
            }
        }

        public Boolean CancelDragOver
        {
            set
            {
                _CancelDragOver = value;
            }
        }

        public String NewParentKey
        {
            get
            {
                return _NewParentKey;
            }
        }

        public int SelectOnlyOnLevel
        {
            get
            {
                return _SelectOnlyOnLevel;
            }
            set
            {
                _SelectOnlyOnLevel = value;
            }
        }

        public Boolean AllowDragOver
        {
            get
            {
                return _allowDragOver;
            }

            set
            {
                _allowDragOver = value;
            }
        }

        //4112
        public int IdxContextMenuItemClicked
        {
            get
            {
                return _idxContextMenuItemClicked;
            }
        }
		public string TextContextMenuItemClicked
        {
            get
            {
				return _textContextMenuItemClicked;
            }
        }
		
        //4112
        public Boolean ContextMenuItemClickResponse
        {
            get
            {
                return _ContextMenuItemClickResponse;
            }
        }

        //4112
        public String CaptionBoxConfirm
        {
            set
            {
                _CaptionBoxConfirm = value;
            }
        }

        //4112
        public String TextBoxConfirm
        {
            set
            {
                _TextBoxConfirm = value;
            }
        }

        //4112
        public Boolean DragAndDropOnSameLevel
        {
            set
            {
                _dragAndDropOnSameLevel = value;
            }
        }

        //4112
		public Boolean EditEnabled
		{
			set
			{
				NodeTextBox.EditEnabled = value;
			}
		}

		public String CurrentTextEdited
		{
			get
			{
				return _currentTextEdited;
			}
		}

		public Boolean CanDirectlyEditing
		{
			get
			{
				return _canDirectlyEditing;
			}

			set
			{
				_canDirectlyEditing = value;
			}
		}

		public String DefaultTextForEditing
		{
			get
			{
				return _defaultTextForEditing;
			}

			set
			{
				_defaultTextForEditing = value;
				NodeTextBox.DefaultTextForEditing = value;
			}
		}

        public NodeCheckBox NodeCheckBox
        {
            get
            {
                if (_nodeCheckBox == null)
                    _nodeCheckBox = new NodeCheckBox();
                return _nodeCheckBox;
            }
        }

        public NodeStateIcon NodeStateIcon
        {
            get
            {
                if (_nodeStateIcon == null)
                    _nodeStateIcon = new NodeStateIcon();
                return _nodeStateIcon;
            }
        }

        public NodeTextBox NodeTextBox
        {
            get
            {
                if (_nodeTextBox == null)
                    _nodeTextBox = new NodeTextBox();
                _nodeTextBox.Font = this.Font;    
                return _nodeTextBox;
            }
        }

        public Boolean DblClickWithExpandCollapse
        {
            set
            {
                _dblClickWithExpandCollapse = value;
            }
        }

		#region Internal Properties

		private int ColumnHeaderHeight
		{
			get
			{
				if (UseColumns)
					return _columnHeaderHeight;
				else
					return 0;
			}
		}

		/// <summary>
		/// returns all nodes, which parent is expanded
		/// </summary>
		private IEnumerable<TreeNodeAdv> ExpandedNodes
		{
			get
			{
				if (_root.Nodes.Count > 0)
				{
					TreeNodeAdv node = _root.Nodes[0];
					while (node != null)
					{
						yield return node;
						if (node.IsExpanded && node.Nodes.Count > 0)
							node = node.Nodes[0];
						else if (node.NextNode != null)
							node = node.NextNode;
						else
							node = node.BottomNode;
					}
				}
			}
		}

		private bool _suspendSelectionEvent;
		internal bool SuspendSelectionEvent
		{
			get { return _suspendSelectionEvent; }
			set 
			{ 
				_suspendSelectionEvent = value;
				if (!_suspendSelectionEvent && _fireSelectionEvent)
					OnSelectionChanged();
			}
		}

		private List<TreeNodeAdv> _rowMap;
		internal List<TreeNodeAdv> RowMap
		{
			get { return _rowMap; }
		}

		private TreeNodeAdv _selectionStart;
		internal TreeNodeAdv SelectionStart
		{
			get { return _selectionStart; }
			set { _selectionStart = value; }
		}

		private InputState _input;
		internal InputState Input
		{
			get { return _input; }
			set 
			{ 
				_input = value;
			}
		}

		private bool _itemDragMode;
		internal bool ItemDragMode
		{
			get { return _itemDragMode; }
			set { _itemDragMode = value; }
		}

		private Point _itemDragStart;
		internal Point ItemDragStart
		{
			get { return _itemDragStart; }
			set { _itemDragStart = value; }
		}


		/// <summary>
		/// Number of rows fits to the screen
		/// </summary>
		internal int PageRowCount
		{
			get
			{
                return Math.Max((DisplayRectangle.Height - TopMargin - ColumnHeaderHeight) / RowHeight, 0);
			}
		}

		/// <summary>
		/// Number of all visible nodes (which parent is expanded)
		/// </summary>
		internal int RowCount
		{
			get
			{
				return _rowMap.Count;
			}
		}

		private int _contentWidth = 0;
		private int ContentWidth
		{
			get
			{
				return _contentWidth;
			}
		}

		internal int FirstVisibleRow
		{
			get { return _firstVisibleRow; }
			set
			{
				HideEditor();
				_firstVisibleRow = value;
				UpdateView();
			}
		}

		private int OffsetX
		{
			get { return _offsetX; }
			set
			{
				HideEditor();
				_offsetX = value;
				UpdateView();
			}
		}
		private bool _autoScroll = true;
		public bool AutoScroll { get { return _autoScroll; } set { _autoScroll = value; UpdateVScrollBar(); UpdateHScrollBar(); } }
		public override Rectangle DisplayRectangle
		{
			get
			{
				Rectangle r = ClientRectangle;
				//r.Y += ColumnHeaderHeight;
				//r.Height -= ColumnHeaderHeight;
				int w = _vScrollBar.Visible ? _vScrollBar.Width : 0;
				int h = _hScrollBar.Visible ? _hScrollBar.Height : 0;
				return new Rectangle(r.X, r.Y, r.Width - w, r.Height - h);
			}
		}


		//Property usate da MagoWeb
		private Boolean _isContextMenuVisible = false;
  
		public Boolean IsContextMenuVisible
		{
			get { return _isContextMenuVisible; }
			set { _isContextMenuVisible = value; }
		}
		//

		private List<TreeNodeAdv> _selection;
		internal List<TreeNodeAdv> Selection
		{
			get { return _selection; }
		}

		#endregion

		#region Public Properties

		#region DesignTime

		private bool _fullRowSelect;
		[DefaultValue(false), Category("Behavior")]
		public bool FullRowSelect
		{
			get { return _fullRowSelect; }
			set 
			{ 
				_fullRowSelect = value;
				UpdateView();
			}
		}

		private bool _useColumns;
		[DefaultValue(false), Category("Behavior")]
		public bool UseColumns
		{
			get { return _useColumns; }
			set 
			{ 
				_useColumns = value;
				FullUpdate();
			}
		}

		private bool _showLines = true;
		[DefaultValue(true), Category("Behavior")]
		public bool ShowLines
		{
			get { return _showLines; }
			set 
			{ 
				_showLines = value;
				UpdateView();
			}
		}

		private bool _showNodeToolTips = false;
		[DefaultValue(false), Category("Behavior")]
		public bool ShowNodeToolTips
		{
			get { return _showNodeToolTips; }
			set { _showNodeToolTips = value; }
		}

		private bool _keepNodesExpanded;
		[DefaultValue(false), Category("Behavior")]
		public bool KeepNodesExpanded
		{
			get { return _keepNodesExpanded; }
			set { _keepNodesExpanded = value; }
		}

		//private ITreeModel _model;
        private TreeModel _model;
		[Category("Data")]
		public TreeModel Model
		{
			get 
            {
                if (_model == null)
                {
                    _model = new Tree.TreeModel();
                    if (_model != null)
                        UnbindModelEvents();
                    CreateNodes();
                    FullUpdate();
                    if (_model != null)
                        BindModelEvents();
                }
                return _model; 
            }
            //modifica per 4112
            //set
            //{
            //    if (_model != value)
            //    {
            //        if (_model != null)
            //            UnbindModelEvents();
            //        _model = value;
            //        CreateNodes();
            //        FullUpdate();
            //        if (_model != null)
            //            BindModelEvents();
            //    }
            //}
		}

		private BorderStyle _borderStyle;
		[DefaultValue(BorderStyle.Fixed3D), Category("Appearance")]
		public BorderStyle BorderStyle
		{
			get
			{
				return this._borderStyle;
			}
			set
			{
				if (_borderStyle != value)
				{
					_borderStyle = value;
					base.UpdateStyles();
				}
			}
		}

		private int _rowHeight = 16;
		[DefaultValue(16), Category("Appearance")]
		public int RowHeight
		{
			get
			{
				return _rowHeight;
			}
			set
			{
				if (value <= 0)
					throw new ArgumentOutOfRangeException();

				_rowHeight = value;
				FullUpdate();
			}
		}

		private TreeSelectionMode _selectionMode = TreeSelectionMode.Single;
		[DefaultValue(TreeSelectionMode.Single), Category("Behavior")]
		public TreeSelectionMode SelectionMode
		{
			get { return _selectionMode; }
			set { _selectionMode = value; }
		}

		private bool _hideSelection;
		[DefaultValue(false), Category("Behavior")]
		public bool HideSelection
		{
			get { return _hideSelection; }
			set 
			{ 
				_hideSelection = value;
				UpdateView();
			}
		}

		private float _topEdgeSensivity = 0.3f;
		[DefaultValue(0.3f), Category("Behavior")]
		public float TopEdgeSensivity
		{
			get { return _topEdgeSensivity; }
			set
			{
				if (value < 0 || value > 1)
					throw new ArgumentOutOfRangeException();
				_topEdgeSensivity = value;
			}
		}

		private float _bottomEdgeSensivity = 0.3f;
		[DefaultValue(0.3f), Category("Behavior")]
		public float BottomEdgeSensivity
		{
			get { return _bottomEdgeSensivity; }
			set
			{
				if (value < 0 || value > 1)
					throw new ArgumentOutOfRangeException("value should be from 0 to 1");
				_bottomEdgeSensivity = value;
			}
		}

		private bool _loadOnDemand;
		[DefaultValue(false), Category("Behavior")]
		public bool LoadOnDemand
		{
			get { return _loadOnDemand; }
			set { _loadOnDemand = value; }
		}

		private int _indent = 19;
		[DefaultValue(19), Category("Behavior")]
		public int Indent
		{
			get { return _indent; }
			set 
			{ 
				_indent = value;
				UpdateView();
			}
		}

		private Color _lineColor = SystemColors.ControlDark;
		[Category("Behavior")]
		public Color LineColor
		{
			get { return _lineColor; }
			set 
			{ 
				_lineColor = value;
				CreateLinePen();
				UpdateView();
			}
		}

		private Color _dragDropMarkColor = Color.Black;
		[Category("Behavior")]
		public Color DragDropMarkColor
		{
			get { return _dragDropMarkColor; }
			set 
			{ 
				_dragDropMarkColor = value;
				CreateMarkPen();
			}
		}

		private float _dragDropMarkWidth = 3.0f;
		[DefaultValue(3.0f), Category("Behavior")]
		public float DragDropMarkWidth
		{
			get { return _dragDropMarkWidth; }
			set 
			{ 
				_dragDropMarkWidth = value; 
				CreateMarkPen();
			}
		}

		private TreeColumnCollection _columns;
		[Category("Behavior"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public Collection<TreeColumn> Columns
		{
			get { return _columns; }
		}

		private NodeControlsCollection _controls;
		[Category("Behavior"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Editor(typeof(NodeControlCollectionEditor), typeof(UITypeEditor))]
		public Collection<NodeControl> NodeControls
		{
			get
			{
				return _controls;
			}
		}

		#endregion

		#region RunTime

		[Browsable(false)]
		public IEnumerable<TreeNodeAdv> AllNodes
		{
			get
			{
				if (_root.Nodes.Count > 0)
				{
					TreeNodeAdv node = _root.Nodes[0];
					while (node != null)
					{
						yield return node;
						if (node.Nodes.Count > 0)
							node = node.Nodes[0];
						else if (node.NextNode != null)
							node = node.NextNode;
						else
							node = node.BottomNode;
					}
				}
			}
		}

		private DropPosition _dropPosition;
		[Browsable(false)]
		public DropPosition DropPosition
		{
			get { return _dropPosition; }
			set { _dropPosition = value; }
		}

		private TreeNodeAdv _root;
		[Browsable(false)]
		public TreeNodeAdv Root
		{
			get { return _root; }
		}

		[Browsable(false)]
		public ReadOnlyCollection<TreeNodeAdv> SelectedNodes
		{
			get
			{
				return _readonlySelection;
			}
		}

		[Browsable(false)]
		public TreeNodeAdv SelectedNode
		{
			get
			{
				if (Selection.Count > 0)
				{
					if (CurrentNode != null && CurrentNode.IsSelected)
						return CurrentNode;
					else
						return Selection[0];
				}
				else
					return null;
			}
			set
			{
				if (SelectedNode == value)
					return;

				BeginUpdate();
				try
				{
					if (value == null)
					{
						ClearSelection();
					}
					else
					{
						if (!IsMyNode(value))
							throw new ArgumentException();

						ClearSelection();
						value.IsSelected = true;
						CurrentNode = value;
						EnsureVisible(value);
					}
				}
				finally
				{
					EndUpdate();
				}
			}
		}

		private TreeNodeAdv _currentNode;
		[Browsable(false)]
		public TreeNodeAdv CurrentNode
		{
			get { return _currentNode; }
			internal set { _currentNode = value; }
		}


		#endregion

		#endregion

		#region Public Events

        //Prog. 5831
        public event EventHandler<TreeModelEventArgs> NodeChanged;
        private object _lock = string.Empty;
        private void OnNodeChanged(object sender, TreeModelEventArgs e) 
        {
            lock (_lock) 
            {
                if (e != null)
                {
                    if (e.Children != null && e.Children.Length > 0)
                        _lastChangedNode = e.Children[0] as Node;
                }

                if (NodeChanged != null)
                    NodeChanged(sender, e);
            }
        }
	
		[Category("Action")]
		public event ItemDragEventHandler ItemDrag;
		private void OnItemDrag(MouseButtons buttons, object item)
		{
            if (ItemDrag != null)
            {
                //4112
                TreeNodeAdv[] nodes = new TreeNodeAdv[SelectedNodes.Count];
                SelectedNodes.CopyTo(nodes, 0);
                DoDragDrop(nodes, DragDropEffects.Move);
                //
                ItemDrag(this, new ItemDragEventArgs(buttons, item));
            }
		}

		[Category("Behavior")]
		public event EventHandler<TreeNodeAdvMouseEventArgs> NodeMouseDoubleClick;
		private void OnNodeMouseDoubleClick(TreeNodeAdvMouseEventArgs args)
		{
			if (NodeMouseDoubleClick != null)
				NodeMouseDoubleClick(this, args);
		}

		[Category("Behavior")]
		public event EventHandler<TreeColumnEventArgs> ColumnWidthChanged;
		internal void OnColumnWidthChanged(TreeColumn column)
		{
			if (ColumnWidthChanged != null)
				ColumnWidthChanged(this, new TreeColumnEventArgs(column));
		}

        //4112 - call this method on SelectionChange to add a separator to the context menu
        public void AddSeparatorMenuItem()
        {
            if (SelectedNode != null && _ctxMenu != null)
            {
                MenuItem mi;
                mi = _ctxMenu.MenuItems.Add("-");
                //mi.Click += new EventHandler(OnContextMenuItemClick);
            }
        }

        //4112 - call this method on SelectionChanged
        public void AddContextMenuItem(string itemMenu, Boolean confirm)
        {
            if (SelectedNode != null && !string.IsNullOrEmpty(itemMenu) && _ctxMenu != null)
            {
                MenuItem mi;
                mi = _ctxMenu.MenuItems.Add(itemMenu);
				mi.Name = itemMenu;
                mi.Tag = confirm;
                mi.Click += new EventHandler(OnContextMenuItemClick);
            }
        }

		public void AddContextSubMenuItem(string itemMenu, string[] subItems, Boolean confirm)
		{
			if (SelectedNode != null && !string.IsNullOrEmpty(itemMenu) && subItems != null && subItems.Length != 0 && _ctxMenu != null)
			{
				MenuItem mi = _ctxMenu.MenuItems.Add(itemMenu); 
				mi.Tag = confirm;
				mi.Name = itemMenu;
				mi.Click += new EventHandler(OnContextMenuItemClick);

				foreach (string item in subItems)
				{
					MenuItem tempItem = new MenuItem(item);
					tempItem.Tag = confirm;
					tempItem.Name = item;
					tempItem.Click += new EventHandler(OnContextMenuItemClick);
					mi.MenuItems.Add(tempItem);
				}
			}
		}

        //4112
        public void RemoveContextMenuItem(int idxContextMenuItem)
        {
            if (SelectedNode != null && idxContextMenuItem > -1 && idxContextMenuItem < _ctxMenu.MenuItems.Count)
                _ctxMenu.MenuItems.RemoveAt(idxContextMenuItem);
        }

        //4112
        public void AddContextMenuItemDisabled(string itemMenu)
        {
            if (SelectedNode != null && !string.IsNullOrEmpty(itemMenu) && _ctxMenu != null)
            {
                MenuItem mi;
                mi = _ctxMenu.MenuItems.Add(itemMenu);
                mi.Enabled = false;
            }
        }

		//4112
		public void SetMenuItemCheck(string itemMenu, bool check)
		{
			if (SelectedNode != null && !string.IsNullOrEmpty(itemMenu) && _ctxMenu != null)
			{
				MenuItem[] mi = _ctxMenu.MenuItems.Find(itemMenu, true);
				if (mi == null || mi.Length == 0)
					return;
				mi[0].Checked = check;
			}
		}

		//4112
		public void SetMenuItemEnable(string itemMenu, bool enabled)
		{
			if (SelectedNode != null && !string.IsNullOrEmpty(itemMenu) && _ctxMenu != null)
			{
				MenuItem[] mi = _ctxMenu.MenuItems.Find(itemMenu, true);
				if (mi == null || mi.Length == 0)
					return;
				mi[0].Enabled = enabled;
			}
		}

        [DllImport("User32", CharSet = CharSet.Auto)]
        public static extern bool PostMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
		
        //4112
        [Category("Behavior")]
        public event EventHandler ContextMenuItemClick;
        internal void OnContextMenuItemClick(object sender, EventArgs e)
        { 
            _idxContextMenuItemClicked = ((MenuItem)sender).Index;
			_textContextMenuItemClicked = ((MenuItem)sender).Text;
            if ((Boolean)((MenuItem)sender).Tag)
            {
                DialogResult resp = MessageBox.Show(_TextBoxConfirm, _CaptionBoxConfirm, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                _ContextMenuItemClickResponse = (resp == DialogResult.Yes);
            }
            ContextMenuItemClick(this, EventArgs.Empty);

        }
       
        //

		[Category("Behavior")]
		public event EventHandler SelectionChanged;
		internal void OnSelectionChanged()
		{
			if (SuspendSelectionEvent)
				_fireSelectionEvent = true;
			else
			{
				_fireSelectionEvent = false;
                if (SelectionChanged != null)
                {
					_ctxMenu = new ContextMenu();
					_ctxMenu.Collapse += new EventHandler(_ctxMenu_Collapse);
					_ctxMenu.Popup += new EventHandler(_ctxMenu_Popup);
					this.ContextMenu = _ctxMenu;
                    SelectionChanged(this, EventArgs.Empty);
                }
			}
		}

		void _ctxMenu_Popup(object sender, EventArgs e)
		{
			IsContextMenuVisible = true;
		}

		void _ctxMenu_Collapse(object sender, EventArgs e)
		{
			IsContextMenuVisible = false;
		}

		[Category("Behavior")]
		public event EventHandler<TreeViewAdvEventArgs> Collapsing;
		internal void OnCollapsing(TreeNodeAdv node)
		{
			if (Collapsing != null)
				Collapsing(this, new TreeViewAdvEventArgs(node));
		}

		[Category("Behavior")]
		public event EventHandler<TreeViewAdvEventArgs> Collapsed;
		internal void OnCollapsed(TreeNodeAdv node)
		{
			if (Collapsed != null)
				Collapsed(this, new TreeViewAdvEventArgs(node));
		}

		[Category("Behavior")]
		public event EventHandler<TreeViewAdvEventArgs> Expanding;
		internal void OnExpanding(TreeNodeAdv node)
		{
			if (Expanding != null)
				Expanding(this, new TreeViewAdvEventArgs(node));
		}

		[Category("Behavior")]
		public event EventHandler<TreeViewAdvEventArgs> Expanded;
		internal void OnExpanded(TreeNodeAdv node)
		{
			if (Expanded != null)
				Expanded(this, new TreeViewAdvEventArgs(node));
		}

		#endregion

		public TreeViewAdv()
		{
            _model = null;
			InitializeComponent();
			SetStyle(ControlStyles.AllPaintingInWmPaint
				| ControlStyles.UserPaint
				| ControlStyles.OptimizedDoubleBuffer
				| ControlStyles.ResizeRedraw
				| ControlStyles.Selectable
				, true);


			if (Application.RenderWithVisualStyles)
				_columnHeaderHeight = 20;
			else
				_columnHeaderHeight = 17;

			BorderStyle = BorderStyle.Fixed3D;
			_hScrollBar.Height = SystemInformation.HorizontalScrollBarHeight;
			_vScrollBar.Width = SystemInformation.VerticalScrollBarWidth;
			_rowMap = new List<TreeNodeAdv>();
			_selection = new List<TreeNodeAdv>();
			_readonlySelection = new ReadOnlyCollection<TreeNodeAdv>(_selection);
			_columns = new TreeColumnCollection(this);
			_toolTip = new ToolTip();
            customColors = new CustomColors();

			Input = new NormalInputState(this);
			CreateNodes();
			CreatePens();

			ArrangeControls();

			_plusMinus = new NodePlusMinus();
			_controls = new NodeControlsCollection(this);

            //4112
            _OldNodes = new Stack<Node>();
            _imagesCollection = new Hashtable();

        }


		#region Public Methods

		public void BeginEdit()
		{
			if (SelectedNode == null)
				return;

			_currentTextEdited = String.Empty;
			NodeTextBox.BeginEdit();
		}

        public void SetCheckBoxControls(Boolean bValue)
        {
            _checkBoxNode = bValue;
        }

        public void SetNodeStateIcon(Boolean bValue)
        {
            _stateIconNode = bValue;
        }

        //4112
        public void AddImage(String imageKey, String imagePath)
        {
            if (_imagesCollection[imageKey.ToUpper()] == null)
            {
                _imagesCollection.Add(imageKey.ToUpper(), imagePath); 
            }
        }

        //4112
        private void SetImages()
        {
            foreach (string key in _imagesCollection.Keys)
                NodeStateIcon.AddImage(key, _imagesCollection[key] as String);
        }

        //4112
        public void SetImage(String nodeKey, String imageKey)
        {
            BeginUpdate();
            SetNodeAsSelected(nodeKey);
            if (SelectedNode == null)
                return;
            (SelectedNode.Tag as Node).NodeImageKey = imageKey;
            EndUpdate();
        }

        //4112
		// Seleziona l'icona del nodo senza spostarvici
		public void SetImageNoSel(String nodeKey, String imageKey)
		{
			BeginUpdate();
			TreeNodeAdv aNode = GetNode(nodeKey);
			if (aNode == null)
				return;
			(aNode.Tag as Node).NodeImageKey = imageKey;
			EndUpdate();
		}

		//4112
        public void SetSelectedNodeImage(String imageKey)
        {
            if (SelectedNode == null)
                return;

            BeginUpdate();
            (SelectedNode.Tag as Node).NodeImageKey = imageKey;
            EndUpdate();
        }

		//5093
		public List<string> GetImageKeys()
		{
			List<string> keys = new List<string>();
			foreach (string key in _imagesCollection.Keys)
			{
				keys.Add(key);
			}
			return keys; 
		}


        public void AddControls()
        {
            if (_checkBoxNode)
            //5831
            {
                NodeCheckBox cbControl = NodeCheckBox;
                cbControl.EditEnabled = IsEditable;
                NodeControls.Add(cbControl);
            }
            if (_stateIconNode)
            {
                //4112
                //set all images - SetImages
                SetImages();
                NodeControls.Add(NodeStateIcon);
            }
            NodeControls.Add(NodeTextBox);
        }

        //5831
        public void SetChecksBoxEditable(Boolean bValue)
        {
            foreach (NodeControl c in NodeControls)
            {
                if (c.GetType().Equals(typeof(NodeCheckBox)))
                {
                    NodeCheckBox checkBox = (NodeCheckBox)c;
                    checkBox.EditEnabled = bValue;
                }
            }
                /*foreach (NodeControlInfo c in GetNodeControls(node))
                {
                    if (c.Control.GetType().Equals(typeof(NodeCheckBox)))
                    {
                        NodeCheckBox checkBox = (NodeCheckBox)c.Control;
                        checkBox.EditEnabled = bValue;
                    }
                }*/
            
        }

        public void SetNodeCheckBoxProperty(string dataPropertyName)
        {
            NodeCheckBox.DataPropertyName = dataPropertyName;
        }

        public void SetNodeCheckBoxThreeState(Boolean threeState)
        {
            NodeCheckBox.ThreeState = threeState;
        }

        public void SetNodeTextBoxProperty(string dataPropertyName)
        {
            NodeTextBox.DataPropertyName = dataPropertyName;
        }

        public void SetToolTip()
        {
            NodeTextBox.ToolTipProvider = new ToolTipProvider();
        }

		public TreePath GetPath(TreeNodeAdv node)
		{
			if (node == _root)
				return TreePath.Empty;
			else
			{
				Stack<object> stack = new Stack<object>();
				while (node != _root)
				{
					stack.Push(node.Tag);
					node = node.Parent;
				}
				return new TreePath(stack.ToArray());
			}
		}

		public TreeNodeAdv GetNodeAt(Point point)
		{
			if (point.X < 0 || point.Y < 0)
				return null;
			
			point = ToAbsoluteLocation(point);
			int row = (point.Y - TopMargin) / RowHeight;
			if (row < RowCount && row >= 0)
			{
				NodeControlInfo info = GetNodeControlInfoAt(_rowMap[row], point);
				if (info.Control != null)
					return _rowMap[row];
			}
			return null;
		}

		public void BeginUpdate()
		{
			_suspendUpdate = true;
		}

		public void EndUpdate()
		{
			_suspendUpdate = false;
			if (_needFullUpdate)
				FullUpdate();
			else
				UpdateView();
		}

        //4112
        private void SetIsExpanded(TreeNodeAdv root, bool value, int level)
        {
            foreach (TreeNodeAdv node in root.Nodes)
            {
                if (node.Level <= level)
                {
                    node.IsExpanded = value;
                    SetIsExpanded(node, value, level);
                }
            }
        }

        //4112
		public void ExpandAll(Boolean fromRoot)
		{
            BeginUpdate();
            //SetIsExpanded(SelectedNode.Parent, true);
            if (fromRoot && Root != null)
                SetIsExpanded(Root, true);
            else if (SelectedNode != null)
            {
                SetIsExpanded(SelectedNode, true);
                SelectedNode.IsExpanded = true;
            }
			EndUpdate();
		}

        //4112
        public void ExpandLevels(int level)
        {
            BeginUpdate();
            if (Root != null)
                SetIsExpanded(Root, true, level);
            EndUpdate();
        }

        //4112
		public void CollapseAll(Boolean fromRoot)
		{
            BeginUpdate();
            //SetIsExpanded(SelectedNode.Parent, false);
            if (fromRoot && Root != null)
                SetIsExpanded(Root, false);
            else if (SelectedNode != null)
            {
                SetIsExpanded(SelectedNode, false);
                SelectedNode.IsExpanded = false;
            }
			EndUpdate();
		}


		/// <summary>
		/// Expand all parent nodes, andd scroll to the specified node
		/// </summary>
		public void EnsureVisible(TreeNodeAdv node)
		{
			if (node == null)
				throw new ArgumentNullException("node");

			if (!IsMyNode(node))
				throw new ArgumentException();

			TreeNodeAdv parent = node.Parent;
			while (parent != _root)
			{
				parent.IsExpanded = true;
				parent = parent.Parent;
			}
			ScrollTo(node);
		}

		/// <summary>
		/// Make node visible, scroll if needed. All parent nodes of the specified node must be expanded
		/// </summary>
		/// <param name="node"></param>
		public void ScrollTo(TreeNodeAdv node)
		{
			if (node == null)
				throw new ArgumentNullException("node");

			if (!IsMyNode(node))
				throw new ArgumentException();

			if (node.Row < 0)
				CreateRowMap();

			int row = 0;
            if (node.Row < FirstVisibleRow)
                row = node.Row;
            else if (node.Row >= FirstVisibleRow + (PageRowCount - 1))
                row = node.Row - (PageRowCount - 1);
            else
                return;

			if (row >= _vScrollBar.Minimum && row <= _vScrollBar.Maximum)
				_vScrollBar.Value = row;
		}

		#endregion

		private Point ToAbsoluteLocation(Point point)
		{
			return new Point(point.X + _offsetX, point.Y + (FirstVisibleRow * RowHeight) - ColumnHeaderHeight);
		}

		private Point ToViewLocation(Point point)
		{
			return new Point(point.X - _offsetX, point.Y - (FirstVisibleRow * RowHeight) + ColumnHeaderHeight);
		}

		protected override void OnSizeChanged(EventArgs e)
		{
			ArrangeControls();
			SafeUpdateScrollBars();
			base.OnSizeChanged(e);
		}

		private void ArrangeControls()
		{
			int hBarSize = _hScrollBar.Height;
			int vBarSize = _vScrollBar.Width;
			Rectangle clientRect = ClientRectangle;
			
			_hScrollBar.SetBounds(clientRect.X, clientRect.Bottom - hBarSize,
				clientRect.Width - vBarSize, hBarSize);

			_vScrollBar.SetBounds(clientRect.Right - vBarSize, clientRect.Y,
				vBarSize, clientRect.Height - hBarSize);
		}

		private void SafeUpdateScrollBars()
		{
			if (InvokeRequired)
				Invoke(new MethodInvoker(UpdateScrollBars));
			else
				UpdateScrollBars();
		}

		private void UpdateScrollBars()
		{
			UpdateVScrollBar();
			UpdateHScrollBar();
			UpdateVScrollBar();
			UpdateHScrollBar();
			_hScrollBar.Width = DisplayRectangle.Width;
			_vScrollBar.Height = DisplayRectangle.Height;
		}

		private void UpdateHScrollBar()
		{
			_hScrollBar.Maximum = ContentWidth;
			_hScrollBar.LargeChange = Math.Max(DisplayRectangle.Width, 0);
			_hScrollBar.SmallChange = 5;
			_hScrollBar.Visible = _autoScroll && _hScrollBar.LargeChange < _hScrollBar.Maximum;
			_hScrollBar.Value = Math.Min(_hScrollBar.Value, _hScrollBar.Maximum - _hScrollBar.LargeChange + 1);
		}

		private void UpdateVScrollBar()
		{
			_vScrollBar.LargeChange = PageRowCount;
			_vScrollBar.Maximum = Math.Max(RowCount - 1, 0) + ((_vScrollBar.LargeChange > 0) ? TopMargin / _vScrollBar.LargeChange : 0);
			_vScrollBar.Visible = _autoScroll && _vScrollBar.LargeChange <= _vScrollBar.Maximum;
			_vScrollBar.Value = Math.Min(_vScrollBar.Value, _vScrollBar.Maximum - _vScrollBar.LargeChange + 1);
		}

		private void CreatePens()
		{
			CreateLinePen();
			CreateMarkPen();
		}

		private void CreateMarkPen()
		{
			GraphicsPath path = new GraphicsPath();
			path.AddLines(new Point[] { new Point(0, 0), new Point(1, 1), new Point(-1, 1), new Point(0, 0) });
			if (_markPen != null)
			{
				_markPen.Dispose();
			}
			CustomLineCap customCap = new CustomLineCap(null, path);
			customCap.WidthScale = 1.0f;
			_markPen = new Pen(_dragDropMarkColor, _dragDropMarkWidth);
			_markPen.CustomStartCap = customCap;
			_markPen.CustomEndCap = customCap;
		}

		private void CreateLinePen()
		{
			if (_linePen != null)
				_linePen.Dispose();
			_linePen = new Pen(_lineColor);
			_linePen.DashStyle = DashStyle.Dot;
		}

		protected override CreateParams CreateParams
		{
			[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
			get
			{
				CreateParams res = base.CreateParams;
				switch (BorderStyle)
				{
					case BorderStyle.FixedSingle:
							res.Style |= 0x800000;
							break;
					case BorderStyle.Fixed3D:
							res.ExStyle |= 0x200;
						break;
				}
				return res;
			}
		}

		protected override void OnGotFocus(EventArgs e)
		{
			DisposeEditor();
			UpdateView();
			ChangeInput();
			base.OnGotFocus(e);
		}

		protected override void OnLeave(EventArgs e)
		{
			DisposeEditor();
			UpdateView();
			base.OnLeave(e);
		}

		#region Keys

		protected override bool IsInputKey(Keys keyData)
		{
			if (((keyData & Keys.Up) == Keys.Up)
				|| ((keyData & Keys.Down) == Keys.Down)
				|| ((keyData & Keys.Left) == Keys.Left)
				|| ((keyData & Keys.Right) == Keys.Right))
				return true;
			else
				return base.IsInputKey(keyData);
		}

		internal void ChangeInput()
		{
			if ((ModifierKeys & Keys.Shift) == Keys.Shift)
			{
				if (!(Input is InputWithShift))
					Input = new InputWithShift(this);
			}
			else if ((ModifierKeys & Keys.Control) == Keys.Control)
			{
				if (!(Input is InputWithControl))
					Input = new InputWithControl(this);
			}
			else 
			{
				if (!(Input.GetType() == typeof(NormalInputState)))
					Input = new NormalInputState(this);
			}
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
			if (!e.Handled)
			{
				if (e.KeyCode == Keys.ShiftKey || e.KeyCode == Keys.ControlKey)
					ChangeInput();
				Input.KeyDown(e);
				if (!e.Handled)
				{
					foreach (NodeControlInfo item in GetNodeControls(CurrentNode))
					{
						item.Control.KeyDown(e);
						if (e.Handled)
							return;
					}
				}
			}
		}

		protected override void OnKeyUp(KeyEventArgs e)
		{
			base.OnKeyUp(e);
			if (!e.Handled)
			{
				if (e.KeyCode == Keys.ShiftKey || e.KeyCode == Keys.ControlKey)
					ChangeInput();
				if (!e.Handled)
				{
					foreach (NodeControlInfo item in GetNodeControls(CurrentNode))
					{
						item.Control.KeyUp(e);
						if (e.Handled)
							return;
					}
				}
			}
		}

		#endregion 

		#region Mouse

		private TreeNodeAdvMouseEventArgs CreateMouseArgs(MouseEventArgs e)
		{
			TreeNodeAdvMouseEventArgs args = new TreeNodeAdvMouseEventArgs(e);
			args.ViewLocation = e.Location;
			args.AbsoluteLocation = ToAbsoluteLocation(e.Location);
			args.ModifierKeys = ModifierKeys;
			args.Node = GetNodeAt(e.Location);
			NodeControlInfo info = GetNodeControlInfoAt(args.Node, args.AbsoluteLocation);
			args.ControlBounds = info.Bounds;
			args.Control = info.Control;
			return args;
		}

		protected override void OnMouseWheel(MouseEventArgs e)
		{
			base.OnMouseWheel(e);
			if (SystemInformation.MouseWheelScrollLines > 0)
			{
				int lines = e.Delta / 120 * SystemInformation.MouseWheelScrollLines;
				int newValue = _vScrollBar.Value - lines;
				_vScrollBar.Value = Math.Max(_vScrollBar.Minimum,
					Math.Min(_vScrollBar.Maximum - _vScrollBar.LargeChange + 1, newValue));
			}
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			if (!Focused)
				Focus();

			if (e.Button == MouseButtons.Left)
			{
				TreeColumn c = GetColumnDividerAt(e.Location);
				if (c != null)
				{
					Input = new ResizeColumnState(this, c, e.Location);
					return;
				}
			}

			ChangeInput();
			TreeNodeAdvMouseEventArgs args = CreateMouseArgs(e);

			if (args.Node != null && args.Control != null)
				args.Control.MouseDown(args);

			if (!args.Handled)
				Input.MouseDown(args);
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			TreeNodeAdvMouseEventArgs args = CreateMouseArgs(e);
			if (Input is ResizeColumnState)
				Input.MouseUp(args);
			else
			{
				base.OnMouseUp(e);
				if (args.Node != null && args.Control != null)
					args.Control.MouseUp(args);
				if (!args.Handled)
					Input.MouseUp(args);
			}
		}

		protected override void OnMouseDoubleClick(MouseEventArgs e)
		{
			base.OnMouseDoubleClick(e);
			TreeNodeAdvMouseEventArgs args = CreateMouseArgs(e);
			if (args.Node != null)
			{
				OnNodeMouseDoubleClick(args);
				if (args.Handled)
					return;
			}

			if (args.Node != null && args.Control != null)
				args.Control.MouseDoubleClick(args);
			if (!args.Handled)
			{
                //4112 - modification point
                if (args.Node != null && args.Button == MouseButtons.Left && _dblClickWithExpandCollapse)
					args.Node.IsExpanded = !args.Node.IsExpanded;
			}
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (Input.MouseMove(e))
				return;

			base.OnMouseMove(e);
			SetCursor(e);
			if (e.Location.Y <= ColumnHeaderHeight)
			{
				_toolTip.Active = false;
			}
			else
			{
				UpdateToolTip(e);
				if (ItemDragMode && Dist(e.Location, ItemDragStart) > ItemDragSensivity
					&& CurrentNode != null && CurrentNode.IsSelected)
				{
					ItemDragMode = false;
					_toolTip.Active = false;
					OnItemDrag(e.Button, Selection.ToArray());
				}
			}
		}

		private void SetCursor(MouseEventArgs e)
		{
			if (GetColumnDividerAt(e.Location) == null)
				this.Cursor = Cursors.Default;
			else
				this.Cursor = Cursors.VSplit;
		}

		private TreeColumn GetColumnDividerAt(Point p)
		{
			if (p.Y > ColumnHeaderHeight)
				return null;

			int x = -OffsetX;
			foreach (TreeColumn c in Columns)
			{
				if (c.IsVisible)
				{
					x += c.Width;
					Rectangle rect = new Rectangle(x - DividerWidth / 2, 0, DividerWidth, ColumnHeaderHeight);
					if (rect.Contains(p))
						return c;
				}
			}
			return null;
		}

        //4112 - manage custom toolTip
        private void _toolTip_Draw(object sender, DrawToolTipEventArgs e)
        {
			using (LinearGradientBrush myBrush = new LinearGradientBrush(new Point(e.Bounds.X, e.Bounds.Y + e.Bounds.Height), new Point(e.Bounds.X, e.Bounds.Y), _toolTip.BackColor, Color.WhiteSmoke))
			{
            TextFormatFlags format = TextFormatFlags.PathEllipsis;
            e.Graphics.FillRectangle(myBrush, e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height);

            e.DrawBorder();
            e.DrawText(format);
        }
		}

        //4112 - Set balloon for ToolTip
        public void SetBalloonToolTip(Boolean isBalloon)
        {
            _toolTip.IsBalloon = isBalloon;
        }

        //4112
        public void SetCustomToolTip(Color bkColor, Color foreColor)
        {
            _toolTip.OwnerDraw = true;
            _toolTip.BackColor = bkColor;
            _toolTip.ForeColor = foreColor;
        }

        TreeNodeAdv _hoverNode;
		NodeControl _hoverControl;
		private void UpdateToolTip(MouseEventArgs e)
		{
			if (_showNodeToolTips)
			{
				TreeNodeAdvMouseEventArgs args = CreateMouseArgs(e);
				if (args.Node != null)
				{
					if (args.Node != _hoverNode || args.Control != _hoverControl)
					{
						string msg = args.Control.GetToolTip(args.Node);
						if (!String.IsNullOrEmpty(msg))
						{
							_toolTip.SetToolTip(this, msg);
							_toolTip.Active = true;
                            //4112 - manage ToolTip custom
                            //_toolTip.IsBalloon = true;
                            //_toolTip.BackColor = Color.Yellow;
                            //_toolTip.ForeColor = Color.Black;
                            //_toolTip.OwnerDraw = true;
							_toolTip.Draw += new DrawToolTipEventHandler(_toolTip_Draw);
                            //
						}
						else
							_toolTip.SetToolTip(this, null);
					}
				}
				else
					_toolTip.SetToolTip(this, null);

				_hoverControl = args.Control;
				_hoverNode = args.Node;

			}
			else
				_toolTip.SetToolTip(this, null);
		}
		#endregion

		#region DragDrop
		protected override void OnDragOver(DragEventArgs drgevent)
		{
            ItemDragMode = false;
			_dragMode = true;
			Point pt = PointToClient(new Point(drgevent.X, drgevent.Y));
			SetDropPosition(pt);
			UpdateView();
			base.OnDragOver(drgevent);

            //4112
            if (_CancelDragOver)
            {
                _CancelDragOver = false;
                return;
            }

            //4112
            if (drgevent.Data.GetDataPresent(typeof(TreeNodeAdv[])) && DropPosition.Node != null)
                drgevent.Effect = drgevent.AllowedEffect;
            else
                drgevent.Effect = DragDropEffects.None;
		}

		protected override void OnDragLeave(EventArgs e)
		{
			_dragMode = false;
			UpdateView();
			base.OnDragLeave(e);
		}

		protected override void OnDragDrop(DragEventArgs drgevent)
		{
			_dragMode = false;
			UpdateView();

            TreeNodeAdv[] nodes = (TreeNodeAdv[])drgevent.Data.GetData(typeof(TreeNodeAdv[]));
            Node dropNode = DropPosition.Node.Tag as Node;

            //4112
            if (DropPosition.Position == NodePosition.Inside)
                _NewParentKey = dropNode.Key;
            else
                _NewParentKey = dropNode.Parent.Key;

            base.OnDragDrop(drgevent);

            //4112
            if (_CancelDragDrop)
            {
                //reset value of 
                _CancelDragDrop = false;
                return;
            }
            //
            
            //
            
            //4112 - managed if the nodes in nodes have different parents => return
            if (_dragAndDropOnSameLevel)
                foreach (TreeNodeAdv n in nodes)
                    if ((n.Tag as Node).Parent != dropNode.Parent)
                        return;
                //


            if (DropPosition.Position == NodePosition.Inside)
            {
                //only if AllowDragOver
                if (_allowDragOver)
                {
                    foreach (TreeNodeAdv n in nodes)
                    {
                        (n.Tag as Node).Parent = dropNode;
                    }
                    DropPosition.Node.IsExpanded = true;
                }
            }
            else
            {
                Node parent = dropNode.Parent;
                Node nextItem = dropNode;
                if (DropPosition.Position == NodePosition.After)
                    nextItem = dropNode.NextNode;

                foreach (TreeNodeAdv node in nodes)
                    (node.Tag as Node).Parent = null;

                int index = -1;
                index = parent.Nodes.IndexOf(nextItem);
                foreach (TreeNodeAdv node in nodes)
                {
                    Node item = node.Tag as Node;
                    if (index == -1)
                        parent.Nodes.Add(item);
                    else
                    {
                        parent.Nodes.Insert(index, item);
                        index++;
                    }
                }
            }
		}
		#endregion

		private IEnumerable<NodeControlInfo> GetNodeControls(TreeNodeAdv node)
		{
			if (node == null)
				yield break;

			int y = node.Row * RowHeight + TopMargin;
			int x = (node.Level - 1) * _indent + LeftMargin;
			int width = _plusMinus.MeasureSize(node).Width;

			Rectangle rect = new Rectangle(x, y, width, RowHeight);
			if (UseColumns && Columns.Count > 0 && Columns[0].Width < rect.Right)
				rect.Width = Columns[0].Width - x;
			yield return new NodeControlInfo(_plusMinus, rect);

			x += (width + 1);
			if (!UseColumns)
			{
				foreach (NodeControl c in NodeControls)
				{
                    // begin 5831
                    if (!(node.Tag as Node).CheckBox && c.GetType().Equals(typeof(NodeCheckBox))) 
                    {
                        continue;
                    }
                    // end 5831
					width = c.MeasureSize(node).Width; 
					rect = new Rectangle(x, y, width, RowHeight);
					x += (width + 1);
					yield return new NodeControlInfo(c, rect);
				}
			}
			else
			{
				int right = 0;
				foreach (TreeColumn col in Columns)
				{
					if (col.IsVisible)
					{
						right += col.Width;
						for (int i = 0; i < NodeControls.Count; i++)
						{
							NodeControl nc = NodeControls[i];
							if (nc.Column == col.Index)
							{
								bool isLastControl = true;
								for (int k = i + 1; k < NodeControls.Count; k++)
									if (NodeControls[k].Column == col.Index)
									{
										isLastControl = false;
										break;
									}

								width = right - x;
								if (!isLastControl)
									width = nc.MeasureSize(node).Width;
								int maxWidth = Math.Max(0, right - x);
								rect = new Rectangle(x, y, Math.Min(maxWidth, width), RowHeight);
								x += (width + 1);
								yield return new NodeControlInfo(nc, rect);
							}
						}
						x = right;
					}
				}
			}
		}

		private static double Dist(Point p1, Point p2)
		{
			return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
		}

		private void SetDropPosition(Point pt)
		{
			TreeNodeAdv node = GetNodeAt(pt);
			_dropPosition.Node = node;
			if (node != null)
			{
				float pos = (pt.Y - ColumnHeaderHeight - ((node.Row - FirstVisibleRow) * RowHeight)) / (float)RowHeight;
				if (pos < TopEdgeSensivity)
					_dropPosition.Position = NodePosition.Before;
				else if (pos > (1 - BottomEdgeSensivity))
					_dropPosition.Position = NodePosition.After;
				else
					_dropPosition.Position = NodePosition.Inside;
			}
		}

		internal void FullUpdate()
		{
			CreateRowMap();
			SafeUpdateScrollBars();
			UpdateView();
			_needFullUpdate = false;
		}

		internal void UpdateView()
		{
			if (!_suspendUpdate)
				Invalidate(false);
		}

		private void CreateNodes()
		{
			Selection.Clear();
			SelectionStart = null;
			_root = new TreeNodeAdv(this, null);
			_root.IsExpanded = true;
			if (_root.Nodes.Count > 0)
				CurrentNode = _root.Nodes[0];
			else
				CurrentNode = null;
		}

		internal void ReadChilds(TreeNodeAdv parentNode)
		{
			ReadChilds(parentNode, null);
		}

		private void ReadChilds(TreeNodeAdv parentNode, Collection<ExpandedNode> expandedNodes)
		{
			if (!parentNode.IsLeaf)
			{
				parentNode.IsExpandedOnce = true;
				foreach (TreeNodeAdv n in parentNode.Nodes)
				{
					n.Parent = null;
				}
				parentNode.Nodes.Clear();
                //modifica per 4112
				//if (Model != null)
                if (_model != null)
				{
					IEnumerable items = Model.GetChildren(GetPath(parentNode));
					if (items != null)
					{
						foreach (object obj in items)
						{
							Collection<ExpandedNode> expandedChildren = null;
							if (expandedNodes != null)
								foreach (ExpandedNode str in expandedNodes)
								{
									if (str.Tag == obj)
									{
										expandedChildren = str.Children;
										break;
									}
								}
							AddNode(parentNode, obj, -1, expandedChildren);
						}
					}
				}
			}
		}

		private void AddNode(TreeNodeAdv parent, object tag, int index, Collection<ExpandedNode> expandedChildren)
		{
			TreeNodeAdv node = new TreeNodeAdv(this, tag);
			node.Parent = parent;

			if (index >= 0 && index < parent.Nodes.Count)
				parent.Nodes.Insert(index, node);
			else
				parent.Nodes.Add(node);

			node.IsLeaf = Model.IsLeaf(GetPath(node));
			if (!LoadOnDemand)
				ReadChilds(node);
			else if (expandedChildren != null)
			{
				ReadChilds(node, expandedChildren);
				node.IsExpanded = true;
			}
		}

		private void AddNode(TreeNodeAdv parent, object tag, int index)
		{
			AddNode(parent, tag, index, null);
		}

		private void CreateRowMap()
		{
			_rowMap.Clear();
			int row = 0;
			_contentWidth = 0;
			foreach (TreeNodeAdv node in ExpandedNodes)
			{
				node.Row = row;
				_rowMap.Add(node);
				if (!UseColumns)
				{
					Rectangle rect = GetNodeBounds(node);
					_contentWidth = Math.Max(_contentWidth, rect.Right);
				}
				row++;
			}
			if (UseColumns)
			{
				_contentWidth = 0;
				foreach (TreeColumn col in _columns)
					if (col.IsVisible)
						_contentWidth += col.Width;
			}
		}

		private NodeControlInfo GetNodeControlInfoAt(TreeNodeAdv node, Point point)
		{
			foreach (NodeControlInfo info in GetNodeControls(node))
				if (info.Bounds.Contains(point))
					return info;

			return NodeControlInfo.Empty;
		}

		//Reso pubblico per magoWeb
		public Rectangle GetNodeBounds(TreeNodeAdv node)
		{
			Rectangle res = Rectangle.Empty;
			foreach (NodeControlInfo info in GetNodeControls(node))
			{
				if (res == Rectangle.Empty)
					res = info.Bounds;
				else
					res = Rectangle.Union(res, info.Bounds);
			}
			return res;
		}

		private void _vScrollBar_ValueChanged(object sender, EventArgs e)
		{
			FirstVisibleRow = _vScrollBar.Value;
		}

		private void _hScrollBar_ValueChanged(object sender, EventArgs e)
		{
			OffsetX = _hScrollBar.Value;
		}

		private void SetIsExpanded(TreeNodeAdv root, bool value)
		{
			foreach (TreeNodeAdv node in root.Nodes)
			{
				node.IsExpanded = value;
				SetIsExpanded(node, value);
			}
		}

		public void ClearSelection()
		{
			while (Selection.Count > 0)
				Selection[0].IsSelected = false;
		}

		internal void SmartFullUpdate()
		{
			if (_suspendUpdate || _structureUpdating)
				_needFullUpdate = true;
			else
				FullUpdate();
		}

		internal bool IsMyNode(TreeNodeAdv node)
		{
			if (node == null)
				return false;

			if (node.Tree != this)
				return false;

			while (node.Parent != null)
				node = node.Parent;

			return node == _root;
		}

		private void UpdateSelection()
		{
			bool flag = false;

			if (!IsMyNode(CurrentNode))
				CurrentNode = null;
			if (!IsMyNode(_selectionStart))
				_selectionStart = null;

			for (int i = Selection.Count - 1; i >= 0; i--)
				if (!IsMyNode(Selection[i]))
				{
					flag = true;
					Selection.RemoveAt(i);
				}

			if (flag)
				OnSelectionChanged();
		}

		internal void UpdateHeaders()
		{
			UpdateView();
		}

		internal void UpdateColumns()
		{
			FullUpdate();
		}

		internal void ChangeColumnWidth(TreeColumn column)
		{
			if (!(_input is ResizeColumnState))
			{
				FullUpdate();
				OnColumnWidthChanged(column);
			}
		}

		#region Draw

		protected override void OnPaint(PaintEventArgs e)
		{
			DrawContext context = new DrawContext();
			context.Graphics = e.Graphics;
            context.Font = this.Font;
            context.Enabled = Enabled;

			int y = 0;
			if (UseColumns)
			{
				DrawColumnHeaders(e.Graphics);
				y = ColumnHeaderHeight;
				if (Columns.Count == 0)
					return;
			}

			e.Graphics.ResetTransform();
			e.Graphics.TranslateTransform(-OffsetX, y - (FirstVisibleRow * RowHeight));
			int row = FirstVisibleRow;
			while (row < RowCount && row - FirstVisibleRow <= PageRowCount)
			{
				TreeNodeAdv node = _rowMap[row];
				context.DrawSelection = DrawSelectionMode.None;
				context.CurrentEditorOwner = _currentEditorOwner;
				if (_dragMode)
				{
					if ((_dropPosition.Node == node) && _dropPosition.Position == NodePosition.Inside)
						context.DrawSelection = DrawSelectionMode.Active;
				}
				else
				{
					if (node.IsSelected && Focused)
						context.DrawSelection = DrawSelectionMode.Active;
					else if (node.IsSelected && !Focused && !HideSelection)
						context.DrawSelection = DrawSelectionMode.Inactive;
				}
				context.DrawFocus = Focused && CurrentNode == node;

				if (FullRowSelect)
				{
					context.DrawFocus = false;
					if (context.DrawSelection == DrawSelectionMode.Active || context.DrawSelection == DrawSelectionMode.Inactive)
					{
						Rectangle focusRect = new Rectangle(OffsetX, row * RowHeight, ClientRectangle.Width, RowHeight);
						if (context.DrawSelection == DrawSelectionMode.Active)
						{
							e.Graphics.FillRectangle(customColors.HightLightBkgColor.TheBrush, focusRect);
							context.DrawSelection = DrawSelectionMode.FullRowSelect;
						}
						else
						{
							e.Graphics.FillRectangle(customColors.InactiveBorderBkgColor.TheBrush, focusRect);
							context.DrawSelection = DrawSelectionMode.None;
						}
					}
				}

				if (ShowLines)
					DrawLines(e.Graphics, node);

				DrawNode(node, context);
				row++;
			}

			if (_dropPosition.Node != null && _dragMode)
				DrawDropMark(e.Graphics);

			e.Graphics.ResetTransform();
			DrawScrollBarsBox(e.Graphics);
        }

		private void DrawColumnHeaders(Graphics gr)
		{
			int x = 0;
			VisualStyleRenderer renderer = null;
			if (Application.RenderWithVisualStyles)
				renderer = new VisualStyleRenderer(VisualStyleElement.Header.Item.Normal);

			DrawHeaderBackground(gr, renderer, new Rectangle(0, 0, ClientRectangle.Width + 10, ColumnHeaderHeight));
			gr.TranslateTransform(-OffsetX, 0);
			foreach (TreeColumn c in Columns)
			{
				if (c.IsVisible)
				{
					Rectangle rect = new Rectangle(x, 0, c.Width, ColumnHeaderHeight);
					x += c.Width;
					DrawHeaderBackground(gr, renderer, rect);
					c.Draw(gr, rect, Font);
				}
			}
		}

		private static void DrawHeaderBackground(Graphics gr, VisualStyleRenderer renderer, Rectangle rect)
		{
			if (renderer != null)
				renderer.DrawBackground(gr, rect);
			else
			{
				gr.FillRectangle(SystemBrushes.Control, rect);

				gr.DrawLine(SystemPens.ControlDark, rect.X, rect.Bottom - 2, rect.Right, rect.Bottom - 2);
				gr.DrawLine(SystemPens.ControlLightLight, rect.X, rect.Bottom - 1, rect.Right, rect.Bottom - 1);

				gr.DrawLine(SystemPens.ControlDark, rect.Right - 2, rect.Y, rect.Right - 2, rect.Bottom - 2);
				gr.DrawLine(SystemPens.ControlLightLight, rect.Right - 1, rect.Y, rect.Right - 1, rect.Bottom - 1);
			}
		}

		public void DrawNode(TreeNodeAdv node, DrawContext context)
		{
            foreach (NodeControlInfo item in GetNodeControls(node))
			{
				context.Bounds = item.Bounds;
				context.Graphics.SetClip(item.Bounds);
				item.Control.Draw(node, context);
				context.Graphics.ResetClip();
			}
        }

		private void DrawScrollBarsBox(Graphics gr)
		{
			Rectangle r1 = DisplayRectangle;
			Rectangle r2 = ClientRectangle;
			gr.FillRectangle(customColors.ScrollbarColor.TheBrush,
				new Rectangle(r1.Right, r1.Bottom, r2.Width - r1.Width, r2.Height - r1.Height));
		}

		private void DrawDropMark(Graphics gr)
		{
			if (_dropPosition.Position == NodePosition.Inside)
				return;

			Rectangle rect = GetNodeBounds(_dropPosition.Node);
			int right = DisplayRectangle.Right - LeftMargin + OffsetX;
			int y = rect.Y;
			if (_dropPosition.Position == NodePosition.After)
				y = rect.Bottom;
			gr.DrawLine(_markPen, rect.X, y, right, y);
		}

		private void DrawLines(Graphics gr, TreeNodeAdv node)
		{
			if (UseColumns && Columns.Count > 0)
				gr.SetClip(new Rectangle(0, 0, Columns[0].Width, RowCount * RowHeight + ColumnHeaderHeight));

			int row = node.Row;
			TreeNodeAdv curNode = node;
			while (curNode != _root)
			{
				int level = curNode.Level;
				int x = (level - 1) * _indent + NodePlusMinus.ImageSize / 2 + LeftMargin;
				int width = NodePlusMinus.Width - NodePlusMinus.ImageSize / 2;
				int y = row * RowHeight + TopMargin;
				int y2 = y + RowHeight;

				if (curNode == node)
				{
					int midy = y + RowHeight / 2;
					gr.DrawLine(_linePen, x, midy, x + width, midy);
					if (curNode.NextNode == null)
						y2 = y + RowHeight / 2;
				}

				if (node.Row == 0)
					y = RowHeight / 2;
				if (curNode.NextNode != null || curNode == node)
					gr.DrawLine(_linePen, x, y, x, y2);

				curNode = curNode.Parent;
			}

			gr.ResetClip();
		}

		#endregion

		#region Editor
		public void DisplayEditor(Control control, EditableControl owner)
		{
			if (control == null || owner == null)
				throw new ArgumentNullException();

			if (CurrentNode != null)
			{
				DisposeEditor();
				EditorContext context = new EditorContext();
				context.Owner = owner;
				context.CurrentNode = CurrentNode;
				context.Editor = control;

				SetEditorBounds(context);

				_currentEditor = control;
				_currentEditorOwner = owner;
				UpdateView();
				control.Parent = this;
				control.Focus();
				owner.UpdateEditor(control);
			}
		}

		private void SetEditorBounds(EditorContext context)
		{
			foreach (NodeControlInfo info in GetNodeControls(context.CurrentNode))
			{
				if (context.Owner == info.Control && info.Control is EditableControl)
				{
					Point p = ToViewLocation(info.Bounds.Location);
					int width = DisplayRectangle.Width - p.X;
					if (UseColumns && info.Control.Column < Columns.Count)
					{
						Rectangle rect = GetColumnBounds(info.Control.Column);
						width = rect.Right - OffsetX - p.X;
					}
					context.Bounds = new Rectangle(p.X, p.Y, width, info.Bounds.Height);
					((EditableControl)info.Control).SetEditorBounds(context);
					return;
				}
			}
		}

		private Rectangle GetColumnBounds(int column)
		{
			int x = 0;
			for (int i = 0; i < Columns.Count; i++)
			{
				if (Columns[i].IsVisible)
				{
					if (i < column)
						x += Columns[i].Width;
					else
						return new Rectangle(x, 0, Columns[i].Width, 0);
				}
			}
			return Rectangle.Empty;
		}

		public void HideEditor()
		{
			this.Focus();
			DisposeEditor();
		}

		public void UpdateEditorBounds()
		{
			if (_currentEditor != null)
			{
				EditorContext context = new EditorContext();
				context.Owner = _currentEditorOwner;
				context.CurrentNode = CurrentNode;
				context.Editor = _currentEditor;
				SetEditorBounds(context);
			}
		}

		private void DisposeEditor()
		{
			//Prj. 5071
			if (_currentEditor != null && !String.IsNullOrEmpty(_currentEditor.Text) && String.IsNullOrEmpty(_currentTextEdited))
				_currentTextEdited = _currentEditor.Text;

			if (_currentEditor != null)
				_currentEditor.Parent = null;
			if (_currentEditor != null)
				_currentEditor.Dispose();
			_currentEditor = null;
			_currentEditorOwner = null;
		}
		#endregion

		#region ModelEvents
		private void BindModelEvents()
		{
			_model.NodesChanged += new EventHandler<TreeModelEventArgs>(_model_NodesChanged);
			_model.NodesInserted += new EventHandler<TreeModelEventArgs>(_model_NodesInserted);
			_model.NodesRemoved += new EventHandler<TreeModelEventArgs>(_model_NodesRemoved);
			_model.StructureChanged += new EventHandler<TreePathEventArgs>(_model_StructureChanged);
		}

		private void UnbindModelEvents()
		{
			_model.NodesChanged -= new EventHandler<TreeModelEventArgs>(_model_NodesChanged);
			_model.NodesInserted -= new EventHandler<TreeModelEventArgs>(_model_NodesInserted);
			_model.NodesRemoved -= new EventHandler<TreeModelEventArgs>(_model_NodesRemoved);
			_model.StructureChanged -= new EventHandler<TreePathEventArgs>(_model_StructureChanged);
		}

		private void _model_StructureChanged(object sender, TreePathEventArgs e)
		{
			if (e.Path == null)
				throw new ArgumentNullException();

			TreeNodeAdv node = FindNode(e.Path);
			if (node != null)
			{
				Collection<ExpandedNode> expandedNodes = null;
				if (KeepNodesExpanded && node.IsExpanded)
				{
					expandedNodes = FindExpandedNodes(node);
				}
				_structureUpdating = true;
				try
				{
					ReadChilds(node, expandedNodes);
					UpdateSelection();
				}
				finally
				{
					_structureUpdating = false;
				}
				SmartFullUpdate();
			}
		}

		private Collection<ExpandedNode> FindExpandedNodes(TreeNodeAdv parent)
		{
			Collection<ExpandedNode> expandedNodes = null;
			expandedNodes = new Collection<ExpandedNode>();
			foreach (TreeNodeAdv child in parent.Nodes)
			{
				if (child.IsExpanded)
				{
					ExpandedNode str = new ExpandedNode();
					str.Tag = child.Tag;
					str.Children = FindExpandedNodes(child);
					expandedNodes.Add(str);
				}
			}
			return expandedNodes;
		}

		private void _model_NodesRemoved(object sender, TreeModelEventArgs e)
		{
			TreeNodeAdv parent = FindNode(e.Path);
			if (parent != null)
			{
				if (e.Indices != null)
				{
					List<int> list = new List<int>(e.Indices);
					list.Sort();
					for (int n = list.Count - 1; n >= 0; n--)
					{
						int index = list[n];
						if (index >= 0 && index <= parent.Nodes.Count)
						{
							parent.Nodes[index].Parent = null;
							parent.Nodes.RemoveAt(index);
						}
						else
							throw new ArgumentOutOfRangeException("Index out of range");
					}
				}
				else
				{
					for (int i = parent.Nodes.Count - 1; i >= 0; i--)
					{
						for (int n = 0; n < e.Children.Length; n++)
							if (parent.Nodes[i].Tag == e.Children[n])
							{
								parent.Nodes[i].Parent = null;
								parent.Nodes.RemoveAt(i);
								break;
							}
					}
				}
			}
			UpdateSelection();
			SmartFullUpdate();
		}

		private void _model_NodesInserted(object sender, TreeModelEventArgs e)
		{
			if (e.Indices == null)
				throw new ArgumentNullException("Indices");

			TreeNodeAdv parent = FindNode(e.Path);
			if (parent != null)
			{
				for (int i = 0; i < e.Children.Length; i++)
					AddNode(parent, e.Children[i], e.Indices[i]);
			}
			SmartFullUpdate();
		}

		private void _model_NodesChanged(object sender, TreeModelEventArgs e)
		{
			TreeNodeAdv parent = FindNode(e.Path);
            TreeNodeAdv senderNode = null;
			if (parent != null)
			{
				if (e.Indices != null)
				{
					foreach (int index in e.Indices)
					{
						if (index >= 0 && index < parent.Nodes.Count)
						{
                            senderNode = parent.Nodes[index];
                            Rectangle rect = GetNodeBounds(senderNode);
							_contentWidth = Math.Max(_contentWidth, rect.Right);
						}
						else
							throw new ArgumentOutOfRangeException("Index out of range");
					}
				}
				else
				{
					foreach (TreeNodeAdv node in parent.Nodes)
					{
						foreach (object obj in e.Children)
							if (node.Tag == obj)
							{
								Rectangle rect = GetNodeBounds(node);
								_contentWidth = Math.Max(_contentWidth, rect.Right);
							}
					}
				}
			}
			SafeUpdateScrollBars();
			UpdateView();
            OnNodeChanged(senderNode, e);
		}

		public TreeNodeAdv FindNode(TreePath path)
		{
			if (path.IsEmpty())
				return _root;
			else
				return FindNode(_root, path, 0);
		}

		private TreeNodeAdv FindNode(TreeNodeAdv root, TreePath path, int level)
		{
			foreach (TreeNodeAdv node in root.Nodes)
				if (node.Tag == path.FullPath[level])
				{
					if (level == path.FullPath.Length - 1)
						return node;
					else
						return FindNode(node, path, level + 1);
				}
			return null;
		}

        //4112
        public void CreateNewNode(string textNode, string keyNode)
        {
            _NewNode = new Node(textNode, keyNode, string.Empty);
        }

        public void CreateNewNode(string textNode, string keyNode, string keyImage)
        {
            _NewNode = new Node(textNode, keyNode, keyImage);
        }

        //4112
        public void CreateNewNode(string textNode, string keyNode, string textToolTip, string keyImage)
        {
            _NewNode = new Node(textNode, keyNode, keyImage, textToolTip);
        }

        public void AddToSelectedNode()
        {
            if (this.SelectedNode != null && _NewNode != null)
            {
                (this.SelectedNode.Tag as Node).Nodes.Add(_NewNode);
            }
        }

        public void AddToCurrentNode()
        {
            if (_CurrentNode != null && _NewNode != null)
                _CurrentNode.Nodes.Add(_NewNode);
        }

        public int OldNodesCount()
        {
            return _OldNodes.Count;
        }

        public void OldNodesPop()
        {
            _CurrentNode = _OldNodes.Pop();
        }

        public void OldNodesPeek()
        {
            _CurrentNode = _OldNodes.Peek();
        }

        public void OldNodesPush()
        {
            _OldNodes.Push(_NewNode);
        }

        public void OldNodesClear()
        {
            _OldNodes.Clear();
        }

        //4112
        public void SetColorNewNode(Color c)
        {
            if (_NewNode == null)
                return;

            FindNode(Model.GetPath(_NewNode)).ForeColor = c;
        }

        //4112
        public void SetStyleForSelectedNode(Boolean bold, Boolean italic, Boolean strikeout, Boolean underline)
        {
            BeginUpdate();
            if (SelectedNode == null)
                return;

            SelectedNode.Bold = bold;
            SelectedNode.Italic = italic;
            SelectedNode.Strikeout = strikeout;
            SelectedNode.UnderLine = underline;
            EndUpdate();
        }

        //4112
		public void SetStyleForNode(string nodeKey, Boolean bold, Boolean italic, Boolean strikeout, Boolean underline, Color foreColor)
        {
            BeginUpdate();
            TreeNodeAdv foundedNode = GetNode(nodeKey);
            if (foundedNode == null)
                return;

            foundedNode.Bold = bold;
            foundedNode.Italic = italic;
            foundedNode.Strikeout = strikeout;
            foundedNode.UnderLine = underline;
			foundedNode.ForeColor = foreColor;
			EndUpdate();
		}

        //4112
        public void SetForeColorForNode(string nodeKey, Color foreColor)
        {
            BeginUpdate();
            TreeNodeAdv foundedNode = GetNode(nodeKey);
            if (foundedNode == null)
                return;

            foundedNode.ForeColor = foreColor;
            EndUpdate();
        }
        //========================================================================================================================
		//Metodi utilizzati da MagoWeb
		//========================================================================================================================
		
		///<summary>
		///Metodo che ritorna il rettagolo occupato dal nodo con chiave nodeKey
		/// </summary>
		//========================================================================================================================
		public Rectangle GetNodeBounds(string nodeKey)
		{
			TreeNodeAdv foundNode = GetNode(nodeKey);
			if (foundNode == null)
				return Rectangle.Empty;

			return GetNodeBounds(foundNode);
		}

		///<summary>
		///Metodo che espande o collapsa il nodo con chiave nodeKey
		/// </summary>
		//========================================================================================================================
		public void ToggleNode(string nodeKey)
		{
			TreeNodeAdv foundNode = GetNode(nodeKey);
			if (foundNode == null)
				return;

			foundNode.IsExpanded = !foundNode.IsExpanded;
		}

		///<summary>
		///Metodo che seleziona il nodo con chiave nodeKey
		/// </summary>
		//========================================================================================================================
		public void SelectNode(string nodeKey)
		{
			TreeNodeAdv foundNode = GetNode(nodeKey);
			if (foundNode == null)
				return;

			foundNode.IsSelected = true;
			SelectedNode = foundNode;
		}

        //4112
        public void SetColorCurrentNode(Color c)
        {
            if (_CurrentNode == null)
                return;

            FindNode(Model.GetPath(_CurrentNode)).ForeColor = c;
        }
        //

        //4112
        public void AddToSelectedAndSetNodeAsCurrent(string textNode, string keyNode, string keyImage, string textToolTip)
        {
            Boolean found = false;

            Node selectedNode = SelectedNode.Tag as Node;

            if (selectedNode == null)
                return;

            foreach (Node n in selectedNode.Nodes)
            {
                if (n.Key.ToUpper() == keyNode.ToUpper())
                {
                    found = true;
                    //Set this node as _CurrentNode
                    _CurrentNode = n;
                    break;
                }
            }

            if (!found)
            {
                Node n = new Node(textNode, keyNode, keyImage, textToolTip);
                selectedNode.Nodes.Add(n);
                _CurrentNode = n;
            }

            //_CurrentNode.NodeType = eNodeType;
        }

        public void AddToSelectedAndSetNodeAsCurrent(string textNode, string keyNode)
        {
            AddToSelectedAndSetNodeAsCurrent(textNode, keyNode, string.Empty, string.Empty);
        }

        public void AddAndSetNewNodeFromCurrent(string textNode, string keyNode, string keyImage, string textToolTip)
        {
            Boolean found = false;

            if (_CurrentNode == null)
                return;

            foreach (Node n in _CurrentNode.Nodes)
            {
                if (n.Key.ToUpper() == keyNode.ToUpper())
                {
                    found = true;
                    //Set this node as _NewNode
                    _NewNode = n;
                    break;
                }
            }

            if (!found)
            {
                Node n = new Node(textNode, keyNode, keyImage, textToolTip);
                _CurrentNode.Nodes.Add(n);
                //Set this node as _NewNode
                _NewNode = n;
            }

            //_NewNode.NodeType = eNodeType;
        }

        public void AddAndSetNewNodeFromCurrent(string textNode, string keyNode)
        {
            AddAndSetNewNodeFromCurrent(textNode, keyNode, string.Empty, string.Empty);
        }

        public void AddAndSetNewNodeFromActual(string textNode, string keyNode, string keyImage, string textToolTip)
        {
            Boolean found = false;

            if (_NewNode == null)
                return;

            foreach (Node n in _NewNode.Nodes)
            {
                if (n.Key.ToUpper() == keyNode.ToUpper())
                {
                    found = true;
                    //Set this node as _NewNode
                    _NewNode = n;
                    break;
                }
            }

            if (!found)
            {
                Node n = new Node(textNode, keyNode, keyImage, textToolTip);
                _NewNode.Nodes.Add(n);
                //Set this node as _NewNode
                _NewNode = n;
            }

            //_NewNode.NodeType = eNodeType;
        }

        public void AddAndSetNewNodeFromActual(string textNode, string keyNode)
        {
            AddAndSetNewNodeFromActual(textNode, keyNode, string.Empty, string.Empty);
        }
        //

        //Aggiunta per il progetto 4112
        public Boolean AddTreeNode(string textNode, string keyNode)
        {
            return AddTreeNode(textNode, keyNode, string.Empty, Color.Black, string.Empty);
        }

        //4112
        public Boolean AddTreeNode(string textNode, string keyNode, string keyImage)
        {
            return AddTreeNode(textNode, keyNode, keyImage, Color.Black, string.Empty);
        }

        //4112
        public Boolean AddTreeNode(string textNode, string keyNode, Color c)
        {
            return AddTreeNode(textNode, keyNode, string.Empty, c, string.Empty);
        }

        //5831
        public Boolean AddTreeNode(string textNode, string keyNode, string textToolTip, bool checkBox)
        {
            return AddTreeNode(textNode, keyNode, string.Empty, Color.Black, string.Empty, checkBox);
        }

        //4112
        public Boolean AddTreeNode(string textNode, string keyNode, string keyImage, Color c, string textToolTip)
        {
            if (this.SelectedNode == null)
            {
                //add root node
                Node node = new Node(textNode, keyNode, keyImage, textToolTip);
                this.Model.Nodes.Add(node);
                this.SelectedNode = this.FindNode(this.Model.GetPath(node));
                this.SelectedNode.ForeColor = c;
                return true;
            }
            else
            {
                //add a child node
                Node parent = this.SelectedNode.Tag as Node;
                Node node = new Node(textNode, keyNode, keyImage, textToolTip);
                parent.Nodes.Add(node);
                this.SelectedNode = this.FindNode(this.Model.GetPath(node));
                this.SelectedNode.ForeColor = c;
                this.SelectedNode.IsExpanded = true;
                return true;
            }
         }

        //5831
        public Boolean AddTreeNode(string textNode, string keyNode, string keyImage, Color c, string textToolTip, bool checkBox)
        {
            if (this.SelectedNode == null)
            {
                //add root node
                Node node = new Node(textNode, keyNode, keyImage, textToolTip, checkBox);
                this.Model.Nodes.Add(node);
                this.SelectedNode = this.FindNode(this.Model.GetPath(node));
                this.SelectedNode.ForeColor = c;
                return true;
            }
            else
            {
                //add a child node
                Node parent = this.SelectedNode.Tag as Node;
                Node node = new Node(textNode, keyNode, keyImage, textToolTip, checkBox);
                parent.Nodes.Add(node);
                this.SelectedNode = this.FindNode(this.Model.GetPath(node));
                this.SelectedNode.ForeColor = c;
                this.SelectedNode.IsExpanded = true;
                return true;
            }
        }

        //4112
        public Boolean AddChild(string textNode, string keyNode)
        {
            return AddChild(textNode, keyNode, string.Empty, Color.Black, string.Empty);
        }

        //4112
        public Boolean AddChild(string textNode, string keyNode, string keyImage)
        {
            return AddChild(textNode, keyNode, keyImage, Color.Black, string.Empty);
        }

        //4112
        public Boolean AddChild(string textNode, string keyNode, Color c)
        {
            return AddChild(textNode, keyNode, string.Empty, c, string.Empty);
        }
        
        //4112
        public Boolean AddChild(string textNode, string keyNode, string keyImage, Color c, string textToolTip)
        {
            if (this.SelectedNode != null)
            {
                Node parent = this.SelectedNode.Tag as Node;
                Node node = new Node(textNode, keyNode, keyImage, textToolTip);
                parent.Nodes.Add(node);
                TreeNodeAdv n = this.FindNode(this.Model.GetPath(node));
                n.ForeColor = c;
                return true;
            }

            return false;
        }

        //4112
		private TreeNodeAdv FindNodeForKey(string keyToSearch, TreeNodeAdv nodeFrom)
        {
            if (nodeFrom == null)
                return null;

			if (String.Compare(((Node)nodeFrom.Tag).Key, keyToSearch, true) == 0)
                return nodeFrom;

			TreeNodeAdv foundNode = null;
            if (nodeFrom != null && nodeFrom.Nodes.Count > 0)
            {
					TreeNodeAdv currentNode = nodeFrom.Nodes[0];
				if (String.Compare(((Node)currentNode.Tag).Key, keyToSearch, true) == 0)
						return currentNode;

					foundNode = FindNodeForKey(keyToSearch, currentNode);
                    if (foundNode != null)
                          return foundNode;
            }
            else
            {
                if (nodeFrom != null && nodeFrom.NextNode != null)
                    return FindNodeForKey(keyToSearch, nodeFrom.NextNode);
                else
                {
                    while (nodeFrom != null && nodeFrom.Parent != null && nodeFrom.Parent != nodeFrom && nodeFrom.Parent.NextNode == null)
                        nodeFrom = nodeFrom.Parent;

                    if (nodeFrom != null && nodeFrom.Parent != null && nodeFrom.Parent.NextNode != null)
                        return FindNodeForKey(keyToSearch, nodeFrom.Parent.NextNode);
                    else
                        return null;
                }
            }

            return null;
        }

        //4112
        public Boolean SetNodeAsSelected(string keyToSearch)
        {
            if (Root == null || Root.Children.Count == 0)
                return false;

            for (int i = 0; i < Root.Children.Count; i++)
				if (String.Compare((Root.Children[i].Tag as Node).Key, keyToSearch, true) == 0)
                {
                    SelectedNode = Root.Children[i];
                    return true;
                }

            for (int i = 0; i < Root.Children.Count; i++)
            {
                    SelectedNode = FindNodeForKey(keyToSearch, Root.Children[i]);
                    if (SelectedNode != null)
                        return true;
            }
            
            return false;
        }

        //112
        public Boolean SetUpdateTextNode(string keyToSearch, string newText)
        {
            Node nodeFound;
            if (Root == null || Root.Children.Count == 0)
                return false;

            for (int i = 0; i < Root.Children.Count; i++)
				if (String.Compare((Root.Children[i].Tag as Node).Key, keyToSearch) == 0)
                {
                    nodeFound = (Root.Children[i].Tag as Node);
                    nodeFound.Text = newText;
                    return true;
                }

            for (int i = 0; i < Root.Children.Count; i++)
            {
                TreeNodeAdv n = FindNodeForKey(keyToSearch, Root.Children[i]);
                if (n != null)
                {
                    nodeFound = (n.Tag as Node);
                    nodeFound.Text = newText;
                    return true;
                }
            }

            return false;
        }

        //4112
        public Boolean ExistsNode(string keyToSearch)
        {
            if (Root == null || Root.Children.Count == 0)
                return false;

			return (GetNode(keyToSearch) != null);
        }

        public Boolean RemoveNode(string keyToSearch)
        {
            if (Root == null || Root.Children.Count == 0)
                return false;

            TreeNodeAdv foundNode = GetNode(keyToSearch);
            if (foundNode == null)
                return false;

            try
            {
                int n = foundNode.Parent.Children.IndexOf(foundNode);

                if (foundNode.Parent.Tag != null)
                {
                    if ((foundNode.Parent.Tag as Node).Nodes != null)
                        (foundNode.Parent.Tag as Node).Nodes.RemoveAt(n);
                }
            }
            finally
            {
            }

            return true;
        }

        //4112
        public TreeNodeAdv GetNode(string keyToSearch)
        {
            if (Root == null || Root.Children.Count == 0)
                return null;
	
			TreeNodeAdv n = null;
			for (int i = 0; i < Root.Children.Count; i++)
			{
				if (String.Compare((Root.Children[i].Tag as Node).Key, keyToSearch, true) == 0)
				  	n = Root.Children[i];
				else
					n = FindNodeForKey(keyToSearch, Root.Children[i]);
				
				if (n != null)
					return n;
			}

            return null;
        }

        //4112
        public Boolean InsertChild(string keyParent, string textNode, string keyNode, string keyImageNode, Color c, string textToolTip)
        {
            if (String.IsNullOrEmpty(keyParent))
            {
                SelectedNode = null;
                return AddTreeNode(textNode, keyNode, keyImageNode, c, textToolTip);
            }

            SetNodeAsSelected(keyParent);
            return AddChild(textNode, keyNode, keyImageNode, c, textToolTip);
        }

        //4112
        public String GetParentKey(string currentNodeKey)
        {
            if (Root == null || Root.Children.Count == 0)
                return String.Empty;

			TreeNodeAdv foundedNode = FindNodeForKey(currentNodeKey, Root.Children[0]);
            if (foundedNode != null)
            {
                return (foundedNode.Tag as Node).Parent.Key;
            }
            else
                return String.Empty;
        }
        
        //4112
        public void MoveChildrenOfSelectedNodeToChildrenOfRoot(Boolean bAlsoLeaves)
        {
            Node root = Root.Children[0].Tag as Node;

            if (SelectedNode == null)
                return;

            for (int i = 0; i < (SelectedNode.Tag as Node).Nodes.Count; i++)
            {
                if ((SelectedNode.Tag as Node).Nodes[i].Nodes.Count == 0 && !bAlsoLeaves)
                    (SelectedNode.Tag as Node).Nodes.RemoveAt(i);
                else
                    root.Nodes.Add((SelectedNode.Tag as Node).Nodes[i]);
                i = -1;
            }
            (SelectedNode.Tag as Node).Parent = null;
        }

        //4112
        public void MoveSelectedNodeToChildrenOfRoot()
        {
            Node root = Root.Children[0].Tag as Node;
            if (SelectedNode != null)
                (SelectedNode.Tag as Node).Parent = root;
        }

        //Prog. 5831
        public String LastChangedNodeKey 
        {
            get 
            {
                if(_lastChangedNode != null)
                    return _lastChangedNode.Key;

                return string.Empty;
            }
        }
		
        //Prog. 5831
        public Boolean IsEditable
        {
            get
            {
                return _isEditable;
            }
            set 
            {
                _isEditable = value;
            }
        }

        public CustomColors CustomColors
        {
            get
            {
                return customColors;
            }

            set
            {
                customColors = value;
            }
        }

        //prj. 5093
        //----------------------------------------------------------------------
        ThreadLocal<WndProcEventArgs> args = new ThreadLocal<WndProcEventArgs>(() => { return new WndProcEventArgs(); });
		
		protected override void WndProc(ref Message msg)
		{
			if (OnWndProc != null || OnAfterWndProc != null)
			{
				args.Value.Handled = false;
				args.Value.Msg = msg;
			}
			if (OnWndProc != null)
			{
				OnWndProc(this, args.Value);
				if (!args.Value.Handled)
					base.WndProc(ref msg);
			}
			else
			{
				base.WndProc(ref msg);
			}

			if (OnAfterWndProc != null)
			{
				OnWndProc(this, args.Value);
			}
		}
        #endregion
	}

	//prj. 5093
	//=========================================================================
	public class WndProcEventArgs : EventArgs
	{
		public Message Msg { get; set; }
		public bool Handled { get; set; }
	}
}
