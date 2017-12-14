using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Microarea.EasyAttachment.Components;

namespace Microarea.EasyAttachment.UI.Controls
{
    /// <summary>
    /// Delegate used to handle clicking of list items.
    /// </summary>
    public delegate void ListItemClickedHandler(ExtendedListView sender, Control listItem, bool isSelected);

    /// <summary>
    /// This class is the actual list view, it handles addition and removal of list items, 
    /// fires events when items are clicked and animates the list based on mouse dragging.
    /// The list accepts any <c>System.Windows.Forms.Control</c> as list item but if the 
    /// list item extends <c>IExtendedListItem</c> additional functionality is enabled.
    /// </summary>
    //---------------------------------------------------------------------
    public partial class ExtendedListView : UserControl
    {
          /// <summary>
        /// Event that clients hooks into to get item clicked events.
        /// </summary>
        public event ListItemClickedHandler ListItemClicked;

        private MouseEventHandler mouseDownEventHandler;
        private MouseEventHandler mouseUpEventHandler;
        
        private Point mouseDownPoint = Point.Empty;
        private Point previousPoint = Point.Empty;
        //private bool mouseIsDown = false;
        
        /// <summary>
        /// Contains the selected state for all list items.
        /// </summary>
        private Dictionary<Control, bool> selectedItemsMap = new Dictionary<Control, bool>();

        private bool multiSelectEnabled = false;
        private bool unselectEnabled = false;


        //---------------------------------------------------------------------
        public ExtendedListView()
        {
            InitializeComponent();

			mouseDownEventHandler = new MouseEventHandler(MouseDownHandler);
			mouseUpEventHandler = new MouseEventHandler(MouseUpHandler);
        }

        //---------------------------------------------------------------------
        private void FireListItemClicked(Control listItem)
        {
            if (ListItemClicked != null)
                ListItemClicked(this, listItem, selectedItemsMap[listItem]);
        }

        /// <summary>
        /// When resizing the list box the iternal items has to 
        /// be resized as well.
        /// </summary>
        //---------------------------------------------------------------------
        protected override void OnResize(EventArgs e)
        {
			base.OnResize(e);
			itemsPanel.Location = ClientRectangle.Location;
			itemsPanel.Width = ClientSize.Width;

			foreach (Control itemControl in itemsPanel.Controls)
				itemControl.Width = itemsPanel.ClientSize.Width;
        }

        /// <summary>
        /// Since the list items can be made up of any number of
        /// <c>Control</c>s this helper method determines the
        /// "root" <c>Control</c> of the list item when a mouse
        /// event has been fired.
        /// </summary>
        //---------------------------------------------------------------------
        private Control GetListItemFromEvent(Control sender)
        {
            if (sender == this || sender == itemsPanel)
                return null;
            else
            {
                while (sender.Parent != itemsPanel)
                    sender = sender.Parent;

                return sender;
            }
        }

        /// <summary>
        /// Handles mouse down events by storing a set of <c>Point</c>s that
        /// will be used to determine the selected item.
        /// </summary>
        //---------------------------------------------------------------------
        private void MouseDownHandler(object sender, MouseEventArgs e)
        {
			if (e.Button == MouseButtons.Left)
			{
				//mouseIsDown = true;
				// Since list items move when scrolled all locations are 
				// in absolute values (meaning local to "this" rather than to "sender".
				mouseDownPoint = Utils.GetAbsolute(new Point(e.X, e.Y), sender as Control, this);
				previousPoint = mouseDownPoint;
			}
        }

        /// <summary>
        /// Handles the mouse up event and determines if the list needs to animate 
        /// after it has been "released".
        /// </summary>
        //---------------------------------------------------------------------
        private void MouseUpHandler(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // If the mouse was lifted from the same location it was pressed down on 
                // then this is not a drag but a click, do item selection logic instead
                // of dragging logic.
                if (Utils.GetAbsolute(new Point(e.X, e.Y), sender as Control, this).Equals(mouseDownPoint))
                {
                    // Get the list item (regardless if it was a child Control that was clicked). 
                    Control item = GetListItemFromEvent(sender as Control);
                    if (item != null)
                    {
                        bool newState = UnselectEnabled ? !selectedItemsMap[item] : true;
                        if (newState != selectedItemsMap[item])
                        {
                            selectedItemsMap[item] = newState;
                            FireListItemClicked(item);

                            if (!MultiSelectEnabled && selectedItemsMap[item])
                            {
                                foreach (Control listItem in itemsPanel.Controls)
                                {
                                    if (listItem != item)
                                        selectedItemsMap[listItem] = false;
                                }
                            }

                            // After "normal" selection rules have been applied,
                            // check if the list items affected are IExtendedListItems
                            // and call the appropriate methods if it is so.
                            foreach (Control listItem in itemsPanel.Controls)
                            {
                                if (listItem is IExtendedListItem)
                                    (listItem as IExtendedListItem).SelectedChanged(selectedItemsMap[listItem]);
                            }

                            // Force a re-layout of all items
                            LayoutItems();
                        }
                    }
                }
            }
            //mouseIsDown = false;
        }

        /// <summary>
        /// Layout the items and make sure they line up properly as they can change size during runtime.
        /// </summary>
        //---------------------------------------------------------------------
        public void LayoutItems()
        {
			int top = 0;
            foreach (Control itemControl in itemsPanel.Controls)
            {
                itemControl.Location = new Point(0, top);
                itemControl.Width = itemsPanel.ClientSize.Width;
                top += itemControl.Height;
	        }
			
			itemsPanel.Height = top;
        }

        /// <summary>
        /// Adds a new item to the list box.
        /// </summary>
        //---------------------------------------------------------------------
        public void AddItem(Control control)
		{
			if (!itemsPanel.Controls.Contains(control))
			{
				itemsPanel.Controls.Add(control);
				selectedItemsMap.Add(control, false);

				LayoutItems();

				if (control is IExtendedListItem)
					((IExtendedListItem)control).PositionChanged(itemsPanel.Controls.Count);

				((IExtendedListItem)control).ListItemResizing += new EventHandler(ExtendedListView_ListItemResizing);
				Utils.SetHandlers(this, mouseDownEventHandler, mouseUpEventHandler);
			}
			else
			{
				throw new ArgumentException("Each item in ExtendedListView must be a unique Control", "control");
			}
        }

        //---------------------------------------------------------------------
        void ExtendedListView_ListItemResizing(object sender, EventArgs e)
		{
			LayoutItems();
		}
	
        /// <summary>
        /// Removes an item from the list box.
        /// </summary>
        /// <param name="control"></param>
        //---------------------------------------------------------------------
        public void RemoveItem(Control control)
        {
            itemsPanel.Controls.Remove(control);
            selectedItemsMap.Remove(control);

            Utils.RemoveHandlers(control, mouseDownEventHandler, mouseUpEventHandler);

            for (int i = 0; i < itemsPanel.Controls.Count; ++i)
            {
                Control itemControl = itemsPanel.Controls[i];
                if (itemControl is IExtendedListItem)
                    (itemControl as IExtendedListItem).PositionChanged(i);
            }

            LayoutItems();
        }

        /// <summary>
        /// This method resets all items to unselected and 
        /// resets scrolling to the top of the list.
        /// </summary>
        //---------------------------------------------------------------------
        public void Reset()
        {
            itemsPanel.Top = 0;
            foreach (Control control in itemsPanel.Controls)
            {
                if (control is IExtendedListItem)
                    (control as IExtendedListItem).SelectedChanged(false);
                selectedItemsMap[control] = false;
            }

            LayoutItems();
        }

		/// <summary>
		/// Use to remove all items from the list
		/// </summary>
        //---------------------------------------------------------------------
        public void RemoveAllItems()
		{
			Control control = null;
			for (int i = itemsPanel.Controls.Count - 1; i >= 0; i--)
			{
				control = itemsPanel.Controls[i];
				itemsPanel.Controls.RemoveAt(i);
				selectedItemsMap.Remove(control);
				Utils.RemoveHandlers(control, mouseDownEventHandler, mouseUpEventHandler);
			}
			LayoutItems();
		}

		/// <summary>
		/// If set to <c>True</c> multiple items can be selected at the same
		/// time, otherwise a selected item is automatically de-selected when
		/// a new item is selected.
		/// </summary>
        //---------------------------------------------------------------------
        public bool MultiSelectEnabled
        {
            get { return multiSelectEnabled; }
            set { multiSelectEnabled = value; }
        }

        /// <summary>
        /// If set to <c>True</c> then the user can explicitly unselect a
        /// selected item.
        /// </summary>
        //---------------------------------------------------------------------
        public bool UnselectEnabled
        {
            get { return unselectEnabled; }
            set { unselectEnabled = value; }
        }

        //---------------------------------------------------------------------
        public ControlCollection Items
        {
            get { return itemsPanel.Controls; }
        }

        //---------------------------------------------------------------------
        public List<Control> SelectedItems
        {
            get
            {
                List<Control> selectedItems = new List<Control>();
                foreach (Control key in selectedItemsMap.Keys)
                {
                    if (selectedItemsMap[key])
                        selectedItems.Add(key);
                }
                return selectedItems;
            }
		}
    }
}
