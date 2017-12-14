using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Microarea.Console.Plugin.SysAdmin.UserControls
{
	///	<summary>
	///	Inherited ListView to allow in-place editing of subitems
	///	</summary>
	//=========================================================================
	public partial class ExtendedListView : System.Windows.Forms.ListView
	{
		#region Interop structs, imports and constants
		/// <summary>
		/// MessageHeader for WM_NOTIFY
		/// </summary>
		private struct NMHDR 
		{ 
			public IntPtr hwndFrom; 
			public Int32  idFrom; 
			public Int32  code; 

			public NMHDR(int o)
			{
				hwndFrom = IntPtr.Zero;
				idFrom = 0;
				code = 0;
			}
		}

		[DllImport("user32.dll")]
		private	static extern IntPtr SendMessage(IntPtr hWnd, int msg,	IntPtr wPar, IntPtr	lPar);
		[DllImport("user32.dll", CharSet=CharSet.Ansi)]
		private	static extern IntPtr SendMessage(IntPtr	hWnd, int msg, int len,	ref	int	[] order);

		// ListView messages
		private const int LVM_FIRST					= 0x1000;
		private const int LVM_GETCOLUMNORDERARRAY	= (LVM_FIRST + 59);

		// Windows Messages that will abort editing
		private	const int WM_HSCROLL = 0x114;
		private	const int WM_VSCROLL = 0x115;
		private const int WM_SIZE	 = 0x05;
		private const int WM_NOTIFY	 = 0x4E;

		private const int HDN_FIRST = -300;
		private const int HDN_BEGINDRAG = (HDN_FIRST-10);
		private const int HDN_ITEMCHANGINGA = (HDN_FIRST-0);
		private const int HDN_ITEMCHANGINGW = (HDN_FIRST-20);
		#endregion

		public event SubItemEventHandler SubItemClicked;
		public event SubItemEventHandler SubItemBeginEditing;
		public event SubItemEndEditingEventHandler SubItemEndEditing;

		# region Constructors
		//-------------------------------------------------------------------------
		public ExtendedListView()
		{
			InitializeComponent();

			base.FullRowSelect = true;
			base.View = View.Details;
			base.AllowColumnReorder = true;
		}

		//-------------------------------------------------------------------------
		public ExtendedListView(System.ComponentModel.IContainer container)
		{
			container.Add(this);
			InitializeComponent();
		}
		# endregion

/*			private bool _doubleClickActivation = false;
		/// <summary>
		/// Is a double click required to start editing a cell?
		/// </summary>
		public bool DoubleClickActivation
		{
			get {  return _doubleClickActivation; }
			set { _doubleClickActivation = value; }    
		}
*/
		/// <summary>
		/// Retrieve the order in which columns appear
		/// </summary>
		/// <returns>Current display order of column indices</returns>
		//-------------------------------------------------------------------------
		public int[] GetColumnOrder()
		{
			IntPtr lPar	= Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)) * Columns.Count);

				IntPtr res = SendMessage(Handle, LVM_GETCOLUMNORDERARRAY, new IntPtr(Columns.Count), lPar);
			if (res.ToInt32() == 0)	// Something went wrong
			{
				Marshal.FreeHGlobal(lPar);
				return null;
			}

			int	[] order = new int[Columns.Count];
			Marshal.Copy(lPar, order, 0, Columns.Count);

			Marshal.FreeHGlobal(lPar);

			return order;
		}

		/// <summary>
		/// Find ListViewItem and SubItem Index at position (x,y)
		/// </summary>
		/// <param name="x">relative to ListView</param>
		/// <param name="y">relative to ListView</param>
		/// <param name="item">Item at position (x,y)</param>
		/// <returns>SubItem index</returns>
		//-------------------------------------------------------------------------
		public int GetSubItemAt(int x, int y, out ListViewItem item)
		{
			item = this.GetItemAt(x, y);
	
			if (item !=	null)
			{
				int[] order = GetColumnOrder();
				Rectangle lviBounds;
				int	subItemX;

				lviBounds =	item.GetBounds(ItemBoundsPortion.Entire);
				subItemX = lviBounds.Left;
				for (int i=0; i<order.Length; i++)
				{
					ColumnHeader h = this.Columns[order[i]];
					if (x <	subItemX+h.Width)
					{
						return h.Index;
					}
					subItemX += h.Width;
				}
			}
		
			return -1;
		}

		/// <summary>
		/// Get bounds for a SubItem
		/// </summary>
		/// <param name="Item">Target ListViewItem</param>
		/// <param name="SubItem">Target SubItem index</param>
		/// <returns>Bounds of SubItem (relative to ListView)</returns>
		//-------------------------------------------------------------------------
		public Rectangle GetSubItemBounds(ListViewItem Item, int SubItem)
		{
			int[] order = GetColumnOrder();

			Rectangle subItemRect = Rectangle.Empty;
			if (SubItem >= order.Length)
				throw new IndexOutOfRangeException("SubItem "+SubItem+" out of range");

			if (Item == null)
				throw new ArgumentNullException("Item");
		
			Rectangle lviBounds = Item.GetBounds(ItemBoundsPortion.Entire);
			int	subItemX = lviBounds.Left;

			ColumnHeader col;
			int i;
			for (i=0; i<order.Length; i++)
			{
				col = this.Columns[order[i]];
				if (col.Index == SubItem)
					break;
				subItemX += col.Width;
			} 
			subItemRect	= new Rectangle(subItemX, lviBounds.Top, this.Columns[order[i]].Width, lviBounds.Height);
			return subItemRect;
		}

		//-------------------------------------------------------------------------
		protected override void	WndProc(ref	Message	msg)
		{
			switch (msg.Msg)
			{
				// Look	for	WM_VSCROLL,WM_HSCROLL or WM_SIZE messages.
				case WM_VSCROLL:
				case WM_HSCROLL:
				case WM_SIZE:
					EndEditing(false);
					break;
				case WM_NOTIFY:
					// Look for WM_NOTIFY of events that might also change the
					// editor's position/size: Column reordering or resizing
					NMHDR h = (NMHDR)Marshal.PtrToStructure(msg.LParam, typeof(NMHDR));
					if (h.code == HDN_BEGINDRAG ||
						h.code == HDN_ITEMCHANGINGA ||
						h.code == HDN_ITEMCHANGINGW)
						EndEditing(false);
					break;
			}

			base.WndProc(ref msg);
		}

		#region Initialize editing depending of DoubleClickActivation property
		//-------------------------------------------------------------------------
		protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e)
		{
			base.OnMouseUp(e);

/*				if (DoubleClickActivation)
			{
				return;
			} 
*/
			EditSubitemAt(new Point(e.X, e.Y));
		}
	
		//-------------------------------------------------------------------------
		protected override void OnDoubleClick(EventArgs e)
		{
			base.OnDoubleClick (e);

/*				if (!DoubleClickActivation)
			{
				return;
			} 
*/
			Point pt = this.PointToClient(Cursor.Position);
	
			EditSubitemAt(pt);
		}

		///<summary>
		/// Fire SubItemClicked
		///</summary>
		///<param name="p">Point of click/doubleclick</param>
		//-------------------------------------------------------------------------
		private void EditSubitemAt(Point p)
		{
			ListViewItem item;
			int idx = GetSubItemAt(p.X, p.Y, out item);
			if (idx >= 0)
				OnSubItemClicked(new SubItemEventArgs(item, idx));
		}
		#endregion

		#region In-place editing functions
		// The control performing the actual editing
		private Control editingControl;
		// The LVI being edited
		private ListViewItem editItem;
		// The SubItem being edited
		private int editSubItem;

		//-------------------------------------------------------------------------
		protected void OnSubItemBeginEditing(SubItemEventArgs e)
		{
			if (SubItemBeginEditing != null)
				SubItemBeginEditing(this, e);
		}
		//-------------------------------------------------------------------------
		protected void OnSubItemEndEditing(SubItemEndEditingEventArgs e)
		{
			if (SubItemEndEditing != null)
				SubItemEndEditing(this, e);
		}
		//-------------------------------------------------------------------------
		protected void OnSubItemClicked(SubItemEventArgs e)
		{
			if (SubItemClicked != null)
				SubItemClicked(this, e);
		}

		/// <summary>
		/// Begin in-place editing of given cell
		/// </summary>
		/// <param name="c">Control used as cell editor</param>
		/// <param name="Item">ListViewItem to edit</param>
		/// <param name="SubItem">SubItem index to edit</param>
		//-------------------------------------------------------------------------
		public void StartEditing(Control c, ListViewItem Item, int SubItem)
		{
			OnSubItemBeginEditing(new SubItemEventArgs(Item, SubItem));

			Rectangle rcSubItem = GetSubItemBounds(Item, SubItem);

			if (rcSubItem.X < 0)
			{
				// Left edge of SubItem not visible - adjust rectangle position and width
				rcSubItem.Width += rcSubItem.X;
				rcSubItem.X=0;
			}
			if (rcSubItem.X+rcSubItem.Width > this.Width)
			{
				// Right edge of SubItem not visible - adjust rectangle width
				rcSubItem.Width = this.Width-rcSubItem.Left;
			}

			// Subitem bounds are relative to the location of the ListView!
			rcSubItem.Offset(Left, Top);

			// In case the editing control and the listview are on different parents,
			// account for different origins
			Point origin = new Point(0,0);
			Point lvOrigin  = this.Parent.PointToScreen(origin);
			Point ctlOrigin  = c.Parent.PointToScreen(origin);

			rcSubItem.Offset(lvOrigin.X-ctlOrigin.X, lvOrigin.Y-ctlOrigin.Y);

			// Position and show editor
			c.Bounds = rcSubItem;
			c.Text = Item.SubItems[SubItem].Text;
			c.Visible = true;
			c.BringToFront();
			c.Focus();

			editingControl = c;
			editingControl.Leave += new EventHandler(_editControl_Leave);
			editingControl.KeyPress += new KeyPressEventHandler(_editControl_KeyPress);

			editItem = Item;
			editSubItem = SubItem;
		}

		//-------------------------------------------------------------------------
		private void _editControl_Leave(object sender, EventArgs e)
		{
			// cell editor losing focus
			EndEditing(true);
		}

		//-------------------------------------------------------------------------
		private void _editControl_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
		{
			switch (e.KeyChar)
			{
				case (char)(int)Keys.Escape:
				{
					EndEditing(false);
					break;
				}

				case (char)(int)Keys.Enter:
				{  
					EndEditing(true);
					break;
				}
			}
		}

		/// <summary>
		/// Accept or discard current value of cell editor control
		/// </summary>
		/// <param name="AcceptChanges">Use the editingControl's Text as new SubItem text or discard changes?</param>
		//-------------------------------------------------------------------------
		public void EndEditing(bool AcceptChanges)
		{
			if (editingControl == null)
				return;

			SubItemEndEditingEventArgs e = new SubItemEndEditingEventArgs(
				editItem,		// The item being edited
				editSubItem,	// The subitem index being edited
				AcceptChanges ?
				editingControl.Text :	// Use editControl text if changes are accepted
				editItem.SubItems[editSubItem].Text,	// or the original subitem's text, if changes are discarded
				!AcceptChanges	// Cancel?
				);

			OnSubItemEndEditing(e);

			editItem.SubItems[editSubItem].Text = e.DisplayText;

			editingControl.Leave -= new EventHandler(_editControl_Leave);
			editingControl.KeyPress -= new KeyPressEventHandler(_editControl_KeyPress);
			editingControl.Visible = false;
			editingControl = null;

			editItem = null;
			editSubItem = -1;
		}
		#endregion
	}

    # region Event handlers
    /// <summary>
    /// Event Handler for SubItem events
    /// </summary>
    public delegate void SubItemEventHandler(object sender, SubItemEventArgs e);
    /// <summary>
    /// Event Handler for SubItemEndEditing events
    /// </summary>
    public delegate void SubItemEndEditingEventHandler(object sender, SubItemEndEditingEventArgs e);
    # endregion

    # region SubItemEventArgs class
    /// <summary>
    /// Event Args for SubItemClicked event
    /// </summary>
    //=========================================================================
    public class SubItemEventArgs : EventArgs
    {
        private int subItemIndex = -1;
        private ListViewItem item = null;

        //-------------------------------------------------------------------------
        public SubItemEventArgs(ListViewItem item, int subItem)
        {
            subItemIndex = subItem;
            this.item = item;
        }

        public int SubItem { get { return subItemIndex; } }
        public ListViewItem Item { get { return this.item; } }
    }
    # endregion

    # region SubItemEndEditingEventArgs class
    /// <summary>
    /// Event Args for SubItemEndEditingClicked event
    /// </summary>
    //=========================================================================
    public class SubItemEndEditingEventArgs : SubItemEventArgs
    {
        private string text = string.Empty;
        private bool cancel = true;

        //-------------------------------------------------------------------------
        public SubItemEndEditingEventArgs(ListViewItem item, int subItem, string display, bool cancel)
            : base(item, subItem)
        {
            text = display;
            this.cancel = cancel;
        }

        public string DisplayText { get { return text; } set { text = value; } }
        public bool Cancel { get { return this.cancel; } set { this.cancel = value; } }
    }
    # endregion
}
